using Ninject;
using System.Collections.Generic;
using System.Drawing;

namespace ImageResizer.Role.Startup
{
    public class NinjectKernel : StandardKernel
    {
        protected override void AddComponents()
        {
            base.AddComponents();

            var settings = new Settings
            {
                AzureStorageAccountName = "weave",
                AzureStorageKey = "uudFrra70qkI64bifaI2Rrm37CZ1HkzaBQrLMyw6U/hmzNDZehXeo9DdUv7BCpuZY4N2q/CNpNwYxW2fa218xA==",
                BlobImageContainer = "images",
                OutputContentType = "image/jpeg",
                OutputFileExtension = "jpg",
                ImageQuality = 90L,
                OutputSizes = new List<OutputSize>
                {
                    OutputSize.Create("sd", 691, 390),
                    OutputSize.Create("small", 120, 120),
                },
            };

            Bind<Settings>().ToConstant(settings);
        }
    }
}
