using System;
//using System.IO;
//using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
    Script: SAEArcadeCabinetInput
    Author: Gareth Lockett
    Version: 1.0
    Description:    
                    NOTES:
                            - Make sure old InputManager has been set up...

    [ TEST ]
    YELLOW = joystick 2
    BLUE = joystick 4
    RED = joystick 3
    GREEN = joystick 1
*/

public abstract class SAEArcadeCabinetInput
{
    // Events
    public static Action<int,int> joystickKeyCodePressedButton;         // <Joystick id, Button id> Invoked when a button has started to be pressed.
    public static Action<int, int> joystickKeyCodeHelddButton;          // <Joystick id, Button id> Invoked each frame while a button is held down.
    public static Action<int, int> joystickKeyCodeReleasedButton;       // <Joystick id, Button id> Invoked when a button has stopped been pressed.

    public static Action<int, Vector2> joystickAxisChanged;             // <Joystick id, Vector2 XY axis values> Invoked when an axis value changed.

    // Sub classes
    //public struct

    private class PlayerInput
    {
        public int playerId;
        public List<KeyCode> buttonsPressed = new List<KeyCode>();
        public Vector2 lastJoystickPosition;
    }
    private static PlayerInput[] playerInputs;

    // Constants
    public const int UNKNOWNPLAYER = -1, YELLOWPLAYER = 0, BLUEPLAYER = 1, REDPLAYER = 2, GREENPLAYER = 3;

    // Methods
    public static void ActivateInputEvents()
    {
        // HACK: Tap into the applications' onBeforeRender event to check inputs ;D
        Application.onBeforeRender -= SAEArcadeCabinetInput.InputUpdate;  Application.onBeforeRender += SAEArcadeCabinetInput.InputUpdate;
        Debug.Log( "[SAEArcadeCabinetInput] Input events ACTIVATED." );
    }
    public static void DeactivateInputEvents()
    {
        // HACK: Tap into the applications' onBeforeRender event to check inputs ;D
        Application.onBeforeRender -= SAEArcadeCabinetInput.InputUpdate;
        Debug.Log( "[SAEArcadeCabinetInput] Input events DEACTIVATED." );
    }
    private static void InputUpdate()
    {
        // Check buttons for events.
        SAEArcadeCabinetInput.CheckKeyCodeJoystickButtons();

        // Check axis for events.
        if( SAEArcadeCabinetInput.playerInputs != null )
        {
            for( int i = 0; i < SAEArcadeCabinetInput.playerInputs.Length; i++ )
            {
                SAEArcadeCabinetInput.JoyStickAxis( i +1 ); // X axis.
                SAEArcadeCabinetInput.JoyStickAxis( i +1, false ); // Y axis.
            }
        }
    }
    
    // Called internally to check if any buttons have been pressed/held/released (For invoking events)
    private static void CheckKeyCodeJoystickButtons()
    {
        if( SAEArcadeCabinetInput.joystickKeyCodePressedButton == null ) { return; } // Nothing listening for events.

        // Check there are player inputs set up!
        if( SAEArcadeCabinetInput.playerInputs == null )
        {
            SAEArcadeCabinetInput.playerInputs = new PlayerInput[ 4 ];
            for( int i=0; i< SAEArcadeCabinetInput.playerInputs.Length; i++ )
            {
                SAEArcadeCabinetInput.playerInputs[ i ] = new PlayerInput();
                playerInputs[ i ].playerId = SAEArcadeCabinetInput.UNKNOWNPLAYER;
            }
        }

        int numOfButtons = 19;

        // Joystick 1 buttons .. 350
        for( int b = ( int ) KeyCode.Joystick1Button0; b < ( int ) KeyCode.Joystick2Button0 + numOfButtons; b++ ) { SAEArcadeCabinetInput.CheckKeyCodeButton( ( KeyCode ) b ); }

        // Joystick 2 buttons .. 370
        for( int b = ( int ) KeyCode.Joystick2Button0; b < ( int ) KeyCode.Joystick2Button0 + numOfButtons; b++ ) { SAEArcadeCabinetInput.CheckKeyCodeButton( ( KeyCode ) b ); }

        // Joystick 3 buttons .. 390
        for( int b = ( int ) KeyCode.Joystick3Button0; b < ( int ) KeyCode.Joystick3Button0 + numOfButtons; b++ ) { SAEArcadeCabinetInput.CheckKeyCodeButton( ( KeyCode ) b ); }

        // Joystick 4 buttons .. 410
        for( int b = ( int ) KeyCode.Joystick4Button0; b < ( int ) KeyCode.Joystick4Button0 + numOfButtons; b++ ) { SAEArcadeCabinetInput.CheckKeyCodeButton( ( KeyCode ) b ); }

    }
    // Called internally to check if a specific button has been pressed/held/released (For invoking events)
    private static void CheckKeyCodeButton( KeyCode keyCode )
    {
        if( SAEArcadeCabinetInput.playerInputs == null ) { return; }

        // Get the joystick id from the keyCode (Eg Joystick1Button0 == 1)
        if( keyCode.ToString().StartsWith( "Joystick" ) == false ) { return; }
        string[] tmpStrParts = keyCode.ToString().Replace( "Joystick", "" ).Replace( "Button", "," ).Split( ",".ToCharArray() );
        if( tmpStrParts.Length != 2 ) { return; }
        int joystickId = int.Parse( tmpStrParts[ 0 ] );
        int buttonId = int.Parse( tmpStrParts[ 1 ] );

        // Check joystick id is between 1 and 4 (inclusive)
        if( joystickId < 1 || joystickId > SAEArcadeCabinetInput.playerInputs.Length ) { return; }

        // Check if the key is pressed.
        if( Input.GetKey( keyCode ) == true )
        {
            // Check if key is been held down or just pressed for the first time.
            if( SAEArcadeCabinetInput.playerInputs[ joystickId  - 1 ].buttonsPressed.Contains( keyCode ) == false )
            {
                // Invoke pressed event.
                SAEArcadeCabinetInput.joystickKeyCodePressedButton?.Invoke( joystickId, buttonId );
                SAEArcadeCabinetInput.playerInputs[ joystickId - 1 ].buttonsPressed.Add( keyCode ); // Add keyCode to buttons pressed.
            }
            else
            {
                // Invoke holding event.
                SAEArcadeCabinetInput.joystickKeyCodeHelddButton?.Invoke( joystickId, buttonId );
            }
        }
        else if( SAEArcadeCabinetInput.playerInputs[ joystickId - 1 ].buttonsPressed.Contains( keyCode ) == true )
        {
            // Invoke release event.
            SAEArcadeCabinetInput.joystickKeyCodeReleasedButton?.Invoke( joystickId, buttonId );
            SAEArcadeCabinetInput.playerInputs[ joystickId - 1 ].buttonsPressed.Remove( keyCode ); // Remove keyCode from buttons pressed.
        }
    }


