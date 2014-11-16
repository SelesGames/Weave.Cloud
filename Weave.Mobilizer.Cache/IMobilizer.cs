using System.Threading.Tasks;
using Weave.Services.Mobilizer.DTOs;

namespace Weave.Mobilizer.Cache
{
    public interface IMobilizer
    {
        Task<MobilizerResult> Mobilize(string url);
    }
}
