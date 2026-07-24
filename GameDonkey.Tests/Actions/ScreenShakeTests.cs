using FilenameBuddy;
using GameDonkeyLib;
using NUnit.Framework;
using Shouldly;

namespace GameDonkey.Tests
{
    [TestFixture]
    public class ScreenShakeTests
    {
        const float time = 1f;
        const float length = 1.5f;
        const float delta = 0.25f;
        const float amount = 0.4f;

        [SetUp]
        public void Setup()
        {
            TestHelpers.InitFilePaths();
        }

        [Test]
        public void ModelToAction()
        {
            var model = new ScreenShakeActionModel()
            {
                Time = time,
                Length = length,
                Delta = delta,
                Amount = amount
            };

            var action = new ScreenShakeAction(null, model);

            action.Time.ShouldBe(time);
            action.Length.ShouldBe(length);
            action.Delta.ShouldBe(delta);
            action.Amount.ShouldBe(amount);
        }

        [Test]
        public void ActionToModel()
        {
            var action = new ScreenShakeAction(null)
            {
                Time = time,
                Length = length,
                Delta = delta,
                Amount = amount
            };

            var model = new ScreenShakeActionModel(action);

            model.ActionType.ShouldBe(EActionType.ScreenShake);
            model.Time.ShouldBe(time);
            model.Length.ShouldBe(length);
            model.Delta.ShouldBe(delta);
            model.Amount.ShouldBe(amount);
        }

        [Test]
        public void Persist()
        {
            var model = new ScreenShakeActionModel()
            {
                Time = time,
                Length = length,
                Delta = delta,
                Amount = amount
            };

            var container = new StateContainerModel(new Filename("ScreenShakeTests.xml"));
            var actions = new SingleStateActionsModel();
            container.StatesActions.Add(actions);
            actions.ActionModels.Add(model);
            container.WriteXml();

            var container2 = new StateContainerModel(new Filename("ScreenShakeTests.xml"));
            container2.ReadXmlFile();

            container2.StatesActions.Count.ShouldBe(1);
            container2.StatesActions[0].ActionModels.Count.ShouldBe(1);
            var model2 = container2.StatesActions[0].ActionModels[0] as ScreenShakeActionModel;
            model2.ShouldNotBeNull();
            model2.ActionType.ShouldBe(EActionType.ScreenShake);
            model2.Time.ShouldBe(time);
            model2.Length.ShouldBe(length);
            model2.Delta.ShouldBe(delta);
            model2.Amount.ShouldBe(amount);
        }
    }
}
