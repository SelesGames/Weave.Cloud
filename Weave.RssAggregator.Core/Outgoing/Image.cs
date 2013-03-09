using ProtoBuf;
using System.Collections.Generic;

namespace Weave.RssAggregator.Core.DTOs.Outgoing
{
    [ProtoContract]
    public class Image
    {
        [ProtoMember(1)] public int Width { get; set; }
        [ProtoMember(2)] public int Height { get; set; }
        [ProtoMember(3)] public string OriginalUrl { get; set; }
        [ProtoMember(3)] public string BaseImageUrl { get; set; }
        [ProtoMember(4)] public string SupportedFormats { get; set; }
    }
}
