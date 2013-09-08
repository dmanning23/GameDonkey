using System;
using GameTimer;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Framework.Content;
using AnimationLib;
using FilenameBuddy;
using RenderBuddy;

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
		private List<Garment> m_listAllGarments;

		/// <summary>
		/// The base object that owns this dude
		/// </summary>
		private BaseObject m_rOwner;

		#endregion //Members

		#region Methods

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="rOwner">the object that owns this guy.  Cannot be null</param>
		public GarmentManager(BaseObject rOwner)
		{
			Debug.Assert(null != rOwner);
			m_rOwner = rOwner;
			m_listAllGarments = new List<Garment>();
		}

		/// <summary>
		/// Add a garment to be managed.  Adds it to the object and sets up removal time
		/// </summary>
		/// <param name="rAction">The garment to be added.  Cannot be null</param>
		public override void AddAction(AddGarmentAction rAction, GameClock rClock)
		{
			Debug.Assert(null != rAction);

			//Store the garment
			base.AddAction(rAction, rClock);

			//add to the model
			rAction.Garment.AddToModel();

			//redo the physics lists
			m_rOwner.Physics.GarmentChange(rAction.Garment);
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
				curAction.Garment.RemoveFromModel();

				if (curAction.Garment.HasPhysics)
				{
					bHasPhysics = true;
				}
			}

			//clear the physics if required
			if (bHasPhysics)
			{
				m_rOwner.Physics.ClearPhysicsLists();
				m_rOwner.Physics.SortBones(m_rOwner.AnimationContainer.Model);
			}

			//clear out the list
			base.Reset();
		}

		/// <summary>
		/// Update the garment manager.  Checks if any garments are ready to be removed
		/// </summary>
		/// <param name="rClock"></param>
		public override void Update(GameClock rClock)
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
					(myGarmentAction.DoneTime <= rClock.CurrentTime))
				{
					//remove teh garment
					if (myGarmentAction.Garment.HasPhysics)
					{
						bHasPhysics = true;
					}

					myGarmentAction.Garment.RemoveFromModel();
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
				m_rOwner.Physics.ClearPhysicsLists();
				m_rOwner.Physics.SortBones(m_rOwner.AnimationContainer.Model);
			}
		}

		#region Tools

		/// <summary>
		/// Get a list of all the weapon
		/// </summary>
		/// <param name="listWeapons"></param>
		public void GetAllWeaponBones(List<string> listWeapons)
		{
			foreach (Garment curGarment in m_listAllGarments)
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
		/// <param name="strGarmentFile">filename of the garment to check for</param>
		/// <returns>the matching object if found, otherwise null</returns>
		private Garment CheckForXMLGarment(Filename strGarmentFile)
		{
			foreach (Garment curGarment in m_listAllGarments)
			{
				if (curGarment.Filename.File == strGarmentFile.File)
				{
					//found a garment already loaded
					return curGarment;
				}
			}

			return null;
		}

		public Garment LoadGarment(Filename strGarmentFile, Renderer rRenderer)
		{
			//first check if the garment is already loaded
			Garment myGarment = CheckForXMLGarment(strGarmentFile);
			if (null != myGarment)
			{
				//found a garment already loaded
				return myGarment;
			}
			else
			{
				//create a new one
				myGarment = new Garment();
			}
			Debug.Assert(null != myGarment);

			//load the garment
			if (!myGarment.ReadXMLFormat(strGarmentFile, rRenderer, m_rOwner.AnimationContainer.Model))
			{
				//something bad happened
				return null;
			}
			Debug.Assert(strGarmentFile.File == myGarment.Filename.File);

			//store and return the garment
			m_listAllGarments.Add(myGarment);
			return myGarment;
		}

		/// <summary>
		/// check if a garment is already loaded
		/// </summary>
		/// <param name="strGarmentFile">filename of the garment to check for</param>
		/// <returns>the matching object if found, otherwise null</returns>
		private Garment CheckForXNAGarment(string strGarmentFile)
		{
			foreach (Garment curGarment in m_listAllGarments)
			{
				if (curGarment.Filename.GetRelFilename() == strGarmentFile)
				{
					//found a garment already loaded
					return curGarment;
				}
			}

			return null;
		}

		public Garment LoadGarment(ContentManager myContent, string strGarmentFile, Renderer rRenderer)
		{
			//setup the filename
			Filename myFileName = new Filename();
			myFileName.SetRelFilename(strGarmentFile);

			//first check if the garment is already loaded
			Garment myGarment = CheckForXNAGarment(myFileName.GetRelPathFileNoExt());
			if (null != myGarment)
			{
				//found a garment already loaded
				return myGarment;
			}
			else
			{
				//create a new one
				myGarment = new Garment();
			}
			Debug.Assert(null != myGarment);

			//load the garment
			if (!myGarment.ReadXNAContent(myContent,  myFileName.GetRelPathFileNoExt(), rRenderer, m_rOwner.AnimationContainer.Model))
			{
				//something bad happened
				return null;
			}

			//store and return the garment
			m_listAllGarments.Add(myGarment);
			return myGarment;
		}

		#endregion //File IO
	}
}