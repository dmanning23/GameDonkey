using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ParticleBuddy;
using AnimationLib;

namespace GameDonkey
{
	public class ParticleEffectAction : IBaseAction
	{
		#region Members

		/// <summary>
		/// the particle effect template to use
		/// </summary>
		private EmitterTemplate m_rTemplate;

		/// <summary>
		/// the name of the bone to emanate from
		/// </summary>
		private string m_strBoneName;

		/// <summary>
		/// the bone to attach the particle emitter to
		/// </summary>
		private Bone m_rBone;

		/// <summary>
		/// the direction to shoot the particle effect
		/// </summary>
		public Vector2 Direction { get; set; }

		/// <summary>
		/// the offset from the character origin to start the particle effect from
		/// ignored if the source bone is set
		/// </summary>
		public Vector2 StartOffset { get; set; }

		private ParticleEngine m_ParticleEngine;

		#endregion //Members

		#region Properties

		public string BoneName
		{
			get { return m_strBoneName; }
			set
			{
				Debug.Assert(null != Owner);
				m_strBoneName = value;
				if (String.IsNullOrEmpty(m_strBoneName))
				{
					m_rBone = null;
				}
				else
				{
					m_rBone = Owner.AnimationContainer.Model.GetBone(m_strBoneName);
					Debug.Assert(null != m_rBone);
				}
			}
		}

		public EmitterTemplate Emitter
		{
			get { return m_rTemplate; }
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
			m_rTemplate = new EmitterTemplate();
			m_strBoneName = "";
			m_rBone = null;
			Direction = new Vector2(0.0f);
			StartOffset = new Vector2(0.0f);

			m_ParticleEngine = rEngine.ParticleEngine;
		}

		/// <summary>
		/// execute this action (overridden in all child classes)
		/// </summary>
		/// <returns>bool: whether or not to continue running actions after this dude runs</returns>
		public override bool Execute()
		{
			Debug.Assert(null != m_ParticleEngine);
			Debug.Assert(!AlreadyRun);

			Emitter myEmitter = m_ParticleEngine.PlayParticleEffect(m_rTemplate, Direction, Owner.Position, StartOffset, m_rBone, m_rTemplate.ParticleColor, Owner);
			Debug.Assert(null != myEmitter);
			Owner.Emitters.Add(myEmitter);

			return base.Execute();
		}

		public override bool Compare(IBaseAction rInst)
		{
			ParticleEffectAction myAction = (ParticleEffectAction)rInst;

			Debug.Assert(ActionType == myAction.ActionType);
			Debug.Assert(Time == myAction.Time);
			Debug.Assert(m_rTemplate.Compare(myAction.m_rTemplate));
			Debug.Assert(m_strBoneName == myAction.m_strBoneName);
			Debug.Assert(Direction.X == myAction.Direction.X);
			Debug.Assert(Direction.Y == myAction.Direction.Y);
			Debug.Assert(m_rTemplate.ParticleColor == myAction.m_rTemplate.ParticleColor);
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
		public bool ReadSerialized(XmlNode rXMLNode, IGameDonkey rEngine)
		{
			//read in xml action

			Debug.Assert(null != rEngine);
			Debug.Assert(null != m_ParticleEngine);

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
					else if (strName == "bone")
					{
						m_strBoneName = strValue;
						if (m_strBoneName != "")
						{
							m_rBone = Owner.AnimationContainer.Model.GetBone(m_strBoneName);
							Debug.Assert(null != m_rBone);
						}
					}
					else if (strName == "direction")
					{
						Direction = CStringUtils.ReadVectorFromString(strValue);
					}
					else if (strName == "StartOffset")
					{
						StartOffset = CStringUtils.ReadVectorFromString(strValue);
					}
					else if (strName == "emitter")
					{
						if (!childNode.HasChildNodes)
						{
							//has to have the emiiter xml under here
							Debug.Assert(false);
							return false;
						}

						if (!m_rTemplate.ReadXML(childNode.FirstChild, ((null == rEngine) ? null : rEngine.Renderer)))
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
			}

			return true;
		}

		/// <summary>
		/// overloaded in child classes to write out action specific stuff
		/// </summary>
		/// <param name="rXMLFile"></param>
		public override void WriteXML(XmlTextWriter rXMLFile)
		{
			rXMLFile.WriteStartElement("bone");
			rXMLFile.WriteString(m_strBoneName);
			rXMLFile.WriteEndElement();

			rXMLFile.WriteStartElement("direction");
			rXMLFile.WriteString(CStringUtils.StringFromVector(Direction));
			rXMLFile.WriteEndElement();

			rXMLFile.WriteStartElement("StartOffset");
			rXMLFile.WriteString(CStringUtils.StringFromVector(StartOffset));
			rXMLFile.WriteEndElement();

			rXMLFile.WriteStartElement("emitter");
			m_rTemplate.WriteXML(rXMLFile, false);
			rXMLFile.WriteEndElement();
		}

		/// <summary>
		/// Read from a serialized file
		/// </summary>
		/// <param name="myAction">the xml item to read the action from</param>
		public bool ReadSerialized(SPFSettings.ParticleEffectActionXML myAction, IGameDonkey rEngine)
		{
			Debug.Assert(myAction.type == ActionType.ToString());
			ReadSerializedBase(myAction);

			Debug.Assert(null != rEngine);
			Debug.Assert(null != m_ParticleEngine);

			m_strBoneName = myAction.bone;
			if (m_strBoneName != "")
			{
				m_rBone = Owner.AnimationContainer.Model.GetBone(m_strBoneName);
				Debug.Assert(null != m_rBone);
			}

			Direction = myAction.direction;
			StartOffset = myAction.StartOffset;
			Debug.Assert(myAction.emitter.Count == 1);
			if (!m_rTemplate.ReadSerializedObject(myAction.emitter[0], rEngine.Renderer))
			{
				return false;
			}

			return true;
		}

		#endregion //File IO
	}
}