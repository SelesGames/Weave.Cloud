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
        ReadabilityResult result;
        HtmlDocument doc;

        string testurl =
//"http://thenextweb.com/insider/2013/06/23/researchers-are-developing-a-system-to-securely-leak-information-by-surfing-the-web/?utm_source";
//"http://www.theverge.com/2013/6/21/4451172/xbox-engineer-disappointed-pastebin-note-rumored-features?utm_source";
//"http://news.cnet.com/8301-1009_3-57590599-83/whistle-blower-update-snowden-lands-in-moscow-wikileakers-gmail-searched/?part";
//"http://www.guardian.co.uk/world/2013/jun/23/chen-guangcheng-visits-taiwan";
"http://www.wpcentral.com/e-online-releases-official-app-windows-phone-8";

        //public async Task TryStuff()
        //{
        //    var dto = await new MobilizerServiceClient("hxyuiplkx78!ksdfl").Get(testurl);
        //    ProcessContent(dto);
        //    System.Diagnostics.Debug.WriteLine(dto);
        //}

        public void ProcessContent(ReadabilityResult result)
        {
            this.result = result;
            this.mobilizerDetectedFirstImage = result.lead_image_url;

            if (string.IsNullOrWhiteSpace(mobilizerDetectedFirstImage))
                return;

            LoadHtml();
            RemoveContentAboveFirstParagraph();
            RemoveImageFromContentMatchingLead();
            WriteToResult();
        }

        void LoadHtml()
        {
            var html = result.content;

            this.doc = new HtmlDocument();
            doc.OptionDefaultStreamEncoding = Encoding.UTF8;
            doc.LoadHtml(html);
        }

        void RemoveContentAboveFirstParagraph()
        {
            var firstParagraph = doc.DocumentNode
                .Descendants()
                .Where(o => o.Name == "p")
                .FirstOrDefault();

            if (firstParagraph != null)
                RemoveAllParentsPreviousSiblings(firstParagraph);
        }

        void RemoveImageFromContentMatchingLead()
        {
            var matchingImage = doc.DocumentNode
                .Descendants()
                .Where(o => NodeHasImageMatchingUrl(o))
                .FirstOrDefault();

            if (matchingImage != null)
                RemoveSelfAndEmptyParentNodes(matchingImage);
        }

        void WriteToResult()
        {
            var fixedHtml = doc.DocumentNode.WriteTo();
            result.content = fixedHtml;
        }




        #region previous sibling process - remove previous siblings of first paragraph

        static void RemoveAllParentsPreviousSiblings(HtmlNode node)
        {
            HtmlNode parent, currentNode;

            currentNode = node;

            while (currentNode != null)
            {
                parent = currentNode.ParentNode;
                RemoveAllPreviousSiblings(currentNode);
                currentNode = parent;
            }
        }

        static void RemoveAllPreviousSiblings(HtmlNode node)
        {
            HtmlNode previous, currentNode;

            currentNode = node.PreviousSibling;

            while (currentNode != null)
            {
                previous = currentNode.PreviousSibling;
                currentNode.Remove();
                currentNode = previous;
            }
        }

        #endregion




        #region image process - delete lead image url from content

        bool NodeHasImageMatchingUrl(HtmlNode node)
        {
            return
                node.Name == "img" &&
                node.Attributes["src"] != null &&
                mobilizerDetectedFirstImage.Equals(node.Attributes["src"].Value, StringComparison.OrdinalIgnoreCase);
        }

        static void RemoveSelfAndEmptyParentNodes(HtmlNode node)
        {
            HtmlNode parent, currentNode;

            currentNode = node;
            
            while (currentNode != null && string.IsNullOrWhiteSpace(currentNode.InnerHtml))
            {
                parent = currentNode.ParentNode;
                currentNode.Remove();
                currentNode = parent;
            }

            if (currentNode != null)
                currentNode.InnerHtml = currentNode.InnerHtml.Trim();
        }

        #endregion
    }
}
