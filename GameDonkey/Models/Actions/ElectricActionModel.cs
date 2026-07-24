using Microsoft.Xna.Framework;
using System;
using System.Xml;
using Vector2Extensions;

namespace GameDonkeyLib
{
    public class ElectricActionModel : BaseActionModel
    {
        #region Properties

        public override EActionType ActionType
        {
            get
            {
                return EActionType.Electric;
            }
        }

        public Color ElectricColor { get; set; }
        public float Radius { get; set; }
        public float Duration { get; set; }

        #endregion //Properties

        #region Initialization

        public ElectricActionModel()
        {
            ElectricColor = Color.White;
            Radius = 0.20f;
            Duration = 0.60f;
        }

        public ElectricActionModel(ElectricAction action) : base(action)
        {
            ElectricColor = action.ElectricColor;
            Radius = action.Radius;
            Duration = action.Duration;
        }

        public ElectricActionModel(BaseAction action) : this(action as ElectricAction)
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
                case "electriccolor":
                    {
                        ElectricColor = value.ToColor();
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
            xmlWriter.WriteAttributeString("ElectricColor", ElectricColor.StringFromColor());
            xmlWriter.WriteAttributeString("Radius", Radius.ToString());
            xmlWriter.WriteAttributeString("Duration", Duration.ToString());
        }

        #endregion //Methods
    }
}
