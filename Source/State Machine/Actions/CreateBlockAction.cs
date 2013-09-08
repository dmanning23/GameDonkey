using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;

namespace GameDonkey
{
	/// <summary>
	/// This action makes a character temporarily invulnerable
	/// </summary>
	public class CCreateBlockAction : IBaseAction
	{
		#region Members

		/// <summary>
		/// the length of time the block is active
		/// </summary>
		protected float m_fTimeDelta;

		#endregion //Members

		#region Properties

		public float TimeDelta
		{
			get { return m_fTimeDelta; }
			set { m_fTimeDelta = value; }
		}

		#endregion //Properties

		#region Methods

		/// <summary>
		/// Standard constructor
		/// </summary>
		public CCreateBlockAction(BaseObject rOwner)
			: base(rOwner)
		{
			ActionType = EActionType.CreateBlock;
			m_fTimeDelta = 0.0f;
		}

		/// <summary>
		/// execute this action (overridden in all child classes)
		/// </summary>
		/// <returns>bool: whether or not to continue running actions after this dude runs</returns>
		public override bool Execute()
		{
			Debug.Assert(null != Owner);
			Debug.Assert(!AlreadyRun);

			Owner.AddBlock(this);

			return base.Execute();
		}

		public override bool Compare(IBaseAction rInst)
		{
			CCreateBlockAction myAction = (CCreateBlockAction)rInst;
			return ((ActionType == myAction.ActionType) &&
				(Time == myAction.Time) &&
				(m_fTimeDelta == myAction.m_fTimeDelta));
		}

		#endregion //Methods

		#region File IO

#if WINDOWS

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
						m_fTimeDelta = Convert.ToSingle(strValue);
						if (0.0 > TimeDelta)
						{
							Debug.Assert(0.0f <= TimeDelta);
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

		/// <summary>
		/// overloaded in child classes to write out action specific stuff
		/// </summary>
		/// <param name="rXMLFile"></param>
		public override void WriteXML(XmlTextWriter rXMLFile)
		{
			rXMLFile.WriteStartElement("timeDelta");
			rXMLFile.WriteString(m_fTimeDelta.ToString());
			rXMLFile.WriteEndElement();
		}

#endif

		/// <summary>
		/// Read from a serialized file
		/// </summary>
		/// <param name="myAction">the xml item to read the action from</param>
		public bool ReadSerialized(SPFSettings.CreateBlockActionXML myAction)
		{
			Debug.Assert(myAction.type == ActionType.ToString());
			ReadSerializedBase(myAction);

			m_fTimeDelta = myAction.timeDelta;

			return true;
		}


		#endregion //File IO
	}
}