using FilenameBuddy;
using GameDonkeyLib;
using Microsoft.Xna.Framework;
using NUnit.Framework;
using Shouldly;

namespace GameDonkey.Tests
{
    [TestFixture]
    public class AnimeSuperTests
    {
        const float time = 1f;
        static readonly Color superColor = Color.Blue;
        const float flashIn = 0.1f;
        const float hold = 0.5f;
        const float fadeOut = 0.6f;

        [SetUp]
        public void Setup()
        {
            TestHelpers.InitFilePaths();
        }

        [Test]
        public void ModelToAction()
        {
            var model = new AnimeSuperActionModel()
            {
                Time = time,
                SuperColor = superColor,
                FlashIn = flashIn,
                Hold = hold,
                FadeOut = fadeOut
            };

            var action = new AnimeSuperAction(null, model);

            action.Time.ShouldBe(time);
            action.SuperColor.ShouldBe(superColor);
            action.FlashIn.ShouldBe(flashIn);
            action.Hold.ShouldBe(hold);
            action.FadeOut.ShouldBe(fadeOut);
        }

        [Test]
        public void ActionToModel()
        {
            var action = new AnimeSuperAction(null)
            {
                Time = time,
                SuperColor = superColor,
                FlashIn = flashIn,
                Hold = hold,
                FadeOut = fadeOut
            };

            var model = new AnimeSuperActionModel(action);

            model.ActionType.ShouldBe(EActionType.AnimeSuper);
            model.Time.ShouldBe(time);
            model.SuperColor.ShouldBe(superColor);
            model.FlashIn.ShouldBe(flashIn);
            model.Hold.ShouldBe(hold);
            model.FadeOut.ShouldBe(fadeOut);
        }

        [Test]
        public void Persist()
        {
            var model = new AnimeSuperActionModel()
            {
                Time = time,
                SuperColor = superColor,
                FlashIn = flashIn,
                Hold = hold,
                FadeOut = fadeOut
            };

            var container = new StateContainerModel(new Filename("AnimeSuperTests.xml"));
            var actions = new SingleStateActionsModel();
            container.StatesActions.Add(actions);
            actions.ActionModels.Add(model);
            container.WriteXml();

            var container2 = new StateContainerModel(new Filename("AnimeSuperTests.xml"));
            container2.ReadXmlFile();

            container2.StatesActions.Count.ShouldBe(1);
            container2.StatesActions[0].ActionModels.Count.ShouldBe(1);
            var model2 = container2.StatesActions[0].ActionModels[0] as AnimeSuperActionModel;
            model2.ShouldNotBeNull();
            model2.ActionType.ShouldBe(EActionType.AnimeSuper);
            model2.Time.ShouldBe(time);
            model2.SuperColor.ShouldBe(superColor);
            model2.FlashIn.ShouldBe(flashIn);
            model2.Hold.ShouldBe(hold);
            model2.FadeOut.ShouldBe(fadeOut);
        }
    }
}
