using FilenameBuddy;
using GameDonkeyLib;
using NUnit.Framework;
using ScreenFXBuddy;
using Shouldly;

namespace GameDonkey.Tests
{
    [TestFixture]
    public class VortexTests
    {
        const float time = 1f;
        const float radius = 0.4f;
        const float speed = 1.5f;
        const float spinInTime = 0.5f;
        const float spinOutTime = 0.6f;
        const FadeCurve vortexFadeCurve = FadeCurve.Exponential;
        const bool clockwise = false;

        [SetUp]
        public void Setup()
        {
            TestHelpers.InitFilePaths();
        }

        [Test]
        public void ModelToAction()
        {
            var model = new VortexActionModel()
            {
                Time = time,
                Radius = radius,
                Speed = speed,
                SpinInTime = spinInTime,
                SpinOutTime = spinOutTime,
                VortexFadeCurve = vortexFadeCurve,
                Clockwise = clockwise
            };

            var action = new VortexAction(null, model);

            action.Time.ShouldBe(time);
            action.Radius.ShouldBe(radius);
            action.Speed.ShouldBe(speed);
            action.SpinInTime.ShouldBe(spinInTime);
            action.SpinOutTime.ShouldBe(spinOutTime);
            action.VortexFadeCurve.ShouldBe(vortexFadeCurve);
            action.Clockwise.ShouldBe(clockwise);
        }

        [Test]
        public void ActionToModel()
        {
            var action = new VortexAction(null)
            {
                Time = time,
                Radius = radius,
                Speed = speed,
                SpinInTime = spinInTime,
                SpinOutTime = spinOutTime,
                VortexFadeCurve = vortexFadeCurve,
                Clockwise = clockwise
            };

            var model = new VortexActionModel(action);

            model.ActionType.ShouldBe(EActionType.Vortex);
            model.Time.ShouldBe(time);
            model.Radius.ShouldBe(radius);
            model.Speed.ShouldBe(speed);
            model.SpinInTime.ShouldBe(spinInTime);
            model.SpinOutTime.ShouldBe(spinOutTime);
            model.VortexFadeCurve.ShouldBe(vortexFadeCurve);
            model.Clockwise.ShouldBe(clockwise);
        }

        [Test]
        public void Persist()
        {
            var model = new VortexActionModel()
            {
                Time = time,
                Radius = radius,
                Speed = speed,
                SpinInTime = spinInTime,
                SpinOutTime = spinOutTime,
                VortexFadeCurve = vortexFadeCurve,
                Clockwise = clockwise
            };

            var container = new StateContainerModel(new Filename("VortexTests.xml"));
            var actions = new SingleStateActionsModel();
            container.StatesActions.Add(actions);
            actions.ActionModels.Add(model);
            container.WriteXml();

            var container2 = new StateContainerModel(new Filename("VortexTests.xml"));
            container2.ReadXmlFile();

            container2.StatesActions.Count.ShouldBe(1);
            container2.StatesActions[0].ActionModels.Count.ShouldBe(1);
            var model2 = container2.StatesActions[0].ActionModels[0] as VortexActionModel;
            model2.ShouldNotBeNull();
            model2.ActionType.ShouldBe(EActionType.Vortex);
            model2.Time.ShouldBe(time);
            model2.Radius.ShouldBe(radius);
            model2.Speed.ShouldBe(speed);
            model2.SpinInTime.ShouldBe(spinInTime);
            model2.SpinOutTime.ShouldBe(spinOutTime);
            model2.VortexFadeCurve.ShouldBe(vortexFadeCurve);
            model2.Clockwise.ShouldBe(clockwise);
        }
    }
}
