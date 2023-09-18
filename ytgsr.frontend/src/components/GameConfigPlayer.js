import { Button } from "react-bootstrap";
import React from "react";

const GameConfigPlayer = ({
    gameOptions,
}) => {

    return <div>
        <div>
            <h1>Player screen</h1>
        </div>
        <div>
            <h2>
                Playlist URL: {gameOptions.PlaylistLink}
            </h2>
        </div>
        <div>
            <h2>
                Media type: {gameOptions.MediaType}
            </h2>
        </div>
        <div>
            <h2>
                Answer type: {gameOptions.AnswerType == "Closed" ? "Choose between 4 options" : (gameOptions.AnswerType == "Open" ? "Type an answer directly" : "")}
            </h2>
        </div>
        <div>
            <h2>
                Time for round: {gameOptions.Time} seconds
            </h2>
        </div>


    </div>

}

export default GameConfigPlayer;