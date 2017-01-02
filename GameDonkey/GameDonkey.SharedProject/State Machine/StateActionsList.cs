using FilenameBuddy;
using GameTimer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using StateMachineBuddy;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;

namespace GameDonkey
{
	/// <summary>
	/// A list of all the state actions for a single state machine.
	/// </summary>
	public class StateActionsList
	{
		#region Members

		private List<StateActions> m_listActions;

		#endregion //Members

		#region Methods

		/// <summary>
		/// standard constructor
		/// </summary>
		public StateActionsList()
		{
			m_listActions = new List<StateActions>();
		}

		/// <summary>
		/// The states have changed, go through and set all the actions of the new state to "not run"
		/// </summary>
		/// <param name="iCurState">the new state of the object</param>
		public void StateChange(int iCurState)
		{
			Debug.Assert(iCurState >= 0);
			Debug.Assert(iCurState < m_listActions.Count);

			//set the new state actions to 'not run'
			m_listActions[iCurState].StateChange();
		}

		/// <summary>
		/// Execute the actions for the current state
		/// </summary>
		/// <param name="fPrevTime">the last time that the object executed actions</param>
		/// <param name="fCurTime">the time in seconds that the object has been in this state</param>
		public void ExecuteActions(GameClock rGameClock, int iCurState)
		{
			float fCurTime = rGameClock.CurrentTime;
			float fPrevTime = rGameClock.CurrentTime - rGameClock.TimeDelta;

			Debug.Assert(fCurTime >= fPrevTime);
			Debug.Assert(iCurState >= 0);
			Debug.Assert(iCurState < m_listActions.Count);

			//execute the correct action container
			m_listActions[iCurState].ExecuteAction(fPrevTime, fCurTime);
		}

		/// <summary>
		/// Check if a state is an attack state
		/// </summary>
		/// <param name="iState">The state to check if is an attack state</param>
		/// <returns>bool: whether or not the requested state has an attack</returns>
		public bool IsStateAttack(int iState)
		{
			Debug.Assert(iState >= 0);
			Debug.Assert(iState < m_listActions.Count);
			return m_listActions[iState].IsAttack;
		}

		/// <summary>
		/// Check if an attack is active in this state.  Used to queue moves during a combo.
		/// </summary>
		/// <returns>bool: this state hasnt yet entered the recovery phase.  
		/// The move should be queued until it finishes.</returns>
		public bool IsAttackActive(GameClock rGameClock, int iCurState)
		{
			Debug.Assert(iCurState >= 0);
			Debug.Assert(iCurState < m_listActions.Count);

			//check if the current state is an attack state, and if an attack is active
			return m_listActions[iCurState].IsAttackActive(rGameClock);
		}

