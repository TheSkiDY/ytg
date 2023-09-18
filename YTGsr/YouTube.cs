using Google.Apis.YouTube.v3;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Xml;

namespace YTGsr
{
    public class YouTube
    {
        YouTubeService YTService;

        private string IsURLValid(string url)
        {
            string playlistRegex = @"^(?:https?:\/\/)?(?:www\.)?youtube\.com\/playlist\?list=([a-zA-Z0-9_-]+)";

            Regex regex = new Regex(playlistRegex);
            Match match = regex.Match(url);

            if (match.Success && match.Groups.Count > 1)
            {
                string result = match.Groups[1].Value;
                return result;
            }
            else
            {
                return string.Empty;
            }
        }

        private async Task<bool> ValidateIdAndLength(string playlistId, int roundCount)
        {
            var request = YTService.PlaylistItems.List("snippet");
            request.PlaylistId = playlistId;
            request.Fields = "pageInfo";

            try
            {
                var response = await request.ExecuteAsync();
                var len = response.PageInfo.TotalResults;

                return (len != null && (int)len > roundCount);
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private async Task<string> GetUploadPlaylistIdFromChannelId(string channelId)
        {
            var request = YTService.Channels.List("snippet,contentDetails");
            request.Id = channelId;

            try
            {
                var response = await request.ExecuteAsync();
                var item = response.Items[0];
                string playlistId = item.ContentDetails.RelatedPlaylists.Uploads;
                return playlistId;
            }
            catch(Exception e)
            {
                return string.Empty;
            }
        }

        public async Task<string> ValidatePlaylist(string url, bool isChannel, int roundCount)
        {
            //check if link input is correct
            string playlistId = string.Empty;
            if(!isChannel)
            {
                playlistId = IsURLValid(url);
            }
            else
            {
                playlistId = await GetUploadPlaylistIdFromChannelId(url);
            }

            if (string.IsNullOrEmpty(playlistId))
                return string.Empty;

            //check if playlist exists and meets the requirements
            bool valid = await ValidateIdAndLength(playlistId, roundCount);
            if (valid)
                return playlistId;
            else
                return string.Empty;
        }
        

        public async Task<int[]> GetDurationOfVideos(string videoIds, int roundCount)
        {
            int[] duration = new int[roundCount];

            var durationRequest = YTService.Videos.List("snippet,contentDetails");
            durationRequest.Id = videoIds;
            int i = 0;
            try
            {
                var response = await durationRequest.ExecuteAsync();
                string durationIso = string.Empty;
                foreach(var result in response.Items)
                {
                    durationIso = result.ContentDetails.Duration;
                    duration[i] = (int)XmlConvert.ToTimeSpan(durationIso).TotalSeconds;
                    i++;
                }
            }
            catch(Exception e)
            {
                return duration;
            }
            return duration;
        }

        public async Task<List<Video>> GetVideosFromPlaylist(string playlistId)
        {
            List<Video> videos = new List<Video>();
            var nextPageToken = "";

            while(nextPageToken != null)
            {
                try
                {
                    var playlistItemsRequest = YTService.PlaylistItems.List("snippet");
                    playlistItemsRequest.PlaylistId = playlistId;
                    playlistItemsRequest.MaxResults = 50;
                    playlistItemsRequest.PageToken = nextPageToken;
                    playlistItemsRequest.Fields = "(nextPageToken,items/snippet(title,resourceId))";
                    var response = await playlistItemsRequest.ExecuteAsync();

                    foreach(var result in response.Items)
                    {
                        videos.Add(new Video()
                        {
                            title = result.Snippet.Title,
                            id = result.Snippet.ResourceId.VideoId
                        });
                    }

                    nextPageToken = response.NextPageToken;
                }
                catch(Exception e)
                {
                    break;
                }
            }
            return videos;
        }



        public YouTube()
        {
            YTService = new YouTubeService(new Google.Apis.Services.BaseClientService.Initializer()
            {
                ApiKey = YTApiKey.Key,
                ApplicationName = this.GetType().ToString()
            });
        }
    }
}
