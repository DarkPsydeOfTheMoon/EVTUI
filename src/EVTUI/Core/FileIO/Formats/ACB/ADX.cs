using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Serialization;

namespace EVTUI;

public class Adx : ISerializable
{
    public const           ushort HEADER_MAGIC = 0x8000;
    public static readonly byte[] AUDIO_MAGIC  = Encoding.ASCII.GetBytes("(c)CRI");
    public const           ushort FOOTER_MAGIC = 0x8001;

    public UInt16     HeaderMagic;
    public UInt16     HeaderSize;
    public byte       EncodingType;
    public byte       FrameSize;
    public byte       BitDepth;
    public byte       ChannelCount;
    public UInt32     SampleRate;
    public UInt32     SampleCount;
    public UInt16     HighpassFreq;
    public byte       Version;
    public byte       Revision;
    public byte       CodingType;

    public UInt32     HistoryPrePad;
    public UInt16[][] HistorySamples;
    public UInt32     HistoryPostPad;

    public UInt16     InsertedSamples;
    public UInt16     LoopCount;
    public UInt32     LoopType;
    public UInt32     LoopStartSample;
    public UInt32     LoopStartByte;
    public UInt32     LoopEndSample;
    public UInt32     LoopEndByte;

    public byte[]     AudioMagic;
    public int        SamplesPerFrame;
    public int        FrameCount;
    public int        AudioSize;
    public byte[]     AudioDataBytes;

    public UInt16     FooterMagic;
    public UInt16     FooterSignature;
    public byte[]     Padding;

    public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
    {
        rw.SetLittleEndian(false);

        rw.RwUInt16(ref this.HeaderMagic);
        Trace.Assert(this.HeaderMagic == Adx.HEADER_MAGIC, $"Magic string ({this.HeaderMagic}) doesn't match expected string ({Adx.HEADER_MAGIC})");

        rw.RwUInt16(ref this.HeaderSize);
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

        if (rw.RelativeTell()+24 <= this.HeaderSize)
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

        int magicSize = this.HeaderSize+4-(int)rw.RelativeTell();
        rw.RwBytestring(ref this.AudioMagic, this.HeaderSize+4-(int)rw.RelativeTell());
        byte[] checkMagic = Enumerable.Repeat((byte)0, this.AudioMagic.Length).ToArray();
        Array.Copy(Adx.AUDIO_MAGIC, 0, checkMagic, checkMagic.Length-6, 6);
        Trace.Assert(Enumerable.SequenceEqual(this.AudioMagic, checkMagic), $"Magic audio string doesn't match expected zero-padded '(c)CRI'");

        Trace.Assert(rw.RelativeTell() == this.HeaderSize+4, $"Header size ({rw.RelativeTell}) does not match expected size ({this.HeaderSize+4})");

        this.SamplesPerFrame = (this.FrameSize - 2) * 2;
        Trace.Assert(this.SamplesPerFrame == 32, $"Samples per frame ({this.SamplesPerFrame}) does not match expected count (32)");

        this.FrameCount = (int)Math.Ceiling((double)this.SampleCount / this.SamplesPerFrame);
        this.AudioSize = (int)this.FrameSize * (int)this.FrameCount * (int)this.ChannelCount;
        rw.RwBytestring(ref this.AudioDataBytes, this.AudioSize);

        rw.RwUInt16(ref this.FooterMagic);
        Trace.Assert(this.FooterMagic == Adx.FOOTER_MAGIC, $"Magic string ({this.FooterMagic}) doesn't match expected string ({Adx.FOOTER_MAGIC})");
        rw.RwUInt16(ref this.FooterSignature);

        int paddingSize = this.FrameSize - 4;
        if (this.LoopCount > 0 && (rw.RelativeTell() + this.FrameSize) % 2048 > 0)
            paddingSize = this.FrameSize + 2048 - ((int)rw.RelativeTell() + this.FrameSize) % 2048;
        rw.RwBytestring(ref this.Padding, paddingSize);
        Trace.Assert(Enumerable.SequenceEqual(Enumerable.Repeat((byte)0, paddingSize), this.Padding), "Ending padding should be all zero bytes, but it contains other content");

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
        Trace.Assert(!(codingType is null) || this.CodingType != 0, "Must specify coding type for encryption");
        if (!(codingType is null))
            this.Revision = (byte)codingType;
        else
            this.Revision = this.CodingType;
        this.Crypt(keyCode);
    }

    public void Crypt(ulong keyCode)
    {
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
