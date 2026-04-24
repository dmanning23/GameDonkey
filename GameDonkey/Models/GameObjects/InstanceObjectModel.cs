using FilenameBuddy;
using System;
using System.Xml;

namespace GameDonkeyLib
{
	public class InstanceObjectModel : BaseObjectModel
	{
		public InstanceObjectModel(Filename filename, string contentName = "model") : base(contentName, filename)
		{
		}
	}
}
