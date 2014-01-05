using AnimationLib;
using StateMachineBuddy;
using System;
using System.Diagnostics;
using System.Xml;

namespace GameDonkey
{
	public class PlayAnimationAction : IBaseAction
	{
		#region Members

		/// <summary>
		/// Name of the animation to play
		/// </summary>
		protected string m_strAnimationName;

		/// <summary>
		/// the index of the animation to play, set at load time
		/// </summary>
		protected int m_iAnimationIndex;

		/// <summary>
		/// which playback mode to use
		/// </summary>
		protected EPlayback m_ePlaybackMode;

		#endregion //Members

		#region Properties

		public string AnimationName
		{
			get { return m_strAnimationName; }
			set
			{
				m_strAnimationName = value;
				m_iAnimationIndex = Owner.AnimationContainer.FindAnimationIndex(m_strAnimationName);
				Debug.Assert(0 <= m_iAnimationIndex);
				Debug.Assert(m_iAnimationIndex < Owner.AnimationContainer.Animations.Count);
			}
		}

		public int AnimationIndex
		{
			get { return m_iAnimationIndex; }
		}

		public EPlayback PlaybackMode
		{
			get { return m_ePlaybackMode; }
			set { m_ePlaybackMode = value; }
		}

		#endregion //Properties

		#region Methods

		/// <summary>
		/// Standard constructor
		/// </summary>
		public PlayAnimationAction(BaseObject rOwner) : base(rOwner)
		{
			ActionType = EActionType.PlayAnimation;
			m_strAnimationName = "";
			m_iAnimationIndex = 0;
			m_ePlaybackMode = EPlayback.Forwards;
		}

		/// <summary>
		/// execute this action (overridden in all child classes)
		/// </summary>
		/// <returns>bool: whether or not to continue running actions after this dude runs</returns>
		public override bool Execute()
		{
			Debug.Assert(null != Owner);
			Debug.Assert(null != Owner.AnimationContainer);
			Debug.Assert(null != Owner.AnimationContainer.Model);
			Debug.Assert(!AlreadyRun);

			Owner.AnimationContainer.SetAnimation(m_iAnimationIndex, m_ePlaybackMode);

			return base.Execute();
		}

		public override bool Compare(IBaseAction rInst)
		{
			PlayAnimationAction myAction = (PlayAnimationAction)rInst;

			Debug.Assert((ActionType == myAction.ActionType) &&
				(Time == myAction.Time) &&
				(m_strAnimationName == myAction.m_strAnimationName) &&
				(m_iAnimationIndex == myAction.m_iAnimationIndex) &&
				(m_ePlaybackMode == myAction.m_ePlaybackMode));

			return ((ActionType == myAction.ActionType) &&
				(Time == myAction.Time) &&
				(m_strAnimationName == myAction.m_strAnimationName) &&
				(m_iAnimationIndex == myAction.m_iAnimationIndex) &&
				(m_ePlaybackMode == myAction.m_ePlaybackMode));
		}

		#endregion //Methods

		#region File IO

		/// <summary>
		/// Read from an xml file
		/// </summary>
		/// <param name="rXMLNode">the xml node to read from</param>
		/// <returns></returns>
		public override bool ReadXml(XmlNode rXMLNode, IGameDonkey rEngine, StateMachine rStateMachine)
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
					if (ActionType != StateActionFactory.XMLTypeToType(strValue))
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
					else if (strName == "animation")
					{
						AnimationName = strValue;

						if (m_iAnimationIndex < 0)
						{
							Debug.Assert(0 <= m_iAnimationIndex);
							return false;
						}
						if (m_iAnimationIndex >= Owner.AnimationContainer.Animations.Count)
						{
							Debug.Assert(m_iAnimationIndex < Owner.AnimationContainer.Animations.Count);
							return false;
						}
					}
					else if (strName == "playback")
					{
						m_ePlaybackMode = (EPlayback)Enum.Parse(typeof(EPlayback), strValue);
					}
					else
					{
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
			rXMLFile.WriteStartElement("animation");
			rXMLFile.WriteString(Owner.AnimationContainer.Animations[m_iAnimationIndex].Name);
			rXMLFile.WriteEndElement();

			rXMLFile.WriteStartElement("playback");
			rXMLFile.WriteString(m_ePlaybackMode.ToString());
			rXMLFile.WriteEndElement();
		}

		/// <summary>
		/// Read from a serialized file
		/// </summary>
		/// <param name="myAction">the xml item to read the action from</param>
		public bool ReadSerialized(SPFSettings.PlayAnimationActionXML myAction)
		{
			Debug.Assert(myAction.type == ActionType.ToString());
			ReadSerializedBase(myAction);

			//read in teh animation
			AnimationName = myAction.animation;
			Debug.Assert(0 <= m_iAnimationIndex);
			Debug.Assert(m_iAnimationIndex < Owner.AnimationContainer.Animations.Count);

			//read in teh playback style
			m_ePlaybackMode = (EPlayback)Enum.Parse(typeof(EPlayback), myAction.playback);

			return true;
		}


		#endregion //File IO
	}
}