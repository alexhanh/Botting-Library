#include "Action.h"

#include <algorithm>
using namespace std;

void ActionList::Clear() { 
	actions.clear(); 
}

void ActionList::Add(Action& action) { 
	actions.push_back(action); 
	sort(actions.begin(), actions.end());
}

bool ActionList::GetAction(ActionType type, Action& actionOut)
{
	for (int i=0; i<actions.size(); i++)
	{
		if (actions[i].type == type)
		{
			actionOut = actions[i];
			return true;
		}
	}

	return false;
}

string Action::GetTypeString()
{
	switch (type)
	{
	case Stand: return "Stand";
	case Hit: return "Hit";
	case Double: return "Double";
	case Split: return "Split";
	case Surrender: return "Surrender";
	case Insurance: return "Insurance";
	}

	return "None";
}