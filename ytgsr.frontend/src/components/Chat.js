import { Button } from "react-bootstrap";
import ChatMessages from "./ChatMessages";
import SendMessageForm from "./SendMessageForm";
import React from "react";

const Chat = ({messages, sendMessage, closeConnection, playerCount}) => 
{

   return <div className="chat-container">
        <div className="leave-room">
            <h5 className="player-counter">Players: {playerCount}</h5>
            <Button variant="primary" onClick={() => closeConnection()}>Leave Room</Button>
        </div>
        <div className="chat">
            <ChatMessages messages={messages} />
            <SendMessageForm sendMessage={sendMessage} />
        </div>
    </div>

}

export default Chat;