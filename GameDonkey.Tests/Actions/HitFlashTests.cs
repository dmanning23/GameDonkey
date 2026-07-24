using FilenameBuddy;
using GameDonkeyLib;
using Microsoft.Xna.Framework;
using NUnit.Framework;
using ScreenFXBuddy;
using Shouldly;

namespace GameDonkey.Tests
{
    [TestFixture]
    public class HitFlashTests
    {
        const float time = 1f;
        static readonly Color blendColor = Color.Red;
        const FadeMode mode = FadeMode.FadeIn;
        const FadeCurve curve = FadeCurve.Logarithmic;
        const EffectBlendMode blendMode = EffectBlendMode.Screen;
        const float duration = 2f;

        [SetUp]
        public void Setup()
        {
            TestHelpers.InitFilePaths();
        }

        [Test]
        public void ModelToAction()
        {
            var model = new HitFlashActionModel()
            {
                Time = time,
                BlendColor = blendColor,
                Mode = mode,
                Curve = curve,
                BlendMode = blendMode,
                Duration = duration
            };

            var action = new HitFlashAction(null, model);

            action.Time.ShouldBe(time);
            action.BlendColor.ShouldBe(blendColor);
            action.Mode.ShouldBe(mode);
            action.Curve.ShouldBe(curve);
            action.BlendMode.ShouldBe(blendMode);
            action.Duration.ShouldBe(duration);
        }

        [Test]
        public void ActionToModel()
        {
            var action = new HitFlashAction(null)
            {
                Time = time,
                BlendColor = blendColor,
                Mode = mode,
                Curve = curve,
                BlendMode = blendMode,
                Duration = duration
            };

            var model = new HitFlashActionModel(action);

            model.ActionType.ShouldBe(EActionType.HitFlash);
            model.Time.ShouldBe(time);
            model.BlendColor.ShouldBe(blendColor);
            model.Mode.ShouldBe(mode);
            model.Curve.ShouldBe(curve);
            model.BlendMode.ShouldBe(blendMode);
            model.Duration.ShouldBe(duration);
        }

        [Test]
        public void Persist()
        {
            var model = new HitFlashActionModel()
            {
                Time = time,
                BlendColor = blendColor,
                Mode = mode,
                Curve = curve,
                BlendMode = blendMode,
                Duration = duration
            };

            var container = new StateContainerModel(new Filename("HitFlashTests.xml"));
            var actions = new SingleStateActionsModel();
            container.StatesActions.Add(actions);
            actions.ActionModels.Add(model);
            container.WriteXml();

            var container2 = new StateContainerModel(new Filename("HitFlashTests.xml"));
            container2.ReadXmlFile();

            container2.StatesActions.Count.ShouldBe(1);
            container2.StatesActions[0].ActionModels.Count.ShouldBe(1);
            var model2 = container2.StatesActions[0].ActionModels[0] as HitFlashActionModel;
            model2.ShouldNotBeNull();
            model2.ActionType.ShouldBe(EActionType.HitFlash);
            model2.Time.ShouldBe(time);
            model2.BlendColor.ShouldBe(blendColor);
            model2.Mode.ShouldBe(mode);
            model2.Curve.ShouldBe(curve);
            model2.BlendMode.ShouldBe(blendMode);
            model2.Duration.ShouldBe(duration);
        }
    }
}
