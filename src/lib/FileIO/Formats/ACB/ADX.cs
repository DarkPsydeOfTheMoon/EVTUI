using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Serialization;

using static EVTUI.Utils;

namespace EVTUI;

public class Adx : ISerializable
{
    public ConstUInt16 HeaderMagic = new ConstUInt16(0x8000);

    public PositionalInt16 HeaderSize = new PositionalInt16();

    public byte   EncodingType;
    public byte   FrameSize;
    public byte   BitDepth;
    public byte   ChannelCount;
    public UInt32 SampleRate;
    public UInt32 SampleCount;
    public UInt16 HighpassFreq;
    public byte   Version;
    public byte   Revision;
    public byte   CodingType;

    public UInt32     HistoryPrePad;
    public UInt16[][] HistorySamples;
    public UInt32     HistoryPostPad;

    public UInt16 InsertedSamples;
    public UInt16 LoopCount;
    public UInt32 LoopType;
    public UInt32 LoopStartSample;
    public UInt32 LoopStartByte;
    public UInt32 LoopEndSample;
    public UInt32 LoopEndByte;

    public byte[]     MagicPadding;

    public MagicString AudioMagic = new MagicString("(c)CRI");

    public int    SamplesPerFrame;
    public int    FrameCount;
    public int    AudioSize;
    public byte[] AudioDataBytes;

    public ConstUInt16 FooterMagic = new ConstUInt16(0x8001);

    public UInt16     FooterSignature;
    public byte[]     Padding;

    public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
    {
        if (rw.IsConstructlike())
            Trace.TraceInformation("Reading ADX object");
        else if (rw.IsParselike())
            Trace.TraceInformation("Writing ADX object");

        rw.SetLittleEndian(false);

        rw.RwObj(ref this.HeaderMagic);

        rw.RwObj(ref this.HeaderSize);
        rw.RwUInt8(ref this.EncodingType);
        rw.RwUInt8(ref this.FrameSize);
        rw.RwUInt8(ref this.BitDepth);
        rw.RwUInt8(ref this.ChannelCount);
        rw.RwUInt32(ref this.SampleRate);
        rw.RwUInt32(ref this.SampleCount);
        rw.RwUInt16(ref this.HighpassFreq);
        rw.RwUInt8(ref this.Version);
        rw.RwUInt8(ref this.Revision);
        if (this.Revision > 0)
            this.CodingType = this.Revision;

        if (this.Version >= 4)
        {
            rw.RwUInt32(ref this.HistoryPrePad);
            if (rw.IsConstructlike())
                this.HistorySamples = new UInt16[this.ChannelCount][];
            for (int i=0; i<this.ChannelCount; i++)
                rw.RwUInt16s(ref this.HistorySamples[i], 2);
            if (this.ChannelCount == 1)
                rw.RwUInt32(ref this.HistoryPostPad);
        }

        if (rw.RelativeTell()+24 <= this.HeaderSize.Value)
        {
            rw.RwUInt16(ref this.InsertedSamples);
            rw.RwUInt16(ref this.LoopCount);
            if (this.LoopCount > 0)
            {
                rw.RwUInt32(ref this.LoopType);
                rw.RwUInt32(ref this.LoopStartSample);
                rw.RwUInt32(ref this.LoopStartByte);
                rw.RwUInt32(ref this.LoopEndSample);
                rw.RwUInt32(ref this.LoopEndByte);
            }
        }

        int magicPaddingSize = this.HeaderSize.Value+4-(int)rw.RelativeTell()-6;
        if (magicPaddingSize > 0)
            rw.RwBytestring(ref this.MagicPadding, magicPaddingSize);
        //Trace.Assert(Enumerable.SequenceEqual(Enumerable.Repeat((byte)0, magicPaddingSize), this.MagicPadding), "Ending padding should be all zero bytes, but it contains other content");
        CheckBytes(this.MagicPadding, 0);
        rw.RwObj(ref this.AudioMagic);

        this.HeaderSize.Validate((short)(rw.RelativeTell()-4), rw.IsParselike());

        this.SamplesPerFrame = (this.FrameSize - 2) * 2;
        this.FrameCount = (int)Math.Ceiling((double)this.SampleCount / this.SamplesPerFrame);
        this.AudioSize = (int)this.FrameSize * (int)this.FrameCount * (int)this.ChannelCount;
        rw.RwBytestring(ref this.AudioDataBytes, this.AudioSize);

        rw.RwObj(ref this.FooterMagic);
        rw.RwUInt16(ref this.FooterSignature);

        int paddingSize = this.FrameSize - 4;
        if (this.LoopCount > 0 && (rw.RelativeTell() + this.FrameSize) % 2048 > 0)
            paddingSize = this.FrameSize + 2048 - ((int)rw.RelativeTell() + this.FrameSize) % 2048;
        rw.RwBytestring(ref this.Padding, paddingSize);
        CheckBytes(this.Padding, 0);
        //Trace.Assert(Enumerable.SequenceEqual(Enumerable.Repeat((byte)0, paddingSize), this.Padding), "Ending padding should be all zero bytes, but it contains other content");

        rw.ResetEndianness();
    }

    public void Write(string filepath) { TraitMethods.Write(this, filepath); }
    public void Read (string filepath) { TraitMethods.Read (this, filepath); }
    public byte[] ToBytes() { return TraitMethods.ToBytes(this); }
    public void FromBytes(byte[] bytes) { TraitMethods.FromBytes(this, bytes); }

    public void Decrypt(ulong keyCode)
    {
        this.Crypt(keyCode);
        this.Revision = 0;
    }

    public void Encrypt(ulong keyCode, byte? codingType)
    {
        if (codingType is null && this.CodingType == 0)
            Trace.TraceInformation("Failed to specify coding type for encryption");
        if (codingType is null)
            this.Revision = this.CodingType;
        else
            this.Revision = (byte)codingType;
        this.Crypt(keyCode);
    }

    public void Crypt(ulong keyCode)
    {
        if (keyCode == 0)
            return;
        keyCode -= 1;
        int seed = (int)(keyCode >> 27)  & 0x7fff;
        int mult = (int)((keyCode >> 12) & 0x7ffc) | 1;
        int inc  = (int)((keyCode << 1)  & 0x7fff) | 1;
        int xor = seed;
        for (int i=0; i<this.FrameCount*this.ChannelCount; i++)
        {
            int pos = i*this.FrameSize;
            this.AudioDataBytes[pos] ^= (byte)((xor >> 8) & 0xff);
            if (this.Revision == 9)
                this.AudioDataBytes[pos] &= 0x1f;
            this.AudioDataBytes[pos+1] ^= (byte)(xor & 0xff);
            xor = (xor * mult + inc) & 0x7fff;
        }
    }
}
