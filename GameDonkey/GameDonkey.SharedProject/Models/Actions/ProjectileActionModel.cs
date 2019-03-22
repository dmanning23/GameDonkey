using FilenameBuddy;
using Microsoft.Xna.Framework;
using System;
using System.Xml;
using Vector2Extensions;
using XmlBuddy;

namespace GameDonkeyLib
{
	public class ProjectileActionModel : BaseActionModel, IHasFilenameActionModel, IDirectionalActionModel
	{
		#region Properties

		public override EActionType ActionType
		{
			get
			{
				return EActionType.Projectile;
			}
		}

		public Filename Filename { get; private set; }
		public Vector2 StartOffset { get; private set; }
		public float Scale { get; private set; }
		public DirectionActionModel Direction { get; private set; }

		#endregion //Properties

		#region Initialization

		public ProjectileActionModel()
		{
			Filename = new Filename();
			Direction = new DirectionActionModel();
			StartOffset = Vector2.Zero;
			Scale = 1f;
		}

		public ProjectileActionModel(ProjectileAction action) : base(action)
		{
			Filename = new Filename(action.FileName);
			StartOffset = action.StartOffset;
			Scale = action.Scale;
			Direction = new DirectionActionModel(action.Velocity);
		}

		public ProjectileActionModel(BaseAction action) : this(action as ProjectileAction)
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
				case "startoffset":
					{
						StartOffset = value.ToVector2();
					}
					break;
				case "scale":
					{
						Scale = Convert.ToSingle(value);
					}
					break;
				case "direction":
					{
						XmlFileBuddy.ReadChildNodes(node, Direction.ParseXmlNode);
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
			xmlWriter.WriteAttributeString("StartOffset", StartOffset.StringFromVector());
			xmlWriter.WriteAttributeString("Scale", Scale.ToString());
			Direction.WriteXmlNodes(xmlWriter);
		}

#endif

		#endregion //Methods
	}
}
