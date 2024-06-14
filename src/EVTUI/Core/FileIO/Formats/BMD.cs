using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Serialization;

namespace EVTUI;

public static partial class Globals
{
    public static UInt32 LAST_TURN_KIND {get;set;}
}

public class BMD : ISerializable
{
    private string MAGIC = "GSM";

    public byte FileType;
    public byte Format;
    public UInt16 UserId;
    public UInt32 FileSize;

    public string Endianness;
    public string Magic;

    public UInt32 ExtSize;
    public UInt32 RelocationTableOffset;
    public UInt32 RelocationTableSize;
    public UInt32 TurnCount;
    public byte IsRelocated;
    public byte ReservedByte;
    public UInt16 Version;

    public UInt32[] TurnKinds;
    public UInt32[] TurnOffsets;
    public Turn[] Turns;

    public UInt32 SpeakerNameArrayOffset;
    public UInt32 SpeakerCount;
    public UInt32 ExtDataOffset;
    public UInt32 ReservedUInt32;

    public UInt32[] SpeakerNameAddresses;
    public List<byte[]> Speakers;

    public byte[] RelocationTable;
    public byte[] ExtData;

    private byte[] TmpBytes;
    private string TmpFile = Path.GetTempFileName();

    public void ExbipHook<T>(T rw) where T : struct, IBaseBinaryTarget
    {
        rw.RwUInt8(ref this.FileType);
        rw.RwUInt8(ref this.Format);
        rw.RwUInt16(ref this.UserId);
        rw.RwUInt32(ref this.FileSize);

        rw.SetEndianness("big");
        rw.RwString(ref this.Endianness, 1, Encoding.ASCII);
        if (this.Endianness == "0")
            rw.SetEndianness("little");

        rw.RwString(ref this.Magic, 3, Encoding.ASCII);
        if (this.Magic != this.MAGIC)
            throw new Exception($"Magic string ({this.Magic}) doesn't match expected string ({this.MAGIC})");

        rw.RwUInt32(ref this.ExtSize);
        rw.RwUInt32(ref this.RelocationTableOffset);
        rw.RwUInt32(ref this.RelocationTableSize);
        rw.RwUInt32(ref this.TurnCount);
        rw.RwUInt8(ref this.IsRelocated);
        rw.RwUInt8(ref this.ReservedByte);
        rw.RwUInt16(ref this.Version);

        rw.SetRelativeOffset(rw.RelativeTell());

        if (rw.IsConstructlike())
        {
            this.TurnKinds = new UInt32[this.TurnCount];
            this.TurnOffsets = new UInt32[this.TurnCount];
            this.Turns = new Turn[this.TurnCount];
        }

        for (int i=0; i<this.TurnCount; i++)
        {
            rw.RwUInt32(ref this.TurnKinds[i]);
            rw.RwUInt32(ref this.TurnOffsets[i]);
        }

        rw.RwUInt32(ref this.SpeakerNameArrayOffset);
        rw.RwUInt32(ref this.SpeakerCount);
        rw.RwUInt32(ref this.ExtDataOffset);
        rw.RwUInt32(ref this.ReservedUInt32);

        for (int i=0; i<this.TurnCount; i++)
        {
            if (rw.IsParselike())
                this.TurnOffsets[i] = (UInt32)rw.RelativeTell();
            rw.RelativeSeek(this.TurnOffsets[i], 0);
            Globals.LAST_TURN_KIND = this.TurnKinds[i];
            rw.RwObj(ref this.Turns[i]);
        }

        if (rw.IsParselike())
            this.SpeakerNameArrayOffset = (UInt32)rw.RelativeTell();
        rw.RelativeSeek(this.SpeakerNameArrayOffset, 0);
        rw.RwUInt32s(ref this.SpeakerNameAddresses, (int)this.SpeakerCount);

        if (rw.IsConstructlike())
            this.Speakers = new List<byte[]>((Int32)this.SpeakerCount);

        for (int i=0; i<this.SpeakerCount; i++)
        {
            if (rw.IsParselike())
                this.SpeakerNameAddresses[i] = (UInt32)rw.RelativeTell();
            rw.RelativeSeek(this.SpeakerNameAddresses[i], 0);
            // error CS0206: A non ref-returning property or indexer may not be used as an out or ref value
            ////rw.RwCBytestring(ref (this.Speakers[i]));
            //byte[] tmpBytes = this.Speakers[i];
            //rw.RwCBytestring(ref tmpBytes);
            //this.Speakers[i] = tmpBytes;
            if (rw.IsParselike())
                this.TmpBytes = this.Speakers[i];
            rw.RwCBytestring(ref this.TmpBytes);
            if (rw.IsConstructlike())
                this.Speakers.Add(this.TmpBytes);
        }

        rw.SetRelativeOffset(0);

        if (this.RelocationTableSize > 0)
        {
            if (rw.IsParselike())
                this.RelocationTableOffset = (UInt32)rw.RelativeTell();
            rw.RelativeSeek(this.RelocationTableOffset, 0);
            rw.RwBytestring(ref this.RelocationTable, (int)this.RelocationTableSize);
        }

        if (this.ExtSize > 0)
        {
            if (rw.IsParselike())
                this.ExtDataOffset = (UInt32)rw.RelativeTell();
            rw.RelativeSeek(this.ExtDataOffset, 0);
            rw.RwBytestring(ref this.ExtData, (int)this.ExtSize);
        }

        rw.AssertEOF();
    }

