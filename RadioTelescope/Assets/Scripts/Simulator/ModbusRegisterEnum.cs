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
	firstStatusAzimuth = 1,				// AZ_Status_Bits_MSW
	secondStatusAzimuth = 2,			// AZ_Status_Bist_LSW
	firstWordAzimuthSteps = 3,			// AZ_Current_Position_MSW
	secondWordAzimuthSteps = 4,			// AZ_Current_Position_LSW
	firstWordAzimuthEncoder = 5,		// AZ_MTR_Encoder_Pos_MSW
	secondWordAzimuthEncoder = 6,		// AZ_MTR_Encoder_Pos_LSW
	// 7, 8, 9 unused.
	heartbeat = 10,						// NetworkConnectivity
	// Register 10 bit 14 should be flippped every 0.5 seconds.
	// Register 10 bit 13 should be set if connection to the CR is lost.
	
	firstStatusElevation = 11,			// EL_Status_Bits_MSW
	secondStatusElevation = 12,			// EL_Status_Bist_LSW
	firstWordElevationSteps = 13,		// EL_Current_Position_MSW
	secondWordElevationSteps = 14,		// EL_Current_Position_LSW
	firstWordElevationEncoder = 15,		// EL_MTR_Encoder_Pos_MSW
	secondWordElevationEncoder = 16,	// EL_MTR_Encoder_Pos_LSW
	// 17, 18, 19, 20 unused
}

// These are the bit positions and names that we use for the simulation.
// A renamed and trimmed down version of the MCUStatusBitsMSW enum copied from
// the control room, which can be found in the MCUConstants file.
public enum StatusBit : int
{
	// Set if the motor is moving in the negative or positive direction.
	negMoving = 0,			// CW_Motion				0x0001
	posMoving = 1,			// CCW_Motion				0x0002
	
	// Set if the motors are not moving.
	// NOTE: The CR doesn't look for this bit.
	stopped = 3,			// Axis_Stopped				0x0008
	
	// Set if the motor is in the homed position.
	homed = 4,				// At_Home					0x0010
	
	// Set if the motors are accelerating or decelerating.
	// NOTE: proper acceleration and deceleration are not yet implemented.
	// The sim just takes the first third of a movement as accelerating,
	// the middle third as neither, and the last third as decelerating.
	// NOTE: The CR doesn't look for these bits.
	accelerating = 5,		// Move_Accelerating		0x0020
	decelerating = 6,		// Move_Decelerating		0x0040
	
	// Set if a relative movement command has successfully completed.
	complete = 7,			// Move_Complete			0x0080
	
	// Set if the hardware/simulation has been turned on but has yet to be homed.
	// Might also be hit by the hardware if a limit switch is hit.
	// NOTE: The CR doesn't look for this bit.
	invalidPosition = 10,	// Position_Invalid			0x0400
	
	// Set if the "limit swithes" are hit. The control room assumes that this
	// bit being set means a limit switch was hit. See MCUManager::MovementMonitor
	// for how that is handled.
	invalidInput = 11,		// Input_Error				0x0800
	
	// Always set active after receiving MCU configure
	// NOTE: The CR doesn't look for this bit.
	axisEnabled = 14, 		// Axis_Enabled				0x4000
	
	///
	/// UNUSED/UNIMPLEMENTED
	///
	
	// Hold_State would be flipped if the control room sent a Hold_Move
	// command, but the control room never does this.
	hold = 2,				// Hold_State				0x0004
	
	// This would presumably be set of there were an issue with homing, but
	// no such issue has ever been encountered. The control room
	// also isn't built to handle such an error, so testing it would be meaningless.
	invalidHome = 8,		// Home_Invalid_Error		0x0100
	
	// Profile_Invalid would be flipped if an invalid blend move were sent,
	// but the control room doesn't send blend moves.
	invalidProfile = 9,		// Profile_Invalid			0x0200
	
	// We should never received invalid commands from the control room.
	invalidCommand = 12,	// Command_Error			0x1000
	
	// Would presumably be flipped if configuration somehow fails. This hasn't
	// been observed on the hardware, and the control room isn't built
	// to handle such an error, so testing it would be meaningless.
	invalidConfig = 13,		// Configuration_Error		0x2000
	
	// Presumably set when the hardware starts up, but would be meaningless to
	// set on the simulation since the control room always sends a configure MCU
	// command upon connecting.
	// If ever this was set, we'd just have it flipped on as the simulation starts,
	// then flip it off after the configure MCU command is received.
	axisConfigMode = 15,	// Axis_Configuration_Mode	0x8000
}
