using System;
using HadoukInput;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using GameTimer;
using FontBuddyLib;
#if NETWORKING
using Microsoft.Xna.Framework.Net;
#endif
using RenderBuddy;
using ParticleBuddy;
using FilenameBuddy;
using ResolutionBuddy;

namespace GameDonkey
{
	public class CSPFDonkey : IGameDonkey
	{
		#region Members

		/// <summary>
		/// list of all the player objects in the game
		/// </summary>
		protected List<CPlayerQueue> m_listPlayers;

		/// <summary>
		/// player queue for updating level objects
		/// </summary>
		protected CPlayerQueue m_LevelObjects;

		/// <summary>
		/// the spawn points for characters
		/// </summary>
		protected List<Vector2> m_listSpawnPoints;

		static protected Random g_Random = new Random(DateTime.Now.Millisecond);

		//debugging flags
		protected KeyboardState m_LastKeyboardState;
		protected bool m_bRenderJointSkeleton;
		protected bool m_bRenderPhysics;

		/// <summary>
		/// render the dots used to calculate camera position
		/// </summary>
		protected bool m_bDrawCameraInfo;
		protected bool m_bRenderWorldBoundaries;
		protected bool m_bRenderSpawnPoints;

		private Texture2D m_SkyBox;
		private Texture2D m_HUDBackground;
		private Color m_SkyColor;
		private int m_iNumTiles;

		FontBuddy m_Font;

		/// <summary>
		/// Timer used to countdown to end of game
		/// </summary>
		CountdownTimer m_GameTimer;

		/// <summary>
		/// Clock used to update all the characters & board objects
		/// </summary>
		GameClock m_CharacterClock;

		//Game over stuff!!!
		CPlayerQueue m_rWinner;
		bool m_bTie;
		bool m_bGameOver;

		/// <summary>
		/// the music resource for the current board
		/// </summary>
		string m_strMusicFile;

		/// <summary>
		/// The noise to play when a character falls off the board
		/// </summary>
		private string m_strDeathNoise;

		/// <summary>
		/// the center point between all the players
		/// </summary>
		private Vector2 m_CenterPoint;

		/// <summary>
		/// a list of all the default particle effects used int he game
		/// </summary>
		private List<EmitterTemplate> m_DefaultParticles;

		#endregion //Members

		#region Properties

		public CPlayerQueue Character
		{
			get { return m_listPlayers[0]; }
		}

		public List<CPlayerQueue> Players
		{
			get { return m_listPlayers; }
		}

		public CPlayerQueue LevelObjects
		{
			get { return m_LevelObjects; }
		}

		/// <summary>
		/// The win condition for this game
		/// </summary>
		public EGameMode GameMode { get; private set; }

		/// <summary>
		/// The max amount of time a game will last
		/// </summary>
		public float MaxTime { get; private set; }

		public CountdownTimer GameTimer
		{
			get { return m_GameTimer; }
		}

		public GameClock CharacterClock
		{
			get { return m_CharacterClock; }
		}

		public bool GameOver
		{
			get { return m_bGameOver; }
		}

		public string Music
		{
			get { return m_strMusicFile; }
		}

		private EmitterTemplate Block
		{
			get { return m_DefaultParticles[(int)EDefaultParticleEffects.Block]; }
		}

		private EmitterTemplate HitSpark
		{
			get { return m_DefaultParticles[(int)EDefaultParticleEffects.HitSpark]; }
		}

		private EmitterTemplate HitCloud
		{
			get { return m_DefaultParticles[(int)EDefaultParticleEffects.HitCloud]; }
		}

		private EmitterTemplate StunnedBounce
		{
			get { return m_DefaultParticles[(int)EDefaultParticleEffects.StunnedBounce]; }
		}

		private EmitterTemplate DeathParticles
		{
			get { return m_DefaultParticles[(int)EDefaultParticleEffects.Death]; }
		}

		private EmitterTemplate HeadBop
		{
			get { return m_DefaultParticles[(int)EDefaultParticleEffects.HeadBop]; }
		}

		private EmitterTemplate WeaponHit
		{
			get { return m_DefaultParticles[(int)EDefaultParticleEffects.WeaponHit]; }
		}

		#endregion //Properties

		#region Construction

		public CSPFDonkey(Renderer rRenderer)
			: base(rRenderer)
		{
			Setup();
		}

