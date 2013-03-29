using System.Runtime.Serialization;

namespace Weave.User.DataStore
{
    [DataContract]
    public class Image
    {
        [DataMember(Order=1)]   public int Width { get; set; }
        [DataMember(Order=2)]   public int Height { get; set; }
        [DataMember(Order=3)]   public string OriginalUrl { get; set; }
        [DataMember(Order=4)]   public string BaseImageUrl { get; set; }
        [DataMember(Order=5)]   public string SupportedFormats { get; set; }
    }
}
