using System;
using System.Xml;

namespace GameDonkeyLib
{
    public class ZoomBlurActionModel : BaseActionModel
    {
        #region Properties

        public override EActionType ActionType
        {
            get
            {
                return EActionType.ZoomBlur;
            }
        }

        public float Strength { get; set; }
        public float Radius { get; set; }
        public float Duration { get; set; }

        #endregion //Properties

        #region Initialization

        public ZoomBlurActionModel()
        {
            Strength = 0.05f;
            Radius = 1.0f;
            Duration = 0.4f;
        }

        public ZoomBlurActionModel(ZoomBlurAction action) : base(action)
        {
            Strength = action.Strength;
            Radius = action.Radius;
            Duration = action.Duration;
        }

        public ZoomBlurActionModel(BaseAction action) : this(action as ZoomBlurAction)
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
                case "strength":
                    {
                        Strength = Convert.ToSingle(value);
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
            xmlWriter.WriteAttributeString("Strength", Strength.ToString());
            xmlWriter.WriteAttributeString("Radius", Radius.ToString());
            xmlWriter.WriteAttributeString("Duration", Duration.ToString());
        }

        #endregion //Methods
    }
}
