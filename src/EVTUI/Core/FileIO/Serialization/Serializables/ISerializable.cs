namespace Serialization
{
    public interface ISerializable
    {
        public void ExbipHook<T>(T rw)  where T : struct, IBaseBinaryTarget;
    }
}
