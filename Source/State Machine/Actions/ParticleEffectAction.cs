using AnimationLib;
using XmlBuddy;
using Microsoft.Xna.Framework;
using ParticleBuddy;
using System;
using System.Diagnostics;
using System.Xml;
using Vector2Extensions;

namespace GameDonkey
{
	public class ParticleEffectAction : IBaseAction
	{
		#region Members

		/// <summary>
		/// the particle effect template to use
		/// </summary>
		public EmitterTemplate Emitter { get; private set; }

		/// <summary>
		/// the name of the bone to emanate from
		/// </summary>
		private string _boneName;

		/// <summary>
		/// the bone to attach the particle emitter to
		/// </summary>
		private Bone _bone;

		/// <summary>
		/// the direction to shoot the particle effect
		/// </summary>
		public ActionDirection Velocity { get; set; }

		/// <summary>
		/// the offset from the character origin to start the particle effect from
		/// ignored if the source bone is set
		/// </summary>
		public Vector2 StartOffset { get; set; }

		private ParticleEngine ParticleEngine { get; set; }

		/// <summary>
		/// When a particle is fired, whether or not it should match the rotation of the specified bone.
		/// </summary>
		public bool UseBoneRotation { get; set; }

		#endregion //Members

		#region Properties

		public string BoneName
		{
			get { return _boneName; }
			set
			{
				Debug.Assert(null != Owner);
				_boneName = value;
				if (String.IsNullOrEmpty(_boneName))
				{
					_bone = null;
				}
				else
				{
					_bone = Owner.AnimationContainer.Model.GetBone(_boneName);
					Debug.Assert(null != _bone);
				}
			}
		}

		#endregion //Properties

		#region Methods

		/// <summary>
		/// Standard constructor
		/// </summary>
		public ParticleEffectAction(BaseObject rOwner, IGameDonkey rEngine) : base(rOwner)
		{
			Debug.Assert(null != rEngine);
			Debug.Assert(null != rEngine.ParticleEngine);
			ActionType = EActionType.ParticleEffect;
			Emitter = new EmitterTemplate();
			_boneName = "";
			_bone = null;
			Velocity = new ActionDirection();
			StartOffset = new Vector2(0.0f);
			UseBoneRotation = false;

			ParticleEngine = rEngine.ParticleEngine;
		}

		/// <summary>
		/// execute this action (overridden in all child classes)
		/// </summary>
		/// <returns>bool: whether or not to continue running actions after this dude runs</returns>
		public override bool Execute()
		{
			Debug.Assert(null != ParticleEngine);
			Debug.Assert(!AlreadyRun);

			Emitter myEmitter = ParticleEngine.PlayParticleEffect(
				Emitter, 
				Velocity.GetDirection(Owner), 
				Owner.Position,
				StartOffset,
				Emitter.ParticleColor,
				GetFlip(),
				GetPosDelegate(),
				GetRotationDelegate(),
				GetOwnerRotation());
			
			if (null != myEmitter)
			{
				Owner.Emitters.Add(myEmitter);
			}

			return base.Execute();
		}

		private PositionDelegate GetPosDelegate()
		{
			if (null != _bone)
			{
				return _bone.GetPosition;
			}
			
			return null;
		}

		private RotationDelegate GetRotationDelegate()
		{
			if ((null != _bone) && UseBoneRotation)
			{
				return _bone.TrueRotationAngle;
			}

			return null;
		}

		private bool GetFlip()
		{
			if ((null != _bone) && UseBoneRotation)
			{
				return _bone.Flipped;
			}

			return Owner.Flip;
		}

		private RotationDelegate GetOwnerRotation()
		{
			if ((null != _bone) && UseBoneRotation)
			{
				return null;
			}

			return Owner.Rotation;
		}

		public override bool Compare(IBaseAction rInst)
		{
			ParticleEffectAction myAction = (ParticleEffectAction)rInst;

			Debug.Assert(ActionType == myAction.ActionType);
			Debug.Assert(Time == myAction.Time);
			Debug.Assert(Emitter.Compare(myAction.Emitter));
			Debug.Assert(_boneName == myAction._boneName);
			Debug.Assert(Velocity.Compare(myAction.Velocity));
			Debug.Assert(Emitter.ParticleColor == myAction.Emitter.ParticleColor);
			Debug.Assert(StartOffset.X == myAction.StartOffset.X);
			//Debug.Assert(m_StartOffset.Y == myAction.m_StartOffset.Y);

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
			Debug.Assert(null != rEngine);
			Debug.Assert(null != ParticleEngine);

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
						case "bone":
						{
							_boneName = strValue;
							if (!string.IsNullOrEmpty(_boneName))
							{
								_bone = Owner.AnimationContainer.Model.GetBone(_boneName);
								Debug.Assert(null != _bone);
							}
						}
						break;
						case "direction":
						{
							try
							{
								Velocity.ReadXml(childNode);
							}
							catch (Exception)
							{
								Vector2 vect = strValue.ToVector2();
							}
						}
						break;
						case "StartOffset":
						{
							StartOffset = strValue.ToVector2();
						}
						break;
						case "UseBoneRotation":
						{
							UseBoneRotation = Convert.ToBoolean(strValue);
						}
						break;
						case "emitter":
						{
							if (!childNode.HasChildNodes)
							{
								//has to have the emiiter xml under here
								Debug.Assert(false);
								return false;
							}

							XmlFileBuddy.ReadChildNodes(childNode.FirstChild, Emitter.ParseXmlNode);
							if (null != rEngine)
							{
								Emitter.LoadContent(rEngine.Renderer);
							}
						}
						break;
						case "useObjectDirection":
						{
							bool dir = Convert.ToBoolean(strValue);
							Velocity.DirectionType = (dir ? EDirectionType.Controller : EDirectionType.Absolute);
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
			rXMLFile.WriteStartElement("bone");
			rXMLFile.WriteString(_boneName);
			rXMLFile.WriteEndElement();

			rXMLFile.WriteStartElement("direction");
			Velocity.WriteXml(rXMLFile);
			rXMLFile.WriteEndElement();

			rXMLFile.WriteStartElement("StartOffset");
			rXMLFile.WriteString(StartOffset.StringFromVector());
			rXMLFile.WriteEndElement();

			rXMLFile.WriteStartElement("UseBoneRotation");
			rXMLFile.WriteString(UseBoneRotation.ToString());
			rXMLFile.WriteEndElement();

			rXMLFile.WriteStartElement("emitter");

			rXMLFile.WriteStartElement("Item");
			rXMLFile.WriteAttributeString("Type", Emitter.ContentName);

			Emitter.WriteXmlNodes(rXMLFile);

			rXMLFile.WriteEndElement();
			rXMLFile.WriteEndElement();

			rXMLFile.WriteEndElement();
		}

		#endregion //File IO
	}
}