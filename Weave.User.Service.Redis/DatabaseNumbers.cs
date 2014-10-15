
namespace Weave.User.Service.Redis
{
    // redis maxes out to 16 databases by default, so don't index higher than 15
    public static class DatabaseNumbers
    {
        public const int USER_INDICES = 0;
        public const int FEED_UPDATER = 2;
        public const int CANONICAL_NEWSITEMS = 3;
        public const int LOCK = 4;
        public const int MESSAGE_QUEUE = 9;
    }
}