		private void Setup()
		{
			m_listPlayers = new List<CPlayerQueue>();
			m_LevelObjects = new CLevelObjectQueue();
			m_listSpawnPoints = new List<Vector2>();

			m_DefaultParticles = new List<EmitterTemplate>();
			for (EDefaultParticleEffects i = 0; i < EDefaultParticleEffects.NumDefaultParticleEffects; i++)
			{
				m_DefaultParticles.Add(new EmitterTemplate());
			}

			m_Font = new FontBuddy();
			m_CharacterClock = new GameClock();

			//debugging stuff
			m_bRenderJointSkeleton = false;
			m_bRenderPhysics = false;
			m_bDrawCameraInfo = false;
			m_bRenderWorldBoundaries = false;
			m_bRenderSpawnPoints = false;

			//game over stuff
			m_GameTimer = new CountdownTimer();
			GameMode = EGameMode.Stock;
			MaxTime = 186.0f;
			m_rWinner = null;
			m_bGameOver = false;
			m_bTie = false;

			m_SkyBox = null;
			m_HUDBackground = null;
			m_SkyColor = Color.White;
			m_iNumTiles = 1;
			m_CenterPoint = Vector2.Zero;
		}

		/// <summary>
		/// load all the content in an xna game
		/// </summary>
		/// <param name="rContent">content manager</param>
		public override void LoadContent(ContentManager rXmlContent, GraphicsDevice rGraphics)
		{
			m_LastKeyboardState = Keyboard.GetState();

			//load all the content

			//load up the renderer graphics content, so we can use its conent manager to load all our graphics
			Renderer.LoadContent();

			//load the background image used for the HUD
			m_HUDBackground = Renderer.Content.Load<Texture2D>(@"Resources\HUDBackground");

			//load the hit spark
			HitSpark.ReadSerializedFile(rXmlContent, @"Resources\Particles\Hit Spark", Renderer);

			//load the hit cloud
			HitCloud.ReadSerializedFile(rXmlContent, @"Resources\Particles\Hit Cloud", Renderer);

			//load the death particle effect
			DeathParticles.ReadSerializedFile(rXmlContent, @"Resources\Particles\Death Particles", Renderer);

			//load the block particle effect
			Block.ReadSerializedFile(rXmlContent, @"Resources\Particles\Block", Renderer);

			//load the weapon hit particle effect
			WeaponHit.ReadSerializedFile(rXmlContent, @"Resources\Particles\Weapon Hit", Renderer);

			//load the head bop particle effect
			HeadBop.ReadSerializedFile(rXmlContent, @"Resources\Particles\ceiling bop", Renderer);

			//load the stunned bounce particle effect
			StunnedBounce.ReadSerializedFile(rXmlContent, @"Resources\Particles\Stunned Bounce", Renderer);

			//load up our sprite font
			Debug.Assert(null != m_Font);
			m_Font.LoadContent(Renderer.Content);
		}

		/// <summary>
		/// load all a players data into the game
		/// </summary>
		/// <param name="rContent">content manager to use to load</param>
		/// <param name="myColor">color for this player</param>
		/// <param name="strCharacterFile">relative path for the player data file</param>
		/// <param name="eIndex">gamepad index for this player.</param>
		/// <param name="eType">the type of dude to load, accepts human and AI</param>
		/// <returns></returns>
		public CPlayerQueue LoadHumanPlayer(ContentManager rXmlContent,
			Color myColor,
			string strCharacterFile,
			PlayerIndex eIndex,
			string strPlayerName)
		{
			//create and load a player
			CPlayerQueue rPlayer = new CPlayerQueue(myColor, m_listPlayers.Count);
			Filename myChar = new Filename();
			myChar.SetRelFilename(strCharacterFile);
			if (null == rPlayer.LoadObject(rXmlContent, myChar, this, EObjectType.Human, 0))
			{
				Debug.Assert(false);
			}
			m_listPlayers.Add(rPlayer);

			//create a controller for that player
			InputWrapper rQueue = new InputWrapper(eIndex, MasterClock.GetCurrentTime);
			if (!rQueue.ReadSerializedFile(rXmlContent, @"Resources\Move List", rPlayer.Character.States.GetMessageIndexFromText))
			{
				Debug.Assert(false);
			}
			rPlayer.InputQueue = rQueue;

			rPlayer.PlayerName = strPlayerName;
			return rPlayer;
		}

		public CPlayerQueue LoadAiPlayer(ContentManager rXmlContent,
			Color myColor,
			string strCharacterFile,
			int iDifficulty,
			string strPlayerName)
		{
			//create and load a player
			CPlayerQueue rPlayer = new CPlayerQueue(myColor, m_listPlayers.Count);
			Filename myChar = new Filename();
			myChar.SetRelFilename(strCharacterFile);
			if (null == rPlayer.LoadObject(rXmlContent, myChar, this, EObjectType.AI, iDifficulty))
			{
				Debug.Assert(false);
			}
			m_listPlayers.Add(rPlayer);

			//create a controller for that player
			InputWrapper rQueue = new InputWrapper(PlayerIndex.One, MasterClock.GetCurrentTime);
			if (!rQueue.ReadSerializedFile(rXmlContent, @"Resources\Move List", rPlayer.Character.States.GetMessageIndexFromText))
			{
				Debug.Assert(false);
			}
			rPlayer.InputQueue = rQueue;

			rPlayer.PlayerName = strPlayerName;
			return rPlayer;
		}

