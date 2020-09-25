using FilenameBuddy;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using XmlBuddy;

namespace GameDonkeyLib
{
	public class BackgroundLayerModel : XmlObject
	{
		#region Properties

		BoardModel BoardModel { get; set; }

		public Filename ImageFile { get; set; }

		public float Scale { get; set; }

		#endregion //Properties

		#region Methods

		/// <summary>
		/// hello, standard constructor!
		/// </summary>
		public BackgroundLayerModel(BoardModel boardModel)
		{
			ImageFile = new Filename();
			BoardModel = boardModel;
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
				case "image":
					{
						ImageFile.SetRelFilename(value);
					}
					break;
				case "imageFile1":
					{
						//read in a relative to the model file
						ImageFile.SetFilenameRelativeToPath(BoardModel.Filename, value);
					}
					break;
				case "scale":
					{
						//set the scale
						Scale = Convert.ToSingle(value);
					}
					break;
				default:
					{
						base.ParseXmlNode(node);
					}
					break;
			}
		}

		/// <summary>
		/// Write this dude out to the xml format
		/// </summary>
		/// <param name="xmlWriter">the xml file to add this dude as a child of</param>
		public override void WriteXmlNodes(XmlTextWriter xmlWriter)
		{
		}

		#endregion //File IO
	}
}
