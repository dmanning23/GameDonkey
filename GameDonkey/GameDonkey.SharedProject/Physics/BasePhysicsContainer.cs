using AnimationLib;
using CollisionBuddy;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GameDonkeyLib
{
	public abstract class BasePhysicsContainer
	{
		#region Properties

		/// <summary>
		/// the guy that owns this dude
		/// </summary>
		public BaseObject Owner { get; private set; }

		/// <summary>
		/// the list of hits for this dude
		/// </summary>
		public Hit[] Hits { get; protected set; }

		/// <summary>
		/// flag for whether the hits are active this frame
		/// </summary>
		public bool[] HitFlags { get; protected set; }

		/// <summary>
		/// A list of bones that have collision information and are labelled as "weapons"
		/// </summary>
		public List<Bone> Weapons { get; private set; }

		/// <summary>
		/// A list of bones that have collision information and are labelled as "feet"
		/// </summary>
		public List<Bone> Feet { get; private set; }

		/// <summary>
		/// A list of bones that have collision information and aren't weapons
		/// </summary>
		public List<Bone> CollisionBones { get; private set; }

		/// <summary>
		/// Whether or not the bones have been sorted
		/// </summary>
		protected bool BonesSorted { get; set; }

		#endregion //

		#region Methods

		/// <summary>
		/// constructor
		/// </summary>
		/// <param name="object">the dude who owns this physics container</param>
		public BasePhysicsContainer(BaseObject baseObject)
		{
			BonesSorted = false;
			Owner = baseObject;
			HitFlags = new bool[(int)EHitType.NumHits];

			Hits = new Hit[(int)EHitType.NumHits];
			for (int i = 0; i < (int)EHitType.NumHits; i++)
			{
				Hits[i] = new Hit();
			}

			Weapons = new List<Bone>();
			Feet = new List<Bone>();
			CollisionBones = new List<Bone>();

			Reset();
		}

		/// <summary>
		/// Clear out all the physics data.
		/// </summary>
		public void ClearPhysicsLists()
		{
			Feet.Clear();
			Weapons.Clear();
			CollisionBones.Clear();
			BonesSorted = false;
		}

		/// <summary>
		/// sort the bones into the appropriate lists at startup
		/// </summary>
		/// <param name="bone"></param>
		public virtual void SortBones(Bone bone)
		{
			if (bone.HasPhysicsData())
			{
				//what sort of bone is it?

				//is it a weapon?
				if (bone.IsWeapon)
				{
					Weapons.Add(bone);
				}
				else
				{
					//is it a foot?
					if (bone.IsFoot)
					{
						Feet.Add(bone);
					}

					//All bones that have collsion info but aren't weapons get added to this list
					CollisionBones.Add(bone);
				}
			}

			//run through all the child bones
			for (int i = 0; i < bone.Bones.Count; i++)
			{
				SortBones(bone.Bones[i]);
			}

			BonesSorted = true;
		}

		/// <summary>
		/// The garment of the owner changed, resort the physics lists
		/// This gets called AFTER the garment has been added or removed from the model
		/// </summary>
		public void GarmentChange(Garment garment)
		{
			if (garment.HasPhysics)
			{
				ClearPhysicsLists();
				SortBones(Owner.AnimationContainer.Skeleton.RootBone);
			}
		}

		/// <summary>
		/// Reset all the hits to false
		/// </summary>
		public void Reset()
		{
			for (int i = 0; i < (int)EHitType.NumHits; i++)
			{
				HitFlags[i] = false;
			}
		}

		/// <summary>
		/// Replace all the base object pointers in this dude to point to a replacement object
		/// </summary>
		/// <param name="bot">the replacement dude</param>
		public void ReplaceOwner(BaseObject bot)
		{
			Owner = bot;
		}

		/// <summary>
		/// Find a weapon bone at runtime
		/// </summary>
		/// <param name="weaponName">name of the bone to find</param>
		/// <returns>bone with matching name, else null</returns>
		public Bone FindWeapon(string weaponName)
		{
			foreach (var weapon in Weapons)
			{
				if (weaponName == weapon.Name)
				{
					return weapon;
				}
			}

			//no matching bone found!
			return null;
		}

		#region Collision Methods

		/// <summary>
		/// Check for collisions against another dude
		/// </summary>
		/// <param name="otherGuy"></param>
		public virtual void CheckCollisions(BasePhysicsContainer otherGuy)
		{
			if (GameObjectType.Level.ToString() == otherGuy.Owner.ObjectType)
			{
				//recurse through the whole model, starting with the root bone
				IterateLevelCollisions(otherGuy);
			}
			else
			{
				//check if the other guy should be pushed away, or if we should quit checking
				if (!CheckPushCollisions(otherGuy))
				{
					//The objects are so far apart, there is no point in checking for further 
					return;
				}

				//Check if any attacks are blocked
				IterateBlockedAttacks(otherGuy);

				//if not a level object, check for attacks
				IterateAttackCollisions(otherGuy);
			}
		}

		/// <summary>
		/// check if a push hit occurs between me an another guy, or if we even need to check collisions
		/// </summary>
		/// <param name="otherGuy">the guy to check against</param>
		/// <returns>bool: whether or not I should even check for collisions between these two objects</returns>
		protected abstract bool CheckPushCollisions(BasePhysicsContainer otherGuy);

		/// <summary>
		/// Recursive function to check if the owner is hitting a level object
		/// </summary>
		/// <param name="levelObjects">the level object to check against</param>
		protected abstract void IterateLevelCollisions(BasePhysicsContainer levelObjects);

		/// <summary>
		/// recursive function check if a single bone is hitting another bone in a level object.
		/// </summary>
		/// <param name="bone">my bone</param>
		/// <param name="levelObjects">the level object being checked against</param>
		protected virtual void CheckLevelCollision(Bone bone, BasePhysicsContainer levelObjects)
		{
			//get the image we are checking of this bone
			var image = bone.GetCurrentImage();
			if (null == image)
			{
				return;
			}

			//loop through the bones of the level object
			foreach (var otherBone in levelObjects.CollisionBones)
			{
				var otherImage = otherBone.GetCurrentImage();
				if (null == otherImage)
				{
					continue;
				}

				//Do the actual collision check

				//loop through all polygons, checking for collisions
				var first = Vector2.Zero;
				var second = Vector2.Zero;
				for (int i = 0; i < image.Circles.Count; i++)
				{
					//check my circles against his lines
					for (int j = 0; j < otherImage.Lines.Count; j++)
					{
						//Check if there is a regular collision
						if (CollisionCheck.CircleLineCollision(image.Circles[i], otherImage.Lines[j], ref first, ref second))
						{
							//A collisoin occurred, parse it in the level object
							levelObjects.Owner.CollisionResponse(this, null, second, first);
						}
					}
				}
			}
		}

		private void IterateBlockedAttacks(BasePhysicsContainer otherGuy)
		{
			//Check my attacks against the other character's blocks
			var attackIndex = 0;
			while (attackIndex < Owner.CurrentAttacks.Count)
			{
				if (CheckBlockedAttack(Owner.CurrentAttacks[attackIndex], otherGuy))
				{
					//the attack was blocked, remove from the list
					if (!Owner.RemoveAttack(attackIndex))
					{
						++attackIndex;
					}
				}
				else
				{
					++attackIndex;
				}
			}

			//check the other charcters attacks against my dude's blocks
			var otherAttackIndex = 0;
			while (otherAttackIndex < otherGuy.Owner.CurrentAttacks.Count)
			{
				if (otherGuy.CheckBlockedAttack(otherGuy.Owner.CurrentAttacks[otherAttackIndex], this))
				{
					//the attack was blocked, remove from the list
					if (!otherGuy.Owner.RemoveAttack(otherAttackIndex))
					{
						++otherAttackIndex;
					}
				}
				else
				{
					++otherAttackIndex;
				}
			}
		}

		/// <summary>
		/// recursively check one of my attacks against another characters current blocks
		/// </summary>
		/// <param name="attack">my attack to check</param>
		/// <param name="otherGuy">the other guys physics container</param>
		/// <returns>bool: true if the attack connected, false if there was no connection</returns>
		private bool CheckBlockedAttack(CreateAttackAction attack, BasePhysicsContainer otherGuy)
		{
			foreach (var otherBlock in otherGuy.Owner.CurrentBlocks.CurrentActions)
			{
				//make sure this attack has a circle
				if ((null == attack.GetCircle()) || (null == otherBlock.GetCircle()))
				{
					//no collision occured because that attack bone isn't even being displayed
					return false;
				}

				//check my circle against his circles
				if (CollisionCheck.CircleCircleCollision(attack.GetCircle(), otherBlock.GetCircle()))
				{
					//A collisoin occurred, add a collision to this container
					var first = Vector2.Zero;
					var second = Vector2.Zero;
					CollisionCheck.ClosestPoints(attack.GetCircle(), otherBlock.GetCircle(), ref first, ref second);
					Owner.BlockResponse(otherGuy, attack, otherBlock, first, second);
					return true;
				}
			}

			//no collisions occured between the attack and the other guy
			return false;
		}


		private void IterateAttackCollisions(BasePhysicsContainer otherGuy)
		{
			//check my attacks against his attacks to check for weapon clashes
			var myAttackIndex = 0;
			var otherAttackIndex = 0;
			if ((Owner.CurrentAttacks.Count > 0) && (otherGuy.Owner.CurrentAttacks.Count > 0))
			{
				//check my list of attacks against the other dudes list of attacks
				while (myAttackIndex < Owner.CurrentAttacks.Count)
				{
					//get my attack
					var myAttack = Owner.CurrentAttacks[myAttackIndex];

					//get my circle
					var myCircle = myAttack.GetCircle();
					if (null == myCircle)
					{
						//no collision occured because that attack bone isn't even being displayed
						++myAttackIndex;
						continue;
					}

					otherAttackIndex = 0;
					while (otherAttackIndex < otherGuy.Owner.CurrentAttacks.Count)
					{
						//get his attack
						var hisAttack = otherGuy.Owner.CurrentAttacks[otherAttackIndex];

						//get his circle
						var hisCircle = hisAttack.GetCircle();
						if (null == hisCircle)
						{
							//no collision occured because that attack bone isn't even being displayed
							++otherAttackIndex;
							continue;
						}

						//do those attacks hit each other?

						//check my circle against his circle

						if (CollisionCheck.CircleCircleCollision(myCircle, hisCircle))
						{
							//A weapon clash collisoin occurred!

							var first = Vector2.Zero;
							var second = Vector2.Zero;
							CollisionCheck.ClosestPoints(myCircle, hisCircle, ref first, ref second);

							//add a weapon hit collision to this container
							Owner.WeaponCollisionResponse(otherGuy, myAttack, first, second);

							//add a weapon hit collision to the other dude container
							otherGuy.Owner.WeaponCollisionResponse(this, hisAttack, second, first);

							Owner.RemoveAttack(myAttackIndex);
							otherGuy.Owner.RemoveAttack(otherAttackIndex);

							//just one weapon hit per check
							return;
						}

						++otherAttackIndex;
					}

					++myAttackIndex;
				}
			}

			//Check my attacks against the other character
			myAttackIndex = 0;
			while (myAttackIndex < Owner.CurrentAttacks.Count)
			{
				if (CheckAttackCollisions(Owner.CurrentAttacks[myAttackIndex], otherGuy))
				{
					//the attack connected, remove from the list
					if (!Owner.RemoveAttack(myAttackIndex))
					{
						++myAttackIndex;
					}
				}
				else
				{
					++myAttackIndex;
				}
			}

			//check the other charcters attacks against my dude
			otherAttackIndex = 0;
			while (otherAttackIndex < otherGuy.Owner.CurrentAttacks.Count)
			{
				if (otherGuy.CheckAttackCollisions(otherGuy.Owner.CurrentAttacks[otherAttackIndex], this))
				{
					//the attack connected, remove from the list
					if (!otherGuy.Owner.RemoveAttack(otherAttackIndex))
					{
						++otherAttackIndex;
					}
				}
				else
				{
					++otherAttackIndex;
				}
			}
		}

		/// <summary>
		/// recursively check one of my attacks against another characters skeletal structure
		/// </summary>
		/// <param name="attack">my attack to check</param>
		/// <param name="otherGuy">the other guys physics container</param>
		/// <returns>bool: true if the attack connected, false if there was no connection</returns>
		private bool CheckAttackCollisions(CreateAttackAction attack, BasePhysicsContainer otherGuy)
		{
			foreach (var otherBone in otherGuy.CollisionBones)
			{
				//make sure they have images
				var otherImage = otherBone.GetCurrentImage();
				if (null == otherImage)
				{
					return false;
				}

				//can't attack feet or weapons
				if (otherBone.IsFoot || otherBone.IsWeapon)
				{
					return false;
				}

				//make sure this attack has a circle
				var myCircle = attack.GetCircle();
				if (null == myCircle)
				{
					//no collision occured because that attack bone isn't even being displayed
					return false;
				}

				//check my circle against his circles
				var first = Vector2.Zero;
				var second = Vector2.Zero;
				for (int i = 0; i < otherImage.Circles.Count; i++)
				{
					if (CollisionCheck.CircleCircleCollision(myCircle, otherImage.Circles[i]))
					{
						//A collisoin occurred, add a collision to this container
						CollisionCheck.ClosestPoints(myCircle, otherImage.Circles[i], ref first, ref second);
						Owner.CollisionResponse(otherGuy, attack, first, second);
						return true;
					}
				}
			}

			//no collisions occured between the attack and the other guy
			return false;
		}

		/// <summary>
		/// Check if this object is colliding with the world
		/// </summary>
		/// <param name="velocity">the current velocity of this dude</param>
		/// <param name="worldBoundaries">rectangle of teh world boundaries</param>
		public void CheckWorldCollisions(Vector2 velocity, Rectangle worldBoundaries)
		{
			float velocityLength = (velocity.Length() * Owner.CharacterClock.TimeDelta);

			//get the velocity direction if it isn't zero
			var VelocityDirection = velocity;
			if (velocityLength > 0.0f)
			{
				VelocityDirection.Normalize();
			}

			//check children of other bone for collisions
			foreach (var bone in CollisionBones)
			{
				//get the images we are checking
				var image = bone.GetCurrentImage();
				if (null == image)
				{
					continue;
				}

				//Do the actual collision check

				//loop through all polygons, checking for collisions
				for (var i = 0; i < image.Circles.Count; i++)
				{
					//get the bottom of the polygon
					var bottom = image.Circles[i].Pos.Y + image.Circles[i].Radius;

					//check for fast ground hits
					var fastBottom = bottom + (VelocityDirection.Y * velocityLength);
					if (fastBottom > worldBoundaries.Bottom)
					{
						//a floor hit occured

						//get the delta between the current pos and the ground
						var overlap = (worldBoundaries.Bottom - bottom);
						if (!HitFlags[(int)EHitType.GroundHit] || (Math.Abs(overlap) > Math.Abs(Hits[(int)EHitType.GroundHit].Strength)))
						{
							HitFlags[(int)EHitType.GroundHit] = true;
							Hits[(int)EHitType.GroundHit].Set(
								new Vector2(0.0f, 1.0f),
								null,
								overlap,
								EHitType.GroundHit,
								null,
								new Vector2(image.Circles[i].Pos.X, worldBoundaries.Bottom));
						}
					}
					else
					{
						//get the top of the polygon (will be fast bottom plus 2 * radius
						var fastTop = fastBottom - (2.0f * image.Circles[i].Radius);
						if (fastTop < worldBoundaries.Top)
						{
							//a ceiling hit occured

							//get the top of the polygon
							var top = image.Circles[i].Pos.Y - image.Circles[i].Radius;

							var overlap = (top - worldBoundaries.Top);
							if (!HitFlags[(int)EHitType.CeilingHit] || (Math.Abs(overlap) > Math.Abs(Hits[(int)EHitType.CeilingHit].Strength)))
							{
								HitFlags[(int)EHitType.CeilingHit] = true;
								Hits[(int)EHitType.CeilingHit].Set(
									new Vector2(0.0f, -1.0f),
									null,
									overlap,
									EHitType.CeilingHit,
									null,
									new Vector2(image.Circles[i].Pos.X, worldBoundaries.Top));
							}
						}
					}

					//get the right edge of the polygon
					var right = image.Circles[i].Pos.X + image.Circles[i].Radius;

					//check for fast right wall hits
					var fastRight = right + (VelocityDirection.X * velocityLength);
					if (fastRight > worldBoundaries.Right)
					{
						//a right wall hit occured

						//TODO: checking abs values is no good... will always get the bone FARTHEST AWAY!

						//get the delta between the current pos and the right wall
						var overlap = (worldBoundaries.Right - fastRight);
						if (!HitFlags[(int)EHitType.RightWallHit] || (Math.Abs(overlap) > Math.Abs(Hits[(int)EHitType.RightWallHit].Strength)))
						{
							HitFlags[(int)EHitType.RightWallHit] = true;
							Hits[(int)EHitType.RightWallHit].Set(
								new Vector2(1.0f, 0.0f),
								null,
								overlap,
								EHitType.RightWallHit,
								null,
								new Vector2(worldBoundaries.Right, image.Circles[i].Pos.Y));
						}
					}
					else
					{
						//get the left of the polygon (will be fast right plus 2 * radius
						var fastLeft = fastRight - (2.0f * image.Circles[i].Radius);
						if (fastLeft < worldBoundaries.Left)
						{
							//a left wall hit occured

							//get the left edge of the polygon
							var left = image.Circles[i].Pos.X - image.Circles[i].Radius;

							var overlap = (left - worldBoundaries.Left);
							if (!HitFlags[(int)EHitType.LeftWallHit] || (Math.Abs(overlap) > Math.Abs(Hits[(int)EHitType.LeftWallHit].Strength)))
							{
								HitFlags[(int)EHitType.LeftWallHit] = true;
								Hits[(int)EHitType.LeftWallHit].Set(
									new Vector2(-1.0f, 0.0f),
									null,
									overlap,
									EHitType.LeftWallHit,
									null,
									new Vector2(worldBoundaries.Left, image.Circles[i].Pos.Y));
							}
						}
					}

					//check for fast wall hits
				}
			}
		}

		/// <summary>
		/// Check if a circle is hitting this dude
		/// </summary>
		/// <param name="circle"></param>
		/// <returns></returns>
		public bool CheckCircleCollision(Circle circle)
		{
			//loop through all my collision bones
			for (var i = 0; i < CollisionBones.Count; i++)
			{
				//check this bone for collision
				//get the image we are checking of this bone
				var image = CollisionBones[i].GetCurrentImage();
				if (null == image)
				{
					continue;
				}

				//Do the actual collision check

				//loop through all polygons, checking for collisions
				for (var j = 0; j < image.Circles.Count; j++)
				{
					if (CollisionCheck.CircleCircleCollision(image.Circles[j], circle))
					{
						return true;
					}
				}
			}

			return false;
		}

		#endregion //Collision Methods

		#endregion //Methods
	}
}