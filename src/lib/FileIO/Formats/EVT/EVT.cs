using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using DeepCopy;

using Serialization;

namespace EVTUI;

public class EVT : ISerializable
{
    public const string MAGIC = "EVT";
    public MagicString Magic = new MagicString(EVT.MAGIC);
    public byte Endianness;

    public UInt32 Version;
    public Int16 MajorId;
    public Int16 MinorId;
    public byte Rank;
    public byte Level;

    public PositionalInt32 FileSize = new PositionalInt32();
    public Int32 FileHeaderSize;

    public Bitfield32 Flags = new Bitfield32();

    public Int32 FrameCount;
    public byte FrameRate = 30;
    public byte InitScriptIndex;
    public Int16 StartingFrame;
    public Int16 CinemascopeStartingFrame;
    public Int32 InitEnvAssetID;
    public Int32 InitDebugEnvAssetID;

    public PositionalInt32 ObjectCount = new PositionalInt32();
    public PositionalInt32 ObjectOffset = new PositionalInt32();
    public ConstUInt32 ObjectSize = new ConstUInt32(48);

    public PositionalInt32 CommandCount = new PositionalInt32();
    public PositionalInt32 CommandOffset = new PositionalInt32();
    public ConstUInt32 CommandSize = new ConstUInt32(48);

    public PositionalInt32 PointerToEventBmdPath = new PositionalInt32();
    public PositionalInt32 EventBmdPathLength = new PositionalInt32();
    public ConstUInt32 EmbedMsgFileOfs = new ConstUInt32(0);
    public ConstUInt32 EmbedMsgFileSize = new ConstUInt32(0);

    public PositionalInt32 PointerToEventBfPath = new PositionalInt32();
    public PositionalInt32 EventBfPathLength = new PositionalInt32();
    public ConstUInt32 EmbedBfFileOfs = new ConstUInt32(0);
    public ConstUInt32 EmbedBfFileSize = new ConstUInt32(0);

    public int MarkerFrameCount;
    public Int32[] MarkerFrame;

    public string EventBmdPath;
    public string EventBfPath;
    public SerialObject[] Objects;
    public SerialCommand[] Commands;
    public ArrayList CommandData;

    public ConstUInt16[] UNUSED_UINT16 = Enumerable.Range(0, 2).Select(i => new ConstUInt16()).ToArray();
    public ConstUInt32[] UNUSED_UINT32 = Enumerable.Range(0, 2).Select(i => new ConstUInt32()).ToArray();

    public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
    {
        if (rw.IsConstructlike())
            Trace.TraceInformation("Reading EVT file");
        else if (rw.IsParselike())
            Trace.TraceInformation("Writing EVT file");

        rw.SetLittleEndian(true);
        rw.RwObj(ref this.Magic);
        rw.RwUInt8(ref this.Endianness);
        if (this.Endianness == 1)
            rw.SetLittleEndian(false);

        rw.RwUInt32(ref this.Version);
        rw.RwInt16(ref this.MajorId);
        rw.RwInt16(ref this.MinorId);

        rw.RwUInt8(ref this.Rank);
        rw.RwUInt8(ref this.Level);

        rw.RwObj(ref this.UNUSED_UINT16[0]);

        rw.RwObj(ref this.FileSize);
        rw.RwInt32(ref this.FileHeaderSize);

        rw.RwObj(ref this.Flags);
        rw.RwInt32(ref this.FrameCount);
        rw.RwUInt8(ref this.FrameRate);
        rw.RwUInt8(ref this.InitScriptIndex);
        rw.RwInt16(ref this.StartingFrame);
        rw.RwInt16(ref this.CinemascopeStartingFrame);

        rw.RwObj(ref this.UNUSED_UINT16[1]);

        rw.RwInt32(ref this.InitEnvAssetID);
        rw.RwInt32(ref this.InitDebugEnvAssetID);

        rw.RwObj(ref this.ObjectCount);
        rw.RwObj(ref this.ObjectOffset);
        rw.RwObj(ref this.ObjectSize);

        rw.RwObj(ref this.UNUSED_UINT32[0]);

        rw.RwObj(ref this.CommandCount);
        rw.RwObj(ref this.CommandOffset);
        rw.RwObj(ref this.CommandSize);

        rw.RwObj(ref this.UNUSED_UINT32[1]);

        rw.RwObj(ref this.PointerToEventBmdPath);
        rw.RwObj(ref this.EventBmdPathLength);
        rw.RwObj(ref this.EmbedMsgFileOfs);
        rw.RwObj(ref this.EmbedMsgFileSize);

        rw.RwObj(ref this.PointerToEventBfPath);
        rw.RwObj(ref this.EventBfPathLength);
        rw.RwObj(ref this.EmbedBfFileOfs);
        rw.RwObj(ref this.EmbedBfFileSize);

        this.MarkerFrameCount = (this.FileHeaderSize - (int)rw.RelativeTell()) / 4;
        if (rw.IsConstructlike())
            this.MarkerFrame = new Int32[this.MarkerFrameCount];
        rw.RwInt32s(ref this.MarkerFrame, this.MarkerFrameCount);

        this.ObjectOffset.Validate((int)rw.RelativeTell(), rw.IsParselike());
        if (rw.IsConstructlike())
            this.Objects = Enumerable.Range(0, this.ObjectCount.Value).Select(i => new SerialObject()).ToArray();
        this.ObjectCount.Validate(this.Objects.Length, rw.IsParselike());
        rw.RwObjs(ref this.Objects, this.ObjectCount.Value);

        this.CommandOffset.Validate((int)rw.RelativeTell(), rw.IsParselike());
        if (rw.IsConstructlike())
            this.Commands = Enumerable.Range(0, this.CommandCount.Value).Select(i => new SerialCommand()).ToArray();
        this.CommandCount.Validate(this.Commands.Length, rw.IsParselike());
        rw.RwObjs(ref this.Commands, this.CommandCount.Value);

        if (rw.IsConstructlike())
           this.CommandData = new ArrayList();
        for (var i=0; i<this.CommandCount.Value; i++)
        {
            if (rw.IsConstructlike())
            {
                Type commandType = typeof(CommandTypes).GetNestedType(this.Commands[i].CommandCode);
                if (commandType == null)
                    this.CommandData.Add(new SerialCommandData());
                else
                    this.CommandData.Add(Activator.CreateInstance(commandType));
            }
            this.Commands[i].DataOffset.Validate((int)rw.RelativeTell(), rw.IsParselike());
            if (this.Commands[i].DataOffset.Value != (int)rw.RelativeTell())
                rw.RelativeSeek(this.Commands[i].DataOffset.Value, 0);
            rw.RwObj((ISerializable)this.CommandData[i], new Dictionary<string, object>()
                { ["dataSize"] = this.Commands[i].DataSize.Value });
            this.Commands[i].DataSize.Validate((int)rw.RelativeTell() - this.Commands[i].DataOffset.Value, rw.IsParselike());
            if (this.Commands[i].DataSize.Value != (int)rw.RelativeTell())
                rw.RelativeSeek(this.Commands[i].DataOffset.Value + this.Commands[i].DataSize.Value, 0);
        }

        if (this.Flags[12])
        {
            this.PointerToEventBmdPath.Validate((int)rw.RelativeTell(), rw.IsParselike());
            this.EventBmdPathLength.Validate(48, rw.IsParselike());
            if (rw.IsParselike())
                this.EventBmdPath = this.EventBmdPath.PadRight(48, '\0').Substring(0, 48);
            rw.RwString(ref this.EventBmdPath, this.EventBmdPathLength.Value, Encoding.ASCII);
        }
        else
        {
            this.PointerToEventBmdPath.Validate(0, rw.IsParselike());
            this.EventBmdPathLength.Validate(0, rw.IsParselike());
        }

        if (this.Flags[14])
        {
            this.PointerToEventBfPath.Validate((int)rw.RelativeTell(), rw.IsParselike());
            this.EventBfPathLength.Validate(48, rw.IsParselike());
            if (rw.IsParselike())
                this.EventBfPath = this.EventBfPath.PadRight(48, '\0').Substring(0, 48);
            rw.RwString(ref this.EventBfPath, this.EventBfPathLength.Value, Encoding.ASCII);
        }
        else
        {
            this.PointerToEventBfPath.Validate(0, rw.IsParselike());
            this.EventBfPathLength.Validate(0, rw.IsParselike());
        }

        this.FileSize.Validate((int)rw.RelativeTell(), rw.IsParselike());

        rw.AssertEOF();
    }

