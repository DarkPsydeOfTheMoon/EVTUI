using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using DeepCopy;

using Serialization;

namespace EVTUI;

public class ECS : ISerializable
{
    private static Int32 ENTRY_OFFSET = 16;
    private static Int32 RESERVE      = 0;

    public static HashSet<string> ValidEcsCommands = new HashSet<string> {"SBEA", "SBE_", "SFts", "Snd_"};

    public bool   IsLittleEndian;
    public Int32  CommandCount;
    public UInt32 CommandOffset;
    public UInt32 CommandSize;
    public UInt32 Reserve = 0;

    public SerialCommand[] Commands;
    public ArrayList       CommandData;

    public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
    {
        if (rw.IsConstructlike())
        {
            this.IsLittleEndian = false;
            rw.SetLittleEndian(this.IsLittleEndian);
            rw.RwInt32(ref this.CommandCount);
            rw.RwUInt32(ref this.CommandOffset);
            rw.RwUInt32(ref this.CommandSize);
            if (this.CommandSize != EVT.ENTRY_SIZE)
            {
                this.IsLittleEndian = true;
                rw.SetLittleEndian(this.IsLittleEndian);
            }
            rw.Seek(0, 0);
        }
        else if (rw.IsParselike())
            rw.SetLittleEndian(this.IsLittleEndian);

        if (rw.IsParselike())
            this.CommandCount = this.Commands.Length;
        rw.RwInt32(ref this.CommandCount);

        rw.RwUInt32(ref this.CommandOffset);
        Trace.Assert(this.CommandOffset == ECS.ENTRY_OFFSET, $"ECS command offset ({this.CommandOffset}) does not match expected offset ({ECS.ENTRY_OFFSET})");

        rw.RwUInt32(ref this.CommandSize);
        Trace.Assert(this.CommandSize == EVT.ENTRY_SIZE, $"ECS command size ({this.CommandSize}) does not match expected offset ({EVT.ENTRY_SIZE})");

        rw.RwUInt32(ref this.Reserve);
        Trace.Assert(this.Reserve == ECS.RESERVE, $"ECS reserve ({this.Reserve}) does not match expected value ({ECS.RESERVE})");

        if (rw.IsParselike())
            this.CommandOffset = (uint)rw.RelativeTell();
        Trace.Assert(this.CommandOffset == rw.RelativeTell(), $"Stream position should be at end of ECS header ({this.CommandOffset}) but is instead at position {rw.RelativeTell()}");

        rw.RwObjs(ref this.Commands, this.CommandCount);
        Trace.Assert(this.Commands.Length == this.CommandCount, $"Number of commands ({this.Commands.Length}) doesn't match expected CommandCount ({this.CommandCount})");

        if (rw.IsConstructlike())
           this.CommandData = new ArrayList();
        for (var i=0; i<this.CommandCount; i++) {
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
                { ["dataSize"]  = this.Commands[i].DataSize });
        }

        rw.AssertEOF();
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

    // TODO: AddObject

    //public int CopyCommandToNewFrame(int index, int frame)
    public int CopyCommandToNewFrame(SerialCommand cmd, dynamic cmdData, int frame)
    {
        //if (index < 0 || index >= this.CommandCount)
        //    return -1;

        //SerialCommand newCmd = DeepCopier.Copy(this.Commands[index]);
        SerialCommand newCmd = DeepCopier.Copy(cmd);
        newCmd.FrameStart = frame;

        List<SerialCommand> cmdList = new List<SerialCommand>(this.Commands);
        cmdList.Add(newCmd);
        this.Commands = cmdList.ToArray();

        //this.CommandData.Add(DeepCopier.Copy(this.CommandData[index]));
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
