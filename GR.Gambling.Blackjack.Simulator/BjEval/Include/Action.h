#ifndef __ACTION_H__
#define __ACTION_H__

#include <vector>
#include <string>

enum ActionType
{
	None,
	Stand, 
	Hit, 
	Double, 
	Split, 
	Surrender, 
	Insurance
};

struct Action
{
	Action() : type(None) {}
	Action(ActionType type, double ev) : type(type), ev(ev) {}

	bool operator < (const Action& a) { return (this->ev > a.ev); }

	std::string GetTypeString();

	ActionType type;
	double ev;
};

class ActionList
{
public:
	ActionList()
	{
	}

	int Count() { return actions.size(); }
	int GetCount() { return actions.size(); }

	Action operator [] (int index) { return actions[index]; }

	void Clear();

	void Add(Action& action);

	bool GetAction(ActionType type, Action& actionOut);


private:

	std::vector<Action> actions;
};

#endif