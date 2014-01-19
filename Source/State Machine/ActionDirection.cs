using Microsoft.Xna.Framework;
using SPFSettings;
using System;
using System.Diagnostics;
using System.Xml;
using Vector2Extensions;

namespace GameDonkey
{
	/// <summary>
	/// This is a class to wrap up getting a direction for directional actions.
	/// </summary>
	public class ActionDirection
	{
		#region Members

		/// <summary>
		/// The direction to use
		/// </summary>
		public Vector2 m_Velocity;

		/// <summary>
		/// Whether or not we want this action to use the left thumbstick to get its direction.
		/// </summary>
		public EDirectionType DirectionType { get; set; }

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

		public float VelocityLength
		{
			get { return m_fVelocityLength; }
		}

		#endregion //Properties

		#region Methods

		/// <summary>
		/// Standard constructor
		/// </summary>
		public ActionDirection()
		{
			Velocity = new Vector2(0.0f);
			DirectionType = EDirectionType.SetDirection;
		}

		/// <summary>
		/// execute this action (overridden in all child classes)
		/// </summary>
		/// <returns>bool: whether or not to continue running actions after this dude runs</returns>
		public Vector2 GetDirection(BaseObject rOwner)
		{
			Debug.Assert(null != rOwner);

			//the final velocity we will add to the character
			Vector2 myVelocity = Velocity;

			if ((EDirectionType.Controller == DirectionType) && (rOwner.Direction() != Vector2.Zero))
			{
				//use the thumbstick direction from the object
				myVelocity = rOwner.Direction() * VelocityLength;
			}
			else if ((EDirectionType.NegController == DirectionType) && (rOwner.Direction() != Vector2.Zero))
			{
				//use the thumbstick direction from the object
				myVelocity = rOwner.Direction() * -VelocityLength;
			}
			else
			{
				//use the velocity from this action
				myVelocity.X = (rOwner.Flip ? -Velocity.X : Velocity.X);
			}

			return myVelocity * rOwner.Scale;
		}

		public bool Compare(ActionDirection rInst)
		{
			Debug.Assert(Velocity == rInst.Velocity);
			Debug.Assert(DirectionType == rInst.DirectionType);
			Debug.Assert(VelocityLength == rInst.VelocityLength);

			return true;
		}

		#endregion //Methods

		#region File IO

		/// <summary>
		/// Read from an xml file
		/// </summary>
		/// <param name="rXMLNode">the xml node to read from</param>
		/// <returns></returns>
		public bool ReadXml(XmlNode rXMLNode)
		{
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

					if (strName == "velocity")
					{
						Velocity = strValue.ToVector2();
					}
					else if (strName == "directionType")
					{
						DirectionType = (EDirectionType)Enum.Parse(typeof(EDirectionType), strValue);
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
		public void WriteXml(XmlTextWriter rXMLFile)
		{
			rXMLFile.WriteStartElement("velocity");
			rXMLFile.WriteString(Velocity.StringFromVector());
			rXMLFile.WriteEndElement();

			rXMLFile.WriteStartElement("directionType");
			rXMLFile.WriteString(DirectionType.ToString());
			rXMLFile.WriteEndElement();
		}

		/// <summary>
		/// Read from a serialized file
		/// </summary>
		/// <param name="myAction">the xml item to read the action from</param>
		public bool ReadSerialized(DirectionActionXML myAction)
		{
			Velocity = myAction.velocity;
			DirectionType = (EDirectionType)Enum.Parse(typeof(EDirectionType), myAction.directionType);
			return true;
		}

		#endregion //File IO
	}
}