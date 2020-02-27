using CameraBuddy;
using DrawListBuddy;
using FilenameBuddy;
using GameTimer;
using HadoukInput;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using RenderBuddy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameDonkeyLib
{
	/// <summary>
	/// this class is for queueing up players, fireballs, etc.
	/// </summary>
	public abstract class PlayerQueue
	{
		#region Properties

		/// <summary>
		/// this is a counter for assigning round-robin item ids, this is the next id to use
		/// </summary>
		private static int _nextQueueId;

		/// <summary>
		/// The ID of this queue, used to send AIs over the network
		/// </summary>
		public int QueueId { get; private set; }

		//the list of active objects
		public List<BaseObject> Active { get; private set; }

		//the list of inactive objects
		public List<BaseObject> Inactive { get; private set; }

		/// <summary>
		/// drawlists used for character trails
		/// </summary>
		protected List<DrawList> TrailDrawLists { get; set; }

		public bool HasTrails => TrailDrawLists.Count > 0;

		/// <summary>
		/// this is the player's character
		/// </summary>
		public PlayerObject Character { get; protected set; }

		/// <summary>
		/// This clock synchronizes the clocks between all the dudes this guy contains
		/// </summary>
		public HitPauseClock CharacterClock { get; protected set; }

		/// <summary>
		/// the number of points this player has
		/// </summary>
		public int Points { get; private set; }

		/// <summary>
		/// the number of stock this player has.  This starts at 0 and gets incremented every time the player dies.
		/// </summary>
		public int Stock { get; private set; }

		/// <summary>
		/// This timer gets started for a few seconds every time the player scores.  Used to color the hud.
		/// </summary>
		private CountdownTimer ScoreTimer { get; set; }

		/// <summary>
		/// The player's name, either their gamertag, "AI", or name of the level
		/// </summary>
		public string PlayerName { get; set; }

		public InputWrapper InputQueue { get; set; }

		private Color _playerColor;
		/// <summary>
		/// the color of the characters in this player queue
		/// </summary>
		public Color PlayerColor
		{
			get
			{
				return _playerColor;
			}
			set
			{
				_playerColor = value;
				foreach (var active in Active)
				{
					active.PlayerColor = PlayerColor;
				}
				foreach (var inactive in Inactive)
				{
					inactive.PlayerColor = PlayerColor;
				}
			}
		}

		public float Scale
		{
			set
			{
				//set active stuff
				foreach (var gameObject in Active)
				{
					gameObject.Scale = value;
				}

				//set inactive stuff
				foreach (var gameObject in Inactive)
				{
					gameObject.Scale = value;
				}
			}
		}

		public bool HasActive => Active.Count > 0;

		#endregion //Properties

		#region Methods 

		/// <summary>
		/// standard constructor
		/// </summary>
		public PlayerQueue(Color playerColor)
		{
			Active = new List<BaseObject>();
			Inactive = new List<BaseObject>();
			TrailDrawLists = new List<DrawList>();
			Character = null;
			CharacterClock = new HitPauseClock();
			PlayerColor = playerColor;
			Points = 0;
			Stock = 1;
			InputQueue = null;
			QueueId = _nextQueueId++;

			ScoreTimer = new CountdownTimer();
		}

		public abstract PlayerObject CreateHumanPlayer();

		public abstract PlayerObject CreateAiPlayer();

		public virtual PlayerObjectModel CreatePlayerObjectModel(Filename filename)
		{
			return new PlayerObjectModel(filename);
		}

		/// <summary>
		/// pull an item out of the inactive list and add it to the active list.
		/// </summary>
		/// <param name="rObject">the object to activate</param>
		/// <returns>true if the object was spawned, false if the object is already active</returns>
		public bool ActivateObject(BaseObject gameObject)
		{
			//find the object in the inactive list
			for (var i = 0; i < Inactive.Count; i++)
			{
				if (Inactive[i].Id == gameObject.Id)
				{
					//remove from the incative list and add to active
					Active.Add(Inactive[i]);
					Inactive.RemoveAt(i);

					//reset the thing back to it's start state
					gameObject.Reset();

					return true;
				}
			}

			//that object must already be in the active list
			return false;
		}

		/// <summary>
		/// Take an object out of the active list, put it in the inactive list
		/// </summary>
		/// <param name="gameObject">the object to deactivate</param>
		public void DeactivateObject(BaseObject gameObject)
		{
			//go through the active list, look for that object
			for (var i = 0; i < Active.Count; i++)
			{
				if (Active[i].Id == gameObject.Id)
				{
					//pop into the inactive list, remove from the active list
					Inactive.Add(Active[i]);
					Active.RemoveAt(i);
					return;
				}
			}
		}

		public void DeactivateObjects(string objectType)
		{
			//go through the active list, look for that object
			var i = 0;
			while (i < Active.Count)
			{
				if (Active[i].ObjectType == objectType)
				{
					//pop into the inactive list, remove from the active list
					Inactive.Add(Active[i]);
					Active.RemoveAt(i);
				}
				else
				{
					i++;
				}
			}
		}

		public void DeactivateAllObjects()
		{
			while (Active.Count > 0)
			{
				//put the first obect in the inactive list and remove from active
				var gameObject = Active[0];
				Inactive.Add(gameObject);
				Active.RemoveAt(0);

				//reset the thing back to it's start state
				gameObject.States.Reset();
			}

			//clear out the trails too
			TrailDrawLists.Clear();
		}

		/// <summary>
		/// reset and reposition the all the objects
		/// </summary>
		/// <param name="spawnPoint">the location to place the main object</param>
		public virtual void Reset(Vector2 spawnPoint)
		{
			Character.Flip = (spawnPoint.X >= 0f);
			Character.Position = spawnPoint;
			Character.Velocity = Vector2.Zero;

			//make sure all the objects are inactivated
			DeactivateAllObjects();

			//okay, we only want the first character active
			ActivateObject(Character);
		}

		/// <summary>
		/// Reset the object, but keep the same position
		/// </summary>
		public virtual void Reset()
		{
			//make sure all the objects are inactivated
			DeactivateAllObjects();

			//okay, we only want the first character active
			ActivateObject(Character);
		}

		/// <summary>
		/// Check whether an object is in one of the lists or not
		/// </summary>
		/// <param name="gameObject">the object to look for</param>
		/// <param name="activeList">wether to check the active or inactive list</param>
		/// <returns>bool: whether or not the requested object was found in the specified list</returns>
		public bool CheckListForObject(BaseObject gameObject, bool activeList)
		{
			if (activeList)
			{
				//check the active list
				for (var i = 0; i < Active.Count; i++)
				{
					if (Active[i].Id == gameObject.Id)
					{
						return true;
					}
				}
			}
			else
			{
				//check the inactive list
				for (var i = 0; i < Inactive.Count; i++)
				{
					if (Inactive[i].Id == gameObject.Id)
					{
						return true;
					}
				}
			}

			//the specified object was not found in the list
			return false;
		}

		public virtual void Update(GameClock clock)
		{
			//update the clock
			CharacterClock.Update(clock);
			ScoreTimer.Update(clock);

			//update all the active objects in this dude
			for (var i = 0; i < Active.Count; i++)
			{
				Active[i].Update();
			}
		}

		/// <summary>
		/// Check if an object is dead (out of bounds)
		/// </summary>
		/// <returns>whether or not the thing is dead</returns>
		public bool CheckIfDead()
		{
			return Character.CheckIfDead();
		}

		public void UpdateInput(IInputState input)
		{
			Character.UpdateInput(InputQueue, input);
		}

		public void GetPlayerInput(List<PlayerQueue> badGuys, bool ignoreAttackInput)
		{
			for (var i = 0; i < Active.Count; i++)
			{
				Active[i].GetPlayerInput(InputQueue, badGuys, ignoreAttackInput);
			}
		}

		public void CheckHardCodedStates()
		{
			for (var i = 0; i < Active.Count; i++)
			{
				Active[i].CheckHardCodedStates();
			}
		}

		public void UpdateRagdoll()
		{
			for (var i = 0; i < Active.Count; i++)
			{
				Active[i].UpdateRagdoll();
			}
		}

		public void CheckCollisions(PlayerQueue otherGuy)
		{
			//check for collisions
			for (var i = 0; i < Active.Count; i++)
			{
				for (var j = 0; j < otherGuy.Active.Count; j++)
				{
					Active[i].CheckCollisions(otherGuy.Active[j]);
				}
			}
		}

		public void CheckWorldCollisions(Rectangle worldBroundaries)
		{
			//check for collisions
			for (var i = 0; i < Active.Count; i++)
			{
				Active[i].CheckWorldCollisions(worldBroundaries);
			}
		}

		public void RespondToHits(IGameDonkey engine)
		{
			//respond to any hits that may have occured resulting from collisions.
			for (var i = 0; i < Active.Count; i++)
			{
				Active[i].HitResponse(engine);
			}
		}

		/// <summary>
		/// This function is called after the characters are totally done updating
		/// </summary>
		/// <param name="rClock">the current time</param>
		public void UpdateDrawlists()
		{
			//update all the trail drawlists
			var drawlistIndex = 0;
			while (drawlistIndex < TrailDrawLists.Count)
			{
				if (TrailDrawLists[drawlistIndex].Update(CharacterClock))
				{
					//this drawlist is expired
					TrailDrawLists.RemoveAt(drawlistIndex);
				}
				else
				{
					drawlistIndex++;
				}
			}

			//add drawlists for all the active items
			for (var i = 0; i < Active.Count; i++)
			{
				//add a character trail, if we need it
				if (Active[i].DoesNeedCharacterTrail())
				{
					//add a trail right in front of the main dude
					var trailDrawList = new DrawList();
					trailDrawList.Set(Active[i].TrailAction.TrailLifeDelta,
						Active[i].TrailAction.StartColor,
						Active[i].Scale);

					Active[i].AnimationContainer.Render(trailDrawList);
					TrailDrawLists.Add(trailDrawList);
				}

				//add the main drawlist for the character
				Active[i].UpdateDrawlist();
			}
		}

		public void AddToCamera(ICamera camera)
		{
			for (var i = 0; i < Active.Count; i++)
			{
				Active[i].AddToCamera(camera);
			}
		}

		public void DrawCameraInfo(IRenderer renderer)
		{
			for (var i = 0; i < Active.Count; i++)
			{
				Active[i].DrawCameraInfo(renderer);
			}
		}

		/// <summary>
		/// render one of the lists of drawlists
		/// </summary>
		/// <param name="renderer">renderer to render to</param>
		/// <param name="renderMain">whether to render the main list or the list of trails</param>
		public void Render(IRenderer renderer, bool renderMain)
		{
			if (renderMain)
			{
				//render all the main drawlists
				for (var i = 0; i < Active.Count; i++)
				{
					Active[i].Render(renderer);
				}
			}
			else
			{
				//render all the trail drawlists
				for (var i = 0; i < TrailDrawLists.Count; i++)
				{
					TrailDrawLists[i].Render(renderer);
				}
			}
		}

		/// <summary>
		/// add one stock to this player.
		/// </summary>
		public void SubtractStock()
		{
			Stock--;
			ScoreTimer.Start(3.0f);
		}

		public virtual void RenderCharacterShadows(IGameDonkey engine)
		{
			for (var i = 0; i < Active.Count; i++)
			{
				Active[i].RenderCharacterShadow(engine);
			}
		}

		public void RenderPhysics(IRenderer renderer)
		{
			for (var i = 0; i < Active.Count; i++)
			{
				Active[i].RenderPhysics(renderer);
			}
		}

		public void RenderAttacks(IRenderer renderer)
		{
			for (var i = 0; i < Active.Count; i++)
			{
				Active[i].RenderAttacks(renderer);
			}
		}

		#endregion //Methods

		#region File IO

		/// <summary>
		/// This is only kept aroud for legacy game donkeys
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="engine"></param>
		/// <param name="objectType"></param>
		/// <param name="difficulty"></param>
		/// <param name="xmlContent"></param>
		/// <returns></returns>
		public BaseObject LoadXmlObject(Filename fileName, IGameDonkey engine, GameObjectType objectType, int difficulty, ContentManager xmlContent)
		{
			return LoadXmlObject(fileName, engine, objectType.ToString(), xmlContent);
		}

		public BaseObject LoadXmlObject(Filename fileName, IGameDonkey engine, string objectType, ContentManager xmlContent)
		{
			//load the model
			ObjectModelFactory(fileName, objectType, out BaseObjectModel gameObjectModel);
			gameObjectModel.ReadXmlFile(xmlContent);

			return LoadXmlObject(gameObjectModel, engine, objectType, xmlContent);
		}

		public BaseObject LoadXmlObject(BaseObjectModel gameObjectModel, IGameDonkey engine, string objectType, ContentManager xmlContent)
		{
			var gameObject = Inactive.FirstOrDefault(x => x.ObjectType == objectType);
			if (null == gameObject)
			{
				//try to load the object
				ObjectFactory(objectType, out gameObject);

				//load the object data into the thing
				gameObject.PlayerQueue = this;
				gameObject.ParseXmlData(gameObjectModel, engine, xmlContent);

				//add to the correct list
				if (objectType == "Level")
				{
					Active.Add(gameObject);
				}
				else
				{
					Inactive.Add(gameObject);
				}

				//set the color too
				gameObject.PlayerColor = PlayerColor;
			}

			return gameObject;
		}

		protected virtual void ObjectFactory(string objectType, out BaseObject gameObject)
		{
			switch (objectType)
			{
				case "Human":
					{
						gameObject = CreateHumanPlayer();

						//set as the main character
						AddCharacterToList(gameObject);
					}
					break;
				case "AI":
					{
						gameObject = CreateAiPlayer();

						//set as the main character
						AddCharacterToList(gameObject);
					}
					break;
				case "Projectile":
					{
						gameObject = new ProjectileObject(CharacterClock, Character, QueueId);
					}
					break;
				case "Level":
					{
						gameObject = new LevelObject(CharacterClock, QueueId);
					}
					break;
				default:
					{
						throw new Exception($"Unknown objectType passed to ObjectFactory: {objectType}");
					}
			}
		}

		protected virtual void ObjectModelFactory(Filename fileName, string objectType, out BaseObjectModel gameObjectModel)
		{
			switch (objectType)
			{
				case "Human":
					{
						gameObjectModel = CreatePlayerObjectModel(fileName);
					}
					break;
				case "AI":
					{
						gameObjectModel = CreatePlayerObjectModel(fileName);
					}
					break;
				case "Projectile":
					{
						gameObjectModel = new ProjectileObjectModel(fileName);
					}
					break;
				case "Level":
					{
						gameObjectModel = new LevelObjectModel(fileName);
					}
					break;
				default:
					{
						throw new Exception($"Unknown objectType passed to ObjectModelFactory: {objectType}");
					}
			}
		}

		protected virtual void AddCharacterToList(BaseObject gameObject)
		{
			Character = gameObject as PlayerObject;
		}

		#endregion
	}
}