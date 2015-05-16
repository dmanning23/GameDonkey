using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Audio;
using HadoukInput;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FontBuddyLib;
using RenderBuddy;
using ParticleBuddy;
using FilenameBuddy;
using ResolutionBuddy;
using GameTimer;
using System.IO;
using System.Xml;
using Vector2Extensions;
#if OUYA
using Ouya.Console.Api;
#endif

namespace GameDonkey
{
	public class XnaDonkey : GameDonkeyBase
	{
		#region Members

		public const float RubberBandLength = 3000.0f;

		private XNARenderer m_Renderer;

		/// <summary>
		/// list of all the player objects in the game
		/// </summary>
		protected List<PlayerQueue> m_listPlayers;

		/// <summary>
		/// player queue for updating level objects
		/// </summary>
		protected PlayerQueue m_LevelObjects;

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

		private XNATexture m_SkyBox;
		private XNATexture m_HUDBackground;
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
		public GameClock CharacterClock { get; protected set; }

		/// <summary>
		/// the music resource for the current board
		/// </summary>
		string m_strMusicFile;

		/// <summary>
		/// The noise to play when a character falls off the board
		/// </summary>
		private string m_strDeathNoise;

		/// <summary>
		/// The velocity of the center point
		/// </summary>
		public Vector2 CenterVelocity { get; set; }

		/// <summary>
		/// a list of all the default particle effects used int he game
		/// </summary>
		private List<EmitterTemplate> m_DefaultParticles;

		#endregion //Members

		#region Properties

		public override IRenderer Renderer
		{
			get
			{
				return m_Renderer;
			}
		}

		public PlayerQueue Character
		{
			get { return m_listPlayers[0]; }
		}

		public List<PlayerQueue> Players
		{
			get { return m_listPlayers; }
		}

		public PlayerQueue LevelObjects
		{
			get { return m_LevelObjects; }
		}

		/// <summary>
		/// The max amount of time a game will last
		/// </summary>
		public float MaxTime { get; private set; }

		public CountdownTimer GameTimer
		{
			get { return m_GameTimer; }
		}

		public string Music
		{
			get { return m_strMusicFile; }
		}

		protected EmitterTemplate Block
		{
			get { return m_DefaultParticles[(int)EDefaultParticleEffects.Block]; }
		}

		protected EmitterTemplate HitSpark
		{
			get { return m_DefaultParticles[(int)EDefaultParticleEffects.HitSpark]; }
		}

		protected EmitterTemplate HitCloud
		{
			get { return m_DefaultParticles[(int)EDefaultParticleEffects.HitCloud]; }
		}

		protected EmitterTemplate StunnedBounce
		{
			get { return m_DefaultParticles[(int)EDefaultParticleEffects.StunnedBounce]; }
		}

		protected EmitterTemplate DeathParticles
		{
			get { return m_DefaultParticles[(int)EDefaultParticleEffects.Death]; }
		}

		protected EmitterTemplate HeadBop
		{
			get { return m_DefaultParticles[(int)EDefaultParticleEffects.HeadBop]; }
		}

		protected EmitterTemplate WeaponHit
		{
			get { return m_DefaultParticles[(int)EDefaultParticleEffects.WeaponHit]; }
		}

		//Game over stuff!!!
		public PlayerQueue Winner { get; protected set; }
		public bool Tie { get; protected set; }
		public bool GameOver { get; protected set; }

		/// <summary>
		/// dumb thing for loading sound effects.
		/// </summary>
		/// <value>The content of the sound.</value>
		private ContentManager SoundContent { get; set; }

		#endregion //Properties

		#region Construction

		public XnaDonkey(XNARenderer rRenderer, Game game)
			: base()
		{
			if (null != game)
			{
				SoundContent = new ContentManager(game.Services, "Content");
			}

			m_Renderer = rRenderer;
			m_listPlayers = new List<PlayerQueue>();
			m_LevelObjects = new LevelObjectQueue();
			m_listSpawnPoints = new List<Vector2>();

			m_DefaultParticles = new List<EmitterTemplate>();

			m_Font = new FontBuddy();
			CharacterClock = new GameClock();

			//debugging stuff
			m_bRenderJointSkeleton = false;
			m_bRenderPhysics = false;
			m_bDrawCameraInfo = false;
			m_bRenderWorldBoundaries = false;
			m_bRenderSpawnPoints = false;

			//game over stuff
			m_GameTimer = new CountdownTimer();
			MaxTime = 186.0f;
			Winner = null;
			GameOver = false;
			Tie = false;

			m_SkyBox = null;
			m_HUDBackground = null;
			m_SkyColor = Color.White;
			m_iNumTiles = 1;
		}

