
namespace SelesGames.Common
{
    public static class ConvertExtension
    {
        public static TOutput Convert<TInput, TOutput>(this TInput o, IConverter<TInput, TOutput> converter)
        {
            return converter.Convert(o);
        }
    }
}
