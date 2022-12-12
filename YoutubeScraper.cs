using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace YotubeAPIExtras
{
    public class YoutubeScraper
    {
        int QuotaFilled;
        const int QuotaLimit = 10000;
        List<Video> videos = new List<Video>();

        [STAThread]
        public void Start(string id)
        {
            QuotaFilled = 0;

            Console.WriteLine("Adding Youtube Bookmarks...");

            bool reachedLimit = GetNewVideos(id).GetAwaiter().GetResult();

            var folder = @"C:\Users\Favorites\Youtube Bookmarks\";

            Console.WriteLine("Saving {0} videos.", videos.Count());

            foreach (var video in videos)
            {
                CreateShortcut(video, folder);
            }

        }

        private async Task<bool> GetNewVideos(string channelIDs)
        {
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = "",
                ApplicationName = this.GetType().ToString()
            });

            var channelsListRequest = youtubeService.Channels.List("contentDetails");
            channelsListRequest.Id = channelIDs;
            QuotaFilled += 3;

            var channelsListResponse = await channelsListRequest.ExecuteAsync();

            bool reachedLimit = false;
            foreach (var channel in channelsListResponse.Items)
            {
                var uploadsListId = channel.ContentDetails.RelatedPlaylists.Uploads;
                var id = channel.Id;

                bool end = false;
                var nextPageToken = "";

                while (nextPageToken != null)
                {
                    if (QuotaFilled < QuotaLimit - 50)
                    {
                        var playlistItemsListRequest = youtubeService.PlaylistItems.List("snippet");
                        QuotaFilled += 3;
                        playlistItemsListRequest.PlaylistId = uploadsListId;
                        playlistItemsListRequest.MaxResults = 50;
                        playlistItemsListRequest.PageToken = nextPageToken;

                        // Retrieve the list of videos uploaded to the channel.
                        var playlistItemsListResponse = await playlistItemsListRequest.ExecuteAsync();
                        DateTime date = DateTime.Parse("June 8, 2018");

                        foreach (var playlistItem in playlistItemsListResponse.Items)
                        {
                            if (playlistItem.Snippet.PublishedAt.Value.Date >= date)
                            {
                                videos.Add(new Video(playlistItem.Snippet.PublishedAt.Value, playlistItem.Snippet.Title, playlistItem.Snippet.ResourceId.VideoId));
                            }
                            else
                            {
                                end = true;
                                break;
                            }
                        }

                        nextPageToken = end ? null : playlistItemsListResponse.NextPageToken;
                    }
                    else
                    {
                        reachedLimit = true;

                        SaveQuotaFilled();
                        return reachedLimit;
                    }

                }
            }

            SaveQuotaFilled();

            return reachedLimit;
        }

        void SaveQuotaFilled()
        {
            var path = @"C:\Users\Favorites\Youtube Bookmarks\quota.txt";

            string txt = QuotaFilled.ToString();
            File.WriteAllText(path, txt);
        }

        int GetQuota()
        {
            int quota = 0;

            var path = @"C:\Users\Favorites\Youtube Bookmarks\Main.txt";
            string[] lines = File.ReadLines(path).ToArray();

            DateTime now = DateTime.Now;
            DateTime last = DateTime.Parse(lines[2]);

            if (now.Date == last.Date)
            {
                quota = int.Parse(lines[1]);
            }

            return quota;
        }

        private static void CreateShortcut(Video video, string folderPath)
        {
            using (StreamWriter writer = new StreamWriter(TrimPath(folderPath + video.ShortcutName)))
            {
                writer.WriteLine("[InternetShortcut]");
                writer.WriteLine("URL=" + video.URL.Trim());
                writer.Flush();
            }
        }

        private static string TrimPath(string path)
        {
            if (path.Length > 260)
            {
                path = path.Substring(0, 259);
            }
            return path;
        }
    }
}
