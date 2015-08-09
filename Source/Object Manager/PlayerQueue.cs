using System.Collections.Generic;
using HadoukInput;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using GameTimer;
using DrawListBuddy;
using CameraBuddy;
using RenderBuddy;
using FilenameBuddy;

namespace GameDonkey
{
	/// <summary>
	/// this class is for queueing up players, fireballs, etc.
	/// </summary>
	public class PlayerQueue
	{
		#region Members

		/// <summary>
		/// The ID of this queue, used to send AIs over the network
		/// </summary>
		private int m_iQueueID;

		//the player queue ID that can be used by the player queue to identify objects
		protected int m_iNextObjectID;

		//the list of active objects
		protected List<BaseObject> m_listActive;

		//the list of inactive objects
		protected List<BaseObject> m_listInactive;

		/// <summary>
		/// drawlists used for character trails
		/// </summary>
		protected List<DrawList> m_listTrailDrawLists;

		//this is the id of the player's characters
		protected BaseObject m_rCharacter;

		//the color of the characters in this player queue
		private Color m_PlayerColor;

		/// <summary>
		/// This clock synchronizes the clocks between all the dudes this guy contains
		/// </summary>
		protected HitPauseClock m_CharacterClock;

		/// <summary>
		/// the number of points this player has
		/// </summary>
		private int m_iPoints;

		/// <summary>
		/// the number of stock this player has.  This starts at 0 and gets incremented every time the player dies.
		/// </summary>
		private int m_iStock;

		/// <summary>
		/// This timer gets started for a few seconds every time the player scores.  Used to color the hud.
		/// </summary>
		private CountdownTimer m_ScoreTimer;

		/// <summary>
		/// The player's name, either their gamertag, "AI", or name of the level
		/// </summary>
		private string m_strPlayerName;

		#endregion //Members

		#region Properties

		public BaseObject Character
		{
			get { return m_rCharacter; }
		}

		public int Points
		{
			get { return m_iPoints; }
		}

		public int Stock
		{
			get { return m_iStock; }
		}

		public InputWrapper InputQueue { get; set; }

		public List<BaseObject> ActiveObjects
		{
			get { return m_listActive; }
		}

		public int QueueID
		{
			get { return m_iQueueID; }
		}

		public string PlayerName
		{
			get { return m_strPlayerName; }
			set { m_strPlayerName = value; }
		}

		public CountdownTimer ScoreTimer
		{
			get { return m_ScoreTimer; }
		}

		public Color PlayerColor
		{
			get { return m_PlayerColor; }
		}

		public HitPauseClock CharacterClock
		{
			get { return m_CharacterClock; }
		}

		public float Scale
		{
			set
			{
				//set active stuff
				foreach (BaseObject rObject in m_listActive)
				{
					rObject.Scale = value;
				}

				//set inactive stuff
				foreach (BaseObject rObject in m_listInactive)
				{
					rObject.Scale = value;
				}
			}
		}

		#endregion //Properties

		#region Methods 

		/// <summary>
		/// standard constructor
		/// </summary>
		public PlayerQueue(Color PlayerColor, int iQueueID)
		{
			m_listActive = new List<BaseObject>();
			m_listInactive = new List<BaseObject>();
			m_listTrailDrawLists = new List<DrawList>();
			m_rCharacter = null;
			m_CharacterClock = new HitPauseClock();
			m_PlayerColor = PlayerColor;
			m_iPoints = 0;
			m_iStock = 1;
			InputQueue = null;
			m_iNextObjectID = 0;
			m_iQueueID = iQueueID;
			Debug.Assert(m_iQueueID >= 0);

			m_ScoreTimer = new CountdownTimer();
		}

		public virtual PlayerObject CreateHumanPlayer()
		{
			return new PlayerObject(m_CharacterClock, m_iNextObjectID++);
		}

		public virtual PlayerObject CreateAiPlayer()
		{
			return new PlayerObject(m_CharacterClock, m_iNextObjectID++);
		}

		public virtual PlayerObjectData CreatePlayerObjectData()
		{
			return new PlayerObjectData();
		}

		/// <summary>
		/// pull an item out of the inactive list and add it to the active list.
		/// </summary>
		/// <param name="rObject">the object to activate</param>
		/// <returns>true if the object was spawned, false if the object is already active</returns>
		public bool ActivateObject(BaseObject rObject)
		{
			//find the object in the inactive list
			for (int i = 0; i < m_listInactive.Count; i++)
			{
				Debug.Assert(null != m_listInactive[i]);
				if (m_listInactive[i].GlobalID == rObject.GlobalID)
				{
					//remove from the incative list and add to active
					Debug.Assert(false == CheckListForObject(rObject, true));
					m_listActive.Add(m_listInactive[i]);
					m_listInactive.RemoveAt(i);

					//reset the thing back to it's start state
					rObject.Reset();

					return true;
				}
			}

			//that object must already be in the active list
			Debug.Assert(true == CheckListForObject(rObject, true));
			return false;
		}

