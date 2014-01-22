
namespace GameDonkey
{
	/// <summary>
	/// These are all the different ways to get a direcion for an action
	/// </summary>
	public enum EDirectionType
	{
		Absolute, //the direction is exactly as stated
		Relative, //the direction is based on the direction the character is facing
		Controller, //the direction is based on the direction the controller is pointing
		NegController //the direction is the opposite of the direction the controller is pointing
	}
}