using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Reflection;
using System.Linq;

/*
    Script: SAEArcadeCabinetInputTest
    Author: Gareth Lockett
    Version: 1.0
    Description:    Simple script for testing SAEArcadeCabinetInput.
*/

public class SAEArcadeCabinetInputTest : MonoBehaviour
{
    // Methods
    private void Start()
    {
        SAEArcadeCabinetInput.ActivateInputEvents();

        SAEArcadeCabinetInput.joystickKeyCodePressedButton += this.ButtonKeyCodePressed;
        SAEArcadeCabinetInput.joystickKeyCodeHelddButton += this.ButtonKeyCodeHeld;
        SAEArcadeCabinetInput.joystickKeyCodeReleasedButton += this.ButtonKeyCodeReleased;

        SAEArcadeCabinetInput.joystickAxisChanged += this.JoystickAxisChanged;
    }

    private void Update()
    {
        //if( SAEArcadeCabinetInput.IsButtonPressed( SAEArcadeCabinetInput.SAEArcadeCabinetColorId.YELLOW, SAEArcadeCabinetInput.SAEArcadeCabinetButton.BUTTON0 ) == true )
        //{
        //    Debug.Log( "YELLOW BUTTON0 is PRESSED!" );
        //}

        //if( Input.GetButton( "Joystick1_Button_0" ) == true ) { Debug.Log( "HERE" ); }
        //if( Input.GetKey( KeyCode.Joystick1Button0 ) == true ) { Debug.Log( "HERE" ); }

        //Debug.Log( "joystick1: " + SAEArcadeCabinetInput.JoyStickAxis( 1 ) +", " + SAEArcadeCabinetInput.JoyStickAxis( 1, false ) );
        //Debug.Log( Input.GetAxis( "Joystick1_xAxis" ) );

        //if( SAEArcadeCabinetInput.JoyStickButton( KeyCode.Joystick4Button0 ) == true ) { Debug.Log( "HERE" ); }// { SAEArcadeCabinetInput.DeactivateInputEvents(); }
    }

    //private void ButtonPressed( SAEArcadeCabinetInput.SAEArcadeCabinetPlayerControls playerControls, SAEArcadeCabinetInput.SAEArcadeCabinetButton buttonPressed )
    //{
    //    Debug.Log( "ButtonPressed: " +playerControls.colorId.ToString() +"  " +buttonPressed.ToString() );
    //}

    private void ButtonKeyCodePressed( int joystickId, int buttonId )
    {
        Debug.Log( "ButtonKeyCodePressed: " + joystickId + "  " + buttonId );
    }
    private void ButtonKeyCodeHeld( int joystickId, int buttonId )
    {
        Debug.Log( "ButtonKeyCodeHeld: " + joystickId + "  " + buttonId );
    }
    private void ButtonKeyCodeReleased( int joystickId, int buttonId )
    {
        Debug.Log( "ButtonKeyCodeReleased: " + joystickId + "  " + buttonId );
    }
    private void JoystickAxisChanged( int joystickId, Vector2 axis )
    {
        Debug.Log( "JoystickAxisChanged: " + joystickId + "  " + axis );
    }
}
