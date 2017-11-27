using MathNet.Numerics;
using Microsoft.Xna.Framework;
using System;
using System.Xml;

namespace GameDonkeyLib
{
	public class RotateActionModel : BaseActionModel
	{
		#region Properties

		public override EActionType ActionType
		{
			get
			{
				return EActionType.Rotate;
			}
		}

		public float Rotation { get; set; }

		#endregion //Properties

		#region Initialization

		public RotateActionModel()
		{
		}

		public RotateActionModel(RotateAction action) : base(action)
		{
			Rotation = action.Rotation;
		}

		public RotateActionModel(BaseAction action) : this(action as RotateAction)
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

			var stateAction = inst as RotateActionModel;
			if (null == stateAction)
			{
				return false;
			}

			if (!Rotation.AlmostEqual(stateAction.Rotation))
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
				case "Rotation":
					{
						Rotation = Convert.ToSingle(value);
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
			xmlWriter.WriteAttributeString("Rotation", Rotation.ToString());
		}

#endif

		#endregion //Methods
	}
}
