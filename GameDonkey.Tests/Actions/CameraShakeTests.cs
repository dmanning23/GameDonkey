using FilenameBuddy;
using GameDonkeyLib;
using NUnit.Framework;
using Shouldly;

namespace GameDonkey.Tests
{
	[TestFixture]
	public class CameraShakeTests
	{
		const float time = 1f;
		const float shakeAmount = 3f;
		const float timeDelta = 2f;

		[SetUp]
		public void Setup()
		{
			Filename.SetCurrentDirectory(@"C:\Projects\gamedonkey\GameDonkey\GameDonkey.Tests\");
		}

		[Test]
		public void ModelToAction()
		{
			var model = new CameraShakeActionModel()
			{
				Time = time,
				ShakeAmount = shakeAmount,
				TimeDelta = new TimedActionModel(timeDelta)
			};

			var action = new CameraShakeAction(null, model);

			action.Time.ShouldBe(time);
			action.ShakeAmount.ShouldBe(shakeAmount);
			action.TimeDelta.ShouldBe(timeDelta);
		}

		[Test]
		public void ActionToModel()
		{
			var action = new CameraShakeAction(null)
			{
				Time = time,
				ShakeAmount = shakeAmount,
				TimeDelta = timeDelta
			};

			var model = new CameraShakeActionModel(action);

			model.Time.ShouldBe(time);
			model.ShakeAmount.ShouldBe(shakeAmount);
			model.TimeDelta.TimeDelta.ShouldBe(timeDelta);
		}

		[Test]
		public void Persist()
		{
			var model = new CameraShakeActionModel()
			{
				Time = time,
				ShakeAmount = shakeAmount,
				TimeDelta = new TimedActionModel(timeDelta)
			};

			//write the action out
			var container = new SingleStateContainerModel(new Filename("CameraShakeTests.xml"));
			var actions = new SingleStateActionsModel();
			container.StatesActions.Add(actions);
			actions.ActionModels.Add(model);
			container.WriteXml();

			//read it back in
			var container2 = new SingleStateContainerModel(new Filename("CameraShakeTests.xml"));
			container2.ReadXmlFile();

			//get the action
			container2.StatesActions.Count.ShouldBe(1);
			container2.StatesActions[0].ActionModels.Count.ShouldBe(1);
			var model2 = container2.StatesActions[0].ActionModels[0] as CameraShakeActionModel;
			model2.ShouldNotBeNull();
			model2.ActionType.ShouldBe(EActionType.CameraShake);
			model2.Time.ShouldBe(time);
			model2.ShakeAmount.ShouldBe(shakeAmount);
			model2.TimeDelta.TimeDelta.ShouldBe(timeDelta);
		}
	}
}
