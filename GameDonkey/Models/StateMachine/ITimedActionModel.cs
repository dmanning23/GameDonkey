using System;
using System.Collections.Generic;
using System.Text;

namespace GameDonkeyLib
{
	public interface ITimedActionModel
	{
		TimedActionModel TimeDelta { get; }
	}
}