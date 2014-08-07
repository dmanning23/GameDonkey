using GameTimer;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using ParticleBuddy;
using RenderBuddy;
using FilenameBuddy;

namespace GameDonkey
{
	public interface IGameDonkey
	{
		#region Properties

		IRenderer Renderer { get; }

		ParticleEngine ParticleEngine { get; }

		GameClock MasterClock { get; }

		Rectangle WorldBoundaries { get; set; }

		#endregion //Properties

		#region Methods

		/// <summary>
		/// load all the content in a windows forms game
		/// </summary>
		void LoadContent();

		/// <summary>
		/// load all the content in an xna game
		/// </summary>
		/// <param name="rContent"></param>
		void LoadSerializedContent(ContentManager rXmlContent, GraphicsDevice rGraphics);

		/// <summary>
		/// load all the content in an xna game
		/// </summary>
		/// <param name="rContent"></param>
		void LoadXmlContent(GraphicsDevice rGraphics);

		void Start();

		/// <summary>
		/// update the game engine
		/// </summary>
		/// <param name="rGameTime">current gametime</param>
		/// <returns>bool: true if the game is over, false if it isn't</returns>
		bool Update(GameTime rGameTime);

		/// <summary>
		/// update the game engine
		/// </summary>
		/// <param name="rTime">current gametime</param>
		/// <returns>bool: true if the game is over, false if it isn't</returns>
		bool Update(TimeUpdater rTime);

		void AddCameraShake(float fShakeAmount);

		void PlayParticleEffect(
			EDefaultParticleEffects eEffect,
			Vector2 Velocity,
			Vector2 Position,
			Color myColor);

		SoundEffect LoadSound(Filename cueName);

		#endregion //Methods
	}
}