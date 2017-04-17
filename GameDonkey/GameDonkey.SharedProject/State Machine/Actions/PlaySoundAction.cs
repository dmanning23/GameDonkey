using Microsoft.Xna.Framework.Audio;
using System;
using System.Diagnostics;
using System.Xml;
using FilenameBuddy;
using Microsoft.Xna.Framework.Content;

namespace GameDonkeyLib
{
	public class PlaySoundAction : BaseAction
	{
		#region Members

		/// <summary>
		/// the filename of the sound file to use
		/// </summary>		/// <value>The name of the sound cue.</value>
		public Filename SoundCueName { get; set; }

		/// <summary>
		/// Gets the sound.
		/// </summary>
		/// <value>The sound.</value>
		public SoundEffect Sound { get ; private set; }

		#endregion //Members

		#region Methods

		/// <summary>
		/// Standard constructor
		/// </summary>
		public PlaySoundAction(BaseObject rOwner, IGameDonkey rEngine) : base(rOwner)
		{
			Debug.Assert(null != rEngine);
			ActionType = EActionType.PlaySound;
		}

		/// <summary>
		/// execute this action (overridden in all child classes)
		/// </summary>
		/// <returns>bool: whether or not to continue running actions after this dude runs</returns>
		public override bool Execute()
		{
			Debug.Assert(null != Owner);
			Debug.Assert(!AlreadyRun);

			//execute sound action
			if (null != Sound)
			{
				Sound.Play();
			}

			return base.Execute();
		}

		public override bool Compare(BaseAction rInst)
		{
			PlaySoundAction myAction = (PlaySoundAction)rInst;
			
			Debug.Assert((ActionType == myAction.ActionType) &&
				(Time == myAction.Time) &&
				(SoundCueName.ToString() == myAction.SoundCueName.ToString()));
			
			return ((ActionType == myAction.ActionType) &&
				(Time == myAction.Time) &&
				(SoundCueName.ToString() == myAction.SoundCueName.ToString()));
		}

		#endregion //Methods

		#region File IO

		/// <summary>
		/// Read from an xml file
		/// </summary>
		/// <param name="rXMLNode">the xml node to read from</param>
		/// <returns></returns>
		public override bool ReadXml(XmlNode rXMLNode, IGameDonkey rEngine, SingleStateContainer stateContainer, ContentManager content)
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
					else if ((strName == "filename") && !string.IsNullOrEmpty(strValue))
					{
						SoundCueName = new Filename(strValue);
						Sound = rEngine.LoadSound(SoundCueName);
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

#if !WINDOWS_UWP
		/// <summary>
		/// overloaded in child classes to write out action specific stuff
		/// </summary>
		/// <param name="rXMLFile"></param>
		protected override void WriteActionXml(XmlTextWriter rXMLFile)
		{
			rXMLFile.WriteStartElement("filename");
			rXMLFile.WriteString(SoundCueName.GetRelFilename());
			rXMLFile.WriteEndElement();
		}
#endif

		#endregion //File IO
	}
}