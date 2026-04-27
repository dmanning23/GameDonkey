using FilenameBuddy;
using GameDonkeyLib;
using Microsoft.Xna.Framework;
using NUnit.Framework;
using Shouldly;

namespace GameDonkey.Tests
{
    [TestFixture]
    public class PointLightTests
    {
        const float time = 1f;
        const string bone = "hip";
        const float attack = 0.5f;
        const float sustain = 1.0f;
        const float delay = 1.5f;
        const float flare = 0.1f;
        const float minBrightness = 50f;
        const float maxBrightness = 200f;

        [SetUp]
        public void Setup()
        {
            TestHelpers.InitFilePaths();
        }

        [Test]
        public void ModelToAction_SetsProperties()
        {
            var model = new PointLightActionModel()
            {
                Time = time,
                Bone = bone,
                StartOffset = new Vector3(1f, 2f, 3f),
                LightColor = Color.Blue,
                AttackTimeDelta = attack,
                SustainTimeDelta = sustain,
                DelayTimeDelta = delay,
                FlareTimeDelta = flare,
                MinBrightness = minBrightness,
                MaxBrightness = maxBrightness
            };

            var action = new PointLightAction(null, model);

            action.Time.ShouldBe(time);
            action.BoneName.ShouldBe(bone);
            action.StartOffset.ShouldBe(new Vector3(1f, 2f, 3f));
            action.LightColor.ShouldBe(Color.Blue);
            action.AttackTimeDelta.ShouldBe(attack);
            action.SustainTimeDelta.ShouldBe(sustain);
            action.DelayTimeDelta.ShouldBe(delay);
            action.FlareTimeDelta.ShouldBe(flare);
            action.MinBrightness.ShouldBe(minBrightness);
            action.MaxBrightness.ShouldBe(maxBrightness);
        }

        [Test]
        public void ActionToModel_SetsProperties()
        {
            var action = new PointLightAction(null)
            {
                Time = time,
                BoneName = bone,
                StartOffset = new Vector3(1f, 2f, 3f),
                LightColor = Color.Blue,
                AttackTimeDelta = attack,
                SustainTimeDelta = sustain,
                DelayTimeDelta = delay,
                FlareTimeDelta = flare,
                MinBrightness = minBrightness,
                MaxBrightness = maxBrightness
            };

            var model = new PointLightActionModel(action);

            model.ActionType.ShouldBe(EActionType.PointLight);
            model.Time.ShouldBe(time);
            model.Bone.ShouldBe(bone);
            model.StartOffset.ShouldBe(new Vector3(1f, 2f, 3f));
            model.LightColor.ShouldBe(Color.Blue);
            model.AttackTimeDelta.ShouldBe(attack);
            model.SustainTimeDelta.ShouldBe(sustain);
            model.DelayTimeDelta.ShouldBe(delay);
            model.FlareTimeDelta.ShouldBe(flare);
            model.MinBrightness.ShouldBe(minBrightness);
            model.MaxBrightness.ShouldBe(maxBrightness);
        }

        [Test]
        public void Persist()
        {
            var model = new PointLightActionModel()
            {
                Time = time,
                Bone = bone,
                StartOffset = new Vector3(1f, 2f, 3f),
                LightColor = Color.Blue,
                AttackTimeDelta = attack,
                SustainTimeDelta = sustain,
                DelayTimeDelta = delay,
                FlareTimeDelta = flare,
                MinBrightness = minBrightness,
                MaxBrightness = maxBrightness
            };

            var container = new StateContainerModel(new Filename("PointLightTests.xml"));
            var actions = new SingleStateActionsModel();
            container.StatesActions.Add(actions);
            actions.ActionModels.Add(model);
            container.WriteXml();

            var container2 = new StateContainerModel(new Filename("PointLightTests.xml"));
            container2.ReadXmlFile();

            container2.StatesActions.Count.ShouldBe(1);
            container2.StatesActions[0].ActionModels.Count.ShouldBe(1);
            var model2 = container2.StatesActions[0].ActionModels[0] as PointLightActionModel;
            model2.ShouldNotBeNull();
            model2.ActionType.ShouldBe(EActionType.PointLight);
            model2.Time.ShouldBe(time);
            model2.Bone.ShouldBe(bone);
            model2.StartOffset.ShouldBe(new Vector3(1f, 2f, 3f));
            model2.LightColor.ShouldBe(Color.Blue);
            model2.AttackTimeDelta.ShouldBe(attack);
            model2.SustainTimeDelta.ShouldBe(sustain);
            model2.DelayTimeDelta.ShouldBe(delay);
            model2.FlareTimeDelta.ShouldBe(flare);
            model2.MinBrightness.ShouldBe(minBrightness);
            model2.MaxBrightness.ShouldBe(maxBrightness);
        }
    }
}
