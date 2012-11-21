using ProtoBuf;

namespace Weave.RssAggregator.Core.DTOs.Incoming
{
    [ProtoContract]
    public class FeedRequest
    {
        [ProtoMember(1)] public string Id { get; set; }
        [ProtoMember(2)] public string Url { get; set; }
        [ProtoMember(3)] public string MostRecentNewsItemPubDate { get; set; }  // optional to get all
        [ProtoMember(4)] public string Etag { get; set; }  //optional
        [ProtoMember(5)] public string LastModified { get; set; }  //optional

        public override string ToString()
        {
            return string.Format("Id:{0}  MostRecent:{1}  Etag:{2}  LastModified:{3}  Url:{4}",
                Id,
                MostRecentNewsItemPubDate ?? "{null}",
                Etag ?? "{null}",
                LastModified ?? "{null}",
                Url ?? "{null}");
        }
    }
}