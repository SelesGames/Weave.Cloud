using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.IO;

namespace Common.Azure.Blob
{
    public class BlobContent : IDisposable
    {
        private BlobContent() { }

        public Stream Content { get; private set; }
        public BlobProperties Properties { get; private set; }

        public static BlobContent Create(ICloudBlob blob, Stream content)
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