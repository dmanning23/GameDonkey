using FilenameBuddy;
using Microsoft.Xna.Framework.Content;
using ParticleBuddy;
using RenderBuddy;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameDonkeyLib
{
	public class ParticleEffectCollection
	{
		#region Properties

		/// <summary>
		/// a list of all the default particle effects used int he game
		/// </summary>
		private List<EmitterTemplate> DefaultParticles { get; set; }

		#endregion //Properties

		#region Methods

		public ParticleEffectCollection()
		{
			DefaultParticles = new List<EmitterTemplate>();

			foreach (var effect in Enum.GetNames(typeof(DefaultParticleEffect)))
			{
				DefaultParticles.Add(new EmitterTemplate());
			}
		}

		public void LoadParticleEffect(DefaultParticleEffect effect, ContentManager xmlContent, IRenderer renderer, string file)
		{
			var emitter = new EmitterTemplate(new Filename(file));
			emitter.ReadXmlFile(xmlContent);
			emitter.LoadContent(renderer);
			DefaultParticles[(int)effect] = emitter;
		}

		public void LoadParticleEffect(DefaultParticleEffect effect, IRenderer renderer, string file)
		{
			var emitter = new EmitterTemplate(new Filename(file));
			emitter.ReadXmlFile();
			emitter.LoadContent(renderer);
			DefaultParticles[(int)effect] = emitter;
		}

		public EmitterTemplate GetEmitterTemplate(DefaultParticleEffect effect)
		{
			return DefaultParticles[(int)effect];
		}

		#endregion //Methods
	}
}
