using FilenameBuddy;
using System;
using System.Xml;

namespace GameDonkeyLib
{
	/// <summary>
	/// This is a model of a game object that gets added to the game as an "instanced object"
	/// These items aren't stored in the playerQueue, they are discarded after being removed.
	/// </summary>
	public class InstanceObjectModel : BaseObjectModel
	{
		/// <summary>
		/// hello, standard constructor!
		/// </summary>
		public InstanceObjectModel(Filename filename) : base("model", filename)
		{
		}
	}
}
