using System;
using System.Diagnostics;
using System.Xml;

namespace GameDonkey
{
	public class DeactivateAction : IBaseAction
	{
		#region Members

		#endregion //Members

		#region Properties

		#endregion //Properties

		#region Methods

		/// <summary>
		/// Standard constructor
		/// </summary>
		public DeactivateAction(BaseObject rOwner) : base(rOwner)
		{
			ActionType = EActionType.Deactivate;
		}

		/// <summary>
		/// execute this action (overridden in all child classes)
		/// </summary>
		/// <returns>bool: whether or not to continue running actions after this dude runs</returns>
		public override bool Execute()
		{
			Debug.Assert(null != Owner);
			Debug.Assert(null != Owner.PlayerQueue);
			Debug.Assert(!AlreadyRun);

			//get that dude's characterqeueu and deactiuvate it
			Owner.PlayerQueue.DeactivateObject(Owner);

			//goddamit, make sure that worked!!!
			Debug.Assert(Owner.PlayerQueue.CheckListForObject(Owner, false));
			Debug.Assert(!Owner.PlayerQueue.CheckListForObject(Owner, true));

			//never set these actions to already run
			AlreadyRun = false;
			return true;
		}

		public override bool Compare(IBaseAction rInst)
		{
			DeactivateAction myAction = (DeactivateAction)rInst;
			
			Debug.Assert((ActionType == myAction.ActionType) &&
				(Time == myAction.Time));
			
			return ((ActionType == myAction.ActionType) &&
				(Time == myAction.Time));
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
		}

		/// <summary>
		/// Read from a serialized file
		/// </summary>
		/// <param name="myAction">the xml item to read the action from</param>
		public bool ReadSerialized(SPFSettings.DeactivateActionXML myAction)
		{
			Debug.Assert(myAction.type == ActionType.ToString());
			ReadSerializedBase(myAction);

			return true;
		}

		#endregion //File IO
	}
}