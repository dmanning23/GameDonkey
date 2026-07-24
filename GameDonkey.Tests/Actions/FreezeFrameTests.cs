using FilenameBuddy;
using GameDonkeyLib;
using Microsoft.Xna.Framework;
using NUnit.Framework;
using Shouldly;

namespace GameDonkey.Tests
{
    [TestFixture]
    public class FreezeFrameTests
    {
        const float time = 1f;
        static readonly Color tintColor = Color.Gray;
        const float flashIn = 0.2f;
        const float hold = 0.6f;
        const float fadeOut = 0.5f;

        [SetUp]
        public void Setup()
        {
            TestHelpers.InitFilePaths();
        }

        [Test]
        public void ModelToAction()
        {
            var model = new FreezeFrameActionModel()
            {
                Time = time,
                TintColor = tintColor,
                FlashIn = flashIn,
                Hold = hold,
                FadeOut = fadeOut
            };

            var action = new FreezeFrameAction(null, model);

            action.Time.ShouldBe(time);
            action.TintColor.ShouldBe(tintColor);
            action.FlashIn.ShouldBe(flashIn);
            action.Hold.ShouldBe(hold);
            action.FadeOut.ShouldBe(fadeOut);
        }

        [Test]
        public void ActionToModel()
        {
            var action = new FreezeFrameAction(null)
            {
                Time = time,
                TintColor = tintColor,
                FlashIn = flashIn,
                Hold = hold,
                FadeOut = fadeOut
            };

            var model = new FreezeFrameActionModel(action);

            model.ActionType.ShouldBe(EActionType.FreezeFrame);
            model.Time.ShouldBe(time);
            model.TintColor.ShouldBe(tintColor);
            model.FlashIn.ShouldBe(flashIn);
            model.Hold.ShouldBe(hold);
            model.FadeOut.ShouldBe(fadeOut);
        }

        [Test]
        public void Persist()
        {
            var model = new FreezeFrameActionModel()
            {
                Time = time,
                TintColor = tintColor,
                FlashIn = flashIn,
                Hold = hold,
                FadeOut = fadeOut
            };

            var container = new StateContainerModel(new Filename("FreezeFrameTests.xml"));
            var actions = new SingleStateActionsModel();
            container.StatesActions.Add(actions);
            actions.ActionModels.Add(model);
            container.WriteXml();

            var container2 = new StateContainerModel(new Filename("FreezeFrameTests.xml"));
            container2.ReadXmlFile();

            container2.StatesActions.Count.ShouldBe(1);
            container2.StatesActions[0].ActionModels.Count.ShouldBe(1);
            var model2 = container2.StatesActions[0].ActionModels[0] as FreezeFrameActionModel;
            model2.ShouldNotBeNull();
            model2.ActionType.ShouldBe(EActionType.FreezeFrame);
            model2.Time.ShouldBe(time);
            model2.TintColor.ShouldBe(tintColor);
            model2.FlashIn.ShouldBe(flashIn);
            model2.Hold.ShouldBe(hold);
            model2.FadeOut.ShouldBe(fadeOut);
        }
    }
}
