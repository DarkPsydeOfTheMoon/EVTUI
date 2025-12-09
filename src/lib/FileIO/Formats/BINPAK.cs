using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Serialization;

//using static EVTUI.Utils;

namespace EVTUI;

public class AtlusArchive : ISerializable
{
    private bool IsCutin = false;

    private bool IsOldVersion   = false;
    private int  NamesLength    = 32;
    private bool IsLittleEndian = false;

    public Int32       EntryCount;
    public FileEntry[] Entries;

    public AtlusArchive(bool isCutin = false)
    {
        this.IsCutin = isCutin;
    }

    public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
    {
        if (rw.IsConstructlike())
            Trace.TraceInformation("Reading BIN/PAK object");
        else if (rw.IsParselike())
            Trace.TraceInformation("Writing BIN/PAK object");

        if (rw.IsConstructlike())
        {
            byte[] versionPeek = new byte[5];
            rw.RwUInt8s(ref versionPeek, 5);
            if (versionPeek[0] == 0)
            {
                this.IsOldVersion   = false;
                this.NamesLength    = 32;
                this.IsLittleEndian = false;
            }
            else if (versionPeek[3] == 0 && versionPeek[4] != 0)
            {
                this.IsOldVersion   = false;
                this.NamesLength    = 32;
                this.IsLittleEndian = true;
            }
            else
            {
                this.IsOldVersion   = true;
                this.NamesLength    = 252;
                this.IsLittleEndian = true;
            }
            rw.RelativeSeek(0, 0);
        }

        rw.SetLittleEndian(this.IsLittleEndian);

        if (this.IsOldVersion)
        {
            Console.WriteLine("die, i guess...");
        }
        else
        {
            rw.RwInt32(ref this.EntryCount);
            if (rw.IsConstructlike())
                this.Entries = Enumerable.Range(0, this.EntryCount).Select(i => new FileEntry()).ToArray();
            rw.RwObjs(ref this.Entries, this.EntryCount, new Dictionary<string, object>()
                { ["nameLength"] = this.NamesLength, ["isCutin"] = this.IsCutin });
        }

        rw.AssertEOF();
    }

    public void Write(string filepath) { TraitMethods.Write(this, filepath); }
    public void Read (string filepath) { TraitMethods.Read (this, filepath); }
    public byte[] ToBytes() { return TraitMethods.ToBytes(this); }
    public void FromBytes(byte[] bytes) { TraitMethods.FromBytes(this, bytes); }
}

public class FileEntry : ISerializable
{
    public string Name = "";
    public Int32  Ind  = 0;

    public byte[] Header;

    public Int32  Size = 0;
    public byte[] Data;

    public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
    {
        if (args.ContainsKey("isCutin") && (bool)args["isCutin"])
            rw.RwInt32(ref this.Ind);
        else
            rw.RwString(ref this.Name, (int)args["nameLength"], Encoding.ASCII);

        rw.RwInt32(ref this.Size);

        //if (args.ContainsKey("isCutin") && (bool)args["isCutin"])
        //{
        //    rw.RwBytestring(ref this.Header, 32);
        //    rw.RwBytestring(ref this.Data, this.Size-32);
        //}
        //else
            rw.RwBytestring(ref this.Data, this.Size);
    }
}

public class GLH : ISerializable
{
    public MagicString GLHMagic = new MagicString("0HLG");

    public UInt32 Version  = 0x01105030;
    public UInt32 GlzCount = 1;

    public Int32 DecompressedSize;
    public Int32 CompressedSize;

    public byte[] Padding;
    public byte[] Data;
    public GLZ    DecompressedData = new GLZ();

    public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
    {
        rw.RwObj(ref this.GLHMagic);
        rw.RwUInt32(ref this.Version);
        rw.RwUInt32(ref this.GlzCount);
        rw.RwInt32(ref this.DecompressedSize);
        rw.RwInt32(ref this.CompressedSize);
        rw.RwBytestring(ref this.Padding, 12);

        rw.RwUInt8s(ref this.Data, this.CompressedSize - (int)rw.RelativeTell());
        //rw.RwObj();
        //Console.WriteLine(System.Text.Encoding.Default.GetString(this.Header));
        if (System.Text.Encoding.Default.GetString(new byte[] { this.Data[0], this.Data[1], this.Data[2], this.Data[3] }) == "0ZLG")
            this.DecompressedData.FromBytes(this.Data);

        rw.AssertEOF();
    }

    public void Write(string filepath) { TraitMethods.Write(this, filepath); }
    public void Read (string filepath) { TraitMethods.Read (this, filepath); }
    public byte[] ToBytes() { return TraitMethods.ToBytes(this); }
    public void FromBytes(byte[] bytes) { TraitMethods.FromBytes(this, bytes); }
}

public class GLZ : ISerializable
{
    public MagicString GLZMagic = new MagicString("0ZLG");

    public UInt32 Version  = 0x01105030;

    public Int32 DecompressedSize;
    public Int32 CompressedSize;

    public byte Marker;

    public byte[] Padding;
    public byte[] CompressedData;
    public byte[] Data;

    public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
    {
        rw.RwObj(ref this.GLZMagic);
        rw.RwUInt32(ref this.Version);
        rw.RwInt32(ref this.DecompressedSize);
        rw.RwInt32(ref this.CompressedSize);

        rw.RwUInt8(ref this.Marker);

        rw.RwBytestring(ref this.Padding, 15);
        //Console.WriteLine(System.Text.Encoding.Default.GetString(this.Padding));
        //Console.WriteLine(rw.RelativeTell());

        //if (rw.IsParselike())
        rw.RwUInt8s(ref this.CompressedData, this.CompressedSize - (int)rw.RelativeTell());
        if (rw.IsConstructlike())
            this.Decompress();

        rw.AssertEOF();
    }

    public void Decompress()
    {
        ArrayList tmp = new ArrayList();
        int i = 0;
        while (i < this.CompressedData.Length)
        {
            byte b = this.CompressedData[i];
            i += 1;

            if (b == this.Marker)
            {
                byte offset = this.CompressedData[i];
                i += 1;

                if (offset == this.Marker)
                    tmp.Add(offset);
                else
                {
                    int count = this.CompressedData[i];
                    i += 1;

                    if (offset > this.Marker)
                        offset -= 1;

                    for (int j=0; j<count; j++)
                        tmp.Add(tmp[tmp.Count-offset]);
                }
            }
            else
                tmp.Add(b);
        }
        this.Data = (byte[])tmp.ToArray(typeof(byte));
    }

    public void Write(string filepath) { TraitMethods.Write(this, filepath); }
    public void Read (string filepath) { TraitMethods.Read (this, filepath); }
    public byte[] ToBytes() { return TraitMethods.ToBytes(this); }
    public void FromBytes(byte[] bytes) { TraitMethods.FromBytes(this, bytes); }
}
