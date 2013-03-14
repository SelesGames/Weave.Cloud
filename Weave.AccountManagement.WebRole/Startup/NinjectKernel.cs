using Common.Azure;
using Newtonsoft.Json;
using Ninject;
using Weave.AccountManagement.DTOs;

namespace Weave.AccountManagement.WebRole.Startup
{
    public class NinjectKernel : StandardKernel
    {
        protected override void AddComponents()
        {
            base.AddComponents();

            var blobClient = new AzureJsonDotNetBlobClient<UserInfo>(
                "weave",
                "uudFrra70qkI64bifaI2Rrm37CZ1HkzaBQrLMyw6U/hmzNDZehXeo9DdUv7BCpuZY4N2q/CNpNwYxW2fa218xA==",
                "userinfo",
                false) { SerializerSettings = new JsonSerializerSettings { Formatting = Formatting.Indented }, UseGzipOnUpload = true };

            var userManager = new UserManager(blobClient);

            Bind<UserManager>().ToConstant(userManager);
        }
    }
}