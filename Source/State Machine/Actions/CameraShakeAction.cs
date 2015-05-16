using CameraBuddy;
using System;
using System.Diagnostics;
using System.Xml;

namespace GameDonkey
{
	public class CameraShakeAction : IBaseAction
	{
		#region Members

		/// <summary>
		/// the length of time to shake the camera
		/// </summary>
		public float TimeDelta { get; set; }

		/// <summary>
		/// how hard to shake the camera
		/// </summary>
		public float ShakeAmount { get; set; }

		/// <summary>
		/// the camera to shake
		/// </summary>
		public Camera Camera { get; set; }

		#endregion //Members

		#region Properties

		#endregion //Properties

		#region Methods

		/// <summary>
		/// Standard constructor
		/// </summary>
		public CameraShakeAction(BaseObject rOwner)
			: base(rOwner)
		{
			ActionType = EActionType.CameraShake;
			TimeDelta = 0.25f;
			ShakeAmount = 1.0f;
		}

		/// <summary>
		/// execute this action (overridden in all child classes)
		/// </summary>
		/// <returns>bool: whether or not to continue running actions after this dude runs</returns>
		public override bool Execute()
		{
			Debug.Assert(!AlreadyRun);
			Debug.Assert(null != Camera);

			Camera.AddCameraShake(TimeDelta, ShakeAmount);

			return base.Execute();
		}

		public override bool Compare(IBaseAction rInst)
		{
			CameraShakeAction myAction = (CameraShakeAction)rInst;
			return ((ActionType == myAction.ActionType) &&
				(Time == myAction.Time) &&
				(TimeDelta == myAction.TimeDelta) &&
				(ShakeAmount == myAction.ShakeAmount));
		}

		#endregion //Methods

		#region File IO

		/// <summary>
		/// Read from an xml file
		/// </summary>
		/// <param name="rXMLNode">the xml node to read from</param>
		/// <returns></returns>
		public override bool ReadXml(XmlNode rXMLNode, IGameDonkey rEngine, SingleStateContainer stateContainer)
		{
			Camera = rEngine.Renderer.Camera;

			#if DEBUG
			if ("Item" != rXMLNode.Name)
			{
				Debug.Assert(false);
				return false;
			}

			//should have an attribute Type
			XmlNamedNodeMap mapAttributes = rXMLNode.Attributes;
			for (int i = 0; i < mapAttributes.Count; i++)
			{
				//will only have the name attribute
				string strName = mapAttributes.Item(i).Name;
				string strValue = mapAttributes.Item(i).Value;
				if ("Type" == strName)
				{
					if (ActionType != StateActionFactory.XMLTypeToType(strValue))
					{
						Debug.Assert(false);
						return false;
					}
				}
				else
				{
					Debug.Assert(false);
					return false;
				}
			}
#endif

			//Read in child nodes
			if (rXMLNode.HasChildNodes)
			{
				for (XmlNode childNode = rXMLNode.FirstChild;
					null != childNode;
					childNode = childNode.NextSibling)
				{
					//what is in this node?
					string strName = childNode.Name;
					string strValue = childNode.InnerText;

					switch (strName)
					{
						case "type":
						{
							Debug.Assert(strValue == ActionType.ToString());
						}
						break;
						case "time":
						{
							Time = Convert.ToSingle(strValue);
							if (0.0f > Time)
							{
								Debug.Assert(0.0f <= Time);
								return false;
							}
						}
						break;
						case "TimeDelta":
						{
							TimeDelta = Convert.ToSingle(strValue);
							if (0.0f > TimeDelta)
							{
								Debug.Assert(0.0f <= TimeDelta);
								return false;
							}
						}
						break;
						case "ShakeAmount":
						{
							ShakeAmount = Convert.ToSingle(strValue);
							if (0.0f > ShakeAmount)
							{
								Debug.Assert(0.0f <= ShakeAmount);
								return false;
							}
						}
						break;
						default:
						{
							Debug.Assert(false);
							return false;
						}
					}
				}
			}

			return true;
		}

		/// <summary>
		/// overloaded in child classes to write out action specific stuff
		/// </summary>
		/// <param name="rXMLFile"></param>
		protected override void WriteActionXml(XmlTextWriter rXMLFile)
		{
			rXMLFile.WriteStartElement("TimeDelta");
			rXMLFile.WriteString(TimeDelta.ToString());
			rXMLFile.WriteEndElement();

			rXMLFile.WriteStartElement("ShakeAmount");
			rXMLFile.WriteString(ShakeAmount.ToString());
			rXMLFile.WriteEndElement();
		}

		#endregion //File IO
	}
}