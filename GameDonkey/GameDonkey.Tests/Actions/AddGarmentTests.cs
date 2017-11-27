using FilenameBuddy;
using GameDonkeyLib;
using NUnit.Framework;
using Shouldly;

namespace GameDonkey.Tests
{
	[TestFixture]
    public class AddGarmentTests
    {
		const float time = 1f;
		const string filename = "catpants.jpg";
		const float timeDelta = 2f;

		[SetUp]
		public void Setup()
		{
			Filename.SetCurrentDirectory(@"C:\Projects\gamedonkey\GameDonkey\GameDonkey.Tests\");
		}

		[Test]
		public void ModelToAction()
		{
			var model = new AddGarmentActionModel()
			{
				Time = time,
				Filename = new Filename(filename),
				TimeDelta = new TimedActionModel(timeDelta)
			};

			var action = new AddGarmentAction(null, model);

			action.Time.ShouldBe(time);
			action.Filename.GetFile().ShouldBe(filename);
			action.TimeDelta.ShouldBe(timeDelta);
		}

		[Test]
		public void ActionToModel()
		{
			var action = new AddGarmentAction(null)
			{
				Time = time,
				Filename = new Filename(filename),
				TimeDelta = timeDelta
			};

			var model = new AddGarmentActionModel(action);

			model.Time.ShouldBe(time);
			model.Filename.GetFile().ShouldBe(filename);
			model.TimeDelta.TimeDelta.ShouldBe(timeDelta);
		}

		[Test]
		public void Persist()
		{
			var model = new AddGarmentActionModel()
			{
				Time = time,
				Filename = new Filename(filename),
				TimeDelta = new TimedActionModel(timeDelta)
			};

			//write the action out
			var container = new SingleStateContainerModel(new Filename("AddGarmentTests.xml"));
			var actions = new StateActionsModel();
			container.StatesActions.Add(actions);
			actions.StateActions.Add(model);
			container.WriteXml();

			//read it back in
			var container2 = new SingleStateContainerModel(new Filename("AddGarmentTests.xml"));
			container2.ReadXmlFile();

			//get the action
			container2.StatesActions.Count.ShouldBe(1);
			container2.StatesActions[0].StateActions.Count.ShouldBe(1);
			var model2 = container2.StatesActions[0].StateActions[0] as AddGarmentActionModel;
			model2.ShouldNotBeNull();
			model2.ActionType.ShouldBe(EActionType.AddGarment);
			model2.Time.ShouldBe(time);
			model2.Filename.GetFile().ShouldBe(filename);
			model2.TimeDelta.TimeDelta.ShouldBe(timeDelta);
		}
    }
}
