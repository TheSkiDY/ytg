import { useEffect, useRef, useState } from "react";
import { Button } from "react-bootstrap";
import VideoEmbed from "./VideoEmbed";
import AnswersClosed from "./AnswersClosed";
import React from "react";
import AnswersOpen from "./AnswersOpen";


const Question = ({
    connection,
    user,
    adminUuid,
    gameOptions,
    gameState,
    ansArray,
    advanceGameState,
}) =>
{
    const [screenWidth, setScreenWidth] = useState('100px');
    const [screenHeight, setScreenHeight] = useState('100px');
    const [seconds, setSeconds] = useState(0);
    const [indexChosen, setIndexChosen] = useState();
    const [activeHoverClass, setActiveHoverClass] = useState('ans-hover');

    const screenRef = useRef();

    useEffect(() => {
        if(screenRef && screenRef.current)
        {
            const width = screenRef.current.offsetWidth;
            const height = (width*9)/16;
            setScreenWidth(width);
            setScreenHeight(height);
        }

        setSeconds(gameOptions.Time + 2);
        const interval = setInterval(() => {
            setSeconds(seconds => seconds - 1);
        }, 1000);
        
        const timer = setTimeout(() => {
            if(user.uuid == adminUuid)
            {
                advanceGameState();
            }
        }, (gameOptions.Time+2)*1000);

        return () => {
            clearInterval(interval);
            clearTimeout(timer);
        }
    }, [gameOptions.Time]);

    const onAnswerClick = (index) =>
    {
        if(activeHoverClass)
        {
            console.log('clicked at ' + index);
            const isCorrect = (index == gameState.ProperAnswer);
            console.log('isCorrect: ' + isCorrect + "|| seconds: " + seconds);
            setIndexChosen(index);
            setActiveHoverClass();
            Guess(isCorrect, seconds);
        }
    }

    const Guess = async(isCorrect, sec) => 
    {
        try
        {
            connection.invoke("GuessAnswer",isCorrect,sec);
        }
        catch(e)
        {
            console.log(e);
        }
    }


    return <div className="question-container">
        <div className="question-header">
            <div className="round-display">
                {
                    <h2>Round {gameState.Round}</h2>
                }
            </div>
            {user.uuid==adminUuid ?
            (
                <div className="skip-round-btn">
                    <Button 
                        variant="primary"
                        onClick={advanceGameState}>
                        Skip Round
                    </Button>
                </div>
            ): null}
            <div className='timer'>
                <span>
                    {
                        seconds<0
                        ? '00:00'
                        : '00:'+String(seconds).padStart(2,'0')
                    }
                </span>
            </div>
        </div>
        <div ref={screenRef} style={{minHeight: screenHeight}} className='video-screen'>
            {
                {
                    "Audio": <VideoEmbed gameOptions={gameOptions} gameState={gameState} width={screenWidth} height={screenHeight}></VideoEmbed>,
                    "Video": <VideoEmbed gameOptions={gameOptions} gameState={gameState} width={screenWidth} height={screenHeight}></VideoEmbed>,
                    "Thumbnail": 
                    <img src={'https://i.ytimg.com/vi/' + gameState.URL + '/sddefault.jpg'} height={screenHeight} width={screenWidth}> 
                    </img>
                }[gameOptions.MediaType]
            }
            
            
        </div>
        {gameOptions.AnswerType == "Closed"
            ?  <AnswersClosed 
                gameState={gameState}
                activeHoverClass={activeHoverClass}
                onAnswerClick={onAnswerClick} />
            : <AnswersOpen
                gameState={gameState}
                ansArray={ansArray}
                onAnswerClick={onAnswerClick} />
        }
        <div>
            Selected option: {indexChosen}
        </div>
    </div>
}

export default Question;