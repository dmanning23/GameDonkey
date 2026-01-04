using FilenameBuddy;
using GameTimer;
using HadoukInput;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using ParticleBuddy;
using RenderBuddy;
using System;
using System.Collections.Generic;

namespace GameDonkeyLib
{
    public interface IGameDonkey
    {
        #region Properties

        Random Rand { get; }

        Game Game { get; }

        bool ToolMode { get; set; }
        bool ProjectileXML { get; set; }

        IRenderer Renderer { get; }

        ParticleEngine ParticleEngine { get; }

        GameClock MasterClock { get; }

        IBoard Board { get; set; }

        Rectangle WorldBoundaries { get; set; }

        List<IPlayerQueue> Players { get; }

        //PlayerQueue Winner { get; }

        //bool Tie { get; }

        //bool GameOver { get; }

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

        void UpdateInput(IInputState input);

        void AddCameraShake(float shakeAmount);

        void PlayParticleEffect(
            DefaultParticleEffect effect,
            Vector2 velocity,
            Vector2 position,
            Color color);

        SoundEffect LoadSound(Filename cueName);

        PlayerObjectModel LoadModel(Filename characterFile, ContentManager xmlContent = null);

        IPlayerQueue LoadPlayer(Color color,
           Filename characterFile,
           int playerIndex,
           string playerName,
           string playerType = "Human",
           ContentManager xmlContent = null,
           bool useKeyboard = false);

        IBoard LoadBoard(Filename boardFile, ContentManager xmlContent = null);

        void RespawnPlayer(IPlayerQueue playerQueue);

        void UpdateCameraMatrix(bool forceToScreen = false);

        Matrix GetCameraMatrix();

        void Render(BlendState characterBlendState, SpriteSortMode sortMode = SpriteSortMode.Immediate);

        #endregion //Methods
    }
}