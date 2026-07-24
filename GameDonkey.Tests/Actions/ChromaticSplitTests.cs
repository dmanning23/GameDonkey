using FilenameBuddy;
using GameDonkeyLib;
using NUnit.Framework;
using Shouldly;

namespace GameDonkey.Tests
{
    [TestFixture]
    public class ChromaticSplitTests
    {
        const float time = 1f;
        const float maxDistance = 0.12f;
        const float duration = 0.6f;

        [SetUp]
        public void Setup()
        {
            TestHelpers.InitFilePaths();
        }

        [Test]
        public void ModelToAction()
        {
            var model = new ChromaticSplitActionModel()
            {
                Time = time,
                MaxDistance = maxDistance,
                Duration = duration
            };

            var action = new ChromaticSplitAction(null, model);

            action.Time.ShouldBe(time);
            action.MaxDistance.ShouldBe(maxDistance);
            action.Duration.ShouldBe(duration);
        }

        [Test]
        public void ActionToModel()
        {
            var action = new ChromaticSplitAction(null)
            {
                Time = time,
                MaxDistance = maxDistance,
                Duration = duration
            };

            var model = new ChromaticSplitActionModel(action);

            model.ActionType.ShouldBe(EActionType.ChromaticSplit);
            model.Time.ShouldBe(time);
            model.MaxDistance.ShouldBe(maxDistance);
            model.Duration.ShouldBe(duration);
        }

        [Test]
        public void Persist()
        {
            var model = new ChromaticSplitActionModel()
            {
                Time = time,
                MaxDistance = maxDistance,
                Duration = duration
            };

            var container = new StateContainerModel(new Filename("ChromaticSplitTests.xml"));
            var actions = new SingleStateActionsModel();
            container.StatesActions.Add(actions);
            actions.ActionModels.Add(model);
            container.WriteXml();

            var container2 = new StateContainerModel(new Filename("ChromaticSplitTests.xml"));
            container2.ReadXmlFile();

            container2.StatesActions.Count.ShouldBe(1);
            container2.StatesActions[0].ActionModels.Count.ShouldBe(1);
            var model2 = container2.StatesActions[0].ActionModels[0] as ChromaticSplitActionModel;
            model2.ShouldNotBeNull();
            model2.ActionType.ShouldBe(EActionType.ChromaticSplit);
            model2.Time.ShouldBe(time);
            model2.MaxDistance.ShouldBe(maxDistance);
            model2.Duration.ShouldBe(duration);
        }
    }
}
