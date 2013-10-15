using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using FilenameBuddy;

namespace GameDonkey
{
	/// <summary>
	/// This class is used to load character data from xml for the tools
	/// </summary>
	public class CharacterData
	{
		#region Members

		Filename m_strModelFile;
		Filename m_strAnimationFile;
		Filename m_strStateMachineFile;
		Filename m_strStateActionsFile;

		//the sparrow hawk state machine action files, if this is a character data file
		Filename m_strGroundStateMachineFile;
		Filename m_strGroundStateActionsFile;

		Filename m_strUpStateMachineFile;
		Filename m_strUpStateActionsFile;

		Filename m_strDownStateMachineFile;
		Filename m_strDownStateActionsFile;

		Filename m_strForwardStateMachineFile;
		Filename m_strForwardStateActionsFile;

		List<Filename> m_listGarments;

		float m_fHeight;

		#endregion //Members

		#region Properties

		public Filename ModelFile
		{
			get { return m_strModelFile; }
		}

		public Filename AnimationFile
		{
			get { return m_strAnimationFile; }
		}

		public Filename StateMachineFile
		{
			get { return m_strStateMachineFile; }
		}

		public Filename StateActionsFile
		{
			get { return m_strStateActionsFile; }
		}

		public Filename GroundStateMachineFile
		{
			get { return m_strGroundStateMachineFile; }
		}

		public Filename GroundStateActionsFile
		{
			get { return m_strGroundStateActionsFile; }
		}

		public Filename UpStateMachineFile
		{
			get { return m_strUpStateMachineFile; }
		}

		public Filename UpStateActionsFile
		{
			get { return m_strUpStateActionsFile; }
		}

		public Filename DownStateMachineFile
		{
			get { return m_strDownStateMachineFile; }
		}

		public Filename DownStateActionsFile
		{
			get { return m_strDownStateActionsFile; }
		}

		public Filename ForwardStateMachineFile
		{
			get { return m_strForwardStateMachineFile; }
		}

		public Filename ForwardStateActionsFile
		{
			get { return m_strForwardStateActionsFile; }
		}

		public List<Filename> Garments
		{
			get { return m_listGarments; }
		}

		public float Height
		{
			get { return m_fHeight; }
		}

		#endregion //Properties

		#region Methods

		public CharacterData()
		{
			m_strModelFile = new Filename();
			m_strAnimationFile = new Filename();
			m_strStateMachineFile = new Filename();
			m_strStateActionsFile = new Filename();

			m_strGroundStateMachineFile = new Filename();
			m_strGroundStateActionsFile = new Filename();

			m_strUpStateMachineFile = new Filename();
			m_strUpStateActionsFile = new Filename();

			m_strDownStateMachineFile = new Filename();
			m_strDownStateActionsFile = new Filename();

			m_strForwardStateMachineFile = new Filename();
			m_strForwardStateActionsFile = new Filename();

			m_listGarments = new List<Filename>();
		}

		public virtual bool LoadObject(Filename strFileName, IGameDonkey rEngine)
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

			//set the state machine filenames
			m_strGroundStateMachineFile.SetRelFilename(@"wedding state machines\ground state machine.xml");
			m_strUpStateMachineFile.SetRelFilename(@"wedding state machines\up state machine.xml");
			m_strDownStateMachineFile.SetRelFilename(@"wedding state machines\down state machine.xml");
			m_strForwardStateMachineFile.SetRelFilename(@"wedding state machines\forward state machine.xml");

			//Read in child nodes
			if (rXMLNode.HasChildNodes)
			{
				for (XmlNode childNode = rXMLNode.FirstChild;
					null != childNode;
					childNode = childNode.NextSibling)
				{
					//what is in this node?
					string strName = childNode.Name;
					string strValue = childNode.InnerText;

					if (strName == "model")
					{
						m_strModelFile.SetRelFilename(strValue);
					}
					else if (strName == "animations")
					{
						m_strAnimationFile.SetRelFilename(strValue);
					}
					else if (strName == "stateMachine")
					{
						m_strStateMachineFile.SetRelFilename(strValue);
					}
					else if (strName == "states")
					{
						m_strStateActionsFile.SetRelFilename(strValue);
					}
					else if (strName == "height")
					{
						m_fHeight = Convert.ToSingle(strValue);
					}
					else if (strName == "GroundStates")
					{
						m_strGroundStateActionsFile.SetRelFilename(strValue);
					}
						else if (strName == "UpStates")
					{
						m_strUpStateActionsFile.SetRelFilename(strValue);
					}
						else if (strName == "DownStates")
					{
						m_strDownStateActionsFile.SetRelFilename(strValue);
					}
					else if (strName == "ForwardStates")
					{
						m_strForwardStateActionsFile.SetRelFilename(strValue);
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
								m_listGarments.Add(strGarmentFile);
							}
						}
					}
				}
			}

			// Close the file, we done
			stream.Close();
			return true;
		}

		#endregion //Methods
	}
}