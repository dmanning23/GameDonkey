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
	public class TargetRotationTests
	{
		const float time = 1f;
		const float timeDelta = 2f;
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
			var model = new TargetRotationActionModel()
			{
				Time = time,
				TimeDelta = new TimedActionModel(timeDelta),
				Direction = new DirectionActionModel()
				{
					DirectionType = directionType,
					Velocity = new Vector2(X, Y)
				}
			};

			var action = new TargetRotationAction(null, model);

			action.Time.ShouldBe(time);
			action.TimeDelta.ShouldBe(timeDelta);
			action.TargetRotation.DirectionType.ShouldBe(directionType);
			action.TargetRotation.Velocity.X.ShouldBe(X);
			action.TargetRotation.Velocity.Y.ShouldBe(Y);
		}

		[Test]
		public void ActionToModel()
		{
			var action = new TargetRotationAction(null)
			{
				Time = time,
				TimeDelta = timeDelta,
				TargetRotation = new ActionDirection()
				{
					DirectionType = directionType,
					Velocity = new Vector2(X, Y)
				}
			};

			var model = new TargetRotationActionModel(action);

			model.Time.ShouldBe(time);
			model.TimeDelta.TimeDelta.ShouldBe(timeDelta);
			model.Direction.DirectionType.ShouldBe(directionType);
			model.Direction.Velocity.X.ShouldBe(X);
			model.Direction.Velocity.Y.ShouldBe(Y);
		}

		[Test]
		public void Persist()
		{
			var model = new TargetRotationActionModel()
			{
				Time = time,
				TimeDelta = new TimedActionModel(timeDelta),
				Direction = new DirectionActionModel()
				{
					DirectionType = directionType,
					Velocity = new Vector2(X, Y)
				}
			};

			//write the action out
			var container = new SingleStateContainerModel(new Filename("TargetRotationTests.xml"));
			var actions = new SingleStateActionsModel();
			container.StatesActions.Add(actions);
			actions.ActionModels.Add(model);
			container.WriteXml();

			//read it back in
			var container2 = new SingleStateContainerModel(new Filename("TargetRotationTests.xml"));
			container2.ReadXmlFile();

			//get the action
			container2.StatesActions.Count.ShouldBe(1);
			container2.StatesActions[0].ActionModels.Count.ShouldBe(1);
			var model2 = container2.StatesActions[0].ActionModels[0] as TargetRotationActionModel;
			model2.Time.ShouldBe(time);
			model2.TimeDelta.TimeDelta.ShouldBe(timeDelta);
			model2.Direction.DirectionType.ShouldBe(directionType);
			model2.Direction.Velocity.X.ShouldBe(X);
			model2.Direction.Velocity.Y.ShouldBe(Y);
		}
	}
}
