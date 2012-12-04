using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Weave.RssAggregator.Parsing
{
    public class Feed
    {
        Stream stream;
        const double DEFAULT_FAULT_TOLERANCE = 0.3;         // by default we will accept under 30% fault rate while parsing articles
        
        public string Name { get; set; }
        public string Description { get; set; }
        public IEnumerable<Entry> News { get; set; }
        public IEnumerable<Exception> Faults { get; set; }
        public double FaultTolerance { get; set; }
        
        public Feed(Stream stream)
        {
            this.stream = stream;
            FaultTolerance = DEFAULT_FAULT_TOLERANCE;
        }

        public void Load()
        {
            var entryIntermediates = stream.ToRssIntermediates().ToList();
            foreach (var entry in entryIntermediates)
                entry.ParseEntry();

            var total = entryIntermediates.Count;
            var faults = entryIntermediates.Select(o => o.ParseException).OfType<Exception>().ToList();
            var faultRate = faults.Count / (double)total;

            Faults = faults;
            if (faultRate < FaultTolerance)
                throw new AggregateException(Faults);
            else
                News = entryIntermediates.Select(o => o.Entry).OfType<Entry>().ToList();
        }
    }
}
