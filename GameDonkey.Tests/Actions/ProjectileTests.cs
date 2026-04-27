using FilenameBuddy;
using GameDonkeyLib;
using Microsoft.Xna.Framework;
using NUnit.Framework;
using Shouldly;

namespace GameDonkey.Tests
{
    [TestFixture]
    public class ProjectileTests
    {
        const float time = 1f;
        const float scale = 2f;
        const float X = 3f;
        const float Y = 4f;
        const float offsetX = 5f;
        const float offsetY = 6f;
        const EDirectionType directionType = EDirectionType.Absolute;

        [SetUp]
        public void Setup()
        {
            TestHelpers.InitFilePaths();
        }

        [Test]
        public void ModelToAction_SetsProperties()
        {
            var model = new ProjectileActionModel(); // defaults: Scale=1, StartOffset=Zero

            var action = new ProjectileAction(null, model);

            action.Scale.ShouldBe(1f);
            action.StartOffset.ShouldBe(Vector2.Zero);
        }

        [Test]
        public void ActionToModel_SetsProperties()
        {
            var action = new ProjectileAction(null)
            {
                Time = time,
                Scale = scale,
                StartOffset = new Vector2(offsetX, offsetY),
                Velocity = new ActionDirection()
                {
                    DirectionType = directionType,
                    Velocity = new Vector2(X, Y)
                }
            };

            var model = new ProjectileActionModel(action);

            model.ActionType.ShouldBe(EActionType.Projectile);
            model.Time.ShouldBe(time);
            model.Scale.ShouldBe(scale);
            model.StartOffset.ShouldBe(new Vector2(offsetX, offsetY));
            model.Direction.DirectionType.ShouldBe(directionType);
            model.Direction.Velocity.X.ShouldBe(X);
            model.Direction.Velocity.Y.ShouldBe(Y);
        }

        [Test]
        public void Persist()
        {
            var action = new ProjectileAction(null)
            {
                Time = time,
                Scale = scale,
                StartOffset = new Vector2(offsetX, offsetY),
                Velocity = new ActionDirection()
                {
                    DirectionType = directionType,
                    Velocity = new Vector2(X, Y)
                }
            };
            var model = new ProjectileActionModel(action);

            var container = new StateContainerModel(new Filename("ProjectileTests.xml"));
            var stateActions = new SingleStateActionsModel();
            container.StatesActions.Add(stateActions);
            stateActions.ActionModels.Add(model);
            container.WriteXml();

            var container2 = new StateContainerModel(new Filename("ProjectileTests.xml"));
            container2.ReadXmlFile();

            container2.StatesActions.Count.ShouldBe(1);
            container2.StatesActions[0].ActionModels.Count.ShouldBe(1);
            var model2 = container2.StatesActions[0].ActionModels[0] as ProjectileActionModel;
            model2.ShouldNotBeNull();
            model2.ActionType.ShouldBe(EActionType.Projectile);
            model2.Time.ShouldBe(time);
            model2.Scale.ShouldBe(scale);
            model2.StartOffset.ShouldBe(new Vector2(offsetX, offsetY));
            model2.Direction.DirectionType.ShouldBe(directionType);
            model2.Direction.Velocity.X.ShouldBe(X);
            model2.Direction.Velocity.Y.ShouldBe(Y);
        }
    }
}
