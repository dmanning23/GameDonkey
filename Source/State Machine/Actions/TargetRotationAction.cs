using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;
using System.Xml;
using Vector2Extensions;

namespace GameDonkey
{
	public class TargetRotationAction : TimedAction
	{
		#region Members

		/// <summary>
		/// The rotation this action will aim for
		/// </summary>
		public ActionDirection TargetRotation { get; set; }

		#endregion //Members

		#region Methods

		/// <summary>
		/// Standard constructor
		/// </summary>
		public TargetRotationAction(BaseObject rOwner)
			: base(rOwner)
		{
			ActionType = EActionType.TargetRotation;
			TargetRotation = new ActionDirection();
		}

		/// <summary>
		/// execute this action (overridden in all child classes)
		/// </summary>
		/// <returns>bool: whether or not to continue running actions after this dude runs</returns>
		public override bool Execute()
		{
			Debug.Assert(null != Owner);
			Debug.Assert(!AlreadyRun);

			//get the direction
			Vector2 direction = TargetRotation.GetDirection(Owner);
			direction.X = (Owner.Flip ? (direction.X * -1.0f) : direction.X);

			//Convert the direction to a rotation
			float fAngle = Helper.ClampAngle(direction.Angle());

			//get the amount of rotation/second to add
			float rotationDelta = 0.0f;
			if (Owner.Flip)
			{
				rotationDelta = fAngle + Owner.CurrentRotation;
			}
			else
			{
				rotationDelta = fAngle - Owner.CurrentRotation;
			}

			//change to rotation / second
			rotationDelta /= TimeDelta;

			//set the rotation action variable in the base object
			Owner.RotationPerSecond = rotationDelta;

			return base.Execute();
		}

		public override bool Compare(IBaseAction rInst)
		{
			var myAction = (TargetRotationAction)rInst;
			
			Debug.Assert(ActionType == myAction.ActionType);
			Debug.Assert(Time == myAction.Time);
			Debug.Assert(TargetRotation.Compare(myAction.TargetRotation));

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
						case "timeDelta":
						{
							TimeDelta = Convert.ToSingle(strValue);
						}
						break;
						case "targetRotation":
						{
							TargetRotation.ReadXml(childNode);
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

		/// <summary>
		/// overloaded in child classes to write out action specific stuff
		/// </summary>
		/// <param name="rXMLFile"></param>
		protected override void WriteActionXml(XmlTextWriter rXMLFile)
		{
			rXMLFile.WriteStartElement("timeDelta");
			rXMLFile.WriteString(TimeDelta.ToString());
			rXMLFile.WriteEndElement();

			rXMLFile.WriteStartElement("targetRotation");
			TargetRotation.WriteXml(rXMLFile);
			rXMLFile.WriteEndElement();
		}

		#endregion //File IO
	}
}