
namespace System
{
    public static class StringExtensions
    {
        public static bool IsWellFormedUriString(this string url)
        {
            Uri uri;
            return Uri.TryCreate(url, UriKind.Absolute, out uri);
        }
    }
}