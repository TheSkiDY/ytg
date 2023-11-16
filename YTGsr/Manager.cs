using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using YTGsr.Hubs;

namespace YTGsr
{
    public interface IManager
    {
        bool DoesRoomExist(string roomCode);
        bool IsRoomEmpty(string roomCode);
        Room GetRoom(string roomCode);
        Room CreateRoom(string roomCode);
        void RemoveRoom(string roomCode);

        Task SendMessage(string message, UserConnection userConnection);
        Task RemoveUserFromRoom(string roomCode, UserConnection userConnection);
        Task SetRoomAdmin(string user, string roomCode);
        Task SendConnectedUsers(string roomCode);
        Task UpdateOptionInConfigPanel(string optionStr, string arg, string roomCode);
        Task InitGameState(string roomCode);
        Task AdvanceGameState(string roomCode);
        Task<bool> ValidatePlaylist(string url, string roomCode);
        Task GuessAnswer(bool isCorrect, int seconds, UserConnection userConnection);

        Task SendTestMessage(string message, string roomCode);
    }


    public class Manager : IManager
    {
        Dictionary<string, Room> rooms = new Dictionary<string, Room>();
        private IHubContext<MainHub, IMainHub> _hubContext;

        public bool DoesRoomExist(string roomCode)
        {
            return rooms.ContainsKey(roomCode);
        }

        public Room GetRoom(string roomCode)
        {
            Room room = rooms[roomCode];
            return room;
        }

        public bool IsRoomEmpty(string roomCode)
        {
            Room room = GetRoom(roomCode);
            return room.PlayerCount() <= 0;
        }

        public Room CreateRoom(string roomCode)
        {
            Room room = new Room();
            rooms.Add(roomCode, room);
            return room;
        }

        public void RemoveRoom(string roomCode)
        {
            rooms.Remove(roomCode);
            GC.Collect();
        }

        public async Task SendMessage(string message, UserConnection userConnection)
        {
            Room room = GetRoom(userConnection.Room);
            room.AddMessageToHistory(userConnection.Uuid, message);
            await _hubContext.Clients.Group(userConnection.Room).ReceiveMessage(userConnection.User, message, Util.GetHourFromDateTime(DateTime.Now));
        }

        public Task RemoveUserFromRoom(string roomCode, UserConnection userConnection)
        {
            Room room = GetRoom(roomCode);
            room.RemovePlayer(userConnection);
            string adminId = room.GetAdminId();
            return _hubContext.Clients.Group(roomCode).UpdateAdmin(adminId);
        }

        public async Task SetRoomAdmin(string id, string roomCode)
        {
            Room room = GetRoom(roomCode);
            room.SetAdmin(id);
            await _hubContext.Clients.Group(roomCode).UpdateAdmin(id);
        }

        public async Task SendConnectedUsers(string roomCode)
        {
            Room room = GetRoom(roomCode);
            var players = room.GetPlayers();
            var ids = room.GetPlayerIds();
            int playerCount = players.Count();
            await _hubContext.Clients.Group(roomCode).UpdatePlayersList(players, ids);
            await _hubContext.Clients.Group(roomCode).PlayersCount(playerCount);
        }

public async Task UpdateOptionInConfigPanel(string optionStr, string arg, string roomCode)
{
    try
    {
    Room.Option option = default;
    Room room = GetRoom(roomCode);
    if (Enum.TryParse<Room.Option>(optionStr, true, out option))
    {
        switch (option)
        {
            case Room.Option.PlaylistType:
                Game.PlaylistType list = default;
                if (Enum.TryParse<Game.PlaylistType>(arg, true, out list))
                {
                    room.game.SetPlaylistType(list);
                }
                break;

            case Room.Option.PlaylistLink:
                room.game.SetPlaylistURL(arg);
                break;

            case Room.Option.MediaType:
                Game.MediaType media = default;
                if (Enum.TryParse<Game.MediaType>(arg, true, out media))
                {
                    room.game.SetMediaType(media);
                }
                break;

            case Room.Option.AnswerType:
                Game.AnswerType answer = default;
                if (Enum.TryParse<Game.AnswerType>(arg, true, out answer))
                {
                    room.game.SetAnswerType(answer);
                }
                break;

            case Room.Option.Time:
                int time = default;
                if (Int32.TryParse(arg, out time))
                {
                    room.game.SetTime(time);
                }
                break;

            case Room.Option.Rounds:
                int rounds = default;
                if(Int32.TryParse(arg, out rounds))
                {
                    room.game.SetRoundsAmount(rounds);
                }
                break;
        }
    }
    await _hubContext.Clients.Group(roomCode).UpdatePanel(room.game.playlistId, 
        room.game.playlistType.ToString(), 
        room.game.mediaType.ToString(), 
        room.game.answerType.ToString(), 
        room.game.time,
        room.game.roundCount);
    }
    catch
    {

    }
        }

