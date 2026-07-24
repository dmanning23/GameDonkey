using FilenameBuddy;
using GameDonkeyLib;
using Microsoft.Xna.Framework;
using NUnit.Framework;
using Shouldly;

namespace GameDonkey.Tests
{
    [TestFixture]
    public class FrostTests
    {
        const float time = 1f;
        static readonly Color tintColor = Color.LightBlue;
        const float radius = 0.4f;
        const float duration = 2.5f;

        [SetUp]
        public void Setup()
        {
            TestHelpers.InitFilePaths();
        }

        [Test]
        public void ModelToAction()
        {
            var model = new FrostActionModel()
            {
                Time = time,
                TintColor = tintColor,
                Radius = radius,
                Duration = duration
            };

            var action = new FrostAction(null, model);

            action.Time.ShouldBe(time);
            action.TintColor.ShouldBe(tintColor);
            action.Radius.ShouldBe(radius);
            action.Duration.ShouldBe(duration);
        }

        [Test]
        public void ActionToModel()
        {
            var action = new FrostAction(null)
            {
                Time = time,
                TintColor = tintColor,
                Radius = radius,
                Duration = duration
            };

            var model = new FrostActionModel(action);

            model.ActionType.ShouldBe(EActionType.Frost);
            model.Time.ShouldBe(time);
            model.TintColor.ShouldBe(tintColor);
            model.Radius.ShouldBe(radius);
            model.Duration.ShouldBe(duration);
        }

        [Test]
        public void Persist()
        {
            var model = new FrostActionModel()
            {
                Time = time,
                TintColor = tintColor,
                Radius = radius,
                Duration = duration
            };

            var container = new StateContainerModel(new Filename("FrostTests.xml"));
            var actions = new SingleStateActionsModel();
            container.StatesActions.Add(actions);
            actions.ActionModels.Add(model);
            container.WriteXml();

            var container2 = new StateContainerModel(new Filename("FrostTests.xml"));
            container2.ReadXmlFile();

            container2.StatesActions.Count.ShouldBe(1);
            container2.StatesActions[0].ActionModels.Count.ShouldBe(1);
            var model2 = container2.StatesActions[0].ActionModels[0] as FrostActionModel;
            model2.ShouldNotBeNull();
            model2.ActionType.ShouldBe(EActionType.Frost);
            model2.Time.ShouldBe(time);
            model2.TintColor.ShouldBe(tintColor);
            model2.Radius.ShouldBe(radius);
            model2.Duration.ShouldBe(duration);
        }
    }
}
