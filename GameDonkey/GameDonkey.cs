using FilenameBuddy;
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
using System.Threading.Tasks;

namespace GameDonkeyLib
{
    public class GameDonkey : IGameDonkey, IDisposable
    {
        #region Properties

        public Random Rand { get; private set; } = new Random(DateTime.Now.Millisecond);

        protected KeyboardState _lastKeyboardState;

        protected bool _renderJointSkeleton;
        public bool DebugPhysics { get; set; } = false;
        public bool DebugWorldBoundaries { get; set; } = false;
        protected bool _renderAI;
        protected bool _drawCameraInfo;
        protected bool _renderSpawnPoints;

        public Game Game { get; set; }

        private bool _toolMode = false;
        public bool ToolMode
        {
            get
            {
                return _toolMode;
            }
            set
            {
                _toolMode = value;
                ProjectileXML = ToolMode;
            }
        }

        public bool ProjectileXML { get; set; } = false;

        public IRenderer Renderer { get; private set; }

        public ParticleEngine ParticleEngine { get; protected set; }

        public GameClock MasterClock { get; protected set; }

        public IBoard Board { get; set; }

        public Rectangle WorldBoundaries
        {
            get { return Board.WorldBoundaries; }
            set
            {
                Board.WorldBoundaries = value;

                //make the camera rect a little bit smaller so we can see more of the ground
                Renderer.Camera.WorldBoundary = new Rectangle(WorldBoundaries.X, WorldBoundaries.Y, WorldBoundaries.Width, WorldBoundaries.Height);
            }
        }

        public IPlayerQueue Character
        {
            get { return Players[0]; }
        }

        public List<IPlayerQueue> Players { get; private set; }

        public CountdownTimer GameTimer { get; private set; }

        public GameClock CharacterClock { get; protected set; }

        public string Music { get; private set; }

        protected ContentManager SoundContent { get; private set; }

        protected ParticleEffectCollection ParticleEffects { get; set; }

