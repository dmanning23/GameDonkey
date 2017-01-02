using AnimationLib;
using FilenameBuddy;
using GameTimer;
using Microsoft.Xna.Framework.Content;
using RenderBuddy;
using System.Collections.Generic;
using System.Diagnostics;

namespace GameDonkey
{
	/// <summary>
	/// This is a class to make it easier to add/remove garments from a baseobject
	/// </summary>
	public class GarmentManager : TimedActionList<AddGarmentAction>
	{
		#region Members

		/// <summary>
		/// A list of all the garments loaded from all the add garment actions
		/// Used so that actions can share garments, instead of loading an instance of the same garment for each action
		/// </summary>
		private List<Garment> ListAllGarments { get; set; }

		/// <summary>
		/// The base object that owns this dude
		/// </summary>
		private BaseObject Owner { get; set; }

		#endregion //Members

		#region Methods

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="owner">the object that owns this guy.  Cannot be null</param>
		public GarmentManager(BaseObject owner)
		{
			Debug.Assert(null != owner);
			Owner = owner;
			ListAllGarments = new List<Garment>();
		}

		/// <summary>
		/// Add a garment to be managed.  Adds it to the object and sets up removal time
		/// </summary>
		/// <param name="action">The garment to be added.  Cannot be null</param>
		/// <param name="clock"></param>
		public override void AddAction(AddGarmentAction action, GameClock clock)
		{
			Debug.Assert(null != action);

			//Store the garment
			base.AddAction(action, clock);

			//add to the model
			action.Garment.AddToSkeleton();

			//redo the physics lists
			Owner.Physics.GarmentChange(action.Garment);

			//the animation has changed!!!
			for (int i = 0; i < action.Garment.Fragments.Count; i++)
			{
				action.Garment.Fragments[i].AnimationContainer.RestartAnimation();
			}
		}

		/// <summary>
		/// Reset the garment manager.  Removes all the current garments.
		/// </summary>
		public override void Reset()
		{
			//remove all the garments from the model
			bool bHasPhysics = false; //flag to check if any physics need to be updated
			foreach (AddGarmentAction curAction in CurrentActions)
			{
				curAction.Garment.RemoveFromSkeleton();

				if (curAction.Garment.HasPhysics)
				{
					bHasPhysics = true;
				}
			}

			//clear the physics if required
			if (bHasPhysics)
			{
				Owner.Physics.ClearPhysicsLists();
				Owner.Physics.SortBones(Owner.AnimationContainer.Skeleton.RootBone);
			}

			//clear out the list
			base.Reset();
		}

		/// <summary>
		/// Update the garment manager.  Checks if any garments are ready to be removed
		/// </summary>
		/// <param name="clock"></param>
		public override void Update(GameClock clock)
		{
			bool bHasPhysics = false; //flag to check if any physics need to be updated

			//remove any finished actions from the list
			int iCurrent = 0;
			while (iCurrent < CurrentActions.Count)
			{
				AddGarmentAction myGarmentAction = CurrentActions[iCurrent];
				Debug.Assert(null != myGarmentAction);

				//checked if this action has expired...
				if (!myGarmentAction.ActiveForWholeState &&
					(myGarmentAction.DoneTime <= clock.CurrentTime))
				{
					//remove teh garment
					if (myGarmentAction.Garment.HasPhysics)
					{
						bHasPhysics = true;
					}

					myGarmentAction.Garment.RemoveFromSkeleton();
					CurrentActions.RemoveAt(iCurrent);
				}
				else
				{
					iCurrent++;
				}
			}

			//clear the physics if required
			if (bHasPhysics)
			{
				Owner.Physics.ClearPhysicsLists();
				Owner.Physics.SortBones(Owner.AnimationContainer.Skeleton.RootBone);
			}
		}

		#region Tools

		/// <summary>
		/// Get a list of all the weapon
		/// </summary>
		/// <param name="listWeapons"></param>
		public void GetAllWeaponBones(List<string> listWeapons)
		{
			foreach (Garment curGarment in ListAllGarments)
			{
				curGarment.GetAllWeaponBones(listWeapons);
			}
		}

		#endregion //Tools

		#endregion //Methods

		#region File IO

		/// <summary>
		/// check if a garment is already loaded
		/// </summary>
		/// <param name="garmentFile">filename of the garment to check for</param>
		/// <returns>the matching object if found, otherwise null</returns>
		private Garment CheckForXmlGarment(Filename garmentFile)
		{
			foreach (Garment garment in ListAllGarments)
			{
				if (garment.GarmentFile.File == garmentFile.File)
				{
					//found a garment already loaded
					return garment;
				}
			}

			return null;
		}

		public Garment LoadGarment(Filename garmentFile, IRenderer renderer, ContentManager content)
		{
			//first check if the garment is already loaded
			Garment myGarment = CheckForXmlGarment(garmentFile);
			if (null != myGarment)
			{
				//found a garment already loaded
				return myGarment;
			}
			else
			{
				//create a new one
				myGarment = new Garment(content, garmentFile, Owner.AnimationContainer.Skeleton, renderer);
			}
			Debug.Assert(null != myGarment);

			//store and return the garment
			ListAllGarments.Add(myGarment);
			return myGarment;
		}

		#endregion //File IO
	}
}