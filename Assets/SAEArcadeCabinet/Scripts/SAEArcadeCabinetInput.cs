using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;


/*
    Script: SAEArcadeCabinetInput
    Author: Gareth Lockett
    Version: 1.0
    Description:    
                    NOTES:
                            - Make sure old InputManager has been set up...
*/

public abstract class SAEArcadeCabinetInput //: MonoBehaviour
{
    // Events
    //public static Action<SAEArcadeCabinetPlayerControls,SAEArcadeCabinetButton> playerPressedButton;

    public static Action<int,int> joystickKeyCodePressedButton; // <Joystick id, Button id> Started pressing
    public static Action<int, int> joystickKeyCodeHelddButton; // <Joystick id, Button id> Holding down
    public static Action<int, int> joystickKeyCodeReleasedButton; // <Joystick id, Button id> Stopped pressing

    public static Action<int, Vector2> joystickAxisChanged;         // <Joystick id, Vector2 XY axis values>

    /*
    // Sub classes
    [Serializable]
    public class SAEArcadeCabinetPlayerControls
    {
        // Properties
        public SAEArcadeCabinetColorId colorId;
        public sbyte joystickId = 1; // 1, 2, 3, 4
        public sbyte[] buttonIds;

        // Constructor
        public SAEArcadeCabinetPlayerControls( SAEArcadeCabinetColorId colorId )
        {
            this.colorId = colorId;
            this.joystickId = -1;
            this.buttonIds = new sbyte[ Enum.GetNames( typeof( SAEArcadeCabinetButton ) ).Length ];
            for( int i = 0; i < this.buttonIds.Length; i++ ) { this.buttonIds[ i ] = ( sbyte ) i; }
        }

        // Methods
        public int GetButtonId( SAEArcadeCabinetButton buttonId )
        {
            int id = ( int ) buttonId;
            if( id < 0 || id >= this.buttonIds.Length ) { return -1; }
            return this.buttonIds[ id ];
        }

        public void CheckEvents()
        {
            // Sanity checks.
            if( this.colorId == SAEArcadeCabinetColorId.UNKNOWN ) { return; }
            if( this.joystickId < 1 ) { return; }
            if( this.buttonIds == null ) { return; }
            if( this.buttonIds.Length == 0 ) { return; }

            // Check for button presses. Invoke an event if button is found to be pressed.
            if( SAEArcadeCabinetInput.playerPressedButton != null ) // Make sure something is listening for playerPressedButton events.
            {
                for( int b=0; b<this.buttonIds.Length; b++ )
                {
                    if( this.buttonIds[ b ] < 0 ) { continue; }
                    string buttonName = "Joystick" + this.joystickId.ToString() + "_Button_" + this.buttonIds[ b ].ToString();
                    //if( SAEArcadeCabinetInput.IsButtonAvailable( buttonName ) == false ) { continue; }
                    if( Input.GetButton( buttonName ) == true )
                        { SAEArcadeCabinetInput.playerPressedButton.Invoke( this, ( SAEArcadeCabinetButton ) this.buttonIds[ b ] ); }
                }
            }
        }
    }
    [Serializable]
    private class SAEArcadeCabinetAllInputControls { public SAEArcadeCabinetPlayerControls[] playercontrols; }

    // Enumerators
    public enum SAEArcadeCabinetColorId { UNKNOWN, YELLOW, BLUE, RED, GREEN }
    public enum SAEArcadeCabinetButton { BUTTON0 = 0, BUTTON1 = 1, BUTTON2 = 2, BUTTON3 = 3, BUTTON4 = 4, BUTTON5 = 5, BUTTON6 = 6, BUTTON7 = 7 }

    // Properties
    private static SAEArcadeCabinetAllInputControls saeArcadeCabinetAllInputControls;           // The currently loaded controls for all 4 players (null if not loaded yet)
    */

    // Methods
    /*
    public SAEArcadeCabinetInput() // Constructor .. used?
    {
        // Check if the globally accessible inputs have already been initialized.
        if( SAEArcadeCabinetInput.saeArcadeCabinetAllInputControls == null ) { SAEArcadeCabinetInput.InitializeInputs(); }

        //Debug.Log( "CONSTRUCTOR" );
    }
    */

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
        /*
        // Check there are saeArcadeCabinetAllInputControls set up.
        if( SAEArcadeCabinetInput.saeArcadeCabinetAllInputControls == null )
            { SAEArcadeCabinetInput.InitializeInputs(); if( SAEArcadeCabinetInput.saeArcadeCabinetAllInputControls == null ) { return; } }
        if( SAEArcadeCabinetInput.saeArcadeCabinetAllInputControls.playercontrols == null )
            { SAEArcadeCabinetInput.InitializeInputs(); if( SAEArcadeCabinetInput.saeArcadeCabinetAllInputControls.playercontrols == null ) { return; } }
            */

        // Loop through player controls checking for events.
        //for( int i=0; i<SAEArcadeCabinetInput.saeArcadeCabinetAllInputControls.playercontrols.Length; i++ )
        //{
        //    if( SAEArcadeCabinetInput.saeArcadeCabinetAllInputControls.playercontrols[ i ] == null ) { continue; } //?
        //    SAEArcadeCabinetInput.saeArcadeCabinetAllInputControls.playercontrols[ i ].CheckEvents();
        //}

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
    
