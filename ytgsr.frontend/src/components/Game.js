import { useState } from "react";
import Chat from "./Chat";
import GameConfigAdmin from "./GameConfigAdmin";
import GameConfigPlayer from "./GameConfigPlayer";
import PlayerList from "./PlayerList";
import Question from "./Question";
import RoundResults from "./RoundResults";
import Lounge from "./Lounge";
import EndResults from "./EndResults";  
import React from "react";

const Game = ({
    connection,
    messages, sendMessage, kickPlayer, 
    setAdmin, closeConnection, playerCount, 
    user, adminUuid, playerList, room, 
    gameOptions, setGameOptions, gameState,
    results, ansArray,
    joinedInProgress}) => {

    const [test, setTest] = useState("t");
    const advanceGameState = async() =>
    {
        try
        {
            await connection.invoke("AdvanceGameState");
        }
        catch(e)
        {
            console.log(e);
        }
    }

    return <div className="container">
        <PlayerList 
            connection={connection} user={user} adminUuid={adminUuid} kickPlayer={kickPlayer} setAdmin={setAdmin} playerList={playerList} room={room}
        />
        <div className="game-screen">
            {
                !joinedInProgress?
                { 
                    0: user.uuid == adminUuid
                        ?    <GameConfigAdmin 
                                connection={connection} 
                                gameOptions={gameOptions}
                                setGameOptions={setGameOptions}
                                ></GameConfigAdmin>
                        :    <GameConfigPlayer
                                gameOptions={gameOptions}
                            ></GameConfigPlayer>,
                    1: <Question 
                            connection={connection}
                            user={user} 
                            adminUuid={adminUuid} 
                            gameOptions={gameOptions} 
                            gameState={gameState} 
                            ansArray={ansArray}
                            advanceGameState={advanceGameState}/>,
                    2: <RoundResults 
                            user={user}
                            adminUuid={adminUuid} 
                            advanceGameState={advanceGameState}
                            gameState={gameState}
                            gameOptions={gameOptions}
                            ansArray={ansArray}
                            results={results}/>,
                    3: <EndResults 
                            user={user}
                            adminUuid={adminUuid}
                            results={results}
                            advanceGameState={advanceGameState}/>
                    
                }[gameState.Stage]
                :
                <Lounge></Lounge>
            }
        </div>
        <Chat messages={messages} sendMessage={sendMessage} closeConnection={closeConnection} playerCount={playerCount} />
    </div>
}

export default Game;