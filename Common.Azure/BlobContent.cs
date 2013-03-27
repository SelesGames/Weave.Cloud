using Microsoft.WindowsAzure.StorageClient;
using System;
using System.IO;

namespace Common.Azure
{
    public class BlobContent : IDisposable
    {
        private BlobContent() { }

        public Stream Content { get; private set; }
        public BlobProperties Properties { get; private set; }

        public static BlobContent Create(CloudBlob blob, Stream content)
        {
            return new BlobContent
            {
                Content = content,
                Properties = blob.Properties
            };
        }

        public void Dispose()
        {
            if (Content != null)
                Content.Dispose();
        }
    }
}
