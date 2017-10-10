using MathNet.Numerics;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.Xml;

namespace GameDonkeyLib
{
	public class TrailActionModel : BaseActionModel, ITimedActionModel
	{
		#region Properties

		public override EActionType ActionType
		{
			get
			{
				return EActionType.Trail;
			}
		}

		public Color Color { get; private set; }
		public float LifeDelta { get; private set; }
		public float SpawnDelta { get; private set; }
		public TimedActionModel TimeDelta { get; private set; }

		#endregion //Properties

		#region Initialization

		public TrailActionModel()
		{
			Color = Color.White;
			TimeDelta = new TimedActionModel();
		}

		public TrailActionModel(TrailAction action) : base(action)
		{
			Color = action.StartColor;
			LifeDelta = action.TrailLifeDelta;
			SpawnDelta = action.SpawnDelta;
			TimeDelta = new TimedActionModel(action);
		}

		public TrailActionModel(BaseAction action) : this(action as TrailAction)
		{
		}

		#endregion //Initialization

		#region Methods

		public override bool Compare(BaseActionModel inst)
		{
			if (!base.Compare(inst))
			{
				return false;
			}

			var stateAction = inst as TrailActionModel;
			if (null == stateAction)
			{
				return false;
			}

			if (!TimeDelta.Compare(stateAction.TimeDelta))
			{
				return false;
			}

			if (Color != stateAction.Color)
			{
				return false;
			}

			if (!LifeDelta.AlmostEqual(stateAction.LifeDelta))
			{
				return false;
			}

			if (!SpawnDelta.AlmostEqual(stateAction.SpawnDelta))
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
				case "Color":
					{
						Color = JsonConvert.DeserializeObject<Color>(value);
					}
					break;
				case "LifeDelta":
					{
						LifeDelta = Convert.ToSingle(value);
					}
					break;
				case "SpawnDelta":
					{
						SpawnDelta = Convert.ToSingle(value);
					}
					break;
				case "TimeDelta":
					{
						TimeDelta.ParseXmlNode(node);
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
			TimeDelta.WriteXmlNodes(xmlWriter);
			xmlWriter.WriteAttributeString("LifeDelta", LifeDelta.ToString());
			xmlWriter.WriteAttributeString("SpawnDelta", SpawnDelta.ToString());
			xmlWriter.WriteAttributeString("Color", JsonConvert.SerializeObject(Color));
		}

#endif

		#endregion //Methods
	}
}
