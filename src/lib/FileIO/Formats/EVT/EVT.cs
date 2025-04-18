using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using DeepCopy;

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

    public Bitfield32 Flags = new Bitfield32();

    public Int32 FrameCount;
    public byte FrameRate = 30;
    public byte InitScriptIndex;
    public Int16 StartingFrame;
    public Int16 CinemascopeStartingFrame;
    public Int32 InitEnvAssetID;
    public Int32 InitDebugEnvAssetID;
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
        Trace.Assert(this.DUMMY_INT16[0] == 0);
        rw.RwInt32(ref this.FileSize);
        rw.RwInt32(ref this.FileHeaderSize);
        rw.RwObj(ref this.Flags);
        rw.RwInt32(ref this.FrameCount);
        rw.RwUInt8(ref this.FrameRate);
        rw.RwUInt8(ref this.InitScriptIndex);
        rw.RwInt16(ref this.StartingFrame);
        rw.RwInt16(ref this.CinemascopeStartingFrame);
        rw.RwInt16(ref this.DUMMY_INT16[1]);
        Trace.Assert(this.DUMMY_INT16[1] == 0);
        rw.RwInt32(ref this.InitEnvAssetID);
        rw.RwInt32(ref this.InitDebugEnvAssetID);

        rw.RwInt32(ref this.ObjectCount);
        rw.RwInt32(ref this.ObjectOffset);
        rw.RwInt32(ref this.ObjectSize);
        Trace.Assert(this.ObjectSize == EVT.ENTRY_SIZE, $"ObjectSize ({this.ObjectSize}) does not match expected entry size ({EVT.ENTRY_SIZE})");

        rw.RwInt32(ref this.DUMMY_INT32[0]);
        Trace.Assert(this.DUMMY_INT32[0] == 0);

        rw.RwInt32(ref this.CommandCount);
        rw.RwInt32(ref this.CommandOffset);
        rw.RwInt32(ref this.CommandSize);
        Trace.Assert(this.CommandSize == EVT.ENTRY_SIZE, $"CommandSize ({this.CommandSize}) does not match expected entry size ({EVT.ENTRY_SIZE})");

        rw.RwInt32(ref this.DUMMY_INT32[1]);
        Trace.Assert(this.DUMMY_INT32[1] == 0);

        rw.RwInt32(ref this.PointerToEventBmdPath);
        rw.RwInt32(ref this.EventBmdPathLength);
        Trace.Assert(this.EventBmdPathLength == 0 || this.EventBmdPathLength == 48);
        rw.RwInt32(ref this.EmbedMsgFileOfs);
        Trace.Assert(this.EmbedMsgFileOfs == 0);
        rw.RwInt32(ref this.EmbedMsgFileSize);
        Trace.Assert(this.EmbedMsgFileSize == 0);

        rw.RwInt32(ref this.PointerToEventBfPath);
        rw.RwInt32(ref this.EventBfPathLength);
        Trace.Assert(this.EventBfPathLength == 0 || this.EventBfPathLength == 48);
        rw.RwInt32(ref this.EmbedBfFileOfs);
        Trace.Assert(this.EmbedBfFileOfs == 0);
        rw.RwInt32(ref this.EmbedBfFileSize);
        Trace.Assert(this.EmbedBfFileSize == 0);

        this.MarkerFrameCount = (this.FileHeaderSize - (int)rw.RelativeTell()) / 4;
        if (rw.IsConstructlike())
            this.MarkerFrame = new Int32[this.MarkerFrameCount];
        rw.RwInt32s(ref this.MarkerFrame, this.MarkerFrameCount);

        if (rw.IsParselike())
        {
            this.ObjectOffset = (int)rw.RelativeTell();
            this.ObjectCount = this.Objects.Length;
        }
        Trace.Assert(this.ObjectOffset == rw.RelativeTell());
        rw.RwObjs(ref this.Objects, this.ObjectCount);
        Trace.Assert(this.Objects.Length == this.ObjectCount, $"Number of objects ({this.Objects.Length}) doesn't match expected ObjectCount ({this.ObjectCount})");

        if (rw.IsParselike())
        {
            this.CommandOffset = (int)rw.RelativeTell();
            this.CommandCount = this.Commands.Length;
        }
        Trace.Assert(this.CommandOffset == rw.RelativeTell());
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
            if (rw.IsParselike())
                this.Commands[i].DataOffset = (int)rw.RelativeTell();
            Trace.Assert(this.Commands[i].DataOffset == rw.RelativeTell());
            rw.RwObj((ISerializable)this.CommandData[i], new Dictionary<string, object>()
                { ["dataSize"] = this.Commands[i].DataSize });
        }

        if (this.Flags[12])
        {
            if (rw.IsParselike())
            {
                this.PointerToEventBmdPath = (int)rw.RelativeTell();
                this.EventBmdPathLength = 48;
                this.EventBmdPath = this.EventBmdPath.PadRight(48, '\0').Substring(0, 48);
            }
            Trace.Assert(this.PointerToEventBmdPath == rw.RelativeTell());
            rw.RwString(ref this.EventBmdPath, this.EventBmdPathLength, Encoding.ASCII);
        }
        else
        {
            this.PointerToEventBmdPath = 0;
            this.EventBmdPathLength = 0;
        }

        if (this.Flags[14])
        {
            if (rw.IsParselike())
            {
                this.PointerToEventBfPath = (int)rw.RelativeTell();
                this.EventBfPathLength = 48;
                this.EventBfPath = this.EventBfPath.PadRight(48, '\0').Substring(0, 48);
            }
            Trace.Assert(this.PointerToEventBfPath == rw.RelativeTell());
            rw.RwString(ref this.EventBfPath, this.EventBfPathLength, Encoding.ASCII);
        }
        else
        {
            this.PointerToEventBfPath = 0;
            this.EventBfPathLength = 0;
        }

        if (rw.IsParselike())
            this.FileSize = (int)rw.RelativeTell();
        Trace.Assert(this.FileSize == rw.RelativeTell());

        rw.AssertEOF();
    }

    public bool DeleteObject(SerialObject obj)
    {
        List<SerialObject> objList = new List<SerialObject>(this.Objects);
        if (!objList.Contains(obj))
            return false;
        objList.Remove(obj);
        this.Objects = objList.ToArray();
        Trace.Assert(this.Objects.Length == this.ObjectCount-1);

        this.ObjectCount -= 1;
        return true;
    }

    public bool DeleteCommand(int index)
    {
        if (index < 0 || index >= this.CommandCount)
            return false;

        List<SerialCommand> cmdList = new List<SerialCommand>(this.Commands);
        cmdList.RemoveAt(index);
        this.Commands = cmdList.ToArray();
        Trace.Assert(this.Commands.Length == this.CommandCount-1);

        this.CommandData.RemoveAt(index);
        Trace.Assert(this.CommandData.Count == this.CommandCount-1);

        this.CommandCount -= 1;
        return true;
    }

    public SerialObject NewObject(int type)
    {
        SerialObject newObj = new SerialObject();
        newObj.Type = type;

        // TODO: surely there's a better place to create this
        HashSet<int> ids = new HashSet<int>();
        foreach (SerialObject obj in this.Objects)
            ids.Add(obj.Id);
        // always pick the smallest unused (u)int
        for (int i=1; i<=this.ObjectCount+1; i++)
            if (!ids.Contains(i))
            {
                newObj.Id = i;
                break;
            }

        List<SerialObject> objList = new List<SerialObject>(this.Objects);
        objList.Add(newObj);
        this.Objects = objList.ToArray();

        this.ObjectCount += 1;
        return newObj;
    }

    // TODO: consolidate code between this method and prev
    public SerialObject DuplicateObject(SerialObject oldObj)
    {
        SerialObject newObj = DeepCopier.Copy(oldObj);

        // TODO: surely there's a better place to create this
        HashSet<int> ids = new HashSet<int>();
        foreach (SerialObject obj in this.Objects)
            ids.Add(obj.Id);
        // always pick the smallest unused (u)int
        for (int i=1; i<=this.ObjectCount+1; i++)
            if (!ids.Contains(i))
            {
                newObj.Id = i;
                break;
            }

        HashSet<int> dupes = new HashSet<int>();
        foreach (SerialObject obj in this.Objects)
            if (obj.Type == newObj.Type && obj.ResourceCategory == newObj.ResourceCategory && obj.ResourceMajorId == newObj.ResourceMajorId && obj.ResourceMinorId == newObj.ResourceMinorId && obj.ResourceSubId == newObj.ResourceSubId)
                dupes.Add(obj.ResourceUniqueId);
        // always pick the smallest unused (u)int
        for (int i=0; i<=dupes.Count; i++)
            if (!dupes.Contains(i))
            {
                newObj.ResourceUniqueId = i;
                break;
            }

        List<SerialObject> objList = new List<SerialObject>(this.Objects);
        objList.Add(newObj);
        this.Objects = objList.ToArray();

        this.ObjectCount += 1;
        return newObj;
    }

    public int CopyCommandToNewFrame(SerialCommand cmd, dynamic cmdData, int frame)
    {
        SerialCommand newCmd = DeepCopier.Copy(cmd);
        newCmd.FrameStart = frame;

        List<SerialCommand> cmdList = new List<SerialCommand>(this.Commands);
        cmdList.Add(newCmd);
        this.Commands = cmdList.ToArray();

        this.CommandData.Add(DeepCopier.Copy(cmdData));

        this.CommandCount += 1;
        return this.CommandCount-1;
    }

    public int NewCommand(string commandCode, int frameStart)
    {
        Type commandType = typeof(CommandTypes).GetNestedType(commandCode);
        if (commandType == null)
        {
            Console.WriteLine($"Unsupported new command type: {commandCode}");
            return -1;
        }

        SerialCommand newCmd = new SerialCommand();
        newCmd.CommandCode = commandCode;
        newCmd.FrameStart = frameStart;
        // TODO: maybe null check here
        newCmd.DataSize = (int)commandType.GetField("DataSize").GetRawConstantValue();

        List<SerialCommand> cmdList = new List<SerialCommand>(this.Commands);
        cmdList.Add(newCmd);
        this.Commands = cmdList.ToArray();

        this.CommandData.Add(Activator.CreateInstance(commandType));

        this.CommandCount += 1;
        return this.CommandCount-1;
    }

    public void Write(string filepath)
    {
        // a cheap way to update all the offsets before actually writing to the file
        TraitMethods.ToBytes(this);
        TraitMethods.Write(this, filepath);
    }
    public void Read (string filepath) { TraitMethods.Read (this, filepath); }
}

