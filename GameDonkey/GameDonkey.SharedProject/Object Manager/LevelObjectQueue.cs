using Microsoft.Xna.Framework;

namespace GameDonkeyLib
{
	public class LevelObjectQueue : PlayerQueue
	{
		public LevelObjectQueue() : base(Color.White)
		{
		}

		public override PlayerObject CreateAiPlayer()
		{
			throw new System.NotImplementedException();
		}

		public override PlayerObject CreateHumanPlayer()
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