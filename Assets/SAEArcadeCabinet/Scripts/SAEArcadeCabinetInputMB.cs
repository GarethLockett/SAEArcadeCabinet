using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
    Script: SAEArcadeCabinetInputMB
    Author: Gareth Lockett
    Version: 1.0
    Description:    ...

                    NOTES:
                            - Make sure old InputManager has been set up! ... axis 1-8/16 joysticks? (KeyCode buttons seem to only go to 8 per joystick?)

                    TODO:
                            - Add UnityEvents.
*/

public class SAEArcadeCabinetInputMB : MonoBehaviour
{
    // Events
    public static Action<PlayerColorId, int> playerPressedButton;               // <Joystick id, Button id> Invoked when a button has started to be pressed.
    public static Action<PlayerColorId, int> playerHeldButton;                  // <Joystick id, Button id> Invoked each frame while a button is held down.
    public static Action<PlayerColorId, int> playerReleasedButton;              // <Joystick id, Button id> Invoked when a button has stopped been pressed.

    public static Action<PlayerColorId, Vector2> playerJoystickAxisChanged;     // <Joystick id, Vector2 XY axis values> Invoked when an axis value changed.
    public static Action configurationFinished;                                 // Invoked when configuraion finishes.

    // Enumerators
    public enum PlayerColorId { UNKNOWN, YELLOW_PLAYER, BLUE_PLAYER, RED_PLAYER, GREEN_PLAYER }

    // Sub classes
    private class PlayerInput
    {
        public PlayerColorId playerId;                                          // Will be either:UNKNOWN, YELLOW_PLAYER, BLUE_PLAYER, RED_PLAYER, GREEN_PLAYER;
        public int joystickId;                                                  // The mapped joystick id (1-4)
        public int firstButtonKeyCodeId;                                        // Joystick 1 = 350, Joystick 2 = 370, Joystick 3 = 390, Joystick 4 = 410
        public List<KeyCode> buttonsPressed;                                    // Keycodes of buttons currently pressed.
        public Vector2 lastJoystickPosition;                                    // Last checked joystick x,y axis position.

        // Constructor
        public PlayerInput()
        {
            this.playerId = PlayerColorId.UNKNOWN;
            this.joystickId = -1;
            this.buttonsPressed = new List<KeyCode>();
        }
    }

    // Properties
    public static SAEArcadeCabinetInputMB instance;                             // Singleton reference.
    public bool useEvents;                                                      // Allow invoking of events for button presses, axis changes etc
    
    private PlayerInput[] playerInputs;                                         // The 4 player inputs representing the joysticks & buttons.
    private bool configuring;                                                   // Track the configuring state.

    private float configurationStartTime;                                       // Used to give a few seconds before listen for buttons/axis when configuring.
    private float nextEventListenTime;                                          // Used to give a few seconds after configuring before listening for events.
    private Canvas configureCanvas;                                             // Reference to the configuration canvas.
    private Text configurationText;                                             // Reference to the configuration text.

    // Methods
    private void Awake()
    {
        // Set up singleton.
        if( SAEArcadeCabinetInputMB.instance != null ) { if( SAEArcadeCabinetInputMB.instance != this ) { Destroy( this ); return; } }
        SAEArcadeCabinetInputMB.instance = this;
    }

    private void Start()
    {
        // Manually create 4 players.
        this.playerInputs = new PlayerInput[ 4 ];
        for( int i = 0; i < this.playerInputs.Length; i++ ) { this.playerInputs[ i ] = new PlayerInput(); }
    }

    private void Update()
    {
        // Check if configuring.
        if( this.configuring == true ) { this.ConfiguringPlayers(); return; }
        else if( this.configureCanvas != null ) { Destroy( this.configureCanvas.gameObject ); }

        // Check for events.
        if( this.useEvents == true ) { if( Time.unscaledTime > this.nextEventListenTime ) { this.CheckForEvents(); } }
    }

