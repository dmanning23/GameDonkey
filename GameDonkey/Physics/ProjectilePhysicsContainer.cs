using AnimationLib;
using System.Diagnostics;

namespace GameDonkeyLib
{
	class ProjectilePhysicsContainer : BasePhysicsContainer
	{
		#region Methods

		public ProjectilePhysicsContainer(ProjectileObject projectileObject) : base(projectileObject)
		{
		}

		public override void SortBones(Bone bone)
		{
			if (bone.HasPhysicsData())
			{
				Weapons.Add(bone);

				CollisionBones.Add(bone);
			}

			//run through all the child bones
			for (int i = 0; i < bone.Bones.Count; i++)
			{
				SortBones(bone.Bones[i]);
			}

			BonesSorted = true;
		}
		protected override bool CheckPushCollisions(BasePhysicsContainer otherGuy)
		{
			return true;
		}
		protected override void IterateLevelCollisions(BasePhysicsContainer levelObject)
		{
			//check the all bones for collision
			for (var i = 0; i < CollisionBones.Count; i++)
			{
				CheckLevelCollision(CollisionBones[i], levelObject);
			}
		}

		#endregion //Methods
	}
}