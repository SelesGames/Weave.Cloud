using System;
using System.Threading.Tasks;

namespace Weave.User.BusinessObjects.v2.Repositories
{
    public interface IUserInfoRepository
    {
        Task<MasterNewsItemCollection> GetAllNews(Guid id);
        Task<NewsItemStateCache> GetNewsItemStateCache(Guid id);
        Task<UserInfo> GetUser(Guid id);
        Task Save(MasterNewsItemCollection allNews);
        Task Save(NewsItemStateCache newsState);
        Task Save(UserInfo user);
    }
}