		public void LoadBoard(ContentManager rXmlContent, string strBoardFile)
		{
			//load the resource
			SPFSettings.BoardXML myDude = rXmlContent.Load<SPFSettings.BoardXML>(strBoardFile);

			//grab all the spawn points
			for (int i = 0; i < myDude.spawnPoints.Count; i++)
			{
				m_listSpawnPoints.Add(myDude.spawnPoints[i].location);
			}

			//grab teh name of teh music resource for this board
			m_strMusicFile = myDude.music;

			//TODO: load the death noise
			m_strDeathNoise = myDude.deathNoise;
			//Debug.Assert(null != CAudioManager.GetCue(m_strDeathNoise));

			//open the background image stuff
			m_SkyBox = Renderer.Content.Load<Texture2D>(myDude.backgroundTile);
			m_SkyColor.R = myDude.backgroundR;
			m_SkyColor.G = myDude.backgroundG;
			m_SkyColor.B = myDude.backgroundB;
			m_SkyColor.A = 255;

			m_iNumTiles = myDude.numTiles;

			//grab the world boundaries
			WorldBoundaries = new Rectangle((-1 * (myDude.boardWidth / 2)),
				(-1 * (myDude.boardHeight / 2)),
				myDude.boardWidth,
				myDude.boardHeight);

			//load all the level objects
			for (int i = 0; i < myDude.objects.Count; i++)
			{
				//load the level object
				Filename myLevelObjectFile = new Filename();
				myLevelObjectFile.SetRelFilename(myDude.objects[i]);
				BaseObject rLevelObject = m_LevelObjects.LoadObject(rXmlContent, myLevelObjectFile, this, EObjectType.Level, 0);
				if (null == rLevelObject)
				{
					Debug.Assert(false);
				}
			}

			m_LevelObjects.PlayerName = "Board";
		}

		#endregion //Construction

		#region Methods

		/// <summary>
		/// Change the speed of the character clock
		/// </summary>
		/// <param name="fSpeed">multiplier to speed up/slow down the character clock</param>
		public void SetClockSpeed(float fSpeed)
		{
			m_CharacterClock.TimerSpeed = fSpeed;
		}

		public override void Start()
		{
			base.Start();

			//speed up the character clock
			SetClockSpeed(1.0f);

			//reset the game timer
			m_GameTimer.Start(MaxTime);
			m_CharacterClock.Start();

			//reset teh level objects
			m_LevelObjects.Reset();

			//reset the players
			Debug.Assert(m_listSpawnPoints.Count > 0);
			int iSpawnIndex = 0;
			for (int i = 0; i < m_listPlayers.Count; i++)
			{
				Debug.Assert(null != m_listPlayers[i]);

				if (null != m_listPlayers[i].InputQueue)
				{
					Debug.Assert(null != m_listPlayers[i].InputQueue.Controller);
					m_listPlayers[i].InputQueue.Controller.ResetController();
				}
				m_listPlayers[i].Reset(m_listSpawnPoints[iSpawnIndex]);

				//increment to the next spawn point
				if (iSpawnIndex < (m_listSpawnPoints.Count - 1))
				{
					++iSpawnIndex;
				}
				else
				{
					iSpawnIndex = 0;
				}
			}

			//force the camera to fit the whole scene
			for (int i = 0; i < m_listPlayers.Count; i++)
			{
				m_listPlayers[i].AddToCamera(Renderer.Camera);
			}

			Renderer.Camera.ForceToScreen();
		}

