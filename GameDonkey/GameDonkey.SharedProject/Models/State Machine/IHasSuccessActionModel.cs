using System;
using System.Collections.Generic;
using System.Text;

namespace GameDonkeyLib
{
	public interface IHasSuccessActionModel
	{
		List<BaseActionModel> SuccessActions { get; }
	}
}