using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

using Serialization;

namespace EVTUI;

public class Afs2 : ISerializable
{
    public const string MAGIC = "AFS2";
    public MagicString Magic = new MagicString(Afs2.MAGIC);

    public byte       Type;
    public byte       PositionFieldLength;
    public byte       IdFieldLength;
    public byte       Padding;
    public Int32      EntryCount;
    public UInt32     Align;
    public AfsValue[] EntryIds;
    public AfsValue[] EntryPositions;
    public AfsValue   EndPosition;
    public byte[]     HeaderPadding;
    public byte[][]   EntryPads;
    public byte[][]   EntryData;

    public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
    {
        if (rw.IsConstructlike())
            Trace.TraceInformation("Reading AFS2 object");
        else if (rw.IsParselike())
            Trace.TraceInformation("Writing AFS2 object");

        rw.SetLittleEndian(true);

        rw.RwObj(ref this.Magic);

        rw.RwUInt8(ref this.Type);
        rw.RwUInt8(ref this.PositionFieldLength);
        rw.RwUInt8(ref this.IdFieldLength);
        rw.RwUInt8(ref this.Padding);

        rw.RwInt32(ref this.EntryCount);
        rw.RwUInt32(ref this.Align);

        if (rw.IsConstructlike())
        {
            this.EntryIds = Enumerable.Range(0, this.EntryCount).Select(i => new AfsValue()).ToArray();
            this.EntryPositions = Enumerable.Range(0, this.EntryCount).Select(i => new AfsValue()).ToArray();
        }
        rw.RwObjs(ref this.EntryIds, this.EntryCount, new Dictionary<string, object>()
            { ["fieldLength"] = this.IdFieldLength });
        rw.RwObjs(ref this.EntryPositions, this.EntryCount, new Dictionary<string, object>()
            { ["fieldLength"] = this.PositionFieldLength });
        rw.RwObj(ref this.EndPosition, new Dictionary<string, object>()
            { ["fieldLength"] = this.PositionFieldLength });

        if (this.EntryCount > 1)
            rw.RwBytestring(ref this.HeaderPadding, (int)(this.Align - (rw.RelativeTell() % this.Align)));

        rw.RelativeSeek(this.EndPosition.GetValue()-1, 0);
        if ((rw.IsConstructlike() && !rw.IsEOF()) || (rw.IsParselike() && !(this.EntryData is null)))
            this.GetEntries(rw);

        rw.ResetEndianness();
    }

    public void GetEntries<T>(T rw) where T : struct, IBaseBinaryTarget
    {
        if (rw.IsConstructlike())
        {
            this.EntryPads = new byte[this.EntryCount][];
            this.EntryData = new byte[this.EntryCount][];
        }

        for (int i=0; i<this.EntryCount; i++)
        {
            int entryPosition = (int)this.EntryPositions[i].GetValue();
            int nextEntryPosition = (int)this.EndPosition.GetValue();
            if (i < this.EntryCount-1)
                nextEntryPosition = (int)this.EntryPositions[i+1].GetValue();

            int padSize = (int)(this.Align - (entryPosition % this.Align));
            long checkpoint = rw.RelativeTell();
            rw.RelativeSeek(entryPosition, 0);

            if (entryPosition % this.Align != 0)
            {
                rw.RwBytestring(ref this.EntryPads[i], padSize);
                entryPosition += padSize;
            }

            int dataSize = (int)(nextEntryPosition-entryPosition);
            rw.RwBytestring(ref this.EntryData[i], dataSize);
            rw.RelativeSeek(checkpoint, 0);
        }
    }

    public byte[] GetStreamEntry(string filepath, int awbId)
    {
        using (var stream = File.Open(filepath, FileMode.Open))
        {
            using (var r = new BinaryReader(stream))
            {
                for (int i=0; i<this.EntryCount; i++)
                {
                    if (this.EntryIds[i].GetValue() == awbId)
                    {
                        Reader reader = new Reader(r);

                        int entryPosition = (int)this.EntryPositions[i].GetValue();
                        int nextEntryPosition = (int)this.EndPosition.GetValue();
                        if (i < this.EntryCount-1)
                            nextEntryPosition = (int)this.EntryPositions[i+1].GetValue();

                        int padSize = (int)(this.Align - (entryPosition % this.Align));
                        if (entryPosition % this.Align != 0)
                            entryPosition += padSize;
                        reader.RelativeSeek(entryPosition, 0);

                        int dataSize = (int)(nextEntryPosition-entryPosition);
                        byte[] entryData = new byte[dataSize];
                        reader.RwBytestring(ref entryData, dataSize);
                        return entryData;
                    }
                }
            }
        }
        return null;
    }

    public byte[] GetMemoryEntry(int awbId)
    {
        for (int i=0; i<this.EntryCount; i++)
            if (this.EntryIds[i].GetValue() == awbId)
                return this.EntryData[i];
        return null;
    }

    public void Write(string filepath) { TraitMethods.Write(this, filepath); }
    public void Read (string filepath) { TraitMethods.Read (this, filepath); }
}

public class AfsValue : ISerializable
{
    private byte   FieldLength;

    private byte   ByteValue;
    private UInt16 UInt16Value;
    private UInt32 UInt32Value;
    private UInt64 UInt64Value;

    public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
    {
        this.FieldLength = (byte)args["fieldLength"];
        switch (this.FieldLength)
        {
            case 1:
                rw.RwUInt8(ref this.ByteValue);
                break;
            case 2:
                rw.RwUInt16(ref this.UInt16Value);
                break;
            case 4:
                rw.RwUInt32(ref this.UInt32Value);
                break;
            case 8:
                rw.RwUInt64(ref this.UInt64Value);
                break;
            default:
                break;
        }
    }

    public dynamic GetValue()
    {
        switch (this.FieldLength)
        {
            case 1:
                return this.ByteValue;
            case 2:
                return this.UInt16Value;
            case 4:
                return this.UInt32Value;
            case 8:
                return this.UInt64Value;
            default:
                return null;
        }
    }
}
