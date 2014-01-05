using Microsoft.Xna.Framework;
using StateMachineBuddy;
using System;
using System.Diagnostics;
using System.Xml;

namespace GameDonkey
{
	public class TrailAction : IBaseAction
	{
		#region Members

		/// <summary>
		/// the start color of this trail
		/// </summary>
		private Color m_StartColor;

		/// <summary>
		/// how long each individual trail lasts
		/// </summary>
		private float m_fTrailLifeDelta;

		/// <summary>
		/// how often to spawn a new trail
		/// </summary>
		private float m_fSpawnDelta;

		/// <summary>
		/// the time delta of how long this trail action is active
		/// </summary>
		private float m_fTimeDelta;

		/// <summary>
		/// The time when this trail is done. Set at runtime when the attack is activated
		/// </summary>
		private float m_fDoneTime;

		#endregion //Members

		#region Properties

		public Color StartColor
		{
			get { return m_StartColor; }
		}

		public float DoneTime
		{
			get { return m_fDoneTime; }
		}

		public float SpawnDelta
		{
			get { return m_fSpawnDelta; }
		}

		public float TrailLifeDelta
		{
			get { return m_fTrailLifeDelta; }
		}

		#endregion //Properties

		#region Methods

		/// <summary>
		/// Standard constructor
		/// </summary>
		public TrailAction(BaseObject rOwner) : base(rOwner)
		{
			ActionType = EActionType.Trail;
			m_StartColor = new Color(1.0f, 1.0f, 1.0f);
			m_fTrailLifeDelta = 0.0f;
			m_fSpawnDelta = 0.0f;
			m_fTimeDelta = 0.0f;
			m_fDoneTime = 0.0f;
		}

		/// <summary>
		/// execute this action (overridden in all child classes)
		/// </summary>
		/// <returns>bool: whether or not to continue running actions after this dude runs</returns>
		public override bool Execute()
		{
			Debug.Assert(null != Owner);
			Debug.Assert(null != Owner.CharacterClock);
			Debug.Assert(!AlreadyRun);

			//activate the trail
			m_fDoneTime = Owner.CharacterClock.CurrentTime + m_fTimeDelta;

			//set the base objects character trail to this dude
			Owner.TrailAction = this;

			//start the base objects trail timer
			Owner.TrailTimer.Start(m_fSpawnDelta);

			return base.Execute();
		}

		public override bool Compare(IBaseAction rInst)
		{
			TrailAction myAction = (TrailAction)rInst;

			Debug.Assert(ActionType == myAction.ActionType);
			Debug.Assert(Time == myAction.Time);
			Debug.Assert(m_StartColor == myAction.m_StartColor);
			Debug.Assert(m_fTrailLifeDelta == myAction.m_fTrailLifeDelta);
			Debug.Assert(m_fSpawnDelta == myAction.m_fSpawnDelta);
			Debug.Assert(m_fTimeDelta == myAction.m_fTimeDelta);
			Debug.Assert(m_fDoneTime == myAction.m_fDoneTime);

			return true;
		}

		#endregion //Methods

		#region File IO

		/// <summary>
		/// Read from an xml file
		/// </summary>
		/// <param name="rXMLNode">the xml node to read from</param>
		/// <returns></returns>
		public override bool ReadXml(XmlNode rXMLNode, IGameDonkey rEngine, StateMachine rStateMachine)
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
					if (ActionType != StateActionFactory.XMLTypeToType(strValue))
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
					else if (strName == "R")
					{
						m_StartColor.R = Convert.ToByte(strValue);
					}
					else if (strName == "G")
					{
						m_StartColor.G = Convert.ToByte(strValue);
					}
					else if (strName == "B")
					{
						m_StartColor.B = Convert.ToByte(strValue);
					}
					else if (strName == "A")
					{
						m_StartColor.A = Convert.ToByte(strValue);
					}
					else if (strName == "lifeDelta")
					{
						m_fTrailLifeDelta = Convert.ToSingle(strValue);
					}
					else if (strName == "spawnDelta")
					{
						m_fSpawnDelta = Convert.ToSingle(strValue);
					}
					else if (strName == "timeDelta")
					{
						m_fTimeDelta = Convert.ToSingle(strValue);
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
		protected override void WriteActionXml(XmlTextWriter rXMLFile)
		{
			rXMLFile.WriteStartElement("R");
			rXMLFile.WriteString(m_StartColor.R.ToString());
			rXMLFile.WriteEndElement();

			rXMLFile.WriteStartElement("G");
			rXMLFile.WriteString(m_StartColor.G.ToString());
			rXMLFile.WriteEndElement();

			rXMLFile.WriteStartElement("B");
			rXMLFile.WriteString(m_StartColor.B.ToString());
			rXMLFile.WriteEndElement();

			rXMLFile.WriteStartElement("A");
			rXMLFile.WriteString(m_StartColor.A.ToString());
			rXMLFile.WriteEndElement();

			rXMLFile.WriteStartElement("lifeDelta");
			rXMLFile.WriteString(m_fTrailLifeDelta.ToString());
			rXMLFile.WriteEndElement();

			rXMLFile.WriteStartElement("spawnDelta");
			rXMLFile.WriteString(m_fSpawnDelta.ToString());
			rXMLFile.WriteEndElement();

			rXMLFile.WriteStartElement("timeDelta");
			rXMLFile.WriteString(m_fTimeDelta.ToString());
			rXMLFile.WriteEndElement();
		}

		/// <summary>
		/// Read from a serialized file
		/// </summary>
		/// <param name="myAction">the xml item to read the action from</param>
		public bool ReadSerialized(SPFSettings.TrailActionXML myAction)
		{
			Debug.Assert(myAction.type == ActionType.ToString());
			ReadSerializedBase(myAction);

			m_StartColor.R = myAction.R;
			m_StartColor.G = myAction.G;
			m_StartColor.B = myAction.B;
			m_StartColor.A = myAction.A;
			m_fTrailLifeDelta = myAction.lifeDelta;
			m_fSpawnDelta = myAction.spawnDelta;
			m_fTimeDelta = myAction.timeDelta;

			Debug.Assert(m_fTrailLifeDelta >= 0.0f);
			Debug.Assert(m_fSpawnDelta >= 0.0f);
			Debug.Assert(m_fTimeDelta >= 0.0f);
			Debug.Assert(m_StartColor.A > 0);

			return true;
		}

		#endregion //File IO
	}
}