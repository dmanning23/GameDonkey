using System;
using System.Diagnostics;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using FilenameBuddy;

namespace GameDonkey
{
	public class CProjectileAction : IBaseAction
	{
		#region Members

		/// <summary>
		/// the projectile to add
		/// </summary>
		private BaseObject m_rProjectile;

		/// <summary>
		/// the filename of the projectile data.xml file to use
		/// </summary>
		private Filename m_strProjectileFileName;

		/// <summary>
		/// the offset from the bone to start the particle effect from
		/// this is ignored if the bone thing is set
		/// </summary>
		public Vector2 StartOffset { get; set; }

		/// <summary>
		/// Whether or not we want this projectile to get initial velocity from the left thumbstick to get its direction.
		/// </summary>
		private bool m_bUseObjectDirection;

		/// <summary>
		/// The direction to set the projectile's initial velocity when this action is run.
		/// This is only used if the thumbstick flag is false
		/// </summary>
		private Vector2 m_Velocity;

		/// <summary>
		/// The length of the velocity to add to the projectile.
		/// This is only used if the thumbstick flag is true
		/// </summary>
		private float m_fVelocityLength;

		/// <summary>
		/// How much to scale the projectile.  Read in from the xml file, not the "runtime scale"
		/// </summary>
		private float m_fScale;

		#endregion //Members

		#region Properties

		public Filename Filename
		{
			get { return m_strProjectileFileName; }
		}

		public Vector2 Velocity
		{
			get { return m_Velocity; }
			set 
			{ 
				m_Velocity = value;
				m_fVelocityLength = m_Velocity.Length();
			}
		}

		public bool UseObjectDirection
		{
			get { return m_bUseObjectDirection; }
			set { m_bUseObjectDirection = value; }
		}

		public float VelocityLength
		{
			get { return m_fVelocityLength; }
		}

		public float Scale
		{
			get { return m_fScale; }
			set
			{
				m_fScale = value;
				if (null != m_rProjectile)
				{
					m_rProjectile.Scale = m_fScale * Owner.Scale;
				}
			}
		}

		#endregion //Properties

		#region Methods

		/// <summary>
		/// Standard constructor
		/// </summary>
		public CProjectileAction(BaseObject rOwner) : base(rOwner)
		{
			ActionType = EActionType.Projectile;
			m_rProjectile = null;
			m_strProjectileFileName = new Filename();
			StartOffset = new Vector2(0.0f);
			m_fScale = 1.0f;
			m_Velocity = new Vector2(0.0f);
			m_bUseObjectDirection = false;
			m_fVelocityLength = 0.0f;
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

			if (null == m_rProjectile)
			{
				//boo... you need to have a projectile loaded
				return true;
			}

			//activate the projectile
			bool bActivated = Owner.PlayerQueue.ActivateObject(m_rProjectile);

			//if it was activated (wont be activated if already active)
			if (bActivated)
			{
				//get the start position for the projectile
				Vector2 ProjectilePosition = StartOffset * m_rProjectile.Scale;
				ProjectilePosition.Y = Owner.Position.Y + ProjectilePosition.Y;
				ProjectilePosition.X = Owner.Position.X + (Owner.Flip ? -ProjectilePosition.X : ProjectilePosition.X);

				//set the position
				m_rProjectile.Position = ProjectilePosition;
				m_rProjectile.Flip = Owner.Flip;

				//set the initial velocity
				Vector2 myVelocity = Velocity;

				//If we want the thumbstick direction and it is pointing in a direction...
				if (m_bUseObjectDirection && (Owner.Direction() != Vector2.Zero))
				{
					//use the thumbstick direction from the object
					myVelocity = Owner.Direction() * VelocityLength;
				}
				else
				{
					//use the velocity from this action
					myVelocity.X = (Owner.Flip ? -Velocity.X : Velocity.X);
				}

				m_rProjectile.Velocity = myVelocity * m_rProjectile.Scale;

				//run the animation container so all the bones will be in the correct position when it updates
				//This way, any particle effects created will be in correct location.
				m_rProjectile.AnimationContainer.SetAnimation(0, EPlayback.Loop);
				m_rProjectile.AnimationContainer.Update(Owner.PlayerQueue.CharacterClock, 
					m_rProjectile.Position, 
					m_rProjectile.Flip, 
					m_rProjectile.Scale, 
					0.0f, 
					true);
			}

			//goddamit, make sure that worked!!!
			Debug.Assert(Owner.PlayerQueue.CheckListForObject(m_rProjectile, true));
			Debug.Assert(!Owner.PlayerQueue.CheckListForObject(m_rProjectile, false));

			return base.Execute();
		}

