using MathNet.Numerics;
using System;
using System.Xml;
using XmlBuddy;

namespace GameDonkeyLib
{
	public class ConstantDeccelerationActionModel : BaseActionModel, IDirectionalActionModel
	{
		#region Properties

		public override EActionType ActionType
		{
			get
			{
				return EActionType.ConstantDecceleration;
			}
		}

		public float MinYVelocity { get; private set; }
		public DirectionActionModel Direction { get; private set; }

		#endregion //Properties

		#region Initialization

		public ConstantDeccelerationActionModel()
		{
			Direction = new DirectionActionModel();
		}

		public ConstantDeccelerationActionModel(ConstantDeccelerationAction action) : base(action)
		{
			MinYVelocity = action.MinYVelocity;
			Direction = new DirectionActionModel(action.Velocity);
		}

		public ConstantDeccelerationActionModel(BaseAction action) : this(action as ConstantDeccelerationAction)
		{
		}

		#endregion //Initialization

		#region Methods

		public override bool Compare(BaseActionModel inst)
		{
			var stateAction = inst as ConstantDeccelerationActionModel;
			if (null == stateAction)
			{
				return false;
			}

			if (!MinYVelocity.AlmostEqual(stateAction.MinYVelocity))
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
				case "MinYVelocity":
					{
						MinYVelocity = Convert.ToSingle(value);
					}
					break;
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
			xmlWriter.WriteAttributeString("MinYVelocity", MinYVelocity.ToString());
			Direction.WriteXmlNodes(xmlWriter);
		}

#endif

		#endregion //Methods
	}
}
