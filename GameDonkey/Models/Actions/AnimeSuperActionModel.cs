using Microsoft.Xna.Framework;
using System;
using System.Xml;
using Vector2Extensions;

namespace GameDonkeyLib
{
    public class AnimeSuperActionModel : BaseActionModel
    {
        #region Properties

        public override EActionType ActionType
        {
            get
            {
                return EActionType.AnimeSuper;
            }
        }

        public Color SuperColor { get; set; }
        public float FlashIn { get; set; }
        public float Hold { get; set; }
        public float FadeOut { get; set; }

        #endregion //Properties

        #region Initialization

        public AnimeSuperActionModel()
        {
            SuperColor = Color.White;
            FlashIn = 0.05f;
            Hold = 0.30f;
            FadeOut = 0.40f;
        }

        public AnimeSuperActionModel(AnimeSuperAction action) : base(action)
        {
            SuperColor = action.SuperColor;
            FlashIn = action.FlashIn;
            Hold = action.Hold;
            FadeOut = action.FadeOut;
        }

        public AnimeSuperActionModel(BaseAction action) : this(action as AnimeSuperAction)
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
                case "supercolor":
                    {
                        SuperColor = value.ToColor();
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
            xmlWriter.WriteAttributeString("SuperColor", SuperColor.StringFromColor());
            xmlWriter.WriteAttributeString("FlashIn", FlashIn.ToString());
            xmlWriter.WriteAttributeString("Hold", Hold.ToString());
            xmlWriter.WriteAttributeString("FadeOut", FadeOut.ToString());
        }

        #endregion //Methods
    }
}
