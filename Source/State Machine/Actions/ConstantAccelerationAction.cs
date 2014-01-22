using Microsoft.Xna.Framework;
using StateMachineBuddy;
using System;
using System.Diagnostics;
using System.Xml;

namespace GameDonkey
{
	public class ConstantAccelerationAction : IBaseAction
	{
		#region Members

		/// <summary>
		/// The pixels/second to add to this characters velocity every second.
		/// </summary>
		public ActionDirection Velocity { get; set; }

		/// <summary>
		/// The point at which to stop adding velocity to the character
		/// </summary>
		public float MaxVelocity { get; set; }

		#endregion //Members

		#region Methods

		/// <summary>
		/// Standard constructor
		/// </summary>
		public ConstantAccelerationAction(BaseObject rOwner) : base(rOwner)
		{
			ActionType = EActionType.ConstantAcceleration;
			Velocity = new ActionDirection();
			MaxVelocity = 0.0f;
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
			Owner.AccelAction = this;

			return base.Execute();
		}

		public override bool Compare(IBaseAction rInst)
		{
			ConstantAccelerationAction myAction = (ConstantAccelerationAction)rInst;
			
			Debug.Assert(ActionType == myAction.ActionType);
			Debug.Assert(Time == myAction.Time);
			Debug.Assert(Velocity.Compare(myAction.Velocity));

			return true;
		}

		public Vector2 GetMyVelocity()
		{
			return Velocity.GetDirection(Owner);
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
					if (ActionType != StateActionFactory.XMLTypeToType(strValue))
					{
						Debug.Assert(false);
						return false;
					}
				}
				else
				{
					Debug.Assert(false);
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
						Velocity.ReadXml(childNode);
					}
					else if (strName == "maxVelocity")
					{
						MaxVelocity = Convert.ToSingle(strValue);
					}
					else
					{
						Debug.Assert(false);
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
			rXMLFile.WriteStartElement("direction");
			Velocity.WriteXml(rXMLFile);
			rXMLFile.WriteEndElement();

			rXMLFile.WriteStartElement("maxVelocity");
			rXMLFile.WriteString(MaxVelocity.ToString());
			rXMLFile.WriteEndElement();
		}

		/// <summary>
		/// Read from a serialized file
		/// </summary>
		/// <param name="myAction">the xml item to read the action from</param>
		public bool ReadSerialized(SPFSettings.ConstantAccelerationActionXML myAction)
		{
			Debug.Assert(myAction.type == ActionType.ToString());
			Velocity.ReadSerialized(myAction.direction);
			MaxVelocity = myAction.maxVelocity;
			ReadSerializedBase(myAction);

			return true;
		}

		#endregion //File IO
	}
}