using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using System;
using AnimationLib;
using CollisionBuddy;

namespace GameDonkey
{
	public abstract class IPhysicsContainer
	{
		#region Members

		/// <summary>
		/// the guy that owns this dude
		/// </summary>
		private BaseObject m_Object;

		/// <summary>
		/// the list of hits for this dude
		/// </summary>
		protected Hit[] m_rgHits;

		/// <summary>
		/// flag for whether the hits are active this frame
		/// </summary>
		protected bool[] m_rgHitFlags;

		/// <summary>
		/// A list of bones that have collision information and are labelled as "weapons"
		/// </summary>
		private List<Bone> m_listWeaponBones;

		/// <summary>
		/// A list of bones that have collision information and are labelled as "feet"
		/// </summary>
		private List<Bone> m_listFootBones;

		/// <summary>
		/// A list of bones that have collision information and aren't weapons
		/// </summary>
		private List<Bone> m_listCollisionBones;

		/// <summary>
		/// Whether or not the bones have been sorted
		/// </summary>
		private bool m_bBonesSorted;

		#endregion

		#region Properties

		public BaseObject Owner
		{
			get { return m_Object; }
		}

		public Hit[] Hits
		{
			get { return m_rgHits; }
		}

		public bool[] HitFlags
		{
			get { return m_rgHitFlags; }
		}

		protected List<Bone> Feet
		{
			get 
			{
				Debug.Assert(true == m_bBonesSorted);
				return m_listFootBones; 
			}
		}

		protected List<Bone> Weapons
		{
			get 
			{
				Debug.Assert(true == m_bBonesSorted);
				return m_listWeaponBones; 
			}
		}

		protected List<Bone> CollisionBones
		{
			get
			{
				Debug.Assert(true == m_bBonesSorted);
				return m_listCollisionBones; 
			}
		}

		#endregion //Properties

		#region Methods

		/// <summary>
		/// constructor
		/// </summary>
		/// <param name="rObject">the dude who owns this physics container</param>
		public IPhysicsContainer(BaseObject rObject)
		{
			m_bBonesSorted = false;
			m_Object = rObject;
			Debug.Assert(null != rObject);
			m_rgHitFlags = new bool[(int)EHitType.NumHits];

			m_rgHits = new Hit[(int)EHitType.NumHits];
			for (int i = 0; i < (int)EHitType.NumHits; i++)
			{
				m_rgHits[i] = new Hit();
			}

			m_listWeaponBones = new List<Bone>();
			m_listFootBones = new List<Bone>();
			m_listCollisionBones = new List<Bone>();

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
			m_bBonesSorted = false;
		}

		/// <summary>
		/// sort the bones into the appropriate lists at startup
		/// </summary>
		/// <param name="rBone"></param>
		public void SortBones(Bone rBone)
		{
			if (rBone.HasPhysicsData())
			{
				//what sort of bone is it?

				//is it a weapon?
				if (rBone.IsWeapon)
				{
					m_listWeaponBones.Add(rBone);
				}
				else
				{
					//is it a foot?
					if (rBone.IsFoot)
					{
						m_listFootBones.Add(rBone);
					}

					//All bones that have collsion info but aren't weapons get added to this list
					m_listCollisionBones.Add(rBone);
				}
			}

			//run through all the child bones
			for (int i = 0; i < rBone.Bones.Count; i++)
			{
				SortBones(rBone.Bones[i]);
			}

			m_bBonesSorted = true;
		}

		/// <summary>
		/// The garment of the owner changed, resort the physics lists
		/// This gets called AFTER the garment has been added or removed from the model
		/// </summary>
		public void GarmentChange(Garment rGarment)
		{
			if (rGarment.HasPhysics)
			{
				ClearPhysicsLists();
				SortBones(Owner.AnimationContainer.Model);
			}
		}

		/// <summary>
		/// Reset all the hits to false
		/// </summary>
		public void Reset()
		{
			for (int i = 0; i < (int)EHitType.NumHits; i++)
			{
				m_rgHitFlags[i] = false;
			}
		}

		/// <summary>
		/// Replace all the base object pointers in this dude to point to a replacement object
		/// </summary>
		/// <param name="myBot">the replacement dude</param>
		public void ReplaceOwner(BaseObject myBot)
		{
			m_Object = myBot;
		}

