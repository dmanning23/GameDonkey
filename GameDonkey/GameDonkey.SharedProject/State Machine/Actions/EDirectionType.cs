
namespace GameDonkeyLib
{
	/// <summary>
	/// These are all the different ways to get a direcion for an action
	/// </summary>
	public enum EDirectionType
	{
		Absolute = 0, //the direction is exactly as stated
		Relative, //the direction is based on the direction the character is facing
		Velocity, //the direction is based on the direction the character is travelling
		Controller, //the direction is based on the direction the controller is pointing
		NegController //the direction is the opposite of the direction the controller is pointing
	}
}