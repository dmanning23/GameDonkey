using AnimationLib;
using FilenameBuddy;
using GameDonkeyLib;
using NUnit.Framework;
using Shouldly;

namespace GameDonkey.Tests
{
	[TestFixture]
	public class PlayAnimationTests
	{
		const float time = 1f;
		const string animation = "catpants";
		const EPlayback playback = EPlayback.LoopBackwards;

		[SetUp]
		public void Setup()
		{
			Filename.SetCurrentDirectory(@"C:\Projects\gamedonkey\GameDonkey\GameDonkey.Tests\");
		}

		[Test]
		public void ModelToAction()
		{
			var model = new PlayAnimationActionModel()
			{
				Time = time,
				Animation = animation,
				Playback = playback
			};

			var action = new PlayAnimationAction(null, model);

			action.Time.ShouldBe(time);
			action.AnimationName.ShouldBe(animation);
			action.PlaybackMode.ShouldBe(playback);
		}

		[Test]
		public void ActionToModel()
		{
			var action = new PlayAnimationAction(null)
			{
				Time = time,
				AnimationName = animation,
				PlaybackMode = playback
			};

			var model = new PlayAnimationActionModel(action);

			model.Time.ShouldBe(time);
			model.Animation.ShouldBe(animation);
			model.Playback.ShouldBe(playback);
		}

		[Test]
		public void Persist()
		{
			var model = new PlayAnimationActionModel()
			{
				Time = time,
				Animation = animation,
				Playback = playback
			};

			//write the action out
			var container = new SingleStateContainerModel(new Filename("PlayAnimationTests.xml"));
			var actions = new StateActionsModel();
			container.StatesActions.Add(actions);
			actions.StateActions.Add(model);
			container.WriteXml();

			//read it back in
			var container2 = new SingleStateContainerModel(new Filename("PlayAnimationTests.xml"));
			container2.ReadXmlFile();

			//get the action
			container2.StatesActions.Count.ShouldBe(1);
			container2.StatesActions[0].StateActions.Count.ShouldBe(1);
			var model2 = container2.StatesActions[0].StateActions[0] as PlayAnimationActionModel;
			model2.ShouldNotBeNull();
			model2.ActionType.ShouldBe(EActionType.PlayAnimation);
			model2.Time.ShouldBe(time);
			model2.Animation.ShouldBe(animation);
			model2.Playback.ShouldBe(playback);
		}
	}
}
