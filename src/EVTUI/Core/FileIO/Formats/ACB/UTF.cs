using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using Serialization;

namespace EVTUI;

public class UtfTable : ISerializable
{
    public const string MAGIC = "@UTF";

    public string    Magic;
    public UInt32    TableSize;
    public byte      ReservedByte;
    public byte      EncodingType;
    public UInt16    RowOffset;
    public UInt32    StringsOffset;
    public UInt32    DataOffset;
    public RefString TableName;
    public UInt16    ColumnCount;
    public UInt16    RowLength;
    public UInt32    RowCount;

    public Dictionary<string, UtfField>   Fields;
    public Dictionary<string, UtfValue>[] Rows;

    private UtfValue TmpRowValue;

    public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
    {
        rw.SetLittleEndian(false);

        rw.RwString(ref this.Magic, 4, Encoding.ASCII);
        Trace.Assert(this.Magic == UtfTable.MAGIC, $"Magic string ({this.Magic}) doesn't match expected string ({UtfTable.MAGIC})");

        rw.RwUInt32(ref this.TableSize);
        rw.RwUInt8(ref this.ReservedByte);
        rw.RwUInt8(ref this.EncodingType);

        rw.RwUInt16(ref this.RowOffset);
        rw.RwUInt32(ref this.StringsOffset);
        rw.RwUInt32(ref this.DataOffset);

        rw.RwObj(ref this.TableName, new Dictionary<string, object>()
        {
            ["stringsOffset"] = this.StringsOffset + 8,
            ["encodingType"]  = this.EncodingType
        });

        rw.RwUInt16(ref this.ColumnCount);
        rw.RwUInt16(ref this.RowLength);
        rw.RwUInt32(ref this.RowCount);

        if (rw.IsConstructlike())
        {
            UtfField[] fields = new UtfField[this.ColumnCount];
            rw.RwObjs(ref fields, this.ColumnCount, new Dictionary<string, object>()
            {
                ["stringsOffset"] = this.StringsOffset + 8,
                ["encodingType"]  = this.EncodingType,
                ["dataOffset"] = this.DataOffset + 8
            });
            this.Fields = new Dictionary<string, UtfField>();
            for (int i=0; i<this.ColumnCount; i++)
                this.Fields[fields[i].Name.Value] = fields[i];
        }
        else if (rw.IsParselike())
            foreach (string fieldName in this.Fields.Keys)
            {
                rw.RwObj(this.Fields[fieldName], new Dictionary<string, object>()
                {
                    ["stringsOffset"] = this.StringsOffset + 8,
                    ["encodingType"]  = this.EncodingType,
                    ["dataOffset"] = this.DataOffset + 8
                });
            }

        if (rw.IsConstructlike())
            this.Rows = new Dictionary<string, UtfValue>[this.RowCount];
        rw.RelativeSeek(this.RowOffset + 8, 0);
        for (int i=0; i<this.RowCount; i++)
        {
            if (rw.IsConstructlike())
                this.Rows[i] = new Dictionary<string, UtfValue>();
            foreach (string fieldName in this.Fields.Keys)
            {
                if (this.Fields[fieldName].RowStorageFlag == 1)
                {
                    if (rw.IsConstructlike())
                        this.Rows[i][fieldName] = new UtfValue();
                    rw.RwObj(this.Rows[i][fieldName], new Dictionary<string, object>()
                    {
                        ["typeFlag"] = this.Fields[fieldName].TypeFlag,
                        ["stringsOffset"] = this.StringsOffset + 8,
                        ["encodingType"]  = this.EncodingType,
                        ["dataOffset"] = this.DataOffset + 8
                    });
                }
            }
        }

        rw.ResetEndianness();
    }

    public UtfValue GetRowField(int rowInd, string fieldName)
    {
        if (this.Fields[fieldName].RowStorageFlag == 1)
            return this.Rows[rowInd][fieldName];
        if (this.Fields[fieldName].DefaultValueFlag == 1)
            return this.Fields[fieldName].DefaultValue;
        return null;
    }

