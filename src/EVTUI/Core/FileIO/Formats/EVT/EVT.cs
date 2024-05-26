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
        private string MAGIC = "EVT";
        private int ROYAL_VERSION = 0x00029B9C;
        private Int32 ENTRY_SIZE = 0x30;

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

        public void ExbipHook<T>(T rw) where T : struct, IBaseBinaryTarget
        {
            rw.SetEndianness("little");
            rw.RwString(ref this.Magic, 3, Encoding.ASCII);
            if (this.Magic != this.MAGIC)
                throw new Exception($"Magic string ({this.Magic}) doesn't match expected string ({this.MAGIC})");
            rw.RwUInt8(ref this.Endianness);
            if (this.Endianness == 1)
                rw.SetEndianness("big");

            rw.RwUInt32(ref this.Version);
            rw.RwInt16(ref this.MajorId);
            rw.RwInt16(ref this.MinorId);

            // leaving this in temporarily just to show that parsing is happening
            Console.WriteLine($"{this.MajorId} {this.MinorId}");

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
            if (this.ObjectSize != this.ENTRY_SIZE)
                throw new Exception($"ObjectSize ({this.ObjectSize}) does not match expected entry size ({this.ENTRY_SIZE})");

            rw.RwInt32(ref this.DUMMY_INT32[0]);

            rw.RwInt32(ref this.CommandCount);
            rw.RwInt32(ref this.CommandOffset);
            rw.RwInt32(ref this.CommandSize);
            if (this.CommandSize != this.ENTRY_SIZE)
                throw new Exception($"CommandSize ({this.CommandSize}) does not match expected entry size ({this.ENTRY_SIZE})");

            rw.RwInt32(ref this.DUMMY_INT32[1]);

            rw.RwInt32(ref this.PointerToEventBmdPath);
            rw.RwInt32(ref this.EventBmdPathLength);
            rw.RwInt32(ref this.EmbedMsgFileOfs);
            rw.RwInt32(ref this.EmbedMsgFileSize);

            rw.RwInt32(ref this.PointerToEventBfPath);
            rw.RwInt32(ref this.EventBfPathLength);
            rw.RwInt32(ref this.EmbedBfFileOfs);
            rw.RwInt32(ref this.EmbedBfFileSize);

            if (rw.IsConstructlike()) {
                this.MarkerFrameCount = 8;
                if (this.Version == this.ROYAL_VERSION)
                    this.MarkerFrameCount = 48;
                this.MarkerFrame = new Int32[this.MarkerFrameCount];
            }
            rw.RwInt32s(ref this.MarkerFrame, this.MarkerFrameCount);

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
                    Type commandType = typeof(CommandTypes).GetNestedType(this.Commands[i].CommandCode);
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

        public void ExbipHook<T>(T rw) where T : struct, IBaseBinaryTarget
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

        public void Write(string filepath) { TraitMethods.Write(this, filepath); }
        public void Read (string filepath) { TraitMethods.Read (this, filepath); }
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

        public void ExbipHook<T>(T rw) where T : struct, IBaseBinaryTarget
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

}
