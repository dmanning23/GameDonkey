using FilenameBuddy;
using GameDonkeyLib;
using NUnit.Framework;
using Shouldly;

namespace GameDonkey.Tests
{
	[TestFixture]
	public class PlaySoundTests
	{
		const float time = 1f;
		const string filename = "catpants.jpg";

		[SetUp]
		public void Setup()
		{
			Filename.SetCurrentDirectory(@"C:\Projects\gamedonkey\GameDonkey\GameDonkey.Tests\");
		}

		[Test]
		public void ModelToAction()
		{
			var model = new PlaySoundActionModel()
			{
				Time = time,
				Filename = new Filename(filename),
			};

			var action = new PlaySoundAction(null, model);

			action.Time.ShouldBe(time);
			action.SoundCueName.GetFile().ShouldBe(filename);
		}

		[Test]
		public void ActionToModel()
		{
			var action = new PlaySoundAction(null)
			{
				Time = time,
				SoundCueName = new Filename(filename),
			};

			var model = new PlaySoundActionModel(action);

			model.Time.ShouldBe(time);
			model.Filename.GetFile().ShouldBe(filename);
		}

		[Test]
		public void Persist()
		{
			var model = new PlaySoundActionModel()
			{
				Time = time,
				Filename = new Filename(filename),
			};

			//write the action out
			var container = new SingleStateContainerModel(new Filename("PlaySoundTests.xml"));
			var actions = new StateActionsModel();
			container.StatesActions.Add(actions);
			actions.StateActions.Add(model);
			container.WriteXml();

			//read it back in
			var container2 = new SingleStateContainerModel(new Filename("PlaySoundTests.xml"));
			container2.ReadXmlFile();

			//get the action
			container2.StatesActions.Count.ShouldBe(1);
			container2.StatesActions[0].StateActions.Count.ShouldBe(1);
			var model2 = container2.StatesActions[0].StateActions[0] as PlaySoundActionModel;
			model2.ShouldNotBeNull();
			model2.ActionType.ShouldBe(EActionType.PlaySound);
			model2.Time.ShouldBe(time);
			model2.Filename.GetFile().ShouldBe(filename);
		}
	}
}

