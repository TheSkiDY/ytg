import { Button } from "react-bootstrap"
import { useState } from "react"
import { Form } from "react-bootstrap"
import React from "react";

const Lobby = ({joinRoom}) => 
{
    const [user, setUser] = useState();
    const [room, setRoom] = useState();

    const [joinButtonDisabled, setJoinButtonDisabled] = useState(false);

    const setRandomRoomCode = () =>
    {
        const list = "ABCDEFGHIJKLMNPQRSTUVWXYZ";
        var res = "";
        for(var i = 0; i < 4; i++) {
            var rnd = Math.floor(Math.random() * list.length);
            res = res + list.charAt(rnd);
        }
        setRoom(res);
    }

    return <Form className="lobby" onSubmit={e => {
        e.preventDefault();
        setJoinButtonDisabled(true);
        joinRoom(user, room);
    }}>
        <Form.Group>
            <Form.Control placeholder="Name" onChange={e => setUser(e.target.value)}/>
            <div className="room-container">
                <Form.Control placeholder="Room Code" value={room} onChange={e => setRoom(e.target.value)}/>
                <div className="random-code" onClick={setRandomRoomCode}>🎲</div>
            </div>
        </Form.Group>
        <Button variant='success' type='submit' disabled={!user || !room || joinButtonDisabled}>Join Room</Button>
    </Form>
}

export default Lobby;