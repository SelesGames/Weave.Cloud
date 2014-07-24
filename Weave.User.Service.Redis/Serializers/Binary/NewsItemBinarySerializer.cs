using System.Linq;
using Weave.User.Service.Redis.DTOs;

namespace Weave.User.Service.Redis.Serializers.Binary
{
    class NewsItemBinarySerializer : IByteSerializer
    {
        public byte[] WriteNewsItem(NewsItem newsItem)
        {
            using (var helper = new NewsItemWriter(newsItem))
            {
                helper.Write();
                return helper.GetBytes();
            }
        }

        public NewsItem ReadNewsItem(byte[] byteArray)
        {
            using (var helper = new NewsItemReader(byteArray))
            {
                helper.Read();
                return helper.GetNewsItem();
            }
        }

        public T ReadObject<T>(byte[] byteArray)
        {
            return new[] { ReadNewsItem(byteArray) }.Cast<T>().First();
        }

        public byte[] WriteObject<T>(T obj)
        {
            return WriteNewsItem(new[] { obj }.Cast<NewsItem>().First());
        }
    }
}