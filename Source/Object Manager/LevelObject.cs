using StateMachineBuddy;
using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using GameTimer;
using FilenameBuddy;

namespace GameDonkey
{
	public class LevelObject : BaseObject
	{
		#region Members

		/// <summary>
		/// how fast players will pop out of a level object, pixels/second
		/// </summary>
		private const float m_fMoveSpeed = 1750.0f;

		#endregion //Members

		#region Methods

		public LevelObject(HitPauseClock rClock, int iQueueID) : base(EObjectType.Level, rClock, iQueueID)
		{
		}

		protected override void Init()
		{
			m_Physics = new LevelObjectPhysicsContainer(this);
			States = new ObjectStateContainer(new StateMachine());
			States.StateChangedEvent += this.StateChanged;
		}

		public override void CollisionResponse(IPhysicsContainer rOtherObject,
			CreateAttackAction rAttackAction,
			Vector2 FirstCollisionPoint,
			Vector2 SecondCollisionPoint)
		{
			Debug.Assert(null != m_Physics);
			Debug.Assert(null != rOtherObject);

			//get a vector from the level object to the object
			Vector2 LevelToObject = FirstCollisionPoint - SecondCollisionPoint;

			//set how far to move the other object
			float fMoveSpeed = LevelToObject.Length();
			if (fMoveSpeed <= 0.0f)
			{
				return;
			}

			if (LevelToObject.Y > 0.0f)
			{
				//set the distance to diameter of the circle minus the current y
				fMoveSpeed += (CharacterClock.TimeDelta * m_fMoveSpeed);
			}
			//if (fMoveSpeed > (CharacterClock.TimeDelta * m_fMoveSpeed))
			//{
			//    fMoveSpeed = (CharacterClock.TimeDelta * m_fMoveSpeed);
			//}

			//add a "ground hit" to the other object?
			LevelToObject.Y = -1.0f * Math.Abs(LevelToObject.Y);
			LevelToObject.Normalize();
			if (!rOtherObject.HitFlags[(int)EHitType.GroundHit] || (fMoveSpeed > rOtherObject.Hits[(int)EHitType.GroundHit].Strength))
			{
				Debug.Assert(null != rOtherObject.Hits[(int)EHitType.GroundHit]);

				rOtherObject.HitFlags[(int)EHitType.GroundHit] = true;
				rOtherObject.Hits[(int)EHitType.GroundHit].Set(
					LevelToObject,
					null,
					fMoveSpeed,
					EHitType.GroundHit,
					null,
					FirstCollisionPoint);
			}
		}

		protected override void RespondToGroundHit(Hit rGroundHit, IGameDonkey rEngine)
		{
			//should never get here
			Debug.Assert(false);
		}
		
		#endregion //Methods

		#region File IO

