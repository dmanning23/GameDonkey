using FilenameBuddy;
using GameDonkeyLib;
using Microsoft.Xna.Framework;
using NUnit.Framework;
using ScreenFXBuddy;
using ScreenFXBuddy.Effects;
using Shouldly;

namespace GameDonkey.Tests
{
    [TestFixture]
    public class SpeedLinesTests
    {
        const float time = 1f;
        static readonly Color speedLinesColor = Color.Yellow;
        const SpeedLinesMode linesMode = SpeedLinesMode.Static;
        const FadeMode speedLinesFadeMode = FadeMode.Constant;
        const FadeCurve speedLinesFadeCurve = FadeCurve.Linear;
        const int lineCount = 40;
        const float maxRadius = 0.8f;
        const float duration = 1.6f;

        [SetUp]
        public void Setup()
        {
            TestHelpers.InitFilePaths();
        }

        [Test]
        public void ModelToAction()
        {
            var model = new SpeedLinesActionModel()
            {
                Time = time,
                SpeedLinesColor = speedLinesColor,
                LinesMode = linesMode,
                SpeedLinesFadeMode = speedLinesFadeMode,
                SpeedLinesFadeCurve = speedLinesFadeCurve,
                LineCount = lineCount,
                MaxRadius = maxRadius,
                Duration = duration
            };

            var action = new SpeedLinesAction(null, model);

            action.Time.ShouldBe(time);
            action.SpeedLinesColor.ShouldBe(speedLinesColor);
            action.LinesMode.ShouldBe(linesMode);
            action.SpeedLinesFadeMode.ShouldBe(speedLinesFadeMode);
            action.SpeedLinesFadeCurve.ShouldBe(speedLinesFadeCurve);
            action.LineCount.ShouldBe(lineCount);
            action.MaxRadius.ShouldBe(maxRadius);
            action.Duration.ShouldBe(duration);
        }

        [Test]
        public void ActionToModel()
        {
            var action = new SpeedLinesAction(null)
            {
                Time = time,
                SpeedLinesColor = speedLinesColor,
                LinesMode = linesMode,
                SpeedLinesFadeMode = speedLinesFadeMode,
                SpeedLinesFadeCurve = speedLinesFadeCurve,
                LineCount = lineCount,
                MaxRadius = maxRadius,
                Duration = duration
            };

            var model = new SpeedLinesActionModel(action);

            model.ActionType.ShouldBe(EActionType.SpeedLines);
            model.Time.ShouldBe(time);
            model.SpeedLinesColor.ShouldBe(speedLinesColor);
            model.LinesMode.ShouldBe(linesMode);
            model.SpeedLinesFadeMode.ShouldBe(speedLinesFadeMode);
            model.SpeedLinesFadeCurve.ShouldBe(speedLinesFadeCurve);
            model.LineCount.ShouldBe(lineCount);
            model.MaxRadius.ShouldBe(maxRadius);
            model.Duration.ShouldBe(duration);
        }

        [Test]
        public void Persist()
        {
            var model = new SpeedLinesActionModel()
            {
                Time = time,
                SpeedLinesColor = speedLinesColor,
                LinesMode = linesMode,
                SpeedLinesFadeMode = speedLinesFadeMode,
                SpeedLinesFadeCurve = speedLinesFadeCurve,
                LineCount = lineCount,
                MaxRadius = maxRadius,
                Duration = duration
            };

            var container = new StateContainerModel(new Filename("SpeedLinesTests.xml"));
            var actions = new SingleStateActionsModel();
            container.StatesActions.Add(actions);
            actions.ActionModels.Add(model);
            container.WriteXml();

            var container2 = new StateContainerModel(new Filename("SpeedLinesTests.xml"));
            container2.ReadXmlFile();

            container2.StatesActions.Count.ShouldBe(1);
            container2.StatesActions[0].ActionModels.Count.ShouldBe(1);
            var model2 = container2.StatesActions[0].ActionModels[0] as SpeedLinesActionModel;
            model2.ShouldNotBeNull();
            model2.ActionType.ShouldBe(EActionType.SpeedLines);
            model2.Time.ShouldBe(time);
            model2.SpeedLinesColor.ShouldBe(speedLinesColor);
            model2.LinesMode.ShouldBe(linesMode);
            model2.SpeedLinesFadeMode.ShouldBe(speedLinesFadeMode);
            model2.SpeedLinesFadeCurve.ShouldBe(speedLinesFadeCurve);
            model2.LineCount.ShouldBe(lineCount);
            model2.MaxRadius.ShouldBe(maxRadius);
            model2.Duration.ShouldBe(duration);
        }
    }
}
