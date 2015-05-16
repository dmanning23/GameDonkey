using FilenameBuddy;
using GameTimer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using ParticleBuddy;
using RenderBuddy;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework.Input;

namespace GameDonkey
{
	public class GdiDonkey : GameDonkeyBase
	{
		#region Members

		WinFormRenderer m_Renderer;

		static protected Random g_Random = new Random(DateTime.Now.Millisecond);

		//debugging flags
#if WINDOWS
		protected KeyboardState m_LastKeyboardState;
#endif
		protected bool m_bRenderJointSkeleton;
		protected bool m_bRenderPhysics;

		/// <summary>
		/// render the dots used to calculate camera position
		/// </summary>
		protected bool m_bDrawCameraInfo;
		protected bool m_bRenderWorldBoundaries;
		protected bool m_bRenderSpawnPoints;

		#endregion //Members

		#region Properties

		public PlayerQueue Player { get; private set; }

		/// <summary>
		/// Clock used to update all the characters & board objects
		/// </summary>
		public GameClock CharacterClock { get; private set; }

		public override IRenderer Renderer
		{
			get
			{
				return m_Renderer;
			}
		}

		#endregion //Properties

		#region Methods

		#region Construction

		public GdiDonkey(WinFormRenderer rRenderer) : base()
		{
			m_Renderer = rRenderer;
			Setup();
		}

		private void Setup()
		{
			Player = null;
			CharacterClock = new GameClock();

			//debugging stuff
			m_bRenderJointSkeleton = false;
			m_bRenderPhysics = false;
			m_bDrawCameraInfo = false;
			m_bRenderWorldBoundaries = true;
			m_bRenderSpawnPoints = false;
		}

		#endregion //Construction

		private Vector2 CenterPoint()
		{
			Debug.Assert(null != WorldBoundaries);
			Debug.Assert(null != WorldBoundaries.Center);
			return new Vector2(WorldBoundaries.Center.X, WorldBoundaries.Center.Y);
		}

		/// <summary>
		/// Change the speed of the character clock
		/// </summary>
		/// <param name="fSpeed">multiplier to speed up/slow down the character clock</param>
		public void SetClockSpeed(float fSpeed)
		{
			CharacterClock.TimerSpeed = fSpeed;
		}

		public override void Start()
		{
			base.Start();

			//speed up the character clock
			SetClockSpeed(1.15f);

			//reset the game timer
			Debug.Assert(null != CharacterClock);
			CharacterClock.Start();

			//reset the players
			Debug.Assert(null != Player);
			Player.Reset(CenterPoint());
			Player.Character.Flip = false;

			Debug.Assert(null != Renderer);
			Renderer.Camera.ForceToScreen();
		}

		/// <summary>
		/// update the game engine.  
		/// This function is overridden in child classes.  
		/// The server class checks for game over and updates the client class
		/// The client class will not check for game over.
		/// </summary>
		/// <returns>bool: whether or not this update resulted in a game over situation</returns>
		protected override bool Update()
		{
			//update the camera stuff
			Renderer.Camera.Update(MasterClock);

			//update all our clocks
			CharacterClock.Update(MasterClock);

			Debug.Assert(null != Player);

			//update the characters
			Player.Update(CharacterClock, true);

			//check for world collisions
			Player.CheckWorldCollisions(WorldBoundaries);

			//respond to hits!
			Player.RespondToHits(this);

			//update all the ragdoll stuff
			Player.UpdateRagdoll(false);

			Player.UpdateDrawlists();

			//update the particle engine!!
			ParticleEngine.Update(MasterClock);
			
			//debugging stuff!!!
#if FALSE
			KeyboardState currentState = Keyboard.GetState();
			if (currentState.IsKeyDown(Keys.Y) && m_LastKeyboardState.IsKeyUp(Keys.Y))
			{
				m_bRenderSpawnPoints = !m_bRenderSpawnPoints;
			}5
			if (currentState.IsKeyDown(Keys.U) && m_LastKeyboardState.IsKeyUp(Keys.U))
			{
				m_bRenderJointSkeleton = !m_bRenderJointSkeleton;
			}
			if (currentState.IsKeyDown(Keys.I) && m_LastKeyboardState.IsKeyUp(Keys.I))
			{
				m_bRenderPhysics = !m_bRenderPhysics;
			}
			if (currentState.IsKeyDown(Keys.O) && m_LastKeyboardState.IsKeyUp(Keys.O))
			{
				m_bDrawCameraInfo = !m_bDrawCameraInfo;
			}
			if (currentState.IsKeyDown(Keys.P) && m_LastKeyboardState.IsKeyUp(Keys.P))
			{
				m_bRenderWorldBoundaries = !m_bRenderWorldBoundaries;
			}
#endif

			return false;
		}

