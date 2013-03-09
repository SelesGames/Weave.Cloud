
namespace SelesGames.Common
{
    public interface IConverter<TInput, TOutput>
    {
        TOutput Convert(TInput input);
    }
}
