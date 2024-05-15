using System;
using System.Collections;
using System.Text;

using Serialization;

namespace EVTUI
{
    public static class Globals
    {
       public static Int32 LAST_DATA_SIZE {get;set;}
    }

    public class EVT : ISerializable
    {
        private Int32 VANILLA_MAGIC = 0x45565401;
        private Int32 ROYAL_MAGIC = 0x45565400;
        private Int32 VANILLA_FLAG = 0x90;
        private Int32 ROYAL_FLAG = 0x130;
        private Int32 ENTRY_SIZE = 0x30;

        public Int32 Magic;
        public UInt32 Version;
        public Int16 MajorId;
        public Int16 MinorId;
        public Int32 FileSize;
        public Int32 Flag;
        public Int32 Duration;
        public Int32 ObjectCount;
        public Int32 ObjectOffset;
        public Int32 ObjectSize;
        public Int32 CommandCount;
        public Int32 CommandOffset;
        public Int32 CommandSize;
        public Int32 BmdPathPointer;
        public Int32 BmdPathLength;
        public Int32 BfPathPointer;
        public Int32 BfPathLength;
        public string BmdPath;
        public string BfPath;

        public SerialObject[] Objects;
        public SerialCommand[] Commands;
        public ArrayList CommandData;

        public sbyte[] UNK_INT8 = new sbyte[4];
        public Int16[] UNK_INT16 = new Int16[2];
        public Int32[] UNK_INT32 = new Int32[57];
        public UInt32[] UNK_UINT32 = new UInt32[1];

