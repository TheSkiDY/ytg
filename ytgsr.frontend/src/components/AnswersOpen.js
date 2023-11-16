import React, { useState } from "react";
import Select from 'react-select'

const AnswersOpen = ({
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
            maxMenuHeight={100}
            isDisabled={disabledInput}
            autoFocus
        />
</div>

}

export default AnswersOpen;