    public bool DeleteObject(SerialObject obj)
    {
        List<SerialObject> objList = new List<SerialObject>(this.Objects);
        if (!objList.Contains(obj))
            return false;

        objList.Remove(obj);
        this.Objects = objList.ToArray();

        this.ObjectCount.Validate(this.Objects.Length, true);
        return true;
    }

    public bool DeleteCommand(int index)
    {
        if (index < 0 || index >= this.CommandCount.Value || this.Commands.Length != this.CommandData.Count)
            return false;

        List<SerialCommand> cmdList = new List<SerialCommand>(this.Commands);
        cmdList.RemoveAt(index);
        this.Commands = cmdList.ToArray();
        this.CommandData.RemoveAt(index);

        this.CommandCount.Validate(this.Commands.Length, true);
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
        for (int i=1; i<=this.ObjectCount.Value+1; i++)
            if (!ids.Contains(i))
            {
                newObj.Id = i;
                break;
            }

        List<SerialObject> objList = new List<SerialObject>(this.Objects);
        objList.Add(newObj);
        this.Objects = objList.ToArray();

        this.ObjectCount.Validate(this.Objects.Length, true);
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
        for (int i=1; i<=this.ObjectCount.Value+1; i++)
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

        this.ObjectCount.Validate(this.Objects.Length, true);
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

        this.CommandCount.Validate(this.Commands.Length, true);
        return this.CommandCount.Value-1;
    }

    public int NewCommand(string commandCode, int frameStart)
    {
        Type commandType = typeof(CommandTypes).GetNestedType(commandCode);
        if (commandType == null)
        {
            Trace.TraceWarning($"Unsupported new command type: {commandCode}");
            return -1;
        }

        SerialCommand newCmd = new SerialCommand();
        newCmd.CommandCode = commandCode;
        newCmd.FrameStart = frameStart;
        // TODO: maybe null check here
        newCmd.DataSize.Value = (int)commandType.GetField("DataSize").GetRawConstantValue();

        List<SerialCommand> cmdList = new List<SerialCommand>(this.Commands);
        cmdList.Add(newCmd);
        this.Commands = cmdList.ToArray();
        this.CommandData.Add(Activator.CreateInstance(commandType));

        this.CommandCount.Validate(this.Commands.Length, true);
        return this.CommandCount.Value-1;
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
    public ConstUInt32 UNUSED_UINT32 = new ConstUInt32(0);

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

        rw.RwObj(ref this.UNUSED_UINT32);
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

    public PositionalInt32 DataOffset = new PositionalInt32();
    public PositionalInt32 DataSize = new PositionalInt32();

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
        rw.RwObj(ref this.DataOffset);
        rw.RwObj(ref this.DataSize);
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
