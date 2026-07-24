using Microsoft.Xna.Framework;
using ScreenFXBuddy;
using ScreenFXBuddy.Effects;
using System;
using System.Xml;
using Vector2Extensions;

namespace GameDonkeyLib
{
    public class SpeedLinesActionModel : BaseActionModel
    {
        #region Properties

        public override EActionType ActionType
        {
            get
            {
                return EActionType.SpeedLines;
            }
        }

        public Color SpeedLinesColor { get; set; }
        public SpeedLinesMode LinesMode { get; set; }
        public FadeMode SpeedLinesFadeMode { get; set; }
        public FadeCurve SpeedLinesFadeCurve { get; set; }
        public int LineCount { get; set; }
        public float MaxRadius { get; set; }
        public float Duration { get; set; }

        #endregion //Properties

        #region Initialization

        public SpeedLinesActionModel()
        {
            SpeedLinesColor = Color.White;
            LinesMode = SpeedLinesMode.Expand;
            SpeedLinesFadeMode = FadeMode.FadeOut;
            SpeedLinesFadeCurve = FadeCurve.Logarithmic;
            LineCount = 24;
            MaxRadius = 1.0f;
            Duration = 1f;
        }

        public SpeedLinesActionModel(SpeedLinesAction action) : base(action)
        {
            SpeedLinesColor = action.SpeedLinesColor;
            LinesMode = action.LinesMode;
            SpeedLinesFadeMode = action.SpeedLinesFadeMode;
            SpeedLinesFadeCurve = action.SpeedLinesFadeCurve;
            LineCount = action.LineCount;
            MaxRadius = action.MaxRadius;
            Duration = action.Duration;
        }

        public SpeedLinesActionModel(BaseAction action) : this(action as SpeedLinesAction)
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
                case "speedlinescolor":
                    {
                        SpeedLinesColor = value.ToColor();
                    }
                    break;
                case "linesmode":
                    {
                        LinesMode = (SpeedLinesMode)Enum.Parse(typeof(SpeedLinesMode), value);
                    }
                    break;
                case "speedlinesfademode":
                    {
                        SpeedLinesFadeMode = (FadeMode)Enum.Parse(typeof(FadeMode), value);
                    }
                    break;
                case "speedlinesfadecurve":
                    {
                        SpeedLinesFadeCurve = (FadeCurve)Enum.Parse(typeof(FadeCurve), value);
                    }
                    break;
                case "linecount":
                    {
                        LineCount = Convert.ToInt32(value);
                    }
                    break;
                case "maxradius":
                    {
                        MaxRadius = Convert.ToSingle(value);
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
            xmlWriter.WriteAttributeString("SpeedLinesColor", SpeedLinesColor.StringFromColor());
            xmlWriter.WriteAttributeString("LinesMode", LinesMode.ToString());
            xmlWriter.WriteAttributeString("SpeedLinesFadeMode", SpeedLinesFadeMode.ToString());
            xmlWriter.WriteAttributeString("SpeedLinesFadeCurve", SpeedLinesFadeCurve.ToString());
            xmlWriter.WriteAttributeString("LineCount", LineCount.ToString());
            xmlWriter.WriteAttributeString("MaxRadius", MaxRadius.ToString());
            xmlWriter.WriteAttributeString("Duration", Duration.ToString());
        }

        #endregion //Methods
    }
}
