import React, { useState } from "react";
import Select from 'react-select'

const AnswersOpen = ({
    gameState,
    ansArray,
    onAnswerClick
}) =>
{
    const [disabledInput, setDisabledInput] = useState(false);

    return <div>
        <Select
            onChange={(e) =>{
                console.log(e.value + ':' + e.label);
                onAnswerClick(e.value);
                setDisabledInput(true);
            }}
            options={ansArray}
            maxMenuHeight={130}
            isDisabled={disabledInput}
            autoFocus
        />
</div>

}

export default AnswersOpen;