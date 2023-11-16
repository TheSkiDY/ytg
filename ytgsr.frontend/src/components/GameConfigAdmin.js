import { useRef, useState, useEffect } from "react";
import { Button, Form, FormCheck, FormControl, FormGroup, FormLabel } from "react-bootstrap";
import FormRange from "react-bootstrap/esm/FormRange";
import React from "react";


const GameConfigAdmin = ({ connection,
    gameOptions,
    setGameOptions,
    }) => {

    const [localLink, setLocalLink] = useState();
    const [startButtonDisabled, setStartButtonDisabled] = useState(false);
    const [validateButtonDisabled, setValidateButtonDisabled] = useState(false);
    const [validatingInProgress, setValidatingInProgress] = useState(false);
    const timeFormRef = useRef();
    const roundsFormRef = useRef();

    useEffect(() => {
        setValidatingInProgress(false);
    }, [gameOptions.validated]);

    const updateOptionInPanel = async (option, arg) =>
    {
        try
        {
            await connection.invoke("UpdateOptionInConfigPanel", option, arg);
        }
        catch (e)
        {
            console.log(e);
        }
    }

    const initGameState = async () =>
    {
        try
        {   
            await connection.invoke("InitGameState"); 
        }
        catch(e)
        {
            console.log(e);
        }
    }

    const validatePlaylist = async (url) =>
    {
        try
        {
            await connection.invoke("ValidatePlaylist", url);
        }
        catch (e)
        {
            console.log(e);
        }
    }

    useEffect(() => {
        if(timeFormRef && timeFormRef.current)
        {
            timeFormRef.current.value = gameOptions.Time;
        }
        if(roundsFormRef && roundsFormRef.current)
        {
            roundsFormRef.current.value = gameOptions.Rounds;
        }
    });

    const onChangeHandler = (e) => {
        const { name, value } = e.target;
        setGameOptions((prevState) => ({ ...prevState, [name]: value }));
        updateOptionInPanel(name, value);
        };    

    return <Form onSubmit={e => {
        e.preventDefault();
    }}
    >
        <div className="option-container">
            <FormLabel>
                Playlist type:
            </FormLabel>
            <FormGroup>
                <FormCheck inline label="Playlist" value='Playlist' type="radio" name="PlaylistType"
                    onChange={onChangeHandler}/>
                <FormCheck inline label="Channel" value='Channel' type="radio" name="PlaylistType"
                    onChange={onChangeHandler}/>
            </FormGroup>
        </div>
        <div className="option-container">
            <div className="playlist-typein" >
                <FormLabel>
                    {gameOptions.PlaylistType == 'Channel'?'Channel ID':'Playlist Link'}
                </FormLabel>
                <FormControl placeholder={gameOptions.PlaylistType == 'Channel'?'UCWTA5Yd0rAkQt5-9etIFoBA':"https://www.youtube.com/playlist?list=PL8A83124F1D79BD4F"} onChange={e=> {
                    setLocalLink(e.target.value);
                    setGameOptions((op) => ({...op, ["validated"]:false}))
                    setGameOptions((op) => ({...op, ["PlaylistLink"]:e.target.value}))
                    setValidateButtonDisabled(false);
                }} value={localLink} />
            </div>
            <div className="validate-container">
                <Button className='validate-btn' variant="info" disabled={!localLink || gameOptions.validated || validateButtonDisabled} onClick={e=> {
                    setValidateButtonDisabled(true);
                    validatePlaylist(localLink);
                    setValidatingInProgress(true);
                    setGameOptions((op) => ({...op, ["validated"]: 2}));
                    //setGameOptions((op) => ({...op, ["validated"]:true})); //this is for debugging
                    }}>
                    Validate
                </Button>
                <div className="validate-indicator"> 
                    {
                        validatingInProgress
                        ? '⌛'
                        : (gameOptions.validated)
                            ? '✔️'
                            : '❌'
                    }
                </div>
            </div>
        </div> 
        <div className="option-container">
            <FormLabel>
                Media type:
            </FormLabel>
            <FormGroup>
                <FormCheck inline label="Audio" value='Audio' type="radio" name='MediaType' 
                    onChange={onChangeHandler}/>
                <FormCheck inline label="Video" value='Video' type="radio" name='MediaType'
                    onChange={onChangeHandler}/>
                <FormCheck inline label="Thumbnail" value='Thumbnail' type="radio" name='MediaType'
                    onChange={onChangeHandler}/>
            </FormGroup> 
        </div>
        <div className="option-container">
            <FormLabel>
                Answer type:
            </FormLabel>
            <FormGroup>
                <FormCheck inline label="Choose between 4 options" value='Closed' type="radio" name='AnswerType'
                    onChange={onChangeHandler}/>
                <FormCheck inline label="Type an answer directly" value='Open' type="radio" name='AnswerType'
                    onChange={onChangeHandler}/>
            </FormGroup>
        </div>
        <div className="option-container">
            <FormLabel>
                {'Time for guess (' + gameOptions.Time + ' seconds)'}
            </FormLabel>
            <FormRange ref={timeFormRef} min='5' max='40' step='1' name="Time"
                onMouseUp={onChangeHandler}
                onChange={e=>{
                setGameOptions((op) => ({...op, ["Time"]:e.target.value}));
                }}>
                
            </FormRange>
        </div>
        <div className="option-container">
            <FormLabel>
                {'Amount of rounds per game (' + gameOptions.Rounds + ')'}
            </FormLabel>
            <FormRange ref={roundsFormRef} min='3' max='10' step='1' name="Rounds"
                onMouseUp={onChangeHandler}
                onChange={e=>{
                setGameOptions((op) => ({...op, ["Rounds"]:e.target.value, ["validated"]:false}));
                setValidateButtonDisabled(false);
                }}>
                
            </FormRange>
        </div>
        <div>
            <Button 
                className='game-start-btn' 
                variant='success' 
                type='submit' 
                disabled={!gameOptions.validated || gameOptions.MediaType=="" || gameOptions.AnswerType=="" || startButtonDisabled}
                onClick={e=>{
                    setStartButtonDisabled(true);
                    initGameState();
                }}>
                Launch
            </Button>
        </div>
    </Form>

}

export default GameConfigAdmin;