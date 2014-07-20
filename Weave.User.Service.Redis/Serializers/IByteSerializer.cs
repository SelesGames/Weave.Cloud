
namespace Weave.User.Service.Redis.Serializers
{
    interface IByteSerializer
    {
        T ReadObject<T>(byte[] byteArray);
        byte[] WriteObject<T>(T obj);
    }
}
