using AnimationLib;
using FilenameBuddy;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using Vector2Extensions;

namespace GameDonkey
{
	public class CreateAttackAction : IBaseAction
	{
		#region Members

		/// <summary>
		/// the name of the bone to use
		/// </summary>
		private string m_strBoneName;

		/// <summary>
		/// The bone this attack uses.  Has to be a weapon bone!
		/// Starts out as null, and is set at runtime the first time this action is run.
		/// Since it can be a garment bone, these might not actually be in the model at startup
		/// </summary>
		protected Bone m_rAttackBone;

		/// <summary>
		/// the vector to set another object to when this attack connects
		/// </summary>
		protected Vector2 m_Direction;

		/// <summary>
		/// The sound to play if this attack hits.
		/// This sound is only played if the attack is not blocked
		/// </summary>
		public Filename HitSoundName { get; private set; }

		public SoundEffect HitSound { get; private set; }

		/// <summary>
		/// A list of actions that will be run if this attack connects (sound effects, particle effects, etc)
		/// This list of actions is played whether the attack is blocked or not.
		/// </summary>
		private List<IBaseAction> m_listSuccessActions;

		#endregion //Members

		#region Properties

		/// <summary>
		/// the time delta of how long the attack is active
		/// </summary>
		public float TimeDelta { get ; set; }

		/// <summary>
		/// The time when this attack is done.
		/// Set at runtime when the attack is activated
		/// </summary>
		public float DoneTime { get; protected set; }

		public string BoneName
		{
			get { return m_strBoneName; }
			set
			{
				Debug.Assert(null != Owner);
				m_strBoneName = value;

				//if the bone name is changed, it means the bone needs to be reset too...
				m_rAttackBone = null;
			}
		}

		public Vector2 Direction
		{
			get { return m_Direction; }
			set { m_Direction = value; }
		}

		/// <summary>
		/// the amount of damage to deal when this attack connects
		/// </summary>
		public float Damage { get; set ; }

		#endregion //Properties

		#region Methods

		/// <summary>
		/// Standard constructor
		/// </summary>
		public CreateAttackAction(BaseObject rOwner)
			: this(rOwner, EActionType.CreateAttack)
		{
		}

		/// <summary>
		/// constructor called by child classes
		/// </summary>
		/// <param name="rOwner">the dude this action belongs to</param>
		/// <param name="eType">the type of the chlid class</param>
		public CreateAttackAction(BaseObject rOwner, EActionType eType)
			: base(rOwner)
		{
			ActionType = eType;
			m_strBoneName = "";
			m_rAttackBone = null;
			m_Direction = new Vector2(0.0f);
			Damage = 0.0f;
			TimeDelta = 0.0f;
			DoneTime = 0.0f;
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
			if (null == m_rAttackBone)
			{
				m_rAttackBone = Owner.Physics.FindWeapon(m_strBoneName);
			}

			//reset teh success actions
			for (int i = 0; i < m_listSuccessActions.Count; i++)
			{
				m_listSuccessActions[i].AlreadyRun = false;
			}

			//activate the attack
			DoneTime = Owner.CharacterClock.CurrentTime + TimeDelta;

			//add this actionto the list of attacks
			Owner.AddAttack(this);

			return base.Execute();
		}

		public override bool Compare(IBaseAction rInst)
		{
			CreateAttackAction myAction = (CreateAttackAction)rInst;

			Debug.Assert(ActionType == myAction.ActionType);
			Debug.Assert(Time == myAction.Time);
			Debug.Assert(m_strBoneName == myAction.m_strBoneName);
			Debug.Assert(m_rAttackBone == myAction.m_rAttackBone);
			//Debug.Assert(m_Direction.X == myAction.m_Direction.X);
			//Debug.Assert(m_Direction.Y == myAction.m_Direction.Y);
			Debug.Assert(Damage == myAction.Damage);
			Debug.Assert(TimeDelta == myAction.TimeDelta);
			Debug.Assert(DoneTime == myAction.DoneTime);

			for (int i = 0; i < m_listSuccessActions.Count; i++)
			{
				if (!m_listSuccessActions[i].Compare(myAction.m_listSuccessActions[i]))
				{
					Debug.Assert(false);
					return false;
				}
			}

			return true;
		}

