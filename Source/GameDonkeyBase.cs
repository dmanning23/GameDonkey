using GameTimer;
using FilenameBuddy;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using ParticleBuddy;
using RenderBuddy;
using System.Diagnostics;

namespace GameDonkey
{
	public abstract class GameDonkeyBase : IGameDonkey
	{
		#region Members

		/// <summary>
		/// the world boundaries
		/// </summary>
		private Rectangle m_WorldBoundaries;

		#endregion //Members

		#region Properties

		public abstract IRenderer Renderer { get; }

		public ParticleEngine ParticleEngine { get; protected set; }

		public GameClock MasterClock { get; protected set; }

		public Rectangle WorldBoundaries
		{
			get { return m_WorldBoundaries; }
			set
			{
				m_WorldBoundaries = value;

				//make the camera rect a little bit smaller so we can see more of the ground
				Renderer.Camera.WorldBoundary = new Rectangle(m_WorldBoundaries.X, m_WorldBoundaries.Y, m_WorldBoundaries.Width, m_WorldBoundaries.Height + 100);
			}
		}

		#endregion //Properties

		#region Methods

		public GameDonkeyBase()
		{
			ParticleEngine = new ParticleEngine();
			MasterClock = new GameClock();
		}

		/// <summary>
		/// factory method
		/// </summary>
		/// <param name="myColor"></param>
		/// <param name="iQueueID"></param>
		/// <returns></returns>
		public virtual PlayerQueue CreatePlayerQueue(Color myColor, int iQueueID)
		{
			return new PlayerQueue(myColor, iQueueID);
		}

		/// <summary>
		/// load all the content in a windows forms game
		/// </summary>
		public virtual void LoadContent() 
		{
			WorldBoundaries = new Rectangle();
		}

		/// <summary>
		/// load all the content in an xna game
		/// </summary>
		/// <param name="rContent"></param>
		public virtual void LoadSerializedContent(ContentManager rXmlContent, GraphicsDevice rGraphics) { }

		/// <summary>
		/// load all the content in an xna game
		/// </summary>
		/// <param name="rContent"></param>
		public virtual void LoadXmlContent(GraphicsDevice rGraphics) { }

		public virtual void Start()
		{
			MasterClock.Start();
			MasterClock.TimeDelta = 0.0f;
		}

		/// <summary>
		/// update the game engine.  
		/// This function is overridden in child classes.  
		/// The server class checks for game over and updates the client class
		/// The client class will not check for game over.
		/// </summary>
		/// <returns>bool: whether or not this update resulted in a game over situation</returns>
		protected abstract bool Update();

		/// <summary>
		/// update the game engine
		/// </summary>
		/// <param name="rGameTime">current gametime</param>
		/// <returns>bool: true if the game is over, false if it isn't</returns>
		public bool Update(GameTime rGameTime)
		{
			MasterClock.Update(rGameTime);
			return Update();
		}

		/// <summary>
		/// update the game engine
		/// </summary>
		/// <param name="rTime">current gametime</param>
		/// <returns>bool: true if the game is over, false if it isn't</returns>
		public bool Update(TimeUpdater rTime)
		{
			MasterClock.Update(rTime);
			return Update();
		}

		#region Rendering

		public abstract void Render();

		protected abstract void RenderLevel(Matrix cameraMatrix);

		protected abstract void RenderCharacterTrails(Matrix cameraMatrix);

		protected abstract void RenderCharacters(Matrix cameraMatrix);

		protected abstract void RenderParticleEffects(Matrix cameraMatrix);

		#endregion //Rendering

		public virtual void AddCameraShake(float fShakeAmount)
		{
			Renderer.Camera.AddCameraShake(fShakeAmount);
		}

		public virtual void PlayParticleEffect(
			EDefaultParticleEffects eEffect,
			Vector2 Velocity,
			Vector2 Position,
			Color myColor)
		{
		}

		public virtual SoundEffect LoadSound(Filename cueName)
		{
			return null;
		}

		#endregion //Methods
	}
}