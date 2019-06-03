using FilenameBuddy;
using GameDonkeyLib;
using NUnit.Framework;
using Shouldly;

namespace GameDonkey.Tests
{
	[TestFixture]
	public class CreateBlockTests
	{
		const float time = 1f;
		const string bone = "catpants";
		const float timeDelta = 2f;
		const float subTime = 3f;

		[SetUp]
		public void Setup()
		{
			Filename.SetCurrentDirectory(@"C:\Projects\gamedonkey\GameDonkey\GameDonkey.Tests\");
		}

		[Test]
		public void ModelToAction()
		{
			var model = new CreateBlockActionModel()
			{
				Time = time,
				TimeDelta = new TimedActionModel(timeDelta)
			};
			model.ActionModels.ActionModels.Add(new DeactivateActionModel()
			{
				Time = subTime
			});

			var action = new CreateBlockAction(null, model, null);

			action.Time.ShouldBe(time);
			action.TimeDelta.ShouldBe(timeDelta);
			action.Actions.Count.ShouldBe(1);
			var subAction = action.Actions[0] as DeactivateAction;
			subAction.ShouldNotBeNull();
			subAction.Time.ShouldBe(subTime);
		}

		[Test]
		public void ActionToModel()
		{
			var action = new CreateBlockAction(null)
			{
				Time = time,
				TimeDelta = timeDelta
			};
			action.Actions.Add(new DeactivateAction(null)
			{
				Time = subTime
			});

			var model = new CreateBlockActionModel(action);

			model.Time.ShouldBe(time);
			model.TimeDelta.TimeDelta.ShouldBe(timeDelta);
			model.ActionModels.ActionModels.Count.ShouldBe(1);
			var subAction = model.ActionModels.ActionModels[0] as DeactivateActionModel;
			subAction.ShouldNotBeNull();
			subAction.Time.ShouldBe(subTime);
		}

		[Test]
		public void Persist()
		{
			var model = new CreateBlockActionModel()
			{
				Time = time,
				TimeDelta = new TimedActionModel(timeDelta)
			};
			model.ActionModels.ActionModels.Add(new DeactivateActionModel()
			{
				Time = subTime
			});

			//write the action out
			var container = new SingleStateContainerModel(new Filename("CreateBlockTests.xml"));
			var actions = new SingleStateActionsModel();
			container.StatesActions.Add(actions);
			actions.ActionModels.Add(model);
			container.WriteXml();

			//read it back in
			var container2 = new SingleStateContainerModel(new Filename("CreateBlockTests.xml"));
			container2.ReadXmlFile();

			//get the action
			container2.StatesActions.Count.ShouldBe(1);
			container2.StatesActions[0].ActionModels.Count.ShouldBe(1);
			var model2 = container2.StatesActions[0].ActionModels[0] as CreateBlockActionModel;
			model2.ShouldNotBeNull();
			model2.ActionType.ShouldBe(EActionType.CreateBlock);
			model2.Time.ShouldBe(time);
			model2.TimeDelta.TimeDelta.ShouldBe(timeDelta);
			model2.ActionModels.ActionModels.Count.ShouldBe(1);
			var subAction = model2.ActionModels.ActionModels[0] as DeactivateActionModel;
			subAction.ShouldNotBeNull();
			subAction.Time.ShouldBe(subTime);
		}
	}
}
