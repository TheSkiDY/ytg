using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.ObjectPool;
using System.Text;
using YTGsr.Hubs;

namespace YTGsr
{

    public class Message
    {
        public string author { get; set; }
        public string text { get; set; }
        public DateTime timeSent { get; set; }
    }

    public class Player
    {
        public UserConnection userConnection;
        public int Points
        {
            get; set;
        }

        public int PointsGotThisRound
        {
            get; set;
        }

        public string Name
        {
            get
            {
                return userConnection.User;
            }
        }

        public string Id
        {
            get
            {
                return userConnection.Uuid;
            }
        }

        public UserConnection UserConnection
        {
            get
            {
                return userConnection;
            }
        }

        public Player(UserConnection userConnection)
        {
            this.userConnection = userConnection;
        }
    }


    public class Room
    {
        Queue<Message> messages;
        public Game game;
        List<Player> players;
        Player admin;


        private const int msgHistoryCapacity = 50;
        private const int maxPlayers = 12;
        public enum Option
        {
            PlaylistType,
            PlaylistLink,
            MediaType,
            AnswerType,
            Time,
            Rounds,
        }

        public void AddMessageToHistory(string id, string message)
        {
            try
            {
                Player author = players.Where(p => p.Id == id).FirstOrDefault();
                if (messages.Count == msgHistoryCapacity)
                {
                    messages.Dequeue();
                }
                messages.Enqueue(new Message() { author = author.Name, text = message, timeSent = DateTime.Now });
            }
            catch
            {

            }
        }
        public string GetMessageHistory()
        {
            StringBuilder builder = new StringBuilder();
            int historyCount = messages.Count;

            foreach (var msg in messages)
            {
                builder.Append(msg.author);
                builder.Append(",");
                builder.Append(msg.text);
                builder.Append(",");
                builder.Append(Util.GetHourFromDateTime(msg.timeSent));
                builder.Append(";");
            }

            return builder.ToString();
        }

        public string GetAdminName()
        {
            return admin.Name;
        }

        public string GetAdminId()
        {
            return admin.Id;
        }

        public void SetAdmin(string id)
        {
            try
            {
                admin = players.Where(p => p.Id == id).FirstOrDefault();
            }
            catch
            {
                admin = players[0];
            }
        }


        public void AddPlayer(UserConnection userConnection)
        {
            Player player = new Player(userConnection);
            players.Add(player);
        }

        public void RemovePlayer(UserConnection userConnection)
        {
            try
            {
                players.Remove(players.Where(p => p.UserConnection == userConnection).FirstOrDefault());
                admin = players[0];
            }
            catch
            {

            }
        }

        public void ZeroPoints()
        {
            foreach (var p in players)
            {
                p.Points = 0;
                p.PointsGotThisRound = 0;
            }
        }

        public void AddPoints(UserConnection user, int seconds, bool isCorrect)
        {
            var player = players.Where(p => p.UserConnection == user).FirstOrDefault();
            int playTime = game.time;

            seconds = Math.Min(seconds, game.time);
            seconds = Math.Max(0, seconds);

            int score = (seconds * 100) / playTime;

            if (!isCorrect)
            {
                if(player.Points > 0)
                {
                    score /= 2;
                    score = -Math.Min(score, player.Points);
                }
                else
                {
                    score = 0;
                }
            }

            player.PointsGotThisRound = score;
            player.Points += score;

        }

        public string[] GetPlayers()
        {
            return players.Select(p => p.Name).ToArray();
        }

        public string[] GetPlayerIds()
        {
            return players.Select(p => p.Id).ToArray();
        }

        public int PlayerCount()
        {
            return GetPlayers().Count();
        }

        public Game.GameStage GetStage()
        {
            return game.Stage;
        }

        public int GetRound()
        {
            return game.currentRound;
        }

        public Game.AnswerType GetAnsType()
        {
            return game.answerType;
        }

        public string GetURLForRound()
        {
            int i = game.currentRound - 1;
            return game.questions[i].correctURL;
        }

        public string[] GetAnswersForRound()
        {
            int i = game.currentRound - 1;
            return game.questions[i].answerTitles;
        }

        public int GetCorrectAnswerForRound()
        {
            int i = game.currentRound - 1;
            return game.questions[i].correctAnswer;
        }

        public int GetTimePlayedForRound()
        {
            int i = game.currentRound - 1;
            return game.questions[i].startingTime;
        }

        public string GetResultsFromRound()
        {
            List<(string, int, int)> results = new List<(string, int, int)>();
            foreach(var p in players)
            {
                results.Add((p.Name, p.Points, p.PointsGotThisRound));
                p.PointsGotThisRound = 0;
            }
            var sortedResults = results.OrderBy(o => o.Item2).ToList();

            StringBuilder sb = new StringBuilder();
            foreach(var r in sortedResults)
            {
                sb.Append(r.Item1);
                sb.Append(",");
                sb.Append(r.Item2);
                sb.Append(",");
                sb.Append(r.Item3);
                sb.Append(";");
            }
            return sb.ToString();
        }

        public Room()
        {
            messages = new Queue<Message>(msgHistoryCapacity);
            players = new List<Player>(maxPlayers);
            game = new Game();
        }

    }
}
