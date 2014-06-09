using Microsoft.Xna.Framework;
using ParticleBuddy;
using System.Collections.Generic;

namespace SPFSettings
{
	#region Action XML Objects

	public class StateActionListXML
	{
		public List<StateActionsXML> states = new List<StateActionsXML>();
	}

	public class StateActionsXML
	{
		public string name = "";
		public List<BaseActionXML> actions = new List<BaseActionXML>();
	}

	/// <summary>
	/// Contains all the base variables for serializing a state action
	/// </summary>
	public class BaseActionXML
	{
		public string type = "";
		public float time = 0.0f;
	}

	public class DirectionActionXML
	{
		public Vector2 velocity = new Vector2(0.0f);
		public string directionType = "";
	}

	public class AddVelocityActionXML : BaseActionXML
	{
		public DirectionActionXML direction = new DirectionActionXML();
	}

	public class ConstantAccelerationActionXML : BaseActionXML
	{
		public DirectionActionXML direction = new DirectionActionXML();
		public float maxVelocity = 0.0f;
	}

	public class ConstantDeccelerationActionXML : BaseActionXML
	{
		public Vector2 direction = new Vector2(0.0f);
		public float minYVelocity = 0.0f;
	}

	public class RotateActionXML : BaseActionXML
	{
		public float rotation = 0.0f;
	}

	public class TargetRotationActionXML : BaseActionXML
	{
		public float timeDelta = 0.0f;
		public DirectionActionXML targetRotation = new DirectionActionXML();
	}

	public class CreateAttackActionXML : BaseActionXML
	{
		public string boneName = "";
		public Vector2 direction = new Vector2(0.0f);
		public float damage = 0.0f;
		public float timeDelta = 0.0f;
		public string hitSound = "";
		public List<BaseActionXML> successActions = new List<BaseActionXML>();
	}

	public class CreateBlockActionXML : BaseActionXML
	{
		public float timeDelta = 0.0f;
	}

	public class CreateThrowActionXML : CreateAttackActionXML
	{
		public string throwMessage = "";
		public float releaseTimeDelta = 0.0f;
	}

	public class CreateHitCircleActionXML : CreateAttackActionXML
	{
		public float radius = 0.0f;
		public Vector2 startOffset = new Vector2(0.0f);
		public Vector2 velocity = new Vector2(0.0f);
	}

	public class DeactivateActionXML : BaseActionXML
	{
	}

	public class EvadeActionXML : BaseActionXML
	{
		public float timeDelta = 0.0f;
	}

	public class CameraShakeActionXML : BaseActionXML
	{
		public float TimeDelta = 0.0f;
		public float ShakeAmount = 0.0f;
	}

	public class ParticleEffectActionXML : BaseActionXML
	{
		public string bone = "";
		public DirectionActionXML direction = new DirectionActionXML();
		public Vector2 StartOffset = new Vector2(0.0f);
		public bool UseBoneRotation = false;
		public List<ParticleXML> emitter = new List<ParticleXML>();
	}

	public class PlayAnimationActionXML : BaseActionXML
	{
		public string animation = "";
		public string playback = "";
	}

	public class PlaySoundActionXML : BaseActionXML
	{
		public string filename = "";
	}

	public class ProjectileActionXML : BaseActionXML
	{
		public string filename = "";
		public Vector2 startOffset = new Vector2(0.0f);
		public float scale = 0.0f;
		public DirectionActionXML direction = new DirectionActionXML();
	}

	public class SendStateMessageActionXML : BaseActionXML
	{
		public string message = "";
	}

	public class SetVelocityActionXML : BaseActionXML
	{
		public DirectionActionXML direction = new DirectionActionXML();
	}

	public class TrailActionXML : BaseActionXML
	{
		public byte R = 0;
		public byte G = 0;
		public byte B = 0;
		public byte A = 0;
		public float lifeDelta = 0.0f;
		public float spawnDelta = 0.0f;
		public float timeDelta = 0.0f;
	}

	public class AddGarmentActionXML : BaseActionXML
	{
		public float timeDelta = 0.0f;
		public string garmentFile = "";
	}

	public class BlockingStateActionXML : BaseActionXML
	{
		public float timeDelta = 0.0f;
		public string boneName = "";
		public string hitSound = "";
		public List<BaseActionXML> successActions = new List<BaseActionXML>();
	}

	#endregion //Action XML Objects

	#region Character XML Objects

	public class StateContainerXML
	{
		public string stateMachine = "";
		public string stateActions = "";
	}

	public class BaseObjectXML
	{
		public string model = "";
		public string animations = "";
		public List<string> garments = new List<string>();
		public List<StateContainerXML> states = new List<StateContainerXML>();
		public int height = 0;
	}

	public class PlayerObjectXML : BaseObjectXML
	{
		public string portrait = "";
		public string deathSound = "";
	}

	public class ProjectileObjectXML : BaseObjectXML
	{
		public bool weaponhits = false;
	}

	#endregion //Character XML Objects

	#region Board Objects

	/// <summary>
	/// This object stores all the information to load a level object
	/// </summary>
	public class LevelObjectXML : BaseObjectXML
	{
		public float size = 0.0f;
		public Vector2 location = new Vector2();
	}

	public class SpawnPointXML
	{
		public Vector2 location = new Vector2();
	}

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

	#endregion //Board Objects
}