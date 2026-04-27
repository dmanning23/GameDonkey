using FilenameBuddy;
using GameDonkeyLib;
using Microsoft.Xna.Framework;
using NUnit.Framework;
using Shouldly;

namespace GameDonkey.Tests
{
    [TestFixture]
    public class TrailTests
    {
        const float time = 1f;
        const float timeDelta = 2f;
        const float lifeDelta = 3f;
        const float spawnDelta = 1.5f;

        [SetUp]
        public void Setup()
        {
            TestHelpers.InitFilePaths();
        }

        [Test]
        public void ModelToAction_SetsProperties()
        {
            var model = new TrailActionModel();

            var action = new TrailAction(null, model);

            action.StartColor.ShouldBe(Color.White);
            action.TrailLifeDelta.ShouldBe(0f);
            action.SpawnDelta.ShouldBe(0f);
        }

        [Test]
        public void ActionToModel_SetsProperties()
        {
            var action = new TrailAction(null)
            {
                Time = time,
                TimeDelta = timeDelta,
                StartColor = Color.Red,
                TrailLifeDelta = lifeDelta,
                SpawnDelta = spawnDelta
            };

            var model = new TrailActionModel(action);

            model.ActionType.ShouldBe(EActionType.Trail);
            model.Time.ShouldBe(time);
            model.TimeDelta.TimeDelta.ShouldBe(timeDelta);
            model.Color.ShouldBe(Color.Red);
            model.LifeDelta.ShouldBe(lifeDelta);
            model.SpawnDelta.ShouldBe(spawnDelta);
        }

        [Test]
        public void Persist()
        {
            var action = new TrailAction(null)
            {
                Time = time,
                TimeDelta = timeDelta,
                StartColor = Color.Red,
                TrailLifeDelta = lifeDelta,
                SpawnDelta = spawnDelta
            };
            var model = new TrailActionModel(action);

            var container = new StateContainerModel(new Filename("TrailTests.xml"));
            var stateActions = new SingleStateActionsModel();
            container.StatesActions.Add(stateActions);
            stateActions.ActionModels.Add(model);
            container.WriteXml();

            var container2 = new StateContainerModel(new Filename("TrailTests.xml"));
            container2.ReadXmlFile();

            container2.StatesActions.Count.ShouldBe(1);
            container2.StatesActions[0].ActionModels.Count.ShouldBe(1);
            var model2 = container2.StatesActions[0].ActionModels[0] as TrailActionModel;
            model2.ShouldNotBeNull();
            model2.ActionType.ShouldBe(EActionType.Trail);
            model2.Time.ShouldBe(time);
            model2.TimeDelta.TimeDelta.ShouldBe(timeDelta);
            model2.Color.ShouldBe(Color.Red);
            model2.LifeDelta.ShouldBe(lifeDelta);
            model2.SpawnDelta.ShouldBe(spawnDelta);
        }

        [Test]
        public void Execute_SetsOwnerTrailAction()
        {
            var owner = new TestObject();
            var action = new TrailAction(owner) { SpawnDelta = 2f };

            action.Execute(0f);

            owner.TrailAction.ShouldBeSameAs(action);
            action.AlreadyRun.ShouldBeTrue();
        }
    }
}
