using FilenameBuddy;
using GameDonkeyLib;
using NUnit.Framework;
using Shouldly;

namespace GameDonkey.Tests
{
	[TestFixture]
	public class RotateTests
	{
		const float time = 1f;
		const float rotation = 3f;

		[SetUp]
		public void Setup()
		{
			Filename.SetCurrentDirectory(@"C:\Projects\gamedonkey\GameDonkey\GameDonkey.Tests\");
		}

		[Test]
		public void ModelToAction()
		{
			var model = new RotateActionModel()
			{
				Time = time,
				Rotation = rotation,
			};

			var action = new RotateAction(null, model);

			action.Time.ShouldBe(time);
			action.Rotation.ShouldBe(rotation);
		}

		[Test]
		public void ActionToModel()
		{
			var action = new RotateAction(null)
			{
				Time = time,
				Rotation = rotation,
			};

			var model = new RotateActionModel(action);

			model.Time.ShouldBe(time);
			model.Rotation.ShouldBe(rotation);
		}

		[Test]
		public void Persist()
		{
			var model = new RotateActionModel()
			{
				Time = time,
				Rotation = rotation,
			};

			//write the action out
			var container = new SingleStateContainerModel(new Filename("RotateTests.xml"));
			var actions = new SingleStateActionsModel();
			container.StatesActions.Add(actions);
			actions.ActionModels.Add(model);
			container.WriteXml();

			//read it back in
			var container2 = new SingleStateContainerModel(new Filename("RotateTests.xml"));
			container2.ReadXmlFile();

			//get the action
			container2.StatesActions.Count.ShouldBe(1);
			container2.StatesActions[0].ActionModels.Count.ShouldBe(1);
			var model2 = container2.StatesActions[0].ActionModels[0] as RotateActionModel;
			model2.ShouldNotBeNull();
			model2.ActionType.ShouldBe(EActionType.Rotate);
			model2.Time.ShouldBe(time);
			model2.Rotation.ShouldBe(rotation);
		}
	}
}