		/// <summary>
		/// this is a hack put in just for local testing.
		/// In the real game, each individual player queue will be updated in the network game loop.
		/// </summary>
		/// <param name="rInput"></param>
		public void UpdateInput(InputState rInput)
		{
			for (int i = 0; i < m_listPlayers.Count; i++)
			{
				Debug.Assert(null != m_listPlayers[i]);

				if (null != m_listPlayers[i].InputQueue)
				{
					m_listPlayers[i].InputQueue.Update(rInput, m_listPlayers[i].Character.Flip);
				}
			}
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
			float fOldTime = m_GameTimer.RemainingTime();
			m_GameTimer.Update(MasterClock);
			m_CharacterClock.Update(m_GameTimer);

			//check for a winner
			if (!m_bGameOver)
			{
				//warn about time almost over?
				if (EGameMode.Time == GameMode)
				{
					if ((m_GameTimer.RemainingTime() < 20.0f) && (fOldTime >= 20.0f))
					{
						PlaySound("twenty");
					}
				}

				//check if anyone has won
				CheckForWinner();

				//update the level objects
				m_LevelObjects.Update(m_GameTimer, true);

				//update all the player stuff
				for (int i = 0; i < m_listPlayers.Count; i++)
				{
					Debug.Assert(null != m_listPlayers[i]);

					if (!CheckIfPlayerStockOut(m_listPlayers[i]))
					{
						//check if the player is dead
						CheckIfDead(m_listPlayers[i]);

						//update the characters
						m_listPlayers[i].Update(m_CharacterClock, true);
					}
				}
			}

			//TODO: update animation with master clock if game is over

			if (!m_bGameOver)
			{
				//check for collisions!
				for (int i = 0; i < m_listPlayers.Count; i++)
				{
					//check for collisions between players
					for (int j = i + 1; j < m_listPlayers.Count; j++)
					{
						m_listPlayers[i].CheckCollisions(m_listPlayers[j]);
					}

					//check for collisions with level objects
					m_listPlayers[i].CheckCollisions(m_LevelObjects);

					//check for world collisions
					m_listPlayers[i].CheckWorldCollisions(WorldBoundaries);
				}

				//respond to hits!
				for (int i = 0; i < m_listPlayers.Count; i++)
				{
					m_listPlayers[i].RespondToHits(this);
				}
				m_LevelObjects.RespondToHits(this);

				//Stick all the players together
				RubberBand();
			}

			//update all the ragdoll stuff
			for (int i = 0; i < m_listPlayers.Count; i++)
			{
				m_listPlayers[i].UpdateRagdoll(false);
			}
			m_LevelObjects.UpdateRagdoll(false);

			UpdateDrawlists();

			//update the particle engine!!
			ParticleEngine.Update(MasterClock);

			//debugging stuff!!!
#if DEBUG
			KeyboardState currentState = Keyboard.GetState();
			if (currentState.IsKeyDown(Keys.Y) && m_LastKeyboardState.IsKeyUp(Keys.Y))
			{
				m_bRenderSpawnPoints = !m_bRenderSpawnPoints;
			}
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

			return m_bGameOver;
		}

		protected virtual bool CheckForWinner()
		{
			//check if only one player remains
			int iNumPlayers = 0;
			for (int i = 0; i < m_listPlayers.Count; i++)
			{
				if (!CheckIfPlayerStockOut(m_listPlayers[i]))
				{
					iNumPlayers++;
				}
			}

			//there can be only one winner!
			if (1 >= iNumPlayers)
			{
				StopTimers();
				m_bGameOver = true;

				//find the winner!
				for (int i = 0; i < m_listPlayers.Count; i++)
				{
					if (!CheckIfPlayerStockOut(m_listPlayers[i]))
					{
						m_rWinner = m_listPlayers[i];
					}
				}

				if (0 == iNumPlayers)
				{
					//all the players died the same exact frame
					m_bTie = true;
				}
			}
			else
			{
				//check for time over
				Debug.Assert(0.0f < MaxTime);
				if ((null == m_rWinner) && (m_GameTimer.RemainingTime() <= 0.0f))
				{
					//find winner
					int iCurrentMaxStock = 0;
					for (int i = 0; i < m_listPlayers.Count; i++)
					{
						if (m_listPlayers[i].Stock >= iCurrentMaxStock)
						{
							//found someone with the max points, but is it a tie?
							if ((m_rWinner != null) && (m_rWinner != m_listPlayers[i]))
							{
								if (m_rWinner.Stock == m_listPlayers[i].Stock)
								{
									m_bTie = true;
								}
							}

							//TODO: whenever a winner is found, set their animation to the win animation

							m_rWinner = m_listPlayers[i];
							iCurrentMaxStock = m_rWinner.Stock;
						}
					}

					StopTimers();
					m_bGameOver = true;
				}
			}

			return m_bGameOver;
		}

		/// <summary>
		/// Check if a player has run out of stock
		/// </summary>
		/// <param name="rPlayerQueue">the player queue to check</param>
		/// <returns>true if the player has run out of stock</returns>
		protected virtual bool CheckIfPlayerStockOut(CPlayerQueue rPlayerQueue)
		{
			return (0 >= rPlayerQueue.Stock);
		}

