using FilenameBuddy;
using GameTimer;
using HadoukInput;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using ParticleBuddy;
using RenderBuddy;
using System.Collections.Generic;

namespace GameDonkeyLib
{
	public interface IGameDonkey
	{
		#region Properties

		bool ToolMode { get; set; }

		IRenderer Renderer { get; }

		ParticleEngine ParticleEngine { get; }

		GameClock MasterClock { get; }

		Rectangle WorldBoundaries { get; set; }

		List<PlayerQueue> Players { get; }

		List<Vector2> SpawnPoints { get; set; }

		#endregion //Properties

		#region Methods

		/// <summary>
		/// load all the content in a windows forms game
		/// </summary>
		void LoadContent(GraphicsDevice device, ContentManager xmlContent);

		void UnloadContent();

		void SetClockSpeed(float speed);

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

		void UpdateInput(InputState input);

		void AddCameraShake(float shakeAmount);

		void PlayParticleEffect(
			DefaultParticleEffect effect,
			Vector2 velocity,
			Vector2 position,
			Color color);

		SoundEffect LoadSound(Filename cueName);

		PlayerQueue LoadPlayer(Color color,
		   Filename characterFile,
		   PlayerIndex index,
		   string playerName,
		   GameObjectType playerType = GameObjectType.Human,
		   ContentManager content = null);

		void LoadBoard(Filename boardFile, ContentManager content);

		void RespawnPlayer(PlayerQueue playerQueue);

		void UpdateCameraMatrix(bool forceToScreen = false);

		void Render(BlendState characterBlendState);

		#endregion //Methods
	}
}