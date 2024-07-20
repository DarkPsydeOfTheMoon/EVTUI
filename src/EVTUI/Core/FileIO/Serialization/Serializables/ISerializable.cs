using System.Collections.Generic;

namespace Serialization
{
    public interface ISerializable
    {
        // added in an optional generic object dictionary for argument-passing lol... lame, but it works
        public void ExbipHook<T>(T rw, Dictionary<string, object> args = null)  where T : struct, IBaseBinaryTarget;
    }
}
