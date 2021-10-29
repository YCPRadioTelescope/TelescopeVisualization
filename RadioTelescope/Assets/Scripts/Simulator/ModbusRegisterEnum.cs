// all of the information here I gathered during digging into what gets sent by each move type from the control room. The spreadsheet where I pulled these values can be found
// here: https://docs.google.com/spreadsheets/d/1vBKsnV7Xso0u19ZyhtVimiCXpZjc007usVjvAsxHJNU/edit#gid=0
// 
// last updated: LG - 4/27/2021

/// <summary>
/// This enum helps make the process of building MCUCommand objects more readable. There is a seperate enum for the regiser positions,
/// but for more internal documentation the comments above each enum type refer to where these values should be in the registerData coming over
/// from the control room
/// </summary>
public enum MoveType : ushort
{
	// first register bit (registerData[0])
	RELATIVE_MOVE = 0x0002,
	
	// first register bit (registerData[0])
	CLOCKWISE_AZIMTUH_JOG = 0x0080,
	
	// first register bit (registerData[0])
	COUNTERCLOCKWISE_AZIMUTH_JOG = 0x0100,
	
	// for this move, the first register bit will be 0x0000, we send the elevation jog information in the second half of the registerData ushort[] array
	// this value lines up with registerData[10]
	NEGATIVE_ELEVATION_JOG = 0x0080,
	
	// for this move, the first register bit will be 0x0000, we send the elevation jog information in the second half of the registerData ushort[] array
	// this value lines up with registerData[10]
	POSITIVE_ELEVATION_JOG = 0x0100,
	
	// first register bit (registerData[0])
	CANCEL_MOVE = 0x0003,
	
	// first register bit (registerData[0])
	CONTROLLED_STOP = 0x0004,
	
	// first register bit (registerData[0])
	IMMEDIATE_STOP = 0x0010,
	
	// first register bit (registerData[0])
	COUNTERCLOCKWISE_HOME = 0x0040,
	
	// first register bit (registerData[0])
	CLOCKWISE_HOME = 0x0020,
	
	// first register bit (registerData[0])
	CLEAR_MCU_ERRORS = 0x0800,
	
	// first register bit (registerData[0])
	CONFIGURE_MCU = 0x852c,
	
	// first register bit (registerData[0])
	SIM_TELESCOPECONTROLLER_INIT = 0x0045,
	
	// first register bit (registerData[0])
	TEST_MOVE = 0x01a4,
}

/// <summary>
/// This enum lines up for the vast majority of the register data we send over, but there are some exceptions. For example, stop moves only use a couple of the registers
/// to send their information over, so the this entire list does not line up for them (the first or second word does this). The homing command or the reset errors command also do 
/// not line up with this list. The majority of moves (we really only use relative and jog moves) do line up with this list.
/// </summary>
public enum RegPos : int
{
	///
	/// AZIMUTH REGISTERS
	/// 
	firstWordAzimuth,
	secondWordAzimuth,
	firstPosAzimuth,
	secondPosAzimuth,
	firstSpeedAzimuth,
	secondSpeedAzimuth,
	accelerationAzimuth,
	decelerationAzimuth,
	
	///
	/// NOTE: these registers are never used from the control room, so they very well might not line up with the names I've used below. From the data sheet,
	/// based on the pattern of the previous registers my best guess is that these correspond to spots registerData[8] & registerData[9]
	/// ( I think this is better than leaving them blank spots in the enum )
	///
	motorCurrentAzimuth,
	jerkAzimuth,
	
	///
	/// ELEVATION REGISTERS
	///
	firstWordElevation,
	secondWordElevation,
	firstPosElevation,
	secondPosElevation,
	firstSpeedElevation,
	secondSpeedElevation,
	accelerationElevation,
	decelerationElevation,
	
	///
	/// same note as above, these are never used by the control room, but for completeness sake I have them listed here
	///
	motorCurrentElevation,
	jerkElevation,
}

