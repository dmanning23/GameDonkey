using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using Microsoft.Xna.Framework;
using Vector2Extensions;
using StateMachineBuddy;

namespace GameDonkey
{
	public class AddVelocityAction : IBaseAction
	{
		#region Members

		/// <summary>
		/// Whether or not we want this add velocity action to use the left thumbstick to get its direction.
		/// </summary>
		private bool m_bUseObjectDirection;

		/// <summary>
		/// The direction to add to the players velocity to when this action is run.
		/// This is only used if the thumbstick flag is false
		/// </summary>
		private Vector2 m_Velocity;

		/// <summary>
		/// The length of the velocity to add to the character.
		/// This is only used if the thumbstick flag is true
		/// </summary>
		private float m_fVelocityLength;

		#endregion //Members

		#region Properties

		public Vector2 Velocity
		{
			get { return m_Velocity; }
			set
			{
				m_Velocity = value;
				m_fVelocityLength = m_Velocity.Length();
			}
		}

		public bool UseObjectDirection
		{
			get { return m_bUseObjectDirection; }
			set { m_bUseObjectDirection = value; }
		}

		public float VelocityLength
		{
			get { return m_fVelocityLength; }
		}

		#endregion //Properties

		#region Methods

		/// <summary>
		/// Standard constructor
		/// </summary>
		public AddVelocityAction(BaseObject rOwner) : base(rOwner)
		{
			ActionType = EActionType.AddVelocity;
			Velocity = new Vector2(0.0f);
			m_bUseObjectDirection = false;
		}

		/// <summary>
		/// execute this action (overridden in all child classes)
		/// </summary>
		/// <returns>bool: whether or not to continue running actions after this dude runs</returns>
		public override bool Execute()
		{
			Debug.Assert(null != Owner);
			Debug.Assert(!AlreadyRun);

			//the final velocity we will add to the character
			Vector2 myVelocity = Velocity;

			//If we want the thumbstick direction and it is pointing in a direction...
			if (m_bUseObjectDirection && (Owner.Direction() != Vector2.Zero))
			{
				//use the thumbstick direction from the object
				myVelocity = Owner.Direction() * VelocityLength;
			}
			else
			{
				//use the velocity from this action
				myVelocity.X = (Owner.Flip ? -Velocity.X : Velocity.X);
			}

			Owner.Velocity += myVelocity * Owner.Scale;

			return base.Execute();
		}

		public override bool Compare(IBaseAction rInst)
		{
			AddVelocityAction myAction = (AddVelocityAction)rInst;
			
			Debug.Assert(ActionType == myAction.ActionType);
			Debug.Assert(Time == myAction.Time);
			Debug.Assert(m_bUseObjectDirection == myAction.m_bUseObjectDirection);
			Debug.Assert(VelocityLength == myAction.VelocityLength);

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
					else if (strName == "velocity")
					{
						Velocity = strValue.ToVector2();
					}
					else if (strName == "useObjectDirection")
					{
						m_bUseObjectDirection = Convert.ToBoolean(strValue);
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
		public override void WriteXml(XmlTextWriter rXMLFile)
		{
			rXMLFile.WriteStartElement("velocity");
			rXMLFile.WriteString(Velocity.StringFromVector());
			rXMLFile.WriteEndElement();

			rXMLFile.WriteStartElement("useObjectDirection");
			rXMLFile.WriteString(m_bUseObjectDirection ? "true" : "false");
			rXMLFile.WriteEndElement();
		}

		/// <summary>
		/// Read from a serialized file
		/// </summary>
		/// <param name="myAction">the xml item to read the action from</param>
		public bool ReadSerialized(SPFSettings.AddVelocityActionXML myAction)
		{
			Debug.Assert(myAction.type == ActionType.ToString());
			Velocity = myAction.velocity;
			m_bUseObjectDirection = myAction.useObjectDirection;
			ReadSerializedBase(myAction);

			return true;
		}

		#endregion //File IO
	}
}