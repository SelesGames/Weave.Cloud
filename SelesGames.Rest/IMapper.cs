
namespace SelesGames.Rest
{
    public interface IMapper<TInput, TOutput>
    {
        TOutput Map(TInput input);
    }
}
