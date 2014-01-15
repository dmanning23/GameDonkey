using Microsoft.Xna.Framework;
using StateMachineBuddy;
using System;
using System.Diagnostics;
using System.Xml;
using Vector2Extensions;

namespace GameDonkey
{
	public class ConstantDeccelerationAction : IBaseAction
	{
		#region Members

		/// <summary>
		/// The pixels/second to add to this characters velocity every second.
		/// should be -x and +y
		/// </summary>
		public ActionDirection Velocity { get; set; }

		/// <summary>
		/// The point at which to stop adding y velocity to the character
		/// should be +y.  Don't care about x, that will always slow down to 0.0
		/// </summary>
		private float m_fMinYVelocity;

		#endregion //Members

		#region Properties

		/// <summary>
		/// The point at which to stop adding y velocity to the character
		/// should be +y
		/// </summary>
		public float MinYVelocity
		{
			get { return m_fMinYVelocity; }
			set { m_fMinYVelocity = value; }
		}

		#endregion //Properties

		#region Methods

		/// <summary>
		/// Standard constructor
		/// </summary>
		public ConstantDeccelerationAction(BaseObject rOwner) : base(rOwner)
		{
			ActionType = EActionType.ConstantDecceleration;
			Velocity = new ActionDirection();
			m_fMinYVelocity = 0.0f;
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
			Owner.DeccelerationAction = this;

			return base.Execute();
		}

		public Vector2 GetMyVelocity()
		{
			return Velocity.GetDirection(Owner);
		}

		public override bool Compare(IBaseAction rInst)
		{
			ConstantDeccelerationAction myAction = (ConstantDeccelerationAction)rInst;

			Debug.Assert(ActionType == myAction.ActionType);
			Debug.Assert(Time == myAction.Time);
			Debug.Assert(Velocity.Compare(myAction.Velocity));

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
					else if (strName == "direction")
					{
						Velocity.ReadXml(childNode.FirstChild);
					}
					else if (strName == "minYVelocity")
					{
						m_fMinYVelocity = Convert.ToSingle(strValue);
					}
					else
					{
						return false;
					}
				}
			}

			Debug.Assert(Velocity.Velocity.X <= 0.0f);
			Debug.Assert(Velocity.Velocity.Y >= 0.0f);
			Debug.Assert(m_fMinYVelocity >= 0.0f);

			return true;
		}

		/// <summary>
		/// overloaded in child classes to write out action specific stuff
		/// </summary>
		/// <param name="rXMLFile"></param>
		protected override void WriteActionXml(XmlTextWriter rXMLFile)
		{
			rXMLFile.WriteStartElement("direction");
			Velocity.WriteXml(rXMLFile);
			rXMLFile.WriteEndElement();

			rXMLFile.WriteStartElement("minYVelocity");
			rXMLFile.WriteString(m_fMinYVelocity.ToString());
			rXMLFile.WriteEndElement();
		}

		/// <summary>
		/// Read from a serialized file
		/// </summary>
		/// <param name="myAction">the xml item to read the action from</param>
		public bool ReadSerialized(SPFSettings.ConstantDeccelerationActionXML myAction)
		{
			Debug.Assert(myAction.type == ActionType.ToString());
			Velocity.ReadSerialized(myAction.direction);
			m_fMinYVelocity = myAction.minYVelocity;
			ReadSerializedBase(myAction);

			Debug.Assert(Velocity.Velocity.Y <= 0.0f);
			Debug.Assert(Velocity.Velocity.X >= 0.0f);
			Debug.Assert(m_fMinYVelocity >= 0.0f);

			return true;
		}

		#endregion //File IO
	}
}