		public void RespawnPlayer(PlayerQueue rPlayerQueue)
		{
			//respawn the player
			rPlayerQueue.Reset(CenterPoint());
			rPlayerQueue.Character.Flip = false;
		}

		#region Draw

		public override void Render()
		{
			RenderLevel(Matrix.Identity);

			RenderCharacterTrails(Matrix.Identity);

			RenderCharacters(Matrix.Identity);

			RenderParticleEffects(Matrix.Identity);
		}

		protected override void RenderLevel(Matrix cameraMatrix)
		{
			//draw the world boundaries in debug mode?
			if (m_bRenderWorldBoundaries)
			{
				Renderer.Primitive.Rectangle(
					new Vector2(WorldBoundaries.Left, WorldBoundaries.Top),
					new Vector2(WorldBoundaries.Right, WorldBoundaries.Bottom),
					0.0f, 1.0f, Color.Red);
			}
		}

		protected override void RenderCharacterTrails(Matrix cameraMatrix)
		{
			//render all the character trails, start another spritebatch
			Player.Render(Renderer, false);
		}

		protected override void RenderCharacters(Matrix cameraMatrix)
		{
			//render all the players
			Player.Render(Renderer, true);

			//draw debug info?
			if (m_bRenderPhysics)
			{
				for (int j = 0; j < Player.ActiveObjects.Count; j++)
				{
					Player.ActiveObjects[j].AnimationContainer.Model.DrawPhysics(Renderer, true, Color.White);
				}
			}

			//draw the push box for each character?
			if (m_bRenderJointSkeleton)
			{
				for (int j = 0; j < Player.ActiveObjects.Count; j++)
				{
					Renderer.Primitive.Circle(Player.Character.Position,
						(int)(Player.Character.MinDistance()),
						Color.White);
				}
			}

			////draw bones, ragdoll
			//m_listPlayers[i].Character.AnimationContainer.Model.RenderJointSkeleton(Renderer);
			//m_listPlayers[i].Character.AnimationContainer.Model.RenderOutline(Renderer, 1.0f);
			//m_listPlayers[i].Character.AnimationContainer.Model.DrawSkeleton(Renderer, true, Color.White);
			//m_listPlayers[i].Character.AnimationContainer.Model.DrawJoints(Renderer, true, Color.Red);
		}

		protected override void RenderParticleEffects(Matrix cameraMatrix)
		{
			//draw all the particles, start another spritebatch for the particles
			ParticleEngine.Render(Renderer);
		}

		#endregion //Draw

		#endregion //Methods

		#region File IO

		/// <summary>
		/// load a player into this game engine
		/// </summary>
		/// <param name="strDataFile">filename of the character data xml file to load</param>
		/// <returns>player queue with all the player's stuff in it</returns>
		public PlayerQueue LoadXmlPlayer(Filename strDataFile)
		{
			Player = CreatePlayerQueue(Color.White, 0);
			if (null == Player.LoadXmlObject(strDataFile, this, EObjectType.Human, 1))
			{
				Debug.Assert(false);
				Player = null;
			}

			return Player;
		}

		#endregion //File IO
	}
}