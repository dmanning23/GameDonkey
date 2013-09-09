using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Xml;
using Microsoft.Xna.Framework.Content;
using StateMachineBuddy;
using AnimationLib;
using CollisionBuddy;

namespace GameDonkey
{
	/// <summary>
	/// This is the state action used for when a state is a "blocking" state
	/// It runs until the state is exited.
	/// </summary>
	public class BlockingStateAction : TimedAction
	{
		#region Members

		//the name of the bone to use
		protected string m_strBoneName;

		//The bone this attack uses
		public Bone AttackBone { get; private set; }

		/// <summary>
		/// The sound to play when this action blocks an attack
		/// </summary>
		public string HitSound { get; private set; }

		/// <summary>
		/// A list of actions that will be run if this action blocks an attack (sound effects, particle effects, etc)
		/// </summary>
		protected List<IBaseAction> m_listSuccessActions;

		#endregion //Members

		#region Properties

		public string BoneName
		{
			get { return m_strBoneName; }
			set
			{
				Debug.Assert(null != Owner);
				m_strBoneName = value;

				//if the bone name is changed, it means the bone needs to be reset too...
				AttackBone = null;
			}
		}

		#endregion //Properties

		#region Methods

		/// <summary>
		/// Standard constructor
		/// </summary>
		public BlockingStateAction(BaseObject rOwner)
			: base(rOwner)
		{
			ActionType = EActionType.BlockState;
			m_strBoneName = "";
			AttackBone = null;
			HitSound = "";
			m_listSuccessActions = new List<IBaseAction>();
		}

		/// <summary>
		/// execute this action (overridden in all child classes)
		/// </summary>
		/// <returns>bool: whether or not to continue running actions after this dude runs</returns>
		public override bool Execute()
		{
			Debug.Assert(null != Owner);
			Debug.Assert(null != Owner.CharacterClock);
			Debug.Assert(!AlreadyRun);

			//Check if the bone is set, if not try and find it...
			if (null == AttackBone)
			{
				AttackBone = Owner.Physics.FindWeapon(m_strBoneName);
			}

			//reset teh success actions
			for (int i = 0; i < m_listSuccessActions.Count; i++)
			{
				m_listSuccessActions[i].AlreadyRun = false;
			}

			//add this action to the list of block states
			Owner.CurrentBlocks.AddAction(this, Owner.CharacterClock);

			return base.Execute();
		}

		public override bool Compare(IBaseAction rInst)
		{
			BlockingStateAction myAction = rInst as BlockingStateAction;
			Debug.Assert(null != myAction);

			Debug.Assert(ActionType == myAction.ActionType);
			Debug.Assert(m_strBoneName == myAction.m_strBoneName);
			//Debug.Assert(m_Direction.X == myAction.m_Direction.X);
			//Debug.Assert(m_Direction.Y == myAction.m_Direction.Y);

			for (int i = 0; i < m_listSuccessActions.Count; i++)
			{
				if (!m_listSuccessActions[i].Compare(myAction.m_listSuccessActions[i]))
				{
					return false;
				}
			}

			return true;
		}

		public virtual Circle GetCircle()
		{
			//return the first circle from this dude's image

			if (null == AttackBone)
			{
				return null;
			}

			//get the current image
			Image rMyImage = AttackBone.GetCurrentImage();

			//hit bones and images must have one circle
			if ((null == rMyImage) || (rMyImage.Circles.Count < 1))
			{
				return null;
			}

			//get the circle
			Circle rMyCircle = rMyImage.Circles[0];
			Debug.Assert(null != rMyCircle);

			return rMyCircle;
		}

		/// <summary>
		/// execute all the success actions after this attack lands
		/// </summary>
		/// <returns>bool: whether or not a state change occurred while this dude was running</returns>
		public bool ExecuteSuccessActions()
		{
			//TODO: play the loaded "blocked" sound

			for (int i = 0; i < m_listSuccessActions.Count; i++)
			{
				if (m_listSuccessActions[i].Execute())
				{
					return true;
				}
			}

			return false;
		}

		#endregion //Methods

		#region File IO

		/// <summary>
		/// Read from an xml file
		/// </summary>
		/// <param name="rXMLNode">the xml node to read from</param>
		/// <returns></returns>
		public virtual bool ReadSerialized(System.Xml.XmlNode rXMLNode, IGameDonkey rEngine, StateMachine rStateMachine)
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
					if (!ReadActionAttribute(childNode, rEngine, rStateMachine))
					{
						return false;
					}
				}
			}

			return true;
		}

		protected virtual bool ReadActionAttribute(XmlNode childNode, IGameDonkey rEngine, StateMachine rStateMachine)
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
			else if (strName == "timeDelta")
			{
				TimeDelta = Convert.ToSingle(strValue);
			}
			else if (strName == "boneName")
			{
				BoneName = strValue;
			}
			else if (strName == "hitSound")
			{
				HitSound = strValue;
			}
			else if (strName == "successActions")
			{
				//Read in all the success actions
				if (!IBaseAction.ReadListActions(Owner, ref m_listSuccessActions, childNode, rEngine, rStateMachine))
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// overloaded in child classes to write out action specific stuff
		/// </summary>
		/// <param name="rXMLFile"></param>
		public override void WriteXML(XmlTextWriter rXMLFile)
		{
			rXMLFile.WriteStartElement("timeDelta");
			rXMLFile.WriteString(TimeDelta.ToString());
			rXMLFile.WriteEndElement();

			rXMLFile.WriteStartElement("boneName");
			rXMLFile.WriteString(m_strBoneName);
			rXMLFile.WriteEndElement();

			rXMLFile.WriteStartElement("hitSound");
			rXMLFile.WriteString(HitSound);
			rXMLFile.WriteEndElement();

			rXMLFile.WriteStartElement("successActions");
			for (int i = 0; i < m_listSuccessActions.Count; i++)
			{
				m_listSuccessActions[i].WriteXMLFormat(rXMLFile);
			}
			rXMLFile.WriteEndElement();
		}

		/// <summary>
		/// Read from a serialized file
		/// </summary>
		/// <param name="myAction">the xml item to read the action from</param>
		public virtual bool ReadSerialized(SPFSettings.BlockingStateActionXML myAction, IGameDonkey rEngine, ContentManager rXmlContent, StateMachine rStateMachine)
		{
			Debug.Assert(myAction.type == ActionType.ToString());
			ReadSerializedBase(myAction);

			TimeDelta = myAction.timeDelta;

			//read in serialized action
			BoneName = myAction.boneName;

			HitSound = myAction.hitSound;
			//TODO: verify the sound action
			//Debug.Assert(null != CAudioManager.GetCue(m_strHitSound));
			IBaseAction.ReadListActions(Owner, myAction.successActions, ref m_listSuccessActions, rEngine, rXmlContent, rStateMachine);

			return true;
		}

		#endregion //File IO
	}
}