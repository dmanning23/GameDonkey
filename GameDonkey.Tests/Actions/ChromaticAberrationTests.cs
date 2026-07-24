using FilenameBuddy;
using GameDonkeyLib;
using NUnit.Framework;
using ScreenFXBuddy;
using Shouldly;

namespace GameDonkey.Tests
{
    [TestFixture]
    public class ChromaticAberrationTests
    {
        const float time = 1f;
        const float distance = 0.3f;
        const float duration = 1.2f;
        const FadeCurve curve = FadeCurve.Exponential;

        [SetUp]
        public void Setup()
        {
            TestHelpers.InitFilePaths();
        }

        [Test]
        public void ModelToAction()
        {
            var model = new ChromaticAberrationActionModel()
            {
                Time = time,
                Distance = distance,
                Duration = duration,
                Curve = curve
            };

            var action = new ChromaticAberrationAction(null, model);

            action.Time.ShouldBe(time);
            action.Distance.ShouldBe(distance);
            action.Duration.ShouldBe(duration);
            action.Curve.ShouldBe(curve);
        }

        [Test]
        public void ActionToModel()
        {
            var action = new ChromaticAberrationAction(null)
            {
                Time = time,
                Distance = distance,
                Duration = duration,
                Curve = curve
            };

            var model = new ChromaticAberrationActionModel(action);

            model.ActionType.ShouldBe(EActionType.ChromaticAberration);
            model.Time.ShouldBe(time);
            model.Distance.ShouldBe(distance);
            model.Duration.ShouldBe(duration);
            model.Curve.ShouldBe(curve);
        }

        [Test]
        public void Persist()
        {
            var model = new ChromaticAberrationActionModel()
            {
                Time = time,
                Distance = distance,
                Duration = duration,
                Curve = curve
            };

            var container = new StateContainerModel(new Filename("ChromaticAberrationTests.xml"));
            var actions = new SingleStateActionsModel();
            container.StatesActions.Add(actions);
            actions.ActionModels.Add(model);
            container.WriteXml();

            var container2 = new StateContainerModel(new Filename("ChromaticAberrationTests.xml"));
            container2.ReadXmlFile();

            container2.StatesActions.Count.ShouldBe(1);
            container2.StatesActions[0].ActionModels.Count.ShouldBe(1);
            var model2 = container2.StatesActions[0].ActionModels[0] as ChromaticAberrationActionModel;
            model2.ShouldNotBeNull();
            model2.ActionType.ShouldBe(EActionType.ChromaticAberration);
            model2.Time.ShouldBe(time);
            model2.Distance.ShouldBe(distance);
            model2.Duration.ShouldBe(duration);
            model2.Curve.ShouldBe(curve);
        }
    }
}
