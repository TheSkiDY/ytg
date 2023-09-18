import { useEffect, useRef } from "react";
import React from "react";

const ChatMessages = ({messages}) => {
    const messageRef = useRef();

    useEffect(() => {
        if(messageRef && messageRef.current)
        {
            const {scrollHeight, clientHeight} = messageRef.current;
            messageRef.current.scrollTo({left:0, top:scrollHeight-clientHeight, behavior:'smooth'});
        }
    }, [messages]);

    return <div ref={messageRef} className="message-container">
        {messages.map((m, index) =>
            <div key={index} className='user-message'>
                {m.user==="_infoBot"
                ?   <div className="message-bot">
                        {m.message}
                    </div>
                :   <div className="message">
                        <div className="time-in-message">{m.datetime}</div> {m.user}: {m.message}
                    </div>
                }
                
            </div>
        )}
    </div>
}

export default ChatMessages;