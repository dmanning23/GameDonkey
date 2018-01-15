using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace GameDonkeyLib
{
	public class LevelObjectQueue : PlayerQueue
	{
		public LevelObjectQueue() : base(Color.White, 0)
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
			Debug.Assert(null == Character);
			Debug.Assert(0 == Inactive.Count);

			//reset all the level objects
			for (int i = 0; i < Active.Count; i++)
			{
				//reset the thing back to it's start state
				Active[i].Reset();
			}
		}
	}
}