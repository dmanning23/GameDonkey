using FilenameBuddy;
using GameDonkeyLib;
using Microsoft.Xna.Framework;
using NUnit.Framework;
using Shouldly;

namespace GameDonkey.Tests
{
	[TestFixture]
	public class SetVelocityTests
	{
		const float time = 1f;
		const EDirectionType directionType = EDirectionType.NegController;
		const float X = 2f;
		const float Y = 3f;

		[SetUp]
		public void Setup()
		{
			Filename.SetCurrentDirectory(@"C:\Projects\gamedonkey\GameDonkey\GameDonkey.Tests\");
		}

		[Test]
		public void ModelToAction()
		{
			var model = new SetVelocityActionModel()
			{
				Time = time,
				Direction = new DirectionActionModel()
				{
					DirectionType = directionType,
					Velocity = new Vector2(X, Y)
				}
			};

			var action = new SetVelocityAction(null, model);

			action.Time.ShouldBe(time);
			action.Velocity.DirectionType.ShouldBe(directionType);
			action.Velocity.Velocity.X.ShouldBe(X);
			action.Velocity.Velocity.Y.ShouldBe(Y);
		}

		[Test]
		public void ActionToModel()
		{
			var action = new SetVelocityAction(null)
			{
				Time = time,
				Velocity = new ActionDirection()
				{
					DirectionType = directionType,
					Velocity = new Vector2(X, Y)
				}
			};

			var model = new SetVelocityActionModel(action);

			model.Time.ShouldBe(time);
			model.Direction.DirectionType.ShouldBe(directionType);
			model.Direction.Velocity.X.ShouldBe(X);
			model.Direction.Velocity.Y.ShouldBe(Y);
		}

		[Test]
		public void Persist()
		{
			var model = new SetVelocityActionModel()
			{
				Time = time,
				Direction = new DirectionActionModel()
				{
					DirectionType = directionType,
					Velocity = new Vector2(X, Y)
				}
			};

			//write the action out
			var container = new SingleStateContainerModel(new Filename("SetVelocityTests.xml"));
			var actions = new StateActionsModel();
			container.StatesActions.Add(actions);
			actions.StateActions.Add(model);
			container.WriteXml();

			//read it back in
			var container2 = new SingleStateContainerModel(new Filename("SetVelocityTests.xml"));
			container2.ReadXmlFile();

			//get the action
			container2.StatesActions.Count.ShouldBe(1);
			container2.StatesActions[0].StateActions.Count.ShouldBe(1);
			var model2 = container2.StatesActions[0].StateActions[0] as SetVelocityActionModel;
			model2.ShouldNotBeNull();
			model2.ActionType.ShouldBe(EActionType.SetVelocity);
			model2.Time.ShouldBe(time);
			model2.Direction.DirectionType.ShouldBe(directionType);
			model2.Direction.Velocity.X.ShouldBe(X);
			model2.Direction.Velocity.Y.ShouldBe(Y);
		}
	}
}