public class SerialObject : ISerializable
{
    public Int32 Id;
    public Int32 Type;
    public Int32 ResourceCategory = 1;
    public Int32 ResourceUniqueId;
    public Int32 ResourceMajorId;
    public Int16 ResourceSubId;
    public Int16 ResourceMinorId;

    public Bitfield32 Flags = new Bitfield32();

    public Int32 BaseMotionNo     = -1;
    public Int32 ExtBaseMotionNo  = -1;
    public Int32 ExtAddMotionNo   = -1;
    public Int32 UnkBool;
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
        rw.RwObj(ref this.Flags);
        rw.RwInt32(ref this.BaseMotionNo);
        rw.RwInt32(ref this.ExtBaseMotionNo);
        rw.RwInt32(ref this.ExtAddMotionNo);
        rw.RwInt32(ref this.UnkBool);

        rw.RwInt32(ref this.Reserve2C);
        Trace.Assert(this.Reserve2C == 0);
    }
}

public class SerialCommand : ISerializable
{
    public string CommandCode;
    public Int16  CommandVersion;
    public Int16  CommandType;
    public Int32  ObjectId;
    public Bitfield32 Flags = new Bitfield32();
    public Int32  FrameStart;
    public Int32  FrameDuration = 1;
    public Int32  DataOffset;
    public Int32  DataSize;
    public UInt32 ConditionalType;
    public UInt32 ConditionalIndex;
    public Int32  ConditionalValue;
    public UInt32 ConditionalComparisonType;

    public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
    {
        rw.RwString(ref this.CommandCode, 4, Encoding.ASCII);
        rw.RwInt16(ref this.CommandVersion);
        rw.RwInt16(ref this.CommandType);
        rw.RwInt32(ref this.ObjectId);
        rw.RwObj(ref this.Flags);
        rw.RwInt32(ref this.FrameStart);
        rw.RwInt32(ref this.FrameDuration);
        rw.RwInt32(ref this.DataOffset);
        rw.RwInt32(ref this.DataSize);
        rw.RwUInt32(ref this.ConditionalType);
        rw.RwUInt32(ref this.ConditionalIndex);
        rw.RwInt32(ref this.ConditionalValue);
        rw.RwUInt32(ref this.ConditionalComparisonType);
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
