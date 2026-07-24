using Microsoft.Xna.Framework;
using System;
using System.Xml;
using Vector2Extensions;

namespace GameDonkeyLib
{
    public class FreezeFrameActionModel : BaseActionModel
    {
        #region Properties

        public override EActionType ActionType
        {
            get
            {
                return EActionType.FreezeFrame;
            }
        }

        public Color TintColor { get; set; }
        public float FlashIn { get; set; }
        public float Hold { get; set; }
        public float FadeOut { get; set; }

        #endregion //Properties

        #region Initialization

        public FreezeFrameActionModel()
        {
            TintColor = Color.White;
            FlashIn = 0.10f;
            Hold = 0.40f;
            FadeOut = 0.30f;
        }

        public FreezeFrameActionModel(FreezeFrameAction action) : base(action)
        {
            TintColor = action.TintColor;
            FlashIn = action.FlashIn;
            Hold = action.Hold;
            FadeOut = action.FadeOut;
        }

        public FreezeFrameActionModel(BaseAction action) : this(action as FreezeFrameAction)
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
                case "flashin":
                    {
                        FlashIn = Convert.ToSingle(value);
                    }
                    break;
                case "hold":
                    {
                        Hold = Convert.ToSingle(value);
                    }
                    break;
                case "fadeout":
                    {
                        FadeOut = Convert.ToSingle(value);
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
            xmlWriter.WriteAttributeString("FlashIn", FlashIn.ToString());
            xmlWriter.WriteAttributeString("Hold", Hold.ToString());
            xmlWriter.WriteAttributeString("FadeOut", FadeOut.ToString());
        }

        #endregion //Methods
    }
}
