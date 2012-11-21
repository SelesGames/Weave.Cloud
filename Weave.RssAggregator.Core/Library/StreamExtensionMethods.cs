
namespace System.IO
{
    public static class StreamExtensionMethods
    {
        public static MemoryStream ToMemoryStream(this Stream inputStream, int readSize = 256)
        {
            //var sw = System.Diagnostics.Stopwatch.StartNew();
            //const int readSize = 256;
            byte[] buffer = new byte[readSize];
            MemoryStream ms = new MemoryStream();

            int count = inputStream.Read(buffer, 0, readSize);
            while (count > 0)
            {
                ms.Write(buffer, 0, count);
                count = inputStream.Read(buffer, 0, readSize);
            }
            ms.Position = 0;

            //sw.Stop();
            //DebugEx.WriteLine("SERVER SIDE Took {0} ms to copy memory stream", sw.ElapsedMilliseconds);
            return ms;
        }
    }
}
