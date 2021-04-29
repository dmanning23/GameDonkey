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

		public string Id { get; set; }

		#endregion //Properties

		#region Initialization

		public BaseActionModel()
		{
		}

		public BaseActionModel(BaseAction action)
		{
			Time = action.Time;
			Id = action.Id;
		}

		#endregion //Initialization

		#region Methods

		public override void ParseXmlNode(XmlNode node)
		{
			//what is in this node?
			var name = node.Name;
			var value = node.InnerText;

			switch (name.ToLower())
			{
				case "type":
					{
						//legacy shit, ignore it
					}
					break;
				case "time":
					{
						Time = Convert.ToSingle(value);
					}
					break;
					case "id":
					{
						Id = value;
					}
					break;
				default:
					{
						base.ParseXmlNode(node);
					}
					break;
			}
		}

		public override void WriteXmlNodes(XmlTextWriter xmlWriter)
		{
			//write out the type
			xmlWriter.WriteStartElement(ActionType.ToString());

			if (!string.IsNullOrEmpty(Id))
			{
				xmlWriter.WriteAttributeString("id", Id);
			}

			if (Time != 0f)
			{
				xmlWriter.WriteAttributeString("Time", Time.ToString());
			}

			WriteActionXml(xmlWriter);

			xmlWriter.WriteEndElement();
		}

		/// <summary>
		/// overloaded in child classes to write out action specific stuff
		/// </summary>
		/// <param name="rXMLFile"></param>
		protected abstract void WriteActionXml(XmlTextWriter xmlWriter);

		#endregion //Methods
	}
}
