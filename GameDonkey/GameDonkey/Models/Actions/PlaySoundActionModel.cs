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

		public Filename Filename { get; set; }

		#endregion //Properties

		#region Initialization

		public PlaySoundActionModel()
		{
			Filename = new Filename();
		}

		public PlaySoundActionModel(PlaySoundAction action) : base(action)
		{
			Filename = new Filename(action.SoundCueName);
		}

		public PlaySoundActionModel(BaseAction action) : this(action as PlaySoundAction)
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
				case "filename":
					{
						if (!string.IsNullOrEmpty(value))
						{
							Filename.SetRelFilename(value);
						}
					}
					break;
				default:
					{
						base.ParseXmlNode(node);
					}
					break;
			}
		}

		protected override void WriteActionXml(XmlTextWriter xmlWriter)
		{
			xmlWriter.WriteAttributeString("Filename", Filename.GetRelFilename());
		}

		#endregion //Methods
	}
}
