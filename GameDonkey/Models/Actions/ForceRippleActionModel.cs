using System;
using System.Xml;

namespace GameDonkeyLib
{
    public class ForceRippleActionModel : BaseActionModel
    {
        #region Properties

        public override EActionType ActionType
        {
            get
            {
                return EActionType.ForceRipple;
            }
        }

        public float Strength { get; set; }
        public float Speed { get; set; }
        public float Size { get; set; }
        public float Duration { get; set; }

        #endregion //Properties

        #region Initialization

        public ForceRippleActionModel()
        {
            Strength = 0.05f;
            Speed = 0.4f;
            Size = 0.08f;
            Duration = 2f;
        }

        public ForceRippleActionModel(ForceRippleAction action) : base(action)
        {
            Strength = action.Strength;
            Speed = action.Speed;
            Size = action.Size;
            Duration = action.Duration;
        }

        public ForceRippleActionModel(BaseAction action) : this(action as ForceRippleAction)
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
                case "speed":
                    {
                        Speed = Convert.ToSingle(value);
                    }
                    break;
                case "size":
                    {
                        Size = Convert.ToSingle(value);
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
            xmlWriter.WriteAttributeString("Speed", Speed.ToString());
            xmlWriter.WriteAttributeString("Size", Size.ToString());
            xmlWriter.WriteAttributeString("Duration", Duration.ToString());
        }

        #endregion //Methods
    }
}
