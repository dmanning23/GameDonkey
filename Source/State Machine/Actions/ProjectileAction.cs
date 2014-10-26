using AnimationLib;
using FilenameBuddy;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;
using System.Diagnostics;
using System.Xml;
using Vector2Extensions;

namespace GameDonkey
{
	public class ProjectileAction : IBaseAction
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
		/// The direction to set the projectile's initial velocity when this action is run.
		/// This is only used if the thumbstick flag is false
		/// </summary>
		public ActionDirection Velocity { get; set; }

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
		public ProjectileAction(BaseObject rOwner) : base(rOwner)
		{
			ActionType = EActionType.Projectile;
			m_rProjectile = null;
			m_strProjectileFileName = new Filename();
			StartOffset = new Vector2(0.0f);
			m_fScale = 1.0f;
			Velocity = new ActionDirection();
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

				m_rProjectile.Velocity = (Velocity.GetDirection(Owner) / Owner.Scale) * m_rProjectile.Scale;

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
			ProjectileAction myAction = (ProjectileAction)rInst;

			Debug.Assert(ActionType == myAction.ActionType);
			Debug.Assert(Time == myAction.Time);
			Debug.Assert(m_strProjectileFileName.File == myAction.m_strProjectileFileName.File);
			//Debug.Assert(m_StartOffset.X == myAction.m_StartOffset.X);
			//Debug.Assert(m_StartOffset.Y == myAction.m_StartOffset.Y);
			Debug.Assert(Velocity.Compare(myAction.Velocity));

			return true;
		}

		public bool SetFilename(string strBitmapFile, IGameDonkey rEngine)
		{
			//grab the filename
			m_strProjectileFileName.SetRelFilename(strBitmapFile);

			//try to load the file into the particle effect
			if ((null != rEngine) && !String.IsNullOrEmpty(strBitmapFile))
			{
				//load object into player queue!
				m_rProjectile = Owner.PlayerQueue.LoadXmlObject(m_strProjectileFileName, rEngine, EObjectType.Projectile, 0);
				if (null == m_rProjectile)
				{
					Debug.Assert(false);
					return false;
				}

				m_rProjectile.Scale = Scale * Owner.Scale;
			}

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
						StartOffset = strValue.ToVector2();
					}
					else if (strName == "scale")
					{
						Scale = Convert.ToSingle(strValue);
					}
					else if (strName == "direction")
					{
						Velocity.ReadXml(childNode);
					}
					else if (strName == "velocity")
					{
						Velocity.Velocity = strValue.ToVector2();
					}
					else if (strName == "useObjectDirection")
					{
						bool dir = Convert.ToBoolean(strValue);
						Velocity.DirectionType = (dir ? EDirectionType.Relative : EDirectionType.Absolute);
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
		protected override void WriteActionXml(XmlTextWriter rXMLFile)
		{
			rXMLFile.WriteStartElement("filename");
			rXMLFile.WriteString(m_strProjectileFileName.GetRelFilename());
			rXMLFile.WriteEndElement();

			rXMLFile.WriteStartElement("startOffset");
			rXMLFile.WriteString(StartOffset.StringFromVector());
			rXMLFile.WriteEndElement();

			rXMLFile.WriteStartElement("scale");
			rXMLFile.WriteString(m_fScale.ToString());
			rXMLFile.WriteEndElement();

			rXMLFile.WriteStartElement("direction");
			Velocity.WriteXml(rXMLFile);
			rXMLFile.WriteEndElement();
		}

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
			m_rProjectile = Owner.PlayerQueue.LoadSerializedObject(rXmlContent, m_strProjectileFileName, rEngine, EObjectType.Projectile, 0);
			if (null == m_rProjectile)
			{
				Debug.Assert(false);
				return false;
			}

			Velocity.ReadSerialized(myAction.direction);

			Scale = myAction.scale;

			return true;
		}

		#endregion //File IO
	}
}