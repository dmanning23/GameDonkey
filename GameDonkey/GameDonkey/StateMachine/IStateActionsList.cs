using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace GameDonkeyLib
{
	public interface IStateActionsList
	{
		List<BaseAction> Actions { get; }

		BaseAction AddNewActionFromType(EActionType actionType, BaseObject owner, IGameDonkey engine, ContentManager content);
		void LoadContent(IGameDonkey engine, ContentManager content);
		void LoadStateActions(StateActionsListModel actionModels, BaseObject owner, IStateContainer stateContainer);
		bool RemoveAction(BaseAction action);
		void Sort();
	}
}