		/// <summary>
		/// Check if an object is dead (out of bounds) and process the death
		/// </summary>
		/// <param name="rObject">the object to check for death</param>
		/// <returns>whether or not the thing is dead</returns>
		private bool CheckIfDead(CPlayerQueue rPlayerQueue)
		{
			Debug.Assert(null != rPlayerQueue);

			BaseObject rObject = rPlayerQueue.Character;
			Debug.Assert(null != rObject);
			Debug.Assert((rObject.Type == EObjectType.Human) || (rObject.Type == EObjectType.AI));

			if (rPlayerQueue.Character.DisplayHealth() <= 0)
			{
				KillPlayer(rPlayerQueue);
				return true;
			}

			return false;
		}

		/// <summary>
		/// Play the death particle effect
		///	Respawn the player
		/// Do the correct score calculation
		/// Play the players death sound
		/// </summary>
		/// <param name="rObject">the player to kill</param>
		private void KillPlayer(CPlayerQueue rPlayerQueue)
		{
			Debug.Assert(m_listSpawnPoints.Count > 0);
			Debug.Assert(null != rPlayerQueue);

			BaseObject rObject = rPlayerQueue.Character;
			Debug.Assert(null != rObject);
			Debug.Assert((rObject.Type == EObjectType.Human) || (rObject.Type == EObjectType.AI));

			//play the effect
			PlayParticleEffect(EDefaultParticleEffects.Death,
				Vector2.Zero,
				rObject.Position,
				rObject.PlayerColor);

			//TODO: Play death squish, and the players death sound
			//PlaySound(m_strDeathNoise);
			//PlaySound(rObject.DeathSound);

			//Do the correct score calculation
			rPlayerQueue.SubtractStock();

			if (!CheckIfPlayerStockOut(rPlayerQueue))
			{
				RespawnPlayer(rPlayerQueue);
			}
			else
			{
				rPlayerQueue.Character.Reset();
				rPlayerQueue.DeactivateAllObjects();
			}
		}

		private void RespawnPlayer(CPlayerQueue rPlayerQueue)
		{
			//respawn the player
			int iSpawnIndex = g_Random.Next(m_listSpawnPoints.Count);
			rPlayerQueue.Reset(m_listSpawnPoints[iSpawnIndex]);
		}

		public override void PlaySound(string strCueName)
		{
#if AUDIO
			CAudioManager.PlayCue(strCueName);
#endif
		}

		protected void StopTimers()
		{
			m_GameTimer.Stop();
			m_CharacterClock.Paused = true;
		}

		/// <summary>
		/// This function takes all the characters and rubber bands them so they cant just fly off into the sunset
		/// </summary>
		public void RubberBand()
		{
			//get the center point in between all the guys
			m_CenterPoint = Vector2.Zero;
			for (int i = 0; i < Players.Count; i++)
			{
				m_CenterPoint += Players[i].Character.Position;
			}
			m_CenterPoint /= Players.Count;

			//Check if any of the players are too far away
			for (int i = 0; i < Players.Count; i++)
			{
				Vector2 DistanceToCenter = m_CenterPoint - Players[i].Character.Position;
				if (DistanceToCenter.LengthSquared() > (3000.0f * 3000.0f))
				{
					//that dude is pretty far away from the action

					//get the amount of acceleration to add
					float fTEST = DistanceToCenter.Length();
					float fRatio = ((DistanceToCenter.Length() - 3000.0f) / 1800.0f) * Players[i].Character.CharacterClock.TimeDelta;

					//get the direction to send him
					DistanceToCenter.Normalize();

					//move the character towards the center of the screen
					Players[i].Character.Position += (fRatio * 1000.0f) * DistanceToCenter;

					//kill the characters velocity a little bit
					Vector2 myVelocity = Players[i].Character.Velocity;
					float fDotProduct = ((myVelocity.X * DistanceToCenter.X) + (myVelocity.Y * DistanceToCenter.Y));
					if (fDotProduct <= 0.0f)
					{
						Players[i].Character.Velocity *= 1.0f - fRatio;
					}
				}
			}
		}

		#region Draw

		public void UpdateDrawlists()
		{
			//update all the drawlists
			for (int i = 0; i < m_listPlayers.Count; i++)
			{
				m_listPlayers[i].UpdateDrawlists();
			}

			m_LevelObjects.UpdateDrawlists();
		}

