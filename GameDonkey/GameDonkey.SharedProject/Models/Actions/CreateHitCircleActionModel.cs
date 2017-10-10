using MathNet.Numerics;
using Microsoft.Xna.Framework;
using System;
using System.Xml;
using Vector2Extensions;

namespace GameDonkeyLib
{
	public class CreateHitCircleActionModel : CreateAttackActionModel
	{
		#region Properties

		public override EActionType ActionType
		{
			get
			{
				return EActionType.CreateHitCircle;
			}
		}

		public float Radius { get; private set; }
		public Vector2 StartOffset { get; private set; }
		public Vector2 Velocity { get; private set; }

		#endregion //Properties

		#region Initialization

		public CreateHitCircleActionModel()
		{
			StartOffset = Vector2.Zero;
			Velocity = Vector2.Zero;
		}

		public CreateHitCircleActionModel(CreateHitCircleAction action) : base(action)
		{
			Radius = action.Radius;
			StartOffset = action.StartOffset;
			Velocity = action.Velocity;
		}

		public CreateHitCircleActionModel(BaseAction action) : this(action as CreateHitCircleAction)
		{
		}

		#endregion //Initialization

		#region Methods

		public override bool Compare(BaseActionModel inst)
		{
			var stateAction = inst as CreateHitCircleActionModel;
			if (null == stateAction)
			{
				return false;
			}

			if (!Radius.AlmostEqual(stateAction.Radius))
			{
				return false;
			}

			if (!StartOffset.AlmostEqual(stateAction.StartOffset))
			{
				return false;
			}

			if (!Velocity.AlmostEqual(stateAction.Velocity))
			{
				return false;
			}

			return base.Compare(inst);
		}

		public override void ParseXmlNode(XmlNode node)
		{
			//what is in this node?
			var name = node.Name;
			var value = node.InnerText;

			switch (name)
			{
				case "Radius":
					{
						Radius = Convert.ToSingle(value);
					}
					break;
				case "StartOffset":
					{
						StartOffset = value.ToVector2();
					}
					break;
				case "Velocity":
					{
						Velocity = value.ToVector2();
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
			xmlWriter.WriteAttributeString("Radius", Radius.ToString());
			xmlWriter.WriteAttributeString("StartOffset", StartOffset.StringFromVector());
			xmlWriter.WriteAttributeString("Velocity", Velocity.StringFromVector());
			base.WriteActionXml(xmlWriter);
		}

#endif

		#endregion //Methods
	}
}
