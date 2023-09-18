//#define DEBUG_LOCAL
using Google.Apis.YouTube.v3.Data;
using System.Text;

namespace YTGsr
{

    public class Game
    {

        public const int defaultTime = 15;
        public const int defaultNumOfRounds = 5;
        public const int numOfAnswers = 4;

        public class Question
        {
            public string correctURL = string.Empty;
            public string[] answerTitles = { };
            public int correctAnswer = 0;
            public int startingTime = 0;
        }

        public enum PlaylistType
        { 
            Playlist,
            Channel,
        }


        public enum GameStage
        {
            ConfigPanel,
            GuessRound,
            RoundResults,
            GameResults
        }

        public enum MediaType
        {
            Audio,
            Video,
            Thumbnail,
        }

        public enum AnswerType
        {
            Closed,
            Open,
        }

        public GameStage Stage { get; private set; }
        public int currentRound { get; private set; }

        public MediaType mediaType { get; private set; }
        public AnswerType answerType { get; private set; }
        public PlaylistType playlistType { get; private set; }
        public int time { get; private set; }
        public int roundCount { get; private set; }
        public string playlistId { get; private set; }
        public string lastPlaylistId { get; private set; }

        public List<Video> videos = new List<Video>();
        public Question[] questions = null;
        Random rd = new Random();
        YouTube YT = new YouTube();

        public void SetPlaylistType(PlaylistType type)
        {
            this.playlistType = type;
        }

        public void SetMediaType(MediaType type)
        {
            this.mediaType = type;
        }

        public void SetAnswerType(AnswerType type)
        {
            this.answerType = type;
        }

        public void SetTime(int time)
        {
            this.time = time;
        }

        public void SetRoundsAmount(int amount)
        {
            this.roundCount = amount;
        }

        public void SetPlaylistURL(string url)
        {
            this.playlistId = url;
        }

        public int RandomizeCorrectVideo(List<int> usedIndices, int playlistLength)
        {
            bool validIndex = false;
            int videoId = 0;

            //randomize video to show
            while (!validIndex)
            {
                validIndex = true;
                videoId = rd.Next(0, playlistLength);
                if(usedIndices.Contains(videoId) || videos[videoId].title.ToLower() == "deleted video" || videos[videoId].title.ToLower() == "private video")
                {
                    validIndex = false;
                }
                else
                {
                    usedIndices.Add(videoId);
                }
            }
            return videoId;
        }

        public void RandomizeTitlesForOtherAnswers(int[] answersIndices, int correctPosition, int videoId, int playlistLength, int answerLen)
        {
            List<int> usedWrongs = new List<int>();
            for (int j = 0; j < answerLen; j++)
            {
                if (j == correctPosition)
                {
                    continue;
                }

                int wrongAnswerId = videoId;
                bool wrongUsed = false;
                while (wrongAnswerId == videoId || wrongUsed)
                {
                    wrongAnswerId = rd.Next(0, playlistLength);
                    if (usedWrongs.Contains(wrongAnswerId))
                    {
                        wrongUsed = true;
                    }
                    else
                    {
                        wrongUsed = false;
                    }

                }
                answersIndices[j] = wrongAnswerId;
                usedWrongs.Add(wrongAnswerId);
            }
        }

        public void AddToQuestionsArray(int[] answersIndices, int index, int correctPosition, int videoId, int answersLen)
        {
            questions[index] = new()
            {
                correctURL = videos[videoId].id,
                correctAnswer = correctPosition,
                answerTitles = new string[answersLen],
            };
            for (int j = 0; j < answersLen; j++)
            {
                questions[index].answerTitles[j] = videos[answersIndices[j]].title;
            }
        }

