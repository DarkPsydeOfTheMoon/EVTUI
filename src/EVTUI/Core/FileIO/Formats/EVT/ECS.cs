using System;
using System.Collections;
using System.Collections.Generic;

using Serialization;

namespace EVTUI;

public class ECS : ISerializable
{
    private static Int32 ENTRY_OFFSET = 16;
    private static Int32 RESERVE      = 0;

    public string Endianness;
    public Int32 CommandCount;
    public UInt32 CommandOffset;
    public UInt32 CommandSize;
    public UInt32 Reserve;
    public SerialCommand[] Commands;
    public ArrayList CommandData;

    public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
    {
        if (rw.IsConstructlike())
        {
            this.Endianness = "big";
            rw.SetEndianness(this.Endianness);
            rw.RwInt32(ref this.CommandCount);
            rw.RwUInt32(ref this.CommandOffset);
            rw.RwUInt32(ref this.CommandSize);
            if (this.CommandSize != EVT.ENTRY_SIZE)
            {
                this.Endianness = "little";
                rw.SetEndianness(this.Endianness);
            }
            rw.Seek(0, 0);
        }
        else if (rw.IsParselike())
            rw.SetEndianness(this.Endianness);

        rw.RwInt32(ref this.CommandCount);

        rw.RwUInt32(ref this.CommandOffset);
        if (this.CommandOffset != ECS.ENTRY_OFFSET)
            throw new Exception($"ECS command offset ({this.CommandOffset}) does not match expected offset ({ECS.ENTRY_OFFSET})");

        rw.RwUInt32(ref this.CommandSize);
        if (this.CommandSize != EVT.ENTRY_SIZE)
            throw new Exception($"ECS command size ({this.CommandSize}) does not match expected offset ({EVT.ENTRY_SIZE})");

        rw.RwUInt32(ref this.Reserve);
        if (this.Reserve != ECS.RESERVE)
            throw new Exception($"ECS reserve ({this.Reserve}) does not match expected value ({ECS.RESERVE})");

        if (rw.Tell() != this.CommandOffset)
            throw new Exception($"Stream position should be at end of ECS header ({this.CommandOffset}) but is instead at position {rw.Tell()}");

        rw.RwObjs(ref this.Commands, this.CommandCount);
        if (this.Commands.Length != this.CommandCount)
            throw new Exception($"Number of commands ({this.Commands.Length}) doesn't match expected CommandCount ({this.CommandCount})");

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
