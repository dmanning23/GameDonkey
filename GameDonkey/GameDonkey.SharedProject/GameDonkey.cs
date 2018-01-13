using FilenameBuddy;
using FontBuddyLib;
using GameTimer;
using HadoukInput;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ParticleBuddy;
using RenderBuddy;
using ResolutionBuddy;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using Vector2Extensions;

namespace GameDonkeyLib
{
	public class GameDonkey : IGameDonkey
	{
		#region Members

		static protected Random _random = new Random(DateTime.Now.Millisecond);

		//debugging flags
		protected KeyboardState _lastKeyboardState;

		//debug shit
		protected bool _renderJointSkeleton;
		protected bool _renderPhysics;
		protected bool _drawCameraInfo;
		protected bool _renderWorldBoundaries;
		protected bool _renderSpawnPoints;

		#endregion //Members

		#region Properties

		public IRenderer Renderer { get; private set; }

		public ParticleEngine ParticleEngine { get; protected set; }

		public GameClock MasterClock { get; protected set; }

		/// <summary>
		/// the world boundaries
		/// </summary>
		private Rectangle _worldBoundaries;
		public Rectangle WorldBoundaries
		{
			get { return _worldBoundaries; }
			set
			{
				_worldBoundaries = value;

				//make the camera rect a little bit smaller so we can see more of the ground
				Renderer.Camera.WorldBoundary = new Rectangle(_worldBoundaries.X, _worldBoundaries.Y, _worldBoundaries.Width, _worldBoundaries.Height + 100);
			}
		}

		public PlayerQueue Character
		{
			get { return Players[0]; }
		}

		/// <summary>
		/// list of all the player objects in the game
		/// </summary>
		public List<PlayerQueue> Players { get; private set; }

		/// <summary>
		/// player queue for updating level objects
		/// </summary>
		public PlayerQueue LevelObjects { get; private set; }

		/// <summary>
		/// the spawn points for characters
		/// </summary>
		public List<Vector2> SpawnPoints { get; set; }

		/// <summary>
		/// The max amount of time a game will last
		/// </summary>
		public float MaxTime { get; private set; }

		/// <summary>
		/// Timer used to countdown to end of game
		/// </summary>
		public CountdownTimer GameTimer { get; private set; }

		/// <summary>
		/// Clock used to update all the characters & board objects
		/// </summary>
		public GameClock CharacterClock { get; protected set; }

		/// <summary>
		/// the music resource for the current board
		/// </summary>
		public string Music { get; private set; }

		protected EmitterTemplate Block
		{
			get { return DefaultParticles[(int)EDefaultParticleEffects.Block]; }
		}

		protected EmitterTemplate HitSpark
		{
			get { return DefaultParticles[(int)EDefaultParticleEffects.HitSpark]; }
		}

		protected EmitterTemplate HitCloud
		{
			get { return DefaultParticles[(int)EDefaultParticleEffects.HitCloud]; }
		}

		protected EmitterTemplate StunnedBounce
		{
			get { return DefaultParticles[(int)EDefaultParticleEffects.StunnedBounce]; }
		}

		protected EmitterTemplate DeathParticles
		{
			get { return DefaultParticles[(int)EDefaultParticleEffects.Death]; }
		}

		protected EmitterTemplate HeadBop
		{
			get { return DefaultParticles[(int)EDefaultParticleEffects.HeadBop]; }
		}

		protected EmitterTemplate WeaponHit
		{
			get { return DefaultParticles[(int)EDefaultParticleEffects.WeaponHit]; }
		}

		//Game over stuff!!!
		public PlayerQueue Winner { get; protected set; }
		public bool Tie { get; protected set; }
		public bool GameOver { get; protected set; }

		/// <summary>
		/// dumb thing for loading sound effects.
		/// </summary>
		/// <value>The content of the sound.</value>
		public ContentManager ContentManager { get; private set; }

		/// <summary>
		/// The noise to play when a character falls off the board
		/// </summary>
		private string DeathNoise { get; set; }

		/// <summary>
		/// The velocity of the center point
		/// </summary>
		public Vector2 CenterVelocity { get; set; }

		/// <summary>
		/// a list of all the default particle effects used int he game
		/// </summary>
		private List<EmitterTemplate> DefaultParticles { get; set; }

		#endregion //Properties

		#region Construction

