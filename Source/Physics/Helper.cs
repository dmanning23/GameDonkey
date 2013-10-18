using Microsoft.Xna.Framework;
using System;

namespace GameDonkey
{
	public class Helper
	{
		public static float RagdollGravity()
		{
			//gravity is positive in this game since y axis is flipped
			return 9.8f * 150.0f; //add some number cuz shit is fucked up
		}

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

		public static float ClampAngle(float fAngle)
		{
			//keep the angle between -180 and 180
			while (-MathHelper.Pi > fAngle)
			{
				fAngle += MathHelper.TwoPi;
			}

			while (MathHelper.Pi < fAngle)
			{
				fAngle -= MathHelper.TwoPi;
			}

			return fAngle;
		}

		public static int SecondsToFrames(float fSeconds)
		{
			fSeconds *= 60.0f;
			return (int)(fSeconds + 0.5f);
		}

		public static float FramesToSeconds(int iFrames)
		{
			return ((float)iFrames / 60.0f);
		}

		public static float Length(float fX, float fY)
		{
			Vector2 myVect = new Vector2(0.0f);
			myVect.X = fX;
			myVect.Y = fY;
			return myVect.Length();
		}

		/// <summary>
		/// hello, standard constructor!
		/// </summary>
		private Helper() { }

	}
}