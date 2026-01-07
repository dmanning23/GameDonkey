using CameraBuddy;
using FilenameBuddy;
using GameTimer;
using HadoukInput;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using RenderBuddy;
using System.Collections.Generic;

namespace GameDonkeyLib
{
    public interface IPlayerQueue
    {
        int QueueId { get; }
        List<BaseObject> Active { get; }
        List<BaseObject> Inactive { get; }
        bool HasTrails { get; }
        PlayerObject Character { get; set; }
        HitPauseClock CharacterClock { get; }
        string PlayerName { get; set; }
        InputWrapper InputQueue { get; set; }
        Color PlayerColor { get; set; }
        float Scale { set; }
        bool HasActive { get; }

        bool ActivateObject(BaseObject gameObject);
        void AddToCamera(ICamera camera);
        void CheckCollisions(IPlayerQueue otherGuy);

        public bool CheckIfDead();

        void CheckHardCodedStates();
        //bool CheckIfDead();
        bool CheckListForObject(BaseObject gameObject, bool activeList);
        void CheckWorldCollisions(Rectangle worldBroundaries);
        PlayerObject CreateAiPlayer(string name);
        PlayerObject CreateHumanPlayer(string name);
        PlayerObjectModel CreatePlayerObjectModel(Filename filename);
        void DeactivateAllObjects();
        void DeactivateObject(BaseObject gameObject);
        void DeactivateObjects(string objectType);
        void DrawCameraInfo(IRenderer renderer);
        void GetPlayerInput(List<IPlayerQueue> badGuys, bool ignoreAttackInput);
        BaseObject LoadXmlObject(Filename fileName, IGameDonkey engine, GameObjectType objectType, int difficulty, ContentManager xmlContent);
        BaseObject LoadXmlObject(Filename fileName, IGameDonkey engine, string objectType, ContentManager xmlContent);
        BaseObject LoadXmlObject(BaseObjectModel gameObjectModel, IGameDonkey engine, string objectType, ContentManager xmlContent);
        void Render(IRenderer renderer, bool renderMain);
        void RenderAttacks(IRenderer renderer);
        void RenderCharacterShadows(IGameDonkey engine);
        void RenderPhysics(IRenderer renderer);
        void Reset(Vector2 spawnPoint);
        void Reset();
        void RespondToHits(IGameDonkey engine);
        void SendToBack(BaseObject gameObject);
        //void SubtractStock();
        void Update(GameClock clock);
        void UpdateDrawlists();
        void UpdateInput(IInputState input);
        void UpdateRagdoll();
    }
}