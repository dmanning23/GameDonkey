using FilenameBuddy;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Xml;
using XmlBuddy;

namespace GameDonkeyLib
{
	/// <summary>
	/// This object describes all the items needed for a board
	/// </summary>
	public class BoardModel : XmlFileBuddy
	{
		#region Properties

		public string Name { get; private set; }
		public int BoardHeight { get; private set; }
		public int BoardWidth { get; private set; }
		public int Floor { get; private set; }
		public Filename Music { get; private set; }
		public Filename DeathNoise { get; private set; }
		public Filename BackgroundTile { get; private set; }
		public Color BackgroundColor { get; private set; }
		public int NumTiles { get; private set; }
		public List<Filename> LevelObjects { get; private set; }
		public List<SpawnPointModel> SpawnPoints { get; private set; }

		public List<BackgroundLayerModel> Background { get; private set; }
		public List<BackgroundLayerModel> Foreground { get; private set; }

		#endregion //Properties

		#region Methods

		public BoardModel(Filename filename) : base("board", filename)
		{
			Music = new Filename();
			DeathNoise = new Filename();
			BackgroundTile = new Filename();
			BackgroundColor = Color.White;
			LevelObjects = new List<Filename>();
			SpawnPoints = new List<SpawnPointModel>();
			Background = new List<BackgroundLayerModel>();
			Foreground = new List<BackgroundLayerModel>();
		}

		public override string ToString()
		{
			return Name;
		}

		#endregion //Methods

		#region File IO

		public override void ParseXmlNode(XmlNode node)
		{
			//what is in this node?
			var name = node.Name;
			var value = node.InnerText;

			switch (name)
			{
				case "Asset":
					{
						//skip these old ass nodes
						XmlFileBuddy.ReadChildNodes(node, ParseXmlNode);
					}
					break;
				case "Type":
					{
						//Really skip these old ass nodes
					}
					break;
				case "name":
					{
						Name = value;
					}
					break;
				case "boardHeight":
					{
						BoardHeight = Convert.ToInt32(value);
					}
					break;
				case "floor":
					{
						Floor = Convert.ToInt32(value);
					}
					break;
				case "boardWidth":
					{
						BoardWidth = Convert.ToInt32(value);
					}
					break;
				case "music":
					{
						Music.SetRelFilename(value);
					}
					break;
				case "deathNoise":
					{
						DeathNoise.SetRelFilename(value);
					}
					break;
				case "background":
					{
						XmlFileBuddy.ReadChildNodes(node, ReadBackground);
					}
					break;
				case "foreground":
					{
						XmlFileBuddy.ReadChildNodes(node, ReadForeground);
					}
					break;
				case "backgroundTile":
					{
						BackgroundTile.SetRelFilename(value);
					}
					break;
				case "BackgroundColor":
					{
						//BackgroundColor = Color.
					}
					break;
				case "backgroundR":
					{
						BackgroundColor = new Color(Convert.ToByte(value), BackgroundColor.G, BackgroundColor.B);
					}
					break;
				case "backgroundG":
					{
						BackgroundColor = new Color(BackgroundColor.R, Convert.ToByte(value), BackgroundColor.B);
					}
					break;
				case "backgroundB":
					{
						BackgroundColor = new Color(BackgroundColor.R, BackgroundColor.G, Convert.ToByte(value));
					}
					break;
				case "numTiles":
				case "NumTiles":
					{
						NumTiles = Convert.ToInt32(value);
					}
					break;
				case "objects":
				case "levelObjects":
					{
						XmlFileBuddy.ReadChildNodes(node, ReadLevelObjects);
					}
					break;
				case "spawnPoints":
					{
						XmlFileBuddy.ReadChildNodes(node, ReadSpawnPoints);
					}
					break;
				default:
					{
						NodeError(node);
					}
					break;
			}
		}

		public void ReadLevelObjects(XmlNode node)
		{
			LevelObjects.Add(new Filename(node.InnerText));
		}

		public void ReadSpawnPoints(XmlNode node)
		{
			var spawnPoint = new SpawnPointModel();
			XmlFileBuddy.ReadChildNodes(node, spawnPoint.ParseXmlNode);
			SpawnPoints.Add(spawnPoint);
		}

		public void ReadBackground(XmlNode node)
		{
			var spawnPoint = new BackgroundLayerModel(this);
			XmlFileBuddy.ReadChildNodes(node, spawnPoint.ParseXmlNode);
			Background.Add(spawnPoint);
		}

		public void ReadForeground(XmlNode node)
		{
			var spawnPoint = new BackgroundLayerModel(this);
			XmlFileBuddy.ReadChildNodes(node, spawnPoint.ParseXmlNode);
			Foreground.Add(spawnPoint);
		}

		/// <summary>
		/// Write this dude out to the xml format
		/// </summary>
		/// <param name="xmlWriter">the xml file to add this dude as a child of</param>
		public override void WriteXmlNodes(XmlTextWriter xmlWriter)
		{
			//write out the item tag
			xmlWriter.WriteStartElement("joint");
			xmlWriter.WriteAttributeString("name", Name);
			xmlWriter.WriteEndElement();
		}

		#endregion //File IO
	}
}
