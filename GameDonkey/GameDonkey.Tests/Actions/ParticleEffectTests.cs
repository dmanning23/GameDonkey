using FilenameBuddy;
using GameDonkeyLib;
using MathNet.Numerics;
using Microsoft.Xna.Framework;
using NUnit.Framework;
using ParticleBuddy;
using Shouldly;

namespace GameDonkey.Tests
{
	[TestFixture]
	public class ParticleEffectTests
	{
		const float time = 1f;
		const string bone = "catpants";
		const bool useBoneRotation = true;
		const EDirectionType directionType = EDirectionType.NegController;
		const float X = 2f;
		const float Y = 3f;
		const float X1 = 4f;
		const float Y1 = 5f;

		[SetUp]
		public void Setup()
		{
			Filename.SetCurrentDirectory(@"C:\Projects\gamedonkey\GameDonkey\GameDonkey.Tests\");
		}

		[TestCase(1f, 2f, 3, 4f, true, 5f, 6f, 7f, 8f, 9f, 10f, 11f, 12f, 13f, "catpants.jpg")]
		public void ModelToAction(float fadeSpeed,
			float particleSize,
			int numStartParticles,
			float emitterLife,
			bool expires,
			float particleLife,
			float creationPeriod,
			float particleGravity,
			float minSpin,
			float maxSpin,
			float minScale,
			float maxScale,
			float minStartRotation,
			float maxStartRotation,
			string imageFile)
		{
			var model = new ParticleEffectActionModel()
			{
				Time = time,
				Bone = bone,
				UseBoneRotation = useBoneRotation,
				Direction = new DirectionActionModel()
				{
					DirectionType = directionType,
					Velocity = new Vector2(X, Y)
				},
				StartOffset = new Vector2(X1, Y1),
				Emitter = new EmitterTemplate()
				{
					FadeSpeed = fadeSpeed,
					ParticleSize = particleSize,
					NumStartParticles = numStartParticles,
					EmitterLife = emitterLife,
					Expires = expires,
					ParticleLife = particleLife,
					CreationPeriod = creationPeriod,
					ParticleGravity = particleGravity,
					MinSpin = minSpin,
					MaxSpin = maxSpin,
					MinScale = minScale,
					MaxScale = maxScale,
					MinStartRotation = minStartRotation,
					MaxStartRotation = maxStartRotation,
					ImageFile = new Filename(imageFile)
				}
			};

			var action = new ParticleEffectAction(null, model);

			action.Time.ShouldBe(time);
			action.BoneName.ShouldBe(bone);
			action.UseBoneRotation.ShouldBe(useBoneRotation);
			action.Velocity.DirectionType.ShouldBe(directionType);
			action.Velocity.Velocity.X.ShouldBe(X);
			action.Velocity.Velocity.Y.ShouldBe(Y);
			action.StartOffset.X.ShouldBe(X1);
			action.StartOffset.Y.ShouldBe(Y1);
			action.Emitter.FadeSpeed.ShouldBe(fadeSpeed);
			action.Emitter.ParticleSize.ShouldBe(particleSize);
			action.Emitter.NumStartParticles.ShouldBe(numStartParticles);
			action.Emitter.EmitterLife.ShouldBe(emitterLife);
			action.Emitter.Expires.ShouldBe(expires);
			action.Emitter.ParticleLife.ShouldBe(particleLife);
			action.Emitter.CreationPeriod.ShouldBe(creationPeriod);
			action.Emitter.ParticleGravity.ShouldBe(particleGravity);
			action.Emitter.MinSpin.ShouldBe(minSpin);
			action.Emitter.MaxSpin.ShouldBe(maxSpin);
			action.Emitter.MinScale.ShouldBe(minScale);
			action.Emitter.MaxScale.ShouldBe(maxScale);
			action.Emitter.MinStartRotation.ShouldBe(minStartRotation);
			action.Emitter.MaxStartRotation.ShouldBe(maxStartRotation);
			action.Emitter.ImageFile.File.ShouldBe(new Filename(imageFile).File);
		}

		[TestCase(1f, 2f, 3, 4f, true, 5f, 6f, 7f, 8f, 9f, 10f, 11f, 12f, 13f, "catpants.jpg")]
		public void ActionToModel(float fadeSpeed,
				float particleSize,
				int numStartParticles,
				float emitterLife,
				bool expires,
				float particleLife,
				float creationPeriod,
				float particleGravity,
				float minSpin,
				float maxSpin,
				float minScale,
				float maxScale,
				float minStartRotation,
				float maxStartRotation,
				string imageFile)
		{
			var action = new ParticleEffectAction(null)
			{
				Time = time,
				BoneName = bone,
				UseBoneRotation = useBoneRotation,
				Velocity = new ActionDirection()
				{
					DirectionType = directionType,
					Velocity = new Vector2(X, Y)
				},
				StartOffset = new Vector2(X1, Y1),
				Emitter = new EmitterTemplate()
				{
					FadeSpeed = fadeSpeed,
					ParticleSize = particleSize,
					NumStartParticles = numStartParticles,
					EmitterLife = emitterLife,
					Expires = expires,
					ParticleLife = particleLife,
					CreationPeriod = creationPeriod,
					ParticleGravity = particleGravity,
					MinSpin = minSpin,
					MaxSpin = maxSpin,
					MinScale = minScale,
					MaxScale = maxScale,
					MinStartRotation = minStartRotation,
					MaxStartRotation = maxStartRotation,
					ImageFile = new Filename(imageFile)
				}
			};

			var model = new ParticleEffectActionModel(action);

			model.Time.ShouldBe(time);
			model.Bone.ShouldBe(bone);
			model.UseBoneRotation.ShouldBe(useBoneRotation);
			model.Direction.DirectionType.ShouldBe(directionType);
			model.Direction.Velocity.X.ShouldBe(X);
			model.Direction.Velocity.Y.ShouldBe(Y);
			model.StartOffset.X.ShouldBe(X1);
			model.StartOffset.Y.ShouldBe(Y1);
			model.Emitter.FadeSpeed.ShouldBe(fadeSpeed);
			model.Emitter.ParticleSize.ShouldBe(particleSize);
			model.Emitter.NumStartParticles.ShouldBe(numStartParticles);
			model.Emitter.EmitterLife.ShouldBe(emitterLife);
			model.Emitter.Expires.ShouldBe(expires);
			model.Emitter.ParticleLife.ShouldBe(particleLife);
			model.Emitter.CreationPeriod.ShouldBe(creationPeriod);
			model.Emitter.ParticleGravity.ShouldBe(particleGravity);
			model.Emitter.MinSpin.ShouldBe(minSpin);
			model.Emitter.MaxSpin.ShouldBe(maxSpin);
			model.Emitter.MinScale.ShouldBe(minScale);
			model.Emitter.MaxScale.ShouldBe(maxScale);
			model.Emitter.MinStartRotation.ShouldBe(minStartRotation);
			model.Emitter.MaxStartRotation.ShouldBe(maxStartRotation);
			model.Emitter.ImageFile.File.ShouldBe(new Filename(imageFile).File);
		}

		[TestCase(1f, 2f, 3, 4f, true, 5f, 6f, 7f, 8f, 9f, 10f, 11f, 12f, 13f, "catpants.jpg")]
		public void Persist(float fadeSpeed,
			float particleSize,
			int numStartParticles,
			float emitterLife,
			bool expires,
			float particleLife,
			float creationPeriod,
			float particleGravity,
			float minSpin,
			float maxSpin,
			float minScale,
			float maxScale,
			float minStartRotation,
			float maxStartRotation,
			string imageFile)
		{
			var model = new ParticleEffectActionModel()
			{
				Time = time,
				Bone = bone,
				UseBoneRotation = useBoneRotation,
				Direction = new DirectionActionModel()
				{
					DirectionType = directionType,
					Velocity = new Vector2(X, Y)
				},
				StartOffset = new Vector2(X1, Y1),
				Emitter = new EmitterTemplate()
				{
					FadeSpeed = fadeSpeed,
					ParticleSize = particleSize,
					NumStartParticles = numStartParticles,
					EmitterLife = emitterLife,
					Expires = expires,
					ParticleLife = particleLife,
					CreationPeriod = creationPeriod,
					ParticleGravity = particleGravity,
					MinSpin = minSpin,
					MaxSpin = maxSpin,
					MinScale = minScale,
					MaxScale = maxScale,
					MinStartRotation = minStartRotation,
					MaxStartRotation = maxStartRotation,
					ImageFile = new Filename(imageFile)
				}
			};

			//write the action out
			var container = new SingleStateContainerModel(new Filename("ParticleEffectTests.xml"));
			var actions = new SingleStateActionsModel();
			container.StatesActions.Add(actions);
			actions.ActionModels.Add(model);
			container.WriteXml();

			//read it back in
			var container2 = new SingleStateContainerModel(new Filename("ParticleEffectTests.xml"));
			container2.ReadXmlFile();

			//get the action
			container2.StatesActions.Count.ShouldBe(1);
			container2.StatesActions[0].ActionModels.Count.ShouldBe(1);
			var model2 = container2.StatesActions[0].ActionModels[0] as ParticleEffectActionModel;
			model2.Time.ShouldBe(time);
			model2.Bone.ShouldBe(bone);
			model2.UseBoneRotation.ShouldBe(useBoneRotation);
			model2.Direction.DirectionType.ShouldBe(directionType);
			model2.Direction.Velocity.X.ShouldBe(X);
			model2.Direction.Velocity.Y.ShouldBe(Y);
			model2.StartOffset.X.ShouldBe(X1);
			model2.StartOffset.Y.ShouldBe(Y1);
			model2.Emitter.FadeSpeed.ShouldBe(fadeSpeed);
			model2.Emitter.ParticleSize.ShouldBe(particleSize);
			model2.Emitter.NumStartParticles.ShouldBe(numStartParticles);
			model2.Emitter.EmitterLife.ShouldBe(emitterLife);
			model2.Emitter.Expires.ShouldBe(expires);
			model2.Emitter.ParticleLife.ShouldBe(particleLife);
			model2.Emitter.CreationPeriod.ShouldBe(creationPeriod);
			model2.Emitter.ParticleGravity.ShouldBe(particleGravity);
			model2.Emitter.MinSpin.AlmostEqual(minSpin).ShouldBeTrue();
			//model2.Emitter.MaxSpin.AlmostEqual(maxSpin).ShouldBeTrue();
			model2.Emitter.MinScale.ShouldBe(minScale);
			model2.Emitter.MaxScale.ShouldBe(maxScale);
			model2.Emitter.MinStartRotation.AlmostEqual(minStartRotation).ShouldBeTrue();
			//model2.Emitter.MaxStartRotation.AlmostEqual(maxStartRotation).ShouldBeTrue();
			model2.Emitter.ImageFile.File.ShouldBe(new Filename(imageFile).File);
		}
	}
}
