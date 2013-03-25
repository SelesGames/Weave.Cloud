﻿
namespace SelesGames.HttpClient
{
    public class ContentEncoderSettings
    {
        // accept header for requests
        public string Accept { get; set; }

        // headers for POST calls
        public string ContentType { get; set; }




        #region Included encoder settings

        public static readonly ContentEncoderSettings Json = new ContentEncoderSettings
        {
            Accept = "application/json",
            ContentType = "application/json"
        };
        public static readonly ContentEncoderSettings Protobuf = new ContentEncoderSettings
        {
            Accept = "application/protobuf",
            ContentType = "application/protobuf"
        };

        #endregion
    }
}