        public bool HasTrails
        {
            get
            {
                for (var i = 0; i < Players.Count; i++)
                {
                    if (Players[i].HasTrails)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        protected virtual bool RenderShadows => false;

        #endregion //Properties

        #region Construction

        public GameDonkey(IRenderer renderer, Game game) : base()
        {
            ToolMode = false;
            ParticleEngine = new ParticleEngine();
            MasterClock = new GameClock();

            Game = game;
            SoundContent = new ContentManager(Game.Services, "Content");

            Renderer = renderer;
            Players = new List<IPlayerQueue>();

            ParticleEffects = new ParticleEffectCollection();

            CharacterClock = new GameClock();

            _renderJointSkeleton = false;
            DebugPhysics = false;
            _renderAI = false;
            _drawCameraInfo = false;
            DebugWorldBoundaries = false;
            _renderSpawnPoints = false;

            GameTimer = new CountdownTimer();
        }

        public virtual IPlayerQueue CreatePlayerQueue(Color color)
        {
            return new PlayerQueue(color);
        }

        public virtual void LoadContent(GraphicsDevice graphics, ContentManager xmlContent)
        {
            Renderer.LoadContent(graphics);
        }

        public virtual void UnloadContent()
        {
            SoundContent?.Dispose();
            SoundContent = null;

            Renderer?.UnloadContent();
            Renderer = null;

            Players = null;
        }

        public void Dispose()
        {
            UnloadContent();
        }

        public virtual void Start()
        {
            _lastKeyboardState = Keyboard.GetState();

            MasterClock.Start();
            MasterClock.TimeDelta = 0.0f;

            SetClockSpeed(1.0f);

            CharacterClock.Start();

            Board.Start();

            for (int i = 0; i < Players.Count; i++)
            {
                Players[i].AddToCamera(Renderer.Camera);
            }

            Renderer.Camera.ForceToScreen();
        }

        public void StartAtSpawnPoints()
        {
            Board.StartAtSpawnPoints(Players);
        }

        public SoundEffect LoadSound(Filename cueName)
        {
            return SoundContent.Load<SoundEffect>(cueName.GetRelPathFileNoExt());
        }

        #endregion //Construction

        #region Methods

        public virtual void AddCameraShake(float shakeAmount)
        {
            Renderer.Camera.AddCameraShake(shakeAmount);
        }

        public void SetClockSpeed(float speed)
        {
            CharacterClock.TimerSpeed = speed;
        }

        // hack for local testing: in the real game each player queue is updated in the network game loop
        public void UpdateInput(IInputState input)
        {
            var tasks = new List<Task>();
            foreach (var player in Players)
            {
                if (null != player.InputQueue)
                {
                    tasks.Add(Task.Factory.StartNew(() => { player.UpdateInput(input); }));
                }
            }
            Task.WaitAll(tasks.ToArray());
        }

        // overridden in child classes: server checks for game over, client does not
        protected virtual bool Update()
        {
            Renderer.Camera.Update(MasterClock);

            GameTimer.Update(MasterClock);
            CharacterClock.Update(GameTimer);

            Renderer.Update(CharacterClock);

            CheckForWinner();

            Board.LevelObjects.Update(GameTimer);

            UpdatePlayers();

            //TODO: update animation with master clock if game is over

            CollisionDetection();

            UpdateRagdoll();

            UpdateDrawlists();

            ParticleEngine.Update(MasterClock);

            UpdateStuff();

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

            if (currentState.IsKeyDown(Keys.O) && _lastKeyboardState.IsKeyUp(Keys.O))
            {
                _drawCameraInfo = !_drawCameraInfo;
            }
            if (currentState.IsKeyDown(Keys.P) && _lastKeyboardState.IsKeyUp(Keys.P))
            {
                DebugWorldBoundaries = !DebugWorldBoundaries;
            }
            if (currentState.IsKeyDown(Keys.T) && _lastKeyboardState.IsKeyUp(Keys.T))
            {
                _renderAI = !_renderAI;
            }
#endif

            return false;
        }

        public bool Update(GameTime time)
        {
            MasterClock.Update(time);
            return Update();
        }

        public bool Update(TimeUpdater time)
        {
            MasterClock.Update(time);
            return Update();
        }

        protected void UpdatePlayers()
        {
            foreach (var player in Players)
            {
                UpdatePlayer(player);
            }
        }

        protected virtual void UpdatePlayer(IPlayerQueue playerQueue)
        {
            playerQueue.Update(CharacterClock);
        }

        protected void UpdateRagdoll()
        {
            List<Task> tasks = new List<Task>();
            foreach (var player in Players)
            {
                tasks.Add(Task.Factory.StartNew(() => { player.UpdateRagdoll(); }));
            }
            tasks.Add(Task.Factory.StartNew(() => { Board.LevelObjects.UpdateRagdoll(); }));
            Task.WaitAll(tasks.ToArray());
        }

        protected virtual void UpdateStuff()
        {
        }

        protected virtual void CollisionDetection()
        {
            for (int i = 0; i < Players.Count; i++)
            {
                for (int j = i + 1; j < Players.Count; j++)
                {
                    Players[i].CheckCollisions(Players[j]);
                }

                Players[i].CheckCollisions(Board.LevelObjects);
                Players[i].CheckWorldCollisions(Board.CollisionBoundaries);
            }

            for (int i = 0; i < Players.Count; i++)
            {
                Players[i].RespondToHits(this);
            }

            Board.CollisionDetection(this);
        }

        protected virtual bool CheckForWinner()
        {
            return false;
        }

        protected virtual void CheckForTimeOver()
        {
        }

        protected virtual bool CheckIfPlayerStockOut(IPlayerQueue playerQueue)
        {
            return false;
        }

        protected virtual bool CheckIfDead(IPlayerQueue playerQueue)
        {
            var deathOcurred = false;
            if (playerQueue.CheckIfDead())
            {
                KillPlayer(playerQueue);
                deathOcurred = true;
            }
            return deathOcurred;
        }

        protected virtual void KillPlayer(IPlayerQueue playerQueue)
        {
        }

        public void RespawnPlayer(IPlayerQueue playerQueue)
        {
            Board.RespawnPlayer(this, playerQueue);
        }

        protected void StopTimers()
        {
            GameTimer.Stop();
            CharacterClock.Paused = true;
        }

        public void PlayParticleEffect(
            DefaultParticleEffect effect,
            Vector2 velocity,
            Vector2 position,
            Color color)
        {
            var emitterTemplate = ParticleEffects.GetEmitterTemplate(effect);
            ParticleEngine.PlayParticleEffect(emitterTemplate, velocity, position, Vector2.Zero, color, false);
        }

        #region Draw

        public void UpdateDrawlists()
        {
            List<Task> tasks = new List<Task>();
            foreach (var player in Players)
            {
                tasks.Add(Task.Factory.StartNew(() => { player.UpdateDrawlists(); }));
            }
            tasks.Add(Task.Factory.StartNew(() => { Board.LevelObjects.UpdateDrawlists(); }));
            Task.WaitAll(tasks.ToArray());
        }

        public virtual void UpdateCameraMatrix(bool forceToScreen)
        {
            for (int i = 0; i < Players.Count; i++)
            {
                Players[i].AddToCamera(Renderer.Camera);
            }

            //Get the camera matrix we are gonna use
            Renderer.Camera.BeginScene(forceToScreen);
        }

        public Matrix GetCameraMatrix()
        {
            return Renderer.Camera.TranslationMatrix * Resolution.TransformationMatrix();
        }

        public virtual void Render(BlendState characterBlendState, SpriteSortMode sortMode = SpriteSortMode.Immediate)
        {
            Matrix cameraMatrix = GetCameraMatrix();

            RenderBackground();

            RenderLevel(cameraMatrix, sortMode);

            RenderCharacterShadows(cameraMatrix, sortMode);

            RenderHUD();

            RenderCharacterTrails(cameraMatrix, sortMode);

            RenderCharacters(cameraMatrix, characterBlendState, sortMode);

            RenderParticleEffects(cameraMatrix);

            RenderForeground();
        }

        protected virtual void RenderBackground()
        {
            Board.RenderBackground(this);
        }

        protected virtual void RenderForeground()
        {
            Board.RenderForeground(this);
        }

        protected virtual void RenderLevel(Matrix cameraMatrix, SpriteSortMode sortMode)
        {
            Board.RenderLevel(this, cameraMatrix, sortMode);

#if DEBUG
            //draw the world boundaries in debug mode?
            if (DebugWorldBoundaries)
            {
                Renderer.SpriteBatchBegin(BlendState.AlphaBlend, cameraMatrix, sortMode);
                Renderer.Primitive.Rectangle(WorldBoundaries, Color.Red);
                Renderer.SpriteBatchEnd();
            }

            //draw the spawn points for debug mode
            if (_renderSpawnPoints)
            {
                Renderer.SpriteBatchBegin(BlendState.AlphaBlend, cameraMatrix, sortMode);
                for (int i = 0; i < Board.SpawnPoints.Count; i++)
                {
                    Renderer.Primitive.Circle(Board.SpawnPoints[i], 10, Color.Red);
                }
                Renderer.SpriteBatchEnd();
            }

#endif
        }

        protected virtual void RenderCharacterShadows(Matrix cameraMatrix, SpriteSortMode sortMode)
        {
            if (!RenderShadows)
            {
                return;
            }

            Renderer.SpriteBatchBeginNoEffect(BlendState.NonPremultiplied, cameraMatrix, sortMode);
            for (int i = 0; i < Players.Count; i++)
            {
                Players[i].RenderCharacterShadows(this);
            }
            Renderer.SpriteBatchEnd();
        }

        protected virtual void RenderHUD()
        {
        }

        protected void RenderCharacterTrails(Matrix cameraMatrix, SpriteSortMode sortMode)
        {
            if (!HasTrails)
            {
                return;
            }

            Renderer.SpriteBatchBegin(BlendState.NonPremultiplied, cameraMatrix, sortMode);
            for (int i = 0; i < Players.Count; i++)
            {
                Players[i].Render(Renderer, false);
            }
            Renderer.SpriteBatchEnd();
        }

        protected void RenderCharacters(Matrix cameraMatrix, BlendState blendState, SpriteSortMode sortMode)
        {
            Renderer.SpriteBatchBegin(blendState, cameraMatrix, sortMode);
            for (int i = 0; i < Players.Count; i++)
            {
                Players[i].Render(Renderer, true);

#if DEBUG
                //draw debug info?
                if (DebugPhysics)
                {
                    for (int j = 0; j < Players[i].Active.Count; j++)
                    {
                        Players[i].Active[j].AnimationContainer.Skeleton.RootBone.DrawPhysics(Renderer, true, Color.White);
                    }

                    for (int j = 0; j < Players[i].Active.Count; j++)
                    {
                        Players[i].RenderAttacks(Renderer);
                    }
                }

                //Draw the AI direction?
                if (_renderAI)
                {
                    for (int j = 0; j < Players[i].Active.Count; j++)
                    {
                        Renderer.Primitive.Line(Players[i].Active[j].Position,
                                                Players[i].Active[j].Position + (100f * Players[i].Active[j].Direction()),
                                                  Color.White);
                    }
                }

                //draw the push box for each character?
                if (_renderJointSkeleton)
                {
                    for (int j = 0; j < Players[i].Active.Count; j++)
                    {
                        Renderer.Primitive.Circle(Players[i].Active[j].Position,
                                                  (int)(Players[i].Active[j].MinDistance()),
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
            if (!ParticleEngine.HasEmitters)
            {
                return;
            }

            Renderer.SpriteBatchBeginNoEffect(BlendState.NonPremultiplied, cameraMatrix, SpriteSortMode.Deferred);
            ParticleEngine.Render(Renderer.SpriteBatch);
            Renderer.SpriteBatchEnd();
        }

        #endregion //Draw

        #endregion //Methods

        #region File IO

        public PlayerObjectModel LoadModel(Filename filename, ContentManager xmlContent)
        {
            var model = new PlayerObjectModel(filename);
            model.ReadXmlFile(xmlContent);
            return model;
        }

        public virtual IPlayerQueue LoadPlayer(Color color,
            Filename characterFile,
            int playerIndex,
            string playerName,
            string playerType,
            ContentManager xmlContent,
            bool useKeyboard)
        {
            var player = CreatePlayerQueue(color);
            player.LoadXmlObject(characterFile, this, playerType, xmlContent);
            Players.Add(player);

            InputWrapper queue = new InputWrapper(new ControllerWrapper(playerIndex), MasterClock.GetCurrentTime)
            {
                BufferedInputExpire = 0.0f,
                QueuedInputExpire = 0.05f
            };
            queue.ReadXmlFile(new Filename(@"MoveList.xml"), xmlContent);
            player.InputQueue = queue;

            player.PlayerName = playerName;
            return player;
        }

        public virtual IBoard CreateBoard()
        {
            return new Board();
        }

        public IBoard LoadBoard(Filename boardFile, ContentManager xmlContent = null)
        {
            Board = CreateBoard();
            Board.LoadBoard(boardFile, this, xmlContent);

            Renderer.Camera.WorldBoundary = new Rectangle(WorldBoundaries.X, WorldBoundaries.Y, WorldBoundaries.Width, WorldBoundaries.Height);

            return Board;
        }

        #endregion //File IO
    }
}