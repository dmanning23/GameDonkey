using FilenameBuddy;
using GameDonkeyLib;
using Microsoft.Xna.Framework;
using NUnit.Framework;
using Shouldly;

namespace GameDonkey.Tests
{
    [TestFixture]
    public class ElectricTests
    {
        const float time = 1f;
        static readonly Color electricColor = Color.Cyan;
        const float radius = 0.35f;
        const float duration = 1f;

        [SetUp]
        public void Setup()
        {
            TestHelpers.InitFilePaths();
        }

        [Test]
        public void ModelToAction()
        {
            var model = new ElectricActionModel()
            {
                Time = time,
                ElectricColor = electricColor,
                Radius = radius,
                Duration = duration
            };

            var action = new ElectricAction(null, model);

            action.Time.ShouldBe(time);
            action.ElectricColor.ShouldBe(electricColor);
            action.Radius.ShouldBe(radius);
            action.Duration.ShouldBe(duration);
        }

        [Test]
        public void ActionToModel()
        {
            var action = new ElectricAction(null)
            {
                Time = time,
                ElectricColor = electricColor,
                Radius = radius,
                Duration = duration
            };

            var model = new ElectricActionModel(action);

            model.ActionType.ShouldBe(EActionType.Electric);
            model.Time.ShouldBe(time);
            model.ElectricColor.ShouldBe(electricColor);
            model.Radius.ShouldBe(radius);
            model.Duration.ShouldBe(duration);
        }

        [Test]
        public void Persist()
        {
            var model = new ElectricActionModel()
            {
                Time = time,
                ElectricColor = electricColor,
                Radius = radius,
                Duration = duration
            };

            var container = new StateContainerModel(new Filename("ElectricTests.xml"));
            var actions = new SingleStateActionsModel();
            container.StatesActions.Add(actions);
            actions.ActionModels.Add(model);
            container.WriteXml();

            var container2 = new StateContainerModel(new Filename("ElectricTests.xml"));
            container2.ReadXmlFile();

            container2.StatesActions.Count.ShouldBe(1);
            container2.StatesActions[0].ActionModels.Count.ShouldBe(1);
            var model2 = container2.StatesActions[0].ActionModels[0] as ElectricActionModel;
            model2.ShouldNotBeNull();
            model2.ActionType.ShouldBe(EActionType.Electric);
            model2.Time.ShouldBe(time);
            model2.ElectricColor.ShouldBe(electricColor);
            model2.Radius.ShouldBe(radius);
            model2.Duration.ShouldBe(duration);
        }
    }
}
