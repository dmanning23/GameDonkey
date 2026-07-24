using FilenameBuddy;
using GameDonkeyLib;
using NUnit.Framework;
using Shouldly;

namespace GameDonkey.Tests
{
    [TestFixture]
    public class LetterboxTests
    {
        const float time = 1f;
        const float barHeight = 0.2f;
        const float slideIn = 0.3f;
        const float hold = 1.5f;
        const float slideOut = 0.25f;

        [SetUp]
        public void Setup()
        {
            TestHelpers.InitFilePaths();
        }

        [Test]
        public void ModelToAction()
        {
            var model = new LetterboxActionModel()
            {
                Time = time,
                BarHeight = barHeight,
                SlideIn = slideIn,
                Hold = hold,
                SlideOut = slideOut
            };

            var action = new LetterboxAction(null, model);

            action.Time.ShouldBe(time);
            action.BarHeight.ShouldBe(barHeight);
            action.SlideIn.ShouldBe(slideIn);
            action.Hold.ShouldBe(hold);
            action.SlideOut.ShouldBe(slideOut);
        }

        [Test]
        public void ActionToModel()
        {
            var action = new LetterboxAction(null)
            {
                Time = time,
                BarHeight = barHeight,
                SlideIn = slideIn,
                Hold = hold,
                SlideOut = slideOut
            };

            var model = new LetterboxActionModel(action);

            model.ActionType.ShouldBe(EActionType.Letterbox);
            model.Time.ShouldBe(time);
            model.BarHeight.ShouldBe(barHeight);
            model.SlideIn.ShouldBe(slideIn);
            model.Hold.ShouldBe(hold);
            model.SlideOut.ShouldBe(slideOut);
        }

        [Test]
        public void Persist()
        {
            var model = new LetterboxActionModel()
            {
                Time = time,
                BarHeight = barHeight,
                SlideIn = slideIn,
                Hold = hold,
                SlideOut = slideOut
            };

            var container = new StateContainerModel(new Filename("LetterboxTests.xml"));
            var actions = new SingleStateActionsModel();
            container.StatesActions.Add(actions);
            actions.ActionModels.Add(model);
            container.WriteXml();

            var container2 = new StateContainerModel(new Filename("LetterboxTests.xml"));
            container2.ReadXmlFile();

            container2.StatesActions.Count.ShouldBe(1);
            container2.StatesActions[0].ActionModels.Count.ShouldBe(1);
            var model2 = container2.StatesActions[0].ActionModels[0] as LetterboxActionModel;
            model2.ShouldNotBeNull();
            model2.ActionType.ShouldBe(EActionType.Letterbox);
            model2.Time.ShouldBe(time);
            model2.BarHeight.ShouldBe(barHeight);
            model2.SlideIn.ShouldBe(slideIn);
            model2.Hold.ShouldBe(hold);
            model2.SlideOut.ShouldBe(slideOut);
        }
    }
}
