using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using Serialization;

namespace EVTUI;

public class EVT : ISerializable
{
    private static string MAGIC         = "EVT";
    public  static Int32  ENTRY_SIZE    = 48;

    public string Magic;
    public byte Endianness;

    public UInt32 Version;
    public Int16 MajorId;
    public Int16 MinorId;
    public byte Rank;
    public byte Level;
    public Int32 FileSize;
    public Int32 FileHeaderSize;
    public UInt32 Flags;
    public Int32 TotalFrame;
    public byte FrameRate;
    public byte InitScriptIndex;
    public Int16 StartFrame;
    public Int16 LetterBoxInFrame;
    public Int32 InitEnvAssetID;
    public Int32 InitEnvAssetIDDbg;
    public Int32 ObjectCount;
    public Int32 ObjectOffset;
    public Int32 ObjectSize;
    public Int32 CommandCount;
    public Int32 CommandOffset;
    public Int32 CommandSize;
    public Int32 PointerToEventBmdPath;
    public Int32 EventBmdPathLength;
    public Int32 EmbedMsgFileOfs;
    public Int32 EmbedMsgFileSize;
    public Int32 PointerToEventBfPath;
    public Int32 EventBfPathLength;
    public Int32 EmbedBfFileOfs;
    public Int32 EmbedBfFileSize;

    public int MarkerFrameCount;
    public Int32[] MarkerFrame;

    public string EventBmdPath;
    public string EventBfPath;
    public SerialObject[] Objects;
    public SerialCommand[] Commands;
    public ArrayList CommandData;

    public Int16[] DUMMY_INT16 = new Int16[2];
    public Int32[] DUMMY_INT32 = new Int32[2];

    public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
    {
        rw.SetLittleEndian(true);
        rw.RwString(ref this.Magic, 3, Encoding.ASCII);
        Trace.Assert(this.Magic == EVT.MAGIC, $"Magic string ({this.Magic}) doesn't match expected string ({EVT.MAGIC})");
        rw.RwUInt8(ref this.Endianness);
        if (this.Endianness == 1)
            rw.SetLittleEndian(false);

        rw.RwUInt32(ref this.Version);
        rw.RwInt16(ref this.MajorId);
        rw.RwInt16(ref this.MinorId);

        rw.RwUInt8(ref this.Rank);
        rw.RwUInt8(ref this.Level);
        rw.RwInt16(ref this.DUMMY_INT16[0]);
        rw.RwInt32(ref this.FileSize);
        rw.RwInt32(ref this.FileHeaderSize);
        rw.RwUInt32(ref this.Flags);
        rw.RwInt32(ref this.TotalFrame);
        rw.RwUInt8(ref this.FrameRate);
        rw.RwUInt8(ref this.InitScriptIndex);
        rw.RwInt16(ref this.StartFrame);
        rw.RwInt16(ref this.LetterBoxInFrame);
        rw.RwInt16(ref this.DUMMY_INT16[1]);
        rw.RwInt32(ref this.InitEnvAssetID);
        rw.RwInt32(ref this.InitEnvAssetIDDbg);

        rw.RwInt32(ref this.ObjectCount);
        rw.RwInt32(ref this.ObjectOffset);
        rw.RwInt32(ref this.ObjectSize);
        Trace.Assert(this.ObjectSize == EVT.ENTRY_SIZE, $"ObjectSize ({this.ObjectSize}) does not match expected entry size ({EVT.ENTRY_SIZE})");

        rw.RwInt32(ref this.DUMMY_INT32[0]);

        rw.RwInt32(ref this.CommandCount);
        rw.RwInt32(ref this.CommandOffset);
        rw.RwInt32(ref this.CommandSize);
        Trace.Assert(this.CommandSize == EVT.ENTRY_SIZE, $"CommandSize ({this.CommandSize}) does not match expected entry size ({EVT.ENTRY_SIZE})");

        rw.RwInt32(ref this.DUMMY_INT32[1]);

        rw.RwInt32(ref this.PointerToEventBmdPath);
        rw.RwInt32(ref this.EventBmdPathLength);
        rw.RwInt32(ref this.EmbedMsgFileOfs);
        rw.RwInt32(ref this.EmbedMsgFileSize);

        rw.RwInt32(ref this.PointerToEventBfPath);
        rw.RwInt32(ref this.EventBfPathLength);
        rw.RwInt32(ref this.EmbedBfFileOfs);
        rw.RwInt32(ref this.EmbedBfFileSize);

        this.MarkerFrameCount = (this.FileHeaderSize - (int)rw.RelativeTell()) / 4;
        if (rw.IsConstructlike())
            this.MarkerFrame = new Int32[this.MarkerFrameCount];
        rw.RwInt32s(ref this.MarkerFrame, this.MarkerFrameCount);

        rw.Seek(this.ObjectOffset, 0);
        rw.RwObjs(ref this.Objects, this.ObjectCount);
        Trace.Assert(this.Objects.Length == this.ObjectCount, $"Number of objects ({this.Objects.Length}) doesn't match expected ObjectCount ({this.ObjectCount})");

        rw.Seek(this.CommandOffset, 0);
        rw.RwObjs(ref this.Commands, this.CommandCount);
        Trace.Assert(this.Commands.Length == this.CommandCount, $"Number of commands ({this.Commands.Length}) doesn't match expected CommandCount ({this.CommandCount})");

        if (rw.IsConstructlike())
           this.CommandData = new ArrayList();
        for (var i=0; i<this.CommandCount; i++)
        {
            if (rw.IsConstructlike())
            {
                Type commandType = typeof(CommandTypes).GetNestedType(this.Commands[i].CommandCode);
                if (commandType == null)
                    this.CommandData.Add(new SerialCommandData());
                else
                    this.CommandData.Add(Activator.CreateInstance(commandType));
            }
            rw.RwObj((ISerializable)this.CommandData[i], new Dictionary<string, object>()
                { ["dataSize"] = this.Commands[i].DataSize });
            // for debugging, TBD eventually
            //Console.WriteLine(this.CommandData[i].GetType().ToString());
        }

        if (this.PointerToEventBmdPath != 0) {
           rw.Seek(this.PointerToEventBmdPath, 0);
           rw.RwString(ref this.EventBmdPath, this.EventBmdPathLength, Encoding.ASCII);
        }

        if (this.PointerToEventBfPath != 0) {
           rw.Seek(this.PointerToEventBfPath, 0);
           rw.RwString(ref this.EventBfPath, this.EventBfPathLength, Encoding.ASCII);
        }

        rw.AssertEOF();
    }

