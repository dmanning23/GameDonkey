using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;
using System.Diagnostics;
using System.Xml;
using Vector2Extensions;

namespace GameDonkey
{
	public class AddVelocityAction : BaseAction
	{
		#region Members

		/// <summary>
		/// The direction to add to the players velocity to when this action is run.
		/// </summary>
		public ActionDirection Velocity { get; set; }

		#endregion //Members

		#region Methods

		/// <summary>
		/// Standard constructor
		/// </summary>
		public AddVelocityAction(BaseObject rOwner)
			: base(rOwner)
		{
			ActionType = EActionType.AddVelocity;
			Velocity = new ActionDirection();
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
			Vector2 myVelocity = Velocity.GetDirection(Owner);
			Owner.Velocity += myVelocity;

			return base.Execute();
		}

		public override bool Compare(BaseAction rInst)
		{
			AddVelocityAction myAction = (AddVelocityAction)rInst;

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
		public override bool ReadXml(XmlNode rXMLNode, IGameDonkey rEngine, SingleStateContainer stateContainer, ContentManager content)
		{
#if DEBUG
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
#endif

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

					switch (strName)
					{
						case "type":
						{
							Debug.Assert(strValue == ActionType.ToString());
						}
						break;

						case  "time":
						{
							Time = Convert.ToSingle(strValue);
							if (0.0f > Time)
							{
								Debug.Assert(0.0f <= Time);
								return false;
							}
						}
						break;

						case  "direction":
						{
							Velocity.ReadXml(childNode);
						}
						break;

						case  "velocity":
						{
							Velocity.Velocity = strValue.ToVector2();
						}
						break;

						case  "useObjectDirection":
						{
							bool dir = Convert.ToBoolean(strValue);
							Velocity.DirectionType = (dir ? EDirectionType.Controller : EDirectionType.Absolute);
						}
						break;
						default: 
						{
							Debug.Assert(false);
							return false;
						}
					}
				}
			}

			return true;
		}

#if !WINDOWS_UWP
		/// <summary>
		/// overloaded in child classes to write out action specific stuff
		/// </summary>
		/// <param name="rXMLFile"></param>
		protected override void WriteActionXml(XmlTextWriter rXMLFile)
		{
			rXMLFile.WriteStartElement("direction");
			Velocity.WriteXml(rXMLFile);
			rXMLFile.WriteEndElement();
		}
#endif

		#endregion //File IO
	}
}