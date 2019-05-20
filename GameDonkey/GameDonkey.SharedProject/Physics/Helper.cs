using Microsoft.Xna.Framework;
using System;

namespace GameDonkeyLib
{
	public static class Helper
	{
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
	}
}