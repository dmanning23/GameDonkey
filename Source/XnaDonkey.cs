using System;
using HadoukInput;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FontBuddyLib;
#if NETWORKING
using Microsoft.Xna.Framework.Net;
#endif
using RenderBuddy;
using ParticleBuddy;
using FilenameBuddy;
using ResolutionBuddy;
using GameTimer;
using System.IO;
using System.Xml;
using Vector2Extensions;

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
		GameClock m_CharacterClock;

		//Game over stuff!!!
		PlayerQueue m_rWinner;
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

		//public XNARenderer Renderer
		//{
		//	get
		//	{
		//		return m_Renderer;
		//	}
		//}

		#endregion //Properties

		#region Construction

		public XnaDonkey(XNARenderer rRenderer)
			: base()
		{
			m_Renderer = rRenderer;
			m_listPlayers = new List<PlayerQueue>();
			m_LevelObjects = new LevelObjectQueue();
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
					m_listPlayers[i].UpdateInput(rInput);
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

				//TODO: check if anyone has won
				//CheckForWinner();

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
				CollisionDetection();
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

			return m_bGameOver;
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
		protected virtual bool CheckIfPlayerStockOut(PlayerQueue rPlayerQueue)
		{
			return (0 >= rPlayerQueue.Stock);
		}

		/// <summary>
		/// Check if an object is dead (out of bounds) and process the death
		/// </summary>
		/// <param name="rObject">the object to check for death</param>
		/// <returns>whether or not the thing is dead</returns>
		private bool CheckIfDead(PlayerQueue rPlayerQueue)
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
		private void KillPlayer(PlayerQueue rPlayerQueue)
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

		private void RespawnPlayer(PlayerQueue rPlayerQueue)
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

		public override void PlayParticleEffect(
			EDefaultParticleEffects eEffect,
			Vector2 Velocity,
			Vector2 Position,
			Color myColor)
		{
			ParticleEngine.PlayParticleEffect(m_DefaultParticles[(int)eEffect], Velocity, Position, Vector2.Zero, null, myColor, false);
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

		/// <summary>
		/// update the camera before rendering
		/// </summary>
		public void UpdateCameraMatrix()
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

			RenderLevel(cameraMatrix);

			RenderHUD();

			RenderCharacterTrails(cameraMatrix);

			RenderCharacters(cameraMatrix);

			RenderParticleEffects(cameraMatrix);
		}

		protected virtual void RenderBackground()
		{
			Debug.Assert(null != m_SkyBox);
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

		protected virtual void RenderLevel(Matrix cameraMatrix)
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

		protected virtual void RenderHUD()
		{
			//draw the hud
			m_Renderer.SpriteBatchBegin(BlendState.AlphaBlend, Resolution.TransformationMatrix());

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
				m_Font.Write(strDamage, new Vector2(iCenter, iBottom), Justify.Center, 1.15f, Color.DarkGray, m_Renderer.SpriteBatch);
				float fCursor = m_Font.Write(strDamage, new Vector2(iCenter, iBottom), Justify.Center, 1.0f, myDamageColor, m_Renderer.SpriteBatch);

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
				m_Font.Write(m_listPlayers[i].PlayerName, new Vector2(fPortraitCenter, iTop), Justify.Center, 0.7f, Color.White, m_Renderer.SpriteBatch);
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
					string strTime = m_GameTimer.ToString();
					m_Font.Write(strTime, new Vector2(fCenterWidth, fPositionY), Justify.Center, 2.0f, TimeColor, m_Renderer.SpriteBatch);
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
						m_Renderer.SpriteBatch);
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
						m_Renderer.SpriteBatch);
				}
			}

			//TEST

			//CPlayerObject myDude = Players[1].Character as CPlayerObject;
			//Write(myDude.ComboCounter.ToString(), new Vector2(200, 100));

			m_Renderer.SpriteBatchEnd();
		}

		protected virtual void RenderCharacterTrails(Matrix cameraMatrix)
		{
			//render all the character trails, start another spritebatch
			m_Renderer.SpriteBatchBegin(BlendState.NonPremultiplied, cameraMatrix);
			for (int i = 0; i < m_listPlayers.Count; i++)
			{
				m_listPlayers[i].Render(Renderer, false);
			}
			m_Renderer.SpriteBatchEnd();
		}

		protected virtual void RenderCharacters(Matrix cameraMatrix)
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
#endif

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

		protected virtual void RenderParticleEffects(Matrix cameraMatrix)
		{
			//draw all the particles, start another spritebatch for the particles
			m_Renderer.SpriteBatchBegin(BlendState.NonPremultiplied, cameraMatrix);
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
			m_Font.Write(strText, position, Justify.Left, 1.0f, Color.White, m_Renderer.SpriteBatch);
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
			m_HUDBackground = (XNATexture)Renderer.LoadImage(@"HUDBackground.png");

			//load the hit spark
			HitSpark.ReadXmlFile(new Filename(@"Particles\Hit Spark.xml"), Renderer);

			//load the hit cloud
			HitCloud.ReadXmlFile(new Filename(@"Particles\Hit Cloud.xml"), Renderer);

			//load the death particle effect
			DeathParticles.ReadXmlFile(new Filename(@"Particles\Death Particles.xml"), Renderer);

			//load the block particle effect
			Block.ReadXmlFile(new Filename(@"Particles\Block.xml"), Renderer);

			//load the weapon hit particle effect
			WeaponHit.ReadXmlFile(new Filename(@"Particles\Weapon Hit.xml"), Renderer);

			//load the head bop particle effect
			HeadBop.ReadXmlFile(new Filename(@"Particles\ceiling bop.xml"), Renderer);

			//load the stunned bounce particle effect
			StunnedBounce.ReadXmlFile(new Filename(@"Particles\Stunned Bounce.xml"), Renderer);

			//load up our sprite font
			Debug.Assert(null != m_Font);
			m_Font.LoadContent(Renderer.Content, "Fonts\\ArialBlack24");
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

			////load the background image used for the HUD
			//m_HUDBackground = (XNATexture)Renderer.LoadImage(@"HUDBackground.png");

			////load the hit spark
			//HitSpark.ReadXmlFile(new Filename(@"Particles\Hit Spark.xml"), Renderer);

			////load the hit cloud
			//HitCloud.ReadXmlFile(new Filename(@"Particles\Hit Cloud.xml"), Renderer);

			////load the death particle effect
			//DeathParticles.ReadXmlFile(new Filename(@"Particles\Death Particles.xml"), Renderer);

			////load the block particle effect
			//Block.ReadXmlFile(new Filename(@"Particles\Block.xml"), Renderer);

			////load the weapon hit particle effect
			//WeaponHit.ReadXmlFile(new Filename(@"Particles\Weapon Hit.xml"), Renderer);

			////load the head bop particle effect
			//HeadBop.ReadXmlFile(new Filename(@"Particles\ceiling bop.xml"), Renderer);

			////load the stunned bounce particle effect
			//StunnedBounce.ReadXmlFile(new Filename(@"Particles\Stunned Bounce.xml"), Renderer);

			//load up our sprite font
			Debug.Assert(null != m_Font);
			m_Font.LoadContent(Renderer.Content, "ArialBlack24");
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
		public PlayerQueue LoadSerializedHumanPlayer(ContentManager rXmlContent,
			Color myColor,
			Filename strCharacterFile,
			PlayerIndex eIndex,
			string strPlayerName)
		{
			//create and load a player
			PlayerQueue rPlayer = CreatePlayerQueue(myColor, m_listPlayers.Count);
			if (null == rPlayer.LoadSerializedObject(rXmlContent, strCharacterFile, this, EObjectType.Human, 0))
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
			if (!rQueue.ReadSerializedFile(rXmlContent, new Filename(@"Move List.xml"), rPlayer.Character.States.GetMessageIndexFromText))
			{
				Debug.Assert(false);
			}
			rPlayer.InputQueue = rQueue;

			rPlayer.PlayerName = strPlayerName;
			return rPlayer;
		}

		/// <summary>
		/// load all a players data into the game
		/// </summary>
		/// <param name="myColor">color for this player</param>
		/// <param name="strCharacterFile">relative path for the player data file</param>
		/// <param name="eIndex">gamepad index for this player.</param>
		/// <param name="eType">the type of dude to load, accepts human and AI</param>
		/// <returns></returns>
		public virtual PlayerQueue LoadXmlHumanPlayer(Color myColor,
			Filename strCharacterFile,
			PlayerIndex eIndex,
			string strPlayerName)
		{
			//create and load a player
			PlayerQueue rPlayer = CreatePlayerQueue(myColor, m_listPlayers.Count);
			if (null == rPlayer.LoadXmlObject(strCharacterFile, this, EObjectType.Human, 0))
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

		public PlayerQueue LoadAiPlayer(ContentManager rXmlContent,
			Color myColor,
			Filename strCharacterFile,
			int iDifficulty,
			string strPlayerName)
		{
			//create and load a player
			PlayerQueue rPlayer = CreatePlayerQueue(myColor, m_listPlayers.Count);
			if (null == rPlayer.LoadSerializedObject(rXmlContent, strCharacterFile, this, EObjectType.AI, iDifficulty))
			{
				Debug.Assert(false);
			}
			m_listPlayers.Add(rPlayer);

			//create a controller for that player
			InputWrapper rQueue = new InputWrapper(null, MasterClock.GetCurrentTime)
			{
				BufferedInputExpire = 0.0f,
				QueuedInputExpire = 0.05f
			};
			if (!rQueue.ReadSerializedFile(rXmlContent, new Filename(@"Move List.xml"), rPlayer.Character.States.GetMessageIndexFromText))
			{
				Debug.Assert(false);
			}
			rPlayer.InputQueue = rQueue;

			rPlayer.PlayerName = strPlayerName;
			return rPlayer;
		}

		public void LoadSerializedBoard(ContentManager rXmlContent, Filename strBoardFile)
		{
			//load the resource
			SPFSettings.BoardXML myDude = rXmlContent.Load<SPFSettings.BoardXML>(strBoardFile.GetRelPathFileNoExt());

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
			m_SkyBox = (XNATexture)Renderer.LoadImage(myDude.backgroundTile);
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
				BaseObject rLevelObject = m_LevelObjects.LoadSerializedObject(rXmlContent, new Filename(myDude.objects[i]), this, EObjectType.Level, 0);
				if (null == rLevelObject)
				{
					Debug.Assert(false);
				}
			}

			m_LevelObjects.PlayerName = "Board";
		}

		public bool LoadXmlBoard(Filename strBoardFile)
		{
			//Open the file.
			FileStream stream = File.Open(strBoardFile.File, FileMode.Open, FileAccess.Read);
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.Load(stream);
			XmlNode rootNode = xmlDoc.DocumentElement;

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
				return false;
			}

			//next node is "<Asset Type="SPFSettings.LevelObjectXML">"
			XmlNode AssetNode = rootNode.FirstChild;
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

		//public int boardHeight = 0;
		//public int boardWidth = 0;
		//public string music = "";
		//public string deathNoise = "";
		//public string backgroundTile = "";
		//public byte backgroundR = 0;
		//public byte backgroundG = 0;
		//public byte backgroundB = 0;
		//public int numTiles = 0;
		//public List<string> objects = new List<string>();
		//public List<SpawnPointXML> spawnPoints = new List<SpawnPointXML>();

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

			//next node is the death noise
			childNode = childNode.NextSibling;
			m_strDeathNoise = childNode.InnerXml;
			
			////TODO: load the death noise
			//Debug.Assert(null != CAudioManager.GetCue(m_strDeathNoise));

			//next node is the background tile
			childNode = childNode.NextSibling;
			Filename backgroundFile = new Filename(childNode.InnerXml);
			m_SkyBox = (XNATexture)Renderer.LoadImage(backgroundFile.GetRelPathFileNoExt());

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