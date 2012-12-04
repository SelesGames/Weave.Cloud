using System;

namespace Weave.RssAggregator.Parsing
{
    public abstract class EntryIntermediate
    {
        public Entry Entry { get; private set; }
        public DateTime? PublicationDate { get; private set; }
        public Exception ParseException { get; private set; }

        public void ParsePublicationDate()
        {
            DateTime? dt = null;
            try
            {
                dt = GetPublicationDate();
            }
            catch { }
            PublicationDate = dt;
        }

        public void ParseEntry()
        {
            try
            {
                Entry = ParseInternal();
            }
            catch (Exception e)
            {
                ParseException = e;
            }
        }

        protected abstract Entry ParseInternal();
        protected abstract DateTime? GetPublicationDate();
    }
}