		/// <summary>
		/// Find a weapon bone at runtime
		/// </summary>
		/// <param name="strWeaponName">name of the bone to find</param>
		/// <returns>bone with matching name, else null</returns>
		public Bone FindWeapon(string strWeaponName)
		{
			foreach (Bone curBone in m_listWeaponBones)
			{
				if (strWeaponName == curBone.Name)
				{
					return curBone;
				}
			}

			//no matching bone found!
			return null;
		}

		#region Collision Methods

		/// <summary>
		/// Check for collisions against another dude
		/// </summary>
		/// <param name="rOtherGuy"></param>
		public virtual void CheckCollisions(IPhysicsContainer rOtherGuy)
		{
			Debug.Assert(true == m_bBonesSorted);
			Debug.Assert(null != m_Object);
			Debug.Assert(null != rOtherGuy.m_Object);
			Debug.Assert(EObjectType.Level != Owner.Type);

			if (EObjectType.Level == rOtherGuy.Owner.Type)
			{
				//recurse through the whole model, starting with the root bone
				IterateLevelCollisions(rOtherGuy);
			}
			else
			{
				//check if the other guy should be pushed away, or if we should quit checking
				if (!CheckPushCollisions(rOtherGuy))
				{
					//The objects are so far apart, there is no point in checking for further 
					return;
				}

				//level objects should always be passed into this function as parameter
				Debug.Assert(Owner.Type != EObjectType.Level);

				//Check if any attacks are blocked
				IterateBlockedAttacks(rOtherGuy);

				//if not a level object, check for attacks
				IterateAttackCollisions(rOtherGuy);
			}
		}

		/// <summary>
		/// check if a push hit occurs between me an another guy, or if we even need to check collisions
		/// </summary>
		/// <param name="rOtherGuy">the guy to check against</param>
		/// <returns>bool: whether or not I should even check for collisions between these two objects</returns>
		protected abstract bool CheckPushCollisions(IPhysicsContainer rOtherGuy);

		/// <summary>
		/// Recursive function to check if the owner is hitting a level object
		/// </summary>
		/// <param name="rLevelObject">the level object to check against</param>
		protected abstract void IterateLevelCollisions(IPhysicsContainer rLevelObject);

		/// <summary>
		/// recursive function check if a single bone is hitting another bone in a level object.
		/// </summary>
		/// <param name="rBone">my bone</param>
		/// <param name="rLevelObject">the level object being checked against</param>
		protected virtual void CheckLevelCollision(Bone rBone, IPhysicsContainer rLevelObject)
		{
			Debug.Assert(true == m_bBonesSorted);
			Debug.Assert(EObjectType.Level == rLevelObject.Owner.Type);
			Debug.Assert(rLevelObject != this);
			Debug.Assert(null != rBone);

			//get the image we are checking of this bone
			Image rImage = rBone.GetCurrentImage();
			if (null == rImage)
			{
				return;
			}

			//make sure that I dont have any lines, level objects always have to be passed to players
			Debug.Assert(rImage.Lines.Count == 0);

			//loop through the bones of the level object
			foreach (Bone rOtherBone in CollisionBones)
			{
				Image rOtherImage = rOtherBone.GetCurrentImage();
				if (null == rOtherImage)
				{
					continue;
				}

				//Do the actual collision check

				//loop through all polygons, checking for collisions
				Vector2 first = Vector2.Zero;
				Vector2 second = Vector2.Zero;
				for (int i = 0; i < rImage.Circles.Count; i++)
				{
					//check my circles against his lines
					for (int j = 0; j < rOtherImage.Lines.Count; j++)
					{
						//Check if there is a regular collision
						if (rImage.Circles[i].IsColliding(rOtherImage.Lines[j], ref first, ref second))
						{
							//A collisoin occurred, parse it in the level object
							rLevelObject.Owner.CollisionResponse(this, null, second, first);
						}
					}
				}
			}
		}