/// <summary>
/// This was taken directly from the control room (MCUConstants.cs)
/// Used to specify how we write back to the CR
/// </summary>
public enum MCUOutputRegs : ushort
{
	/// <summary>
	/// most signifigant word (16 bits) of the az axsys status <see cref="MCUStatusBitsMSW"/> for description of eacs bit
	/// </summary>
	AZ_Status_Bist_MSW = 0,
	/// <summary>
	/// least signifigant word (16 bits) of the az axsys status <see cref="MCUStutusBitsLSW"/> for description of eacs bit
	/// </summary>
	AZ_Status_Bist_LSW = 1,
	/// <summary>
	/// this is the position of the axsys in terms of motor step count (most signifigant word)
	/// </summary>
	AZ_Current_Position_MSW = 2,
	/// <summary>
	/// this is the position of the axsys in terms of motor step count (least signifigant word)
	/// </summary>
	AZ_Current_Position_LSW = 3,
	/// <summary>
	/// if we were using encoders on the motors this is where the data from those encoders would be
	/// </summary>
	AZ_MTR_Encoder_Pos_MSW=4,
	/// <summary>
	/// if we were using encoders on the motors this is where the data from those encoders would be
	/// </summary>
	AZ_MTR_Encoder_Pos_LSW = 5,
	/// <summary>
	/// if the MCU is told to capture the current position this is where that data will be stored
	/// </summary>
	AZ_Capture_Data_MSW=6,
	/// <summary>
	/// if the MCU is told to capture the current position this is where that data will be stored
	/// </summary>
	AZ_Capture_Data_LSW = 7,
	RESERVED1 =8,
	/// <summary>
	/// used to track network conectivity bit 14 of this register will flip every .5 seconds,
	/// bit 13 is set when the MCU looses or has previously lost ethernet conectivity
	/// </summary>
	NetworkConnectivity = 9,
	/// <summary>
	/// most signifigant word (16 bits) of the EL axsys status <see cref="MCUStatusBitsMSW"/> for description of eacs bit
	/// </summary>
	EL_Status_Bist_MSW = 10,
	/// <summary>
	/// least signifigant word (16 bits) of the EL axsys status <see cref="MCUStutusBitsLSW"/> for description of eacs bit
	/// </summary>
	EL_Status_Bist_LSW = 11,
	/// <summary>
	/// this is the position of the axsys in terms of motor step count (most signifigant word)
	/// </summary>
	EL_Current_Position_MSW = 12,
	/// <summary>
	/// this is the position of the axsys in terms of motor step count (least signifigant word)
	/// </summary>
	EL_Current_Position_LSW = 13,
	/// <summary>
	/// if we were using encoders on the motors this is where the data from those encoders would be
	/// </summary>
	EL_MTR_Encoder_Pos_MSW = 14,
	/// <summary>
	/// if we were using encoders on the motors this is where the data from those encoders would be
	/// </summary>
	EL_MTR_Encoder_Pos_LSW = 15,
	/// <summary>
	/// if the MCU is told to capture the current position this is where that data will be stored
	/// </summary>
	EL_Capture_Data_MSW = 16,
	/// <summary>
	/// if the MCU is told to capture the current position this is where that data will be stored
	/// </summary>
	EL_Capture_Data_LSW = 17,
	RESERVED2 = 18,
	RESERVED3 = 19
}

/// <summary>
/// taken from the control room, MCUConstants.cs
/// desciptions taken from anf1-anf2-motion-controller-user-manual.pdf  page 76 - 78
/// </summary>
public enum MCUStatusBitsMSW : int
{
	/// <summary>
	/// Set when the ANF1/2 axis is outputting pulses for clockwise motion
	/// </summary>
	CW_Motion = 0,
	/// <summary>
	/// Set when the ANF1/2 axis is outputting pulses for counter-clockwise motion
	/// </summary>
	CCW_Motion = 1,
	/// <summary>
	/// Set when the ANF1/2 axis has stopped motion as a result of a Hold Move Command
	/// </summary>
	Hold_State = 2,
	/// <summary>
	/// Set when the ANF1/2 axis is not in motion for any reason
	/// </summary>
	Axis_Stopped = 3,
	/// <summary>
	/// This bit is only set after the successful completion of a homing command
	/// </summary>
	At_Home = 4,
	/// <summary>
	/// Set when the ANF1/2 axis is accelerating during any move
	/// </summary>
	Move_Accelerating = 5,
	/// <summary>
	/// Set when the ANF1/2 axis is decelerating during any move
	/// </summary>
	Move_Decelerating = 6,
	/// <summary>
	/// Set when the ANF1/2 axis has successfully completed an Absolute, Relative,
	/// Blend, or Interpolated Move
	/// </summary>
	Move_Complete = 7,
	/// <summary>
	/// Set when the ANF1/2 could not home the axis because of an error durring homeing see MCU documaentation for list of potential eroorrs
	/// </summary>
	Home_Invalid_Error = 8,
	/// <summary>
	/// Set when there was an error in the last Program Blend Profile data block //we don't use blend move so this shouldnt come up
	/// </summary>
	Profile_Invalid = 9,
	/// <summary>
	/// this bit is set when the position stored in the MCU could be incorrect.
	/// set under the fowling conditions, Axis switched to Command Mode | An Immediate Stop command was issued | An Emergency Stop input was activated | CW or CCW Limit reached
	/// </summary>
	Position_Invalid = 10,
	/// <summary>
	/// see MCU documaentation for list of potential eroorrs
	/// </summary>
	Input_Error = 11,
	/// <summary>
	/// Set when the last command issued to the ANF1/2 axis forced an error
	/// </summary>
	Command_Error = 12,
	/// <summary>
	/// set when the axis has a configuration error
	/// </summary>
	Configuration_Error = 13,
	/// <summary>
	/// Set when the axis is enabled. Axis 2 of an ANF2 is disabled by default. An
	/// axis is automatically enabled when valid configuration data is written to it
	/// </summary>
	Axis_Enabled = 14,
	/// <summary>
	/// Set to “1” when the axis is in Configuration Mode. Reset to “0” when the axis is in Command Mode
	/// </summary>
	Axis_Configuration_Mode = 15,
}

public enum MCUWriteBack : ushort
{
	finishedMove = 128, 
	finishedHome = 16,
	stillMoving = 2
}

public enum WriteBackRegPos : int
{
	stillMovingAzimuth = 1,
	stillMovingElevation = 11,
	finishedMovingAzimuth = 1,
	finishedMovingElevation = 11,
	
	firstWordAzimuthSteps = 3,
	secondWordAzimuthSteps = 4,
	firstWordElevationSteps = 13,
	secondWordElevationSteps = 14,
	firstWordAzimuthEncoder = 5,
	secondWordAzimuthEncoder = 6,
	firstWordElevationEncoder = 15,
	secondWordElevationEncoder = 16
}