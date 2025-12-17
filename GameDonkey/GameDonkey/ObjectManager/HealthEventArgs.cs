using System;
using System.Collections.Generic;
using System.Text;

namespace GameDonkeyLib
{
	public class HealthEventArgs : EventArgs
	{
		public float HealthChange { get; set; }

		public HealthEventArgs(float healthChange) : base()
		{
			HealthChange = healthChange;
		}
	}
}
