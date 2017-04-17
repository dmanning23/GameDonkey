using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace GameDonkeyLib
{
	public class LevelObjectQueue : PlayerQueue
	{
		public LevelObjectQueue() : base(Color.White, 0)
		{
		}

		public override void Reset()
		{
			Debug.Assert(null == m_rCharacter);
			Debug.Assert(0 == m_listInactive.Count);

			//reset all the level objects
			for (int i = 0; i < m_listActive.Count; i++)
			{
				//reset the thing back to it's start state
				m_listActive[i].Reset();
			}
		}
	}
}