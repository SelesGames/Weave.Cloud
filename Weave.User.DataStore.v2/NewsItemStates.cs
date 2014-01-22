using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Weave.User.DataStore.v2
{
    [DataContract]
    public class NewsItemStates : Dictionary<string, byte>
    {
    }
}