		/// <summary>
		/// Comapre two state containers and verify that the state actions are exactly the same.
		/// Used for importing/exporting from old formats.
		/// </summary>
		/// <param name="rInst">the object to compare to</param>
		/// <returns>bool: whether or not these state containers are exactly the same.</returns>
		public bool CompareActions(StateActionsList rInst)
		{
			if (m_listActions.Count != rInst.m_listActions.Count)
			{
				int iMyCount = m_listActions.Count;
				int iOtherCount = rInst.m_listActions.Count;
				Debug.Assert(false);
				return false;
			}
			for (int i = 0; i < m_listActions.Count; i++)
			{
				if (!m_listActions[i].Compare(rInst.m_listActions[i]))
				{
					Debug.Assert(false);
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Replace all the base object pointers in this dude to point to a replacement object
		/// </summary>
		/// <param name="myBot">the replacement dude</param>
		public void ReplaceOwner(BaseObject myBot)
		{
			//replace in all the state actions
			for (int i = 0; i < m_listActions.Count; i++)
			{
				m_listActions[i].ReplaceOwner(myBot);
			}
		}

		public StateActions GetStateActions(int iStateIndex)
		{
			Debug.Assert(0 <= iStateIndex);
			Debug.Assert(iStateIndex < m_listActions.Count);
			return m_listActions[iStateIndex];
		}

		#endregion //Methods

		#region State Action File IO

		public bool ReadXmlStateActions(Filename filename, BaseObject owner, IGameDonkey engine, SingleStateContainer stateContainer, ContentManager content)
		{
			if (null == content)
			{
				// Open the file.
#if ANDROID
				using (var stream = Game.Activity.Assets.Open(strFilename.File))
#else
				using (var stream = File.Open(filename.File, FileMode.Open, FileAccess.Read))
#endif
				{
					XmlDocument xmlDoc = new XmlDocument();
					xmlDoc.Load(stream);
					return ReadXmlDocument(xmlDoc, owner, engine, stateContainer, content);
				}
			}
			else
			{
				var xmlContent = content.Load<string>(filename.GetRelPathFileNoExt());
				var xmlDoc = new XmlDocument();
				xmlDoc.LoadXml(xmlContent);
				return ReadXmlDocument(xmlDoc, owner, engine, stateContainer, content);
			}
		}

		protected bool ReadXmlDocument(XmlDocument xmlDoc, BaseObject owner, IGameDonkey engine, SingleStateContainer stateContainer, ContentManager content)
		{
			XmlNode rootNode = xmlDoc.DocumentElement;

			if (rootNode.NodeType != XmlNodeType.Element)
			{
				//should be an xml node!!!
				Debug.Assert(false);
				return false;
			}

			//eat up the name of that xml node
			string strElementName = rootNode.Name;
			if (("XnaContent" != strElementName) || !rootNode.HasChildNodes)
			{
				Debug.Assert(false);
				return false;
			}

			//next node is "<Asset Type="SPFSettings.StateActionListXML">"
			XmlNode AssetNode = rootNode.FirstChild;
			if (null == AssetNode)
			{
				Debug.Assert(false);
				return false;
			}
			if (!AssetNode.HasChildNodes)
			{
				Debug.Assert(false);
				return false;
			}

			//next node is "<states>"
			XmlNode StatesNode = AssetNode.FirstChild;
			if (null == StatesNode)
			{
				Debug.Assert(false);
				return false;
			}
			if (!StatesNode.HasChildNodes)
			{
				Debug.Assert(false);
				return false;
			}

			//the rest of the nodes are the states
			for (XmlNode childNode = StatesNode.FirstChild;
				null != childNode;
				childNode = childNode.NextSibling)
			{
				StateActions myActions = new StateActions();

				//skip all the comments
				if ("#comment" == childNode.Name)
				{
					continue;
				}

				if (!myActions.ReadXml(childNode, owner, engine, stateContainer, content))
				{
					Debug.Assert(false);
					return false;
				}
				m_listActions.Add(myActions);
			}

			return true;
		}

#if !WINDOWS_UWP
		public bool WriteXml(Filename strFilename, StateMachine rStateMachine)
		{
			//open the file, create it if it doesnt exist yet
			XmlTextWriter rXMLFile = new XmlTextWriter(strFilename.File, null);
			rXMLFile.Formatting = Formatting.Indented;
			rXMLFile.Indentation = 1;
			rXMLFile.IndentChar = '\t';

			rXMLFile.WriteStartDocument();

			//add the xml node
			rXMLFile.WriteStartElement("XnaContent");
			rXMLFile.WriteStartElement("Asset");
			rXMLFile.WriteAttributeString("Type", "SPFSettings.StateActionListXML");

			//write out start node
			rXMLFile.WriteStartElement("states");

			//Go through the states and make sure they all have a set of actions in there!
			for (int i = 0; i < rStateMachine.NumStates; i++)
			{
				//Get the state name
				string strStateName = rStateMachine.GetStateName(i);

				//Find the state actions for this state and write them out
				bool bFound = false;
				for (int j = 0; j < m_listActions.Count; j++)
				{
					if (m_listActions[j].StateName == strStateName)
					{
						bFound = true;
						m_listActions[j].WriteXml(rXMLFile);
						break;
					}
				}

				//If it wasn't found, create a new set of state actions, set the name, and write it out
				if (!bFound)
				{
					var actions = new StateActions();
					actions.StateName = strStateName;
					m_listActions.Add(actions);
					actions.WriteXml(rXMLFile);
				}

				rXMLFile.Flush();
			}
			
			rXMLFile.WriteEndElement(); //XnaContent
			rXMLFile.WriteEndElement(); //Asset
			rXMLFile.WriteEndElement(); //states

			rXMLFile.WriteEndDocument();

			// Close the file.
			rXMLFile.Flush();
			rXMLFile.Close();

			return true;
		}
#endif

		#endregion //State Action File IO
	}
}