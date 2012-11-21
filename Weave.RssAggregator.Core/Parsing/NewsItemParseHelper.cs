using System;
using System.Linq;
using System.Text.RegularExpressions;
using Weave.RssAggregator.Core.DTOs.Outgoing;

namespace Weave.RssAggregator.Core.Parsing
{
    internal static class NewsItemParseHelperExtension
    {
        public static void ExtractYoutubeVideoAndPodcastUrlsFromDescription(this NewsItem newsItem)
        {
            if (newsItem == null)
                throw new ArgumentNullException("newsItem in ParseDescriptionAndExtractYoutubeVideoAndPodcast");

            string descriptionText = newsItem.Description;

            if (string.IsNullOrEmpty(descriptionText))
                return;

            Regex r = new Regex(@"http(s)?://([\w+?\.\w+])+([a-zA-Z0-9\~\!\@\#\$\%\^\&amp;\*\(\)_\-\=\+\\\/\?\.\:\;\'\,]*)?");

            var matches = r.Matches(descriptionText)
                .OfType<Match>()
                .Select(o => o.Value)
                .Where(o => !string.IsNullOrEmpty(o))
                .ToList();


            var potentialImage = matches.Where(o => o.IsImageUrl()).FirstOrDefault();
            newsItem.ImageUrl = potentialImage;


            var potentialYoutube = matches
                .Where(o =>
                    o.StartsWith("http://www.youtube.com", StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();

            if (!string.IsNullOrEmpty(potentialYoutube))
            {
                var youtubeVideoId = ExtractYoutubeVideoIdFromUrl(potentialYoutube);
                newsItem.YoutubeId = youtubeVideoId;
            }


            var potentialVideo = matches.Where(o => o.IsVideoFileUrl()).FirstOrDefault();
            newsItem.VideoUri = potentialVideo;


            var potentialPodcast = matches.Where(o => o.IsAudioFileUrl()).FirstOrDefault();
            newsItem.PodcastUri = potentialPodcast;
        }

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




        #region deprecated




        #region deliberate approach

        //public void ParseDescriptionUsingRegexAndDeliberateApproach(string descriptionText)
        //{
        //    if (string.IsNullOrEmpty(descriptionText))
        //        return;

        //    var sw = System.Diagnostics.Stopwatch.StartNew();

        //    Regex r = new Regex(@"http(s)?://([\w+?\.\w+])+([a-zA-Z0-9\~\!\@\#\$\%\^\&amp;\*\(\)_\-\=\+\\\/\?\.\:\;\'\,]*)?");

        //    var matches = r.Matches(descriptionText).OfType<Match>().Select(o => o.Value).ToList();

        //    string potentialImage = null;
        //    string potentialYoutube = null;
        //    string potentialPodcast = null;

        //    bool hasFoundPotentialImage = false;
        //    bool hasFoundPotentialYoutube = false;
        //    bool hasFoundPotentialPodcast = false;

        //    foreach (var match in matches)
        //    {
        //        if (!hasFoundPotentialImage)
        //        {
        //            if (match.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
        //                match.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
        //            {
        //                potentialImage = match;
        //                hasFoundPotentialImage = true;
        //            }
        //        }

        //        if (!hasFoundPotentialYoutube)
        //        {
        //            if (match.StartsWith("http://www.youtube.com", StringComparison.OrdinalIgnoreCase))
        //            {
        //                potentialYoutube = match;
        //                hasFoundPotentialYoutube = true;
        //            }
        //        }

        //        if (!hasFoundPotentialPodcast)
        //        {
        //            if (match.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase) ||
        //                match.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
        //            {
        //                potentialPodcast = match;
        //                hasFoundPotentialPodcast = true;
        //            }
        //        }

        //        if (hasFoundPotentialImage && hasFoundPotentialYoutube && hasFoundPotentialPodcast)
        //            goto CHECK_MATCHES_END;
        //    }



        //CHECK_MATCHES_END:


        //    //DebugEx.WriteLine(potentialImage);
        //    newsItem.ImageUrl = potentialImage;
        //    //DebugEx.WriteLine(potentialYoutube);
        //    newsItem.YoutubeUri = potentialYoutube;
        //    //DebugEx.WriteLine(potentialPodcast);
        //    newsItem.PodcastUri = potentialPodcast;

        //    sw.Stop();
        //    DebugEx.WriteLine("took {0} ticks to SUPER REGEX PARSE html", sw.ElapsedTicks);
        //}

        #endregion




        #endregion
    }
}
