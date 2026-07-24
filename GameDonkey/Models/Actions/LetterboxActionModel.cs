using System;
using System.Xml;

namespace GameDonkeyLib
{
    public class LetterboxActionModel : BaseActionModel
    {
        #region Properties

        public override EActionType ActionType
        {
            get
            {
                return EActionType.Letterbox;
            }
        }

        public float BarHeight { get; set; }
        public float SlideIn { get; set; }
        public float Hold { get; set; }
        public float SlideOut { get; set; }

        #endregion //Properties

        #region Initialization

        public LetterboxActionModel()
        {
            BarHeight = 0.10f;
            SlideIn = 0.15f;
            Hold = 1.00f;
            SlideOut = 0.15f;
        }

        public LetterboxActionModel(LetterboxAction action) : base(action)
        {
            BarHeight = action.BarHeight;
            SlideIn = action.SlideIn;
            Hold = action.Hold;
            SlideOut = action.SlideOut;
        }

        public LetterboxActionModel(BaseAction action) : this(action as LetterboxAction)
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
                case "barheight":
                    {
                        BarHeight = Convert.ToSingle(value);
                    }
                    break;
                case "slidein":
                    {
                        SlideIn = Convert.ToSingle(value);
                    }
                    break;
                case "hold":
                    {
                        Hold = Convert.ToSingle(value);
                    }
                    break;
                case "slideout":
                    {
                        SlideOut = Convert.ToSingle(value);
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
            xmlWriter.WriteAttributeString("BarHeight", BarHeight.ToString());
            xmlWriter.WriteAttributeString("SlideIn", SlideIn.ToString());
            xmlWriter.WriteAttributeString("Hold", Hold.ToString());
            xmlWriter.WriteAttributeString("SlideOut", SlideOut.ToString());
        }

        #endregion //Methods
    }
}
