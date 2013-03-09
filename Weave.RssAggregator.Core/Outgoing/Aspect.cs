using ProtoBuf;

namespace Weave.RssAggregator.Core.DTOs.Outgoing
{
    [ProtoContract]
    public enum Aspect
    {
        [ProtoEnum(Name="L", Value=0)] Landscape,
        [ProtoEnum(Name="P", Value=1)] Portrait,
        [ProtoEnum(Name="S", Value=2)] Square
    }
}
