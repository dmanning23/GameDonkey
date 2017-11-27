using FilenameBuddy;
using GameDonkeyLib;
using NUnit.Framework;
using Shouldly;

namespace GameDonkey.Tests
{
	[TestFixture]
	public class EvadeTests
	{
		const float time = 1f;
		const float timeDelta = 2f;

		[SetUp]
		public void Setup()
		{
			Filename.SetCurrentDirectory(@"C:\Projects\gamedonkey\GameDonkey\GameDonkey.Tests\");
		}

		[Test]
		public void ModelToAction()
		{
			var model = new EvadeActionModel()
			{
				Time = time,
				TimeDelta = new TimedActionModel(timeDelta)
			};

			var action = new EvadeAction(null, model);

			action.Time.ShouldBe(time);
			action.TimeDelta.ShouldBe(timeDelta);
		}

		[Test]
		public void ActionToModel()
		{
			var action = new EvadeAction(null)
			{
				Time = time,
				TimeDelta = timeDelta
			};

			var model = new EvadeActionModel(action);

			model.Time.ShouldBe(time);
			model.TimeDelta.TimeDelta.ShouldBe(timeDelta);
		}

		[Test]
		public void Persist()
		{
			var model = new EvadeActionModel()
			{
				Time = time,
				TimeDelta = new TimedActionModel(timeDelta)
			};

			//write the action out
			var container = new SingleStateContainerModel(new Filename("EvadeTests.xml"));
			var actions = new StateActionsModel();
			container.StatesActions.Add(actions);
			actions.StateActions.Add(model);
			container.WriteXml();

			//read it back in
			var container2 = new SingleStateContainerModel(new Filename("EvadeTests.xml"));
			container2.ReadXmlFile();

			//get the action
			container2.StatesActions.Count.ShouldBe(1);
			container2.StatesActions[0].StateActions.Count.ShouldBe(1);
			var model2 = container2.StatesActions[0].StateActions[0] as EvadeActionModel;
			model2.ShouldNotBeNull();
			model2.ActionType.ShouldBe(EActionType.Evade);
			model2.Time.ShouldBe(time);
			model2.TimeDelta.TimeDelta.ShouldBe(timeDelta);
		}
	}
}