    /*
    private static void InitializeInputs()
    {
        // Attempt to load SAEArcadeCabinet.config JSON file from data path.
        string configFileName = "SAEArcadeCabinetConfig.txt";
        if( SAEArcadeCabinetInput.LoadConfigurationFile( Application.dataPath + Path.DirectorySeparatorChar + configFileName ) == false )
        {
            // If could not load from data path, try persistent data path.
            if( SAEArcadeCabinetInput.LoadConfigurationFile( Application.persistentDataPath + Path.DirectorySeparatorChar + configFileName ) == false )
            {
                // Could not find a config file. Manually create SAEArcadeCabinetInput.saeArcadeCabinetAllInputControls.
                
                
                // Do this from controller input manager....?
                SAEArcadeCabinetInput.saeArcadeCabinetAllInputControls = new SAEArcadeCabinetAllInputControls();

                string[] joystickNames = Input.GetJoystickNames();
                SAEArcadeCabinetInput.saeArcadeCabinetAllInputControls.playercontrols = new SAEArcadeCabinetPlayerControls[ joystickNames.Length ];
                for( int j=0; j < joystickNames.Length; j++ )
                {
                    //if( j == SAEArcadeCabinetInput.saeArcadeCabinetAllInputControls.playercontrols.Length ) { break; }
                    Debug.Log( "Set up joystick: " +joystickNames[ j ] );
                    SAEArcadeCabinetInput.saeArcadeCabinetAllInputControls.playercontrols[ j ] = new SAEArcadeCabinetPlayerControls( ( SAEArcadeCabinetColorId ) ( j + 1 ) );
                    SAEArcadeCabinetInput.saeArcadeCabinetAllInputControls.playercontrols[ j ].joystickId = ( sbyte ) ( j + 1 );
                }

                // Create new SAEArcadeCabinet.config file.
                File.WriteAllText( Application.dataPath + Path.DirectorySeparatorChar + configFileName, JsonUtility.ToJson( SAEArcadeCabinetInput.saeArcadeCabinetAllInputControls, true ) );

                Debug.Log( "[SAEArcadeCabinetInput] Created config file: " + Application.dataPath + Path.PathSeparator + configFileName );
#if UNITY_EDITOR
                UnityEditor.AssetDatabase.Refresh();
#endif


            }
            else { Debug.Log( "[SAEArcadeCabinetInput] Config file loaded OK: " + Application.persistentDataPath + Path.PathSeparator + configFileName ); }
        }
        else { Debug.Log( "[SAEArcadeCabinetInput] Config file loaded OK: " + Application.dataPath + Path.PathSeparator + configFileName ); }
    }
    */

    /*
    private static bool LoadConfigurationFile( string path )
    {
        // Sanity check.
        if( File.Exists( path ) == false ) { return false; }

        // Load the config file.
        string configStr = File.ReadAllText( path );
        if( string.IsNullOrEmpty( configStr ) == true ) { Debug.LogWarning( "[SAEArcadeCabinetInput] Config file empty?! " + path ); return false; }
        SAEArcadeCabinetInput.saeArcadeCabinetAllInputControls = JsonUtility.FromJson<SAEArcadeCabinetAllInputControls>( configStr );

        return true;
    }

    private static SAEArcadeCabinetPlayerControls GetPlayerControlsByColorId( SAEArcadeCabinetColorId colorId )
    {
        // Check there are saeArcadeCabinetAllInputControls set up.
        if( SAEArcadeCabinetInput.saeArcadeCabinetAllInputControls == null )
            { SAEArcadeCabinetInput.InitializeInputs(); if( SAEArcadeCabinetInput.saeArcadeCabinetAllInputControls == null ) { return null; } }
        if( SAEArcadeCabinetInput.saeArcadeCabinetAllInputControls.playercontrols == null )
            { SAEArcadeCabinetInput.InitializeInputs(); if( SAEArcadeCabinetInput.saeArcadeCabinetAllInputControls.playercontrols == null ) { return null; } }

        // Find the player controls that match the color id.
        foreach( SAEArcadeCabinetPlayerControls playerControls in SAEArcadeCabinetInput.saeArcadeCabinetAllInputControls.playercontrols )
        {
            if( playerControls == null ) { continue; }
            if( playerControls.colorId == colorId ) { return playerControls; }
        }

        return null;
    }

    public static bool IsButtonPressed( SAEArcadeCabinetColorId colorId, SAEArcadeCabinetButton buttonId )
    {
        // Check there are saeArcadeCabinetAllInputControls set up.
        if( SAEArcadeCabinetInput.saeArcadeCabinetAllInputControls == null )
            { SAEArcadeCabinetInput.InitializeInputs(); if( SAEArcadeCabinetInput.saeArcadeCabinetAllInputControls == null ) { return false; } }
        if( SAEArcadeCabinetInput.saeArcadeCabinetAllInputControls.playercontrols == null )
            { SAEArcadeCabinetInput.InitializeInputs(); if( SAEArcadeCabinetInput.saeArcadeCabinetAllInputControls.playercontrols == null ) { return false; } }

        // Get the controls that match the color id.
        SAEArcadeCabinetPlayerControls playerControls = SAEArcadeCabinetInput.GetPlayerControlsByColorId( colorId );
        if( playerControls == null ) { Debug.LogWarning( "[SAEArcadeCabinetInput] Could not get player controls for colorId: " +colorId.ToString() ); return false; }

        // Get the actual configured button id.
        int bId = playerControls.GetButtonId( buttonId );
        if( bId < 0 ) { return false; }

        // Check the button. Joystick1_Button_0
if( playerControls.joystickId <= 0 ) { playerControls.joystickId = 1; } // TEST HACK
        string buttonName = "Joystick" + playerControls.joystickId.ToString() + "_Button_" + bId.ToString();
        //if( SAEArcadeCabinetInput.IsButtonAvailable( buttonName ) == false ) { return false; }
        if( Input.GetButton( buttonName ) == true ) { return true; }
        
        return false;
    }
    */

