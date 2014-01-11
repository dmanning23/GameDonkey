using System;
using HadoukInput;
using GameTimer;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using FilenameBuddy;
using System.IO;
using System.Xml;
using MatrixExtensions;
using GameDonkey;

namespace GameDonkey
{
	/// <summary>
	/// this is a player game token, either human or AI
	/// </summary>
	public class RoboJetsPlayerObject : PlayerObject
	{
		#region Members

		/// <summary>
		/// max speed how fast characters can rotate while flying
		/// </summary>
		private const float FlyingRotationSpeed = (540.0f * ((float)Math.PI / 180.0f));

		/// <summary>
		/// the angle of the direction this dude is flying
		/// </summary>
		private float m_fRotation;

		/// <summary>
		/// the target angle of animation rotation
		/// </summary>
		private float m_fTargetAnimationRotation;

		private Vector2 RightThumbstickDirection { get; set; }

		#endregion //Members

		#region Properties

		#endregion //Properties

		#region Methods

		public RoboJetsPlayerObject(HitPauseClock rClock, int iQueueID)
			: base(rClock, iQueueID)
		{
			//init is called by the base class, which will set everything up
		}

		/// <summary>
		/// Constructor for replacing a network player when they leave the game
		/// </summary>
		/// <param name="rHuman">the dude to be replaced, copy all his shit</param>
		public RoboJetsPlayerObject(EObjectType eType, PlayerObject rHuman)
			: base(eType, rHuman)
		{
			//TODO: copy all the required shit
		}

		/// <summary>
		/// Reset the object for game start, character death
		/// </summary>
		public override void Reset()
		{
			base.Reset();
			m_fRotation = 0.0f;
			m_fTargetAnimationRotation = 0.0f;
			RightThumbstickDirection = Vector2.Zero;
		}

		/// <summary>
		/// Update the character rotation before they are animated.
		/// Overload this function in the child class for your game
		/// </summary>
		public override void UpdateRotation()
		{
			//get the rotation based on direction the right thumbstick is pointing
			m_fRotation = Helper.ClampAngle(Helper.atan2(new Vector2(m_Velocity.X * -1.0f, m_Velocity.Y)));

			//if the character isn't flying very fast, stand them straight up
			if ((States.StateMachineIndex() > 0) &&
			    (m_Velocity.LengthSquared() > (m_fNeutralFlyingSpeed * m_fNeutralFlyingSpeed)))
			{
			    //rotate 90 degrees to make the rotation the up vector
			    m_fTargetAnimationRotation = Helper.ClampAngle(m_fRotation - MathHelper.ToRadians(90.0f));
			}
			else
			{
			    m_fTargetAnimationRotation = 0.0f;
			}

			//rotate characters slower so they don't pop (180 degrees/second sounds good)
			if (Math.Abs(CurrentAnimationRotation - m_fTargetAnimationRotation) < (FlyingRotationSpeed * CharacterClock.TimeDelta))
			{
			    CurrentAnimationRotation = m_fTargetAnimationRotation;
			}
			else
			{
			    //how far do we have to go?
			    float fRotation = CurrentAnimationRotation - m_fTargetAnimationRotation;
			    if (fRotation < 0.0f)
			    {
			        CurrentAnimationRotation += (FlyingRotationSpeed * CharacterClock.TimeDelta);
			    }
			    else
			    {
			        CurrentAnimationRotation -= (FlyingRotationSpeed * CharacterClock.TimeDelta);
			    }
			}
		}

		/// <summary>
		/// update an input wrapper
		/// </summary>
		/// <param name="rController"></param>
		/// <param name="rInput"></param>
		public override void UpdateInput(InputWrapper rController, InputState rInput)
		{
			//grab the right thumbstick direction
			RightThumbstickDirection = rController.Controller.Thumbsticks.RightThumbstick.Direction;

			//create the direction from the current rotation
			Matrix rotation = MatrixExt.Orientation(CurrentAnimationRotation);
			Vector2 direction = MatrixExt.Multiply(rotation, new Vector2(1.0f, 0.0f));

			rController.Update(rInput, Flip, direction);
		}

		/// <summary>
		/// Do all the specific processing to get player input.
		/// For human players, this means getting info from the controller.
		/// For AI players, this means reacting to info in the list of "bad guys"
		/// </summary>
		/// <param name="rController">the controller for this player (bullshit and ignored for AI)</param>
		/// <param name="listBadGuys">list of all the players (ignored for human players)</param>
		public override void GetPlayerInput(InputWrapper rController, List<PlayerQueue> listBadGuys)
		{
			//TODO: check right thumbsticks directions, send messages as needed

			//TODO: check the different attacks to shoot stuff

			base.GetPlayerInput(rController, listBadGuys);
		}

		/// <summary>
		/// Send any messages based on the direction the character is moving.
		/// </summary>
		public override void CheckMovementMessages()
		{
			//If this dude is currently flying, check the direction 
			if (States.StateMachineIndex() > 0)
			{
				if (!Flip)
				{
					//if facing forward but moving -x direction, send turn around message
					if (0.0f > Velocity.X)
					{
						SendStateMessage((int)EMessage.TurnAround);
					}
				}
				else
				{
					//else if facing backward but moving +x direction, send turn around message
					if (0.0f < Velocity.X)
					{
						SendStateMessage((int)EMessage.TurnAround);
					}
				}
			}
		}

		#endregion //Methods

		#region File IO

