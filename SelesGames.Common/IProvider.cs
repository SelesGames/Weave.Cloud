
namespace SelesGames.Common
{
    public interface IProvider<out T>
    {
        T Get();
    }
}