    /*
    // Not great using try/catch :P
    private static bool IsAxisAvailable( string axisName )
    {
        try { Input.GetAxis( axisName ); return true; }
        catch( UnityException exception ){ return exception.Message == string.Empty; }
    }
    private static bool IsButtonAvailable( string btnName )
    {
        try { Input.GetButton( btnName ); return true; }
        catch( UnityException exception ){ return exception.Message == string.Empty; }
    }
    */
    
// Simple KeyCode buttons /////////////////////////////////////////////////////////////////////////
    private class PlayerInput
    {
        //public int playerId;
        public List<KeyCode> buttonsPressed = new List<KeyCode>();
        public Vector2 lastJoystickPosition;
    }
    private static PlayerInput[] playerInputs;
    
    private static void CheckKeyCodeJoystickButtons()
    {
        if( SAEArcadeCabinetInput.joystickKeyCodePressedButton == null ) { return; } // Nothing listening for events.

        // Check there are player inputs set up!
        if( SAEArcadeCabinetInput.playerInputs == null )
        {
            SAEArcadeCabinetInput.playerInputs = new PlayerInput[ 4 ];
            for( int i=0; i< SAEArcadeCabinetInput.playerInputs.Length; i++ ) { SAEArcadeCabinetInput.playerInputs[ i ] = new PlayerInput(); }
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
    private static void CheckKeyCodeButton( KeyCode keyCode )
    {
        if( SAEArcadeCabinetInput.playerInputs == null ) { return; }

        if( keyCode.ToString().StartsWith( "Joystick" ) == false ) { return; }
        string[] tmpStrParts = keyCode.ToString().Replace( "Joystick", "" ).Replace( "Button", "," ).Split( ",".ToCharArray() );
        if( tmpStrParts.Length != 2 ) { return; }
        int joystickId = int.Parse( tmpStrParts[ 0 ] );
        int buttonId = int.Parse( tmpStrParts[ 1 ] );

        if( joystickId < 1 || joystickId >= SAEArcadeCabinetInput.playerInputs.Length ) { return; }

        if( Input.GetKey( keyCode ) == true )
            //{ SAEArcadeCabinetInput.joystickKeyCodePressedButton.Invoke( int.Parse( tmpStrParts[ 0 ] ), int.Parse( tmpStrParts[ 1 ] ) ); }
        {
            //SAEArcadeCabinetInput.joystickKeyCodePressedButton.Invoke( joystickId, int.Parse( tmpStrParts[ 1 ] ) );
            if( SAEArcadeCabinetInput.playerInputs[ joystickId ].buttonsPressed.Contains( keyCode ) == false )
            {
                // Invoke pressed event.
                SAEArcadeCabinetInput.joystickKeyCodePressedButton?.Invoke( joystickId, buttonId );
                SAEArcadeCabinetInput.playerInputs[ joystickId ].buttonsPressed.Add( keyCode ); // Add keyCode to buttons pressed.
            }
            else
            {
                // Invoke holding event.
                SAEArcadeCabinetInput.joystickKeyCodeHelddButton?.Invoke( joystickId, buttonId );
            }
        }
        else if( SAEArcadeCabinetInput.playerInputs[ joystickId ].buttonsPressed.Contains( keyCode ) == true )
        {
            // Invoke release event.
            SAEArcadeCabinetInput.joystickKeyCodeReleasedButton?.Invoke( joystickId, buttonId );
            SAEArcadeCabinetInput.playerInputs[ joystickId ].buttonsPressed.Remove( keyCode ); // Remove keyCode from buttons pressed.
        }
    }

    public static bool JoyStickButton( KeyCode keyCode ) { return Input.GetKey( keyCode ); }

// Simple Axis /////////////////////////////////////////////////////////////////////////
    public static float JoyStickAxis( int joystickId, bool xAxis = true )
    {
        // Joystick1_xAxis
        string axisName = "Joystick" + joystickId + "_";
        if( xAxis == true ) { axisName += "xAxis"; } else { axisName += "yAxis"; }
        float axisValue = Input.GetAxis( axisName );

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
}