    public void Write(string filepath) { TraitMethods.Write(this, filepath); }
    public void Read (string filepath) { TraitMethods.Read (this, filepath); }
}

public class RefString : ISerializable
{
    public UInt32 Offset;
    public string Value;

    public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
    {
        rw.RwUInt32(ref this.Offset);
        long checkpoint = rw.RelativeTell();
        rw.RelativeSeek((uint)args["stringsOffset"] + this.Offset, 0);
        switch ((TextEncodingType)args["encodingType"])
        {
            case TextEncodingType.ShiftJis:
                rw.RwCString(ref this.Value, Encoding.GetEncoding("shift_jis"));
                break;
            case TextEncodingType.Utf8:
                rw.RwCString(ref this.Value, Encoding.UTF8);
                break;
            default:
                break;
        }
        rw.RelativeSeek(checkpoint, 0);
    }
}

public class RefData : ISerializable
{
    public UInt32    Offset;
    public Int32     Length;
    public string    Magic;

    private UtfTable UtfVal;
    private Afs2     Afs2Val;
    private byte[]   UnkVal;

    public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
    {
        rw.RwUInt32(ref this.Offset);
        rw.RwInt32(ref this.Length);

        if (this.Length > 0)
        {
            long checkpoint = rw.RelativeTell();
            rw.RelativeSeek((uint)args["dataOffset"] + this.Offset, 0);
            rw.SetRelativeOffset(rw.Tell());
            if (rw.IsConstructlike())
            {
                // TODO: might need to change this to bytes so it won't break on UNK case...
                // (but it hasn't actually broken yet, sooo...)
                rw.RwString(ref this.Magic, 4, Encoding.ASCII);
                rw.RelativeSeek(0, 0);
            }
            switch (this.Magic)
            {
                case UtfTable.MAGIC:
                    rw.RwObj(ref this.UtfVal);
                    break;
                case Afs2.MAGIC:
                    rw.RwObj(ref this.Afs2Val);
                    break;
                default:
                    rw.RwUInt8s(ref this.UnkVal, this.Length);
                    break;
            }
            rw.ResetRelativeOffset();
            rw.RelativeSeek(checkpoint, 0);
        }
    }

    public dynamic GetValue()
    {
        switch (this.Magic)
        {
            case UtfTable.MAGIC:
                return this.UtfVal;
            case Afs2.MAGIC:
                return this.Afs2Val;
            default:
                return this.UnkVal;
        }
    }
}

public class UtfField : ISerializable
{
    public byte      BitFlag;
    public byte      TypeFlag;
    public byte      StorageFlag;

    public byte      NameFlag;
    public byte      DefaultValueFlag;
    public byte      RowStorageFlag;

    public RefString Name;
    public UtfValue  DefaultValue;

    public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
    {
        rw.RwUInt8(ref this.BitFlag);

        this.TypeFlag    = (byte)(this.BitFlag        & 0xF);
        this.StorageFlag = (byte)((this.BitFlag >> 4) & 0xF);

        this.NameFlag         = (byte)(this.StorageFlag        & 0x1);
        this.DefaultValueFlag = (byte)((this.StorageFlag >> 1) & 0x1);
        this.RowStorageFlag   = (byte)((this.StorageFlag >> 2) & 0x1);

        if (this.NameFlag == 1)
            rw.RwObj(ref this.Name, new Dictionary<string, object>()
            {
                ["stringsOffset"] = args["stringsOffset"],
                ["encodingType"]  = args["encodingType"]
            });
        if (this.DefaultValueFlag == 1)
            rw.RwObj(ref this.DefaultValue, new Dictionary<string, object>()
            {
                ["typeFlag"]      = this.TypeFlag,
                ["stringsOffset"] = args["stringsOffset"],
                ["encodingType"]  = args["encodingType"],
                ["dataOffset"]    = args["dataOffset"]
            });
        
    }
}

