using FilenameBuddy;
using GameDonkeyLib;
using NUnit.Framework;
using Shouldly;

namespace GameDonkey.Tests
{
    [TestFixture]
    public class RandomActionTests
    {
        const float time = 1f;
        const float childTime = 0f;
        const float rotation = 2.5f;

        [SetUp]
        public void Setup()
        {
            TestHelpers.InitFilePaths();
        }

        [Test]
        public void ModelToAction_SetsTime()
        {
            var model = new RandomActionModel() { Time = time };
            model.ActionModels.ActionModels.Add(new RotateActionModel() { Time = childTime });

            var action = new RandomAction(null, model, null);

            action.Time.ShouldBe(time);
            action.Actions.Count.ShouldBe(1);
        }

        [Test]
        public void ActionToModel_SetsActionType()
        {
            var action = new RandomAction(null) { Time = time };
            action.Actions.Add(new RotateAction(null) { Time = childTime });

            var model = new RandomActionModel(action);

            model.ActionType.ShouldBe(EActionType.Random);
            model.Time.ShouldBe(time);
            model.ActionModels.ActionModels.Count.ShouldBe(1);
        }

        [Test]
        public void Persist()
        {
            var model = new RandomActionModel() { Time = time };
            model.ActionModels.ActionModels.Add(new RotateActionModel() { Time = childTime });

            var container = new StateContainerModel(new Filename("RandomActionTests.xml"));
            var actions = new SingleStateActionsModel();
            container.StatesActions.Add(actions);
            actions.ActionModels.Add(model);
            container.WriteXml();

            var container2 = new StateContainerModel(new Filename("RandomActionTests.xml"));
            container2.ReadXmlFile();

            container2.StatesActions.Count.ShouldBe(1);
            container2.StatesActions[0].ActionModels.Count.ShouldBe(1);
            var model2 = container2.StatesActions[0].ActionModels[0] as RandomActionModel;
            model2.ShouldNotBeNull();
            model2.ActionType.ShouldBe(EActionType.Random);
            model2.Time.ShouldBe(time);
            model2.ActionModels.ActionModels.Count.ShouldBe(1);
        }

        [Test]
        public void Execute_WithSingleAction_RunsThatAction()
        {
            var owner = new TestObject();
            var action = new RandomAction(owner);
            var child = new RotateAction(owner) { Rotation = rotation };
            action.Actions.Add(child);

            action.Execute(0f);

            owner.RotationPerSecond.ShouldBe(rotation);
            action.AlreadyRun.ShouldBeTrue();
        }
    }
}