    //public void Write(string filepath) { TraitMethods.Write(this, filepath); }
    public void Write(string filepath)
    {
        // a cheap way to update all the offsets before actually writing to the file
        TraitMethods.Write(this, this.TmpFile);
        TraitMethods.Write(this, filepath);
    }
    public void Read (string filepath) { TraitMethods.Read (this, filepath); }
}

public class Turn : ISerializable
{
    public byte[] Name;

    public UInt16 Ext;
    public UInt16 Pattern;
    public UInt16 Reserved;

    public UInt16 SpeakerId;

    public UInt16 ElemCount;
    public UInt32[] ElemStartAddresses;
    public UInt32 TextBufferSize;
    //public byte[] TextBuffer;

    //public List<byte[]> Elems;
    public List<byte[]> Elems = new List<byte[]>();

    private byte[] TmpBytes;

    public void ExbipHook<T>(T rw) where T : struct, IBaseBinaryTarget
    {
        rw.RwBytestring(ref this.Name, 24);
        if (Globals.LAST_TURN_KIND == 0)
        {
            rw.RwUInt16(ref this.ElemCount);
            rw.RwUInt16(ref this.SpeakerId);
        }
        else
        {
            rw.RwUInt16(ref this.Ext);
            rw.RwUInt16(ref this.ElemCount);
            rw.RwUInt16(ref this.Pattern);
            rw.RwUInt16(ref this.Reserved);
        }
        rw.RwUInt32s(ref this.ElemStartAddresses, this.ElemCount);

        if (this.ElemCount > 0)
        {
            if (rw.IsParselike())
            {
                this.TextBufferSize = 0;
                foreach (byte[] elem in this.Elems)
                    this.TextBufferSize += (UInt32)elem.Length;
            }
            rw.RwUInt32(ref this.TextBufferSize);
            int remainingSize = (int)this.TextBufferSize;
            for (int i=0; i<this.ElemCount; i++)
            {
                int elemSize = 0;
                if (rw.IsConstructlike())
                {
                    if (i < this.ElemCount-1)
                        elemSize = (int)(this.ElemStartAddresses[i+1] - this.ElemStartAddresses[i]);
                    else
                        elemSize = remainingSize;
                }
                if (rw.IsParselike())
                {
                    this.ElemStartAddresses[i] = (UInt32)rw.RelativeTell();
                    elemSize = this.Elems[i].Length;
                }
                // error CS0206: A non ref-returning property or indexer may not be used as an out or ref value
                //rw.RwBytestring(ref this.Elems[i], elemSize);
                if (rw.IsParselike())
                    this.TmpBytes = this.Elems[i];
                rw.RwBytestring(ref this.TmpBytes, elemSize);
                if (rw.IsConstructlike())
                    this.Elems.Add(this.TmpBytes);
                remainingSize -= elemSize;
            }
        }
        else
            this.TextBufferSize = 0;
    }

    //public void Write(string filepath) { TraitMethods.Write(this, filepath); }
    //public void Read (string filepath) { TraitMethods.Read (this, filepath); }
}
