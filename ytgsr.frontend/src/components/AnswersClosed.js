import { useState } from "react";
import React from "react";


const AnswersClosed = ({
    gameState,
    activeHoverClass,
    onAnswerClick
}) =>
{
    const [disabledInput, setDisabledInput] = useState(false);
    const lettersArr = ["A","B","C","D"];

    // return <div>
    //     <DropdownInput 
    //         options={gameState.Answers}
    //         onSelect={(e)=>{
    //             if(e.index >= 0)
    //             {
    //                 setDisabledInput(true);
    //                 onAnswerClick(e.index);
    //             }
    //         }}
    //         placeholder='Answer...'
    //         max='5'
    //         maxText=''
    //         disabled={disabledInput}
    //     />
    // </div> 

    return <div className='answers-container'>
                {gameState.Answers.map((ans, index) =>
                    <div className={'ans ' + activeHoverClass +' ans-'+(index+1)} onClick={(e) => onAnswerClick(index)}>
                        {lettersArr[index]}. {ans}
                    </div>
                )}
            </div>
}


export default AnswersClosed;
