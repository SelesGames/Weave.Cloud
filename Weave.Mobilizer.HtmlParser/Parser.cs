using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weave.Mobilizer.Client;

namespace Weave.Mobilizer.HtmlParser
{
    public class Parser
    {
        string url = "http://www.wpcentral.com/e-online-releases-official-app-windows-phone-8";

        public async void TryStuff()
        {
            var dto = await new MobilizerServiceClient("hxyuiplkx78!ksdfl").Get(url);
            var html = dto.content;

            var doc = new HtmlDocument();
            doc.OptionDefaultStreamEncoding = Encoding.UTF8;
            doc.Load(result);
        }
    }
}
