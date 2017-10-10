using MathNet.Numerics;
using System;
using System.Xml;
using XmlBuddy;

namespace GameDonkeyLib
{
	public class TimedActionModel : XmlObject
	{
		#region Properties

		public float TimeDelta { get; set; }

		#endregion //Properties

		#region Initialization

		public TimedActionModel()
		{
		}

		public TimedActionModel(TimedAction action)
		{
			TimeDelta = action.TimeDelta;
		}

		public TimedActionModel(float timeDelta)
		{
			TimeDelta = timeDelta;
		}

		#endregion //Initialization

		#region Methods

		public TimedActionModel(TimedActionModel inst)
		{
			TimeDelta = inst.TimeDelta;
		}

		public bool Compare(TimedActionModel inst)
		{
			return TimeDelta.AlmostEqual(inst.TimeDelta);
		}

		public override void ParseXmlNode(XmlNode node)
		{
			//what is in this node?
			var name = node.Name;
			var value = node.InnerText;

			switch (name)
			{
				case "TimeDelta":
					{
						TimeDelta = Convert.ToSingle(value);
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

		public override void WriteXmlNodes(XmlTextWriter xmlWriter)
		{
			xmlWriter.WriteAttributeString("TimeDelta", TimeDelta.ToString());
		}

#endif

		#endregion //Methods
	}
}
