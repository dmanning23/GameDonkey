using FilenameBuddy;
using GameDonkeyLib;
using NUnit.Framework;
using Shouldly;

namespace GameDonkey.Tests
{
	[TestFixture]
	public class SendStateMessageTests
	{
		const float time = 1f;
		const string message = "catpants";

		[SetUp]
		public void Setup()
		{
			Filename.SetCurrentDirectory(@"C:\Projects\gamedonkey\GameDonkey\GameDonkey.Tests\");
		}

		[Test]
		public void ModelToAction()
		{
			var model = new SendStateMessageActionModel()
			{
				Time = time,
				Message = message,
			};

			var action = new SendStateMessageAction(null, model, null);

			action.Time.ShouldBe(time);
			action.Message.ShouldBe(message);
		}

		[Test]
		public void ActionToModel()
		{
			var action = new SendStateMessageAction(null)
			{
				Time = time,
				Message = message,
			};

			var model = new SendStateMessageActionModel(action);

			model.Time.ShouldBe(time);
			model.Message.ShouldBe(message);
		}

		[Test]
		public void Persist()
		{
			var model = new SendStateMessageActionModel()
			{
				Time = time,
				Message = message,
			};

			//write the action out
			var container = new SingleStateContainerModel(new Filename("SendStateMessageTests.xml"));
			var actions = new SingleStateActionsModel();
			container.StatesActions.Add(actions);
			actions.ActionModels.Add(model);
			container.WriteXml();

			//read it back in
			var container2 = new SingleStateContainerModel(new Filename("SendStateMessageTests.xml"));
			container2.ReadXmlFile();

			//get the action
			container2.StatesActions.Count.ShouldBe(1);
			container2.StatesActions[0].ActionModels.Count.ShouldBe(1);
			var model2 = container2.StatesActions[0].ActionModels[0] as SendStateMessageActionModel;
			model2.ShouldNotBeNull();
			model2.ActionType.ShouldBe(EActionType.SendStateMessage);
			model2.Time.ShouldBe(time);
			model2.Message.ShouldBe(message);
		}
	}
}