    private void CheckForEvents()
    {
        // Sanity check. Check playerInputs have been created/exist (Should have been set up in Start)
        if( this.playerInputs == null ) { return; }

        // Check for any button presses.
        int startKeyCodeId = ( int ) KeyCode.Joystick1Button0;  // 350
        int endKeyCodeId = ( int ) KeyCode.Joystick8Button19;   // 509
        for( int i = startKeyCodeId; i < endKeyCodeId; i++ )
        {
            KeyCode keyCode = ( KeyCode ) i;
            bool keyPressed = Input.GetKey( keyCode );

            // Get the joystick & button id from the keyCode.
            int joystickId = -1, buttonId = -1;
            if( this.GetKeyCodeJoystickAndButtonId( keyCode, ref joystickId, ref buttonId ) == false ) { continue; }
            if( joystickId == -1 || buttonId == -1 ) { continue; } //?

            // Get the player input by the joystick id.
            PlayerInput playerInput = this.GetPlayerInputByJoystickId( joystickId );
            if( playerInput == null )
            {
                if( keyPressed == true )
                {
                    // Key as pressed by an unknown player ... configure!
                    this.configuring = true;
                }
                continue; //?
            }

            // Check for button events.
            if( keyPressed == true )
            {
//Debug.Log( "keyPressed: " +keyCode +"  buttonId: " + buttonId );
                if( playerInput.buttonsPressed.Contains( keyCode ) == false )
                {
                    // Started pressing the button.
                    playerInput.buttonsPressed.Add( keyCode ); // Add the button.
                    SAEArcadeCabinetInputMB.playerPressedButton?.Invoke( playerInput.playerId, buttonId );      // Invoke any pressed button events.
                }
                else { SAEArcadeCabinetInputMB.playerHeldButton?.Invoke( playerInput.playerId, buttonId ); }    // Invoke any holding down the button events.
            }
            else if( playerInput.buttonsPressed.Contains( keyCode ) == true )
            {
                playerInput.buttonsPressed.Remove( keyCode ); // Remove the button.
                SAEArcadeCabinetInputMB.playerReleasedButton?.Invoke( playerInput.playerId, buttonId );         // Invoke any released button events.
            }
        }

        // Check for joystick axis change events.
        for( int i = 0; i < this.playerInputs.Length; i++ )
        {
            if( this.playerInputs[ i ].playerId == PlayerColorId.UNKNOWN ) { continue; }
            if( this.playerInputs[ i ].joystickId <= 0 ) { continue; }

            // Check for changed axis.
            string axisName = "Joystick" + this.playerInputs[ i ].joystickId; // Eg Joystick1_xAxis
            Vector2 axisValues = new Vector2( Input.GetAxis( axisName + "_xAxis" ), Input.GetAxis( axisName + "_yAxis" ) );
            if( axisValues.x == this.playerInputs[ i ].lastJoystickPosition.x && axisValues.y == this.playerInputs[ i ].lastJoystickPosition.y ) { continue; } // No change.

            // Invoke any axis change events.
            SAEArcadeCabinetInputMB.playerJoystickAxisChanged?.Invoke( this.playerInputs[ i ].playerId, axisValues );

            // Update the last axis values.
            this.playerInputs[ i ].lastJoystickPosition = axisValues;
        }
    }

