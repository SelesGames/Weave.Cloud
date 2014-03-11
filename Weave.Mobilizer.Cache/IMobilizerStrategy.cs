
namespace Weave.Mobilizer.Cache
{
    public interface IMobilizerStrategy
    {
        IMobilizer Select(string url);
    }
}
