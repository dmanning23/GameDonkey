using FilenameBuddy;
using System;
using System.Collections.Generic;
using System.Xml;
using XmlBuddy;

namespace GameDonkeyLib
{
	public class BaseObjectModel : XmlFileBuddy
	{
		#region Properties

		public Filename Model { get; private set; }
		public Filename Animations { get; private set; }
		public List<Filename> Garments { get; private set; }
		public List<StateContainerModel> States { get; private set; }
		public float Height { get; private set; }
		public float Scale { get; private set; }

		#endregion //Properties

		#region Methods

		public BaseObjectModel(string contentName, Filename filename) : base(contentName, filename)
		{
			Model = new Filename();
			Animations = new Filename();
			States = new List<StateContainerModel>();
			Garments = new List<Filename>();
			Height = 0f;
			Scale = 1f;
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
				case "Asset":
					{
						//skip these old ass nodes
						XmlFileBuddy.ReadChildNodes(node, ParseXmlNode);
					}
					break;
				case "Type":
					{
						//Really skip these old ass nodes
					}
					break;
				case "model":
					{
						Model.SetRelFilename(value);
					}
					break;
				case "animations":
					{
						Animations.SetRelFilename(value);
					}
					break;
				case "model1":
					{
						Model.SetFilenameRelativeToPath(Filename, value);
					}
					break;
				case "animations1":
					{
						Animations.SetFilenameRelativeToPath(Filename, value);
					}
					break;
				case "garments":
					{
						XmlFileBuddy.ReadChildNodes(node, ReadGarment);
					}
					break;
				case "garments1":
					{
						XmlFileBuddy.ReadChildNodes(node, ReadGarment1);
					}
					break;
				case "states":
					{
						XmlFileBuddy.ReadChildNodes(node, ReadStateContainer);
					}
					break;
				case "height":
					{
						Height = Convert.ToInt32(value);
					}
					break;
				case "scale":
					{
						Scale = Convert.ToSingle(value);
					}
					break;
				default:
					{
						NodeError(node);
					}
					break;
			}
		}

		public void ReadGarment(XmlNode node)
		{
			Filename garmentFile = new Filename(node.InnerText);
			Garments.Add(garmentFile);
		}

		public void ReadGarment1(XmlNode node)
		{
			Filename garmentFile = new Filename();
			garmentFile.SetFilenameRelativeToPath(Filename, node.InnerText);
			Garments.Add(garmentFile);
		}

		public void ReadStateContainer(XmlNode node)
		{
			var states = new StateContainerModel(this);
			XmlFileBuddy.ReadChildNodes(node, states.ParseXmlNode);
			States.Add(states);
		}

		/// <summary>
		/// Write this dude out to the xml format
		/// </summary>
		/// <param name="xmlWriter">the xml file to add this dude as a child of</param>
		public override void WriteXmlNodes(XmlTextWriter xmlWriter)
		{
			//			//write out the item tag
			//			xmlWriter.WriteStartElement("joint");
			//			xmlWriter.WriteAttributeString("name", Name);
			//			xmlWriter.WriteEndElement();
		}

		#endregion //File IO
	}
}