        public void ExbipHook<T>(T rw) where T : struct, IBaseBinaryTarget
        {
            rw.SetEndianness("big");
            rw.RwInt32(ref this.Magic);
            if (this.Magic != this.VANILLA_MAGIC && this.Magic != this.ROYAL_MAGIC)
                throw new Exception($"Magic number ({this.Magic}) doesn't match P5 Vanilla ({this.VANILLA_MAGIC}) or Royal ({this.ROYAL_MAGIC})");
            rw.SetEndianness("little");

            rw.RwUInt32(ref this.Version);
            rw.RwInt16(ref this.MajorId);
            rw.RwInt16(ref this.MinorId);
            // leaving this in temporarily just to show that parsing is happening
            Console.WriteLine($"{this.MajorId} {this.MinorId}");

            rw.RwInt8(ref this.UNK_INT8[0]);
            rw.RwInt8(ref this.UNK_INT8[1]);
            rw.RwInt16(ref this.UNK_INT16[0]);

            rw.RwInt32(ref this.FileSize);
            rw.RwInt32(ref this.Flag);
            if (this.Flag != this.VANILLA_FLAG && this.Flag != this.ROYAL_FLAG)
                throw new Exception($"Flag ({this.Flag}) doesn't match P5 Vanilla (this.{VANILLA_FLAG}) or Royal ({this.ROYAL_FLAG})");

            rw.RwUInt32(ref this.UNK_UINT32[0]);

            rw.RwInt32(ref this.Duration);

            rw.RwInt8(ref this.UNK_INT8[2]);
            rw.RwInt8(ref this.UNK_INT8[3]);
            rw.RwInt16(ref this.UNK_INT16[1]);
            rw.RwInt32(ref this.UNK_INT32[0]);
            rw.RwInt32(ref this.UNK_INT32[1]);
            rw.RwInt32(ref this.UNK_INT32[2]);

            rw.RwInt32(ref this.ObjectCount);
            rw.RwInt32(ref this.ObjectOffset);
            rw.RwInt32(ref this.ObjectSize);
            if (this.ObjectSize != this.ENTRY_SIZE)
                throw new Exception($"ObjectSize ({this.ObjectSize}) does not match expected entry size ({this.ENTRY_SIZE})");

            rw.RwInt32(ref this.UNK_INT32[3]);

            rw.RwInt32(ref this.CommandCount);
            rw.RwInt32(ref this.CommandOffset);
            rw.RwInt32(ref this.CommandSize);
            if (this.CommandSize != this.ENTRY_SIZE)
                throw new Exception($"CommandSize ({this.CommandSize}) does not match expected entry size ({this.ENTRY_SIZE})");

            rw.RwInt32(ref this.UNK_INT32[4]);

            rw.RwInt32(ref this.BmdPathPointer);
            rw.RwInt32(ref this.BmdPathLength);

            rw.RwInt32(ref this.UNK_INT32[5]);
            rw.RwInt32(ref this.UNK_INT32[6]);

            rw.RwInt32(ref this.BfPathPointer);
            rw.RwInt32(ref this.BfPathLength);

            for (var i=7; i<17; i++)
                rw.RwInt32(ref this.UNK_INT32[i]);

            if (this.Flag == this.ROYAL_FLAG)
                for (var i=17; i<57; i++)
                    rw.RwInt32(ref this.UNK_INT32[i]);

            if (this.BmdPathPointer != 0) {
               rw.Seek(this.BmdPathPointer, 0);
               rw.RwString(ref this.BmdPath, this.BmdPathLength, Encoding.ASCII);
            }

            if (this.BfPathPointer != 0) {
               rw.Seek(this.BfPathPointer, 0);
               rw.RwString(ref this.BfPath, this.BfPathLength, Encoding.ASCII);
            }

            rw.Seek(this.ObjectOffset, 0);
            rw.RwObjs(ref this.Objects, this.ObjectCount);
            if (this.Objects.Length != this.ObjectCount)
                throw new Exception($"Number of objects ({this.Objects.Length}) doesn't match expected ObjectCount ({this.ObjectCount})");

            rw.Seek(this.CommandOffset, 0);
            rw.RwObjs(ref this.Commands, this.CommandCount);
            if (this.Commands.Length != this.CommandCount)
                throw new Exception($"Number of commands ({this.Commands.Length}) doesn't match expected CommandCount ({this.CommandCount})");

            if (rw.IsConstructlike())
               this.CommandData = new ArrayList();
            for (var i=0; i<this.CommandCount; i++) {
                if (rw.IsConstructlike())
                {
                    Type commandType = typeof(CommandTypes).GetNestedType(this.Commands[i].Type);
                    if (commandType == null)
                        this.CommandData.Add(new SerialCommandData());
                    else
                        this.CommandData.Add(Activator.CreateInstance(commandType));
                }
                Globals.LAST_DATA_SIZE = Commands[i].DataSize;
                rw.RwObj((ISerializable)this.CommandData[i]);
                // leaving this in temporarily just to show that parsing is happening
                Console.WriteLine(this.CommandData[i].GetType().ToString());
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
        public Int32 DuplicateObjectIndex;
        public Int32 ResourceMajorId;
        public Int16 ResourceSubId;
        public Int16 ResourceMinorId;
        public Int32 AnimationMajorId;
        public Int32 AnimationMinorId;
        public Int32 AnimationSubId;

        public Int32[] UNK_INT32 = new Int32[3];
        public UInt32[] UNK_UINT32 = new UInt32[1];

        public void ExbipHook<T>(T rw) where T : struct, IBaseBinaryTarget
        {
            rw.RwInt32(ref this.Id);
            rw.RwInt32(ref this.Type);

            rw.RwInt32(ref this.UNK_INT32[0]);

            rw.RwInt32(ref this.DuplicateObjectIndex);
            rw.RwInt32(ref this.ResourceMajorId);
            rw.RwInt16(ref this.ResourceSubId);
            rw.RwInt16(ref this.ResourceMinorId);

            rw.RwUInt32(ref this.UNK_UINT32[0]);

            rw.RwInt32(ref this.AnimationMajorId);
            rw.RwInt32(ref this.AnimationMinorId);
            rw.RwInt32(ref this.AnimationSubId);

            rw.RwInt32(ref this.UNK_INT32[1]);
            rw.RwInt32(ref this.UNK_INT32[2]);
        }

        public void Write(string filepath) { TraitMethods.Write(this, filepath); }
        public void Read (string filepath) { TraitMethods.Read (this, filepath); }
    }

    public class SerialCommand : ISerializable
    {
        public string Type;
        public Int32 ObjectId;
        public Int32 ForceSkipCommand;
        public Int32 Frame;
        public Int32 Duration;
        public Int32 DataOffset;
        public Int32 DataSize;
        public Int32 EvtFlagType;
        public UInt32 EvtFlagId;
        public Int32 EvtFlagValue;
        public Int32 EvtFlagConditionalType;

        public Int16[] UNK_INT16 = new Int16[2];

        public void ExbipHook<T>(T rw) where T : struct, IBaseBinaryTarget
        {
            rw.RwString(ref this.Type, 4, Encoding.ASCII);

            rw.RwInt16(ref this.UNK_INT16[0]);
            rw.RwInt16(ref this.UNK_INT16[1]);

            rw.RwInt32(ref this.ObjectId);
            rw.RwInt32(ref this.ForceSkipCommand);
            rw.RwInt32(ref this.Frame);
            rw.RwInt32(ref this.Duration);
            rw.RwInt32(ref this.DataOffset);
            rw.RwInt32(ref this.DataSize);
            rw.RwInt32(ref this.EvtFlagType);
            rw.RwUInt32(ref this.EvtFlagId);
            rw.RwInt32(ref this.EvtFlagValue);
            rw.RwInt32(ref this.EvtFlagConditionalType);
        }

        public void Write(string filepath) { TraitMethods.Write(this, filepath); }
        public void Read (string filepath) { TraitMethods.Read (this, filepath); }
    }

    public class SerialCommandData : ISerializable
    {
        public byte[] UNK;

        public void ExbipHook<T>(T rw) where T : struct, IBaseBinaryTarget
        {
            rw.RwBytestring(ref this.UNK, Globals.LAST_DATA_SIZE);
        }

        public void Write(string filepath) { TraitMethods.Write(this, filepath); }
        public void Read (string filepath) { TraitMethods.Read (this, filepath); }
    }

    public class CommandTypes
    {
        public class MAB_ : ISerializable
        {
            public Int32 PrimaryAnimationId;
            public Int32 LoopBool;
            public float AnimationSpeed;
            public Int32 SecondaryAnimationId;
            public byte AnimationMode;
            public Int32 FirstFrameInd;
            public Int32 LastFrameInd;

            public Int32[] UNK_INT32 = new Int32[8];
            public float[] UNK_FLOAT32 = new float[1];
            public sbyte[] UNK_INT8 = new sbyte[1];
            public Int16[] UNK_INT16 = new Int16[1];

            public void ExbipHook<T>(T rw) where T : struct, IBaseBinaryTarget
            {
                rw.RwInt32(ref this.PrimaryAnimationId);
                rw.RwInt32(ref this.UNK_INT32[0]);
                rw.RwInt32(ref this.LoopBool);
                rw.RwFloat32(ref this.AnimationSpeed);
                rw.RwInt32(ref this.SecondaryAnimationId);
                rw.RwInt32(ref this.UNK_INT32[1]);
                rw.RwInt32(ref this.UNK_INT32[2]);
                rw.RwFloat32(ref this.UNK_FLOAT32[0]);
                rw.RwUInt8(ref this.AnimationMode);
                rw.RwInt8(ref this.UNK_INT8[0]);
                rw.RwInt16(ref this.UNK_INT16[0]);
                rw.RwInt32(ref this.FirstFrameInd);
                rw.RwInt32(ref this.LastFrameInd);
                for (var i=3; i<8; i++)
                    rw.RwInt32(ref this.UNK_INT32[i]);
            }

            public void Write(string filepath) { TraitMethods.Write(this, filepath); }
            public void Read (string filepath) { TraitMethods.Read (this, filepath); }
        }
    }

}