		public GameDonkey(IRenderer renderer, Game game)
			: base()
		{
			ParticleEngine = new ParticleEngine();
			MasterClock = new GameClock();

			if (null != game)
			{
				ContentManager = new ContentManager(game.Services, "Content");
			}

			Renderer = renderer;
			Players = new List<PlayerQueue>();
			LevelObjects = new LevelObjectQueue();
			SpawnPoints = new List<Vector2>();

			DefaultParticles = new List<EmitterTemplate>();

			CharacterClock = new GameClock();

			//debugging stuff
			_renderJointSkeleton = false;
			_renderPhysics = false;
			_drawCameraInfo = false;
			_renderWorldBoundaries = false;
			_renderSpawnPoints = false;

			//game over stuff
			GameTimer = new CountdownTimer();
			MaxTime = 186.0f;
			Winner = null;
			GameOver = false;
			Tie = false;
		}

		/// <summary>
		/// factory method
		/// </summary>
		/// <param name="myColor"></param>
		/// <param name="iQueueID"></param>
		/// <returns></returns>
		public virtual PlayerQueue CreatePlayerQueue(Color color, int queueID)
		{
			return new PlayerQueue(color, queueID);
		}

		/// <summary>
		/// load all the content in a windows forms game
		/// </summary>
		public virtual void LoadContent(GraphicsDevice graphics)
		{
			WorldBoundaries = new Rectangle();

			//load up the renderer graphics content, so we can use its conent manager to load all our graphics
			Renderer.LoadContent(graphics);
		}

		public virtual void UnloadContent()
		{
			ContentManager.Dispose();
			ContentManager = null;
		}

		public virtual void Start()
		{
			_lastKeyboardState = Keyboard.GetState();

			MasterClock.Start();
			MasterClock.TimeDelta = 0.0f;

			//speed up the character clock
			SetClockSpeed(1.0f);

			//reset the game timer
			GameTimer.Start(MaxTime);
			CharacterClock.Start();

			//reset teh level objects
			LevelObjects.Reset();

			//reset the players
			Debug.Assert(SpawnPoints.Count > 0);
			int spawnIndex = 0;
			for (int i = 0; i < Players.Count; i++)
			{
				Debug.Assert(null != Players[i]);

				if (null != Players[i].InputQueue)
				{
					Debug.Assert(null != Players[i].InputQueue.Controller);
					Players[i].InputQueue.Controller.ResetController();
				}
				Players[i].Reset(SpawnPoints[spawnIndex]);

				//increment to the next spawn point
				if (spawnIndex < (SpawnPoints.Count - 1))
				{
					++spawnIndex;
				}
				else
				{
					spawnIndex = 0;
				}
			}

			//force the camera to fit the whole scene
			for (int i = 0; i < Players.Count; i++)
			{
				Players[i].AddToCamera(Renderer.Camera);
			}

			Renderer.Camera.ForceToScreen();
		}

		public SoundEffect LoadSound(Filename cueName)
		{
			if (ContentManager != null)
			{
				return ContentManager.Load<SoundEffect>(cueName.GetRelPathFileNoExt());
			}
			else
			{
				return null;
			}
		}

		#endregion //Construction

		#region Methods

		public virtual void AddCameraShake(float shakeAmount)
		{
			Renderer.Camera.AddCameraShake(shakeAmount);
		}

		/// <summary>
		/// Change the speed of the character clock
		/// </summary>
		/// <param name="fSpeed">multiplier to speed up/slow down the character clock</param>
		public void SetClockSpeed(float speed)
		{
			CharacterClock.TimerSpeed = speed;
		}

		/// <summary>
		/// this is a hack put in just for local testing.
		/// In the real game, each individual player queue will be updated in the network game loop.
		/// </summary>
		/// <param name="rInput"></param>
		public void UpdateInput(InputState input)
		{
			var tasks = new List<Task>();
			foreach (var player in Players)
			{
				Debug.Assert(null != player);
				if (null != player.InputQueue)
				{
					tasks.Add(Task.Factory.StartNew(() => { player.UpdateInput(input); }));
				}
			}
			Task.WaitAll(tasks.ToArray());
		}

