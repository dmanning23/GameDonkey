using FilenameBuddy;
using GameDonkeyLib;
using Microsoft.Xna.Framework;
using NUnit.Framework;
using Shouldly;

namespace GameDonkey.Tests
{
    [TestFixture]
    public class SmokeTests
    {
        const float time = 1f;
        static readonly Color smokeColor = Color.DarkGray;
        const float radius = 0.25f;
        const float duration = 3f;

        [SetUp]
        public void Setup()
        {
            TestHelpers.InitFilePaths();
        }

        [Test]
        public void ModelToAction()
        {
            var model = new SmokeActionModel()
            {
                Time = time,
                SmokeColor = smokeColor,
                Radius = radius,
                Duration = duration
            };

            var action = new SmokeAction(null, model);

            action.Time.ShouldBe(time);
            action.SmokeColor.ShouldBe(smokeColor);
            action.Radius.ShouldBe(radius);
            action.Duration.ShouldBe(duration);
        }

        [Test]
        public void ActionToModel()
        {
            var action = new SmokeAction(null)
            {
                Time = time,
                SmokeColor = smokeColor,
                Radius = radius,
                Duration = duration
            };

            var model = new SmokeActionModel(action);

            model.ActionType.ShouldBe(EActionType.Smoke);
            model.Time.ShouldBe(time);
            model.SmokeColor.ShouldBe(smokeColor);
            model.Radius.ShouldBe(radius);
            model.Duration.ShouldBe(duration);
        }

        [Test]
        public void Persist()
        {
            var model = new SmokeActionModel()
            {
                Time = time,
                SmokeColor = smokeColor,
                Radius = radius,
                Duration = duration
            };

            var container = new StateContainerModel(new Filename("SmokeTests.xml"));
            var actions = new SingleStateActionsModel();
            container.StatesActions.Add(actions);
            actions.ActionModels.Add(model);
            container.WriteXml();

            var container2 = new StateContainerModel(new Filename("SmokeTests.xml"));
            container2.ReadXmlFile();

            container2.StatesActions.Count.ShouldBe(1);
            container2.StatesActions[0].ActionModels.Count.ShouldBe(1);
            var model2 = container2.StatesActions[0].ActionModels[0] as SmokeActionModel;
            model2.ShouldNotBeNull();
            model2.ActionType.ShouldBe(EActionType.Smoke);
            model2.Time.ShouldBe(time);
            model2.SmokeColor.ShouldBe(smokeColor);
            model2.Radius.ShouldBe(radius);
            model2.Duration.ShouldBe(duration);
        }
    }
}
