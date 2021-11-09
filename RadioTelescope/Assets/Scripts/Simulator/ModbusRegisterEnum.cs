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
	// 2 = LSW status
	firstWordAzimuthSteps = 3,
	secondWordAzimuthSteps = 4,
	firstWordAzimuthEncoder = 5,
	secondWordAzimuthEncoder = 6,
	// 7, 8, 9 unused.
	// 10 is heartbeat.
	// Register 10 bit 14 should be flippped every 0.5 seconds.
	// Register 10 bit 13 should be set if connection to the CR is lost.
	
	statusElevation = 11,				// EL_Status_Bits_MSW
	// 12 = LSW status
	firstWordElevationSteps = 13,
	secondWordElevationSteps = 14,
	firstWordElevationEncoder = 15,
	secondWordElevationEncoder = 16,
	// 17, 18, 19, 20 unused
}

// These are the bit positions and names that we use for the simulation.
// A renamed and trimmed down version of the MCUStatusBitsMSW enum copied from
// the control room, which can be found in the MCUConstants file.
public enum StatusBit : int
{
	// Set if the motor is moving in the negative or positive direction.
	negMoving = 0,			// CW_Motion				0x1
	posMoving = 1,			// CCW_Motion				0x2
	
	// Set if the motors are not moving.
	// NOTE: The CR doesn't look for this bit.
	stopped = 3,			// Axis_Stopped				0x8
	
	// Set if the motor is in the homed position.
	homed = 4,				// At_Home					0x10
	
	// Set if the motors are accelerating or decelerating.
	// NOTE: proper acceleration and deceleration are not yet implemented.
	// The sim just takes the first third of a movement as accelerating,
	// the middle third as neither, and the last third as decelerating.
	// NOTE: The CR doesn't look for these bits.
	accelerating = 5,		// Move_Accelerating		0x20
	decelerating = 6,		// Move_Decelerating		0x40
	
	// Set if a relative movement command has successfully completed.
	complete = 7,			// Move_Complete			0x80
	
	// Set if the "limit swithes" are hit. The control room assumes that this
	// bit being set means a limit switch was hit. See MCUManager::MovementMonitor
	ivalidInput = 11,		// Input_Error				0x800
	
	// Always set active after receiving MCU configure
	// NOTE: The CR doesn't look for this bit.
	axisEnabled = 14, 		// Axis_Enabled				0x4000
	
	///
	/// UNUSED STATUS BITS:
	///
	
	// Hold_State is never sent, as the control room never sends.
	// Hold_Move that would cause this status bit to flip.
	// NOTE: The CR doesn't look for this bit.
	holdState = 2,			// Hold_State				0x4
	
	// Set if there's an error in homing. Potential failure scenario?
	invalidHome = 8,		// Home_Invalid_Error		0x100
	
	// Set if a bad blend move is sent, but we never use blend moves.
	invalidProfile = 9,		// Profile_Invalid			0x200
	
	// Set if a movement was prematurely stopped by an E-stop or limit switch.
	// Could be set if a limit switch is hit and the target is beyond the limit switch.
	// NOTE: The CR doesn't look for this bit.
	invalidPosition = 10,	// Position_Invalid			0x400
	
	// "Set when the last command issued to the ANF1/2 axis forced an error"
	invalidCommand = 12,	// Command_Error			0x1000
	
	// "Set when the axis has a configuration error"
	// Failure scenario? Set this after receiving configure MCU?
	// NOTE: The CR doesn't look for this bit.
	invalidConfig = 13,		// Configuration_Error		0x2000
	
	// "Set to '1' when the axis is in Configuration Mode. Reset to '0' when the axis is in Command Mode"
	// Could be set when config MCU is receive, then reset later.
	// NOTE: The CR doesn't look for this bit.
	axisConfigMode = 15,	// Axis_Configuration_Mode	0x8000
}
