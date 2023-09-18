import { useEffect, useState } from "react";
import PlayerListElement from "./PlayerListElement";
import React from "react";

const PlayerList = ({connection, user, adminUuid, kickPlayer, setAdmin, playerList}) => 
{
   return <div className="player-list-container">
        <div className="player-list">
            {playerList.map((player, index) =>
                <PlayerListElement key={index} user={player} activeUser={user} adminUuid={adminUuid} kickPlayer={kickPlayer} setAdmin={setAdmin} />
            )};
        </div>
    </div>

}

export default PlayerList;