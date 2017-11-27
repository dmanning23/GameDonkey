using FilenameBuddy;
using GameDonkeyLib;
using Microsoft.Xna.Framework;
using NUnit.Framework;
using Shouldly;

namespace GameDonkey.Tests
{
	[TestFixture]
	public class ConstantDeccelerationTests
	{
		const float time = 1f;
		const EDirectionType directionType = EDirectionType.NegController;
		const float X = 2f;
		const float Y = 3f;
		const float min = 4f;

		[SetUp]
		public void Setup()
		{
			Filename.SetCurrentDirectory(@"C:\Projects\gamedonkey\GameDonkey\GameDonkey.Tests\");
		}

		[Test]
		public void ModelToAction()
		{
			var model = new ConstantDeccelerationActionModel()
			{
				Time = time,
				Direction = new DirectionActionModel()
				{
					DirectionType = directionType,
					Velocity = new Vector2(X, Y)
				},
				MinYVelocity = min
			};

			var action = new ConstantDeccelerationAction(null, model);

			action.Time.ShouldBe(time);
			action.Velocity.DirectionType.ShouldBe(directionType);
			action.Velocity.Velocity.X.ShouldBe(X);
			action.Velocity.Velocity.Y.ShouldBe(Y);
			action.MinYVelocity.ShouldBe(min);
		}

		[Test]
		public void ActionToModel()
		{
			var action = new ConstantDeccelerationAction(null)
			{
				Time = time,
				Velocity = new ActionDirection()
				{
					DirectionType = directionType,
					Velocity = new Vector2(X, Y)
				},
				MinYVelocity = min
			};

			var model = new ConstantDeccelerationActionModel(action);

			model.Time.ShouldBe(time);
			model.Direction.DirectionType.ShouldBe(directionType);
			model.Direction.Velocity.X.ShouldBe(X);
			model.Direction.Velocity.Y.ShouldBe(Y);
			model.MinYVelocity.ShouldBe(min);
		}

		[Test]
		public void Persist()
		{
			var model = new ConstantDeccelerationActionModel()
			{
				Time = time,
				Direction = new DirectionActionModel()
				{
					DirectionType = directionType,
					Velocity = new Vector2(X, Y)
				},
				MinYVelocity = min
			};

			//write the action out
			var container = new SingleStateContainerModel(new Filename("ConstantDeccelerationTests.xml"));
			var actions = new StateActionsModel();
			container.StatesActions.Add(actions);
			actions.StateActions.Add(model);
			container.WriteXml();

			//read it back in
			var container2 = new SingleStateContainerModel(new Filename("ConstantDeccelerationTests.xml"));
			container2.ReadXmlFile();

			//get the action
			container2.StatesActions.Count.ShouldBe(1);
			container2.StatesActions[0].StateActions.Count.ShouldBe(1);
			var model2 = container2.StatesActions[0].StateActions[0] as ConstantDeccelerationActionModel;
			model2.ShouldNotBeNull();
			model2.ActionType.ShouldBe(EActionType.ConstantDecceleration);
			model2.Time.ShouldBe(time);
			model2.Direction.DirectionType.ShouldBe(directionType);
			model2.Direction.Velocity.X.ShouldBe(X);
			model2.Direction.Velocity.Y.ShouldBe(Y);
			model2.MinYVelocity.ShouldBe(min);
		}
	}
}
