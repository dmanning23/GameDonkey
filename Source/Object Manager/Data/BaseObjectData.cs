using FilenameBuddy;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;
using SPFSettings;

namespace GameDonkey
{
	/// <summary>
	/// This class is used to load base object data from xml for the tools
	/// </summary>
	public class BaseObjectData
	{
		#region Members

		//Base object data

		public Filename ModelFile { get; private set; }
		public Filename AnimationFile { get; private set; }
		public List<Filename> GarmentFiles { get; private set; }
		public List<StateContainerXML> StateContainers { get; private set; }
		public float Height { get; private set; }

		#endregion //Members

		#region Methods

		public BaseObjectData()
		{
			ModelFile = new Filename();
			AnimationFile = new Filename();
			StateContainers = new List<StateContainerXML>();
			GarmentFiles = new List<Filename>();
			Height = 0.0f;
		}

		public virtual bool LoadObject(Filename strFileName)
		{
			//gonna have to do this the HARD way

			//Open the file.
			FileStream stream = File.Open(strFileName.File, FileMode.Open, FileAccess.Read);
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.Load(stream);
			XmlNode rootNode = xmlDoc.DocumentElement;

			//make sure it is actually an xml node
			if (rootNode.NodeType != XmlNodeType.Element)
			{
				Debug.Assert(false);
				return false;
			}

			//eat up the name of that xml node
			string strElementName = rootNode.Name;
			if (("XnaContent" != strElementName) || !rootNode.HasChildNodes)
			{
				Debug.Assert(false);
				return false;
			}

			//get the next node
			XmlNode rXMLNode = rootNode.FirstChild;
			if ("Asset" != rXMLNode.Name)
			{
				Debug.Assert(false);
				return false;
			}

			//should have an attribute Type
			XmlNamedNodeMap mapAttributes = rXMLNode.Attributes;
			for (int i = 0; i < mapAttributes.Count; i++)
			{
				//will only have the name attribute
				string strName = mapAttributes.Item(i).Name;
				string strValue = mapAttributes.Item(i).Value;
				if ("Type" == strName)
				{
					if (("SPFSettings.PlayerObjectXML" != strValue) &&
						("SPFSettings.BaseObjectXML" != strValue) && 
						("SPFSettings.LevelObjectXML" != strValue))
					{
						Debug.Assert(false);
						return false;
					}
				}
			}

			//Read in child nodes
			if (rXMLNode.HasChildNodes)
			{
				for (XmlNode childNode = rXMLNode.FirstChild;
					null != childNode;
					childNode = childNode.NextSibling)
				{
					if (!ParseXmlNode(childNode))
					{
						return false;
					}
				}
			}

			// Close the file, we done
			stream.Close();
			return true;
		}

		protected virtual bool ParseXmlNode(XmlNode childNode)
		{
			//what is in this node?
			string strName = childNode.Name;
			string strValue = childNode.InnerText;

			if (strName == "model")
			{
				ModelFile.SetRelFilename(strValue);
			}
			else if (strName == "animations")
			{
				AnimationFile.SetRelFilename(strValue);
			}
			else if (strName == "garments")
			{
				if (childNode.HasChildNodes)
				{
					for (XmlNode garmentNode = childNode.FirstChild;
						null != garmentNode;
						garmentNode = garmentNode.NextSibling)
					{
						Filename strGarmentFile = new Filename(garmentNode.InnerText);
						GarmentFiles.Add(strGarmentFile);
					}
				}
			}
			else if (strName == "height")
			{
				Height = Convert.ToSingle(strValue);
			}
			else if (strName == "states")
			{
				if (!ReadStates(childNode))
				{
					Debug.Assert(false);
					return false;
				}
			}

			return true;
		}

		private bool ReadStates(XmlNode rXmlNode)
		{
			//get the child node
			if (rXmlNode.HasChildNodes)
			{
				for (XmlNode stateNode = rXmlNode.FirstChild;
					null != stateNode;
					stateNode = stateNode.NextSibling)
				{
					//should be a list of junk
					if ("Item" != stateNode.Name)
					{
						Debug.Assert(false);
						return false;
					}

					//should have an attribute Type
					XmlNamedNodeMap mapAttributes = stateNode.Attributes;
					for (int i = 0; i < mapAttributes.Count; i++)
					{
						//will only have the name attribute
						if ("Type" == mapAttributes.Item(i).Name)
						{
							if ("SPFSettings.StateContainerXML" != mapAttributes.Item(i).Value)
							{
								Debug.Assert(false);
								return false;
							}
						}
					}

					//Read in child nodes
					if (!stateNode.HasChildNodes)
					{
						Debug.Assert(false);
						return false;
					}

					//get the state machine xml node
					XmlNode childNode = stateNode.FirstChild;
					string strName = childNode.Name;
					string strValue = childNode.InnerXml;
					if (strName != "stateMachine")
					{
						Debug.Assert(false);
						return false;
					}
					string stateMachineFile = strValue;

					//get the state action xml node
					childNode = childNode.NextSibling;
					strName = childNode.Name;
					strValue = childNode.InnerXml;
					if (strName != "stateActions")
					{
						Debug.Assert(false);
						return false;
					}
					string stateActionFile = strValue;

					//create the object to store that thing
					StateContainerXML containerXML = new StateContainerXML()
					{
						stateMachine = stateMachineFile,
						stateActions = stateActionFile
					};
					StateContainers.Add(containerXML);
				}
			}

			return true;
		}

		#endregion //Methods
	}
}