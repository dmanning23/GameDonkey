using GameDonkeyLib;
using GameTimer;
using Microsoft.Xna.Framework;
using NUnit.Framework;
using Shouldly;

namespace GameDonkey.Tests
{
    [TestFixture]
    public class ActionDirectionTests
    {
        [Test]
        public void Velocity_Setter_ComputesVelocityLength()
        {
            var dir = new ActionDirection();

            dir.Velocity = new Vector2(3f, 4f);

            dir.VelocityLength.ShouldBe(5f);
        }

        [Test]
        public void GetDirection_Absolute_NoFlip_ReturnsVelocity()
        {
            var owner = new TestObject();
            var dir = new ActionDirection()
            {
                DirectionType = EDirectionType.Absolute,
                Velocity = new Vector2(2f, 3f)
            };

            var result = dir.GetDirection(owner);

            result.X.ShouldBe(2f);
            result.Y.ShouldBe(3f);
        }

        [Test]
        public void GetDirection_Absolute_WithFlip_NegatesX()
        {
            var owner = new TestObject();
            owner.Flip = true;
            var dir = new ActionDirection()
            {
                DirectionType = EDirectionType.Absolute,
                Velocity = new Vector2(2f, 3f)
            };

            var result = dir.GetDirection(owner);

            result.X.ShouldBe(-2f);
            result.Y.ShouldBe(3f);
        }

        [Test]
        public void GetDirection_Absolute_ScalesWithOwnerScale()
        {
            var owner = new TestObject();
            owner.Scale = 2f;
            var dir = new ActionDirection()
            {
                DirectionType = EDirectionType.Absolute,
                Velocity = new Vector2(3f, 4f)
            };

            var result = dir.GetDirection(owner);

            result.X.ShouldBe(6f);
            result.Y.ShouldBe(8f);
        }

        [Test]
        public void GetDirection_VelocityType_WhenOwnerMoving_UsesOwnerVelocityDirection()
        {
            var owner = new TestObject();
            owner.Velocity = new Vector2(3f, 0f);
            var dir = new ActionDirection()
            {
                DirectionType = EDirectionType.Velocity,
                Velocity = new Vector2(5f, 0f) // VelocityLength = 5
            };

            var result = dir.GetDirection(owner);

            // Owner velocity normalizes to (1,0), scaled by VelocityLength=5 and Scale=1
            result.X.ShouldBe(5f, tolerance: 0.001f);
            result.Y.ShouldBe(0f, tolerance: 0.001f);
        }

        [Test]
        public void GetDirection_VelocityType_WhenOwnerStationary_FallsBackToRelative()
        {
            var owner = new TestObject(); // Velocity=Zero, Rotation=0, Flip=false
            var dir = new ActionDirection()
            {
                DirectionType = EDirectionType.Velocity,
                Velocity = new Vector2(4f, 0f)
            };

            var result = dir.GetDirection(owner);

            result.X.ShouldBe(4f, tolerance: 0.001f);
            result.Y.ShouldBe(0f, tolerance: 0.001f);
        }

        [Test]
        public void GetDirection_NegController_WithNoInput_NegatesRelativeDirection()
        {
            var owner = new TestObject(); // Direction()=Zero, Rotation=0, Flip=false
            var dir = new ActionDirection()
            {
                DirectionType = EDirectionType.NegController,
                Velocity = new Vector2(2f, 1f)
            };

            var positive = new ActionDirection()
            {
                DirectionType = EDirectionType.Absolute,
                Velocity = new Vector2(2f, 1f)
            };

            var negResult = dir.GetDirection(owner);
            var posResult = positive.GetDirection(owner);

            negResult.X.ShouldBe(-posResult.X, tolerance: 0.001f);
            negResult.Y.ShouldBe(-posResult.Y, tolerance: 0.001f);
        }

        [Test]
        public void Constructor_FromModel_CopiesDirectionTypeAndVelocity()
        {
            var model = new DirectionActionModel()
            {
                DirectionType = EDirectionType.Absolute,
                Velocity = new Vector2(3f, 4f)
            };

            var dir = new ActionDirection(model);

            dir.DirectionType.ShouldBe(EDirectionType.Absolute);
            dir.Velocity.X.ShouldBe(3f);
            dir.Velocity.Y.ShouldBe(4f);
            dir.VelocityLength.ShouldBe(5f);
        }
    }
}