		public override void Render()
		{
			//set up the camera
			if (m_bGameOver && !m_bTie)
			{
				//only show the winner!
				Debug.Assert(null != m_rWinner);
				m_rWinner.AddToCamera(Renderer.Camera);
			}
			else
			{
				for (int i = 0; i < m_listPlayers.Count; i++)
				{
					m_listPlayers[i].AddToCamera(Renderer.Camera);
				}
			}

			//draw the background before the camera is set
			DrawBackground();

			//Get the camera matrix we are gonna use
			Renderer.Camera.BeginScene(false);
			Matrix cameraMatrix = Renderer.Camera.TranslationMatrix * Resolution.TransformationMatrix();

			//draw the level
			Renderer.SpriteBatchBegin(BlendState.AlphaBlend, cameraMatrix);
			m_LevelObjects.Render(Renderer, true);
#if DEBUG
			//draw the world boundaries in debug mode?
			if (m_bRenderWorldBoundaries)
			{
				Renderer.DrawRectangle(
					new Vector2(WorldBoundaries.Left, WorldBoundaries.Top),
					new Vector2(WorldBoundaries.Right, WorldBoundaries.Bottom),
					0.0f, Color.Red, 1.0f);
			}

			//draw the spawn points for debug mode
			if (m_bRenderSpawnPoints)
			{
				for (int i = 0; i < m_listSpawnPoints.Count; i++)
				{
					Renderer.DrawCircle(m_listSpawnPoints[i], 10, Color.Red);
				}
			}
#endif
			Renderer.SpriteBatchEnd();

			//draw the hud
			Renderer.SpriteBatchBegin(BlendState.AlphaBlend, Resolution.TransformationMatrix());
			DrawHUD();
			Renderer.SpriteBatchEnd();

			//render all the character trails, start another spritebatch
			Renderer.SpriteBatchBegin(BlendState.NonPremultiplied, cameraMatrix);
			for (int i = 0; i < m_listPlayers.Count; i++)
			{
				m_listPlayers[i].Render(Renderer, false);
			}
			Renderer.SpriteBatchEnd();

			//render all the players
			Renderer.SpriteBatchBegin(BlendState.AlphaBlend, cameraMatrix);
			for (int i = 0; i < m_listPlayers.Count; i++)
			{
				m_listPlayers[i].Render(Renderer, true);

#if DEBUG
				//draw debug info?
				if (m_bRenderPhysics)
				{
					for (int j = 0; j < m_listPlayers[i].ActiveObjects.Count; j++)
					{
						m_listPlayers[i].ActiveObjects[j].AnimationContainer.Model.DrawPhysics(Renderer, true, Color.White);
					}
				}
#endif

				//draw the push box for each character?
				if (m_bRenderJointSkeleton)
				{
					for (int j = 0; j < m_listPlayers[i].ActiveObjects.Count; j++)
					{
						Renderer.DrawCircle(m_listPlayers[i].Character.Position,
							(int)(m_listPlayers[i].Character.MinDistance()),
							Color.White);
					}
				}

				////draw bones, ragdoll
				//m_listPlayers[i].Character.AnimationContainer.Model.RenderJointSkeleton(Renderer);
				//m_listPlayers[i].Character.AnimationContainer.Model.RenderOutline(Renderer, 1.0f);
				//m_listPlayers[i].Character.AnimationContainer.Model.DrawSkeleton(Renderer, true, Color.White);
				//m_listPlayers[i].Character.AnimationContainer.Model.DrawJoints(Renderer, true, Color.Red);
			}

			////TEST
			//Renderer.DrawCircle(m_CenterPoint, 56, Color.Black);

#if DEBUG
			if (m_bDrawCameraInfo)
			{
				for (int i = 0; i < m_listPlayers.Count; i++)
				{
					m_listPlayers[i].DrawCameraInfo(Renderer);
				}

				Renderer.Camera.DrawCameraInfo(Renderer);
			}
#endif
			Renderer.SpriteBatchEnd();

			//draw all the particles, start another spritebatch for the particles
			Renderer.SpriteBatchBegin(BlendState.NonPremultiplied, cameraMatrix);
			ParticleEngine.Render(Renderer);
			Renderer.SpriteBatchEnd();
		}

		protected virtual void DrawBackground()
		{
			Debug.Assert(null != m_SkyBox);
			Renderer.SpriteBatchBegin(BlendState.NonPremultiplied, Resolution.TransformationMatrix());
			if (m_iNumTiles <= 1)
			{
				//just cover the whole screen with the skybox
				Renderer.SpriteBatch.Draw(m_SkyBox, Resolution.ScreenArea, m_SkyColor);
			}
			else
			{
				//get the size of each tile
				int iTileSize = Resolution.ScreenArea.Width / m_iNumTiles;

				//get the number of rows 
				int iNumRows = Resolution.ScreenArea.Height / iTileSize;

				//display all the whole tiles
				Rectangle tileRect = new Rectangle();
				tileRect.X = 0;
				tileRect.Y = 0;
				tileRect.Height = iTileSize;
				tileRect.Width = iTileSize;
				for (int i = 0; i < iNumRows; i++)
				{
					for (int j = 0; j < m_iNumTiles; j++)
					{
						//draw one tile and move to the next column
						Renderer.SpriteBatch.Draw(m_SkyBox, tileRect, m_SkyColor);
						tileRect.X += iTileSize;
					}

					//reset the column to 0 and move to the next row
					tileRect.X = 0;
					tileRect.Y += iTileSize;
				}

				//Draw the bottom row, which is cut off :(
				Rectangle sourceRect = m_SkyBox.Bounds;
				tileRect.Height = Resolution.ScreenArea.Height - (iTileSize * iNumRows);
				sourceRect.Height = ((tileRect.Height * sourceRect.Height) / iTileSize);
				for (int i = 0; i < m_iNumTiles; i++)
				{
					Renderer.SpriteBatch.Draw(m_SkyBox, tileRect, sourceRect, m_SkyColor);
					tileRect.X += iTileSize;
				}
			}
			Renderer.SpriteBatchEnd();
		}

