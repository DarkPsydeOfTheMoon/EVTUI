using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Serialization;

namespace EVTUI;

public class MAP : ISerializable
{
    private bool IsLittleEndian = false;

    public Int32 EntryCount;

    public byte UnkBool1;
    public byte UnkBool2;
    public UInt16 Flags;

    public UInt32 Version;

    public MAPEntry[] Entries;

    public byte ExtraUnk1;
    public byte ExtraUnk2;
    public byte ExtraUnk3;
    public byte ExtraUnk4;
    public UInt16 ExtraUnkEnum;

    public ConstUInt8[] UNUSED_UINT8 = Enumerable.Range(0, 2).Select(i => new ConstUInt8()).ToArray();
    public ConstUInt32[] UNUSED_UINT32 = Enumerable.Range(0, 3).Select(i => new ConstUInt32()).ToArray();

    public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
    {
        if (rw.IsConstructlike())
        {
            this.IsLittleEndian = true;
            rw.SetLittleEndian(this.IsLittleEndian);
            rw.RwInt32(ref this.EntryCount);
            // reasonable-enough check to work lol
            if (this.EntryCount > 999)
                this.IsLittleEndian = false;
            rw.ResetEndianness();
            rw.RelativeSeek(0, 0);
        }

        rw.SetLittleEndian(this.IsLittleEndian);

        rw.RwInt32(ref this.EntryCount);

        rw.RwUInt8(ref this.UnkBool1);
        rw.RwUInt8(ref this.UnkBool2);
        rw.RwUInt16(ref this.Flags);

        rw.RwUInt32(ref this.Version);

        rw.RwObj(ref this.UNUSED_UINT32[0]);

        if (rw.IsConstructlike())
            this.Entries = Enumerable.Range(0, this.EntryCount).Select(i => new MAPEntry()).ToArray();
        rw.RwObjs(ref this.Entries, this.EntryCount, new Dictionary<string, object>()
            { ["version"] = this.Version });

        if (((this.Flags >> 9) & 1) == 1)
        {
            rw.RwUInt8(ref this.ExtraUnk1);
            rw.RwUInt8(ref this.ExtraUnk2);
            rw.RwObj(ref this.UNUSED_UINT8[0]);

            rw.RwUInt8(ref this.ExtraUnk3);
            rw.RwUInt8(ref this.ExtraUnk4);
            rw.RwObj(ref this.UNUSED_UINT8[1]);

            rw.RwUInt16(ref this.ExtraUnkEnum);

            rw.RwObj(ref this.UNUSED_UINT32[1]);
            rw.RwObj(ref this.UNUSED_UINT32[2]);
        }

        rw.ResetEndianness();
        rw.AssertEOF();
    }

    public void Write(string filepath) { TraitMethods.Write(this, filepath); }
    public void Read (string filepath) { TraitMethods.Read (this, filepath); }
    public byte[] ToBytes() { return TraitMethods.ToBytes(this); }
    public void FromBytes(byte[] bytes) { TraitMethods.FromBytes(this, bytes); }
}

public class MAPEntry : ISerializable
{
    public UInt16 MajorID;
    public UInt16 MinorID;

    public byte X;
    public byte Y;
    public byte Z;
    public byte Direction;

    public byte UnkEnum;
    public byte UnkBool;
    public byte Priority;
    
    public ConstUInt8  UNUSED_UINT8  = new ConstUInt8();
    public ConstUInt32 UNUSED_UINT32 = new ConstUInt32();

    public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
    {
        rw.RwUInt16(ref this.MajorID);
        rw.RwUInt16(ref this.MinorID);

        rw.RwUInt8(ref this.X);
        //rw.RwUInt8(ref this.Y);
        rw.RwUInt8(ref this.Z);
        //rw.RwUInt8(ref this.Z);
        //rw.RwUInt8(ref this.Y);
        rw.RwUInt8(ref this.Direction);
        //rw.RwUInt8(ref this.Direction);
        rw.RwUInt8(ref this.Y);

        rw.RwObj(ref this.UNUSED_UINT8);

        rw.RwUInt8(ref this.UnkEnum);
        if (args.ContainsKey("version") && (uint)args["version"] < 0x1000100 && this.UnkEnum != 0)
            Trace.TraceWarning("Expected zero value for UnkEnum in MAPEntry");

        rw.RwUInt8(ref this.UnkBool);
        if (args.ContainsKey("version") && (uint)args["version"] < 0x1000100 && this.UnkBool != 0)
            Trace.TraceWarning("Expected zero value for UnkBool in MAPEntry");

        rw.RwUInt8(ref this.Priority);
        if (args.ContainsKey("version") && (uint)args["version"] < 0x1000001 && this.Priority != 0)
            Trace.TraceWarning("Expected zero value for Priority in MAPEntry");

        if (args.ContainsKey("version") && (uint)args["version"] >= 0x1000100)
            rw.RwObj(ref this.UNUSED_UINT32);
    }
}