		#endregion //Construction

		#region Methods

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
			SetClockSpeed(1.0f);

			//reset the game timer
			m_GameTimer.Start(MaxTime);
			CharacterClock.Start();

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
			List<Task> tasks = new List<Task>();
			foreach (var player in m_listPlayers)
			{
				Debug.Assert(null != player);
				if (null != player.InputQueue)
				{
					tasks.Add(Task.Factory.StartNew(() => { player.UpdateInput(rInput); }));
				}
			}
			Task.WaitAll(tasks.ToArray());

			//for (int i = 0; i < m_listPlayers.Count; i++)
			//{
			//	Debug.Assert(null != m_listPlayers[i]);
			//	if (null != m_listPlayers[i].InputQueue)
			//	{
			//		m_listPlayers[i].UpdateInput(rInput);
			//	}
			//}
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
			CharacterClock.Update(m_GameTimer);

			//check for a winner
			if (!GameOver)
			{
//				//warn about time almost over?
//				if (EGameMode.Time == GameMode)
//				{
//					if ((m_GameTimer.RemainingTime() < 20.0f) && (fOldTime >= 20.0f))
//					{
//						PlaySound("twenty");
//					}
//				}

				//check if anyone has won
				CheckForWinner();

				//update the level objects
				m_LevelObjects.Update(m_GameTimer, true);

				UpdatePlayers();
			}

			//TODO: update animation with master clock if game is over

			if (!GameOver)
			{
				CollisionDetection();
			}

			UpdateRagdoll();

			UpdateDrawlists();

			//update the particle engine!!
			ParticleEngine.Update(MasterClock);

			//update everything else!
			UpdateStuff();

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

			return GameOver;
		}

		/// <summary>
		/// update all the player stuff
		/// </summary>
		private void UpdatePlayers()
		{
			List<Task> tasks = new List<Task>();
			foreach (var player in m_listPlayers)
			{
				Debug.Assert(null != player);
				tasks.Add(Task.Factory.StartNew(() => { UpdatePlayer(player); }));
			}
			Task.WaitAll(tasks.ToArray());
		}

		/// <summary>
		/// update a single player
		/// </summary>
		/// <param name="PlayerQueue"></param>
		private void UpdatePlayer(PlayerQueue playerQueue)
		{
			Debug.Assert(null != playerQueue);
			if (!CheckIfPlayerStockOut(playerQueue))
			{
				//check if the player is dead
				CheckIfDead(playerQueue);

				//update the characters
				playerQueue.Update(CharacterClock, true);
			}
		}

		/// <summary>
		/// update all the ragdoll stuff
		/// </summary>
		private void UpdateRagdoll()
		{
			List<Task> tasks = new List<Task>();
			foreach (var player in m_listPlayers)
			{
				tasks.Add(Task.Factory.StartNew(() => { player.UpdateRagdoll(false); }));
			}
			tasks.Add(Task.Factory.StartNew(() => { m_LevelObjects.UpdateRagdoll(false); }));
			Task.WaitAll(tasks.ToArray());
		}

		/// <summary>
		/// Update enything else in a child class
		/// </summary>
		protected virtual void UpdateStuff()
		{
		}

		/// <summary>
		/// check for collisions and respond!
		/// </summary>
		protected virtual void CollisionDetection()
		{
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
				GameOver = true;

				//find the winner!
				for (int i = 0; i < m_listPlayers.Count; i++)
				{
					if (!CheckIfPlayerStockOut(m_listPlayers[i]))
					{
						Winner = m_listPlayers[i];
					}
				}

				if (0 == iNumPlayers)
				{
					//all the players died the same exact frame
					Tie = true;
				}
			}
			else
			{
				CheckForTimeOver();
			}

