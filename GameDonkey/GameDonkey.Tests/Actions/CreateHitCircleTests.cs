using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FilenameBuddy;
using GameDonkeyLib;
using Microsoft.Xna.Framework;
using NUnit.Framework;
using Shouldly;

namespace GameDonkey.Tests
{
	[TestFixture]
	public class CreateHitCircleTests
	{
		const float time = 1f;
		const string bone = "catpants";
		const float timeDelta = 2f;
		const float subTime = 3f;
		const float damage = 4f;
		const EDirectionType directionType = EDirectionType.NegController;
		const float X = 2f;
		const float Y = 3f;

		const float radius = 5f;
		const float X1 = 6f;
		const float Y1 = 7f;
		const float X2 = 8f;
		const float Y2 = 9f;

		[SetUp]
		public void Setup()
		{
			Filename.SetCurrentDirectory(@"C:\Projects\gamedonkey\GameDonkey\GameDonkey.Tests\");
		}

		[Test]
		public void ModelToAction()
		{
			var model = new CreateHitCircleActionModel()
			{
				Time = time,
				BoneName = bone,
				Damage = damage,
				TimeDelta = new TimedActionModel(timeDelta),
				Direction = new DirectionActionModel()
				{
					DirectionType = directionType,
					Velocity = new Vector2(X, Y)
				},
				Radius = radius,
				StartOffset = new Vector2(X1,Y1),
				Velocity = new Vector2(X2, Y2),

			};
			model.SuccessActions.Add(new DeactivateActionModel()
			{
				Time = subTime
			});

			var action = new CreateHitCircleAction(null, model);

			action.Time.ShouldBe(time);
			action.BoneName.ShouldBe(bone);
			action.Damage.ShouldBe(damage);
			action.TimeDelta.ShouldBe(timeDelta);
			action.ActionDirection.DirectionType.ShouldBe(directionType);
			action.ActionDirection.Velocity.X.ShouldBe(X);
			action.ActionDirection.Velocity.Y.ShouldBe(Y);
			action.Radius.ShouldBe(radius);
			action.StartOffset.X.ShouldBe(X1);
			action.StartOffset.Y.ShouldBe(Y1);
			action.Velocity.X.ShouldBe(X2);
			action.Velocity.Y.ShouldBe(Y2);
			action.SuccessActions.Count.ShouldBe(1);
			var subAction = action.SuccessActions[0] as DeactivateAction;
			subAction.ShouldNotBeNull();
			subAction.Time.ShouldBe(subTime);
		}

		[Test]
		public void ActionToModel()
		{
			var action = new CreateHitCircleAction(null)
			{
				Time = time,
				BoneName = bone,
				Damage = damage,
				TimeDelta = timeDelta,
				ActionDirection = new ActionDirection()
				{
					DirectionType = directionType,
					Velocity = new Vector2(X, Y)
				},
				Radius = radius,
				StartOffset = new Vector2(X1, Y1),
				Velocity = new Vector2(X2, Y2),
			};
			action.SuccessActions.Add(new DeactivateAction(null)
			{
				Time = subTime
			});

			var model = new CreateHitCircleActionModel(action);

			model.Time.ShouldBe(time);
			model.BoneName.ShouldBe(bone);
			model.Damage.ShouldBe(damage);
			model.TimeDelta.TimeDelta.ShouldBe(timeDelta);
			model.Direction.DirectionType.ShouldBe(directionType);
			model.Direction.Velocity.X.ShouldBe(X);
			model.Direction.Velocity.Y.ShouldBe(Y);
			model.Radius.ShouldBe(radius);
			model.StartOffset.X.ShouldBe(X1);
			model.StartOffset.Y.ShouldBe(Y1);
			model.Velocity.X.ShouldBe(X2);
			model.Velocity.Y.ShouldBe(Y2);
			model.SuccessActions.Count.ShouldBe(1);
			var subAction = model.SuccessActions[0] as DeactivateActionModel;
			subAction.ShouldNotBeNull();
			subAction.Time.ShouldBe(subTime);
		}

		[Test]
		public void Persist()
		{
			var model = new CreateHitCircleActionModel()
			{
				Time = time,
				BoneName = bone,
				Damage = damage,
				TimeDelta = new TimedActionModel(timeDelta),
				Direction = new DirectionActionModel()
				{
					DirectionType = directionType,
					Velocity = new Vector2(X, Y),
				},
				Radius = radius,
				StartOffset = new Vector2(X1, Y1),
				Velocity = new Vector2(X2, Y2),
			};
			model.SuccessActions.Add(new DeactivateActionModel()
			{
				Time = subTime
			});

			//write the action out
			var container = new SingleStateContainerModel(new Filename("CreateHitCircleTests.xml"));
			var actions = new StateActionsModel();
			container.StatesActions.Add(actions);
			actions.StateActions.Add(model);
			container.WriteXml();

			//read it back in
			var container2 = new SingleStateContainerModel(new Filename("CreateHitCircleTests.xml"));
			container2.ReadXmlFile();

			//get the action
			container2.StatesActions.Count.ShouldBe(1);
			container2.StatesActions[0].StateActions.Count.ShouldBe(1);
			var model2 = container2.StatesActions[0].StateActions[0] as CreateHitCircleActionModel;
			model2.Time.ShouldBe(time);
			model2.BoneName.ShouldBe(bone);
			model2.Damage.ShouldBe(damage);
			model2.TimeDelta.TimeDelta.ShouldBe(timeDelta);
			model2.Direction.DirectionType.ShouldBe(directionType);
			model2.Direction.Velocity.X.ShouldBe(X);
			model2.Direction.Velocity.Y.ShouldBe(Y);
			model2.Radius.ShouldBe(radius);
			model2.StartOffset.X.ShouldBe(X1);
			model2.StartOffset.Y.ShouldBe(Y1);
			model2.Velocity.X.ShouldBe(X2);
			model2.Velocity.Y.ShouldBe(Y2);
			model2.SuccessActions.Count.ShouldBe(1);
			var subAction = model2.SuccessActions[0] as DeactivateActionModel;
			subAction.ShouldNotBeNull();
			subAction.Time.ShouldBe(subTime);
		}
	}
}
