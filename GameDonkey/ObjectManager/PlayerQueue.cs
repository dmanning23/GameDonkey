using CameraBuddy;
using DrawListBuddy;
using FilenameBuddy;
using GameTimer;
using HadoukInput;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using RenderBuddy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameDonkeyLib
{
    public class PlayerQueue : IPlayerQueue
    {
        #region Properties

        private static int _nextQueueId;

        public int QueueId { get; private set; }

        public List<BaseObject> Active { get; private set; }

        public List<BaseObject> Inactive { get; private set; }

        protected List<DrawList> TrailDrawLists { get; set; }

        public bool HasTrails => TrailDrawLists.Count > 0;

        public PlayerObject Character { get; set; }

        public HitPauseClock CharacterClock { get; protected set; }

        public string PlayerName { get; set; }

        public InputWrapper InputQueue { get; set; }

        private Color _playerColor;
        public Color PlayerColor
        {
            get
            {
                return _playerColor;
            }
            set
            {
                _playerColor = value;
                foreach (var active in Active)
                {
                    active.PlayerColor = PlayerColor;
                }
                foreach (var inactive in Inactive)
                {
                    inactive.PlayerColor = PlayerColor;
                }
            }
        }


        public float Scale
        {
            set
            {
                foreach (var gameObject in Active)
                {
                    gameObject.Scale = value;
                }

                foreach (var gameObject in Inactive)
                {
                    gameObject.Scale = value;
                }
            }
        }

        public bool HasActive => Active.Count > 0;

        #endregion //Properties

        #region Methods 

        public PlayerQueue(Color playerColor)
        {
            Active = new List<BaseObject>();
            Inactive = new List<BaseObject>();
            TrailDrawLists = new List<DrawList>();
            Character = null;
            CharacterClock = new HitPauseClock();
            PlayerColor = playerColor;
            InputQueue = null;
            QueueId = _nextQueueId++;
        }

        public virtual PlayerObject CreateHumanPlayer(string name)
        {
            return new PlayerObject(GameObjectType.Human, CharacterClock, QueueId, name);
        }

        public virtual PlayerObject CreateAiPlayer(string name)
        {
            //TODO: where is AIController set?
            return new PlayerObject(GameObjectType.AI, CharacterClock, QueueId, name);
        }

        public virtual PlayerObjectModel CreatePlayerObjectModel(Filename filename)
        {
            return new PlayerObjectModel(filename);
        }

        public bool ActivateObject(BaseObject gameObject)
        {
            for (var i = 0; i < Inactive.Count; i++)
            {
                if (Inactive[i].Id == gameObject.Id)
                {
                    Active.Add(Inactive[i]);
                    Inactive.RemoveAt(i);

                    gameObject.Reset();

                    return true;
                }
            }

            return false;
        }

        public void SendToBack(BaseObject gameObject)
        {
            bool found = false;
            for (var i = 0; i < Active.Count; i++)
            {
                if (Active[i].Id == gameObject.Id)
                {
                    Active.RemoveAt(i);
                    found = true;
                    break;
                }
            }

            if (found)
            {
                Active.Insert(0, gameObject);
            }
        }

        public void DeactivateObject(BaseObject gameObject)
        {
            for (var i = 0; i < Active.Count; i++)
            {
                if (Active[i].Id == gameObject.Id)
                {
                    if (!(Active[i] is ProjectileObject))
                    {
                        Inactive.Add(Active[i]);
                    }

                    Active.RemoveAt(i);
                    return;
                }
            }
        }

        public void DeactivateObjects(string objectType)
        {
            var i = 0;
            while (i < Active.Count)
            {
                if (Active[i].ObjectType == objectType)
                {
                    if (!(Active[i] is ProjectileObject))
                    {
                        Inactive.Add(Active[i]);
                    }
                    Active.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
        }

        public void DeactivateAllObjects()
        {
            while (Active.Count > 0)
            {
                var gameObject = Active[0];
                if (!(gameObject is ProjectileObject))
                {
                    Inactive.Add(gameObject);
                }
                Active.RemoveAt(0);

                gameObject.States.Reset();
            }

            TrailDrawLists.Clear();
        }

        public virtual void Reset(Vector2 spawnPoint)
        {
            Character.Flip = (spawnPoint.X >= 0f);
            Character.Position = spawnPoint;
            Character.Velocity = Vector2.Zero;

            DeactivateAllObjects();
            ActivateObject(Character);
        }

        public virtual void Reset()
        {
            DeactivateAllObjects();
            ActivateObject(Character);
        }

        public bool CheckListForObject(BaseObject gameObject, bool activeList)
        {
            if (activeList)
            {
                for (var i = 0; i < Active.Count; i++)
                {
                    if (Active[i].Id == gameObject.Id)
                    {
                        return true;
                    }
                }
            }
            else
            {
                for (var i = 0; i < Inactive.Count; i++)
                {
                    if (Inactive[i].Id == gameObject.Id)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public virtual void Update(GameClock clock)
        {
            CharacterClock.Update(clock);

            for (var i = 0; i < Active.Count; i++)
            {
                Active[i].Update();
            }
        }

        public virtual bool CheckIfDead()
        {
            return false;
        }

        public void UpdateInput(IInputState input)
        {
            Character.UpdateInput(InputQueue, input);
        }

        public void GetPlayerInput(List<IPlayerQueue> badGuys, bool ignoreAttackInput)
        {
            for (var i = 0; i < Active.Count; i++)
            {
                Active[i].GetPlayerInput(InputQueue, badGuys, ignoreAttackInput);
            }
        }

        public void CheckHardCodedStates()
        {
            for (var i = 0; i < Active.Count; i++)
            {
                Active[i].CheckHardCodedStates();
            }
        }

        public void UpdateRagdoll()
        {
            for (var i = 0; i < Active.Count; i++)
            {
                Active[i].UpdateRagdoll();
            }
        }

        public void CheckCollisions(IPlayerQueue otherGuy)
        {
            for (var i = 0; i < Active.Count; i++)
            {
                for (var j = 0; j < otherGuy.Active.Count; j++)
                {
                    Active[i].CheckCollisions(otherGuy.Active[j]);
                }
            }
        }

        public void CheckWorldCollisions(Rectangle worldBroundaries)
        {
            for (var i = 0; i < Active.Count; i++)
            {
                Active[i].CheckWorldCollisions(worldBroundaries);
            }
        }

        public void RespondToHits(IGameDonkey engine)
        {
            for (var i = 0; i < Active.Count; i++)
            {
                Active[i].HitResponse(engine);
            }
        }

        public void UpdateDrawlists()
        {
            var drawlistIndex = 0;
            while (drawlistIndex < TrailDrawLists.Count)
            {
                if (TrailDrawLists[drawlistIndex].Update(CharacterClock))
                {
                    //this drawlist is expired
                    TrailDrawLists.RemoveAt(drawlistIndex);
                }
                else
                {
                    drawlistIndex++;
                }
            }

            for (var i = 0; i < Active.Count; i++)
            {
                //add a character trail, if we need it
                if (Active[i].DoesNeedCharacterTrail())
                {
                    //add a trail right in front of the main dude
                    var trailDrawList = new DrawList();
                    trailDrawList.Set(Active[i].TrailAction.TrailLifeDelta,
                        Active[i].TrailAction.StartColor,
                        Active[i].Scale);

                    Active[i].AnimationContainer.Render(trailDrawList);
                    TrailDrawLists.Add(trailDrawList);
                }

                Active[i].UpdateDrawlist();
            }
        }

        public void AddToCamera(ICamera camera)
        {
            for (var i = 0; i < Active.Count; i++)
            {
                Active[i].AddToCamera(camera);
            }
        }

        public void DrawCameraInfo(IRenderer renderer)
        {
            for (var i = 0; i < Active.Count; i++)
            {
                Active[i].DrawCameraInfo(renderer);
            }
        }

        public void Render(IRenderer renderer, bool renderMain)
        {
            if (renderMain)
            {
                //render all the main drawlists
                for (var i = 0; i < Active.Count; i++)
                {
                    Active[i].Render(renderer);
                }
            }
            else
            {
                //render all the trail drawlists
                for (var i = 0; i < TrailDrawLists.Count; i++)
                {
                    TrailDrawLists[i].Render(renderer);
                }
            }
        }

        public virtual void RenderCharacterShadows(IGameDonkey engine)
        {
            for (var i = 0; i < Active.Count; i++)
            {
                Active[i].RenderCharacterShadow(engine);
            }
        }

        public void RenderPhysics(IRenderer renderer)
        {
            for (var i = 0; i < Active.Count; i++)
            {
                Active[i].RenderPhysics(renderer);
            }
        }

        public void RenderAttacks(IRenderer renderer)
        {
            for (var i = 0; i < Active.Count; i++)
            {
                Active[i].RenderAttacks(renderer);
            }
        }

        #endregion //Methods

        #region File IO

        // legacy overload kept for older GameDonkey consumers
        public BaseObject LoadXmlObject(Filename fileName, IGameDonkey engine, GameObjectType objectType, int difficulty, ContentManager xmlContent)
        {
            return LoadXmlObject(fileName, engine, objectType.ToString(), xmlContent);
        }

        public BaseObject LoadXmlObject(Filename fileName, IGameDonkey engine, string objectType, ContentManager xmlContent)
        {
            //load the model
            ObjectModelFactory(fileName, objectType, out BaseObjectModel gameObjectModel);
            gameObjectModel.ReadXmlFile(xmlContent);

            return LoadXmlObject(gameObjectModel, engine, objectType, xmlContent);
        }

        public BaseObject LoadXmlObject(BaseObjectModel gameObjectModel, IGameDonkey engine, string objectType, ContentManager xmlContent)
        {
            var gameObject = Inactive.FirstOrDefault(x => x.ObjectType == objectType);
            if (null == gameObject)
            {
                //try to load the object
                ObjectFactory(objectType, out gameObject, gameObjectModel.Filename.GetFileNoExt());

                //load the object data into the thing
                gameObject.PlayerQueue = this;
                gameObject.ParseXmlData(gameObjectModel, engine, xmlContent);

                //add to the correct list
                if (objectType == "Level")
                {
                    Active.Add(gameObject);
                }
                else
                {
                    Inactive.Add(gameObject);
                }

                //set the color too
                gameObject.PlayerColor = PlayerColor;
            }

            return gameObject;
        }

        protected virtual void ObjectFactory(string objectType, out BaseObject gameObject, string name)
        {
            switch (objectType)
            {
                case "Human":
                    {
                        gameObject = CreateHumanPlayer(name);

                        //set as the main character
                        AddCharacterToList(gameObject);
                    }
                    break;
                case "AI":
                    {
                        gameObject = CreateAiPlayer(name);

                        //set as the main character
                        AddCharacterToList(gameObject);
                    }
                    break;
                case "Projectile":
                    {
                        gameObject = new ProjectileObject(CharacterClock, Character, QueueId, name);
                    }
                    break;
                case "Level":
                    {
                        gameObject = new LevelObject(CharacterClock, QueueId, name);
                    }
                    break;
                default:
                    {
                        throw new Exception($"Unknown objectType passed to ObjectFactory: {objectType}");
                    }
            }
        }

        protected virtual void ObjectModelFactory(Filename fileName, string objectType, out BaseObjectModel gameObjectModel)
        {
            switch (objectType)
            {
                case "Human":
                    {
                        gameObjectModel = CreatePlayerObjectModel(fileName);
                    }
                    break;
                case "AI":
                    {
                        gameObjectModel = CreatePlayerObjectModel(fileName);
                    }
                    break;
                case "Projectile":
                    {
                        gameObjectModel = new ProjectileObjectModel(fileName);
                    }
                    break;
                case "Level":
                    {
                        gameObjectModel = new LevelObjectModel(fileName);
                    }
                    break;
                default:
                    {
                        throw new Exception($"Unknown objectType passed to ObjectModelFactory: {objectType}");
                    }
            }
        }

        protected virtual void AddCharacterToList(BaseObject gameObject)
        {
            Character = gameObject as PlayerObject;
        }

        #endregion
    }
}