using FilenameBuddy;
using GameDonkeyLib;
using NUnit.Framework;
using Shouldly;

namespace GameDonkey.Tests
{
    [TestFixture]
    public class TemplateActionTests
    {
        const float time = 1f;

        [SetUp]
        public void Setup()
        {
            TestHelpers.InitFilePaths();
        }

        [Test]
        public void ModelToAction_SetsTime()
        {
            var model = new TemplateActionModel()
            {
                Time = time,
                TemplateContainer = new TemplateContainerModel(new Filename("TemplateActionTests.xml"))
            };

            var action = new TemplateAction(null, model, null);

            action.Time.ShouldBe(time);
        }

        [Test]
        public void ActionToModel_SetsActionType()
        {
            var action = new TemplateAction(null)
            {
                FileName = new Filename("TemplateActionTests.xml"),
                Time = time
            };

            var model = new TemplateActionModel(action);

            model.ActionType.ShouldBe(EActionType.TemplateAction);
            model.Time.ShouldBe(time);
        }

        [Test]
        public void Persist()
        {
            var model = new TemplateActionModel()
            {
                Time = time,
                TemplateContainer = new TemplateContainerModel(new Filename("TemplateTests_template.xml"))
            };

            var container = new StateContainerModel(new Filename("TemplateActionTests.xml"));
            var actions = new SingleStateActionsModel();
            container.StatesActions.Add(actions);
            actions.ActionModels.Add(model);
            container.WriteXml();

            var container2 = new StateContainerModel(new Filename("TemplateActionTests.xml"));
            container2.ReadXmlFile();

            container2.StatesActions.Count.ShouldBe(1);
            container2.StatesActions[0].ActionModels.Count.ShouldBe(1);
            var model2 = container2.StatesActions[0].ActionModels[0] as TemplateActionModel;
            model2.ShouldNotBeNull();
            model2.ActionType.ShouldBe(EActionType.TemplateAction);
            model2.Time.ShouldBe(time);
        }

        [Test]
        public void Execute_WithNoActions_ReturnsFalse()
        {
            var template = new TemplateAction(null);
            template.Reset();

            var result = template.Execute(0f);

            result.ShouldBeFalse();
        }

        [Test]
        public void Execute_WithNoActions_SetsAlreadyRunTrue()
        {
            var template = new TemplateAction(null);
            template.Reset();

            template.Execute(0f);

            template.AlreadyRun.ShouldBeTrue();
        }

        [Test]
        public void Execute_RunsChildAction_WhenTimeMatches()
        {
            var owner = new TestObject();
            var template = new TemplateAction(owner);
            var child = new RotateAction(owner) { Time = 0f, Rotation = 2.5f };
            template.Actions.Add(child);
            template.Reset();

            template.Execute(1f);

            owner.RotationPerSecond.ShouldBe(2.5f);
        }

        [Test]
        public void Execute_SkipsChildAction_WhenTimeNotReached()
        {
            var owner = new TestObject();
            var template = new TemplateAction(owner);
            var child = new RotateAction(owner) { Time = 5f, Rotation = 2.5f };
            template.Actions.Add(child);
            template.Reset();

            template.Execute(1f);

            owner.RotationPerSecond.ShouldBe(0f);
        }

        [Test]
        public void Execute_SetsAlreadyRunTrue_WhenAllChildrenComplete()
        {
            var owner = new TestObject();
            var template = new TemplateAction(owner);
            var child = new RotateAction(owner) { Time = 0f };
            template.Actions.Add(child);
            template.Reset();

            template.Execute(1f);

            template.AlreadyRun.ShouldBeTrue();
        }

        [Test]
        public void Execute_SetsAlreadyRunFalse_WhenChildNotYetDone()
        {
            var owner = new TestObject();
            var template = new TemplateAction(owner);
            template.Actions.Add(new RotateAction(owner) { Time = 0f });
            template.Actions.Add(new RotateAction(owner) { Time = 5f });
            template.Reset();

            template.Execute(1f);

            template.AlreadyRun.ShouldBeFalse();
        }

        [Test]
        public void Execute_ReturnsTrue_WhenChildTriggersStateChange()
        {
            var owner = new TestObject();
            var stub = new StubStateContainer();
            var template = new TemplateAction(owner);
            var messageModel = new SendStateMessageActionModel() { Message = "test", Time = 0f };
            var child = new SendStateMessageAction(owner, messageModel, stub);
            template.Actions.Add(child);
            template.Reset();

            var result = template.Execute(1f);

            result.ShouldBeTrue();
        }

        [Test]
        public void Reset_SetsAlreadyRunFalse()
        {
            var template = new TemplateAction(null);

            template.Reset();

            template.AlreadyRun.ShouldBeFalse();
        }

        [Test]
        public void Reset_ResetsChildActions()
        {
            var owner = new TestObject();
            var template = new TemplateAction(owner);
            var child = new RotateAction(owner) { Time = 0f };
            template.Actions.Add(child);
            template.Reset();
            template.Execute(1f);

            template.Reset();

            child.AlreadyRun.ShouldBeFalse();
        }
    }
}
