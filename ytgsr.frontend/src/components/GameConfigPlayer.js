import { Button } from "react-bootstrap";
import React from "react";

const GameConfigPlayer = ({
    gameOptions,
}) => {

    return <div>
        <div>
            <h1>Current settings</h1>
        </div>
        <div>
            <h3>
                Playlist URL: {gameOptions.PlaylistLink}
            </h3>
        </div>
        <div>
            <h3>
                Media type: {gameOptions.MediaType}
            </h3>
        </div>
        <div>
            <h3>
                Answer type: {gameOptions.AnswerType == "Closed" ? "Choose between 4 options" : (gameOptions.AnswerType == "Open" ? "Type an answer directly" : "")}
            </h3>
        </div>
        <div>
            <h3>
                Time for round: {gameOptions.Time} seconds
            </h3>
        </div>


    </div>

}

export default GameConfigPlayer;