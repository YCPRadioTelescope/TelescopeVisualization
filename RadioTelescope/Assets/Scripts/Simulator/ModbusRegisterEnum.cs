// all of the information here I gathered during digging into what gets sent by each move type from the control room. The spreadsheet where I pulled these values can be found
// here: https://docs.google.com/spreadsheets/d/1vBKsnV7Xso0u19ZyhtVimiCXpZjc007usVjvAsxHJNU/edit#gid=0
// 
// last updated: LG - 4/22/2021

/// <summary>
/// This enum helps make the process of building MCUCommand objects more readable. There is a seperate enum for the regiser positions,
/// but for more internal documentation the comments above each enum type refer to where these values should be in the registerData coming over
/// from the control room
/// </summary>
enum MoveType
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
    HOME = 0x0040,

    // first register bit (registerData[0])
    SIM_SERVER_INIT = 0x0420,

    // first register bit (registerData[0])
    SIM_TELESCOPECONTROLLER_INIT = 0x0069,

    // first register bit (registerData[0])
    TEST_MOVE = 0x0096
}

/// <summary>
/// This enum lines up for the vast majority of the register data we send over, but there are some exceptions. For example, stop moves only use a couple of the registers
/// to send their information over, so the this entire list does not line up for them (the first or second word does this). The homing command or the reset errors command also do 
/// not line up with this list. The majority of moves (we really only use relative and jog moves) do line up with this list.
/// </summary>
enum RegPos
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
    firstAccelerationAzimuth,
    secondAccelerationAzimuth,

    ///
    /// NOTE: these registers are never used from the control room, so they very well might not line up with the names I've used below. From the data sheet,
    ///       based on the pattern of the previous registers my best guess is that these correspond to spots registerData[8] & registerData[9]
    ///       ( I think this is better than leaving them blank spots in the enum )
    ///
    firstDecelerationAzimuth,
    secondDecelerationAzimuth,

    ///
    /// ELEVATION REGISTERS
    ///
    firstWordElevation,
    secondWordElevation,
    firstPosElevation,
    secondPosElevation,
    firstSpeedElevation,
    secondSpeedElevation,
    firstAccelerationElevation,
    secondAccelerationElevation,

    ///
    /// same note as above, these are never used by the control room, but for completeness sake I have them listed here
    ///
    firstDecelerationElevation,
    secondDecelerationElevation,
}