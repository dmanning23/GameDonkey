using Microsoft.Xna.Framework;
using System;
using System.Xml;
using Vector2Extensions;

namespace GameDonkeyLib
{
	public class PointLightActionModel : BaseActionModel
	{
		#region Properties

		public override EActionType ActionType
		{
			get
			{
				return EActionType.PointLight;
			}
		}

		public string Bone { get; set; }
		public Vector3 StartOffset { get; set; }

		public Color LightColor { get; set; }

		public float AttackTimeDelta { get; set; }
		public float SustainTimeDelta { get; set; }
		public float DelayTimeDelta { get; set; }

		public float FlareTimeDelta { get; set; }
		public float MinBrightness { get; set; }
		public float MaxBrightness { get; set; }

		#endregion //Properties

		#region Initialization

		public PointLightActionModel()
		{
			StartOffset = new Vector3(0f, 0f, 20f);
			LightColor = Color.White;
			AttackTimeDelta = 1f;
			SustainTimeDelta = 1f;
			DelayTimeDelta = 1f;
			FlareTimeDelta = 0.05f;
			MinBrightness = 100f;
			MaxBrightness = 100f;
		}

		public PointLightActionModel(PointLightAction action) : base(action)
		{
			Bone = action.BoneName;
			StartOffset = action.StartOffset;
			LightColor = action.LightColor;
			AttackTimeDelta = action.AttackTimeDelta;
			SustainTimeDelta = action.SustainTimeDelta;
			DelayTimeDelta = action.DelayTimeDelta;
			FlareTimeDelta = action.FlareTimeDelta;
			MinBrightness = action.MinBrightness;
			MaxBrightness = action.MaxBrightness;
		}

		public PointLightActionModel(BaseAction action) : this(action as PointLightAction)
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
				case "StartOffset":
					{
						StartOffset = value.ToVector3();
					}
					break;
				case "LightColor":
					{
						LightColor = value.ToColor();
					}
					break;
				case "AttackTimeDelta":
					{
						AttackTimeDelta = Convert.ToSingle(value);
					}
					break;
				case "SustainTimeDelta":
					{
						SustainTimeDelta = Convert.ToSingle(value);
					}
					break;
				case "DelayTimeDelta":
					{
						DelayTimeDelta = Convert.ToSingle(value);
					}
					break;
				case "FlareTimeDelta":
					{
						FlareTimeDelta = Convert.ToSingle(value);
					}
					break;
				case "MinBrightness":
					{
						MinBrightness = Convert.ToSingle(value);
					}
					break;
				case "MaxBrightness":
					{
						MaxBrightness = Convert.ToSingle(value);
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
			xmlWriter.WriteAttributeString("StartOffset", StartOffset.StringFromVector());
			xmlWriter.WriteAttributeString("LightColor", LightColor.StringFromColor());
			xmlWriter.WriteAttributeString("AttackTimeDelta", AttackTimeDelta.ToString());
			xmlWriter.WriteAttributeString("SustainTimeDelta", SustainTimeDelta.ToString());
			xmlWriter.WriteAttributeString("DelayTimeDelta", DelayTimeDelta.ToString());
			xmlWriter.WriteAttributeString("FlareTimeDelta", FlareTimeDelta.ToString());
			xmlWriter.WriteAttributeString("MinBrightness", MinBrightness.ToString());
			xmlWriter.WriteAttributeString("MaxBrightness", MaxBrightness.ToString());
		}

#endif

		#endregion //Methods
	}
}