    public void ConfiguringPlayers()
    {
        this.configuring = true;

        // Find the next player to configure.
        int maxPlayers = Enum.GetNames( typeof( PlayerColorId ) ).Length;
maxPlayers = 2; // TESTING
        for( int p = 1; p < maxPlayers +1; p++ )
        {
            PlayerColorId playerId = ( PlayerColorId ) p;
            if( this.GetPlayerInputById( playerId ) == null )
            {
//Debug.Log( "Configure " + playerId.ToString() );

                // Check there is a configuration canvas. If not, create one.
                if( this.configureCanvas == null )
                {
                    // Create a canvas.
                    GameObject configCanvas = new GameObject( "configCanvas" );
                    this.configureCanvas = configCanvas.AddComponent<Canvas>();
                    this.configureCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

                    // Create a black background to hide the scene.
                    GameObject bgPanelGO = new GameObject( "bgPanel" );
                    bgPanelGO.transform.SetParent( configCanvas.transform );
                    Image bgPanel = bgPanelGO.AddComponent<Image>();
                    bgPanel.color = Color.black;
                    RectTransform rtPanel = bgPanel.GetComponent<RectTransform>();
                    rtPanel.anchoredPosition = Vector2.zero;
                    rtPanel.SetSizeWithCurrentAnchors( RectTransform.Axis.Horizontal, Screen.width );
                    rtPanel.SetSizeWithCurrentAnchors( RectTransform.Axis.Vertical, Screen.height );

                    // Create configuration text.
                    GameObject textGO = new GameObject( "configText" );
                    textGO.transform.SetParent( bgPanel.transform );
                    this.configurationText = textGO.AddComponent<Text>();
                    this.configurationText.color = Color.white;
                    RectTransform rtText = this.configurationText.GetComponent<RectTransform>();
                    rtText.anchoredPosition = Vector2.up *( Screen.height *0.15f );
                    rtText.SetSizeWithCurrentAnchors( RectTransform.Axis.Horizontal, Screen.width * 0.9f );
                    this.configurationText.resizeTextForBestFit = true;
                    this.configurationText.font = Font.CreateDynamicFontFromOSFont( "Arial", 36 );// Mathf.RoundToInt( Screen.width * 0.02f ) );
                this.configurationText.supportRichText = true;
                    this.configurationText.verticalOverflow = VerticalWrapMode.Overflow;
                    this.configurationText.alignment = TextAnchor.UpperCenter;

                    this.configurationStartTime = Time.unscaledTime;
                }

                // Set the title text for configuring.
                this.configurationText.text = "<b>Input Configuration</b>\n\n";

                // Give a few seconds before checking for buttons.
                if( Time.unscaledTime < this.configurationStartTime + 2f ) { return; }

                // Set the text for the configuring player.
                string playerColorStr = playerId.ToString().Replace( "_PLAYER", "" );
                this.configurationText.text += "<color=" + playerColorStr.ToLower() + ">" + playerColorStr + " player press a button or move the joystick.</color>";

                PlayerInput playerInput = null;

                // Check for any axis input.
                int numSetupAxis = 8; // This is the number of xAxis and yAxis joystick inputs set up in the legacy InputManager.
                for( int i=0; i< numSetupAxis; i++ )
                {
                    // Check if the joystick id has already been assigned.
                    if( this.GetPlayerInputByJoystickId( i + 1 ) != null ) { continue; }

                    // Check the axis for movement.
                    string axisName = "Joystick" +( i + 1 ); // Eg Joystick1_xAxis
                    if( Mathf.Abs( Input.GetAxis( axisName +"_xAxis" ) ) < 0.5f && Mathf.Abs( Input.GetAxis( axisName + "_yAxis" ) ) < 0.5f ) { continue; }

                    // Assign the joystick id to the first UNKNOWN player.
                    playerInput = this.GetPlayerInputById( PlayerColorId.UNKNOWN );
                    if( playerInput == null )
                    {
                        Debug.LogWarning( "[SAEArcadeInputMB] Could not get an UNKNOWN player in configuration?!" );
                        this.configuring = false;
                        return;
                    }
                    playerInput.playerId = playerId;
                    playerInput.joystickId = i + 1;
                    
                    break;
                }

                // Check for any button presses.
                if( playerInput == null )
                {
                    int startKeyCodeId = ( int ) KeyCode.Joystick1Button0;  // 350
                    int endKeyCodeId = ( int ) KeyCode.Joystick8Button19;   // 509
                    for( int i = startKeyCodeId; i < endKeyCodeId; i++ )
                    {
                        KeyCode keyCode = ( KeyCode ) i;
                        bool keyPressed = Input.GetKey( keyCode );
                        if( keyPressed == false ) { continue; }

                        // Get the joystick and button ids for the pressed keyCode.
                        int joystickId = -1, buttonId = -1;
                        if( this.GetKeyCodeJoystickAndButtonId( keyCode, ref joystickId, ref buttonId ) == false ) { continue; }
                        if( joystickId == -1 || buttonId == -1 ) { continue; } //?

                        // Check if the pressed buttons' joystick id already belongs to another player.
                        if( this.GetPlayerInputByJoystickId( joystickId ) != null ) { continue; }

                        // Assign the joystick id to the first UNKNOWN player.
                        playerInput = this.GetPlayerInputById( PlayerColorId.UNKNOWN );
                        if( playerInput == null )
                        {
                            Debug.LogWarning( "[SAEArcadeInputMB] Could not get an UNKNOWN player in configuration?!" );
                            this.configuring = false;
                            return;
                        }
                        playerInput.playerId = playerId;
                        playerInput.joystickId = joystickId;

                        break;
                    }
                }

                if( playerInput != null )
                {
                    // Set the first keyCode button based on playerId.
                    switch( playerInput.joystickId )
                    {
                        case 1: playerInput.firstButtonKeyCodeId = 350; break;
                        case 2: playerInput.firstButtonKeyCodeId = 370; break;
                        case 3: playerInput.firstButtonKeyCodeId = 390; break;
                        case 4: playerInput.firstButtonKeyCodeId = 410; break;
                        case 5: playerInput.firstButtonKeyCodeId = 430; break;
                        case 6: playerInput.firstButtonKeyCodeId = 450; break;
                        case 7: playerInput.firstButtonKeyCodeId = 470; break;
                        case 8: playerInput.firstButtonKeyCodeId = 490; break;
                    }

                    Debug.Log( "[SAEArcadeCabinetInputMB] " +playerInput.playerId.ToString() + " was assigned to joystick " +playerInput.joystickId );

                    // Check if finished configuring (Eg no more UNKOWN players)
                    if( p == maxPlayers )
                    {
                        SAEArcadeCabinetInputMB.configurationFinished?.Invoke();
                        this.configuring = false;
                        this.nextEventListenTime = Time.unscaledTime + 2f;
                        Debug.Log( "[SAEArcadeCabinetInputMB] Finished configuration." );
                    }
                }

                return;
            }
        }
    }