		public virtual void Update()
		{
			//nothing to do here, used in child classes
		}

		public virtual PhysicsCircle GetCircle()
		{
			//return the first circle from this dude's image

			//Check if the bone is set, if not try and find it...
			if (null == m_rAttackBone)
			{
				m_rAttackBone = Owner.Physics.FindWeapon(m_strBoneName);
			}

			//the bone for this attack is in a garment that isnt being displayed
			if (null == m_rAttackBone)
			{
				return null;
			}

			//get the current image
			Image rMyImage = m_rAttackBone.GetCurrentImage();

			//hit bones and images must have one circle
			if ((null == rMyImage) || (rMyImage.Circles.Count < 1))
			{
				return null;
			}

			//get the circle
			PhysicsCircle rMyCircle = rMyImage.Circles[0];
			Debug.Assert(null != rMyCircle);

			return rMyCircle;
		}

		/// <summary>
		/// execute all the success actions after this attack lands
		/// </summary>
		/// <param name="rCharacterHit">The dude that got nailed by this attack</param>
		/// <returns>bool: whether or not a state change occurred while this dude was running</returns>
		public virtual bool ExecuteSuccessActions(BaseObject rCharacterHit)
		{
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
					if (!ReadActionAttribute(childNode, rEngine, stateContainer))
					{
						Debug.Assert(false);
						return false;
					}
				}
			}

			return true;
		}

		protected virtual bool ReadActionAttribute(XmlNode childNode, IGameDonkey rEngine, SingleStateContainer stateContainer)
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
			else if (strName == "boneName")
			{
				BoneName = strValue;
			}
			else if (strName == "direction")
			{
				m_Direction = strValue.ToVector2();
			}
			else if (strName == "damage")
			{
				Damage = Convert.ToSingle(strValue);
				if (0.0f > Damage)
				{
					Debug.Assert(0.0f <= Damage);
					return false;
				}
			}
			else if (strName == "timeDelta")
			{
				TimeDelta = Convert.ToSingle(strValue);
				if (0.0f > TimeDelta)
				{
					Debug.Assert(0.0f <= TimeDelta);
					return false;
				}
			}
			else if ((strName == "hitSound") && !string.IsNullOrEmpty(strValue))
			{
				HitSoundName = new Filename(strValue);
				HitSound = rEngine.LoadSound(HitSoundName);
			}
			else if (strName == "successActions")
			{
				//Read in all the success actions
				if (!IBaseAction.ReadXmlListActions(Owner, ref m_listSuccessActions, childNode, rEngine, stateContainer))
				{
					Debug.Assert(false);
					return false;
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
			rXMLFile.WriteStartElement("boneName");
			rXMLFile.WriteString(m_strBoneName);
			rXMLFile.WriteEndElement();

			rXMLFile.WriteStartElement("direction");
			rXMLFile.WriteString(m_Direction.StringFromVector());
			rXMLFile.WriteEndElement();

			rXMLFile.WriteStartElement("damage");
			rXMLFile.WriteString(Damage.ToString());
			rXMLFile.WriteEndElement();

			rXMLFile.WriteStartElement("timeDelta");
			rXMLFile.WriteString(TimeDelta.ToString());
			rXMLFile.WriteEndElement();

			rXMLFile.WriteStartElement("hitSound");
			rXMLFile.WriteString(HitSoundName.GetRelFilename());
			rXMLFile.WriteEndElement();

			rXMLFile.WriteStartElement("successActions");
			for (int i = 0; i < m_listSuccessActions.Count; i++)
			{
				m_listSuccessActions[i].WriteXml(rXMLFile);
			}
			rXMLFile.WriteEndElement();
		}

		#endregion //File IO
	}
}