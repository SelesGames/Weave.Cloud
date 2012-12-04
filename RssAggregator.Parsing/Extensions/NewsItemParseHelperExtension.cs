using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Weave.RssAggregator.Parsing
{
    internal static class NewsItemParseHelperExtension
    {
        public static void ExtractYoutubeVideoAndPodcastUrlsFromDescription(this Entry e)
        {
            if (e == null)
                throw new ArgumentNullException("newsItem in ParseDescriptionAndExtractYoutubeVideoAndPodcast");

            string descriptionText = e.Description;

            if (string.IsNullOrEmpty(descriptionText))
                return;

            Regex r = new Regex(@"http(s)?://([\w+?\.\w+])+([a-zA-Z0-9\~\!\@\#\$\%\^\&amp;\*\(\)_\-\=\+\\\/\?\.\:\;\'\,]*)?");

            var matches = r.Matches(descriptionText)
                .OfType<Match>()
                .Select(o => o.Value)
                .Where(o => !string.IsNullOrEmpty(o))
                .ToList();


            var potentialImage = matches.Where(o => o.IsImageUrl()).FirstOrDefault();
            e.ImageUrl = potentialImage;


            var potentialYoutube = matches
                .Where(o =>
                    o.StartsWith("http://www.youtube.com", StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();

            if (!string.IsNullOrEmpty(potentialYoutube))
            {
                var youtubeVideoId = ExtractYoutubeVideoIdFromUrl(potentialYoutube);
                e.YoutubeId = youtubeVideoId;
            }


            var potentialVideo = matches.Where(o => o.IsVideoFileUrl()).FirstOrDefault();
            e.VideoUri = potentialVideo;


            var potentialPodcast = matches.Where(o => o.IsAudioFileUrl()).FirstOrDefault();
            e.PodcastUri = potentialPodcast;
        }

        static string ExtractYoutubeVideoIdFromUrl(string potentialYoutube)
        {
            string youtubeVideoId = null;

            try
            {
                var remainder = potentialYoutube.Substring(23);

                if (remainder.StartsWith("v", StringComparison.OrdinalIgnoreCase))
                {
                    youtubeVideoId = remainder.Substring(2, 11);
                }

                else if (remainder.StartsWith("em", StringComparison.OrdinalIgnoreCase))
                {
                    youtubeVideoId = remainder.Substring(6, 11);
                }

                else if (remainder.StartsWith("wa", StringComparison.OrdinalIgnoreCase))
                {
                    var watchRegex = new Regex(@"watch\?.*v=(.{11})");
                    youtubeVideoId = watchRegex.Match(remainder).Groups[1].Captures[0].Value;
                }

                else if (remainder.StartsWith("us"))
                {
                    var userRegex = new Regex(@"user/.+#p/u/0/(.{11})");
                    youtubeVideoId = userRegex.Match(remainder).Groups[1].Captures[0].Value; ;
                }
            }
            catch (Exception) { }

            return youtubeVideoId;
        }




        #region sample youtube videos
        
        /// http://www.youtube.com/embed/Km7spIEcN7o?hd=1
        /// http://www.youtube.com/embed/RukvZrFxNuU?hd=1
        /// http://www.youtube.com/watch?v=0N1_0SUGlDQ
        /// http://www.youtube.com/watch?v=EIz4C93i-U8
        /// http://www.youtube.com/v/2BCcqQCmXdg?hl=en&amp;hd=1
        /// http://www.youtube.com/user/PlantsVsZombies#p/u/0/O-FGCZ4Ro8U
        /// http://www.youtube.com/v/1_typayezjM?version=3&amp;hl=en_US
        /// http://www.youtube.com/playlist?list=PL58F8FA906F76CAE9
        /// http://www.youtube.com/v/Iq7le236lHI?version=3&amp;hl=en_US
        /// http://www.youtube.com/embed/6TnLCqozmmU?rel=0&hd=1&wmode=opaque
        /// http://www.youtube.com/embed/Hyi5xNmyptY?wmode=opaque
        /// http://www.youtube.com/watch?v=2eEibo-8XYc&amp;feature=player_embedded
        /// http://www.youtube.com/user/tkelleman#p/u/0/2Ea8iTpdXYg
        /// http://www.youtube.com/watch?feature=player_embedded&v=Ps0cZESV-ec
        /// http://www.youtube.com/watch?v=VUDBmUa27W0&#038;hd=1

        #endregion
    }
}