		public override void PlayParticleEffect(
			EDefaultParticleEffects eEffect,
			Vector2 Velocity,
			Vector2 Position,
			Color myColor)
		{
			ParticleEngine.PlayParticleEffect(m_DefaultParticles[(int)eEffect], Velocity, Position, Vector2.Zero, null, myColor, null);
		}

		protected virtual void DrawHUD()
		{
			//um, the width and height of the player pictures
			float fScreenHeight = Resolution.TitleSafeArea.Height;
			float fScreenWidth = Resolution.TitleSafeArea.Width;

			float fHeight = (fScreenHeight * 0.17f); //height of the protrait
			int iTop = (int)(Resolution.TitleSafeArea.Top * 1.05f); //y position of the top of the portrait
			if (0 == iTop)
			{
				iTop = (int)(fHeight * 0.1f);
			}
			int iBottom = iTop + (int)fHeight;
			float fCenterWidth = (fScreenWidth * 0.5f) + Resolution.TitleSafeArea.Left;

			//parse through the list of players
			for (int i = 0; i < m_listPlayers.Count; i++)
			{
				//Get the left point
				int iLeft = (int)(((fScreenWidth / (m_listPlayers.Count + 1)) * (i + 1)) - (fHeight * .5f));
				iLeft = Resolution.TitleSafeArea.X + iLeft;

				//draw that circle background 
				Color myColor = m_listPlayers[i].Character.PlayerColor;
				myColor.A = 200;

				Debug.Assert(null != Renderer);
				Debug.Assert(null != Renderer.SpriteBatch);
				Renderer.SpriteBatch.Draw(
					m_HUDBackground,
					new Rectangle(iLeft, iTop, (int)fHeight, (int)fHeight),
					myColor);

				//draw the players picture
				PlayerObject myPlayer = m_listPlayers[i].Character as PlayerObject;
				if (null != myPlayer &&
					null != myPlayer.Portrait)
				{
					Renderer.SpriteBatch.Draw(
						myPlayer.Portrait,
						new Rectangle(iLeft, iTop, (int)fHeight, (int)fHeight),
						Color.White);
				}

				int iCenter = (int)(iLeft + (fHeight * .5f));

				//draw the players health
				int iHealth = m_listPlayers[i].Character.DisplayHealth();
				if (CheckIfPlayerStockOut(m_listPlayers[i]))
				{
					iHealth = 0;
				}

				Color myDamageColor = Color.White;
				string strDamage = iHealth.ToString();
				if (iHealth < 25)
				{
					myDamageColor = Color.Red;
				}
				m_Font.Write(strDamage, new Vector2(iCenter, iBottom), Justify.Center, 1.15f, Color.DarkGray, Renderer.SpriteBatch);
				float fCursor = m_Font.Write(strDamage, new Vector2(iCenter, iBottom), Justify.Center, 1.0f, myDamageColor, Renderer.SpriteBatch);

				////draw the player's score

				////get the number to draw, either points or stock
				//int iPlayerScore = m_listPlayers[i].Points;
				//if (EGameMode.Stock == m_eGameMode)
				//{
				//    iPlayerScore = m_iStock - m_listPlayers[i].Stock;
				//}

				//Color ScoreColor = Color.White;
				//if (m_listPlayers[i].ScoreTimer.RemainingTime() > 0.0f)
				//{
				//    ScoreColor = Color.Red;
				//    m_Font.Write(iPlayerScore.ToString(), new Vector2(iLeft + fHeight, iBottom), Justify.Right, 1.8f, ScoreColor, Renderer.SpriteBatch);
				//}
				//else
				//{
				//    //just draw regular color
				//    m_Font.Write(iPlayerScore.ToString(), new Vector2(iLeft + fHeight, iBottom), Justify.Right, 1.65f, Color.DarkGray, Renderer.SpriteBatch);
				//    m_Font.Write(iPlayerScore.ToString(), new Vector2(iLeft + fHeight, iBottom), Justify.Right, 1.5f, ScoreColor, Renderer.SpriteBatch);
				//}

				//write the players name
				float fPortraitCenter = (fHeight * 0.5f) + iLeft;
				m_Font.Write(m_listPlayers[i].PlayerName, new Vector2(fPortraitCenter, iTop), Justify.Center, 0.7f, Color.White, Renderer.SpriteBatch);
			}

			//if the game mode is time, draw the clock
			Debug.Assert(MaxTime > 0.0f);
			if (m_GameTimer.RemainingTime() <= (MaxTime - 5.0f))
			{
				//check if the round is about to end
				Color TimeColor = new Color(0.35f, 0.35f, 0.35f, 0.1f);
				if (m_GameTimer.RemainingTime() <= 20.0f)
				{
					TimeColor = new Color(1.0f, 0.0f, 0.0f, .5f);
				}

				if (!m_bGameOver && (m_GameTimer.RemainingTime() > 0.0f))
				{
					Debug.Assert(null == m_rWinner);

					//draw the time
					float fPositionY = iBottom + (fHeight * 0.4f);
					string strTime = CStringUtils.TimeToString(m_GameTimer.RemainingTime());
					m_Font.Write(strTime, new Vector2(fCenterWidth, fPositionY), Justify.Center, 2.0f, TimeColor, Renderer.SpriteBatch);
				}
			}

			//if someone won, say who
			if (m_bGameOver)
			{
				float fCenterHeight = (fScreenHeight * 0.5f) + Resolution.TitleSafeArea.Top;
				if (!m_bTie && (null != m_rWinner))
				{
					Color myColor = m_rWinner.Character.PlayerColor;
					myColor.A = 100;
					string strMessage = m_rWinner.PlayerName + " WINS!";
					m_Font.Write(
						strMessage,
						new Vector2(fCenterWidth, fCenterHeight),
						Justify.Center,
						3.0f,
						myColor,
						Renderer.SpriteBatch);
				}
				else
				{
					//awww, it was a draw
					m_Font.Write(
						"DRAW GAME",
						new Vector2(fCenterWidth, fCenterHeight),
						Justify.Center,
						3.0f,
						new Color(0.65f, 0.65f, 0.65f, 0.65f),
						Renderer.SpriteBatch);
				}
			}

			//TEST
			//draw the contents of the first players input queue
			//Vector2 myDistance = m_CenterPoint - Players[0].Character.Position;

			//CPlayerObject myDude = Players[1].Character as CPlayerObject;
			//Write(myDude.ComboCounter.ToString(), new Vector2(200, 100));
		}

