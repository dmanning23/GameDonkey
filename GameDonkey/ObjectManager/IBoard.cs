using FilenameBuddy;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace GameDonkeyLib
{
    public interface IBoard
    {
        Rectangle WorldBoundaries { get; set; }

        Rectangle CollisionBoundaries { get; set; }

        PlayerQueue LevelObjects { get; }

        List<Vector2> SpawnPoints { get; set; }

        Vector2 CenterPoint { get; }

        string Music { get; set; }

        Vector2 CenterVelocity { get; }

        void Start();

        void StartAtSpawnPoints(List<IPlayerQueue> players);

        void RespawnPlayer(IGameDonkey engine, IPlayerQueue playerQueue);

        void CollisionDetection(IGameDonkey engine);

        void RenderBackground(IGameDonkey engine);

        void RenderForeground(IGameDonkey engine);

        void RenderLevel(IGameDonkey engine, Matrix cameraMatrix, SpriteSortMode sortMode);

        void LoadBoard(Filename boardFile, IGameDonkey engine, ContentManager xmlContent = null);
    }
}
