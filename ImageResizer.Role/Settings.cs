using System.Collections.Generic;
using System.Drawing;

namespace ImageResizer.Role
{
    public class Settings
    {
        public string AzureStorageAccountName { get; set; }
        public string AzureStorageKey { get; set; }
        public string BlobImageContainer { get; set; }
        public List<OutputSize> OutputSizes { get; set; }
        public string OutputContentType { get; set; }
        public string OutputFileExtension { get; set; }
    }

    public class OutputSize
    {
        public string AppendString { get; set; }
        public Size Size { get; set; }

        public static OutputSize Create(string appendString, int width, int height)
        {
            return new OutputSize
            {
                AppendString = appendString,
                Size = new Size(width, height),
            };
        }
    }
}
