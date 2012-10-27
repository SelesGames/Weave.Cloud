using System.IO;
using System.Xml.Linq;

namespace SelesGames.Rest
{
    public class XmlRestClient : RestClient<XElement>
    {
        protected override XElement ReadObject(Stream stream)
        {
            return XElement.Load(stream);
        }
    }
}
