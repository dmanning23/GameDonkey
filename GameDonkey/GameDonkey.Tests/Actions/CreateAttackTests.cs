using FilenameBuddy;
using GameDonkeyLib;
using Microsoft.Xna.Framework;
using NUnit.Framework;
using Shouldly;

namespace GameDonkey.Tests
{
	[TestFixture]
	public class CreateAttackTests
	{
		const float time = 1f;
		const string bone = "catpants";
		const float timeDelta = 2f;
		const float subTime = 3f;
		const float damage = 4f;
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
			var model = new CreateAttackActionModel()
			{
				Time = time,
				BoneName = bone,
				Damage = damage,
				TimeDelta = new TimedActionModel(timeDelta),
				Direction = new DirectionActionModel()
				{
					DirectionType = directionType,
					Velocity = new Vector2(X, Y)
				}
			};
			model.ActionModels.ActionModels.Add(new DeactivateActionModel()
			{
				Time = subTime
			});

			var action = new CreateAttackAction(null, model, null);

			action.Time.ShouldBe(time);
			action.BoneName.ShouldBe(bone);
			action.Damage.ShouldBe(damage);
			action.TimeDelta.ShouldBe(timeDelta);
			action.ActionDirection.DirectionType.ShouldBe(directionType);
			action.ActionDirection.Velocity.X.ShouldBe(X);
			action.ActionDirection.Velocity.Y.ShouldBe(Y);
			action.Actions.Count.ShouldBe(1);
			var subAction = action.Actions[0] as DeactivateAction;
			subAction.ShouldNotBeNull();
			subAction.Time.ShouldBe(subTime);
		}

		[Test]
		public void ActionToModel()
		{
			var action = new CreateAttackAction(null)
			{
				Time = time,
				BoneName = bone,
				Damage = damage,
				TimeDelta = timeDelta,
				ActionDirection = new ActionDirection()
				{
					DirectionType = directionType,
					Velocity = new Vector2(X, Y)
				}
			};
			action.Actions.Add(new DeactivateAction(null)
			{
				Time = subTime
			});

			var model = new CreateAttackActionModel(action);

			model.Time.ShouldBe(time);
			model.BoneName.ShouldBe(bone);
			model.Damage.ShouldBe(damage);
			model.TimeDelta.TimeDelta.ShouldBe(timeDelta);
			model.Direction.DirectionType.ShouldBe(directionType);
			model.Direction.Velocity.X.ShouldBe(X);
			model.Direction.Velocity.Y.ShouldBe(Y);
			model.ActionModels.ActionModels.Count.ShouldBe(1);
			var subAction = model.ActionModels.ActionModels[0] as DeactivateActionModel;
			subAction.ShouldNotBeNull();
			subAction.Time.ShouldBe(subTime);
		}

		[Test]
		public void Persist()
		{
			var model = new CreateAttackActionModel()
			{
				Time = time,
				BoneName = bone,
				Damage = damage,
				TimeDelta = new TimedActionModel(timeDelta),
				Direction = new DirectionActionModel()
				{
					DirectionType = directionType,
					Velocity = new Vector2(X, Y)
				}
			};
			model.ActionModels.ActionModels.Add(new DeactivateActionModel()
			{
				Time = subTime
			});

			//write the action out
			var container = new SingleStateContainerModel(new Filename("CreateAttackTests.xml"));
			var actions = new SingleStateActionsModel();
			container.StatesActions.Add(actions);
			actions.ActionModels.Add(model);
			container.WriteXml();

			//read it back in
			var container2 = new SingleStateContainerModel(new Filename("CreateAttackTests.xml"));
			container2.ReadXmlFile();

			//get the action
			container2.StatesActions.Count.ShouldBe(1);
			container2.StatesActions[0].ActionModels.Count.ShouldBe(1);
			var model2 = container2.StatesActions[0].ActionModels[0] as CreateAttackActionModel;
			model2.Time.ShouldBe(time);
			model2.BoneName.ShouldBe(bone);
			model2.Damage.ShouldBe(damage);
			model2.TimeDelta.TimeDelta.ShouldBe(timeDelta);
			model2.Direction.DirectionType.ShouldBe(directionType);
			model2.Direction.Velocity.X.ShouldBe(X);
			model2.Direction.Velocity.Y.ShouldBe(Y);
			model2.ActionModels.ActionModels.Count.ShouldBe(1);
			var subAction = model2.ActionModels.ActionModels[0] as DeactivateActionModel;
			subAction.ShouldNotBeNull();
			subAction.Time.ShouldBe(subTime);
		}
	}
}
