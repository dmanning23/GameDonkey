using FilenameBuddy;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using ParallaxBackgroundBuddy;
using ResolutionBuddy;
using System;
using System.Collections.Generic;

namespace GameDonkeyLib
{
    public class Board : IBoard
    {
        #region Properties

        private Rectangle _worldBoundaries;
        public Rectangle WorldBoundaries
        {
            get { return _worldBoundaries; }
            set
            {
                _worldBoundaries = value;
                CollisionBoundaries = value;
            }
        }

        private Rectangle _collisionBoundaries;
        public Rectangle CollisionBoundaries
        {
            get
            {
                return _collisionBoundaries;
            }
            set
            {
                _collisionBoundaries = value;
            }
        }

        public PlayerQueue LevelObjects { get; private set; }

        public List<Vector2> SpawnPoints { get; set; }

        public Vector2 CenterPoint { get; private set; }

        private Texture2D BackgroundImage { get; set; }

        private ParallaxBackground Background { get; set; }

        private ParallaxBackground Foreground { get; set; }

        public string Music { get; set; }

        public Vector2 CenterVelocity { get; private set; }

        #endregion //Properties

        #region Methods

        public Board() : base()
        {
            LevelObjects = new LevelObjectQueue();
            SpawnPoints = new List<Vector2>();
            WorldBoundaries = new Rectangle();

            Background = new ParallaxBackground();
            Foreground = new ParallaxBackground();
        }

        public virtual void Start()
        {
            LevelObjects.Reset();
        }

        public void StartAtSpawnPoints(List<IPlayerQueue> players)
        {
            int spawnIndex = 0;
            for (int i = 0; i < players.Count; i++)
            {
                if (null != players[i].InputQueue)
                {
                    players[i].InputQueue.Controller.ResetController();
                }
                players[i].Reset(SpawnPoints[spawnIndex]);

                if (SpawnPoints[spawnIndex].X > WorldBoundaries.Center.X)
                {
                    players[i].Character.Flip = true;
                }

                if (spawnIndex < (SpawnPoints.Count - 1))
                {
                    ++spawnIndex;
                }
                else
                {
                    spawnIndex = 0;
                }
            }
        }

        public virtual void RespawnPlayer(IGameDonkey engine, IPlayerQueue playerQueue)
        {
            int spawnIndex = engine.Rand.Next(SpawnPoints.Count);
            playerQueue.Reset(SpawnPoints[spawnIndex]);
        }

        public virtual void CollisionDetection(IGameDonkey engine)
        {
            LevelObjects.RespondToHits(engine);

            Vector2 prevCenter = CenterPoint;
            CenterPoint = Vector2.Zero;
            for (int i = 0; i < engine.Players.Count; i++)
            {
                CenterPoint += engine.Players[i].Character.Position;
            }
            CenterPoint /= engine.Players.Count;

            CenterVelocity = prevCenter - CenterPoint;
        }

        #region Draw

        public virtual void RenderBackground(IGameDonkey engine)
        {
            if (null != BackgroundImage)
            {
                engine.Renderer.SpriteBatch.Begin();
                engine.Renderer.SpriteBatch.Draw(BackgroundImage, Resolution.ScreenArea, Color.White);
                engine.Renderer.SpriteBatchEnd();
            }

            if (Background.Layers.Count > 0)
            {
                engine.Renderer.SpriteBatchBeginNoEffect(BlendState.AlphaBlend, engine.GetCameraMatrix());

                Background.Draw(engine.Renderer.SpriteBatch, WorldBoundaries, CenterPoint);

                engine.Renderer.SpriteBatchEnd();
            }
        }

        public virtual void RenderForeground(IGameDonkey engine)
        {
            if (Foreground.Layers.Count > 0)
            {
                engine.Renderer.SpriteBatchBeginNoEffect(BlendState.AlphaBlend, engine.GetCameraMatrix());

                Foreground.Draw(engine.Renderer.SpriteBatch, WorldBoundaries, CenterPoint);

                engine.Renderer.SpriteBatchEnd();
            }
        }

        public virtual void RenderLevel(IGameDonkey engine, Matrix cameraMatrix, SpriteSortMode sortMode)
        {
            if (!LevelObjects.HasActive)
            {
                return;
            }

            engine.Renderer.SpriteBatchBegin(BlendState.AlphaBlend, cameraMatrix, sortMode);
            LevelObjects.Render(engine.Renderer, true);
            engine.Renderer.SpriteBatchEnd();
        }

        #endregion //Draw

        #endregion //Methods

        #region File IO

        public void LoadBoard(Filename boardFile, IGameDonkey engine, ContentManager xmlContent = null)
        {
            try
            {
                var boardModel = new BoardModel(boardFile);
                boardModel.ReadXmlFile(xmlContent);
                LoadBoard(boardModel, engine, xmlContent);
            }
            catch (Exception ex)
            {
                throw new Exception($"There was an error loading {boardFile.GetFile()}", ex);
            }
        }

        protected virtual void LoadBoard(BoardModel boardModel, IGameDonkey engine, ContentManager xmlContent)
        {
            LevelObjects.PlayerName = boardModel.Name;

            WorldBoundaries = new Rectangle((-1 * (boardModel.BoardWidth / 2)),
                (-1 * (boardModel.BoardHeight / 2)),
                boardModel.BoardWidth,
                boardModel.BoardHeight);

            if (boardModel.Floor > 0)
            {
                _collisionBoundaries.Height = boardModel.Floor;
            }

            foreach (var levelObjectFile in boardModel.LevelObjects)
            {
                var levelObject = LevelObjects.LoadXmlObject(levelObjectFile, engine, GameObjectType.Level, 0, xmlContent);
            }

            foreach (var spawnPointModel in boardModel.SpawnPoints)
            {
                SpawnPoints.Add(spawnPointModel.Location);
            }

            if (boardModel.BackgroundImage.HasFilename)
            {
                BackgroundImage = engine.Renderer.Content.Load<Texture2D>(boardModel.BackgroundImage.GetRelPathFileNoExt());
            }

            foreach (var backgroundLayer in boardModel.Background)
            {
                Background.AddLayer(backgroundLayer.ImageFile, backgroundLayer.Scale, engine.Renderer);
            }

            foreach (var foregroundLayer in boardModel.Foreground)
            {
                Foreground.AddLayer(foregroundLayer.ImageFile, foregroundLayer.Scale, engine.Renderer);
            }
        }

        #endregion //File IO
    }
}
