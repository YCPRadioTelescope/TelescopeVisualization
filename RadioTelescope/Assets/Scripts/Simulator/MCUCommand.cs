using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary> 
/// Public class <c>MCUCommand</c> is used to store input register data from the control room
/// in an easy way for us to decode the commands on the <c>TelescopeControllerSim</c>
/// </summary>
public class MCUCommand : MonoBehaviour 
{
    ///
    /// static constants (from the control room, not eyeballed for unity's sake)
    ///
    private const float STEPS_PER_REVOLUTION = 20000.0f;
    private const float AZIMUTH_GEARING_RATIO = 500.0f;
    private const float ELEVATION_GEARING_RATIO = 50.0f;
    
    ///
    /// member fields
    ///
    public float azimuthSpeed = 0.0f;
    public float elevationSpeed = 0.0f;
    public float acceleration = 0.0f;
    public float azimuthDegrees = 0.0f;
    public float elevationDegrees = 0.0f;
    public bool jog = false;
    public bool errorFlag = false;

    /// <summary>
    /// constructor for building mcu command objects
    /// </summary>
    /// <param name="registerData"> raw register data from the control room </param>
    /// <param name="simAzimtuhDegrees"> helper param to calculate how far we need to go for a relative move </param>
    public MCUCommand(ushort[] registerData, float simAzimtuhDegrees = 0.0f) {
        // we can determine move type by looking at the first register value
        switch(registerData[0])
        { 
            case 0x0002: // 2 (in hex) is for RELATIVE moves
            	Debug.Log("RELATIVE MOVE INCOMING");
                // calculate speed fields
                // the /250.0f is what i added to get the speed values down to unity okay things - it was moving way too fast
                azimuthSpeed = ((registerData[4] << 16) + registerData[5]) / 250.0f;
                elevationSpeed = ((registerData[14] << 16) + registerData[15]) / 250.0f;

                // grab acceleration (we set registers 6 & 7 on the control room side, but the previous team only grabbed 6 so only 6 here)
                acceleration = registerData[6];

                // calculate azimuth and elevation steps (this is set on registers 3 & 4 for azimuth and 12 & 13 for elevation)
                // note the var is called *azimuthDegrees* and *elevationDegrees* but right now these are in steps. They get converted below
                azimuthDegrees = (registerData[2] << 16) + registerData[3];
                elevationDegrees = (registerData[12] << 16) + registerData[13];

                // convert raw register values into simulator friendly terms
                convertToUnitySpeak();

                // the simulator targets the *absolute* position, so we need to fix the command'd azimuth position with the sim's absolute position here
                azimuthDegrees += simAzimtuhDegrees;

                logValues();
                break;

            case 0x0080: // JOG Moves - 0x0080 = CLOCKWISE AZIMUTH
                Debug.Log("AZIMTUH JOG RIGHT COMMAND INCOMING");
                jog = true;
                azimuthSpeed = ((registerData[4] << 16) + registerData[5]) / 20;
            
                // convert raw register values into simulator friendly terms
                convertToUnitySpeak();
                logValues();
                break;

            case 0x0100: // 0x0100 = COUNTER-CLOCKWISE AZIMUTH
                Debug.Log("AZIMTUH JOG LEFT COMMAND INCOMING");
                jog = true;
                azimuthSpeed = -((registerData[4] << 16) + registerData[5]) / 20;
                
                // convert raw register values into simulator friendly terms
                convertToUnitySpeak();
                logValues();
                break;

            case 0x000: // COULD BE A BUNCH OF THINGS

                // first check to see if it's an elevation jog command
                if (registerData[10] == 0x0080) // NEGATIVE ELEVATION JOG
                {
                    Debug.Log("NEGATIVE ELEVATION JOG COMMAND INCOMING");
                    jog = true;
                    elevationSpeed = -((registerData[14] << 16) + registerData[15]) / 20;

                    // convert raw register values into simulator friendly terms
                    convertToUnitySpeak();
                    logValues();
                    break;
                } else if (registerData[10] == 0x0100) // POSITIVE ELEVATION JOG
                {
                    Debug.Log("POSITIVE ELEVATION JOG COMMAND INCOMING");
                    jog = true;
                    elevationSpeed = ((registerData[14] << 16) + registerData[15]) / 20;
                
                    // convert raw register values into simulator friendly terms
                    convertToUnitySpeak();
                    logValues();
                    break;
                }

                // Cancel move also starts with a 0x0000, but it is deliminated by the second register (a 3)
                if (registerData[1] == 0x0003) // CANCEL MOVE
                {
                    Debug.Log("CANCEL MOVE INCOMING");
                    // set error flag so TelescopeController doesn't do anything with the currentCommand's fields
                    errorFlag = true;
                    break;
                }

                Debug.Log("MCUCOMMAND: !!ERROR!! We fell through the 0x000 case and did not match any conditions.");
                Debug.Log("Setting everything to 0.0f and breaking...");
                errorFlag = true;
                azimuthSpeed = 0.0f;
                elevationSpeed = 0.0f;
                acceleration = 0.0f;
                azimuthDegrees = 0.0f;
                elevationDegrees = 0.0f;
                break;
            
            case 0x0420: // SIM SERVER INITIALIZATION

                // this is called when we first start the sim'd mcu. we want to set this to default values we can be sure are not from the CR
                Debug.Log("Building MCUCommand object for the first time in SimServer.cs");
                azimuthSpeed = 420.69f;
                elevationSpeed = 420.69f;
                acceleration = 420.69f;
                azimuthDegrees = 420.69f;
                elevationDegrees = 420.69f;
                break;

            case 0x0069: // SIM CONTROLLER INITIALIZATION

                Debug.Log("Building MCUCommand for telescope controller to put in start position");
                azimuthSpeed = 60.0f;
                elevationSpeed = 60.0f;
                acceleration = 50.0f;
                azimuthDegrees = 0.0f;
                elevationDegrees = 15.0f;
                break;

            case 0x0096: // TEST MOVE

                // this is for the TestMove routine to arbitrarily move the telescope from within unity
                Debug.Log("Buidling MCUCommand for TestMove.cs");

                // we can't use convertToUnitySpeak() here because the values here are already in degrees

                azimuthDegrees = Convert.ToInt32(registerData[1]);
                elevationDegrees = Convert.ToInt32(registerData[2]);

                Debug.Log("MCUCOMMAND: Azimuth after converting to int: " + azimuthDegrees);
                Debug.Log("MCUCOMMAND: Elevation after converting to int: " + elevationDegrees);

                azimuthSpeed = 2.0f;
                elevationSpeed = 5.0f;
                break;

            default: // catch "all" and return error command

                Debug.Log("!!! ERROR !!! MCUCommand Constructor: Cannot determine a move type from control room. Setting error flag to true and everything else to 0.0f.");
                errorFlag = true;
                azimuthSpeed = 0.0f;
                elevationSpeed = 0.0f;
                acceleration = 0.0f;
                azimuthDegrees = 0.0f;
                elevationDegrees = 0.0f;
                break;
        }
    }