		public override bool Compare(IBaseAction rInst)
		{
			CProjectileAction myAction = (CProjectileAction)rInst;

			Debug.Assert(ActionType == myAction.ActionType);
			Debug.Assert(Time == myAction.Time);
			Debug.Assert(m_strProjectileFileName.Filename == myAction.m_strProjectileFileName.Filename);
			//Debug.Assert(m_StartOffset.X == myAction.m_StartOffset.X);
			//Debug.Assert(m_StartOffset.Y == myAction.m_StartOffset.Y);
			Debug.Assert(m_bUseObjectDirection == myAction.m_bUseObjectDirection);
			Debug.Assert(VelocityLength == myAction.VelocityLength);

			return true;
		}

#if WINDOWS

		public bool SetFilename(string strBitmapFile, IGameDonkey rEngine)
		{
			//grab the filename
			m_strProjectileFileName.SetRelFilename(strBitmapFile);

			//try to load the file into the particle effect
			if ((null != rEngine) && !String.IsNullOrEmpty(strBitmapFile))
			{
				//load object into player queue!
				m_rProjectile = Owner.PlayerQueue.LoadObject(m_strProjectileFileName, rEngine, EObjectType.Projectile, 0);
				if (null == m_rProjectile)
				{
					return false;
				}

				m_rProjectile.Scale = Scale * Owner.Scale;
			}

			return true;
		}

#endif

		#endregion //Methods

		#region File IO

#if WINDOWS

		/// <summary>
		/// Read from an xml file
		/// </summary>
		/// <param name="rXMLNode">the xml node to read from</param>
		/// <returns></returns>
		public bool ReadSerialized(XmlNode rXMLNode, IGameDonkey rEngine)
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
					if (ActionType != IBaseAction.XMLTypeToType(strValue))
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

			//temp variable to hold the filename
			string strFileName = "";

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
					else if (strName == "filename")
					{
						strFileName = strValue;
					}
					else if (strName == "startOffset")
					{
						StartOffset = CStringUtils.ReadVectorFromString(strValue);
					}
					else if (strName == "scale")
					{
						Scale = Convert.ToSingle(strValue);
					}
					else if (strName == "velocity")
					{
						Velocity = CStringUtils.ReadVectorFromString(strValue);
					}
					else if (strName == "useObjectDirection")
					{
						m_bUseObjectDirection = Convert.ToBoolean(strValue);
					}
					else
					{
						Debug.Assert(false);
						return false;
					}
				}
			}

			//load the projectile object
			return SetFilename(strFileName, rEngine);
		}

		/// <summary>
		/// overloaded in child classes to write out action specific stuff
		/// </summary>
		/// <param name="rXMLFile"></param>
		public override void WriteXML(XmlTextWriter rXMLFile)
		{
			rXMLFile.WriteStartElement("filename");
			rXMLFile.WriteString(m_strProjectileFileName.GetRelFilename());
			rXMLFile.WriteEndElement();

			rXMLFile.WriteStartElement("startOffset");
			rXMLFile.WriteString(CStringUtils.StringFromVector(StartOffset));
			rXMLFile.WriteEndElement();

			rXMLFile.WriteStartElement("scale");
			rXMLFile.WriteString(m_fScale.ToString());
			rXMLFile.WriteEndElement();

			rXMLFile.WriteStartElement("velocity");
			rXMLFile.WriteString(CStringUtils.StringFromVector(Velocity));
			rXMLFile.WriteEndElement();

			rXMLFile.WriteStartElement("useObjectDirection");
			rXMLFile.WriteString(m_bUseObjectDirection ? "true" : "false");
			rXMLFile.WriteEndElement();
		}

#endif

		/// <summary>
		/// Read from a serialized file
		/// </summary>
		/// <param name="myAction">the xml item to read the action from</param>
		public bool ReadSerialized(SPFSettings.ProjectileActionXML myAction, IGameDonkey rEngine, ContentManager rXmlContent)
		{
			Debug.Assert(myAction.type == ActionType.ToString());
			Debug.Assert(null != Owner);
			Debug.Assert(null != Owner.PlayerQueue);

			ReadSerializedBase(myAction);

			StartOffset = myAction.startOffset;

			//load the projectile object
			m_strProjectileFileName.SetRelFilename(myAction.filename);

			//load object into player queue!
			m_rProjectile = Owner.PlayerQueue.LoadObject(rXmlContent, m_strProjectileFileName, rEngine, EObjectType.Projectile, 0);
			if (null == m_rProjectile)
			{
				return false;
			}

			Velocity = myAction.velocity;
			m_bUseObjectDirection = myAction.useObjectDirection;

			Scale = myAction.scale;

			return true;
		}

		#endregion //File IO
	}
}