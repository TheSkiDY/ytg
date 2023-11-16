
import 'bootstrap/dist/css/bootstrap.min.css';
import ReactPlayer from 'react-player';
import Audio from '../images/Audio.png';      
import React from "react";

const VideoEmbed = ({
    gameOptions,
    gameState,
    width,
    height,
}) =>
{
    return  <div class='screen-container'>
        <ReactPlayer
            width={width}
            height={height}
            url={'https://www.youtube.com/watch?v='+gameState.URL+'?t='+gameState.PlayTime}
            playing={true}
        ></ReactPlayer>
        {
            {
                "Audio": <div class='curtain' style={{width:width, minHeight:(height)}}>
                    <img src={Audio} width={width} height={height} />
                </div>,
                "Video": <div class='curtain' style={{width:width, minHeight:(height)}}></div>
            }[gameOptions.MediaType]
        }
    </div>

}

export default VideoEmbed;

