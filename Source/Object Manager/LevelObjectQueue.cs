using Microsoft.Xna.Framework;
using System.Diagnostics;
#if NETWORKING
using Microsoft.Xna.Framework.Net;
#endif

namespace GameDonkey
{
	public class CLevelObjectQueue : CPlayerQueue
	{
		public CLevelObjectQueue() : base(Color.White, 0)
		{
		}

		public override void Reset()
		{
			Debug.Assert(null == m_rCharacter);
			Debug.Assert(0 == m_listInactive.Count);

			//reset all the level objects
			for (int i = 0; i < m_listActive.Count; i++)
			{
				//reset the thing back to it's start state
				m_listActive[i].Reset();
			}
		}

#if NETWORKING

		/// <summary>
		/// Read this object from a network packet reader.
		/// </summary>
		public override void ReadFromNetwork(PacketReader packetReader)
		{
			int iQueueID = packetReader.ReadInt32();
			Debug.Assert(QueueID == iQueueID);

			base.ReadFromNetwork(packetReader);
		}

#endif
	}
}