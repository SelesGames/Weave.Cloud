using HtmlAgilityPack;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weave.Mobilizer.Client;
using Weave.Mobilizer.DTOs;

namespace Weave.Mobilizer.HtmlParser
{
    public class Parser
    {
        string mobilizerDetectedFirstImage;

        string testurl =
//"http://thenextweb.com/insider/2013/06/23/researchers-are-developing-a-system-to-securely-leak-information-by-surfing-the-web/?utm_source";
//"http://www.theverge.com/2013/6/21/4451172/xbox-engineer-disappointed-pastebin-note-rumored-features?utm_source";
//"http://news.cnet.com/8301-1009_3-57590599-83/whistle-blower-update-snowden-lands-in-moscow-wikileakers-gmail-searched/?part";
"http://www.guardian.co.uk/world/2013/jun/23/chen-guangcheng-visits-taiwan";

        public async Task TryStuff()
        {
            var dto = await new MobilizerServiceClient("hxyuiplkx78!ksdfl").Get(testurl);
            RemoveImageFromContentMatchingLead(dto);
            System.Diagnostics.Debug.WriteLine(dto);
        }

        public void RemoveImageFromContentMatchingLead(ReadabilityResult result)
        {
            var html = result.content;
            this.mobilizerDetectedFirstImage = result.lead_image_url;

            if (string.IsNullOrWhiteSpace(mobilizerDetectedFirstImage))
                return;

            var doc = new HtmlDocument();
            doc.OptionDefaultStreamEncoding = Encoding.UTF8;
            doc.LoadHtml(html);

            var matchingImage = doc.DocumentNode
                .Descendants()
                .Where(o => NodeHasImageMatchingUrl(o))
                .FirstOrDefault();

            if (matchingImage != null)
                RemoveSelfAndEmptyParentNodes(matchingImage);

            var fixedHtml = doc.DocumentNode.WriteTo();
            result.content = fixedHtml;
        }

        bool NodeHasImageMatchingUrl(HtmlNode node)
        {
            return
                node.Name == "img" &&
                node.Attributes["src"] != null &&
                mobilizerDetectedFirstImage.Equals(node.Attributes["src"].Value, StringComparison.OrdinalIgnoreCase);
        }

        void RemoveSelfAndEmptyParentNodes(HtmlNode node)
        {
            HtmlNode parent, currentNode;

            currentNode = node;
            
            while (currentNode != null && string.IsNullOrWhiteSpace(currentNode.InnerHtml))
            {
                parent = currentNode.ParentNode;
                currentNode.Remove();
                currentNode = parent;
            }

            currentNode.InnerHtml = currentNode.InnerHtml.Trim();
        }
    }
}
