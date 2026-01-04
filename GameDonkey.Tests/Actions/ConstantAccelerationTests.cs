using FilenameBuddy;
using GameDonkeyLib;
using Microsoft.Xna.Framework;
using NUnit.Framework;
using Shouldly;

namespace GameDonkey.Tests
{
	[TestFixture]
	public class ConstantAccelerationTests
	{
		const float time = 1f;
		const EDirectionType directionType = EDirectionType.NegController;
		const float X = 2f;
		const float Y = 3f;
		const float max = 4f;

		[SetUp]
		public void Setup()
		{
			Filename.SetCurrentDirectory(@"C:\Projects\gamedonkey\GameDonkey\GameDonkey.Tests\");
		}

		[Test]
		public void ModelToAction()
		{
			var model = new ConstantAccelerationActionModel()
			{
				Time = time,
				Direction = new DirectionActionModel()
				{
					DirectionType = directionType,
					Velocity = new Vector2(X, Y)
				},
				MaxVelocity = max
			};

			var action = new ConstantAccelerationAction(null, model);

			action.Time.ShouldBe(time);
			action.Velocity.DirectionType.ShouldBe(directionType);
			action.Velocity.Velocity.X.ShouldBe(X);
			action.Velocity.Velocity.Y.ShouldBe(Y);
			action.MaxVelocity.ShouldBe(max);
		}

		[Test]
		public void ActionToModel()
		{
			var action = new ConstantAccelerationAction(null)
			{
				Time = time,
				Velocity = new ActionDirection()
				{
					DirectionType = directionType,
					Velocity = new Vector2(X, Y)
				},
				MaxVelocity = max
			};

			var model = new ConstantAccelerationActionModel(action);

			model.Time.ShouldBe(time);
			model.Direction.DirectionType.ShouldBe(directionType);
			model.Direction.Velocity.X.ShouldBe(X);
			model.Direction.Velocity.Y.ShouldBe(Y);
			model.MaxVelocity.ShouldBe(max);
		}

		[Test]
		public void Persist()
		{
			var model = new ConstantAccelerationActionModel()
			{
				Time = time,
				Direction = new DirectionActionModel()
				{
					DirectionType = directionType,
					Velocity = new Vector2(X, Y)
				},
				MaxVelocity = max
			};

			//write the action out
			var container = new SingleStateContainerModel(new Filename("ConstantAccelerationTests.xml"));
			var actions = new SingleStateActionsModel();
			container.StatesActions.Add(actions);
			actions.ActionModels.Add(model);
			container.WriteXml();

			//read it back in
			var container2 = new SingleStateContainerModel(new Filename("ConstantAccelerationTests.xml"));
			container2.ReadXmlFile();

			//get the action
			container2.StatesActions.Count.ShouldBe(1);
			container2.StatesActions[0].ActionModels.Count.ShouldBe(1);
			var model2 = container2.StatesActions[0].ActionModels[0] as ConstantAccelerationActionModel;
			model2.ShouldNotBeNull();
			model2.ActionType.ShouldBe(EActionType.ConstantAcceleration);
			model2.Time.ShouldBe(time);
			model2.Direction.DirectionType.ShouldBe(directionType);
			model2.Direction.Velocity.X.ShouldBe(X);
			model2.Direction.Velocity.Y.ShouldBe(Y);
			model2.MaxVelocity.ShouldBe(max);
		}
	}
}
