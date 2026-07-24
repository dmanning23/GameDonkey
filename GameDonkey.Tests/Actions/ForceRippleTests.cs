using FilenameBuddy;
using GameDonkeyLib;
using NUnit.Framework;
using Shouldly;

namespace GameDonkey.Tests
{
    [TestFixture]
    public class ForceRippleTests
    {
        const float time = 1f;
        const float strength = 0.2f;
        const float speed = 0.6f;
        const float size = 0.15f;
        const float duration = 3.5f;

        [SetUp]
        public void Setup()
        {
            TestHelpers.InitFilePaths();
        }

        [Test]
        public void ModelToAction()
        {
            var model = new ForceRippleActionModel()
            {
                Time = time,
                Strength = strength,
                Speed = speed,
                Size = size,
                Duration = duration
            };

            var action = new ForceRippleAction(null, model);

            action.Time.ShouldBe(time);
            action.Strength.ShouldBe(strength);
            action.Speed.ShouldBe(speed);
            action.Size.ShouldBe(size);
            action.Duration.ShouldBe(duration);
        }

        [Test]
        public void ActionToModel()
        {
            var action = new ForceRippleAction(null)
            {
                Time = time,
                Strength = strength,
                Speed = speed,
                Size = size,
                Duration = duration
            };

            var model = new ForceRippleActionModel(action);

            model.ActionType.ShouldBe(EActionType.ForceRipple);
            model.Time.ShouldBe(time);
            model.Strength.ShouldBe(strength);
            model.Speed.ShouldBe(speed);
            model.Size.ShouldBe(size);
            model.Duration.ShouldBe(duration);
        }

        [Test]
        public void Persist()
        {
            var model = new ForceRippleActionModel()
            {
                Time = time,
                Strength = strength,
                Speed = speed,
                Size = size,
                Duration = duration
            };

            var container = new StateContainerModel(new Filename("ForceRippleTests.xml"));
            var actions = new SingleStateActionsModel();
            container.StatesActions.Add(actions);
            actions.ActionModels.Add(model);
            container.WriteXml();

            var container2 = new StateContainerModel(new Filename("ForceRippleTests.xml"));
            container2.ReadXmlFile();

            container2.StatesActions.Count.ShouldBe(1);
            container2.StatesActions[0].ActionModels.Count.ShouldBe(1);
            var model2 = container2.StatesActions[0].ActionModels[0] as ForceRippleActionModel;
            model2.ShouldNotBeNull();
            model2.ActionType.ShouldBe(EActionType.ForceRipple);
            model2.Time.ShouldBe(time);
            model2.Strength.ShouldBe(strength);
            model2.Speed.ShouldBe(speed);
            model2.Size.ShouldBe(size);
            model2.Duration.ShouldBe(duration);
        }
    }
}