		private void IterateBlockedAttacks(IPhysicsContainer rOtherGuy)
		{
			Debug.Assert(true == m_bBonesSorted);

			//Check my attacks against the other character's blocks
			int iMyAttackIndex = 0;
			while (iMyAttackIndex < Owner.CurrentAttacks.Count)
			{
				if (CheckBlockedAttack(Owner.CurrentAttacks[iMyAttackIndex], rOtherGuy))
				{
					//the attack was blocked, remove from the list
					Owner.RemoveAttack(iMyAttackIndex);
				}
				else
				{
					++iMyAttackIndex;
				}
			}

			//check the other charcters attacks against my dude's blocks
			int iHisAttackIndex = 0;
			while (iHisAttackIndex < rOtherGuy.Owner.CurrentAttacks.Count)
			{
				if (rOtherGuy.CheckBlockedAttack(rOtherGuy.Owner.CurrentAttacks[iHisAttackIndex], this))
				{
					//the attack was blocked, remove from the list
					rOtherGuy.Owner.RemoveAttack(iHisAttackIndex);
				}
				else
				{
					++iHisAttackIndex;
				}
			}
		}

		/// <summary>
		/// recursively check one of my attacks against another characters current blocks
		/// </summary>
		/// <param name="rAttack">my attack to check</param>
		/// <param name="rOtherGuy">the other guys physics container</param>
		/// <returns>bool: true if the attack connected, false if there was no connection</returns>
		private bool CheckBlockedAttack(CreateAttackAction rAttack, IPhysicsContainer rOtherGuy)
		{
			Debug.Assert(true == m_bBonesSorted);
			Debug.Assert(null != rOtherGuy);
			Debug.Assert(rOtherGuy != this);
			Debug.Assert(null != rAttack);

			foreach (BlockingStateAction rOtherBlock in rOtherGuy.Owner.CurrentBlocks.CurrentActions)
			{
				Debug.Assert(null != rOtherBlock);

				//make sure this attack has a circle
				if ((null == rAttack.GetCircle()) || (null == rOtherBlock.GetCircle()))
				{
					//no collision occured because that attack bone isn't even being displayed
					return false;
				}

				//check my circle against his circles
				Vector2 first = Vector2.Zero;
				Vector2 second = Vector2.Zero;
				if (rAttack.GetCircle().IsColliding(rOtherBlock.GetCircle(), ref first, ref second))
				{
					//A collisoin occurred, add a collision to this container
					Owner.BlockResponse(rOtherGuy, rAttack, rOtherBlock, first, second);
					return true;
				}
			}

			//no collisions occured between the attack and the other guy
			return false;
		}


		private void IterateAttackCollisions(IPhysicsContainer rOtherGuy)
		{
			Debug.Assert(true == m_bBonesSorted);

			//check my attacks against his attacks to check for weapon clashes
			int iMyAttackIndex = 0;
			int iHisAttackIndex = 0;
			if ((Owner.CurrentAttacks.Count > 0) && (rOtherGuy.Owner.CurrentAttacks.Count > 0))
			{
				//check my list of attacks against the other dudes list of attacks
				while (iMyAttackIndex < Owner.CurrentAttacks.Count)
				{
					//get my attack
					CreateAttackAction myAttack = Owner.CurrentAttacks[iMyAttackIndex];

					//get my circle
					Circle myCircle = myAttack.GetCircle();
					if (null == myCircle)
					{
						//no collision occured because that attack bone isn't even being displayed
						++iMyAttackIndex;
						continue;
					}

					iHisAttackIndex = 0;
					while (iHisAttackIndex < rOtherGuy.Owner.CurrentAttacks.Count)
					{
						//get his attack
						CreateAttackAction hisAttack = rOtherGuy.Owner.CurrentAttacks[iHisAttackIndex];

						//get his circle
						Circle hisCircle = hisAttack.GetCircle();
						if (null == hisCircle)
						{
							//no collision occured because that attack bone isn't even being displayed
							++iHisAttackIndex;
							continue;
						}

						//do those attacks hit each other?

						//check my circle against his circle
						Vector2 first = Vector2.Zero;
						Vector2 second = Vector2.Zero;
						if (myCircle.IsColliding(hisCircle, ref first, ref second))
						{
							//A weapon clash collisoin occurred!

							//add a weapon hit collision to this container
							Owner.WeaponCollisionResponse(rOtherGuy, myAttack, first, second);

							//add a weapon hit collision to the other dude container
							rOtherGuy.Owner.WeaponCollisionResponse(this, hisAttack, second, first);

							Owner.RemoveAttack(iMyAttackIndex);
							rOtherGuy.Owner.RemoveAttack(iHisAttackIndex);

							//just one weapon hit per check
							return;
						}

						++iHisAttackIndex;
					}

					++iMyAttackIndex;
				}
			}

			//Check my attacks against the other character
			iMyAttackIndex = 0;
			while (iMyAttackIndex < Owner.CurrentAttacks.Count)
			{
				Debug.Assert(Owner.CurrentAttacks[iMyAttackIndex] is CreateAttackAction);
				if (CheckAttackCollisions(Owner.CurrentAttacks[iMyAttackIndex], rOtherGuy))
				{
					//the attack connected, remove from the list
					Owner.RemoveAttack(iMyAttackIndex);
				}
				else
				{
					++iMyAttackIndex;
				}
			}

			//check the other charcters attacks against my dude
			iHisAttackIndex = 0;
			while (iHisAttackIndex < rOtherGuy.Owner.CurrentAttacks.Count)
			{
				Debug.Assert(rOtherGuy.Owner.CurrentAttacks[iHisAttackIndex] is CreateAttackAction);
				if (rOtherGuy.CheckAttackCollisions(rOtherGuy.Owner.CurrentAttacks[iHisAttackIndex], this))
				{
					//the attack connected, remove from the list
					rOtherGuy.Owner.RemoveAttack(iHisAttackIndex);
				}
				else
				{
					++iHisAttackIndex;
				}
			}
		}