		/// <summary>
		/// write some text on the screen for debugging purposes
		/// </summary>
		/// <param name="strText">the text to write</param>
		/// <param name="position">where to write the text at</param>
		public void Write(string strText, Vector2 position)
		{
			m_Font.Write(strText, position, Justify.Left, 1.0f, Color.White, Renderer.SpriteBatch);
		}

		#endregion //Draw

		#endregion //Methods

		#region Networking

#if NETWORKING

		/// <summary>
		/// Read this object from a network packet reader.
		/// </summary>
		public void ReadFromNetwork(PacketReader packetReader)
		{
			//read in clocks
			MasterClock.ReadFromNetwork(packetReader);
			m_GameTimer.ReadFromNetwork(packetReader);
		}

		/// <summary>
		/// Write this object to a network packet reader.
		/// </summary>
		public void WriteToNetwork(PacketWriter packetWriter)
		{
			//write out clocks
			MasterClock.WriteToNetwork(packetWriter);
			m_GameTimer.WriteToNetwork(packetWriter);
		}

		/// <summary>
		/// Read the game over data from a network packet reader.
		/// </summary>
		public void ReadGameOver(PacketReader packetReader)
		{
			//read in game over shit
			m_bTie = packetReader.ReadBoolean();
			m_bGameOver = packetReader.ReadBoolean();
			string strWinner = packetReader.ReadString();

			if (m_bGameOver && !m_bTie)
			{
				//find that winner
				for (int i = 0; i < m_listPlayers.Count; i++)
				{
					if (m_listPlayers[i].PlayerName == strWinner)
					{
						m_rWinner = m_listPlayers[i];
						break;
					}
				}
			}
		}

		/// <summary>
		/// Write game over data to a network packet reader.
		/// </summary>
		public void WriteGameOver(PacketWriter packetWriter)
		{
			//write out game over shit
			packetWriter.Write(m_bTie);
			packetWriter.Write(m_bGameOver);

			if (m_bGameOver && (null != m_rWinner))
			{
				packetWriter.Write(m_rWinner.PlayerName);
			}
			else
			{
				packetWriter.Write("");
			}
		}

#endif

		#endregion //Networking
	}
}