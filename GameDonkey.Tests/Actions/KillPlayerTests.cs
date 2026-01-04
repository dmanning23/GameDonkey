using FilenameBuddy;
using GameDonkeyLib;
using NUnit.Framework;
using Shouldly;

namespace GameDonkey.Tests
{
	[TestFixture]
	public class KillPlayerTests
	{
		const float time = 1f;

		[SetUp]
		public void Setup()
		{
			Filename.SetCurrentDirectory(@"C:\Projects\gamedonkey\GameDonkey\GameDonkey.Tests\");
		}

		[Test]
		public void ModelToAction()
		{
			var model = new KillPlayerActionModel()
			{
				Time = time,
			};

			var action = new KillPlayerAction(null, model);

			action.Time.ShouldBe(time);
		}

		[Test]
		public void ActionToModel()
		{
			var action = new KillPlayerAction(null)
			{
				Time = time,
			};

			var model = new KillPlayerActionModel(action);

			model.Time.ShouldBe(time);
		}

		[Test]
		public void Persist()
		{
			var model = new KillPlayerActionModel()
			{
				Time = time,
			};

			//write the action out
			var container = new SingleStateContainerModel(new Filename("KillPlayerTests.xml"));
			var actions = new SingleStateActionsModel();
			container.StatesActions.Add(actions);
			actions.ActionModels.Add(model);
			container.WriteXml();

			//read it back in
			var container2 = new SingleStateContainerModel(new Filename("KillPlayerTests.xml"));
			container2.ReadXmlFile();

			//get the action
			container2.StatesActions.Count.ShouldBe(1);
			container2.StatesActions[0].ActionModels.Count.ShouldBe(1);
			var model2 = container2.StatesActions[0].ActionModels[0] as KillPlayerActionModel;
			model2.ShouldNotBeNull();
			model2.ActionType.ShouldBe(EActionType.KillPlayer);
			model2.Time.ShouldBe(time);
		}
	}
}
