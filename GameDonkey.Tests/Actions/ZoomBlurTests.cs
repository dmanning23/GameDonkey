using FilenameBuddy;
using GameDonkeyLib;
using NUnit.Framework;
using Shouldly;

namespace GameDonkey.Tests
{
    [TestFixture]
    public class ZoomBlurTests
    {
        const float time = 1f;
        const float strength = 0.15f;
        const float radius = 1.5f;
        const float duration = 0.8f;

        [SetUp]
        public void Setup()
        {
            TestHelpers.InitFilePaths();
        }

        [Test]
        public void ModelToAction()
        {
            var model = new ZoomBlurActionModel()
            {
                Time = time,
                Strength = strength,
                Radius = radius,
                Duration = duration
            };

            var action = new ZoomBlurAction(null, model);

            action.Time.ShouldBe(time);
            action.Strength.ShouldBe(strength);
            action.Radius.ShouldBe(radius);
            action.Duration.ShouldBe(duration);
        }

        [Test]
        public void ActionToModel()
        {
            var action = new ZoomBlurAction(null)
            {
                Time = time,
                Strength = strength,
                Radius = radius,
                Duration = duration
            };

            var model = new ZoomBlurActionModel(action);

            model.ActionType.ShouldBe(EActionType.ZoomBlur);
            model.Time.ShouldBe(time);
            model.Strength.ShouldBe(strength);
            model.Radius.ShouldBe(radius);
            model.Duration.ShouldBe(duration);
        }

        [Test]
        public void Persist()
        {
            var model = new ZoomBlurActionModel()
            {
                Time = time,
                Strength = strength,
                Radius = radius,
                Duration = duration
            };

            var container = new StateContainerModel(new Filename("ZoomBlurTests.xml"));
            var actions = new SingleStateActionsModel();
            container.StatesActions.Add(actions);
            actions.ActionModels.Add(model);
            container.WriteXml();

            var container2 = new StateContainerModel(new Filename("ZoomBlurTests.xml"));
            container2.ReadXmlFile();

            container2.StatesActions.Count.ShouldBe(1);
            container2.StatesActions[0].ActionModels.Count.ShouldBe(1);
            var model2 = container2.StatesActions[0].ActionModels[0] as ZoomBlurActionModel;
            model2.ShouldNotBeNull();
            model2.ActionType.ShouldBe(EActionType.ZoomBlur);
            model2.Time.ShouldBe(time);
            model2.Strength.ShouldBe(strength);
            model2.Radius.ShouldBe(radius);
            model2.Duration.ShouldBe(duration);
        }
    }
}
