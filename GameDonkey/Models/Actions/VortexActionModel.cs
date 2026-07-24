using ScreenFXBuddy;
using System;
using System.Xml;

namespace GameDonkeyLib
{
    public class VortexActionModel : BaseActionModel
    {
        #region Properties

        public override EActionType ActionType
        {
            get
            {
                return EActionType.Vortex;
            }
        }

        public float Radius { get; set; }
        public float Speed { get; set; }
        public float SpinInTime { get; set; }
        public float SpinOutTime { get; set; }
        public FadeCurve VortexFadeCurve { get; set; }
        public bool Clockwise { get; set; }

        #endregion //Properties

        #region Initialization

        public VortexActionModel()
        {
            Radius = 0.25f;
            Speed = 1f;
            SpinInTime = 0.3f;
            SpinOutTime = 0.3f;
            VortexFadeCurve = FadeCurve.Linear;
            Clockwise = true;
        }

        public VortexActionModel(VortexAction action) : base(action)
        {
            Radius = action.Radius;
            Speed = action.Speed;
            SpinInTime = action.SpinInTime;
            SpinOutTime = action.SpinOutTime;
            VortexFadeCurve = action.VortexFadeCurve;
            Clockwise = action.Clockwise;
        }

        public VortexActionModel(BaseAction action) : this(action as VortexAction)
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
                case "radius":
                    {
                        Radius = Convert.ToSingle(value);
                    }
                    break;
                case "speed":
                    {
                        Speed = Convert.ToSingle(value);
                    }
                    break;
                case "spinintime":
                    {
                        SpinInTime = Convert.ToSingle(value);
                    }
                    break;
                case "spinouttime":
                    {
                        SpinOutTime = Convert.ToSingle(value);
                    }
                    break;
                case "vortexfadecurve":
                    {
                        VortexFadeCurve = (FadeCurve)Enum.Parse(typeof(FadeCurve), value);
                    }
                    break;
                case "clockwise":
                    {
                        Clockwise = Convert.ToBoolean(value);
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
            xmlWriter.WriteAttributeString("Radius", Radius.ToString());
            xmlWriter.WriteAttributeString("Speed", Speed.ToString());
            xmlWriter.WriteAttributeString("SpinInTime", SpinInTime.ToString());
            xmlWriter.WriteAttributeString("SpinOutTime", SpinOutTime.ToString());
            xmlWriter.WriteAttributeString("VortexFadeCurve", VortexFadeCurve.ToString());
            xmlWriter.WriteAttributeString("Clockwise", Clockwise.ToString());
        }

        #endregion //Methods
    }
}
