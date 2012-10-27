using System;
using System.IO;
using System.Net;
using System.ServiceModel.Web;

namespace Weave.Mobilizer.Core
{
    public class MobilizerService
    {
        public static void Initialize(string ipAddress, Func<InstapaperMobilizerFormatter> creator)
        {
            WebServiceHost clientPolicyHost = new WebServiceHost(creator(), new Uri(ipAddress));
            clientPolicyHost.Open();
        }
    }


    public static class WebRequestExtensionMethods
    {
        public static string GetReadStream(this Stream stream)
        {
            try
            {
                using (var reader = new StreamReader(stream))
                {
                    string result = reader.ReadToEnd();
                    reader.Close();
                    return result;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
