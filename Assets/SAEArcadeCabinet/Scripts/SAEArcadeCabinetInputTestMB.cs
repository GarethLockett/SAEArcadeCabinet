using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Reflection;
using System.Linq;

/*
    Script: SAEArcadeCabinetInputTestMB
    Author: Gareth Lockett
    Version: 1.0
    Description:    Simple script for testing SAEArcadeCabinetInputMB.
*/

public class SAEArcadeCabinetInputTestMB : MonoBehaviour
{
    // Methods
    private void Start()
    {
        // Listen for events.
        SAEArcadeCabinetInputMB.playerPressedButton += this.PlayerPressedButton;
        SAEArcadeCabinetInputMB.playerHeldButton += this.PlayerHeldButton;
        SAEArcadeCabinetInputMB.playerReleasedButton += this.PlayerReleasedButton;
        SAEArcadeCabinetInputMB.playerJoystickAxisChanged += this.PlayerJoystickAxisChanged;
        SAEArcadeCabinetInputMB.configurationFinished += this.ConfigurationFinsihed;

        // If you are only going to use polling, then you need to configure players first! (Probably good to do this for events too)
        SAEArcadeCabinetInputMB.instance.ConfiguringPlayers();
    }

    private void Update()
    {
        // Example of polling buttons (Eg Checking here if the YELLOW players' button 1 is pressed)
        if( SAEArcadeCabinetInputMB.instance.PlayerPressingButton( SAEArcadeCabinetInputMB.PlayerColorId.YELLOW_PLAYER, 1 ) == true )
            { Debug.Log( "Polling: YELLOW player is pressing button 1" ); }

        // Example of polling axis (Eg Checking YELLOW players' joystick axis)
        Debug.Log( "YELLOW joystick axis: " + SAEArcadeCabinetInputMB.instance.PlayerJoystickAxis( SAEArcadeCabinetInputMB.PlayerColorId.YELLOW_PLAYER ) );
    }
    
    // Event handlers.
    private void PlayerPressedButton( SAEArcadeCabinetInputMB.PlayerColorId playerId, int buttonId )
    {
        Debug.Log( "PlayerPressedButton: " + playerId.ToString() + "  " + buttonId );
    }
    private void PlayerHeldButton( SAEArcadeCabinetInputMB.PlayerColorId playerId, int buttonId )
    {
        Debug.Log( "PlayerHeldButton: " + playerId.ToString() + "  " + buttonId );
    }
    private void PlayerReleasedButton( SAEArcadeCabinetInputMB.PlayerColorId playerId, int buttonId )
    {
        Debug.Log( "PlayerReleasedButton: " + playerId.ToString() + "  " + buttonId );
    }
    private void PlayerJoystickAxisChanged( SAEArcadeCabinetInputMB.PlayerColorId playerId, Vector2 axis )
    {
        Debug.Log( "PlayerJoystickAxisChanged: " + playerId.ToString() + "  " + axis );
    }
    private void ConfigurationFinsihed()
    {
        Debug.Log( "ConfigurationFinsihed!" );
    }
}
