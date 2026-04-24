using AnimationLib;
using FilenameBuddy;
using GameTimer;
using Microsoft.Xna.Framework.Content;
using RenderBuddy;
using System.Collections.Generic;

namespace GameDonkeyLib
{
	public class GarmentManager : TimedActionList<AddGarmentAction>
	{
		#region Members
		private List<Garment> AllGarments { get; set; }
		private BaseObject Owner { get; set; }

		#endregion //Members

		#region Methods
		public GarmentManager(BaseObject owner)
		{
			Owner = owner;
			AllGarments = new List<Garment>();
		}
		public override void AddAction(AddGarmentAction action, GameClock clock)
		{
			//Store the garment
			base.AddAction(action, clock);
			AddGarment(action.Garment);
		}

		public void AddGarment(Garment garment)
		{
			//add to the model
			garment.AddToSkeleton();

			//redo the physics lists
			Owner.Physics.GarmentChange(garment);

			//the animation has changed!!!
			for (int i = 0; i < garment.Fragments.Count; i++)
			{
				garment.Fragments[i].AnimationContainer.RestartAnimation();
			}
		}

		public void RemoveGarment(GarmentModel garmentModel)
		{
			var garment = CheckForXmlGarment(garmentModel.Filename);
			if (null != garment)
			{
				RemoveGarment(garment);
			}
		}

		public void RemoveGarment(Garment garment)
		{
			garment.RemoveFromSkeleton();

			var hasPhysics = false; //flag to check if any physics need to be updated
			if (garment.HasPhysics)
			{
				hasPhysics = true;
			}

			//clear the physics if required
			if (hasPhysics)
			{
				Owner.Physics.ClearPhysicsLists();
				Owner.Physics.SortBones(Owner.AnimationContainer.Skeleton.RootBone);
			}
		}
		public override void Reset()
		{
			//remove all the garments from the model
			var hasPhysics = false; //flag to check if any physics need to be updated
			foreach (AddGarmentAction garmentAction in CurrentActions)
			{
				garmentAction.Garment.RemoveFromSkeleton();

				if (garmentAction.Garment.HasPhysics)
				{
					hasPhysics = true;
				}
			}

			//clear the physics if required
			if (hasPhysics)
			{
				Owner.Physics.ClearPhysicsLists();
				Owner.Physics.SortBones(Owner.AnimationContainer.Skeleton.RootBone);
			}

			//clear out the list
			base.Reset();
		}
		public override void Update(GameClock clock)
		{
			var hasPhysics = false; //flag to check if any physics need to be updated

			//remove any finished actions from the list
			var i = 0;
			while (i < CurrentActions.Count)
			{
				var garmentAction = CurrentActions[i];

				//checked if this action has expired...
				if (!garmentAction.ActiveForWholeState &&
					(garmentAction.DoneTime <= clock.CurrentTime))
				{
					//remove teh garment
					if (garmentAction.Garment.HasPhysics)
					{
						hasPhysics = true;
					}

					garmentAction.Garment.RemoveFromSkeleton();
					CurrentActions.RemoveAt(i);
				}
				else
				{
					i++;
				}
			}

			//clear the physics if required
			if (hasPhysics)
			{
				Owner.Physics.ClearPhysicsLists();
				Owner.Physics.SortBones(Owner.AnimationContainer.Skeleton.RootBone);
			}
		}

		#region Tools
		public void GetAllWeaponBones(List<string> listWeapons)
		{
			foreach (var garment in AllGarments)
			{
				garment.GetAllWeaponBones(listWeapons);
			}
		}

		#endregion //Tools

		#endregion //Methods

		#region File IO
		private Garment CheckForXmlGarment(Filename garmentFile)
		{
			foreach (var garment in AllGarments)
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
			var garment = CheckForXmlGarment(garmentFile);
			if (null != garment)
			{
				//found a garment already loaded
				return garment;
			}
			else
			{
				//create a new one
				garment = new Garment(garmentFile, Owner.AnimationContainer.Skeleton, renderer, content);
			}

			//store and return the garment
			AllGarments.Add(garment);
			return garment;
		}

		public Garment LoadGarment(GarmentModel garmentModel, IRenderer renderer)
		{
			//first check if the garment is already loaded
			var garment = CheckForXmlGarment(garmentModel.Filename);
			if (null != garment)
			{
				//found a garment already loaded
				return garment;
			}
			else
			{
				//create a new one
				garment = new Garment(garmentModel, Owner.AnimationContainer.Skeleton, renderer);
			}

			//store and return the garment
			AllGarments.Add(garment);
			return garment;
		}

		#endregion //File IO
	}
}