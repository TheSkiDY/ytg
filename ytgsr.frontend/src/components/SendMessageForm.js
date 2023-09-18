import { useState } from "react";
import { Button, Form, FormControl, InputGroup } from "react-bootstrap";
import React from "react";

const SendMessageForm = ({ sendMessage }) => 
{
    const [message, setMessage] = useState();

    return <Form onSubmit={e => {
        e.preventDefault();
        sendMessage(message);
        setMessage('');
    }}>
        <Form.Group className="message-form">
            <div className="message-typein" >
                <FormControl placeholder="Send a message..." onChange={e => setMessage(e.target.value)} value={message}/>
            </div>
            <div >
                <Button className="chat-btn" variant="danger" type="submit" disabled={!message}>Chat</Button>
            </div>
        </Form.Group>
    </Form>
};

export default SendMessageForm;