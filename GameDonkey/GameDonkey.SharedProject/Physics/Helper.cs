using MathNet.Numerics;
using Microsoft.Xna.Framework;
using System;

namespace GameDonkeyLib
{
	public static class Helper
	{
		public static float Gravity()
		{
			//gravity is positive in this game since y axis is flipped
			return 9.8f * 175.0f; //add some number cuz shit is fucked up
		}

		/// <summary>
		/// This is the fastest speed for the player in the +y direction while falling
		/// </summary>
		/// <returns></returns>
		public static float MaxFallingSpeed()
		{
			return 2300.0f;
		}

		public static float atan2(Vector2 vect)
		{
			return atan2(vect.X, vect.Y);
		}

		public static float atan2(float fX, float fY)
		{
			fY *= -1.0f; //flip on y axis for coordinate system

			//get the angle to that vector
			float fAngle = (float)Math.Atan2((double)fY, (double)fX);

			return fAngle;
		}

		public static float ClampAngle(float angle)
		{
			//keep the angle between -180 and 180
			while (-MathHelper.Pi > angle)
			{
				angle += MathHelper.TwoPi;
			}

			while (MathHelper.Pi < angle)
			{
				angle -= MathHelper.TwoPi;
			}

			return angle;
		}

		public static int SecondsToFrames(float seconds)
		{
			seconds *= 60.0f;
			return (int)(seconds + 0.5f);
		}

		public static float FramesToSeconds(int frames)
		{
			return ((float)frames / 60.0f);
		}

		public static float Length(float fX, float fY)
		{
			Vector2 myVect = new Vector2(0.0f);
			myVect.X = fX;
			myVect.Y = fY;
			return myVect.Length();
		}

		public static bool AlmostEqual(this Vector2 myVector, Vector2 otherVector)
		{
			return myVector.X.AlmostEqual(otherVector.X) && myVector.Y.AlmostEqual(otherVector.Y);
		}
	}
}