		/// <summary>
		/// recursively check one of my attacks against another characters skeletal structure
		/// </summary>
		/// <param name="rAttack">my attack to check</param>
		/// <param name="rOtherGuy">the other guys physics container</param>
		/// <returns>bool: true if the attack connected, false if there was no connection</returns>
		private bool CheckAttackCollisions(CreateAttackAction rAttack, IPhysicsContainer rOtherGuy)
		{
			Debug.Assert(true == m_bBonesSorted);
			Debug.Assert(null != rOtherGuy);
			Debug.Assert(rOtherGuy != this);
			Debug.Assert(null != rAttack);
			Debug.Assert(rAttack is IBaseAction);

			foreach (Bone rOtherBone in rOtherGuy.CollisionBones)
			{
				Debug.Assert(null != rOtherBone);

				//make sure they have images
				Image rOtherImage = rOtherBone.GetCurrentImage();
				if (null == rOtherImage)
				{
					return false;
				}

				//can't attack feet or weapons
				if (rOtherBone.IsFoot || rOtherBone.IsWeapon)
				{
					return false;
				}

				//make sure this attack has a circle
				Circle myCircle = rAttack.GetCircle();
				if (null == myCircle)
				{
					//no collision occured because that attack bone isn't even being displayed
					return false;
				}

				//check my circle against his circles
				Vector2 first = Vector2.Zero;
				Vector2 second = Vector2.Zero;
				for (int i = 0; i < rOtherImage.Circles.Count; i++)
				{
					if (myCircle.IsColliding(rOtherImage.Circles[i], ref first, ref second))
					{
						//A collisoin occurred, add a collision to this container
						Owner.CollisionResponse(rOtherGuy, rAttack, first, second);
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
		/// <param name="rVelocity">the current velocity of this dude</param>
		/// <param name="rWorldBoundaries">rectangle of teh world boundaries</param>
		public void CheckWorldCollisions(Vector2 rVelocity, Rectangle rWorldBoundaries)
		{
			Debug.Assert(true == m_bBonesSorted);
			
			float fVelocityLength = (rVelocity.Length() * m_Object.CharacterClock.TimeDelta);

			//get the velocity direction if it isn't zero
			Vector2 VelocityDirection = rVelocity;
			if (fVelocityLength > 0.0f)
			{
				VelocityDirection.Normalize();
			}

			//check children of other bone for collisions
			foreach (Bone rBone in CollisionBones)
			{
				//get the images we are checking
				Image rImage = rBone.GetCurrentImage();
				if (null == rImage)
				{
					continue;
				}

				//make sure that I dont have any lines, level objects always have to be passed to players
				Debug.Assert(rImage.Lines.Count == 0);

				//Do the actual collision check

				//loop through all polygons, checking for collisions
				for (int i = 0; i < rImage.Circles.Count; i++)
				{
					//get the bottom of the polygon
					float fBottom = rImage.Circles[i].WorldPosition.Y + rImage.Circles[i].Radius;

					//check for fast ground hits
					float fFastBottom = fBottom + (VelocityDirection.Y * fVelocityLength);
					if (fFastBottom > rWorldBoundaries.Bottom)
					{
						//a floor hit occured

						//get the delta between the current pos and the ground
						float fOverlap = (rWorldBoundaries.Bottom - fBottom);
						if (!HitFlags[(int)EHitType.GroundHit] || (Math.Abs(fOverlap) > Math.Abs(Hits[(int)EHitType.GroundHit].Strength)))
						{
							Debug.Assert(null != Hits[(int)EHitType.GroundHit]);

							HitFlags[(int)EHitType.GroundHit] = true;
							Hits[(int)EHitType.GroundHit].Set(
								new Vector2(0.0f, 1.0f),
								null,
								fOverlap,
								EHitType.GroundHit,
								null,
								new Vector2(rImage.Circles[i].WorldPosition.X, rWorldBoundaries.Bottom));
						}
					}
					else
					{
						//get the top of the polygon (will be fast bottom plus 2 * radius
						float fFastTop = fFastBottom - (2.0f * rImage.Circles[i].Radius);
						if (fFastTop < rWorldBoundaries.Top)
						{
							//a ceiling hit occured

							//get the top of the polygon
							float fTop = rImage.Circles[i].WorldPosition.Y - rImage.Circles[i].Radius;

							float fOverlap = (fTop - rWorldBoundaries.Top);
							if (!HitFlags[(int)EHitType.CeilingHit] || (Math.Abs(fOverlap) > Math.Abs(Hits[(int)EHitType.CeilingHit].Strength)))
							{
								Debug.Assert(null != Hits[(int)EHitType.CeilingHit]);

								HitFlags[(int)EHitType.CeilingHit] = true;
								Hits[(int)EHitType.CeilingHit].Set(
									new Vector2(0.0f, -1.0f),
									null,
									fOverlap,
									EHitType.CeilingHit,
									null,
									new Vector2(rImage.Circles[i].WorldPosition.X, rWorldBoundaries.Top));
							}
						}
					}

					//get the right edge of the polygon
					float fRight = rImage.Circles[i].WorldPosition.X + rImage.Circles[i].Radius;

					//check for fast right wall hits
					float fFastRight = fRight + (VelocityDirection.X * fVelocityLength);
					if (fFastRight > rWorldBoundaries.Right)
					{
						//a right wall hit occured

						//TODO: checking abs values is no good... will always get the bone FARTHEST AWAY!

						//get the delta between the current pos and the right wall
						float fOverlap = (rWorldBoundaries.Right - fFastRight);
						if (!HitFlags[(int)EHitType.RightWallHit] || (Math.Abs(fOverlap) > Math.Abs(Hits[(int)EHitType.RightWallHit].Strength)))
						{
							Debug.Assert(null != Hits[(int)EHitType.RightWallHit]);

							HitFlags[(int)EHitType.RightWallHit] = true;
							Hits[(int)EHitType.RightWallHit].Set(
								new Vector2(1.0f, 0.0f),
								null,
								fOverlap,
								EHitType.RightWallHit,
								null,
								new Vector2(rWorldBoundaries.Right, rImage.Circles[i].WorldPosition.Y));
						}
					}
					else
					{
						//get the left of the polygon (will be fast right plus 2 * radius
						float fFastLeft = fFastRight - (2.0f * rImage.Circles[i].Radius);
						if (fFastLeft < rWorldBoundaries.Left)
						{
							//a left wall hit occured

							//get the left edge of the polygon
							float fLeft = rImage.Circles[i].WorldPosition.X - rImage.Circles[i].Radius;

							float fOverlap = (fLeft - rWorldBoundaries.Left);
							if (!HitFlags[(int)EHitType.LeftWallHit] || (Math.Abs(fOverlap) > Math.Abs(Hits[(int)EHitType.LeftWallHit].Strength)))
							{
								Debug.Assert(null != Hits[(int)EHitType.LeftWallHit]);

								HitFlags[(int)EHitType.LeftWallHit] = true;
								Hits[(int)EHitType.LeftWallHit].Set(
									new Vector2(-1.0f, 0.0f),
									null,
									fOverlap,
									EHitType.LeftWallHit,
									null,
									new Vector2(rWorldBoundaries.Left, rImage.Circles[i].WorldPosition.Y));
							}
						}
					}

					//check for fast wall hits

					
				}
			}
		}

		#endregion //Collision Methods

		#endregion //Methods
	}
}