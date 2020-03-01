using AnimationLib;
using FilenameBuddy;
using GameDonkeyLib;
using GameTimer;
using HadoukInput;
using Microsoft.Xna.Framework.Content;
using StateMachineBuddy;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameDonkeyLib
{
	/// <summary>
	/// This is a model of an "item", which is something that can be instanced.
	/// </summary>
	public abstract class ItemObjectModel
	{
		#region Properties

		/// <summary>
		/// The model of this object if it is a summons
		/// </summary>
		protected BaseObjectModel ObjectModel { get; set; }

		protected string ObjectType { get; set; }

		/// <summary>
		/// Animations that will be added to the main characer
		/// </summary>
		protected AnimationsModel Animations { get; set; }

		/// <summary>
		/// A set of state changes that will be added to the main character
		/// </summary>
		protected StateMachineModel StateChanges { get; set; }

		/// <summary>
		/// A list of state actions that will be added to the main character
		/// </summary>
		protected SingleStateContainerModel StateContainerModel { get; set; }

		/// <summary>
		/// A list of all the garments to add to the main character
		/// </summary>
		protected List<GarmentModel> GarmentModels { get; set; }

		/// <summary>
		/// A list of moves to add to the input for the player
		/// </summary>
		protected MoveListModel Moves { get; set; }

		#endregion //Properties

		#region Init

		protected ItemObjectModel()
		{
			GarmentModels = new List<GarmentModel>();
		}

		public abstract void Load(float scale, IGameDonkey engine, ContentManager xmlContent);

		protected void LoadObjectModel(string objectFile, string objectType, ContentManager xmlContent)
		{
			//load up the animations from file
			ObjectType = objectType;
			ObjectModel = new PlayerObjectModel(new Filename(objectFile));
			ObjectModel.ReadXmlFile(xmlContent);
		}

		protected void LoadAnimations(float scale, string animationsFile, ContentManager xmlContent)
		{
			//load up the animations from file
			Animations = new AnimationsModel(new Filename(animationsFile), scale);
			Animations.ReadXmlFile(xmlContent);
		}

		protected void LoadStateChanges(string stateMachineFile, ContentManager xmlContent)
		{
			StateChanges = new StateMachineModel(new Filename(stateMachineFile));
			StateChanges.ReadXmlFile(xmlContent);
		}

		protected void LoadStateActions(string stateActionsFile, ContentManager xmlContent)
		{
			StateContainerModel = new SingleStateContainerModel(new Filename(stateActionsFile));
			StateContainerModel.ReadXmlFile(xmlContent);
		}

		protected void LoadGarment(float scale, IGameDonkey engine, string garmentFile, ContentManager xmlContent)
		{
			var garmentModel = new GarmentModel(new Filename(garmentFile), scale);
			garmentModel.ReadXmlFile(xmlContent);
			GarmentModels.Add(garmentModel);
		}

		protected void LoadMoves(string movesFile, ContentManager xmlContent)
		{
			Moves = new MoveListModel(new Filename(movesFile));
			Moves.ReadXmlFile(xmlContent);
		}

		#endregion //Init

		#region Methods

		public virtual void Activate(PlayerQueue player, IGameDonkey engine, ContentManager content)
		{
			AddAnimations(player);
			AddGarments(player, engine);
			ActivateCharacter(player, engine);
			AddStateMachine(player);
			AddStateActions(player, engine, content);
			AddInput(player);
		}

		public virtual void Deactivate(PlayerQueue player)
		{
			RemoveAnimations(player);
			RemoveGarments(player);
			RemoveCharacter(player);
			RemoveStateMachine(player);
			RemoveStateActions(player);
			RemoveInput(player);
		}

		protected void AddAnimations(PlayerQueue player)
		{
			if (null != Animations)
			{
				player.Character.AnimationContainer.AddAnimations(Animations);
			}
		}

		protected void RemoveAnimations(PlayerQueue player)
		{
			if (null != Animations)
			{
				player.Character.AnimationContainer.RemoveAnimations(Animations);
			}
		}

		protected virtual BaseObject ActivateCharacter(PlayerQueue player, IGameDonkey engine)
		{
			BaseObject summonedObject = null;
			if (null != ObjectModel)
			{
				//load the character into the playerqueue
				using (var content = new ContentManager(engine.Game.Services, "Content"))
				{
					summonedObject = player.LoadXmlObject(ObjectModel, engine, ObjectType, content);

					//set the position of the object
					summonedObject.Flip = player.Character.Flip;
					summonedObject.Position = player.Character.Position;

					player.ActivateObject(summonedObject);
				}
			}
			return summonedObject;
		}

		protected void RemoveCharacter(PlayerQueue player)
		{
			if (null != ObjectModel)
			{
				player.DeactivateObjects(ObjectType);
			}
		}

		protected void AddStateMachine(PlayerQueue player)
		{
			if (null != StateChanges)
			{
				foreach (var container in player.Character.States.StateContainers)
				{
					container.StateMachine.AddStateMachine(StateChanges);
				}
			}
		}

		protected void RemoveStateMachine(PlayerQueue player)
		{
			if (null != StateChanges)
			{
				foreach (var container in player.Character.States.StateContainers)
				{
					container.StateMachine.RemoveStateMachine(StateChanges);
				}
			}
		}

		protected void AddStateActions(PlayerQueue player, IGameDonkey engine, ContentManager content)
		{
			if (null != StateContainerModel)
			{
				var stateActions = new StateMachineActions();
				stateActions.LoadStateActions(StateChanges.StateNames, StateContainerModel, player.Character, player.Character.States);
				stateActions.LoadContent(engine, content);

				foreach (var container in player.Character.States.StateContainers)
				{
					container.Actions.AddStateMachineActions(stateActions);
				}
			}
		}

		protected void RemoveStateActions(PlayerQueue player)
		{
			if (null != StateContainerModel)
			{
				foreach (var container in player.Character.States.StateContainers)
				{
					container.Actions.RemoveStateMachineActions(StateContainerModel);
				}
			}
		}

		protected void AddGarments(PlayerQueue player, IGameDonkey engine)
		{
			foreach (var garmentModel in GarmentModels)
			{
				//Create the garment
				var garment = player.Character.Garments.LoadGarment(garmentModel, engine.Renderer);
				player.Character.Garments.AddGarment(garment);
			}
		}

		protected void RemoveGarments(PlayerQueue player)
		{
			foreach (var garmentModel in GarmentModels)
			{
				player.Character.Garments.RemoveGarment(garmentModel);
			}
		}

		protected void AddInput(PlayerQueue player)
		{
			if (null != Moves)
			{
				player.InputQueue.Moves.AddMoves(Moves);
			}
		}

		protected void RemoveInput(PlayerQueue player)
		{
			if (null != Moves)
			{
				player.InputQueue.Moves.RemoveMoves(Moves);
			}
		}

		#endregion //Methods
	}
}
