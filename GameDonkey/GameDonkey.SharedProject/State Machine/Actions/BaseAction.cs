using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;

namespace GameDonkeyLib
{
	/// <summary>
	/// The base interface for state machine actions
	/// </summary>
	public abstract class BaseAction
	{
		#region Properties

		/// <summary>
		/// the type of this action
		/// </summary>
		public EActionType ActionType { get; protected set; }

		/// <summary>
		/// The game object that owns this action
		/// </summary>
		public BaseObject Owner { get; set; }

		/// <summary>
		/// whether or not this action has been run 
		/// </summary>
		public bool AlreadyRun { get; set; }

		/// <summary>
		/// the time from the start of the state that this action ocuurs
		/// </summary>
		public float Time { get; set; }

		#endregion //Properties

		#region Methods

		/// <summary>
		/// default constructor
		/// </summary>
		public BaseAction(BaseObject rOwner)
		{
			ActionType = EActionType.NumTypes;
			Time = 0.0f;
			AlreadyRun = true;
			Owner = rOwner;
		}

		/// <summary>
		/// execute this action (overridden in all child classes)
		/// </summary>
		/// <returns>bool: whether or not to continue running actions after this dude runs</returns>
		public virtual bool Execute()
		{
			AlreadyRun = true;
			return false;
		}

		public override string ToString()
		{
			return Time.ToString() + ": " + StateActionFactory.TypeToXMLString(ActionType);
		}

		public abstract bool Compare(BaseAction rInst);

		#endregion //Methods

		#region File IO

		/// <summary>
		/// Read from an xml file
		/// </summary>
		/// <param name="rXMLNode">the xml node to read from</param>
		/// <returns></returns>
		public abstract bool ReadXml(XmlNode rXMLNode, IGameDonkey rEngine, SingleStateContainer stateContainer, ContentManager content);

		static public bool ReadXmlListActions(BaseObject rOwner,
			ref List<BaseAction> outputList,
			XmlNode rParentNode,
			IGameDonkey rEngine,
			SingleStateContainer stateContainer, 
			ContentManager content)
		{
#if DEBUG
			//set up all the actions
			if (!rParentNode.HasChildNodes)
			{
				return true;
			}
#endif

			for (XmlNode childNode = rParentNode.FirstChild;
				null != childNode;
				childNode = childNode.NextSibling)
			{
#if DEBUG
				if ("Item" != childNode.Name)
				{
					return false;
				}
#endif

				//should have an attribute Type
				EActionType eChildType = EActionType.NumTypes;
				XmlNamedNodeMap mapAttributes = childNode.Attributes;
				for (int i = 0; i < mapAttributes.Count; i++)
				{
					//will only have the name attribute
					string strName = mapAttributes.Item(i).Name;
					string strValue = mapAttributes.Item(i).Value;
					if ("Type" == strName)
					{
						eChildType = StateActionFactory.XMLTypeToType(strValue);
					}
					else
					{
						Debug.Assert(false);
						return false;
					}
				}

				BaseAction myAction = StateActionFactory.CreateStateAction(eChildType, rOwner, rEngine);
				if (!myAction.ReadXml(childNode, rEngine, stateContainer, content))
				{
					Debug.Assert(false);
					return false;
				}
				outputList.Add(myAction);
			}

			return true;
		}

#if !WINDOWS_UWP
		/// <summary>
		/// Write out all the xml required for a state action
		/// </summary>
		/// <param name="rXMLFile"></param>
		public void WriteXml(XmlTextWriter rXMLFile)
		{
			rXMLFile.WriteStartElement("Item");
			rXMLFile.WriteAttributeString("Type", StateActionFactory.TypeToXMLString(ActionType));

			//write out the type
			rXMLFile.WriteStartElement("type");
			rXMLFile.WriteString(ActionType.ToString());
			rXMLFile.WriteEndElement();

			//write out the time
			rXMLFile.WriteStartElement("time");
			rXMLFile.WriteString(Time.ToString());
			rXMLFile.WriteEndElement();

			//write out the action specific crap
			WriteActionXml(rXMLFile);

			rXMLFile.WriteEndElement(); //Item
		}

		/// <summary>
		/// overloaded in child classes to write out action specific stuff
		/// </summary>
		/// <param name="rXMLFile"></param>
		protected virtual void WriteActionXml(XmlTextWriter rXMLFile)
		{
		}
#endif

		#endregion //File IO
	}
}