    private bool GetKeyCodeJoystickAndButtonId( KeyCode keyCode, ref int joystickId, ref int buttonId )
    {
        // Get the joystick & button id from the keyCode (Eg Joystick1Button0 == 1)
        if( keyCode.ToString().StartsWith( "Joystick" ) == false ) { return false; }
        string[] tmpStrParts = keyCode.ToString().Replace( "Joystick", "" ).Replace( "Button", "," ).Split( ",".ToCharArray() );
        if( tmpStrParts.Length != 2 ) { return false; }
        joystickId = int.Parse( tmpStrParts[ 0 ] );
        buttonId = int.Parse( tmpStrParts[ 1 ] );
        return true;
    }

    private PlayerInput GetPlayerInputById( PlayerColorId playerId )
    {
        // Sanity check.
        if( this.playerInputs == null ) { return null; }
        return this.playerInputs.FirstOrDefault( item => item.playerId == playerId );
    }

    private PlayerInput GetPlayerInputByJoystickId( int joystickId )
    {
        // Sanity check.
        if( this.playerInputs == null ) { return null; }
        return this.playerInputs.FirstOrDefault( item => item.joystickId == joystickId );
    }
    
    public bool PlayerPressingButton( PlayerColorId playerId, int buttonId )
    {
        // Sanity checks.
        if( this.playerInputs == null ) { return false; }
        if( playerId == PlayerColorId.UNKNOWN ) { return false; }
        if( buttonId < 0 ) { return false; }

        // Get the player input via id.
        PlayerInput playerInput = this.GetPlayerInputById( playerId );
        if( playerInput == null ) { return false; }

        // Calculate the button keyCode id and check the key.
        return Input.GetKey( ( KeyCode ) playerInput.firstButtonKeyCodeId + buttonId );
    }
    public Vector2 PlayerJoystickAxis( PlayerColorId playerId )
    {
        // Sanity checks.
        if( this.playerInputs == null ) { return Vector2.zero; }
        if( playerId == PlayerColorId.UNKNOWN ) { return Vector2.zero; }

        // Get the player input via id.
        PlayerInput playerInput = this.GetPlayerInputById( playerId );
        if( playerInput == null ) { return Vector2.zero; }
        if( playerInput.joystickId <= 0 ) { return Vector2.zero; }

        string axisName = "Joystick" + playerInput.joystickId; // Eg Joystick1_xAxis
        return new Vector2( Input.GetAxis( axisName + "_xAxis" ), Input.GetAxis( axisName + "_yAxis" ) );
    }
}
