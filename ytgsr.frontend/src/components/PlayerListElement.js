import crown_icon from "../images/ytg_crown.png";
import kick_icon from "../images/ytg_kick.png";
import { Image } from "react-bootstrap";
import React from "react";

const PlayerListElement = ({user, activeUser, adminUuid, kickPlayer, setAdmin}) =>
{
    return <div className="player-list-element-container">
            <div className="player-list-element">
                <div className="player-list-name">
                    {       user.uuid == adminUuid ? <div className="admin-in-list">{user.name}</div>
                        :   <div className="player-in-list">{user.name}</div>
                    }
                </div>
                {activeUser.uuid == adminUuid && user.uuid != activeUser.uuid
                ?    <div className="player-list-icons">
                        <div className="player-list-crown">
                            <img className="img-crown" width={20} height={20} src={crown_icon} onClick={e => {
                                setAdmin(user.uuid);
                            }}/>
                        </div>
                        <div class="player-list-kick">
                            <img className="img-kick" width={20} height={20} src={kick_icon} onClick={e => {
                                kickPlayer(user.uuid);
                            }}/>
                        </div>
                    </div>
                :   <div></div>
                }
                
            </div>
        </div>
}

export default PlayerListElement;