using Microsoft.Xna.Framework;
using System;
using System.Xml;
using Vector2Extensions;

namespace GameDonkeyLib
{
    public class SmokeActionModel : BaseActionModel
    {
        #region Properties

        public override EActionType ActionType
        {
            get
            {
                return EActionType.Smoke;
            }
        }

        public Color SmokeColor { get; set; }
        public float Radius { get; set; }
        public float Duration { get; set; }

        #endregion //Properties

        #region Initialization

        public SmokeActionModel()
        {
            SmokeColor = Color.White;
            Radius = 0.15f;
            Duration = 2.0f;
        }

        public SmokeActionModel(SmokeAction action) : base(action)
        {
            SmokeColor = action.SmokeColor;
            Radius = action.Radius;
            Duration = action.Duration;
        }

        public SmokeActionModel(BaseAction action) : this(action as SmokeAction)
        {
        }

        #endregion //Initialization

        #region Methods

        public override void ParseXmlNode(XmlNode node)
        {
            var name = node.Name;
            var value = node.InnerText;

            switch (name.ToLower())
            {
                case "smokecolor":
                    {
                        SmokeColor = value.ToColor();
                    }
                    break;
                case "radius":
                    {
                        Radius = Convert.ToSingle(value);
                    }
                    break;
                case "duration":
                    {
                        Duration = Convert.ToSingle(value);
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
            xmlWriter.WriteAttributeString("SmokeColor", SmokeColor.StringFromColor());
            xmlWriter.WriteAttributeString("Radius", Radius.ToString());
            xmlWriter.WriteAttributeString("Duration", Duration.ToString());
        }

        #endregion //Methods
    }
}
