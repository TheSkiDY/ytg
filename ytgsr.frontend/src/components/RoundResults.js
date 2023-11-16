import { useEffect } from "react";
import { Button } from "react-bootstrap"
import React from "react";

const RoundResults = ({
    user,
    adminUuid,
    advanceGameState,
    gameState,
    gameOptions,
    ansArray,
    results,
}) =>
{

    useEffect(()=>{ const timer = setTimeout(() => {
            if(user.uuid==adminUuid)
            {
                advanceGameState();
            }
        },5000);
        return () => {
            clearTimeout(timer);
        }
    },[])

    const PrintPointsThisRound = function(pts)
    {
        if(pts > 0) return "+"+pts;
        else return pts;
    }

    return <div>
        <h1>Results:</h1>
        <h3> Correct answer: {gameOptions.AnswerType == 'Closed' ? gameState.Answers[gameState.ProperAnswer] : ansArray[gameState.ProperAnswer].label}</h3>
        <div className="results">
            {results.map((p, index)=>
                <div>
                    {'#' + (index+1)+') ' + p.Player + ' : ' + 
                    p.Points + ((p.PointsRound && p.PointsRound != 0) ? ('(' + PrintPointsThisRound(p.PointsRound) + ')'):(''))} 
                </div>)}
        </div>
        {user.uuid == adminUuid ?
        (
            <div> <Button  variant='primary' onClick={advanceGameState}>
                        {
                            gameState.Round == gameOptions.Rounds 
                            ? 'Final Results'
                            : 'Next Round'
                        } </Button></div>
        ):null}
    </div>
}

export default RoundResults;