        public async Task SendTestMessage(string message, string roomCode)
        {
            int i = 0;
            while (i < 5)
            {
                await _hubContext.Clients.Group(roomCode).ReceiveMessage("_infoBot", $"{i}", Util.GetHourFromDateTime(DateTime.Now));
                Thread.Sleep(1000);
                i++;
            }
        }

        public async Task InitGameState(string roomCode)
        {
            Room room = GetRoom(roomCode);
            room.ZeroPoints();
            await room.game.Init();
            await _hubContext.Clients.Group(roomCode).ReceiveMessage("_infoBot", "Game started.", Util.GetHourFromDateTime(DateTime.Now));
            await AdvanceGameState(roomCode);
        }

        public async Task AdvanceGameState(string roomCode)
        {
            Room room = GetRoom(roomCode);
            room.game.AdvanceGameState();

            var stage = room.GetStage();
            var round = room.GetRound();
            var ansType = room.GetAnsType();

            //next guessing round data
            string url = string.Empty;
            string[] answers = default;
            int correct = default;
            int time = default;

            string resultsData = string.Empty;

            if (round != 0 && stage == Game.GameStage.GuessRound)
            {
                url = room.GetURLForRound();
                answers = room.GetAnswersForRound();
                correct = room.GetCorrectAnswerForRound();
                time = room.GetTimePlayedForRound();
            }
            if(stage == Game.GameStage.RoundResults || stage == Game.GameStage.GameResults)
            {
                resultsData = room.GetResultsFromRound();
            }

            switch (stage)
            {
                case Game.GameStage.ConfigPanel:
                    await _hubContext.Clients.Group(roomCode).SetJoinedInProgress((int)stage, false);
                    break;
                case Game.GameStage.GuessRound:
                    await _hubContext.Clients.Group(roomCode).UpdateGameData((int)stage, 
                        round, url, 
                        (ansType==Game.AnswerType.Closed || (ansType==Game.AnswerType.Open && round == 1))?answers:null, 
                        correct, time, 
                        (ansType==Game.AnswerType.Open));
                    await _hubContext.Clients.Group(roomCode).ReceiveMessage("_infoBot", $"Round {round} started.", Util.GetHourFromDateTime(DateTime.Now));
                    break;
                case Game.GameStage.RoundResults:
                    await _hubContext.Clients.Group(roomCode).SendRoundResults((int)stage, resultsData);
                    await _hubContext.Clients.Group(roomCode).ReceiveMessage("_infoBot", $"Round {round} finished.", Util.GetHourFromDateTime(DateTime.Now));
                    break;
                case Game.GameStage.GameResults:
                    await _hubContext.Clients.Group(roomCode).SendEndResults((int)stage);
                    await _hubContext.Clients.Group(roomCode).ReceiveMessage("_infoBot", $"Game finished.", Util.GetHourFromDateTime(DateTime.Now));
                    break;
            }
        }

        public async Task<bool> ValidatePlaylist(string url, string roomCode)
        {
            Room room = GetRoom(roomCode);
            bool valid = await room.game.ValidatePlaylist(url);
            if (valid)
            {
                await _hubContext.Clients.Group(roomCode).UpdatePanel(room.game.playlistId, room.game.playlistType.ToString(), room.game.mediaType.ToString(), room.game.answerType.ToString(), room.game.time, room.game.roundCount);
            }
            return valid;
        }

        public async Task GuessAnswer(bool isCorrect, int secs, UserConnection userConnection)
        {
            Room room = GetRoom(userConnection.Room);
            room.AddPoints(userConnection, secs, isCorrect);
            await _hubContext.Clients.Group(userConnection.Room).ReceiveMessage("_infoBot", $"{userConnection.User} guessed.", Util.GetHourFromDateTime(DateTime.Now));
        }

        public void SetHubContext(IHubContext<MainHub, IMainHub>? context)
        {
            if (context != null)
                _hubContext = context;
        }

        public Manager()
        {

        }

    }
}
