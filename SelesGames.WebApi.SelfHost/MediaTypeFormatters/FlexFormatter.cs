using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SelesGames.WebApi.MediaTypeFormatters
{
    /// <summary>
    /// A class to handle unrecognized/mangled/missing Content-Type values for POSTS/PUTS.  Only ever
    /// set this as the last formatter in the sequence
    /// </summary>
    public class FlexFormatter : MediaTypeFormatter
    {
        IEnumerable<MediaTypeFormatter> formatters;

        public FlexFormatter(IEnumerable<MediaTypeFormatter> formatters)
        {
            if (formatters == null || !formatters.Any())
                throw new ArgumentException("formatters");

            this.formatters = formatters;

            foreach (var mediaType in formatters.SelectMany(o => o.SupportedMediaTypes))
                SupportedMediaTypes.Add(mediaType);
        }

        public override bool CanReadType(Type type)
        {
            return formatters.Any(o => o.CanReadType(type));
        }

        public override bool CanWriteType(Type type)
        {
            return formatters.Any(o => o.CanWriteType(type));
        }

        public async override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
        {
            foreach (var formatter in formatters)
            {
                var readAttempt = await TryReadFromStreamAsync(formatter, type, readStream, content, formatterLogger);
                if (readAttempt.Item1)
                    return readAttempt.Item2;
            }

            throw new Exception("no formatter can handle the given type or Content-Type");
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext)
        {
            throw new NotImplementedException();
        }

        async Task<Tuple<bool, object>> TryReadFromStreamAsync(MediaTypeFormatter formatter, Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
        {
            try
            {
                var obj = await formatter.ReadFromStreamAsync(type, readStream, content, formatterLogger);
                return Tuple.Create(true, obj);
            }
            catch
            {
                return Tuple.Create(false, default(object));
            }
        }
    }
}
