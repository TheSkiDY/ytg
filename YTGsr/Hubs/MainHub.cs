using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.ObjectPool;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace YTGsr.Hubs
{
    public interface IMainHub
    {
        Task ReceiveMessage(string user, string message, string date);
        Task UpdateAdmin(string username);
        Task UpdatePanel(string url, string playlistType, string mediaType, string answerType, int time, int roundCount);
        Task KickFromRoom();
        Task UpdatePlayersList(string[] players, string[] ids);
        Task UpdateMessagesUponJoin(string history);
        Task PlayersCount(int playerCount);
        Task SetJoinedInProgress(int stage, bool joined);
        Task UpdateGameData(int stage, int round, string urlRound, string[] answersRound, int properAnswer, int playTime, bool isOpen);
        Task SendRoundResults(int stage, string resultsData);
        Task SendEndResults(int stage);
        Task ValidateLink(bool validated);
    }

    public class MainHub : Hub<IMainHub>
    {
        private string _infoBot;
        private readonly IDictionary<string, UserConnection> connections;
        private readonly IManager manager;
        //private readonly IDictionary<string, Room> rooms;

        public async Task TryJoinRoom(string user, string uuid, string room)
        {
            UserConnection userConnection = new UserConnection()
            {
                User = user,
                Uuid = uuid,
                Room = room,
            };
            await Groups.AddToGroupAsync(Context.ConnectionId, userConnection.Room);
            connections[Context.ConnectionId] = userConnection;

            if (!manager.DoesRoomExist(userConnection.Room))
            {
                await CreateRoom(userConnection);
            }
            else
            {
                await JoinExistingRoom(userConnection);
            }
        }

        public async Task CreateRoom(UserConnection userConnection)
        {
            string roomCode = userConnection.Room;
            Room room = manager.CreateRoom(roomCode);
            room.AddPlayer(userConnection);

            await SetRoomAdmin(userConnection.Uuid);
            await Clients.Group(roomCode).ReceiveMessage(_infoBot, $"{userConnection.User} joined the room!", Util.GetHourFromDateTime(DateTime.Now));
            await SendConnectedUsers(userConnection.Room);
        }

        public async Task JoinExistingRoom(UserConnection userConnection)
        {
            string roomCode = userConnection.Room;
            Room room = manager.GetRoom(roomCode);

            room.AddPlayer(userConnection);
            var history = room.GetMessageHistory();
            string admin = room.GetAdminId();

            var stage = room.GetStage();

            await Clients.Caller.UpdateMessagesUponJoin(history);
            await Clients.Group(userConnection.Room).ReceiveMessage(_infoBot, $"{userConnection.User} joined the room!", Util.GetHourFromDateTime(DateTime.Now));
            await Clients.Group(userConnection.Room).UpdateAdmin(admin);
            await Clients.Caller.UpdatePanel(room.game.playlistId, room.game.playlistType.ToString(), room.game.mediaType.ToString(), room.game.answerType.ToString(), room.game.time, room.game.roundCount);
            await SendConnectedUsers(userConnection.Room);
            await Clients.Caller.SetJoinedInProgress((int)stage, stage != Game.GameStage.ConfigPanel);
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            if (connections.TryGetValue(Context.ConnectionId, out UserConnection userConnection))
            {
                string roomCode = userConnection.Room;
                connections.Remove(Context.ConnectionId);

                Clients.Group(roomCode).ReceiveMessage(_infoBot, $"{userConnection.User} left the room!", Util.GetHourFromDateTime(DateTime.Now));
                RemoveUserFromRoom(userConnection.Room, userConnection);
                SendConnectedUsers(roomCode);  
                if(manager.IsRoomEmpty(roomCode))
                {
                    manager.RemoveRoom(roomCode);
                }

            }

            return base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string message)
        {
            if (connections.TryGetValue(Context.ConnectionId, out UserConnection userConnection))
            {
                await manager.SendMessage(message, userConnection);
            }
        }


        public async Task KickPlayer(string id)
        {
            if(connections.TryGetValue(Context.ConnectionId, out UserConnection callerConnection))
            {
                try
                {
                    var userConnection = connections.Values.Where(c => c.Room == callerConnection.Room && c.Uuid == id).FirstOrDefault();
                    var connectionId = connections.FirstOrDefault(u => u.Value == userConnection).Key;
                    await Clients.Client(connectionId).KickFromRoom();
                }
                catch
                {

                }
            }
            
        }

        public Task RemoveUserFromRoom(string roomCode, UserConnection user)
        {
            return manager.RemoveUserFromRoom(roomCode, user);
        }

        public async Task SetRoomAdmin(string id)
        {
            if(connections.TryGetValue(Context.ConnectionId, out UserConnection callerConnection))
            {
                await manager.SetRoomAdmin(id, callerConnection.Room);
            }
        }

        public async Task SendConnectedUsers(string room)
        {
            await manager.SendConnectedUsers(room);
        }

        public async Task UpdateOptionInConfigPanel(string optionStr, string arg)
        {
            if(connections.TryGetValue(Context.ConnectionId, out UserConnection callerConnection))
            {
                await manager.UpdateOptionInConfigPanel(optionStr, arg, callerConnection.Room);
            }
        }

        public async Task InitGameState()
        {
            if(connections.TryGetValue(Context.ConnectionId, out UserConnection callerConnection))
            {
                await manager.InitGameState(callerConnection.Room);
            }
        }

        public async Task AdvanceGameState()
        {
            if(connections.TryGetValue(Context.ConnectionId, out UserConnection callerConnection))
            {
                await manager.AdvanceGameState(callerConnection.Room);
            }
        }

        public async Task ValidatePlaylist(string url)
        {
            if(connections.TryGetValue(Context.ConnectionId, out UserConnection callerConnection))
            {
                bool valid = await manager.ValidatePlaylist(url, callerConnection.Room);
                await Clients.Client(Context.ConnectionId).ValidateLink(valid);
            }
        }

        public async Task GuessAnswer(bool isCorrect, int secs)
        {
            if(connections.TryGetValue(Context.ConnectionId, out UserConnection callerConnection))
            {
                await manager.GuessAnswer(isCorrect, secs, callerConnection);
            }
        }

        public MainHub(IDictionary<string, UserConnection> connections, IManager manager)
        {
            _infoBot = "_infoBot";
            this.connections = connections;
            this.manager = manager;
        }
    }
}
