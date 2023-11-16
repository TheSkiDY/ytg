import { Button } from "react-bootstrap"
import React from "react";


const EndResults = ({
    user,
    adminUuid,
    advanceGameState,
    results,
}) =>
{
    return <div>
        <h1>Final Results:</h1>
        <div className="results">
            {results.map((p)=>
                <div>
                    {p.Player + ' : ' + p.Points} 
                </div>
            )}
        </div>
        {user.uuid == adminUuid ?
        (
            <Button 
                variant='primary'
                onClick={advanceGameState}>
                Back to config
            </Button>
        ):null}
    </div>
}

export default EndResults;