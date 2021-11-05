// all of the information here I gathered during digging into what gets sent by each move type from the control room. The spreadsheet where I pulled these values can be found
// here: https://docs.google.com/spreadsheets/d/1vBKsnV7Xso0u19ZyhtVimiCXpZjc007usVjvAsxHJNU/edit#gid=0
// 
// last updated: LG - 4/27/2021

/// <summary>
/// This enum helps make the process of building MCUCommand objects more readable. There is a seperate enum for the regiser positions,
/// but for more internal documentation the comments above each enum type refer to where these values should be in the registerData coming over
/// from the control room
/// </summary>
public enum CommandType : ushort
{
	// Both axes, first command register:
	RELATIVE_MOVE = 0x0002,
	CONTROLLED_STOP = 0x0004,
	IMMEDIATE_STOP = 0x0010,
	CLOCKWISE_HOME = 0x0020,
	COUNTERCLOCKWISE_HOME = 0x0040,
	NEGATIVE_JOG = 0x0080,
	POSITIVE_JOG = 0x0100,
	CLEAR_MCU_ERRORS = 0x0800, // MCU errors = bits 8, 9, 11, 12, 13
	
	// Azimuth first command register:
	CONFIGURE_MCU = 0x852c, // 0x852d is received on the elevation register
	
	// Both axes, second command register:
	CANCEL_MOVE = 0x0003,
}

/// <summary>
/// This enum lines up for the vast majority of the register data we send over, but there are some exceptions. For example, stop moves only use a couple of the registers
/// to send their information over, so the this entire list does not line up for them (the first or second word does this). The homing command or the reset errors command also do 
/// not line up with this list. The majority of moves (we really only use relative and jog moves) do line up with this list.
/// </summary>
public enum IncomingRegIndex : int
{
	///
	/// AZIMUTH REGISTERS:
	///
	// Command registers are what determines the command type.
	firstCommandAzimuth,
	secondCommandAzimuth,
	// Data whose purpose is specific to the received command.
	// This is most often a number of steps to move by.
	firstDataAzimuth,
	secondDataAzimuth,
	// The speed that the motor should move at in steps/second.
	firstSpeedAzimuth,
	secondSpeedAzimuth,
	// The speed that the motor should accelerate and decelerate at, in steps/second^2 (?)
	accelerationAzimuth,
	decelerationAzimuth,
	// Unused by the control room, but left here for completeness and code simplicity.
	motorCurrentAzimuth,
	jerkAzimuth,
	
	///
	/// ELEVATION REGISTERS:
	///
	firstCommandElevation,
	secondCommandElevation,
	firstDataElevation,
	secondDataElevation,
	firstSpeedElevation,
	secondSpeedElevation,
	accelerationElevation,
	decelerationElevation,
	motorCurrentElevation,
	jerkElevation,
}

// These are the register indicies and names that we use for the simulation.
// A renamed and trimmed down version of the MCUOutputRegs enum copied from
// the control room, which can be found in the MCUConstants file.
public enum OutgoingRegIndex : int
{
	// For some unknown reason, the index positions seen here are shifted up
	// by one compared to the indices on the control room. The control room
	// still understand what we're sending back, so evidently we're putting
	// data in the right place.
	statusAzimuth = 1,					// AZ_Status_Bits_MSW
	firstWordAzimuthSteps = 3,
	secondWordAzimuthSteps = 4,
	firstWordAzimuthEncoder = 5,
	secondWordAzimuthEncoder = 6,
	
	// Register 9 bit 14 should be flippped ever 0.5 seconds.
	// Register 9 bit 13 should be set if connection to the CR is lost.
	
	statusElevation = 11,				// EL_Status_Bits_MSW
	firstWordElevationSteps = 13,
	secondWordElevationSteps = 14,
	firstWordElevationEncoder = 15,
	secondWordElevationEncoder = 16,
}

// These are the bit positions and names that we use for the simulation.
// A renamed and trimmed down version of the MCUStatusBitsMSW enum copied from
// the control room, which can be found in the MCUConstants file.
public enum StatusBit : int
{
	// Set if the motor is moving in the negative or positive direction.
	negMoving = 0,			// CW_Motion
	posMoving = 1,			// CCW_Motion
	
	// Set if the motors are not moving.
	// NOTE FROM JONATHAN: The CR NEVER looks for this bit.
	stopped = 3,			// Axis_Stopped
	
	// Set if the telescope has successfully homed.
	homed = 4,				// At_Home
	
	// Set if a movement has successfully completed.
	complete = 7,			// Move_Complete
	
	// Set if the "limit swithes" are hit. The control room assumes that this
	// bit being set means a limit switch was hit. See MCUManager::MovementMonitor
	ivalidInput = 11,		// Input_Error
	
	///
	/// UNUSED STATUS BITS:
	///
	
	// Hold_State is never sent, as the control room never sends
	// Hold_Move that would cause this status bit to flip.
	holdState = 2,			// Hold_State
	
	// Accelerating and deceleration are unimplemented on the
	// simulation.
	// NOTE FROM JONATHAN: The CR NEVER looks for these bits.
	accelerating = 5,		// Move_Accelerating
	decelerating = 6,		// Move_Decelerating
	
	// Set if there's an error in homing. Potential failure scenario?
	invalidHome = 8,		// Home_Invalid_Error
	
	// Set if a bad blend move is sent, but we never use blend moves.
	invalidProfile = 9,		// Profile_Invalid
	
	// Set if a movement was prematurely stopped by an E-stop or limit switch.
	// Could be set if a limit switch is hit and the target is beyond the limit switch.
	// NOTE FROM JONATHAN: The CR NEVER looks for this bit.
	invalidPosition = 10,	// Position_Invalid
	
	// "Set when the last command issued to the ANF1/2 axis forced an error"
	invalidCommand = 12,	// Command_Error
	
	// "Set when the axis has a configuration error"
	// Failure scenario? Set this after receiving configure MCU?
	// NOTE FROM JONATHAN: The CR NEVER looks for this bit.
	invalidConfiguration = 13, // Configuration_Error
	
	// "Set when the axis is enabled. An axis is automatically enabled when valid
	// configuration data is written to it"
	// Could be set when config MCU is receive.
	// NOTE FROM JONATHAN: The CR NEVER looks for this bit.
	axisEnabled = 14, 		// Axis_Enabled
	
	// "Set to '1' when the axis is in Configuration Mode. Reset to '0' when the axis is in Command Mode"
	// Could be set when config MCU is receive, then reset later.
	// NOTE FROM JONATHAN: The CR NEVER looks for this bit.
	axisConfigMode = 15,	// Axis_Configuration_Mode
}
