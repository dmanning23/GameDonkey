using System;
using System.Collections.Generic;
using System.Text;

namespace GameDonkey
{
	/// <summary>
	/// This object describes all the items needed for a board
	/// </summary>
	public class BoardXML
	{
		public string name = "";
		public int boardHeight = 0;
		public int boardWidth = 0;
		public string music = "";
		public string deathNoise = "";
		public string backgroundTile = "";
		public byte backgroundR = 0;
		public byte backgroundG = 0;
		public byte backgroundB = 0;
		public int numTiles = 0;
		public List<string> objects = new List<string>();
		public List<SpawnPointXML> spawnPoints = new List<SpawnPointXML>();
	}
}
