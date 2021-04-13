using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary> 
/// Public class <c>MCUCommand</c> is used to store input register data from the control room
/// in an easy way for us to decode the commands on the <c>TelescopeControllerSim</c>
/// </summary>
public class MCUCommand : MonoBehaviour {
    ///
    /// static constants
    ///
    private const float STEPS_PER_REVOLUTION = 20000.0f;
    private const float AZIMUTH_GEARING_RATIO = 500.0f;
    private const float ELEVATION_GEARING_RATIO = 50.0f;
    
    ///
    /// member fields
    ///
    public float azimuthSpeed { get; set; }
    public float elevationSpeed { get; set; }
    public float acceleration { get; set; }
    public float azimuthDegrees { get; set; }
    public float elevationDegrees { get; set; }

    /// <summary>
    /// Constructor to decode the register data into member fields for readable access on <c>TelescopeControllerSim</c>
    /// </summary>
    public MCUCommand(ushort[] registerData) {
        // we can determine move type by looking at the first register value
        switch(registerData[0])
        { 
            case 0x0002: // 2 (in hex) is for RELATIVE moves
            	Debug.Log("RELATIVE MOVE INCOMING");
                // calculate speed fields
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

                logValues();
                break;
            case 0x0080: // JOG Moves - 0x0080 = POSITIVE ELEVATION, CLOCKWISE AZIMUTH
                Debug.Log("JOG COMMAND INCOMING");
                azimuthSpeed = ((registerData[4] << 16) + registerData[5]) / 20;
                elevationSpeed = ((registerData[14] << 16) + registerData[15]) / 20;
            
                // convert raw register values into simulator friendly terms
                convertToUnitySpeak();
                logValues();
                break;
            case 0x0100: // 0x0100 = NEGATIVE ELEVATION, COUNTER-CLOCKWISE AZIMUTH
                Debug.Log("JOG COMMAND INCOMING");
                azimuthSpeed = -((registerData[4] << 16) + registerData[5]) / 20;
                elevationSpeed = -((registerData[14] << 16) + registerData[15]) / 20;
                
                // convert raw register values into simulator friendly terms
                convertToUnitySpeak();
                logValues();
                break;
            case 0x0420:
                // this is called when we first start the sim'd mcu. we want to set this to default values we can be sure are not from the CR
                Debug.Log("Building MCUCommand object for the first time in SimServer.cs");
                azimuthSpeed = 420.69f;
                elevationSpeed = 420.69f;
                acceleration = 420.69f;
                azimuthDegrees = 420.69f;
                elevationDegrees = 420.69f;
                break;
            case 0x0069:
                Debug.Log("Building MCUCommand for telescope controller to put in start position");
                azimuthSpeed = 60.0f;
                elevationSpeed = 60.0f;
                acceleration = 50.0f;
                azimuthDegrees = 0.0f;
                elevationDegrees = 15.0f;
                break;
            default:
                Debug.Log("!!! ERROR !!! MCUCommand Constructor: Cannot determine a move type from control room. Setting everything to 0.0f.");
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
    }

    /// <summary>
    /// Helper method used to convert steps to degrees - this is taken from <c>ConversionHelper.cs</c> on the control room
    /// </summary>
    private float convertStepsToDegrees(float steps, float gearingRatio) {
        return steps * 360.0f / (STEPS_PER_REVOLUTION * gearingRatio);
    }

    /// <summary>
    /// Helper method just to log the relevant fields as we go throughout the process. Shouldn't need to exist when everything is finalized
    /// </summary>
    private void logValues() {
        Debug.Log("acceleration: " + acceleration);
        Debug.Log("azimuthSpeed: " + azimuthSpeed);
        Debug.Log("elevationSpeed: " + elevationSpeed);
        Debug.Log("azimuthDegrees: " + azimuthDegrees);
        Debug.Log("elevationDegrees: " + elevationDegrees);
    }
}