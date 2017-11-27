using FilenameBuddy;
using GameDonkeyLib;
using NUnit.Framework;
using Shouldly;

namespace GameDonkey.Tests
{
	[TestFixture]
	public class BlockingTests
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
			var model = new BlockingStateActionModel()
			{
				Time = time,
				BoneName = bone,
				TimeDelta = new TimedActionModel(timeDelta)
			};
			model.SuccessActions.Add(new DeactivateActionModel()
			{
				Time = subTime
			});

			var action = new BlockingStateAction(null, model);

			action.Time.ShouldBe(time);
			action.BoneName.ShouldBe(bone);
			action.TimeDelta.ShouldBe(timeDelta);
			action.SuccessActions.Count.ShouldBe(1);
			var subAction = action.SuccessActions[0] as DeactivateAction;
			subAction.ShouldNotBeNull();
			subAction.Time.ShouldBe(subTime);
		}

		[Test]
		public void ActionToModel()
		{
			var action = new BlockingStateAction(null)
			{
				Time = time,
				BoneName = bone,
				TimeDelta = timeDelta
			};
			action.SuccessActions.Add(new DeactivateAction(null)
			{
				Time = subTime
			});

			var model = new BlockingStateActionModel(action);

			model.Time.ShouldBe(time);
			model.BoneName.ShouldBe(bone);
			model.TimeDelta.TimeDelta.ShouldBe(timeDelta);
			model.SuccessActions.Count.ShouldBe(1);
			var subAction = model.SuccessActions[0] as DeactivateActionModel;
			subAction.ShouldNotBeNull();
			subAction.Time.ShouldBe(subTime);
		}

		[Test]
		public void Persist()
		{
			var model = new BlockingStateActionModel()
			{
				Time = time,
				BoneName = bone,
				TimeDelta = new TimedActionModel(timeDelta)
			};
			model.SuccessActions.Add(new DeactivateActionModel()
			{
				Time = subTime
			});

			//write the action out
			var container = new SingleStateContainerModel(new Filename("BlockingTests.xml"));
			var actions = new StateActionsModel();
			container.StatesActions.Add(actions);
			actions.StateActions.Add(model);
			container.WriteXml();

			//read it back in
			var container2 = new SingleStateContainerModel(new Filename("BlockingTests.xml"));
			container2.ReadXmlFile();

			//get the action
			container2.StatesActions.Count.ShouldBe(1);
			container2.StatesActions[0].StateActions.Count.ShouldBe(1);
			var model2 = container2.StatesActions[0].StateActions[0] as BlockingStateActionModel;
			model2.ShouldNotBeNull();
			model2.ActionType.ShouldBe(EActionType.BlockState);
			model2.Time.ShouldBe(time);
			model2.BoneName.ShouldBe(bone);
			model2.TimeDelta.TimeDelta.ShouldBe(timeDelta);
			model2.SuccessActions.Count.ShouldBe(1);
			var subAction = model2.SuccessActions[0] as DeactivateActionModel;
			subAction.ShouldNotBeNull();
			subAction.Time.ShouldBe(subTime);
		}
	}
}
