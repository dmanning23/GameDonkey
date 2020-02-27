using Microsoft.Xna.Framework;
using ParticleBuddy;
using System;
using System.Xml;
using Vector2Extensions;
using XmlBuddy;

namespace GameDonkeyLib
{
	public class ParticleEffectActionModel : BaseActionModel, IDirectionalActionModel
	{
		#region Properties

		public override EActionType ActionType
		{
			get
			{
				return EActionType.ParticleEffect;
			}
		}

		public string Bone { get; set; }
		public bool UseBoneRotation { get; set; }
		public Vector2 StartOffset { get; set; }
		public DirectionActionModel Direction  { get; set; }
		public EmitterTemplate Emitter { get; set; }

		#endregion //Properties

		#region Initialization

		public ParticleEffectActionModel()
		{
			Direction = new DirectionActionModel();
			StartOffset = Vector2.Zero;
			Emitter = new EmitterTemplate();
		}

		public ParticleEffectActionModel(ParticleEffectAction action) : base(action)
		{
			Bone = action.BoneName;
			UseBoneRotation = action.UseBoneRotation;
			StartOffset = action.StartOffset;
			Direction = new DirectionActionModel(action.Velocity);
			Emitter = new EmitterTemplate(action.Emitter);
		}

		public ParticleEffectActionModel(BaseAction action) : this(action as ParticleEffectAction)
		{
		}

		#endregion //Initialization

		#region Methods

		public override void ParseXmlNode(XmlNode node)
		{
			//what is in this node?
			var name = node.Name;
			var value = node.InnerText;

			switch (name)
			{
				case "bone":
				case "Bone":
					{
						Bone = value;
					}
					break;
				case "UseBoneRotation":
					{
						UseBoneRotation = Convert.ToBoolean(value);
					}
					break;
				case "StartOffset":
					{
						StartOffset = value.ToVector2();
					}
					break;
				case "direction":
				case "Direction":
					{
						XmlFileBuddy.ReadChildNodes(node, Direction.ParseXmlNode);
					}
					break;
				case "emitter":
				case "Emitter":
					{
						XmlFileBuddy.ReadChildNodes(node, Emitter.ParseXmlNode);
					}
					break;
				default:
					{
						base.ParseXmlNode(node);
					}
					break;
			}
		}

		protected override void WriteActionXml(XmlTextWriter xmlWriter)
		{
			xmlWriter.WriteAttributeString("Bone", Bone);
			xmlWriter.WriteAttributeString("UseBoneRotation", UseBoneRotation.ToString());
			xmlWriter.WriteAttributeString("StartOffset", StartOffset.StringFromVector());
			Direction.WriteXmlNodes(xmlWriter);

			xmlWriter.WriteStartElement("Emitter");
			Emitter.WriteXmlNodes(xmlWriter);
			xmlWriter.WriteEndElement();
		}

		#endregion //Methods
	}
}
