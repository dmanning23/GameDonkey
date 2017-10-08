using FilenameBuddy;
using System.Xml;

namespace GameDonkeyLib
{
	public class PlaySoundActionModel : BaseActionModel, IHasFilenameActionModel
	{
		#region Properties

		public override EActionType ActionType
		{
			get
			{
				return EActionType.PlaySound;
			}
		}

		public Filename Filename { get; private set; }

		#endregion //Properties

		#region Methods

		public PlaySoundActionModel()
		{
			Filename = new Filename();
		}

		public override bool Compare(BaseActionModel inst)
		{
			if (!base.Compare(inst))
			{
				return false;
			}

			var stateAction = inst as PlaySoundActionModel;
			if (null == stateAction)
			{
				return false;
			}

			if (!Filename.Compare(stateAction.Filename))
			{
				return false;
			}

			return true;
		}

		public override void ParseXmlNode(XmlNode node)
		{
			//what is in this node?
			var name = node.Name;
			var value = node.InnerText;

			switch (name)
			{
				case "Filename":
					{
						Filename.SetRelFilename(value);
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
			xmlWriter.WriteAttributeString("Filename", Filename.GetRelFilename());
		}

#endif

		#endregion //Methods
	}
}
