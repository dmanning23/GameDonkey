using FilenameBuddy;
using System;
using System.Collections.Generic;
using System.Xml;
using XmlBuddy;

namespace GameDonkeyLib
{
    public class TemplateActionModel : BaseActionModel, IHasStateActionsListModel, IHasFilenameActionModel
    {
        #region Properties

        public override EActionType ActionType
        {
            get
            {
                return EActionType.TemplateAction;
            }
        }

        public TemplateContainerModel TemplateContainer { get; set; }


        public StateActionsListModel ActionModels => TemplateContainer.StatesActions;

        public Filename Filename => TemplateContainer.Filename;

        #endregion //Properties

        #region Initialization

        public TemplateActionModel()
        {
        }

        public TemplateActionModel(TemplateAction action) : base(action)
        {
            TemplateContainer = new TemplateContainerModel(action.FileName, action.StateActionsList);
        }

        public TemplateActionModel(BaseAction action) : this(action as TemplateAction)
        {
        }

        #endregion //Initialization

        #region Methods

        public override void ParseXmlNode(XmlNode node)
        {
            //what is in this node?
            var name = node.Name;
            var value = node.InnerText;

            switch (name.ToLower())
            {
                case "filename":
                    {
                        var filename = new Filename();
                        filename.SetRelFilename(value);
                        TemplateContainer = new TemplateContainerModel(filename);
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
            xmlWriter.WriteAttributeString("Filename", Filename.GetRelFilename());
            TemplateContainer.WriteXml();
        }

        #endregion //Methods
    }
}