public class UtfValue : ISerializable
{
    public byte       TypeFlag;

    private byte      ByteValue;
    private sbyte     SByteValue;
    private UInt16    UInt16Value;
    private Int16     Int16Value;
    private UInt32    UInt32Value;
    private Int32     Int32Value;
    private UInt64    UInt64Value;
    private Int64     Int64Value;
    private Single    SingleValue;
    private Double    DoubleValue;
    private RefString StringValue;
    private RefData   DataValue;
    private byte[]    GuidValue;

    public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
    {
        this.TypeFlag = (byte)args["typeFlag"];
        switch ((UtfTypeFlag)this.TypeFlag)
        {
            case UtfTypeFlag.UInt8:
                rw.RwUInt8(ref this.ByteValue);
                break;
            case UtfTypeFlag.Int8:
                rw.RwInt8(ref this.SByteValue);
                break;
            case UtfTypeFlag.UInt16:
                rw.RwUInt16(ref this.UInt16Value);
                break;
            case UtfTypeFlag.Int16:
                rw.RwInt16(ref this.Int16Value);
                break;
            case UtfTypeFlag.UInt32:
                rw.RwUInt32(ref this.UInt32Value);
                break;
            case UtfTypeFlag.Int32:
                rw.RwInt32(ref this.Int32Value);
                break;
            case UtfTypeFlag.UInt64:
                rw.RwUInt64(ref this.UInt64Value);
                break;
            case UtfTypeFlag.Int64:
                rw.RwInt64(ref this.Int64Value);
                break;
            case UtfTypeFlag.Single:
                rw.RwFloat32(ref this.SingleValue);
                break;
            case UtfTypeFlag.Double:
                rw.RwFloat64(ref this.DoubleValue);
                break;
            case UtfTypeFlag.String:
                rw.RwObj(ref this.StringValue, new Dictionary<string, object>()
                {
                    ["stringsOffset"] = args["stringsOffset"],
                    ["encodingType"]  = args["encodingType"]
                });
                break;
            case UtfTypeFlag.Data:
                rw.RwObj(ref this.DataValue, new Dictionary<string, object>()
                {
                    ["stringsOffset"] = args["stringsOffset"],
                    ["dataOffset"]  = args["dataOffset"]
                });
                break;
            case UtfTypeFlag.GUID:
                rw.RwUInt8s(ref this.GuidValue, 16);
                break;
            default:
                break;
        }
    }

    public dynamic GetValue()
    {
        switch ((UtfTypeFlag)this.TypeFlag)
        {
            case UtfTypeFlag.UInt8:
                return this.ByteValue;
            case UtfTypeFlag.Int8:
                return this.SByteValue;
            case UtfTypeFlag.UInt16:
                return this.UInt16Value;
            case UtfTypeFlag.Int16:
                return this.Int16Value;
            case UtfTypeFlag.UInt32:
                return this.UInt32Value;
            case UtfTypeFlag.Int32:
                return this.Int32Value;
            case UtfTypeFlag.UInt64:
                return this.UInt64Value;
            case UtfTypeFlag.Int64:
                return this.Int64Value;
            case UtfTypeFlag.Single:
                return this.SingleValue;
            case UtfTypeFlag.Double:
                return this.DoubleValue;
            case UtfTypeFlag.String:
                return this.StringValue;
            case UtfTypeFlag.Data:
                return this.DataValue;
            case UtfTypeFlag.GUID:
                return this.GuidValue;
            default:
                return null;
        }
    }
}

public enum TextEncodingType : byte
{
    ShiftJis = 0,
    Utf8     = 1
}

public enum UtfTypeFlag : byte
{
    UInt8  = 0,
    Int8   = 1,
    UInt16 = 2,
    Int16  = 3,
    UInt32 = 4,
    Int32  = 5,
    UInt64 = 6,
    Int64  = 7,
    Single = 8,
    Double = 9,
    String = 10,
    Data   = 11,
    GUID   = 12
}
