using AnimationLib;
using System;
using System.Xml;

namespace GameDonkeyLib
{
	public class PlayAnimationActionModel : BaseActionModel
	{
		#region Properties

		public override EActionType ActionType
		{
			get
			{
				return EActionType.PlayAnimation;
			}
		}

		public string Animation { get; set; }
		public EPlayback Playback { get; set; }

		#endregion //Properties

		#region Initialization

		public PlayAnimationActionModel()
		{
		}

		public PlayAnimationActionModel(PlayAnimationAction action) : base(action)
		{
			Animation = action.AnimationName;
			Playback = action.PlaybackMode;
		}

		public PlayAnimationActionModel(BaseAction action) : this(action as PlayAnimationAction)
		{
		}

		#endregion //Initialization

		#region Methods

		public override void ParseXmlNode(XmlNode node)
		{
			//what is in this node?
			var name = node.Name;
			var value = node.InnerText;

			switch (name.ToLower())
			{
				case "animation":
					{
						Animation = value;
					}
					break;
				case "playback":
					{
						Playback = (EPlayback)Enum.Parse(typeof(EPlayback), value);
					}
					break;
				default:
					{
						base.ParseXmlNode(node);
					}
					break;
			}
		}

#if !WINDOWS_UWP

		protected override void WriteActionXml(XmlTextWriter xmlWriter)
		{
			xmlWriter.WriteAttributeString("Animation", Animation);
			xmlWriter.WriteAttributeString("Playback", Playback.ToString());
		}

#endif

		#endregion //Methods
	}
}
