using System;
using System.Xml;

namespace GameDonkeyLib
{
    public class HeatHazeActionModel : BaseActionModel
    {
        #region Properties

        public override EActionType ActionType
        {
            get
            {
                return EActionType.HeatHaze;
            }
        }

        public float Strength { get; set; }
        public float Radius { get; set; }
        public float Height { get; set; }
        public float Duration { get; set; }

        #endregion //Properties

        #region Initialization

        public HeatHazeActionModel()
        {
            Strength = 0.02f;
            Radius = 0.15f;
            Height = 0.40f;
            Duration = 3.0f;
        }

        public HeatHazeActionModel(HeatHazeAction action) : base(action)
        {
            Strength = action.Strength;
            Radius = action.Radius;
            Height = action.Height;
            Duration = action.Duration;
        }

        public HeatHazeActionModel(BaseAction action) : this(action as HeatHazeAction)
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
                case "height":
                    {
                        Height = Convert.ToSingle(value);
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
            xmlWriter.WriteAttributeString("Height", Height.ToString());
            xmlWriter.WriteAttributeString("Duration", Duration.ToString());
        }

        #endregion //Methods
    }
}