    // Simple Button check via Input.GetKey (Call this when polling button presses)
    public static bool JoyStickButton( KeyCode keyCode ) { return Input.GetKey( keyCode ); }
    public static bool JoyStickButtonDown( KeyCode keyCode ) { return Input.GetKeyDown( keyCode ); } // Same as above but for keys pressed this frame/update only.

    // Simple Axis check (NOTE: Make sure the Joystick{ID}_xAxis and Joystick{ID}_yAxis axis are set up in the InputManager)
    // Returns the current axis value for a joystick (Call this when polling axis values)
    public static float JoyStickAxis( int joystickId, bool xAxis = true )
    {
        // Create the axis name string (Eg Joystick1_xAxis)
        string axisName = "Joystick" + joystickId + "_";
        if( xAxis == true ) { axisName += "xAxis"; } else { axisName += "yAxis"; }
        
        // Get the axis value.
        float axisValue = Input.GetAxis( axisName );

        // Check last joystick position (For invoking events if joystick position has changed)
        if( SAEArcadeCabinetInput.playerInputs != null )
        {
            if( joystickId > 0 && joystickId < SAEArcadeCabinetInput.playerInputs.Length )
            {
                if( xAxis == true )
                {
                    // Horizontal axis.
                    if( SAEArcadeCabinetInput.playerInputs[ joystickId ].lastJoystickPosition.x != axisValue )
                    {
                        SAEArcadeCabinetInput.playerInputs[ joystickId ].lastJoystickPosition.x = axisValue;
                        SAEArcadeCabinetInput.joystickAxisChanged?.Invoke( joystickId, SAEArcadeCabinetInput.playerInputs[ joystickId ].lastJoystickPosition );
                    }
                }
                else
                {
                    // Vertical axis.
                    if( SAEArcadeCabinetInput.playerInputs[ joystickId ].lastJoystickPosition.y != axisValue )
                    {
                        SAEArcadeCabinetInput.playerInputs[ joystickId ].lastJoystickPosition.y = axisValue;
                        SAEArcadeCabinetInput.joystickAxisChanged?.Invoke( joystickId, SAEArcadeCabinetInput.playerInputs[ joystickId ].lastJoystickPosition );
                    }
                }
            }
        }

        return axisValue;
    }

    //private enum ConfigurationState { NOTCONFIGURING, CONFIGURING_YELLOWPLAYER, CONFIGURING_BLUEPLAYER, CONFIGURING_REDPLAYER, CONFIGURING_GREENPLAYER }
    //private static ConfigurationState configuringState;
    public static void ConfigureJoysticks()
    {
        // Sanity check.
        //if( SAEArcadeCabinetInput.configuringState != ConfigurationState.NOTCONFIGURING )
        //    { Debug.LogWarning( "[SAEArcadeCabinetInput] already in the process of configuring!" ); return; }

        // HACK: Tap into the applications' onBeforeRender event to do joystick configuration ;D
        Application.onBeforeRender -= SAEArcadeCabinetInput.ConfiguringJoysticks; Application.onBeforeRender += SAEArcadeCabinetInput.ConfiguringJoysticks;
    }
    private static void ConfiguringJoysticks()
    {
        // Called each frame/update.

        //Debug.Log( "Configuring... " + SAEArcadeCabinetInput.configuringState );

        // Call this here to draw at the end of the frame (Eg draw over everything else!)
        //SAEArcadeCabinetInput.DrawScreen();

        // INSTANTIATE A TEMP GAMEOBJECT + COMPONENT FOR UPDATE()
        // Application.logMessageReceived << SEND log messages as msg pump mechanisim??
    }
    /*
    private static IEnumerator DrawScreen()
    {
        yield return new WaitForEndOfFrame();

        switch( SAEArcadeCabinetInput.configuringState )
        {
            case ConfigurationState.NOTCONFIGURING:
                Graphics.DrawTexture( new Rect( 0f, 0f, Screen.width, Screen.height ), Texture2D.blackTexture );
                Debug.Log( "Draw black texture" );
                break;

            case ConfigurationState.CONFIGURING_YELLOWPLAYER:
                break;

            case ConfigurationState.CONFIGURING_BLUEPLAYER:
                break;

            case ConfigurationState.CONFIGURING_REDPLAYER:
                break;

            case ConfigurationState.CONFIGURING_GREENPLAYER:
                break;
        }
    }
    */

    public class TestMB : MonoBehaviour // Attach to any gameobect it the scene?... or make an empty?
    {

    }
}