		/// <summary>
		/// update the game engine.  
		/// This function is overridden in child classes.  
		/// The server class checks for game over and updates the client class
		/// The client class will not check for game over.
		/// </summary>
		/// <returns>bool: whether or not this update resulted in a game over situation</returns>
		protected bool Update()
		{
			//update the camera stuff
			Renderer.Camera.Update(MasterClock);

			//update all our clocks
			GameTimer.Update(MasterClock);
			CharacterClock.Update(GameTimer);

			//check for a winner
			if (!GameOver)
			{
//				//warn about time almost over?
//				if (EGameMode.Time == GameMode)
//				{
//					if ((GameTimer.RemainingTime() < 20.0f) && (fOldTime >= 20.0f))
//					{
//						PlaySound("twenty");
//					}
//				}

				//check if anyone has won
				CheckForWinner();

				//update the level objects
				LevelObjects.Update(GameTimer);

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
			if (currentState.IsKeyDown(Keys.Y) && _lastKeyboardState.IsKeyUp(Keys.Y))
			{
				_renderSpawnPoints = !_renderSpawnPoints;
			}
			if (currentState.IsKeyDown(Keys.U) && _lastKeyboardState.IsKeyUp(Keys.U))
			{
				_renderJointSkeleton = !_renderJointSkeleton;
			}
			if (currentState.IsKeyDown(Keys.I) && _lastKeyboardState.IsKeyUp(Keys.I))
			{
				_renderPhysics = !_renderPhysics;
			}
			if (currentState.IsKeyDown(Keys.O) && _lastKeyboardState.IsKeyUp(Keys.O))
			{
				_drawCameraInfo = !_drawCameraInfo;
			}
			if (currentState.IsKeyDown(Keys.P) && _lastKeyboardState.IsKeyUp(Keys.P))
			{
				_renderWorldBoundaries = !_renderWorldBoundaries;
			}
#endif

			return GameOver;
		}

		/// <summary>
		/// update the game engine
		/// </summary>
		/// <param name="rGameTime">current gametime</param>
		/// <returns>bool: true if the game is over, false if it isn't</returns>
		public bool Update(GameTime time)
		{
			MasterClock.Update(time);
			return Update();
		}

		/// <summary>
		/// update the game engine
		/// </summary>
		/// <param name="rTime">current gametime</param>
		/// <returns>bool: true if the game is over, false if it isn't</returns>
		public bool Update(TimeUpdater time)
		{
			MasterClock.Update(time);
			return Update();
		}

		/// <summary>
		/// update all the player stuff
		/// </summary>
		private void UpdatePlayers()
		{
			List<Task> tasks = new List<Task>();
			foreach (var player in Players)
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
				playerQueue.Update(CharacterClock);
			}
		}

		/// <summary>
		/// update all the ragdoll stuff
		/// </summary>
		private void UpdateRagdoll()
		{
			List<Task> tasks = new List<Task>();
			foreach (var player in Players)
			{
				tasks.Add(Task.Factory.StartNew(() => { player.UpdateRagdoll(); }));
			}
			tasks.Add(Task.Factory.StartNew(() => { LevelObjects.UpdateRagdoll(); }));
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
			for (int i = 0; i < Players.Count; i++)
			{
				//check for collisions between players
				for (int j = i + 1; j < Players.Count; j++)
				{
					Players[i].CheckCollisions(Players[j]);
				}

				//check for collisions with level objects
				Players[i].CheckCollisions(LevelObjects);

				//check for world collisions
				Players[i].CheckWorldCollisions(WorldBoundaries);
			}

			//respond to hits!
			for (int i = 0; i < Players.Count; i++)
			{
				Players[i].RespondToHits(this);
			}
			LevelObjects.RespondToHits(this);
		}

