using FilenameBuddy;
using System.Xml;
using XmlBuddy;

namespace GameDonkeyLib
{
	/// <summary>
	/// This is an xml node that contains the data to load one state container
	/// </summary>
	public class StateContainerModel : XmlObject
	{
		#region Properties

		public Filename StateMachineFilename { get; set; }

		public Filename StateActionsFilename { get; set; }

		#endregion //Properties

		#region Methods

		/// <summary>
		/// hello, standard constructor!
		/// </summary>
		public StateContainerModel()
		{
			StateMachineFilename = new Filename();
			StateActionsFilename = new Filename();
		}

		#endregion //Methods

		#region File IO

		public override void ParseXmlNode(XmlNode node)
		{
			//what is in this node?
			var name = node.Name;
			var value = node.InnerText;

			switch (name)
			{
				case "Type":
					{
						//throw these attributes out
					}
					break;
				case "stateMachine":
					{
						StateMachineFilename.SetRelFilename(value);
					}
					break;
				case "stateActions":
					{
						StateActionsFilename.SetRelFilename(value);
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
		/// <summary>
		/// Write this dude out to the xml format
		/// </summary>
		/// <param name="xmlWriter">the xml file to add this dude as a child of</param>
		public override void WriteXmlNodes(XmlTextWriter xmlWriter)
		{
			//write out the item tag
			xmlWriter.WriteStartElement("StateContainer");
			xmlWriter.WriteAttributeString("stateMachine", StateMachineFilename.GetRelFilename());
			xmlWriter.WriteAttributeString("stateActions", StateActionsFilename.GetRelFilename());
			xmlWriter.WriteEndElement();
		}
#endif

		#endregion //File IO
	}
}
