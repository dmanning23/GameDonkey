using Microsoft.Xna.Framework;
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
		public Vector2 Velocity { get; set; }

		/// <summary>
		/// The point at which to stop adding y velocity to the character
		/// should be +y.  Don't care about x, that will always slow down to 0.0
		/// </summary>
		public float MinYVelocity { get; set; }

		#endregion //Members

		#region Methods

		/// <summary>
		/// Standard constructor
		/// </summary>
		public ConstantDeccelerationAction(BaseObject rOwner) : base(rOwner)
		{
			ActionType = EActionType.ConstantDecceleration;
			Velocity = Vector2.Zero;
			MinYVelocity = 0.0f;
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
			Owner.DeccelAction = this;

			return base.Execute();
		}

		public override bool Compare(IBaseAction rInst)
		{
			ConstantDeccelerationAction myAction = (ConstantDeccelerationAction)rInst;

			Debug.Assert(ActionType == myAction.ActionType);
			Debug.Assert(Time == myAction.Time);
			Debug.Assert(Velocity == myAction.Velocity);

			return true;
		}

		#endregion //Methods

		#region File IO

		/// <summary>
		/// Read from an xml file
		/// </summary>
		/// <param name="rXMLNode">the xml node to read from</param>
		/// <returns></returns>
		public override bool ReadXml(XmlNode rXMLNode, IGameDonkey rEngine, SingleStateContainer stateContainer)
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
						case "time":
						{
							Time = Convert.ToSingle(strValue);
							if (0.0f > Time)
							{
								Debug.Assert(0.0f <= Time);
								return false;
							}
						}
						break;
						case "direction":
						{
							Velocity = strValue.ToVector2();
						}
						break;
						case "minYVelocity":
						{
							MinYVelocity = Convert.ToSingle(strValue);
						}
						break;
						case "velocity":
						{
							Velocity = strValue.ToVector2();
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

			Debug.Assert(Velocity.X <= 0.0f);
			Debug.Assert(Velocity.Y >= 0.0f);
			Debug.Assert(MinYVelocity >= 0.0f);

			return true;
		}

		/// <summary>
		/// overloaded in child classes to write out action specific stuff
		/// </summary>
		/// <param name="rXMLFile"></param>
		protected override void WriteActionXml(XmlTextWriter rXMLFile)
		{
			rXMLFile.WriteStartElement("direction");
			rXMLFile.WriteString(Velocity.StringFromVector());
			rXMLFile.WriteEndElement();

			rXMLFile.WriteStartElement("minYVelocity");
			rXMLFile.WriteString(MinYVelocity.ToString());
			rXMLFile.WriteEndElement();
		}

		#endregion //File IO
	}
}