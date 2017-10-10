using MathNet.Numerics;
using System;
using System.Xml;

namespace GameDonkeyLib
{
	public class CameraShakeActionModel : BaseActionModel, ITimedActionModel
	{
		#region Properties

		public override EActionType ActionType
		{
			get
			{
				return EActionType.CameraShake;
			}
		}

		public float ShakeAmount { get; private set; }
		public TimedActionModel TimeDelta { get; private set; }

		#endregion //Properties

		#region Initialization

		public CameraShakeActionModel()
		{
			TimeDelta = new TimedActionModel();
		}

		public CameraShakeActionModel(CameraShakeAction action) : base(action)
		{
			ShakeAmount = action.ShakeAmount;
			TimeDelta = new TimedActionModel(action.TimeDelta);
		}

		public CameraShakeActionModel(BaseAction action) : this(action as CameraShakeAction)
		{
		}

		#endregion //Initialization

		#region Methods
		
		public override bool Compare(BaseActionModel inst)
		{
			var stateAction = inst as CameraShakeActionModel;
			if (null == stateAction)
			{
				return false;
			}

			if (!ShakeAmount.AlmostEqual(stateAction.ShakeAmount))
			{
				return false;
			}

			if (!TimeDelta.Compare(stateAction.TimeDelta))
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
				case "ShakeAmount":
					{
						ShakeAmount = Convert.ToSingle(value);
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
			xmlWriter.WriteAttributeString("ShakeAmount", ShakeAmount.ToString());
			TimeDelta.WriteXmlNodes(xmlWriter);
		}

#endif

		#endregion //Methods
	}
}