		/// <summary>
		/// pull an item out of the inactive list and add it to the active list.
		/// </summary>
		/// <param name="rObject">the object to activate</param>
		/// <returns>reference to the object if the object was spawned, null if the object is already active</returns>
		public BaseObject ActivateObject(int iQueueID)
		{
			//find the object in the inactive list
			for (int i = 0; i < m_listInactive.Count; i++)
			{
				Debug.Assert(null != m_listInactive[i]);
				if (m_listInactive[i].QueueID == iQueueID)
				{
					BaseObject rObject = m_listInactive[i];

					//remove from the incative list and add to active
					m_listActive.Add(m_listInactive[i]);
					m_listInactive.RemoveAt(i);

					//reset the thing back to it's start state
					rObject.Reset();

					//run the animation container so all the bones will be in the correct position when it updates
					rObject.AnimationContainer.Update(m_CharacterClock, rObject.Position, rObject.Flip, rObject.Scale, 0.0f, true);

					return rObject;
				}
			}

			//that object must already be in the active list
			return null;
		}

		/// <summary>
		/// Find an object by its queue id so it can be updated
		/// </summary>
		/// <param name="iQueueID"></param>
		/// <returns></returns>
		protected BaseObject FindActiveObject(int iQueueID)
		{
			//check the active objects
			for (int i = 0; i < m_listActive.Count; i++)
			{
				if (m_listActive[i].QueueID == iQueueID)
				{
					//found it, that was easy enough
					return m_listActive[i];
				}
			}

			//it must be inactive, look through that list
			return ActivateObject(iQueueID);
		}

		/// <summary>
		/// Take an object out of the active list, put it in the inactive list
		/// </summary>
		/// <param name="rObject">the object to deactivate</param>
		public void DeactivateObject(BaseObject rObject)
		{
			//go through the active list, look for that object
			for (int i = 0; i < m_listActive.Count; i++)
			{
				if (m_listActive[i].GlobalID == rObject.GlobalID)
				{
					//pop into the inactive list, remove from the active list
					Debug.Assert(false == CheckListForObject(rObject, false));
					m_listInactive.Add(m_listActive[i]);
					m_listActive.RemoveAt(i);
					return;
				}
			}

			//if it gets here, something fucked up!!!
			Debug.Assert(false);
		}

		/// <summary>
		/// Take an object out of the active list, put it in the inactive list
		/// </summary>
		/// <param name="iQueueID">the queue id of the object to deactivate</param>
		protected void DeactivateObject(int iQueueID)
		{
			//go through the active list, look for that object
			for (int i = 0; i < m_listActive.Count; i++)
			{
				if (m_listActive[i].QueueID == iQueueID)
				{
					//pop into the inactive list, remove from the active list
					m_listInactive.Add(m_listActive[i]);
					m_listActive.RemoveAt(i);
					return;
				}
			}
		}

		public void DeactivateAllObjects()
		{
			Debug.Assert(null != m_listInactive);
			Debug.Assert(null != m_listActive);
			while (m_listActive.Count > 0)
			{
				//put the first obect in the inactive list and remove from active
				BaseObject rDude = m_listActive[0];
				Debug.Assert(null != rDude);
				m_listInactive.Add(rDude);
				m_listActive.RemoveAt(0);

				//reset the thing back to it's start state
				Debug.Assert(null != rDude.States);
				rDude.States.Reset();
			}

			//clear out the trails too
			m_listTrailDrawLists.Clear();
		}

		/// <summary>
		/// reset and reposition the all the objects
		/// </summary>
		/// <param name="rSpawnPoint">the location to place the main object</param>
		public virtual void Reset(Vector2 rSpawnPoint)
		{
			Debug.Assert(null != m_rCharacter);
			m_rCharacter.Flip = (rSpawnPoint.X >= 0.0f);
			m_rCharacter.Position = rSpawnPoint;
			m_rCharacter.Velocity = Vector2.Zero;

			//make sure all the objects are inactivated
			DeactivateAllObjects();

			//okay, we only want the first character active
			Debug.Assert(null != m_rCharacter);
			ActivateObject(m_rCharacter);
		}

		/// <summary>
		/// Reset the object, but keep the same position
		/// </summary>
		public virtual void Reset()
		{
			//make sure all the objects are inactivated
			DeactivateAllObjects();

			//okay, we only want the first character active
			Debug.Assert(null != m_rCharacter);
			ActivateObject(m_rCharacter);
		}

