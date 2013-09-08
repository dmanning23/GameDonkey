using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml;
using Microsoft.Xna.Framework;

namespace GameDonkey
{
	public class ConstantAccelerationAction : IBaseAction
	{
		#region Members

		/// <summary>
		/// The pixels/second to add to this characters velocity every second.
		/// should be +x and -y
		/// </summary>
		private Vector2 m_Velocity;

		/// <summary>
		/// The point at which to stop adding velocity to the character
		/// should be +x and -y
		/// </summary>
		private Vector2 m_MaxVelocity;

		/// <summary>
		/// Whether or not we want this add velocity action to use the left thumbstick to get its direction.
		/// </summary>
		private bool m_bUseObjectDirection;

		/// <summary>
		/// The length of the velocity to add to the character.
		/// This is only used if the thumbstick flag is true
		/// </summary>
		private float m_fVelocityLength;

		#endregion //Members

		#region Properties

		/// <summary>
		/// The pixels/second to add to this characters velocity every second.
		/// should be -y and +x
		/// </summary>
		public Vector2 Velocity
		{
			get { return m_Velocity; }
			set { m_Velocity = value; }
		}

		/// <summary>
		/// The point at which to stop adding velocity to the character
		/// should be -y and +x
		/// </summary>
		public Vector2 MaxVelocity
		{
			get { return m_MaxVelocity; }
			set { m_MaxVelocity = value; }
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
		public ConstantAccelerationAction(BaseObject rOwner) : base(rOwner)
		{
			ActionType = EActionType.ConstantAcceleration;
			m_Velocity = new Vector2(0.0f);
			m_MaxVelocity = new Vector2(0.0f);
			m_bUseObjectDirection = false;
			m_fVelocityLength = 0.0f;
		}

		/// <summary>
		/// execute this action (overridden in all child classes)
		/// </summary>
		/// <returns>bool: whether or not to continue running actions after this dude runs</returns>
		public override bool Execute()
		{
			Debug.Assert(null != Owner);
			Debug.Assert(!AlreadyRun);

			//set the constant accleration variable in the base object
			Owner.AccelerationAction = this;

			return base.Execute();
		}

		public override bool Compare(IBaseAction rInst)
		{
			ConstantAccelerationAction myAction = (ConstantAccelerationAction)rInst;
			
			Debug.Assert(ActionType == myAction.ActionType);
			Debug.Assert(Time == myAction.Time);
			Debug.Assert(m_bUseObjectDirection == myAction.m_bUseObjectDirection);
			Debug.Assert(m_fVelocityLength == myAction.m_fVelocityLength);

			return true;
		}

		public Vector2 GetMyVelocity()
		{
			//the final velocity we will add to the character
			Vector2 myVelocity = Velocity;

			if (m_bUseObjectDirection && (Owner.Direction() != Vector2.Zero))
			{
				//use the thumbstick direction from the object
				myVelocity = Owner.Direction() * m_fVelocityLength;
			}
			else
			{
				//use the velocity from this action
				myVelocity.X = (Owner.Flip ? -m_Velocity.X : m_Velocity.X);
			}

			return myVelocity;
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
					else if (strName == "velocity")
					{
						m_Velocity = CStringUtils.ReadVectorFromString(strValue);
					}
					else if (strName == "maxVelocity")
					{
						m_MaxVelocity = CStringUtils.ReadVectorFromString(strValue);
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

			m_fVelocityLength = m_Velocity.Length();

			Debug.Assert(m_Velocity.Y <= 0.0f);
			Debug.Assert(m_Velocity.X >= 0.0f);
			Debug.Assert(m_MaxVelocity.Y <= 0.0f);
			Debug.Assert(m_MaxVelocity.X >= 0.0f);

			return true;
		}

		/// <summary>
		/// overloaded in child classes to write out action specific stuff
		/// </summary>
		/// <param name="rXMLFile"></param>
		public override void WriteXML(XmlTextWriter rXMLFile)
		{
			rXMLFile.WriteStartElement("velocity");
			rXMLFile.WriteString(CStringUtils.StringFromVector(m_Velocity));
			rXMLFile.WriteEndElement();

			rXMLFile.WriteStartElement("maxVelocity");
			rXMLFile.WriteString(CStringUtils.StringFromVector(m_MaxVelocity));
			rXMLFile.WriteEndElement();

			rXMLFile.WriteStartElement("useObjectDirection");
			rXMLFile.WriteString(m_bUseObjectDirection ? "true" : "false");
			rXMLFile.WriteEndElement();
		}

		/// <summary>
		/// Read from a serialized file
		/// </summary>
		/// <param name="myAction">the xml item to read the action from</param>
		public bool ReadSerialized(SPFSettings.ConstantAccelerationActionXML myAction)
		{
			Debug.Assert(myAction.type == ActionType.ToString());
			m_Velocity = myAction.velocity;
			m_MaxVelocity = myAction.maxVelocity;
			m_bUseObjectDirection = myAction.useObjectDirection;
			m_fVelocityLength = m_Velocity.Length();
			ReadSerializedBase(myAction);

			Debug.Assert(m_Velocity.Y <= 0.0f);
			Debug.Assert(m_Velocity.X >= 0.0f);
			Debug.Assert(m_MaxVelocity.Y <= 0.0f);
			Debug.Assert(m_MaxVelocity.X >= 0.0f);

			return true;
		}

		#endregion //File IO
	}
}