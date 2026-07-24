using FilenameBuddy;
using GameDonkeyLib;
using NUnit.Framework;
using Shouldly;

namespace GameDonkey.Tests
{
    [TestFixture]
    public class ScreenTiltTests
    {
        const float time = 1f;
        const float angle = 6f;
        const float duration = 0.7f;
        const float delta = 0.35f;

        [SetUp]
        public void Setup()
        {
            TestHelpers.InitFilePaths();
        }

        [Test]
        public void ModelToAction()
        {
            var model = new ScreenTiltActionModel()
            {
                Time = time,
                Angle = angle,
                Duration = duration,
                Delta = delta
            };

            var action = new ScreenTiltAction(null, model);

            action.Time.ShouldBe(time);
            action.Angle.ShouldBe(angle);
            action.Duration.ShouldBe(duration);
            action.Delta.ShouldBe(delta);
        }

        [Test]
        public void ActionToModel()
        {
            var action = new ScreenTiltAction(null)
            {
                Time = time,
                Angle = angle,
                Duration = duration,
                Delta = delta
            };

            var model = new ScreenTiltActionModel(action);

            model.ActionType.ShouldBe(EActionType.ScreenTilt);
            model.Time.ShouldBe(time);
            model.Angle.ShouldBe(angle);
            model.Duration.ShouldBe(duration);
            model.Delta.ShouldBe(delta);
        }

        [Test]
        public void Persist()
        {
            var model = new ScreenTiltActionModel()
            {
                Time = time,
                Angle = angle,
                Duration = duration,
                Delta = delta
            };

            var container = new StateContainerModel(new Filename("ScreenTiltTests.xml"));
            var actions = new SingleStateActionsModel();
            container.StatesActions.Add(actions);
            actions.ActionModels.Add(model);
            container.WriteXml();

            var container2 = new StateContainerModel(new Filename("ScreenTiltTests.xml"));
            container2.ReadXmlFile();

            container2.StatesActions.Count.ShouldBe(1);
            container2.StatesActions[0].ActionModels.Count.ShouldBe(1);
            var model2 = container2.StatesActions[0].ActionModels[0] as ScreenTiltActionModel;
            model2.ShouldNotBeNull();
            model2.ActionType.ShouldBe(EActionType.ScreenTilt);
            model2.Time.ShouldBe(time);
            model2.Angle.ShouldBe(angle);
            model2.Duration.ShouldBe(duration);
            model2.Delta.ShouldBe(delta);
        }
    }
}
