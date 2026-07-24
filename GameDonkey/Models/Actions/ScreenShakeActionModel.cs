using System;
using System.Xml;

namespace GameDonkeyLib
{
    public class ScreenShakeActionModel : BaseActionModel
    {
        #region Properties

        public override EActionType ActionType
        {
            get
            {
                return EActionType.ScreenShake;
            }
        }

        public float Length { get; set; }
        public float Delta { get; set; }
        public float Amount { get; set; }

        #endregion //Properties

        #region Initialization

        public ScreenShakeActionModel()
        {
            Length = 1f;
            Delta = 0.1f;
            Amount = 0.1f;
        }

        public ScreenShakeActionModel(ScreenShakeAction action) : base(action)
        {
            Length = action.Length;
            Delta = action.Delta;
            Amount = action.Amount;
        }

        public ScreenShakeActionModel(BaseAction action) : this(action as ScreenShakeAction)
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
                case "length":
                    {
                        Length = Convert.ToSingle(value);
                    }
                    break;
                case "delta":
                    {
                        Delta = Convert.ToSingle(value);
                    }
                    break;
                case "amount":
                    {
                        Amount = Convert.ToSingle(value);
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
            xmlWriter.WriteAttributeString("Length", Length.ToString());
            xmlWriter.WriteAttributeString("Delta", Delta.ToString());
            xmlWriter.WriteAttributeString("Amount", Amount.ToString());
        }

        #endregion //Methods
    }
}