		protected virtual bool CheckForWinner()
		{
			//check if only one player remains
			int numPlayers = 0;
			for (int i = 0; i < Players.Count; i++)
			{
				if (!CheckIfPlayerStockOut(Players[i]))
				{
					numPlayers++;
				}
			}

			//there can be only one winner!
			if (1 >= numPlayers)
			{
				StopTimers();
				GameOver = true;

				//find the winner!
				for (int i = 0; i < Players.Count; i++)
				{
					if (!CheckIfPlayerStockOut(Players[i]))
					{
						Winner = Players[i];
					}
				}

				if (0 == numPlayers)
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
			if ((null == Winner) && (GameTimer.RemainingTime <= 0.0f))
			{
				//find winner
				int currentMaxStock = 0;
				for (int i = 0; i < Players.Count; i++)
				{
					if (Players[i].Stock >= currentMaxStock)
					{
						//found someone with the max points, but is it a tie?
						if ((Winner != null) && (Winner != Players[i]))
						{
							if (Winner.Stock == Players[i].Stock)
							{
								Tie = true;
							}
						}

						//TODO: whenever a winner is found, set their animation to the win animation

						Winner = Players[i];
						currentMaxStock = Winner.Stock;
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
		protected virtual bool CheckIfPlayerStockOut(PlayerQueue playerQueue)
		{
			return (0 >= playerQueue.Stock);
		}

		/// <summary>
		/// Check if an object is dead (out of bounds) and process the death
		/// </summary>
		/// <param name="rObject">the object to check for death</param>
		private void CheckIfDead(PlayerQueue playerQueue)
		{
			Debug.Assert(null != playerQueue);
			if (playerQueue.CheckIfDead())
			{
				KillPlayer(playerQueue);
			}
		}

		/// <summary>
		/// Play the death particle effect
		///	Respawn the player
		/// Do the correct score calculation
		/// Play the players death sound
		/// </summary>
		/// <param name="rPlayerQueue">the player to kill</param>
		protected virtual void KillPlayer(PlayerQueue playerQueue)
		{
			var playerCharacter = playerQueue.Character;

			//TODO: play the death particle effect
			//PlayParticleEffect(EDefaultParticleEffects.Death,
			//	Vector2.Zero,
			//	rObject.Position,
			//	rObject.PlayerColor);

			//TODO: Play death squish, and the players death sound
			//PlaySound(m_strDeathNoise);
			//PlaySound(rObject.DeathSound);

			//Do the correct score calculation
			playerQueue.SubtractStock();

			if (!CheckIfPlayerStockOut(playerQueue))
			{
				RespawnPlayer(playerQueue);
			}
			else
			{
				playerQueue.Character.Reset();
				playerQueue.DeactivateAllObjects();
			}
		}

		public void RespawnPlayer(PlayerQueue playerQueue)
		{
			//respawn the player
			int spawnIndex = _random.Next(SpawnPoints.Count);
			playerQueue.Reset(SpawnPoints[spawnIndex]);
		}

		protected void StopTimers()
		{
			GameTimer.Stop();
			CharacterClock.Paused = true;
		}

		public void PlayParticleEffect(
			EDefaultParticleEffects eEffect,
			Vector2 velocity,
			Vector2 position,
			Color color)
		{
			ParticleEngine.PlayParticleEffect(DefaultParticles[(int)eEffect], velocity, position, Vector2.Zero, color, false);
		}

		#region Draw

		/// <summary>
		/// update all the drawlists
		/// </summary>
		public void UpdateDrawlists()
		{
			List<Task> tasks = new List<Task>();
			foreach (var player in Players)
			{
				tasks.Add(Task.Factory.StartNew(() => { player.UpdateDrawlists(); }));
			}
			tasks.Add(Task.Factory.StartNew(() => { LevelObjects.UpdateDrawlists(); }));
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
				for (int i = 0; i < Players.Count; i++)
				{
					Players[i].AddToCamera(Renderer.Camera);
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

		public virtual void Render()
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
		}

		protected void RenderLevel(Matrix cameraMatrix)
		{
			//draw the level
			Renderer.SpriteBatchBegin(BlendState.AlphaBlend, cameraMatrix);
			LevelObjects.Render(Renderer, true);
#if DEBUG
			//draw the world boundaries in debug mode?
			if (_renderWorldBoundaries)
			{
				Renderer.Primitive.Rectangle(WorldBoundaries, Color.Red);
			}

			//draw the spawn points for debug mode
			if (_renderSpawnPoints)
			{
				for (int i = 0; i < SpawnPoints.Count; i++)
				{
					Renderer.Primitive.Circle(SpawnPoints[i], 10, Color.Red);
				}
			}
#endif
			Renderer.SpriteBatchEnd();
		}

		/// <summary>
		/// draw the hud
		/// </summary>
		protected virtual void RenderHUD()
		{
		}

		protected void RenderCharacterTrails(Matrix cameraMatrix)
		{
			//render all the character trails, start another spritebatch
			Renderer.SpriteBatchBegin(BlendState.NonPremultiplied, cameraMatrix);
			for (int i = 0; i < Players.Count; i++)
			{
				Players[i].Render(Renderer, false);
			}
			Renderer.SpriteBatchEnd();
		}

		protected void RenderCharacters(Matrix cameraMatrix)
		{
			//render all the players
			Renderer.SpriteBatchBegin(BlendState.NonPremultiplied, cameraMatrix);
			for (int i = 0; i < Players.Count; i++)
			{
				Players[i].Render(Renderer, true);

#if DEBUG
				//draw debug info?
				if (_renderPhysics)
				{
					for (int j = 0; j < Players[i].Active.Count; j++)
					{
						Players[i].Active[j].AnimationContainer.Skeleton.RootBone.DrawPhysics(Renderer, true, Color.White);
					}
				}

				//draw the push box for each character?
				if (_renderJointSkeleton)
				{
					for (int j = 0; j < Players[i].Active.Count; j++)
					{
						Renderer.Primitive.Circle(Players[i].Character.Position,
												  (int)(Players[i].Character.MinDistance()),
												  Color.White);
					}
				}
#endif
			}

#if DEBUG
			if (_drawCameraInfo)
			{
				for (int i = 0; i < Players.Count; i++)
				{
					Players[i].DrawCameraInfo(Renderer);
				}

				Renderer.DrawCameraInfo();
			}
#endif
			Renderer.SpriteBatchEnd();
		}

		protected void RenderParticleEffects(Matrix cameraMatrix)
		{
			//draw all the particles, start another spritebatch for the particles
			Renderer.SpriteBatchBegin(BlendState.NonPremultiplied, cameraMatrix, SpriteSortMode.Deferred);
			ParticleEngine.Render(Renderer.SpriteBatch);
			Renderer.SpriteBatchEnd();
		}

		#endregion //Draw

		#endregion //Methods

		#region File IO

		private void AddParticleEffect(ContentManager xmlContent, IRenderer renderer, string file)
		{
			var emitter = new EmitterTemplate(new Filename(file));
			emitter.ReadXmlFile(xmlContent);
			emitter.LoadContent(renderer);
			DefaultParticles.Add(emitter);
		}

		private void AddParticleEffect(IRenderer renderer, string file)
		{
			var emitter = new EmitterTemplate(new Filename(file));
			emitter.ReadXmlFile();
			emitter.LoadContent(renderer);
			DefaultParticles.Add(emitter);
		}

		public virtual PlayerQueue LoadPlayer(Color color,
			Filename characterFile,
			PlayerIndex index,
			string playerName,
			GameObjectType playerType,
			ContentManager content)
		{
			//create and load a player
			PlayerQueue player = CreatePlayerQueue(color, Players.Count);
			if (null == player.LoadXmlObject(characterFile, this, playerType, 0, content))
			{
				Debug.Assert(false);
			}
			Players.Add(player);

			//create a controller for that player
			InputWrapper queue = new InputWrapper(new ControllerWrapper(index, (PlayerIndex.One == index)), MasterClock.GetCurrentTime)
			{
				BufferedInputExpire = 0.0f,
				QueuedInputExpire = 0.05f
			};
			queue.ReadXmlFile(new Filename(@"MoveList.xml"), player.Character.States.GetMessageIndexFromText, content);
			player.InputQueue = queue;

			//if this is player one, let them use the keyboard
			player.InputQueue.Controller.UseKeyboard = (PlayerIndex.One == index);

			player.PlayerName = playerName;
			return player;
		}

		public void LoadBoard(Filename boardFile, ContentManager content)
		{
			var boardModel = new BoardModel(boardFile);
			boardModel.ReadXmlFile(content);

			//First node is the name
			LevelObjects.PlayerName = boardModel.Name;

			//grab the world boundaries
			WorldBoundaries = new Rectangle((-1 * (boardModel.BoardWidth / 2)),
				(-1 * (boardModel.BoardHeight / 2)),
				boardModel.BoardWidth,
				boardModel.BoardHeight);

			////next node is the music
			//Music = boardModel.Music;
			//if (!string.IsNullOrEmpty(Music))
			//{
			//	//TODO: load the music
			//}

			////next node is the death noise
			//DeathNoise = boardModel.DeathNoise;
			//if (!string.IsNullOrEmpty(DeathNoise))
			//{
			//	//TODO: load the death noise
			//}

			//load all the level objects
			foreach (var levelObjectFile in boardModel.LevelObjects)
			{
				//load the level object
				var levelObject = LevelObjects.LoadXmlObject(levelObjectFile, this, GameObjectType.Level, 0, content);
			}

			//spawn points
			foreach (var spawnPointModel in boardModel.SpawnPoints)
			{
				SpawnPoints.Add(spawnPointModel.Location);
			}
		}

		#endregion //File IO
	}
}