			return GameOver;
		}

		/// <summary>
		/// Check if time has run out and determine a winner if it has.
		/// Called from the CheckForWinner method
		/// </summary>
		protected virtual void CheckForTimeOver()
		{
			//check for time over
			Debug.Assert(0.0f < MaxTime);
			if ((null == Winner) && (m_GameTimer.RemainingTime() <= 0.0f))
			{
				//find winner
				int iCurrentMaxStock = 0;
				for (int i = 0; i < m_listPlayers.Count; i++)
				{
					if (m_listPlayers[i].Stock >= iCurrentMaxStock)
					{
						//found someone with the max points, but is it a tie?
						if ((Winner != null) && (Winner != m_listPlayers[i]))
						{
							if (Winner.Stock == m_listPlayers[i].Stock)
							{
								Tie = true;
							}
						}

						//TODO: whenever a winner is found, set their animation to the win animation

						Winner = m_listPlayers[i];
						iCurrentMaxStock = Winner.Stock;
					}
				}

				StopTimers();
				GameOver = true;
			}
		}

		/// <summary>
		/// Check if a player has run out of stock
		/// </summary>
		/// <param name="rPlayerQueue">the player queue to check</param>
		/// <returns>true if the player has run out of stock</returns>
		protected virtual bool CheckIfPlayerStockOut(PlayerQueue rPlayerQueue)
		{
			return (0 >= rPlayerQueue.Stock);
		}

		/// <summary>
		/// Check if an object is dead (out of bounds) and process the death
		/// </summary>
		/// <param name="rObject">the object to check for death</param>
		private void CheckIfDead(PlayerQueue rPlayerQueue)
		{
			Debug.Assert(null != rPlayerQueue);
			if (rPlayerQueue.CheckIfDead())
			{
				KillPlayer(rPlayerQueue);
			}
		}

		/// <summary>
		/// Play the death particle effect
		///	Respawn the player
		/// Do the correct score calculation
		/// Play the players death sound
		/// </summary>
		/// <param name="rPlayerQueue">the player to kill</param>
		protected virtual void KillPlayer(PlayerQueue rPlayerQueue)
		{
			Debug.Assert(m_listSpawnPoints.Count > 0);
			Debug.Assert(null != rPlayerQueue);

			BaseObject rObject = rPlayerQueue.Character;
			Debug.Assert(null != rObject);
			Debug.Assert((rObject.Type == EObjectType.Human) || (rObject.Type == EObjectType.AI));

			//TODO: play the death particle effect
			//PlayParticleEffect(EDefaultParticleEffects.Death,
			//	Vector2.Zero,
			//	rObject.Position,
			//	rObject.PlayerColor);

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

		private void RespawnPlayer(PlayerQueue rPlayerQueue)
		{
			//respawn the player
			int iSpawnIndex = g_Random.Next(m_listSpawnPoints.Count);
			rPlayerQueue.Reset(m_listSpawnPoints[iSpawnIndex]);
		}

		protected void StopTimers()
		{
			m_GameTimer.Stop();
			CharacterClock.Paused = true;
		}

		public override void PlayParticleEffect(
			EDefaultParticleEffects eEffect,
			Vector2 velocity,
			Vector2 position,
			Color myColor)
		{
			ParticleEngine.PlayParticleEffect(m_DefaultParticles[(int)eEffect], velocity, position, Vector2.Zero, myColor, false);
		}

		public override SoundEffect LoadSound(Filename cueName)
		{
			if (SoundContent != null)
			{
				return SoundContent.Load<SoundEffect>(cueName.GetRelPathFileNoExt());
			}
			else
			{
				return null;
			}
		}

		#region Draw

		/// <summary>
		/// update all the drawlists
		/// </summary>
		public void UpdateDrawlists()
		{
			List<Task> tasks = new List<Task>();
			foreach (var player in m_listPlayers)
			{
				tasks.Add(Task.Factory.StartNew(() => { player.UpdateDrawlists(); }));
			}
			tasks.Add(Task.Factory.StartNew(() => { m_LevelObjects.UpdateDrawlists(); }));
			Task.WaitAll(tasks.ToArray());
		}

		/// <summary>
		/// update the camera before rendering
		/// </summary>
		public void UpdateCameraMatrix()
		{
			//set up the camera
			if (GameOver && !Tie)
			{
				//only show the winner!
				Debug.Assert(null != Winner);
				Winner.AddToCamera(Renderer.Camera);
			}
			else
			{
				for (int i = 0; i < m_listPlayers.Count; i++)
				{
					m_listPlayers[i].AddToCamera(Renderer.Camera);
				}
			}

			//draw the background before the camera is set
			//DrawBackground();

			//Get the camera matrix we are gonna use
			Renderer.Camera.BeginScene(false);
		}

		/// <summary>
		/// get the gameplay matrix
		/// </summary>
		/// <returns></returns>
		public Matrix GetCameraMatrix()
		{
			return Renderer.Camera.TranslationMatrix * Resolution.TransformationMatrix();
		}

		public override void Render()
		{
			Matrix cameraMatrix = GetCameraMatrix();

			RenderBackground();

			RenderLevel(cameraMatrix);

			RenderHUD();

			RenderCharacterTrails(cameraMatrix);

			RenderCharacters(cameraMatrix);

			RenderParticleEffects(cameraMatrix);
		}

		protected virtual void RenderBackground()
		{
			//Check if there is any background to draw
			if (null == m_SkyBox)
			{
				return;
			}

			m_Renderer.SpriteBatchBegin(BlendState.NonPremultiplied, Resolution.TransformationMatrix());
			if (m_iNumTiles <= 1)
			{
				//just cover the whole screen with the skybox
				m_Renderer.SpriteBatch.Draw(m_SkyBox.Texture, Resolution.ScreenArea, m_SkyColor);
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
						m_Renderer.SpriteBatch.Draw(m_SkyBox.Texture, tileRect, m_SkyColor);
						tileRect.X += iTileSize;
					}

					//reset the column to 0 and move to the next row
					tileRect.X = 0;
					tileRect.Y += iTileSize;
				}

				//Draw the bottom row, which is cut off :(
				Rectangle sourceRect = m_SkyBox.Texture.Bounds;
				tileRect.Height = Resolution.ScreenArea.Height - (iTileSize * iNumRows);
				sourceRect.Height = ((tileRect.Height * sourceRect.Height) / iTileSize);
				for (int i = 0; i < m_iNumTiles; i++)
				{
					m_Renderer.SpriteBatch.Draw(m_SkyBox.Texture, tileRect, sourceRect, m_SkyColor);
					tileRect.X += iTileSize;
				}
			}
			m_Renderer.SpriteBatchEnd();
		}

		protected override void RenderLevel(Matrix cameraMatrix)
		{
			//draw the level
			m_Renderer.SpriteBatchBegin(BlendState.AlphaBlend, cameraMatrix);
			m_LevelObjects.Render(Renderer, true);
#if DEBUG
			//draw the world boundaries in debug mode?
			if (m_bRenderWorldBoundaries)
			{
				Renderer.Primitive.Rectangle(WorldBoundaries, Color.Red);
			}

			//draw the spawn points for debug mode
			if (m_bRenderSpawnPoints)
			{
				for (int i = 0; i < m_listSpawnPoints.Count; i++)
				{
					Renderer.Primitive.Circle(m_listSpawnPoints[i], 10, Color.Red);
				}
			}
#endif
			m_Renderer.SpriteBatchEnd();
		}

		/// <summary>
		/// draw the hud
		/// </summary>
		protected virtual void RenderHUD()
		{
			//draw the hud
			m_Renderer.SpriteBatchBegin(BlendState.AlphaBlend, Resolution.TransformationMatrix());

			RenderPlayerHUD();

			RenderClockHUD();

			//TEST

			//CPlayerObject myDude = Players[1].Character as CPlayerObject;
			//Write(myDude.ComboCounter.ToString(), new Vector2(200, 100));

			m_Renderer.SpriteBatchEnd();
		}

		protected void RenderPlayerHUD()
		{
			float fHeight;
			int iTop;
			int iBottom;
			float fScreenHeight;
			float fScreenWidth;
			float fCenterWidth;
			GetScreenHUD(out fHeight, out iTop, out iBottom, out fScreenHeight, out fScreenWidth, out fCenterWidth);

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
				Debug.Assert(null != m_Renderer.SpriteBatch);
				if (null != m_HUDBackground)
				{
					m_Renderer.SpriteBatch.Draw(
						m_HUDBackground.Texture,
						new Rectangle(iLeft, iTop, (int)fHeight, (int)fHeight),
						myColor);
				}

				//draw the players picture
				PlayerObject myPlayer = m_listPlayers[i].Character as PlayerObject;
				if (null != myPlayer &&
					null != myPlayer.Portrait)
				{
					m_Renderer.SpriteBatch.Draw(
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
				m_Font.Write(strDamage, new Vector2(iCenter, iBottom), Justify.Center, 1.15f, Color.DarkGray, m_Renderer.SpriteBatch, m_GameTimer);
				float fCursor = m_Font.Write(strDamage, new Vector2(iCenter, iBottom), Justify.Center, 1.0f, myDamageColor, m_Renderer.SpriteBatch, m_GameTimer);

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
				m_Font.Write(m_listPlayers[i].PlayerName, new Vector2(fPortraitCenter, iTop), Justify.Center, 0.7f, Color.White, m_Renderer.SpriteBatch, m_GameTimer);
			}
		}

		protected void RenderClockHUD()
		{
			float fHeight;
			int iTop;
			int iBottom;
			float fScreenHeight;
			float fScreenWidth;
			float fCenterWidth;
			GetScreenHUD(out fHeight, out iTop, out iBottom, out fScreenHeight, out fScreenWidth, out fCenterWidth);

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

				if (!GameOver && (m_GameTimer.RemainingTime() > 0.0f))
				{
					Debug.Assert(null == Winner);

					//draw the time
					float fPositionY = iBottom + (fHeight * 0.4f);
					string strTime = m_GameTimer.ToString();
					m_Font.Write(strTime, new Vector2(fCenterWidth, fPositionY), Justify.Center, 2.0f, TimeColor, m_Renderer.SpriteBatch, m_GameTimer);
				}
			}
		}

		protected override void RenderCharacterTrails(Matrix cameraMatrix)
		{
			//render all the character trails, start another spritebatch
			m_Renderer.SpriteBatchBegin(BlendState.NonPremultiplied, cameraMatrix);
			for (int i = 0; i < m_listPlayers.Count; i++)
			{
				m_listPlayers[i].Render(Renderer, false);
			}
			m_Renderer.SpriteBatchEnd();
		}

		public static void GetScreenHUD(out float fHeight, out int iTop, out int iBottom, out float fScreenHeight, out float fScreenWidth, out float fCenterWidth)
		{
			//um, the width and height of the player pictures
			fScreenHeight = Resolution.TitleSafeArea.Height;
			fScreenWidth = Resolution.TitleSafeArea.Width;

			fHeight = (fScreenHeight * 0.17f);
			iTop = (int)(Resolution.TitleSafeArea.Top * 1.05f);
			if (0 == iTop)
			{
				iTop = (int)(fHeight * 0.1f);
			}
			iBottom = iTop + (int)fHeight;
			fCenterWidth = (fScreenWidth * 0.5f) + Resolution.TitleSafeArea.Left;
		}

		protected override void RenderCharacters(Matrix cameraMatrix)
		{
			//render all the players
			m_Renderer.SpriteBatchBegin(BlendState.AlphaBlend, cameraMatrix);
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

				//draw the push box for each character?
				if (m_bRenderJointSkeleton)
				{
					for (int j = 0; j < m_listPlayers[i].ActiveObjects.Count; j++)
					{
						Renderer.Primitive.Circle(m_listPlayers[i].Character.Position,
												  (int)(m_listPlayers[i].Character.MinDistance()),
												  Color.White);
					}
				}
#endif

				////draw bones, ragdoll
				//m_listPlayers[i].Character.AnimationContainer.Model.RenderJointSkeleton(Renderer);
				//m_listPlayers[i].Character.AnimationContainer.Model.RenderOutline(Renderer, 1.0f);
				//m_listPlayers[i].Character.AnimationContainer.Model.DrawSkeleton(Renderer, true, Color.White);
				//m_listPlayers[i].Character.AnimationContainer.Model.DrawJoints(Renderer, true, Color.Red);
			}

