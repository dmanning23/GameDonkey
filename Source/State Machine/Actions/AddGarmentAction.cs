using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Xml;
using Microsoft.Xna.Framework.Content;
using AnimationLib;
using FilenameBuddy;
using RenderBuddy;

namespace GameDonkey
{
	/// <summary>
	/// This action adds a garment for either a set amount of time, or when the state ends.
	/// </summary>
	public class AddGarmentAction : TimedAction
	{
		#region Members

		/// <summary>
		/// Reference to the garment to add.
		/// These are loaded from the base object's garment manager
		/// </summary>
		Garment m_Garment;

		#endregion //Members

		#region Properties

		public Garment Garment
		{
			get { return m_Garment; }
		}

		#endregion //Properties

		#region Methods

		/// <summary>
		/// Standard constructor
		/// </summary>
		public AddGarmentAction(BaseObject rOwner)
			: base(rOwner)
		{
			ActionType = EActionType.AddGarment;
			m_Garment = null;
		}

		/// <summary>
		/// execute this action (overridden in all child classes)
		/// </summary>
		/// <returns>bool: whether or not to continue running actions after this dude runs</returns>
		public override bool Execute()
		{
			Debug.Assert(null != Owner);
			Debug.Assert(!AlreadyRun);
			Debug.Assert(null != m_Garment);

			//add this actionto the list of garments
			Owner.MyGarments.AddAction(this, Owner.CharacterClock);

			return base.Execute();
		}

		public override bool Compare(IBaseAction rInst)
		{
			Debug.Assert(null != m_Garment);

			AddGarmentAction myAction = (AddGarmentAction)rInst;
			
			Debug.Assert(ActionType == myAction.ActionType);
			Debug.Assert(Time == myAction.Time);
			Debug.Assert(Garment.Name == myAction.Garment.Name);

			return true;
		}

		#endregion //Methods

		#region File IO

		/// <summary>
		/// Read from an xml file
		/// </summary>
		/// <param name="rXMLNode">the xml node to read from</param>
		/// <returns></returns>
		public bool ReadSerialized(XmlNode rXMLNode, IGameDonkey rEngine)
		{
			//read in xml action

			if ("Item" != rXMLNode.Name)
			{
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
					if (ActionType != IBaseAction.XMLTypeToType(strValue))
					{
						return false;
					}
				}
				else
				{
					return false;
				}
			}

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

					if (strName == "type")
					{
						Debug.Assert(strValue == ActionType.ToString());
					}
					else if (strName == "time")
					{
						Time = Convert.ToSingle(strValue);
						if (0.0f > Time)
						{
							Debug.Assert(0.0f <= Time);
							return false;
						}
					}
					else if (strName == "timeDelta")
					{
						TimeDelta = Convert.ToSingle(strValue);
					}
					else if (strName == "garmentFile")
					{
						//make sure we dont already have a garment
						Debug.Assert(null == m_Garment);

						//load the garment from the garment manager
						Filename strGarmentFile = new Filename();
						strGarmentFile.SetRelFilename(strValue);
						if (!LoadGarment(rEngine.Renderer, strGarmentFile))
						{
							return false;
						}
					}
					else
					{
						return false;
					}
				}
			}

			return true;
		}

		public bool LoadGarment(Renderer rRenderer, Filename strFilename)
		{
			m_Garment = Owner.MyGarments.LoadGarment(strFilename, rRenderer);
			return (m_Garment != null);
		}

		/// <summary>
		/// overloaded in child classes to write out action specific stuff
		/// </summary>
		/// <param name="rXMLFile"></param>
		public override void WriteXML(XmlTextWriter rXMLFile)
		{
			rXMLFile.WriteStartElement("timeDelta");
			rXMLFile.WriteString(TimeDelta.ToString());
			rXMLFile.WriteEndElement();

			rXMLFile.WriteStartElement("garmentFile");
			if (null != Garment)
			{
				rXMLFile.WriteString(Garment.Filename.GetRelFilename());
			}
			rXMLFile.WriteEndElement();
		}

		/// <summary>
		/// Read from a serialized file
		/// </summary>
		/// <param name="myAction">the xml item to read the action from</param>
		public bool ReadSerialized(ContentManager myContent, SPFSettings.AddGarmentActionXML myAction, IGameDonkey rEngine)
		{
			Debug.Assert(myAction.type == ActionType.ToString());
			Debug.Assert(null == m_Garment);

			//load the garment from the garment manager
			m_Garment = Owner.MyGarments.LoadGarment(myContent, myAction.garmentFile, rEngine.Renderer);
			if (m_Garment == null)
			{
				return false;
			}

			TimeDelta = myAction.timeDelta;
			ReadSerializedBase(myAction);

			return true;
		}

		#endregion //File IO
	}
}