        public async Task SeedRounds()
        {
            List<int> usedVideos = new List<int>();
            int playlistLength = videos.Count;
            int answersLen = (answerType == AnswerType.Closed) ? numOfAnswers : playlistLength;

            for (int i = 0; i < roundCount; i++)
            {
                int videoId = 0;
                int[] answersIndices = new int[answersLen];
                int correctPosition = 0;

                videoId = RandomizeCorrectVideo(usedVideos, playlistLength);
                correctPosition = (answerType == AnswerType.Closed) ? rd.Next(0, answersLen) : videoId;

                if (answerType == AnswerType.Closed)
                {
                    answersIndices[correctPosition] = videoId;
                    RandomizeTitlesForOtherAnswers(answersIndices, correctPosition, videoId, playlistLength, answersLen);
                }
                else
                {
                    for (int j = 0; j < playlistLength; j++)
                    {
                        answersIndices[j] = j;
                    }
                }
                AddToQuestionsArray(answersIndices, i, correctPosition, videoId, answersLen);
            }

            //randomize time played
            StringBuilder idsBuilder = new StringBuilder();
            for (int i = 0; i < roundCount; i++)
            {
                idsBuilder.Append(questions[i].correctURL);
                if (i != roundCount - 1)
                    idsBuilder.Append(",");
            }
#if DEBUG_LOCAL
            int[] durations = new int[roundCount];
            for (int i = 0; i < roundCount; i++)
            {
                durations[i] = rd.Next(0,100);
            }
#else
            int[] durations = await YT.GetDurationOfVideos(idsBuilder.ToString(), roundCount);
#endif
            for (int i = 0; i < roundCount; i++)
            {
                if (durations[i] < time)
                    questions[i].startingTime = 0;
                else
                    questions[i].startingTime = rd.Next(0, durations[i] - time);
            }
        }

        public async Task<bool> ValidatePlaylist(string url)
        {
            string id = await YT.ValidatePlaylist(url, (playlistType == PlaylistType.Channel), roundCount);
            if (!string.IsNullOrEmpty(id))
            {
                playlistId = id;
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task Init()
        {
            //TO DO: check if last playlist wasnt' changed to avoid new API requests
            if (lastPlaylistId != playlistId)
            {
                videos.Clear();
                questions = new Question[roundCount];
#if DEBUG_LOCAL
                videos.Add(new Video() { id = "MGi1hqjhwdA", title = "[REMIX] DISS NA XARDASA - GOTHIC REMIX" });
                videos.Add(new Video() { id = "Xrwz0r4bjro", title = "[REMIX] DISS NA PYROKARA - GOTHIC REMIX" });
                videos.Add(new Video() { id = "sFDXdBCdnWE", title = "[REMIX] SIEKAM CEBULKĘ - GOTHIC REMIX" });
                videos.Add(new Video() { id = "dLLRUtql_ys", title = "[REMIX] KUCHNIA SNAFA - GOTHIC REMIX" });
                videos.Add(new Video() { id = "05aPGgNXLAc", title = "[REMIX] MY LUBIMY PALIĆ - GOTHIC REMIX" });
                videos.Add(new Video() { id = "l4KqQkaggis", title = "[REMIX] NAJWIĘKSZY WRÓG - GOTHIC REMIX (DRABINA)" });
                videos.Add(new Video() { id = "y_ifjEYEMuY", title = "[REMIX] DŻIN Z KUFRA - GOTHIC REMIX (feat. Kaktus)" });
                videos.Add(new Video() { id = "ww9pHXl4Clk", title = "[REMIX] CEBULOWA ZEMSTA - GOTHIC REMIX" });
                videos.Add(new Video() { id = "Qx2ZSGOOIpo", title = "[REMIX] TOWAR - GOTHIC REMIX (Ukradli nam towar)" });
                videos.Add(new Video() { id = "oTDxl0vZpFA", title = "[REMIX] KWARANTANNA/HOT16 - GOTHIC REMIX" });
                videos.Add(new Video() { id = "C2fc2JBPGow", title = "[REMIX] WSZĘDZIE WIDZĘ MASKĘ - GOTHIC REMIX" });
#else
                videos = await YT.GetVideosFromPlaylist(playlistId);
#endif
            }
            await SeedRounds();
        }

        public void AdvanceGameState()
        {
            switch (Stage)
            {
                case GameStage.ConfigPanel:
                    currentRound++;
                    Stage = GameStage.GuessRound;
                    break;
                case GameStage.GuessRound:
                    Stage = GameStage.RoundResults;
                    break;
                case GameStage.RoundResults:
                    if (currentRound == roundCount)
                    {
                        Stage = GameStage.GameResults;
                    }
                    else
                    {
                        Stage = GameStage.GuessRound;
                        currentRound++;
                    }
                    break;
                case GameStage.GameResults:
                    currentRound = 0;
                    lastPlaylistId = playlistId;
                    Stage = GameStage.ConfigPanel;
                    break;
            }
        }

        public Game()
        {
            this.Stage = GameStage.ConfigPanel;
            this.time = defaultTime;
            this.roundCount = defaultNumOfRounds;
            this.currentRound = 0;
            this.playlistId = string.Empty;
        }
    }
}