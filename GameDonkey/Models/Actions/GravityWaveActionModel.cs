using System;
using System.Xml;

namespace GameDonkeyLib
{
    public class GravityWaveActionModel : BaseActionModel
    {
        #region Properties

        public override EActionType ActionType
        {
            get
            {
                return EActionType.GravityWave;
            }
        }

        public float Strength { get; set; }
        public float StartHeight { get; set; }
        public float EndHeight { get; set; }
        public float Speed { get; set; }
        public float Duration { get; set; }

        #endregion //Properties

        #region Initialization

        public GravityWaveActionModel()
        {
            Strength = 0.04f;
            StartHeight = 0.05f;
            EndHeight = 0.25f;
            Speed = 0.5f;
            Duration = 1.5f;
        }

        public GravityWaveActionModel(GravityWaveAction action) : base(action)
        {
            Strength = action.Strength;
            StartHeight = action.StartHeight;
            EndHeight = action.EndHeight;
            Speed = action.Speed;
            Duration = action.Duration;
        }

        public GravityWaveActionModel(BaseAction action) : this(action as GravityWaveAction)
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
                case "startheight":
                    {
                        StartHeight = Convert.ToSingle(value);
                    }
                    break;
                case "endheight":
                    {
                        EndHeight = Convert.ToSingle(value);
                    }
                    break;
                case "speed":
                    {
                        Speed = Convert.ToSingle(value);
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
            xmlWriter.WriteAttributeString("StartHeight", StartHeight.ToString());
            xmlWriter.WriteAttributeString("EndHeight", EndHeight.ToString());
            xmlWriter.WriteAttributeString("Speed", Speed.ToString());
            xmlWriter.WriteAttributeString("Duration", Duration.ToString());
        }

        #endregion //Methods
    }
}
