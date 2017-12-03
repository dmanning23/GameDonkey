using MathNet.Numerics;
using System;
using System.Xml;
using XmlBuddy;

namespace GameDonkeyLib
{
	public class ConstantAccelerationActionModel : BaseActionModel, IDirectionalActionModel
	{
		#region Properties

		public override EActionType ActionType
		{
			get
			{
				return EActionType.ConstantAcceleration;
			}
		}

		public float MaxVelocity { get; set; }
		public DirectionActionModel Direction { get; set; }

		#endregion //Properties

		#region Initialization

		public ConstantAccelerationActionModel()
		{
			Direction = new DirectionActionModel();
		}

		public ConstantAccelerationActionModel(ConstantAccelerationAction action) : base(action)
		{
			MaxVelocity = action.MaxVelocity;
			Direction = new DirectionActionModel(action.Velocity);
		}

		public ConstantAccelerationActionModel(BaseAction action) : this(action as ConstantAccelerationAction)
		{
		}

		#endregion //Initialization

		#region Methods

		public override bool Compare(BaseActionModel inst)
		{
			var stateAction = inst as ConstantAccelerationActionModel;
			if (null == stateAction)
			{
				return false;
			}

			if (!MaxVelocity.AlmostEqual(stateAction.MaxVelocity))
			{
				return false;
			}

			if (!Direction.Compare(stateAction.Direction))
			{
				return false;
			}

			return base.Compare(inst);
		}

		public override void ParseXmlNode(XmlNode node)
		{
			//what is in this node?
			var name = node.Name;
			var value = node.InnerText;

			switch (name)
			{
				case "maxVelocity":
				case "MaxVelocity":
					{
						MaxVelocity = Convert.ToSingle(value);
					}
					break;
				case "direction":
				case "Direction":
					{
						XmlFileBuddy.ReadChildNodes(node, Direction.ParseXmlNode);
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
			xmlWriter.WriteAttributeString("MaxVelocity", MaxVelocity.ToString());
			Direction.WriteXmlNodes(xmlWriter);
		}

#endif

		#endregion //Methods
	}
}