    public void Write(string filepath) { TraitMethods.Write(this, filepath); }
    public void Read (string filepath) { TraitMethods.Read (this, filepath); }
}

public class SerialObject : ISerializable
{
    public Int32 Id;
    public Int32 Type;
    public Int32 ResourceCategory;
    public Int32 ResourceUniqueId;
    public Int32 ResourceMajorId;
    public Int16 ResourceSubId;
    public Int16 ResourceMinorId;
    public UInt32 Flags;
    public Int32 BaseMotionNo;
    public Int32 ExtBaseMotionNo;
    public Int32 ExtAddMotionNo;
    public Int32 Reserve28;
    public Int32 Reserve2C;

    public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
    {
        rw.RwInt32(ref this.Id);
        rw.RwInt32(ref this.Type);
        rw.RwInt32(ref this.ResourceCategory);
        rw.RwInt32(ref this.ResourceUniqueId);
        rw.RwInt32(ref this.ResourceMajorId);
        rw.RwInt16(ref this.ResourceSubId);
        rw.RwInt16(ref this.ResourceMinorId);
        rw.RwUInt32(ref this.Flags);
        rw.RwInt32(ref this.BaseMotionNo);
        rw.RwInt32(ref this.ExtBaseMotionNo);
        rw.RwInt32(ref this.ExtAddMotionNo);
        rw.RwInt32(ref this.Reserve28);
        rw.RwInt32(ref this.Reserve2C);
    }
}

public class SerialCommand : ISerializable
{
    public string CommandCode;
    public Int16 CommandVersion;
    public Int16 CommandType;
    public Int32 ObjectId;
    public Int32 Flags;
    public Int32 FrameStart;
    public Int32 FrameDuration;
    public Int32 DataOffset;
    public Int32 DataSize;
    public Int32 ConditionalType;
    public UInt32 ConditionalIndex;
    public Int32 ConditionalValue;
    public Int32 ConditionalComparisonType;

    public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
    {
        rw.RwString(ref this.CommandCode, 4, Encoding.ASCII);
        rw.RwInt16(ref this.CommandVersion);
        rw.RwInt16(ref this.CommandType);
        rw.RwInt32(ref this.ObjectId);
        rw.RwInt32(ref this.Flags);
        rw.RwInt32(ref this.FrameStart);
        rw.RwInt32(ref this.FrameDuration);
        rw.RwInt32(ref this.DataOffset);
        rw.RwInt32(ref this.DataSize);
        rw.RwInt32(ref this.ConditionalType);
        rw.RwUInt32(ref this.ConditionalIndex);
        rw.RwInt32(ref this.ConditionalValue);
        rw.RwInt32(ref this.ConditionalComparisonType);
    }
}

public class SerialCommandData : ISerializable
{
    public byte[] UNK;

    public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
    {
        rw.RwBytestring(ref this.UNK, (int)args["dataSize"]);
    }
}
