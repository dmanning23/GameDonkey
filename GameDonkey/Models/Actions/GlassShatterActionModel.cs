using System;
using System.Xml;

namespace GameDonkeyLib
{
    public class GlassShatterActionModel : BaseActionModel
    {
        #region Properties

        public override EActionType ActionType
        {
            get
            {
                return EActionType.GlassShatter;
            }
        }

        public float Strength { get; set; }
        public int NumCells { get; set; }
        public float Duration { get; set; }

        #endregion //Properties

        #region Initialization

        public GlassShatterActionModel()
        {
            Strength = 0.04f;
            NumCells = 20;
            Duration = 0.8f;
        }

        public GlassShatterActionModel(GlassShatterAction action) : base(action)
        {
            Strength = action.Strength;
            NumCells = action.NumCells;
            Duration = action.Duration;
        }

        public GlassShatterActionModel(BaseAction action) : this(action as GlassShatterAction)
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
                case "numcells":
                    {
                        NumCells = Convert.ToInt32(value);
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
            xmlWriter.WriteAttributeString("NumCells", NumCells.ToString());
            xmlWriter.WriteAttributeString("Duration", Duration.ToString());
        }

        #endregion //Methods
    }
}
