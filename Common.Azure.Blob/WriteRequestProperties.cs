
namespace Common.Azure.Blob
{
    public class WriteRequestProperties : RequestProperties
    {
        public bool UseCompression { get; set; }

        public string CacheControl { get; set; }
        public string ContentDisposition { get; set; }
        //public string ContentEncoding { get; set; }
        public string ContentLanguage { get; set; }
        public string ContentMD5 { get; set; }
        public string ContentType { get; set; }
    }
}