#if DEBUG
			if (m_bDrawCameraInfo)
			{
				for (int i = 0; i < m_listPlayers.Count; i++)
				{
					m_listPlayers[i].DrawCameraInfo(Renderer);
				}

				Renderer.DrawCameraInfo();
			}
#endif
			m_Renderer.SpriteBatchEnd();
		}

		protected override void RenderParticleEffects(Matrix cameraMatrix)
		{
			//draw all the particles, start another spritebatch for the particles
			m_Renderer.SpriteBatchBegin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, cameraMatrix);
			ParticleEngine.Render(Renderer);
			m_Renderer.SpriteBatchEnd();
		}

		/// <summary>
		/// write some text on the screen for debugging purposes
		/// </summary>
		/// <param name="strText">the text to write</param>
		/// <param name="position">where to write the text at</param>
		public void Write(string strText, Vector2 position)
		{
			m_Font.Write(strText, position, Justify.Left, 1.0f, Color.White, m_Renderer.SpriteBatch, m_GameTimer);
		}

		#endregion //Draw

		#endregion //Methods

		#region File IO

		/// <summary>
		/// load all the content in an xna game
		/// </summary>
		/// <param name="rContent">content manager</param>
		public override void LoadSerializedContent(ContentManager rXmlContent, GraphicsDevice rGraphics)
		{
			m_LastKeyboardState = Keyboard.GetState();

			//load all the content

			//load up the renderer graphics content, so we can use its conent manager to load all our graphics
			m_Renderer.LoadContent(rGraphics);

			//load the background image used for the HUD
			m_HUDBackground = (XNATexture)Renderer.LoadImage(new Filename(@"HUDBackground.png"));

			//Load all teh default particle effects
			AddParticleEffect(@"Particles\Hit Cloud.xml");
			AddParticleEffect(@"Particles\Hit Spark.xml");
			AddParticleEffect(@"Particles\Death Particles.xml");
			AddParticleEffect(@"Particles\Block.xml");
			AddParticleEffect(@"Particles\Weapon Hit.xml");
			AddParticleEffect(@"Particles\Stunned Bounce.xml");
			AddParticleEffect(@"Particles\ceiling bop.xml");

			//load up our sprite font
			Debug.Assert(null != m_Font);
			m_Font.LoadContent(Renderer.Content, "Fonts\\ArialBlack24");
		}

		private void AddParticleEffect(string file)
		{
			var emitter = new EmitterTemplate(new Filename(file));
			emitter.ReadXmlFile();
			emitter.LoadContent(Renderer);
			m_DefaultParticles.Add(emitter);
		}

		/// <summary>
		/// load all the content in an xna game
		/// </summary>
		/// <param name="rContent">content manager</param>
		public override void LoadXmlContent(GraphicsDevice rGraphics)
		{
			m_LastKeyboardState = Keyboard.GetState();

			//load all the content

			//load up the renderer graphics content, so we can use its conent manager to load all our graphics
			m_Renderer.LoadContent(rGraphics);

			//load up our sprite font
			Debug.Assert(null != m_Font);
			m_Font.LoadContent(Renderer.Content, "Fonts\\ArialBlack24");
		}

		public virtual PlayerQueue LoadPlayer(Color myColor,
			Filename strCharacterFile,
			PlayerIndex eIndex,
			string strPlayerName,
			EObjectType playerType = EObjectType.Human)
		{
			//create and load a player
			PlayerQueue rPlayer = CreatePlayerQueue(myColor, m_listPlayers.Count);
			if (null == rPlayer.LoadXmlObject(strCharacterFile, this, playerType, 0))
			{
				Debug.Assert(false);
			}
			m_listPlayers.Add(rPlayer);

			//create a controller for that player
			InputWrapper rQueue = new InputWrapper(new ControllerWrapper(eIndex, (PlayerIndex.One == eIndex)), MasterClock.GetCurrentTime)
			{
				BufferedInputExpire = 0.0f,
				QueuedInputExpire = 0.05f
			};
			if (!rQueue.ReadXmlFile(new Filename(@"MoveList.xml"), rPlayer.Character.States.GetMessageIndexFromText))
			{
				Debug.Assert(false);
			}
			rPlayer.InputQueue = rQueue;

			//if this is player one, let them use the keyboard
			rPlayer.InputQueue.Controller.UseKeyboard = (PlayerIndex.One == eIndex);

			rPlayer.PlayerName = strPlayerName;
			return rPlayer;
		}

		public bool LoadBoard(Filename strBoardFile)
		{
			//Open the file.
			#if ANDROID
			Stream stream = Game.Activity.Assets.Open(strBoardFile.File);
			#else
			FileStream stream = File.Open(strBoardFile.File, FileMode.Open, FileAccess.Read);
			#endif
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.Load(stream);
			XmlNode rootNode = xmlDoc.DocumentElement;

#if DEBUG
			//make sure it is actually an xml node
			if (rootNode.NodeType != XmlNodeType.Element)
			{
				//should be an xml node!!!
				Debug.Assert(false);
				return false;
			}

			//eat up the name of that xml node
			string strElementName = rootNode.Name;
			if (("XnaContent" != strElementName) || !rootNode.HasChildNodes)
			{
				Debug.Assert(false);
				return false;
			}
#endif
			//next node is "<Asset Type="SPFSettings.LevelObjectXML">"
			XmlNode AssetNode = rootNode.FirstChild;
#if DEBUG
			if (null == AssetNode)
			{
				Debug.Assert(false);
				return false;
			}
			if (!AssetNode.HasChildNodes)
			{
				Debug.Assert(false);
				return false;
			}
			if ("Asset" != AssetNode.Name)
			{
				Debug.Assert(false);
				return false;
			}
#endif

			//First node is the name
			XmlNode childNode = AssetNode.FirstChild;
			m_LevelObjects.PlayerName = childNode.InnerXml;

			//next node is the height
			childNode = childNode.NextSibling;
			int iHeight = Convert.ToInt32(childNode.InnerXml);

			//nect node is the width
			childNode = childNode.NextSibling;
			int iWidth = Convert.ToInt32(childNode.InnerXml);

			////grab the world boundaries
			WorldBoundaries = new Rectangle((-1 * (iWidth / 2)),
				(-1 * (iHeight / 2)),
				iWidth,
				iHeight);

			//next node is the music
			childNode = childNode.NextSibling;
			m_strMusicFile = childNode.InnerXml;
			if (!string.IsNullOrEmpty(m_strMusicFile))
			{
				//TODO: load the music
			}

			//next node is the death noise
			childNode = childNode.NextSibling;
			m_strDeathNoise = childNode.InnerXml;
			if (!string.IsNullOrEmpty(m_strMusicFile))
			{
				//TODO: load the death noise
			}

			//next node is the background tile
			childNode = childNode.NextSibling;
			if (!string.IsNullOrEmpty(childNode.InnerXml))
			{
				Filename backgroundFile = new Filename(childNode.InnerXml);
				m_SkyBox = (XNATexture)Renderer.LoadImage(backgroundFile);
			}

			//load the color!
			childNode = childNode.NextSibling;
			m_SkyColor.R = Convert.ToByte(childNode.InnerXml);
			childNode = childNode.NextSibling;
			m_SkyColor.G = Convert.ToByte(childNode.InnerXml);
			childNode = childNode.NextSibling;
			m_SkyColor.B = Convert.ToByte(childNode.InnerXml);
			m_SkyColor.A = 255;

			//next node is the number of tiles
			childNode = childNode.NextSibling;
			m_iNumTiles = Convert.ToInt32(childNode.InnerXml);

			//load all the level objects
			childNode = childNode.NextSibling;
			for (XmlNode levelNode = childNode.FirstChild;
				null != levelNode;
				levelNode = levelNode.NextSibling)
			{
				//load the level object
				Filename myLevelObjectFile = new Filename(levelNode.InnerXml);
				BaseObject rLevelObject = m_LevelObjects.LoadXmlObject(myLevelObjectFile, this, EObjectType.Level, 0);
				if (null == rLevelObject)
				{
					Debug.Assert(false);
				}
			}

			//spawn points
			childNode = childNode.NextSibling;
			for (XmlNode spawnNode = childNode.FirstChild;
				null != spawnNode;
				spawnNode = spawnNode.NextSibling)
			{
				Debug.Assert(spawnNode.HasChildNodes);
				XmlNode locationNode = spawnNode.FirstChild;
				m_listSpawnPoints.Add(Vector2Ext.ToVector2(locationNode.InnerXml));
			}

			// Close the file.
			stream.Close();
			return true;
		}

		#endregion //File IO
	}
}