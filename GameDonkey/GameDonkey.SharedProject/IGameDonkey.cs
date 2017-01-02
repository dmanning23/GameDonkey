using FilenameBuddy;
using GameTimer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using ParticleBuddy;
using RenderBuddy;

namespace GameDonkey
{
	public interface IGameDonkey
	{
		#region Properties

		IRenderer Renderer { get; }

		ParticleEngine ParticleEngine { get; }

		GameClock MasterClock { get; }

		Rectangle WorldBoundaries { get; set; }

		ContentManager ContentManager { get; }

		#endregion //Properties

		#region Methods

		/// <summary>
		/// load all the content in a windows forms game
		/// </summary>
		void LoadContent();

		void UnloadContent();

		/// <summary>
		/// load all the content in an xna game
		/// </summary>
		/// <param name="device"></param>
		void LoadXmlContent(GraphicsDevice device);

		void Start();

		/// <summary>
		/// update the game engine
		/// </summary>
		/// <param name="time">current gametime</param>
		/// <returns>bool: true if the game is over, false if it isn't</returns>
		bool Update(GameTime time);

		/// <summary>
		/// update the game engine
		/// </summary>
		/// <param name="time">current gametime</param>
		/// <returns>bool: true if the game is over, false if it isn't</returns>
		bool Update(TimeUpdater time);

		void AddCameraShake(float shakeAmount);

		void PlayParticleEffect(
			EDefaultParticleEffects effect,
			Vector2 velocity,
			Vector2 position,
			Color color);

		SoundEffect LoadSound(Filename cueName);

		#endregion //Methods
	}
}