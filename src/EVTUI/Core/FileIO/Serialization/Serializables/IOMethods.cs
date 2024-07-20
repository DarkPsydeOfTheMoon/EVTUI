using System.IO;

namespace Serialization
{
    static class TraitMethods
    {
        // This method is supposed to be implemented on a WriteableTrait class
        // that can be used as a mix-in to any ISerializable.
        public static void Write<T>(T obj, string filepath) where T : ISerializable
        {
            using (var stream = File.Open(filepath, FileMode.Create))
            {
                using (var w = new BinaryWriter(stream))
                {
                    Writer writer = new Writer(w);
                    writer.RwObj(obj);
                }
            }
        }

        // This is supposed to be implemented on a ReadableTrait class
        // that can be used as a mix-in to any ISerializable.
        public static void Read<T>(T obj, string filepath) where T : ISerializable
        {
            using (var stream = File.Open(filepath, FileMode.Open))
            {
                using (var r = new BinaryReader(stream))
                {
                    Reader reader = new Reader(r);
                    reader.RwObj(obj);
                }
            }
        }

        public static byte[] ToBytes<T>(T obj) where T : ISerializable
        {
            using (var stream = new MemoryStream())
            {
                using (var w = new BinaryWriter(stream))
                {
                    Writer writer = new Writer(w);
                    writer.RwObj(obj);
                    stream.Seek(0, 0);
                    return stream.ToArray();
                }
            }
        }

        public static void FromBytes<T>(T obj, byte[] bytes) where T : ISerializable
        {
            using (var stream = new MemoryStream(bytes))
            {
                using (var r = new BinaryReader(stream))
                {
                    Reader reader = new Reader(r);
                    reader.RwObj(obj);
                }
            }
        }
    }
}
