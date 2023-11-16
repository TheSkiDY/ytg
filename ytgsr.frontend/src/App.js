import logo from './logo.svg';
import './App.css';
import 'bootstrap/dist/css/bootstrap.min.css';
import Lobby from './components/Lobby';
import Chat from './components/Chat';
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { useState } from 'react';
import Game from './components/Game';
import Logo from './components/Logo';
import React from 'react';

const App = () =>
{
  const [connection, setConnection] = useState();
  const [messages, setMessages] = useState([]);
  const [playerCount, setPlayerCount] = useState();
  const [room, setRoom] = useState();
  const [playerList, setPlayerList] = useState([]);
  const [gameOptions, setGameOptions] = useState({"PlaylistLink": "", "PlaylistType":"", "MediaType":"", "AnswerType":"", "Time":15, "Rounds":5, "validated":false});
  const [gameState, setGameState] = useState({"Stage": 0, "Round": 1, "URL": "", "Answers":["AnswerA","AnswerB","AnswerC","AnswerD"], "ProperAnswer":0, "PlayTime":0}); 
  const [ansArray, setAnsArray] = useState([]);
  const [joinedInProgress, setJoinedInProgress] = useState(false);
  const [results, setResults] = useState({});
  const [user, setUser] = useState({"name":"", "uuid":""});
  const [adminUuid, setAdminUuid] = useState();

  const getRandomUuid = function()
  {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
      const r = Math.random() * 16 | 0,
        v = c == 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    })
  }

  const joinRoom = async (user, room) =>
  {
    try
    {
      const connection = new HubConnectionBuilder()
        .withUrl("https://localhost:44363/room")
        .configureLogging(LogLevel.Information)
        .build();

      connection.on("PlayersCount", (count) => {
        setPlayerCount(count);
      })

      connection.on("UpdatePlayersList", (users, ids) => {
        setPlayerList([]);
        for(let i = 0; i < users.length; i++)
        {
          console.log('user:' + users[i] + "/// uuid: " + ids[i]);
          setPlayerList(playerList => [...playerList, {['name']:users[i], ['uuid']:ids[i]}]);
        }
      })

      connection.on("ReceiveMessage", (user, message, datetime) => {
        setMessages(messages => [...messages, {user, message, datetime}]);
      });

      connection.on("UpdateAdmin", (id) => {
        console.log('admin ustawiony na: ' + id);
        setAdminUuid(id);
      });

      connection.on("KickFromRoom", () => {
        connection.stop();
      });

      connection.on("UpdateMessagesUponJoin", (history) => {
        var fullMessages = history.split(';');
        fullMessages = fullMessages.slice(0, -1);
        fullMessages.forEach(msg => {
            var msgSegments = msg.split(',');
            var user = msgSegments[0];
            var message = msgSegments[1];
            var datetime = msgSegments[2];
            setMessages(messages => [...messages, {user, message, datetime}]);
        });
      });

      connection.on("UpdatePanel", (url, playlisttype, mediatype, answertype, time, rounds) => {
        setGameOptions((op) => ({ ...op, ["PlaylistLink"]: url, ["PlaylistType"]:playlisttype, ["MediaType"]:mediatype, ["AnswerType"]:answertype, ["Time"]:time, ["Rounds"]:rounds }));
      })

      connection.on("ValidateLink", (validated) =>
      {
          console.log('validated?: ' + validated);
          setGameOptions((op)=>({...op, ["validated"]:validated}));
      })

      connection.on("SetJoinedInProgress", (stage, result)=> {
        setGameState({"Stage": stage});
        setJoinedInProgress(result);
      });

      connection.on("UpdateGameData", (stage, round, urlRound, answersRound, properAnswer, playTime, isOpen) => {
        if(isOpen)
        {
          if(round == 1)
          {
            setAnsArray([]);
            for (let i = 0; i < answersRound.length; i++) {
              setAnsArray(ansArray => [...ansArray, {['value']:i, ['label']:answersRound[i]}]);
            }
          }
          setGameState((st)=>({...st, ['Stage']:stage, ['Round']:round, ['URL']:urlRound, ['ProperAnswer']:properAnswer, ['PlayTime']:playTime}));
        }
        else
        {
          setGameState((st)=>({...st, ['Stage']:stage, ['Round']:round, ['URL']:urlRound, ['Answers']:answersRound, ['ProperAnswer']:properAnswer, ['PlayTime']:playTime}));
        }
      })
      
      connection.on("SendRoundResults", (stage, resultsData) => {
        setResults([]);
        setGameState((st)=>({...st, ['Stage']:stage}));
        var fullRows = resultsData.split(';');
        fullRows = fullRows.slice(0, -1);
        fullRows.forEach(row => {
          var rowSegments = row.split(',');
          var player = rowSegments[0];
          var points = rowSegments[1];
          var pointsRound = rowSegments[2];
          setResults(results => [...results, {['Player']:player, ['Points']:points, ['PointsRound']:pointsRound}]);
        });
    });

    connection.on("SendEndResults", (stage) => {
        setGameState((st) => ({...st, ['Stage']:stage}));
    });

      connection.onclose(e => {
        setConnection();
        setMessages([]);
        setPlayerCount();
        setGameOptions({"PlaylistLink": "", "MediaType":"", "AnswerType":"", "Time":15, "Rounds":5, "validated":false});
        setGameState({"Stage": 0, "Round": 1, "URL": "", "Answers":["AnswerA","AnswerB","AnswerC","AnswerD"], "ProperAnswer":0, "PlayTime":0});
        setRoom();
        setResults([]);
        setUser();
      })

      await connection.start();
      var uuid = getRandomUuid();
      await connection.invoke("TryJoinRoom", user, uuid, room);
      setUser({['name']:user, ['uuid']:uuid});
      setConnection(connection);
      setRoom(room);
    }
    catch(e)
    {
      console.log(e);
    }
  }

  const closeConnection = async () => 
  {
    try 
    {
      await connection.stop();
    }
    catch(e)
    {
      console.log(e);
    }
  }

  const sendMessage = async (message) => 
  {
    try 
    {
      await connection.invoke("SendMessage", message);
    }
    catch (e) 
    {
      console.log(e);
    }
  }

  const setAdmin = async (userId) =>
  {
    try
    {
      await connection.invoke("SetRoomAdmin", userId);
    }
    catch (e)
    {
      console.log(e);
    }
  }

  const kickPlayer = async (userId) =>
  {
    try
    {
      await connection.invoke("KickPlayer", userId);
    }
    catch (e)
    {
      console.log(e);
    }
  }

  
  

  return <div className='app'>
    <Logo />
    {!connection 
    ?   <Lobby joinRoom={joinRoom} />
    :   <Game 
          connection={connection}
          messages={messages} 
          sendMessage={sendMessage} 
          closeConnection={closeConnection} 
          kickPlayer={kickPlayer} 
          setAdmin={setAdmin} 
          playerCount={playerCount} 
          user={user}
          adminUuid={adminUuid}
          playerList={playerList}
          gameOptions={gameOptions}
          setGameOptions={setGameOptions}
          gameState={gameState}
          joinedInProgress={joinedInProgress}
          results={results}
          ansArray={ansArray}
          room={room}/>
    }
  </div>
}

export default App;
