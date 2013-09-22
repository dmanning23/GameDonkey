using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using GameTimer;
using RenderBuddy;
using ParticleBuddy;

namespace GameDonkey
{
	public abstract class IGameDonkey
	{
		#region Members

		private XNARenderer m_Renderer;

		private ParticleEngine m_ParticleEngine;

		private GameClock m_Clock;

		/// <summary>
		/// the world boundaries
		/// </summary>
		private Rectangle m_WorldBoundaries;

		#endregion //Members

		#region Properties

		public XNARenderer Renderer
		{
			get { return m_Renderer; }
		}

		public ParticleEngine ParticleEngine
		{
			get { return m_ParticleEngine; }
		}

		protected GameClock MasterClock
		{
			get { return m_Clock; }
		}

		//public CGameSoundManager SoundEngine
		//{
		//    get { return m_SoundEngine; }
		//}

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

		public IGameDonkey(XNARenderer rRenderer)
		{
			m_Renderer = rRenderer;
			m_ParticleEngine = new ParticleEngine();
			m_Clock = new GameClock();

			WorldBoundaries = new Rectangle();
		}

		/// <summary>
		/// load all the content in a windows forms game
		/// </summary>
		public virtual void LoadContent() { }

		/// <summary>
		/// load all the content in an xna game
		/// </summary>
		/// <param name="rContent"></param>
		public virtual void LoadContent(ContentManager rXmlContent, GraphicsDevice rGraphics) { }

		public virtual void Start()
		{
			m_Clock.Start();
			m_Clock.TimeDelta = 0.0f;
		}

		/// <summary>
		/// update the game engine
		/// </summary>
		/// <param name="rGameTime">current gametime</param>
		/// <returns>bool: true if the game is over, false if it isn't</returns>
		public bool Update(GameTime rGameTime)
		{
			m_Clock.Update(rGameTime);
			return Update();
		}

		/// <summary>
		/// update the game engine
		/// </summary>
		/// <param name="rTime">current gametime</param>
		/// <returns>bool: true if the game is over, false if it isn't</returns>
		public bool Update(TimeUpdater rTime)
		{
			m_Clock.Update(rTime);
			return Update();
		}

		/// <summary>
		/// update the game engine.  
		/// This function is overridden in child classes.  
		/// The server class checks for game over and updates the client class
		/// The client class will not check for game over.
		/// </summary>
		/// <returns>bool: whether or not this update resulted in a game over situation</returns>
		protected abstract bool Update();

		public abstract void Render();

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
			Debug.Assert(false);
		}

		public virtual void PlaySound(string strCueName)
		{
			Debug.Assert(false);
		}

		#endregion //Methods
	}
}