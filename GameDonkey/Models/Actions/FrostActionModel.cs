using Microsoft.Xna.Framework;
using System;
using System.Xml;
using Vector2Extensions;

namespace GameDonkeyLib
{
    public class FrostActionModel : BaseActionModel
    {
        #region Properties

        public override EActionType ActionType
        {
            get
            {
                return EActionType.Frost;
            }
        }

        public Color TintColor { get; set; }
        public float Radius { get; set; }
        public float Duration { get; set; }

        #endregion //Properties

        #region Initialization

        public FrostActionModel()
        {
            TintColor = Color.White;
            Radius = 0.25f;
            Duration = 1.50f;
        }

        public FrostActionModel(FrostAction action) : base(action)
        {
            TintColor = action.TintColor;
            Radius = action.Radius;
            Duration = action.Duration;
        }

        public FrostActionModel(BaseAction action) : this(action as FrostAction)
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
                case "tintcolor":
                    {
                        TintColor = value.ToColor();
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
            xmlWriter.WriteAttributeString("TintColor", TintColor.StringFromColor());
            xmlWriter.WriteAttributeString("Radius", Radius.ToString());
            xmlWriter.WriteAttributeString("Duration", Duration.ToString());
        }

        #endregion //Methods
    }
}
