using FilenameBuddy;
using GameDonkeyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using ParallaxBackgroundBuddy;
using ResolutionBuddy;
using System;
using System.Collections.Generic;

namespace GameDonkey.SharedProject.ObjectManager
{
	public class Board : IBoard
	{
		#region Properties

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

		/// <summary>
		/// player queue for updating level objects
		/// </summary>
		public PlayerQueue LevelObjects { get; private set; }

		/// <summary>
		/// the spawn points for characters
		/// </summary>
		public List<Vector2> SpawnPoints { get; set; }

		/// <summary>
		/// the center point between all the players
		/// </summary>
		public Vector2 CenterPoint { get; private set; }

		private Texture2D BackgroundImage { get; set; }

		private ParallaxBackground Background { get; set; }

		private ParallaxBackground Foreground { get; set; }

		/// <summary>
		/// the music resource for the current board
		/// </summary>
		public string Music { get; set; }

		/// <summary>
		/// The velocity of the center point
		/// </summary>
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
			//reset teh level objects
			LevelObjects.Reset();
		}

		public void StartAtSpawnPoints(List<PlayerQueue> players)
		{
			//reset the players
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
		}

		public virtual void RespawnPlayer(IGameDonkey engine, PlayerQueue playerQueue)
		{
			//respawn the player
			int spawnIndex = engine.Rand.Next(SpawnPoints.Count);
			playerQueue.Reset(SpawnPoints[spawnIndex]);
		}

		public virtual void CollisionDetection(IGameDonkey engine)
		{
			LevelObjects.RespondToHits(engine);

			//get the center point in between all the guys
			Vector2 prevCenter = CenterPoint;
			CenterPoint = Vector2.Zero;
			for (int i = 0; i < engine.Players.Count; i++)
			{
				CenterPoint += engine.Players[i].Character.Position;
			}
			CenterPoint /= engine.Players.Count;

			//set the change of the center point
			CenterVelocity = prevCenter - CenterPoint;
		}

		#region Draw

		public virtual void RenderBackground(IGameDonkey engine)
		{
			if (null != BackgroundImage)
			{
				//draw the background first 
				engine.Renderer.SpriteBatch.Begin();
				engine.Renderer.SpriteBatch.Draw(BackgroundImage, Resolution.ScreenArea, Color.White);
				engine.Renderer.SpriteBatchEnd();
			}

			if (Background.Layers.Count > 0)
			{
				engine.Renderer.SpriteBatchBeginNoEffect(BlendState.AlphaBlend, engine.GetCameraMatrix());

				//draw the background to take up the whole board
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

			//draw the level
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
				throw new Exception($"There was an error loading { boardFile.GetFile() }", ex);
			}
		}

		protected virtual void LoadBoard(BoardModel boardModel, IGameDonkey engine, ContentManager xmlContent)
		{
			//First node is the name
			LevelObjects.PlayerName = boardModel.Name;

			//grab the world boundaries
			WorldBoundaries = new Rectangle((-1 * (boardModel.BoardWidth / 2)),
				(-1 * (boardModel.BoardHeight / 2)),
				boardModel.BoardWidth,
				boardModel.BoardHeight);

			if (boardModel.Floor > 0)
			{
				_collisionBoundaries.Height = boardModel.Floor;
			}

			////next node is the music
			//Music = boardModel.Music;
			//if (!string.IsNullOrEmpty(Music))
			//{
			//	//TODO: load the music
			//}

			//load all the level objects
			foreach (var levelObjectFile in boardModel.LevelObjects)
			{
				//load the level object
				var levelObject = LevelObjects.LoadXmlObject(levelObjectFile, engine, GameObjectType.Level, 0, xmlContent);
			}

			//spawn points
			foreach (var spawnPointModel in boardModel.SpawnPoints)
			{
				SpawnPoints.Add(spawnPointModel.Location);
			}

			//Load the background that will be drawn behind the game.
			if (boardModel.BackgroundImage.HasFilename)
			{
				BackgroundImage = engine.Renderer.Content.Load<Texture2D>(boardModel.BackgroundImage.GetRelPathFileNoExt());
			}

			//load the background images
			foreach (var backgroundLayer in boardModel.Background)
			{
				Background.AddLayer(backgroundLayer.ImageFile, backgroundLayer.Scale, engine.Renderer);
			}

			//load the foreground images
			foreach (var foregroundLayer in boardModel.Foreground)
			{
				Foreground.AddLayer(foregroundLayer.ImageFile, foregroundLayer.Scale, engine.Renderer);
			}
		}

		#endregion //File IO
	}
}
