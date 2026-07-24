using FilenameBuddy;
using GameDonkeyLib;
using NUnit.Framework;
using Shouldly;

namespace GameDonkey.Tests
{
    [TestFixture]
    public class HeatHazeTests
    {
        const float time = 1f;
        const float strength = 0.05f;
        const float radius = 0.3f;
        const float height = 0.6f;
        const float duration = 4.5f;

        [SetUp]
        public void Setup()
        {
            TestHelpers.InitFilePaths();
        }

        [Test]
        public void ModelToAction()
        {
            var model = new HeatHazeActionModel()
            {
                Time = time,
                Strength = strength,
                Radius = radius,
                Height = height,
                Duration = duration
            };

            var action = new HeatHazeAction(null, model);

            action.Time.ShouldBe(time);
            action.Strength.ShouldBe(strength);
            action.Radius.ShouldBe(radius);
            action.Height.ShouldBe(height);
            action.Duration.ShouldBe(duration);
        }

        [Test]
        public void ActionToModel()
        {
            var action = new HeatHazeAction(null)
            {
                Time = time,
                Strength = strength,
                Radius = radius,
                Height = height,
                Duration = duration
            };

            var model = new HeatHazeActionModel(action);

            model.ActionType.ShouldBe(EActionType.HeatHaze);
            model.Time.ShouldBe(time);
            model.Strength.ShouldBe(strength);
            model.Radius.ShouldBe(radius);
            model.Height.ShouldBe(height);
            model.Duration.ShouldBe(duration);
        }

        [Test]
        public void Persist()
        {
            var model = new HeatHazeActionModel()
            {
                Time = time,
                Strength = strength,
                Radius = radius,
                Height = height,
                Duration = duration
            };

            var container = new StateContainerModel(new Filename("HeatHazeTests.xml"));
            var actions = new SingleStateActionsModel();
            container.StatesActions.Add(actions);
            actions.ActionModels.Add(model);
            container.WriteXml();

            var container2 = new StateContainerModel(new Filename("HeatHazeTests.xml"));
            container2.ReadXmlFile();

            container2.StatesActions.Count.ShouldBe(1);
            container2.StatesActions[0].ActionModels.Count.ShouldBe(1);
            var model2 = container2.StatesActions[0].ActionModels[0] as HeatHazeActionModel;
            model2.ShouldNotBeNull();
            model2.ActionType.ShouldBe(EActionType.HeatHaze);
            model2.Time.ShouldBe(time);
            model2.Strength.ShouldBe(strength);
            model2.Radius.ShouldBe(radius);
            model2.Height.ShouldBe(height);
            model2.Duration.ShouldBe(duration);
        }
    }
}