		/// <summary>
		/// Given an xml node, parse the contents.
		/// Override in child classes to read object-specific node types.
		/// </summary>
		/// <param name="childNode">the xml node to read</param>
		/// <param name="rEngine">the engine we are using to load</param>
		/// <param name="iMessageOffset">the message offset of this object's state machine</param>
		/// <returns></returns>
		public override bool ParseXmlNode(XmlNode childNode, IGameDonkey rEngine, int iMessageOffset)
		{
			//what is in this node?
			string strName = childNode.Name;
			string strValue = childNode.InnerXml;

			switch (strName)
			{
				case "GroundStates":
				{
					//get the ground states of this dude
					Filename groundStatesFile = new Filename(strValue);
					if (!States.ReadXmlStateContainer(new Filename(@"Robot\robot state machine.xml"),
						iMessageOffset, 
						groundStatesFile, 
						this,
						rEngine, 
						true, 
						false))
					{
						Debug.Assert(false);
						return false;
					}
					return true;
				}

				case "UpStates":
				{
					//get teh upstates of this dude
					Filename upStatesFile = new Filename(childNode.InnerXml);
					if (!States.ReadXmlStateContainer(new Filename(@"Robot\robot state machine.xml"),
						iMessageOffset, 
						upStatesFile, 
						this, 
						rEngine, 
						true, 
						true))
					{
						Debug.Assert(false);
						return false;
					}
					return true;
				}

				case "DownStates":
				{
					////load the down states
					//strStatesFile.SetRelFilename(myCharXML.DownStates);
					//States.ReadStateContainer(rXmlContent, @"state machines\down state machine", iMessageOffset, strStatesFile.GetRelPathFileNoExt(), this, rEngine, true, true);
					//rXmlContent.Unload();
					return true;
				}

				case "ForwardStates":
				{
					////load the forward states
					//strStatesFile.SetRelFilename(myCharXML.ForwardStates);
					//States.ReadStateContainer(rXmlContent, @"state machines\forward state machine", iMessageOffset, strStatesFile.GetRelPathFileNoExt(), this, rEngine, true, true);
					//rXmlContent.Unload();
					return true;
				}

				default:
				{
					//punt to the base class
					return base.ParseXmlNode(childNode, rEngine, iMessageOffset);
				}
			}
		}

		public override bool LoadSerializedObject(ContentManager rXmlContent, Filename strResource, IGameDonkey rEngine, int iMessageOffset)
		{
			SPFSettings.PlayerObjectXML myCharXML = rXmlContent.Load<SPFSettings.PlayerObjectXML>(strResource.GetRelPathFileNoExt());

			//load the base object stuff
			Debug.Assert(null != PlayerQueue);

			Filename strModelFile = new Filename(myCharXML.model);
			Filename strAnimationFile = new Filename(myCharXML.animations);
			Filename strStateMachineFile = new Filename(myCharXML.stateMachine);
			m_fHeight = (float)myCharXML.height;

			//try to load the model
			if (!AnimationContainer.ReadSerializedModelFormat(rXmlContent, strModelFile, rEngine.Renderer))
			{
				Debug.Assert(false);
				return false;
			}
			rXmlContent.Unload();

			//load all the garments
			foreach (string strGarment in myCharXML.garments)
			{
				//get the garment filename
				Filename strGarmentFile = new Filename(strGarment);
				LoadSerializedGarment(rXmlContent, rEngine, strGarmentFile);
			}

			//read in the animations
			AnimationContainer.ReadSerializedAnimationFormat(rXmlContent, strAnimationFile);
			rXmlContent.Unload();

			//get the character portrait, load it from teh renderer content manager
			Filename strPortraitFile = new Filename(myCharXML.portrait);
			Debug.Assert(null != rEngine.Renderer.Content);
			m_Portrait = rEngine.Renderer.Content.Load<Texture2D>(strPortraitFile.GetRelPathFileNoExt());

			//TODO: grab the deathsound
			DeathSound = myCharXML.deathSound;
			//Debug.Assert(null != CAudioManager.GetCue(m_strDeathSound));

			//SPARROWHAWKS

			//load the ground states
			Filename strStatesFile = new Filename(myCharXML.GroundStates);
			States.ReadSerializedStateContainer(rXmlContent,
				new Filename(@"wedding state machines\ground state machine"), 
				iMessageOffset, 
				strStatesFile, 
				this, 
				rEngine, 
				true, 
				false);
			rXmlContent.Unload();

			//load the up states
			strStatesFile.SetRelFilename(myCharXML.UpStates);
			States.ReadSerializedStateContainer(rXmlContent,
				new Filename(@"wedding state machines\up state machine"), 
				iMessageOffset, 
				strStatesFile, 
				this, 
				rEngine, 
				true, 
				true);
			rXmlContent.Unload();

			////load the down states
			//strStatesFile.SetRelFilename(myCharXML.DownStates);
			//States.ReadStateContainer(rXmlContent, @"state machines\down state machine", iMessageOffset, strStatesFile.GetRelPathFileNoExt(), this, rEngine, true, true);
			//rXmlContent.Unload();

			////load the forward states
			//strStatesFile.SetRelFilename(myCharXML.ForwardStates);
			//States.ReadStateContainer(rXmlContent, @"state machines\forward state machine", iMessageOffset, strStatesFile.GetRelPathFileNoExt(), this, rEngine, true, true);
			//rXmlContent.Unload();

			return true;
		}

		#endregion //File IO
	}
}