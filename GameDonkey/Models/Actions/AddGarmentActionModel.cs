using FilenameBuddy;
using System.Xml;

namespace GameDonkeyLib
{
	public class AddGarmentActionModel : BaseActionModel, ITimedActionModel, IHasFilenameActionModel
	{
		#region Properties

		public override EActionType ActionType
		{
			get
			{
				return EActionType.AddGarment;
			}
		}

		public Filename Filename { get; set; }
		public TimedActionModel TimeDelta { get; set; }

		#endregion //Properties

		#region Initialization

		public AddGarmentActionModel()
		{
			Filename = new Filename();
			TimeDelta = new TimedActionModel();
		}

		public AddGarmentActionModel(AddGarmentAction action) : base(action)
		{
			Filename = new Filename(action.Filename);
			TimeDelta = new TimedActionModel(action);
		}

		public AddGarmentActionModel(BaseAction action) : this(action as AddGarmentAction)
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
						Filename.SetRelFilename(value);
					}
					break;
				case "garmentfile":
					{
						Filename.SetRelFilename(value);
					}
					break;
				case "timedelta":
					{
						TimeDelta.ParseXmlNode(node);
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
			TimeDelta.WriteXmlNodes(xmlWriter);
		}

		#endregion //Methods
	}
}
