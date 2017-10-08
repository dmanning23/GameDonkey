using MathNet.Numerics;
using System;
using System.Xml;
using XmlBuddy;

namespace GameDonkeyLib
{
	/// <summary>
	/// Contains all the base variables for serializing a state action
	/// </summary>
	public abstract class BaseActionModel : XmlObject
	{
		#region Properties

		public abstract EActionType ActionType { get; }
		public float Time { get; set; }

		#endregion //Properties

		#region Methods

		public BaseActionModel()
		{
		}

		public virtual bool Compare(BaseActionModel inst)
		{
			return ((ActionType == inst.ActionType) && !Time.AlmostEqual(inst.Time));
		}

		public override void ParseXmlNode(XmlNode node)
		{
			//what is in this node?
			var name = node.Name;
			var value = node.InnerText;

			switch (name)
			{
				case "Time":
					{
						Time = Convert.ToSingle(value);
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
			//write out the type
			xmlWriter.WriteStartElement(ActionType.ToString());

			xmlWriter.WriteAttributeString("Time", Time.ToString());

			WriteActionXml(xmlWriter);

			xmlWriter.WriteEndElement();
		}

		/// <summary>
		/// overloaded in child classes to write out action specific stuff
		/// </summary>
		/// <param name="rXMLFile"></param>
		protected abstract void WriteActionXml(XmlTextWriter xmlWriter);

#endif

		#endregion //Methods
	}
}