		/// <summary>
		/// Check whether an object is in one of the lists or not
		/// </summary>
		/// <param name="rObject">the object to look for</param>
		/// <param name="bActiveList">wether to check the active or inactive list</param>
		/// <returns>bool: whether or not the requested object was found in the specified list</returns>
		public bool CheckListForObject(BaseObject rObject, bool bActiveList)
		{
			if (bActiveList)
			{
				//check the active list
				for (int i = 0; i < m_listActive.Count; i++)
				{
					Debug.Assert(null != m_listActive[i]);
					if (m_listActive[i].GlobalID == rObject.GlobalID)
					{
						return true;
					}
				}
			}
			else
			{
				//check the inactive list
				for (int i = 0; i < m_listInactive.Count; i++)
				{
					Debug.Assert(null != m_listInactive[i]);
					if (m_listInactive[i].GlobalID == rObject.GlobalID)
					{
						return true;
					}
				}
			}

			//the specified object was not found in the list
			return false;
		}

		public void Update(GameClock rClock, bool bUpdateGravity)
		{
			//update the clock
			m_CharacterClock.Update(rClock);
			m_ScoreTimer.Update(rClock);

			//update all the active objects in this dude
			for (int i = 0; i < m_listActive.Count; i++)
			{
				m_listActive[i].Update(bUpdateGravity);
			}
		}

		/// <summary>
		/// Check if an object is dead (out of bounds)
		/// </summary>
		/// <returns>whether or not the thing is dead</returns>
		public bool CheckIfDead()
		{
			Debug.Assert(null != Character);
			Debug.Assert((Character.Type == EObjectType.Human) || (Character.Type == EObjectType.AI));
			return (Character.DisplayHealth() <= 0);
		}

		public void UpdateInput(InputState rInput)
		{
			Character.UpdateInput(InputQueue, rInput);
		}

		public void GetPlayerInput(List<PlayerQueue> listBadGuys)
		{
			Debug.Assert(null != InputQueue);
			for (int i = 0; i < m_listActive.Count; i++)
			{
				m_listActive[i].GetPlayerInput(InputQueue, listBadGuys);
			}
		}

		public void GetPlayerAttackInput(List<PlayerQueue> listBadGuys)
		{
			Debug.Assert(null != InputQueue);
			for (int i = 0; i < m_listActive.Count; i++)
			{
				m_listActive[i].GetPlayerAttackInput(InputQueue, listBadGuys);
			}
		}

		public void CheckHardCodedStates()
		{
			for (int i = 0; i < m_listActive.Count; i++)
			{
				m_listActive[i].CheckHardCodedStates();
			}
		}

		public void UpdateRagdoll(bool bIgnoreRagdoll)
		{
			for (int i = 0; i < m_listActive.Count; i++)
			{
				m_listActive[i].UpdateRagdoll(bIgnoreRagdoll);
			}
		}

		public void CheckCollisions(PlayerQueue rOtherGuy)
		{
			//check for collisions
			for (int i = 0; i < m_listActive.Count; i++)
			{
				for (int j = 0; j < rOtherGuy.m_listActive.Count; j++)
				{
#if DEBUG
					uint iMyID = m_listActive[i].GlobalID;
					uint iHisID = rOtherGuy.m_listActive[j].GlobalID;
					Debug.Assert(iMyID != iHisID);
#endif
					m_listActive[i].CheckCollisions(rOtherGuy.m_listActive[j]);
				}
			}
		}

		public void CheckWorldCollisions(Rectangle rWorldBroundaries)
		{
			//check for collisions
			for (int i = 0; i < m_listActive.Count; i++)
			{
				m_listActive[i].CheckWorldCollisions(rWorldBroundaries);
			}
		}

		public void RespondToHits(IGameDonkey rEngine)
		{
			//respond to any hits that may have occured resulting from collisions.
			for (int i = 0; i < m_listActive.Count; i++)
			{
				m_listActive[i].HitResponse(rEngine);
			}
		}

		/// <summary>
		/// This function is called after the characters are totally done updating
		/// </summary>
		/// <param name="rClock">the current time</param>
		public void UpdateDrawlists()
		{
			//update all the trail drawlists
			int iDrawlistIndex = 0;
			while (iDrawlistIndex < m_listTrailDrawLists.Count)
			{
				if (m_listTrailDrawLists[iDrawlistIndex].Update(m_CharacterClock))
				{
					//this drawlist is expired
					m_listTrailDrawLists.RemoveAt(iDrawlistIndex);
				}
				else
				{
					iDrawlistIndex++;
				}
			}

			//add drawlists for all the active items
			for (int i = 0; i < m_listActive.Count; i++)
			{
				//add a character trail, if we need it
				if (m_listActive[i].DoesNeedCharacterTrail())
				{
					Debug.Assert(null != m_listActive[i].TrailAction);

					//add a trail right in front of the main dude
					DrawList myTrail = new DrawList();
					Debug.Assert(null != myTrail);
					myTrail.Set(m_listActive[i].TrailAction.TrailLifeDelta,
						m_listActive[i].TrailAction.StartColor,
						m_listActive[i].Scale);

					m_listActive[i].AnimationContainer.Render(myTrail);
					m_listTrailDrawLists.Add(myTrail);
				}

				//add the main drawlist for the character
				m_listActive[i].UpdateDrawlist();
			}
		}

