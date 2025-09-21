using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using DeepCopy;

using Serialization;

namespace EVTUI;

public class ECS : ISerializable
{
    public static HashSet<string> ValidEcsCommands = new HashSet<string> {"SBEA", "SBE_", "SFts", "Snd_"};

    public bool    IsLittleEndian = false;
    private UInt32 TestOffset;

    public PositionalInt32  CommandCount   = new PositionalInt32();
    public ConstUInt32      CommandOffset  = new ConstUInt32(16);
    public ConstUInt32      CommandSize    = new ConstUInt32(48);
    public ConstUInt32      Reserve        = new ConstUInt32(0);

    public SerialCommand[] Commands    = new SerialCommand[0];
    public ArrayList       CommandData = new ArrayList();

    public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
    {
        if (rw.IsConstructlike())
            Trace.TraceInformation("Reading ECS file");
        else if (rw.IsParselike())
            Trace.TraceInformation("Writing ECS file");

        if (rw.IsConstructlike())
        {
            this.IsLittleEndian = false;
            rw.SetLittleEndian(this.IsLittleEndian);
            rw.RwObj(ref this.CommandCount);
            rw.RwUInt32(ref this.TestOffset);
            if (this.TestOffset != 16)
            {
                this.IsLittleEndian = true;
                rw.SetLittleEndian(this.IsLittleEndian);
            }
            rw.Seek(0, 0);
        }
        else if (rw.IsParselike())
            rw.SetLittleEndian(this.IsLittleEndian);

        this.CommandCount.Validate(this.Commands.Length, rw.IsParselike());
        rw.RwObj(ref this.CommandCount);

        rw.RwObj(ref this.CommandOffset);
        rw.RwObj(ref this.CommandSize);
        rw.RwObj(ref this.Reserve);

        if (rw.IsConstructlike())
            this.Commands = Enumerable.Range(0, this.CommandCount.Value).Select(i => new SerialCommand()).ToArray();
        rw.RwObjs(ref this.Commands, this.CommandCount.Value);

        if (rw.IsConstructlike())
           this.CommandData = new ArrayList();
        for (var i=0; i<this.CommandCount.Value; i++) {
            if (rw.IsConstructlike())
            {
                Type commandType = typeof(CommandTypes).GetNestedType(this.Commands[i].CommandCode);
                if (commandType == null)
                    this.CommandData.Add(new SerialCommandData());
                else
                    this.CommandData.Add(Activator.CreateInstance(commandType));
            }
            this.Commands[i].DataOffset.Validate((int)rw.RelativeTell(), rw.IsParselike());
            rw.RwObj((ISerializable)this.CommandData[i], new Dictionary<string, object>()
                { ["dataSize"]  = this.Commands[i].DataSize });
        }

        rw.AssertEOF();
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
        newCmd.DataSize = (int)commandType.GetField("DataSize").GetRawConstantValue();

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
