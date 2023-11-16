import { useState } from "react";
import React from "react";


const AnswersClosed = ({
    gameState,
    activeHoverClass,
    onAnswerClick
}) =>
{
    const lettersArr = ["A","B","C","D"];
    return <div className='answers-container'>
                {gameState.Answers.map((ans, index) =>
                    <div className={'ans ' + activeHoverClass +' ans-'+(index+1)} onClick={(e) => onAnswerClick(index)}>
                        {lettersArr[index]}. {ans}
                    </div>
                )}
            </div>
}


export default AnswersClosed;
