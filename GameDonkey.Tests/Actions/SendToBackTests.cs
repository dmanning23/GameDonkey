using FilenameBuddy;
using GameDonkeyLib;
using NUnit.Framework;
using Shouldly;

namespace GameDonkey.Tests
{
    [TestFixture]
    public class SendToBackTests
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
            var model = new SendToBackActionModel() { Time = time };

            var action = new SendToBackAction(null, model);

            action.Time.ShouldBe(time);
        }

        [Test]
        public void ActionToModel_SetsActionType()
        {
            var action = new SendToBackAction(null) { Time = time };

            var model = new SendToBackActionModel(action);

            model.ActionType.ShouldBe(EActionType.SendToBack);
            model.Time.ShouldBe(time);
        }

        [Test]
        public void Persist()
        {
            var model = new SendToBackActionModel() { Time = time };

            var container = new StateContainerModel(new Filename("SendToBackTests.xml"));
            var actions = new SingleStateActionsModel();
            container.StatesActions.Add(actions);
            actions.ActionModels.Add(model);
            container.WriteXml();

            var container2 = new StateContainerModel(new Filename("SendToBackTests.xml"));
            container2.ReadXmlFile();

            container2.StatesActions.Count.ShouldBe(1);
            container2.StatesActions[0].ActionModels.Count.ShouldBe(1);
            var model2 = container2.StatesActions[0].ActionModels[0] as SendToBackActionModel;
            model2.ShouldNotBeNull();
            model2.ActionType.ShouldBe(EActionType.SendToBack);
            model2.Time.ShouldBe(time);
        }
    }
}
