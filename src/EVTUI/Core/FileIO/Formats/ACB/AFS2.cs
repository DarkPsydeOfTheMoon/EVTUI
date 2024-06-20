using System;
using System.Collections.Generic;
using System.Text;

using Serialization;

namespace EVTUI;

public class Afs2 : ISerializable
{
    public const string MAGIC = "AFS2";

    public string       Magic;
    public byte         Type;
    public byte         PositionFieldLength;
    public byte         IdFieldLength;
    public byte         Padding;
    public Int32        EntryCount;
    public UInt32       Align;
    public AfsValue[]   EntryIds;
    public AfsValue[]   EntryPositions;
    public AfsValue     EndPosition;
    public byte[]       HeaderPadding;
    public List<byte[]> EntryPads;
    public List<byte[]> EntryData;

    public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
    {
        rw.SetEndianness("little");

        rw.RwString(ref this.Magic, 4, Encoding.ASCII);
        if (this.Magic != Afs2.MAGIC)
            throw new Exception($"Magic string ({this.Magic}) doesn't match expected string ({Afs2.MAGIC})");

        rw.RwUInt8(ref this.Type);
        rw.RwUInt8(ref this.PositionFieldLength);
        rw.RwUInt8(ref this.IdFieldLength);
        rw.RwUInt8(ref this.Padding);

        rw.RwInt32(ref this.EntryCount);
        rw.RwUInt32(ref this.Align);

        rw.RwObjs(ref EntryIds, this.EntryCount, new Dictionary<string, object>()
            { ["fieldLength"] = this.IdFieldLength });
        rw.RwObjs(ref EntryPositions, this.EntryCount, new Dictionary<string, object>()
            { ["fieldLength"] = this.PositionFieldLength });
        rw.RwObj(ref EndPosition, new Dictionary<string, object>()
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
            this.EntryPads = new List<byte[]>();
            this.EntryData = new List<byte[]>();
        }

        for (int i=0; i<this.EntryCount; i++)
        {
			int entryPosition = (int)this.EntryPositions[i].GetValue();
			int nextEntryPosition = (int)this.EndPosition.GetValue();
			if (i < this.EntryCount-1)
				nextEntryPosition = (int)this.EntryPositions[i+1].GetValue();

            int padSize = (int)(this.Align - (entryPosition % this.Align));
            int dataSize = (int)(nextEntryPosition-entryPosition);

            byte[] tmpPad = new byte[padSize];
            byte[] tmpData = new byte[dataSize];

            if (rw.IsParselike())
            {
                tmpPad = this.EntryPads[i];
                tmpData = this.EntryData[i];
            }

            long checkpoint = rw.RelativeTell();
            rw.RelativeSeek(entryPosition, 0);

            if (entryPosition % this.Align != 0)
            {
                rw.RwBytestring(ref tmpPad, padSize);
                entryPosition += padSize;
            }

            rw.RwBytestring(ref tmpData, dataSize);

            if (rw.IsConstructlike())
            {
                this.EntryPads.Add(tmpPad);
                this.EntryData.Add(tmpData);
            }

            rw.RelativeSeek(checkpoint, 0);
        }
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
