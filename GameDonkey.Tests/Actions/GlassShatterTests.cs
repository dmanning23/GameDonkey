using FilenameBuddy;
using GameDonkeyLib;
using NUnit.Framework;
using Shouldly;

namespace GameDonkey.Tests
{
    [TestFixture]
    public class GlassShatterTests
    {
        const float time = 1f;
        const float strength = 0.08f;
        const int numCells = 35;
        const float duration = 1.2f;

        [SetUp]
        public void Setup()
        {
            TestHelpers.InitFilePaths();
        }

        [Test]
        public void ModelToAction()
        {
            var model = new GlassShatterActionModel()
            {
                Time = time,
                Strength = strength,
                NumCells = numCells,
                Duration = duration
            };

            var action = new GlassShatterAction(null, model);

            action.Time.ShouldBe(time);
            action.Strength.ShouldBe(strength);
            action.NumCells.ShouldBe(numCells);
            action.Duration.ShouldBe(duration);
        }

        [Test]
        public void ActionToModel()
        {
            var action = new GlassShatterAction(null)
            {
                Time = time,
                Strength = strength,
                NumCells = numCells,
                Duration = duration
            };

            var model = new GlassShatterActionModel(action);

            model.ActionType.ShouldBe(EActionType.GlassShatter);
            model.Time.ShouldBe(time);
            model.Strength.ShouldBe(strength);
            model.NumCells.ShouldBe(numCells);
            model.Duration.ShouldBe(duration);
        }

        [Test]
        public void Persist()
        {
            var model = new GlassShatterActionModel()
            {
                Time = time,
                Strength = strength,
                NumCells = numCells,
                Duration = duration
            };

            var container = new StateContainerModel(new Filename("GlassShatterTests.xml"));
            var actions = new SingleStateActionsModel();
            container.StatesActions.Add(actions);
            actions.ActionModels.Add(model);
            container.WriteXml();

            var container2 = new StateContainerModel(new Filename("GlassShatterTests.xml"));
            container2.ReadXmlFile();

            container2.StatesActions.Count.ShouldBe(1);
            container2.StatesActions[0].ActionModels.Count.ShouldBe(1);
            var model2 = container2.StatesActions[0].ActionModels[0] as GlassShatterActionModel;
            model2.ShouldNotBeNull();
            model2.ActionType.ShouldBe(EActionType.GlassShatter);
            model2.Time.ShouldBe(time);
            model2.Strength.ShouldBe(strength);
            model2.NumCells.ShouldBe(numCells);
            model2.Duration.ShouldBe(duration);
        }
    }
}
