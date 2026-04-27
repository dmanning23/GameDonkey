using AnimationLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using ParticleBuddy;
using RenderBuddy;
using System;

namespace GameDonkeyLib
{
    public class PointLightAction : BaseAction
    {
        #region Properties
        private string _boneName;
        public string BoneName
        {
            get { return _boneName; }
            set
            {
                _boneName = value;
                if (String.IsNullOrEmpty(_boneName) || null == Owner)
                {
                    Bone = null;
                }
                else
                {
                    Bone = Owner.AnimationContainer.Skeleton.RootBone.GetBone(_boneName);
                }
            }
        }
        public Bone Bone { get; private set; }
        public Vector3 StartOffset { get; set; }

        private IRenderer Renderer { get; set; }

        public Color LightColor { get; set; }

        public float AttackTimeDelta { get; set; }
        public float SustainTimeDelta { get; set; }
        public float DelayTimeDelta { get; set; }

        public float FlareTimeDelta { get; set; }
        public float MinBrightness { get; set; }
        public float MaxBrightness { get; set; }

        #endregion //Properties

        #region Initialization

        public PointLightAction(BaseObject owner) :
            base(owner, EActionType.PointLight)
        {
            BoneName = "";
            StartOffset = new Vector3(0f, 0f, 20f);
            LightColor = Color.White;
            AttackTimeDelta = 1f;
            SustainTimeDelta = 1f;
            DelayTimeDelta = 1f;
            FlareTimeDelta = 0.05f;
            MinBrightness = 100f;
            MaxBrightness = 100f;
        }

        public PointLightAction(BaseObject owner, PointLightActionModel actionModel) :
            base(owner, actionModel)
        {
            BoneName = actionModel.Bone;
            StartOffset = actionModel.StartOffset;
            LightColor = actionModel.LightColor;
            AttackTimeDelta = actionModel.AttackTimeDelta;
            SustainTimeDelta = actionModel.SustainTimeDelta;
            DelayTimeDelta = actionModel.DelayTimeDelta;
            FlareTimeDelta = actionModel.FlareTimeDelta;
            MinBrightness = actionModel.MinBrightness;
            MaxBrightness = actionModel.MaxBrightness;
        }

        public PointLightAction(BaseObject owner, BaseActionModel actionModel) :
            this(owner, actionModel as PointLightActionModel)
        {
        }

        public override void LoadContent(IGameDonkey engine, ContentManager content)
        {
            Renderer = engine.Renderer;
        }

        #endregion //Initialization

        #region Methods
        public override bool Execute(float currentTime)
        {
            var light = new FlarePointLight(StartPosition(), LightColor, FlareTimeDelta, AttackTimeDelta, SustainTimeDelta, DelayTimeDelta, MinBrightness, MaxBrightness, GetPosDelegate());
            Renderer.PointLights.Add(light);
            Owner.Lights.Add(light);

            return base.Execute(currentTime);
        }

        private Vector3 StartPosition()
        {
            return new Vector3(Owner.Position.X + (Owner.Flip ? (-1f * StartOffset.X) : StartOffset.X),
                Owner.Position.Y + StartOffset.Y,
                StartOffset.Z);
        }

        private Position3Delegate GetPosDelegate()
        {
            if (null != Bone)
            {
                return BonePosition;
            }
            else
            {
                return null;
            }
        }

        private Vector3 BonePosition()
        {
            var bonePos = Bone.GetPosition();
            return new Vector3(bonePos.X + (Owner.Flip ? (-1f * StartOffset.X) : StartOffset.X),
                bonePos.Y + StartOffset.Y,
                StartOffset.Z);
        }

        public void UpdateLightColor(Color color)
        {
            LightColor = new Color(color.R, color.G, color.B, LightColor.A);
        }

        #endregion //Methods
    }
}