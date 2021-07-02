using FilenameBuddy;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameDonkeyLib
{
	public interface IHasFilenameActionModel
	{
		Filename Filename { get; }
	}
}