		public void AddToCamera(Camera rCamera)
		{
			for (int i = 0; i < m_listActive.Count; i++)
			{
				m_listActive[i].AddToCamera(rCamera);
			}
		}

		public void DrawCameraInfo(IRenderer rRenderer)
		{
			for (int i = 0; i < m_listActive.Count; i++)
			{
				m_listActive[i].DrawCameraInfo(rRenderer);
			}
		}

		/// <summary>
		/// render one of the lists of drawlists
		/// </summary>
		/// <param name="rRenderer">renderer to render to</param>
		/// <param name="bMain">whether to render the main list or the list of trails</param>
		public void Render(IRenderer rRenderer, bool bMain)
		{
			if (bMain)
			{
				//render all the main drawlists
				for (int i = 0; i < m_listActive.Count; i++)
				{
					m_listActive[i].Render(rRenderer);
				}
			}
			else
			{
				//render all the trail drawlists
				for (int i = 0; i < m_listTrailDrawLists.Count; i++)
				{
					m_listTrailDrawLists[i].Render(rRenderer);
				}
			}
		}

		private int GetNextMessageOffset()
		{
			//go through all the current objects and add up the number of messages
			int iNumMessages = 0;
			for (int i = 0; i < m_listActive.Count; i++)
			{
				iNumMessages += m_listActive[i].States.NumMessages();
			}

			for (int i = 0; i < m_listInactive.Count; i++)
			{
				iNumMessages += m_listInactive[i].States.NumMessages();
			}

			return iNumMessages;
		}

		/// <summary>
		/// add one stock to this player.
		/// </summary>
		public void SubtractStock()
		{
			m_iStock--;
			m_ScoreTimer.Start(3.0f);
		}

		public void RenderPhysics(IRenderer rRenderer)
		{
			for (int i = 0; i < m_listActive.Count; i++)
			{
				m_listActive[i].RenderPhysics(rRenderer);
			}
		}

		public void RenderAttacks(IRenderer rRenderer)
		{
			for (int i = 0; i < m_listActive.Count; i++)
			{
				m_listActive[i].RenderAttacks(rRenderer);
			}
		}

		#endregion //Methods

		#region File IO

		public BaseObject LoadXmlObject(Filename strFileName, IGameDonkey rEngine, EObjectType eType, int iDifficulty)
		{
			//try to load all that stuff
			BaseObject myCharacter;
			BaseObjectData myData;
			switch (eType)
			{
				case EObjectType.Human:
					{
						myCharacter = CreateHumanPlayer();
						myData = CreatePlayerObjectData();

						//set as the main character
						AddCharacterToList(myCharacter);
					}
					break;
				case EObjectType.AI:
					{
						//AIObject myDude = new AIObject(m_CharacterClock, m_iNextObjectID++);
						//myDude.Difficulty = iDifficulty;
						//myCharacter = myDude;

						myCharacter = CreateAiPlayer();
						myData = CreatePlayerObjectData();

						//set as the main character
						AddCharacterToList(myCharacter);
					}
					break;
				case EObjectType.Projectile:
					{
						Debug.Assert(null != m_rCharacter);
						myCharacter = new ProjectileObject(m_CharacterClock, m_rCharacter, m_iNextObjectID++);
						myData = new ProjectileObjectData();
					}
					break;
				case EObjectType.Level:
					{
						myCharacter = new LevelObject(m_CharacterClock, m_iNextObjectID++);
						myData = new LevelObjectData();
					}
					break;
				default:
					{
						Debug.Assert(false);
						return null;
					}
			}

			//get the message offset
			int iMessageOffset = GetNextMessageOffset();

			//load the object data
			if (!myData.LoadObject(strFileName))
			{
				Debug.Assert(false);
				return null;
			}

			//load the object data into the thing
			myCharacter.PlayerQueue = this;
			if (!myCharacter.ParseXmlData(myData, rEngine, iMessageOffset))
			{
				Debug.Assert(false);
				return null;
			}

			//add to the correct list
			if (eType == EObjectType.Level)
			{
				m_listActive.Add(myCharacter);
			}
			else
			{
				m_listInactive.Add(myCharacter);
			}

			return myCharacter;
		}

		protected virtual void AddCharacterToList(BaseObject rObject)
		{
			Debug.Assert(null == m_rCharacter);
			m_rCharacter = rObject;

			//set the color too
			rObject.PlayerColor = m_PlayerColor;
		}

		#endregion
	}
}