using AnimationLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using StateMachineBuddy;
using System;
using System.Diagnostics;
using System.Xml;
using Vector2Extensions;

namespace GameDonkey
{
	public class CreateThrowAction : CreateAttackAction
	{
		#region Members

		//After the throw connects:

		/// <summary>
		/// The message to send to the state machine when this grab connects, to switch to the throw
		/// </summary>
		protected string m_strThrowMessage;

		/// <summary>
		/// that message, loaded from the state machine
		/// </summary>
		protected int m_iThrowMessage;

		/// <summary>
		/// the time delta after the grab connects to release the other characters
		/// </summary>
		protected float m_fReleaseTimeDelta;

		/// <summary>
		/// the time to let go of the character, set at runtime when throw is activated
		/// </summary>
		protected float m_fTimeToRelease;

		#endregion //Members

		#region Properties

		public float TimeToRelease
		{
			get { return m_fTimeToRelease; }
		}

		public int ThrowMessage
		{
			get { return m_iThrowMessage; }
		}

		public Bone Bone
		{
			get { return m_rAttackBone; }
		}

		public Vector2 ReleaseDirection
		{
			get { return m_Direction; }
		}

		#endregion //Properties

		#region Methods

		/// <summary>
		/// Standard constructor
		/// </summary>
		public CreateThrowAction(BaseObject rOwner) : base(rOwner, EActionType.CreateThrow)
		{
			m_strThrowMessage = "";
			m_iThrowMessage = 0;
			m_fReleaseTimeDelta = 0.0f;
			m_fTimeToRelease = 0.0f;
		}

		/// <summary>
		/// execute all the success actions after this attack lands
		/// </summary>
		/// <param name="rCharacterHit">The dude that got nailed by this attack</param>
		/// <returns>bool: whether or not a state change occurred while this dude was running</returns>
		public override bool ExecuteSuccessActions(BaseObject rCharacterHit)
		{
			Debug.Assert(null != Owner);
			Debug.Assert(null != Owner.CharacterClock);
			Debug.Assert(null != rCharacterHit);

			//send the state message
			Owner.SendStateMessage(ThrowMessage);

			//activate the throw
			m_fTimeToRelease = rCharacterHit.CharacterClock.CurrentTime + m_fReleaseTimeDelta;
			rCharacterHit.CurrentThrow = this;

			return base.ExecuteSuccessActions(rCharacterHit);
		}

		public override bool Compare(IBaseAction rInst)
		{
			CreateThrowAction myAction = (CreateThrowAction)rInst;

			Debug.Assert(ActionType == myAction.ActionType);
			Debug.Assert(m_strThrowMessage == myAction.m_strThrowMessage);
			Debug.Assert(m_iThrowMessage == myAction.m_iThrowMessage);
			Debug.Assert(m_fReleaseTimeDelta == myAction.m_fReleaseTimeDelta);
			Debug.Assert(m_fTimeToRelease == myAction.m_fTimeToRelease);
			
			return base.Compare(rInst);
		}

		#endregion //Methods

		#region File IO

		protected override bool ReadActionAttribute(XmlNode childNode, IGameDonkey rEngine, StateMachine rStateMachine)
		{
			//what is in this node?
			string strName = childNode.Name;
			string strValue = childNode.InnerText;

			if (strName == "throwMessage")
			{
				m_strThrowMessage = strValue;
				m_iThrowMessage = Owner.States.GetMessageIndexFromText(m_strThrowMessage);
				if (0 > m_iThrowMessage)
				{
					Debug.Assert(0 <= m_iThrowMessage);
					return false;
				}
				if (Owner.States.NumMessages() <= m_iThrowMessage)
				{
					Debug.Assert(Owner.States.NumMessages() > m_iThrowMessage);
					return false;
				}
			}
			else if (strName == "releaseTimeDelta")
			{
				m_fReleaseTimeDelta = Convert.ToSingle(strValue);
				if (0.0f > m_fReleaseTimeDelta)
				{
					Debug.Assert(0.0f <= m_fReleaseTimeDelta);
					return false;
				}
			}
			else if (strName == "releaseDirection")
			{
				m_Direction = strValue.ToVector2();
			}
			
			return base.ReadActionAttribute(childNode, rEngine, rStateMachine);
		}

		/// <summary>
		/// overloaded in child classes to write out action specific stuff
		/// </summary>
		/// <param name="rXMLFile"></param>
		protected override void WriteActionXml(XmlTextWriter rXMLFile)
		{
			base.WriteXml(rXMLFile);

			rXMLFile.WriteStartElement("throwMessage");
			rXMLFile.WriteString(m_strThrowMessage);
			rXMLFile.WriteEndElement();

			rXMLFile.WriteStartElement("releaseTimeDelta");
			rXMLFile.WriteString(m_fReleaseTimeDelta.ToString());
			rXMLFile.WriteEndElement();
		}

		/// <summary>
		/// Read from a serialized file
		/// </summary>
		/// <param name="myAction">the xml item to read the action from</param>
		public bool ReadSerialized(SPFSettings.CreateThrowActionXML myAction, IGameDonkey rEngine, ContentManager rXmlContent, StateMachine rStateMachine)
		{
			Debug.Assert(myAction.type == ActionType.ToString());

			m_strThrowMessage = myAction.throwMessage;
			m_iThrowMessage = Owner.States.GetMessageIndexFromText(m_strThrowMessage);
			Debug.Assert(m_iThrowMessage >= 0);
			m_fReleaseTimeDelta = myAction.releaseTimeDelta;

			return base.ReadSerialized(myAction, rEngine,rXmlContent, rStateMachine);
		}

		#endregion //File IO
	}
}