    /// <summary>
    /// Helper method used to convert raw hex register values to workable values we can use on the controller side 
    /// </summary>
    private void convertToUnitySpeak() 
    {
        // get everything to floats
        Convert.ToSingle(azimuthSpeed);
        Convert.ToSingle(elevationSpeed);
        Convert.ToSingle(azimuthDegrees);
        Convert.ToSingle(elevationDegrees);

        // next convert azimuth and elevation steps to degrees
        // this process will most likely change when we want to make the process interruptable, so instead of an absolute conversion
        // something like (# of steps for 1 degree) -- future work
        azimuthDegrees = convertStepsToDegrees(azimuthDegrees, AZIMUTH_GEARING_RATIO);
        elevationDegrees = convertStepsToDegrees(elevationDegrees, ELEVATION_GEARING_RATIO);

        if (elevationDegrees < 0)
            elevationDegrees *= -1;

        // the speed here is relative to the actual values, but to visualize for unity we need them a little smaller
        azimuthSpeed = azimuthSpeed / 100;
        elevationSpeed = elevationSpeed / 10;
    }

    /// <summary>
    /// Helper method used to convert steps to degrees - this is taken from <c>ConversionHelper.cs</c> on the control room
    /// </summary>
    /// <param name="steps"> steps passed from the control room (or wherever)</param>
    /// <param name="gearingRatio"> the constant ratio associated with the type of movement. For us, this will be azimuth or elevation gearing </param>
    /// <returns> a float "degree" value from the information passed </returns>
    private float convertStepsToDegrees(float steps, float gearingRatio) {
        return steps * 360.0f / (STEPS_PER_REVOLUTION * gearingRatio);
    }

    /// <summary>
    /// Helper method just to log the relevant fields as we go throughout the process. Shouldn't need to exist when everything is finalized
    /// </summary>
    private void logValues() {
        // Debug.Log("acceleration: " + acceleration);
        // Debug.Log("azimuthSpeed: " + azimuthSpeed);
        // Debug.Log("elevationSpeed: " + elevationSpeed);
        // Debug.Log("azimuthDegrees: " + azimuthDegrees);
        // Debug.Log("elevationDegrees: " + elevationDegrees);
    }
}