		public bool LoadXmlBoard(string strBoardFile)
		{
			////Open the file.
			//Filename myBoardFile = new Filename(strBoardFile);
			//FileStream stream = File.Open(myBoardFile.File, FileMode.Open, FileAccess.Read);
			//XmlDocument xmlDoc = new XmlDocument();
			//xmlDoc.Load(stream);
			//XmlNode rootNode = xmlDoc.DocumentElement;

			////make sure it is actually an xml node
			//if (rootNode.NodeType != XmlNodeType.Element)
			//{
			//	//should be an xml node!!!
			//	Debug.Assert(false);
			//	return false;
			//}

			////eat up the name of that xml node
			//string strElementName = rootNode.Name;
			//if (("XnaContent" != strElementName) || !rootNode.HasChildNodes)
			//{
			//	return false;
			//}

			////next node is "<Asset Type="SPFSettings.LevelObjectXML">"
			//XmlNode AssetNode = rootNode.FirstChild;
			//if (null == AssetNode)
			//{
			//	Debug.Assert(false);
			//	return false;
			//}
			//if (!AssetNode.HasChildNodes)
			//{
			//	Debug.Assert(false);
			//	return false;
			//}
			//if ("Asset" != AssetNode.Name)
			//{
			//	Debug.Assert(false);
			//	return false;
			//}

			////First node is the model file
			//XmlNode childNode = AssetNode.FirstChild;
			//Filename strModelFile = new Filename(childNode.InnerXml);
			//if (!AnimationContainer.ReadXMLModelFormat(strModelFile.File, rEngine.Renderer))
			//{
			//	Debug.Assert(false);
			//	return false;
			//}

			////next node is the animation file
			//childNode = childNode.NextSibling;
			//Filename strAnimationFile = new Filename(childNode.InnerXml);
			//if (!AnimationContainer.ReadXMLAnimationFormat(strAnimationFile.File))
			//{
			//	Debug.Assert(false);
			//	return false;
			//}

			////next node is the garments...
			//childNode = childNode.NextSibling;
			//for (XmlNode garmentNode = childNode.FirstChild;
			//	null != garmentNode;
			//	garmentNode = garmentNode.NextSibling)
			//{
			//	//Load up the garment.
			//	Filename strGarmentFile = new Filename(garmentNode.InnerXml);
			//	LoadXmlGarment(rEngine, strGarmentFile);
			//}

			////the state machine?
			//childNode = childNode.NextSibling;
			////Filename strStateMachineFile = new Filename(childNode.InnerXml); //ignored in this game

			////the states file?
			//childNode = childNode.NextSibling;

			////get the height of this dude
			//childNode = childNode.NextSibling;
			//m_fHeight = Convert.ToSingle(childNode.InnerXml);

			////get the portrait file
			//childNode = childNode.NextSibling;
			//Filename strPortraitFile = new Filename(childNode.InnerXml);
			//Debug.Assert(null != rEngine.Renderer.Content);
			//m_Portrait = rEngine.Renderer.Content.Load<Texture2D>(strPortraitFile.GetRelPathFileNoExt());

			////TODO: grab the deathsound
			//childNode = childNode.NextSibling;
			//m_strDeathSound = childNode.InnerXml;
			////Debug.Assert(null != CAudioManager.GetCue(m_strDeathSound));

			////get the ground states of this dude
			//childNode = childNode.NextSibling;
			//Filename groundStatesFile = new Filename(childNode.InnerXml);
			//States.ReadXmlStateContainer(@"Content\wedding state machines\ground state machine.xml", iMessageOffset, groundStatesFile.File, this, rEngine, true, false);

			////get teh upstates of this dude
			//childNode = childNode.NextSibling;
			//Filename upStatesFile = new Filename(childNode.InnerXml);
			//States.ReadXmlStateContainer(@"Content\wedding state machines\up state machine.xml", iMessageOffset, upStatesFile.File, this, rEngine, true, true);

			//////load the down states
			////strStatesFile.SetRelFilename(myCharXML.DownStates);
			////States.ReadStateContainer(rXmlContent, @"state machines\down state machine", iMessageOffset, strStatesFile.GetRelPathFileNoExt(), this, rEngine, true, true);
			////rXmlContent.Unload();

			//////load the forward states
			////strStatesFile.SetRelFilename(myCharXML.ForwardStates);
			////States.ReadStateContainer(rXmlContent, @"state machines\forward state machine", iMessageOffset, strStatesFile.GetRelPathFileNoExt(), this, rEngine, true, true);
			////rXmlContent.Unload();

			//// Close the file.
			//stream.Close();
			//return true;


			//////load the resource
			////SPFSettings.BoardXML myDude = rXmlContent.Load<SPFSettings.BoardXML>(strBoardFile);

			//////grab all the spawn points
			////for (int i = 0; i < myDude.spawnPoints.Count; i++)
			////{
			////	m_listSpawnPoints.Add(myDude.spawnPoints[i].location);
			////}

			//////grab teh name of teh music resource for this board
			////m_strMusicFile = myDude.music;

			//////TODO: load the death noise
			////m_strDeathNoise = myDude.deathNoise;
			//////Debug.Assert(null != CAudioManager.GetCue(m_strDeathNoise));

			//////open the background image stuff
			////m_SkyBox = (XNATexture)Renderer.LoadImage(myDude.backgroundTile);
			////m_SkyColor.R = myDude.backgroundR;
			////m_SkyColor.G = myDude.backgroundG;
			////m_SkyColor.B = myDude.backgroundB;
			////m_SkyColor.A = 255;

			////m_iNumTiles = myDude.numTiles;

			//////grab the world boundaries
			////WorldBoundaries = new Rectangle((-1 * (myDude.boardWidth / 2)),
			////	(-1 * (myDude.boardHeight / 2)),
			////	myDude.boardWidth,
			////	myDude.boardHeight);

			//////load all the level objects
			////for (int i = 0; i < myDude.objects.Count; i++)
			////{
			////	//load the level object
			////	Filename myLevelObjectFile = new Filename(myDude.objects[i]);
			////	BaseObject rLevelObject = m_LevelObjects.LoadSerializedObject(rXmlContent, myLevelObjectFile, this, EObjectType.Level, 0);
			////	if (null == rLevelObject)
			////	{
			////		Debug.Assert(false);
			////	}
			////}

			////m_LevelObjects.PlayerName = "Board";

			Debug.Assert(false);
			return false;
		}

		public override bool LoadSerializedObject(ContentManager rXmlContent, Filename strResource, IGameDonkey rEngine, int iMessageOffset)
		{
			SPFSettings.LevelObjectXML myCharXML = rXmlContent.Load<SPFSettings.LevelObjectXML>(strResource.GetRelPathFileNoExt());
			if (!base.LoadObject(rXmlContent, myCharXML, rEngine, iMessageOffset))
			{
				return false;
			}

			//set the scale
			Scale = (float)myCharXML.size;

			//set teh position
			Position = myCharXML.location;

			return true;
		}

		#endregion //File IO
	}
}