using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using Serialization;

namespace EVTUI;

public class ECS : ISerializable
{
    private static Int32 ENTRY_OFFSET = 16;
    private static Int32 RESERVE      = 0;

    public bool   IsLittleEndian;
    public Int32  CommandCount;
    public UInt32 CommandOffset;
    public UInt32 CommandSize;
    public UInt32 Reserve;

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

        rw.RwInt32(ref this.CommandCount);

        rw.RwUInt32(ref this.CommandOffset);
        Trace.Assert(this.CommandOffset == ECS.ENTRY_OFFSET, $"ECS command offset ({this.CommandOffset}) does not match expected offset ({ECS.ENTRY_OFFSET})");

        rw.RwUInt32(ref this.CommandSize);
        Trace.Assert(this.CommandSize == EVT.ENTRY_SIZE, $"ECS command size ({this.CommandSize}) does not match expected offset ({EVT.ENTRY_SIZE})");

        rw.RwUInt32(ref this.Reserve);
        Trace.Assert(this.Reserve == ECS.RESERVE, $"ECS reserve ({this.Reserve}) does not match expected value ({ECS.RESERVE})");

        Trace.Assert(rw.Tell() == this.CommandOffset, $"Stream position should be at end of ECS header ({this.CommandOffset}) but is instead at position {rw.Tell()}");

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
            rw.RwObj((ISerializable)this.CommandData[i], new Dictionary<string, object>()
                { ["dataSize"]  = this.Commands[i].DataSize });
            // leaving this in temporarily just to show that parsing is happening
            Console.WriteLine(this.CommandData[i].GetType().ToString());
        }

        rw.AssertEOF();
    }

    public void Write(string filepath) { TraitMethods.Write(this, filepath); }
    public void Read (string filepath) { TraitMethods.Read (this, filepath); }
}
