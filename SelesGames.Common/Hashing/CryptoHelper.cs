using System;
using System.Security.Cryptography;
using System.Text;

namespace SelesGames.Common.Hashing
{
    public class CryptoHelper
    {
        // approach using SHA1Managed and GUIDs
        //public static Guid ComputeHashUsedByWeaveClient(string val)
        //{
        //    var md5 = new System.Security.Cryptography.SHA1Managed();
        //    byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(val);
        //    byte[] hash = md5.ComputeHash(inputBytes);
        //    Array.Resize(ref hash, 16);
        //    var guid = new Guid(hash);
        //    return guid;
        //}

        // approach using MD5 and GUIDs
        public static Guid ComputeHashUsedByMobilizer(string val)
        {
            var md5 = MD5.Create();
            byte[] inputBytes = Encoding.ASCII.GetBytes(val);
            byte[] hash = md5.ComputeHash(inputBytes);
            var guid = new Guid(hash);
            return guid;
        }
    }
}
