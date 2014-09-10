
namespace Weave.User.Service.Redis
{
    public static class Timings
    {
        public static bool AreEnabled { get; set; }

        static Timings()
        {
            AreEnabled = true;
        }
    }
}
