using Microsoft.Xna.Framework;

namespace GameDonkeyLib
{
	public class LevelObjectQueue : PlayerQueue
	{
		public LevelObjectQueue() : base(Color.White)
		{
		}

		public override PlayerObject CreateAiPlayer(string name)
		{
			throw new System.NotImplementedException();
		}

		public override PlayerObject CreateHumanPlayer(string name)
		{
			throw new System.NotImplementedException();
		}

		public override void Reset()
		{
			//reset all the level objects
			for (int i = 0; i < Active.Count; i++)
			{
				//reset the thing back to it's start state
				Active[i].Reset();
			}
		}
	}
}