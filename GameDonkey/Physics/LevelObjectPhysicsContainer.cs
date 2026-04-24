using System;

namespace GameDonkeyLib
{
	public class LevelObjectPhysicsContainer : BasePhysicsContainer
	{
		#region Methods
		public LevelObjectPhysicsContainer(LevelObject levelObject) : base(levelObject)
		{
		}
		public override void CheckCollisions(BasePhysicsContainer otherGuy)
		{
			throw new Exception("don't check level objects like this");
		}
		protected override bool CheckPushCollisions(BasePhysicsContainer otherGuy)
		{
			return true;
		}
		protected override void IterateLevelCollisions(BasePhysicsContainer levelObject)
		{
			throw new Exception("should never check level objects against other level objects");
		}

		#endregion //Methods
	}
}