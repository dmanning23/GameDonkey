using FilenameBuddy;
using GameDonkeyLib;
using NUnit.Framework;
using Shouldly;

namespace GameDonkey.Tests
{
    [TestFixture]
    public class GravityWaveTests
    {
        const float time = 1f;
        const float strength = 0.1f;
        const float startHeight = 0.15f;
        const float endHeight = 0.5f;
        const float speed = 0.8f;
        const float duration = 2.2f;

        [SetUp]
        public void Setup()
        {
            TestHelpers.InitFilePaths();
        }

        [Test]
        public void ModelToAction()
        {
            var model = new GravityWaveActionModel()
            {
                Time = time,
                Strength = strength,
                StartHeight = startHeight,
                EndHeight = endHeight,
                Speed = speed,
                Duration = duration
            };

            var action = new GravityWaveAction(null, model);

            action.Time.ShouldBe(time);
            action.Strength.ShouldBe(strength);
            action.StartHeight.ShouldBe(startHeight);
            action.EndHeight.ShouldBe(endHeight);
            action.Speed.ShouldBe(speed);
            action.Duration.ShouldBe(duration);
        }

        [Test]
        public void ActionToModel()
        {
            var action = new GravityWaveAction(null)
            {
                Time = time,
                Strength = strength,
                StartHeight = startHeight,
                EndHeight = endHeight,
                Speed = speed,
                Duration = duration
            };

            var model = new GravityWaveActionModel(action);

            model.ActionType.ShouldBe(EActionType.GravityWave);
            model.Time.ShouldBe(time);
            model.Strength.ShouldBe(strength);
            model.StartHeight.ShouldBe(startHeight);
            model.EndHeight.ShouldBe(endHeight);
            model.Speed.ShouldBe(speed);
            model.Duration.ShouldBe(duration);
        }

        [Test]
        public void Persist()
        {
            var model = new GravityWaveActionModel()
            {
                Time = time,
                Strength = strength,
                StartHeight = startHeight,
                EndHeight = endHeight,
                Speed = speed,
                Duration = duration
            };

            var container = new StateContainerModel(new Filename("GravityWaveTests.xml"));
            var actions = new SingleStateActionsModel();
            container.StatesActions.Add(actions);
            actions.ActionModels.Add(model);
            container.WriteXml();

            var container2 = new StateContainerModel(new Filename("GravityWaveTests.xml"));
            container2.ReadXmlFile();

            container2.StatesActions.Count.ShouldBe(1);
            container2.StatesActions[0].ActionModels.Count.ShouldBe(1);
            var model2 = container2.StatesActions[0].ActionModels[0] as GravityWaveActionModel;
            model2.ShouldNotBeNull();
            model2.ActionType.ShouldBe(EActionType.GravityWave);
            model2.Time.ShouldBe(time);
            model2.Strength.ShouldBe(strength);
            model2.StartHeight.ShouldBe(startHeight);
            model2.EndHeight.ShouldBe(endHeight);
            model2.Speed.ShouldBe(speed);
            model2.Duration.ShouldBe(duration);
        }
    }
}
