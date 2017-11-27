using FilenameBuddy;
using GameDonkeyLib;
using NUnit.Framework;
using Shouldly;

namespace GameDonkey.Tests
{
	[TestFixture]
	public class DeactivateTests
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
			var model = new DeactivateActionModel()
			{
				Time = time,
			};

			var action = new DeactivateAction(null, model);

			action.Time.ShouldBe(time);
		}

		[Test]
		public void ActionToModel()
		{
			var action = new DeactivateAction(null)
			{
				Time = time,
			};

			var model = new DeactivateActionModel(action);

			model.Time.ShouldBe(time);
		}

		[Test]
		public void Persist()
		{
			var model = new DeactivateActionModel()
			{
				Time = time,
			};

			//write the action out
			var container = new SingleStateContainerModel(new Filename("DeactivateTests.xml"));
			var actions = new StateActionsModel();
			container.StatesActions.Add(actions);
			actions.StateActions.Add(model);
			container.WriteXml();

			//read it back in
			var container2 = new SingleStateContainerModel(new Filename("DeactivateTests.xml"));
			container2.ReadXmlFile();

			//get the action
			container2.StatesActions.Count.ShouldBe(1);
			container2.StatesActions[0].StateActions.Count.ShouldBe(1);
			var model2 = container2.StatesActions[0].StateActions[0] as DeactivateActionModel;
			model2.ShouldNotBeNull();
			model2.ActionType.ShouldBe(EActionType.Deactivate);
			model2.Time.ShouldBe(time);
		}
	}
}
