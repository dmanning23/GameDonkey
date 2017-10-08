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

		public string Bone { get; private set; }
		public bool UseBoneRotation { get; private set; }
		public Vector2 StartOffset { get; private set; }
		public DirectionActionModel Direction  { get; private set; }
		public EmitterTemplate Emitter { get; private set; }

		#endregion //Properties

		#region Methods

		public ParticleEffectActionModel()
		{
			Direction = new DirectionActionModel();
			StartOffset = Vector2.Zero;
			Emitter = new EmitterTemplate();
		}

		public override bool Compare(BaseActionModel inst)
		{
			if (!base.Compare(inst))
			{
				return false;
			}

			var stateAction = inst as ParticleEffectActionModel;
			if (null == stateAction)
			{
				return false;
			}

			if (Bone != stateAction.Bone)
			{
				return false;
			}

			if (UseBoneRotation != stateAction.UseBoneRotation)
			{
				return false;
			}

			if (!StartOffset.AlmostEqual(stateAction.StartOffset))
			{
				return false;
			}

			if (!Direction.Compare(stateAction.Direction))
			{
				return false;
			}

			return true;
		}

		public override void ParseXmlNode(XmlNode node)
		{
			//what is in this node?
			var name = node.Name;
			var value = node.InnerText;

			switch (name)
			{
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
				case "Direction":
					{
						XmlFileBuddy.ReadChildNodes(node, Direction.ParseXmlNode);
					}
					break;
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

#if !WINDOWS_UWP

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

#endif

		#endregion //Methods
	}
}
