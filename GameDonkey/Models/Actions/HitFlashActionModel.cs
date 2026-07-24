using Microsoft.Xna.Framework;
using ScreenFXBuddy;
using System;
using System.Xml;
using Vector2Extensions;

namespace GameDonkeyLib
{
    public class HitFlashActionModel : BaseActionModel
    {
        #region Properties

        public override EActionType ActionType
        {
            get
            {
                return EActionType.HitFlash;
            }
        }

        public Color BlendColor { get; set; }
        public FadeMode Mode { get; set; }
        public FadeCurve Curve { get; set; }
        public EffectBlendMode BlendMode { get; set; }
        public float Duration { get; set; }

        #endregion //Properties

        #region Initialization

        public HitFlashActionModel()
        {
            BlendColor = Color.White;
            Mode = FadeMode.FadeOut;
            Curve = FadeCurve.Linear;
            BlendMode = EffectBlendMode.LinearDodge;
            Duration = 1f;
        }

        public HitFlashActionModel(HitFlashAction action) : base(action)
        {
            BlendColor = action.BlendColor;
            Mode = action.Mode;
            Curve = action.Curve;
            BlendMode = action.BlendMode;
            Duration = action.Duration;
        }

        public HitFlashActionModel(BaseAction action) : this(action as HitFlashAction)
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
                case "blendcolor":
                    {
                        BlendColor = value.ToColor();
                    }
                    break;
                case "mode":
                    {
                        Mode = (FadeMode)Enum.Parse(typeof(FadeMode), value);
                    }
                    break;
                case "curve":
                    {
                        Curve = (FadeCurve)Enum.Parse(typeof(FadeCurve), value);
                    }
                    break;
                case "blendmode":
                    {
                        BlendMode = (EffectBlendMode)Enum.Parse(typeof(EffectBlendMode), value);
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
            xmlWriter.WriteAttributeString("BlendColor", BlendColor.StringFromColor());
            xmlWriter.WriteAttributeString("Mode", Mode.ToString());
            xmlWriter.WriteAttributeString("Curve", Curve.ToString());
            xmlWriter.WriteAttributeString("BlendMode", BlendMode.ToString());
            xmlWriter.WriteAttributeString("Duration", Duration.ToString());
        }

        #endregion //Methods
    }
}
