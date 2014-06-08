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
	/// <summary>
	/// This is an attack action that uses a unattached circle instead of a bone
	/// </summary>
	class CreateHitCircleAction : CreateAttackAction
	{
		#region Members

		/// <summary>
		/// this dudes hit circle that will be floating around
		/// </summary>
		protected PhysicsCircle m_HitCircle;

		/// <summary>
		/// the offset from the attached bone location to start this circle at
		/// </summary>
		protected Vector2 m_StartOffset;

		/// <summary>
		/// speed and direction of this circle
		/// </summary>
		protected Vector2 m_Velocity;

		#endregion //Members

		#region Methods

		public CreateHitCircleAction(BaseObject rOwner) : base(rOwner, EActionType.CreateHitCircle)
		{
			m_HitCircle = new PhysicsCircle();
			m_StartOffset = Vector2.Zero;
			m_Velocity = Vector2.Zero;
		}

		/// <summary>
		/// execute this action (overridden in all child classes)
		/// </summary>
		/// <returns>bool: whether or not to continue running actions after this dude runs</returns>
		public override bool Execute()
		{
			//set the circle location

			//get the bone location
			Debug.Assert(null != m_rAttackBone);
			Vector2 myLocation = m_rAttackBone.AnchorPosition;

			//get the start offset
			Vector2 myOffset = m_StartOffset;
			if (Owner.Flip)
			{
				myOffset.X *= -1.0f;
			}

			//set the circle location
			m_HitCircle.Reset(myLocation - myOffset);

			return base.Execute();
		}

		public override bool Compare(IBaseAction rInst)
		{
			return base.Compare(rInst);
		}

		public override void Update()
		{
			//add the velocity
			Vector2 myPosition = m_HitCircle.Pos + ((m_Velocity * Owner.Scale) * Owner.CharacterClock.TimeDelta);

			//update the circle location
			m_HitCircle.Update(myPosition, Owner.Scale);
		}

		public override PhysicsCircle GetCircle()
		{
			return m_HitCircle;
		}

		#endregion //Methods

		#region File IO

		protected override bool ReadActionAttribute(XmlNode childNode, IGameDonkey rEngine, SingleStateContainer stateContainer)
		{
			//what is in this node?
			string strName = childNode.Name;
			string strValue = childNode.InnerText;

			if (strName == "radius")
			{
				//set the radius of that circle
				m_HitCircle.Radius = Convert.ToSingle(strValue);
			}
			else if (strName == "startOffset")
			{
				m_StartOffset = strValue.ToVector2();
			}
			else if (strName == "velocity")
			{
				m_Velocity = strValue.ToVector2();
			}

			return base.ReadActionAttribute(childNode, rEngine, stateContainer);
		}

		/// <summary>
		/// overloaded in child classes to write out action specific stuff
		/// </summary>
		/// <param name="rXMLFile"></param>
		protected override void WriteActionXml(XmlTextWriter rXMLFile)
		{
			base.WriteXml(rXMLFile);

			rXMLFile.WriteStartElement("radius");
			rXMLFile.WriteString(m_HitCircle.Radius.ToString());
			rXMLFile.WriteEndElement();

			rXMLFile.WriteStartElement("startOffset");
			rXMLFile.WriteString(m_StartOffset.StringFromVector());
			rXMLFile.WriteEndElement();

			rXMLFile.WriteStartElement("velocity");
			rXMLFile.WriteString(m_Velocity.StringFromVector());
			rXMLFile.WriteEndElement();
		}

		/// <summary>
		/// Read from a serialized file
		/// </summary>
		/// <param name="myAction">the xml item to read the action from</param>
		public bool ReadSerialized(SPFSettings.CreateHitCircleActionXML myAction, IGameDonkey rEngine, ContentManager rXmlContent, SingleStateContainer stateContainer)
		{
			Debug.Assert(myAction.type == ActionType.ToString());

			m_HitCircle.Radius = myAction.radius;
			m_StartOffset = myAction.startOffset;
			m_Velocity = myAction.velocity;

			return base.ReadSerialized(myAction, rEngine, rXmlContent, stateContainer);
		}

		#endregion //File IO
	}
}