using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CAGA.Dialogue
{
    public enum ActionState
    {
        Unknown = 0,
        Initiated = 1,
        Planned = 2,
        Executing = 3,
        Complete = 4,
        Failed = 5
    }

    class ActionNode : PlanNode
    {
        private string _actType;

        public string ActType
        {
            get { return _actType; }
            set { _actType = value; }
        }
        private string _complexity;

        public string Complexity
        {
            get { return _complexity; }
            set { _complexity = value; }
        }
        private ArrayList _params;

        public ArrayList Params
        {
            get { return _params; }
            set { _params = value; }
        }
        private ArrayList _subActions;

        public ArrayList SubActions
        {
            get { return _subActions; }
            set { _subActions = value; }
        }
        private ArrayList _agents;

        public ArrayList Agents
        {
            get { return _agents; }
            set { _agents = value; }
        }
        private int _recipeId;

        public int RecipeId
        {
            get { return _recipeId; }
            set { _recipeId = value; }
        }


        private ArrayList _recipeList;

        public ArrayList RecipeList
        {
            get { return _recipeList; }
            set { _recipeList = value; }
        }

        private bool _optional;

        public bool Optional
        {
            get { return _optional; }
            set { _optional = value; }
        }

        public ActionNode(string name, string actType, string complexity, string description="", PlanNode parent=null) : base(name, description, parent)
        {
            this._actType = actType;
            this._complexity = complexity;
            this._recipeId = -1;
            this._actState = ActionState.Unknown;
            this._agents = new ArrayList();
            this._params = new ArrayList();
            this._subActions = new ArrayList();
            this._optional = false;
        }

        private CAGA.Dialogue.ActionState _actState;

        public CAGA.Dialogue.ActionState ActState
        {
            get { return _actState; }
            set { _actState = value; }
        }

        public Agent SearchAgent(Agent tempAgent)
        {
            foreach (Agent agent in this._agents)
            {
                if (agent.ID == tempAgent.ID)
                {
                    return agent;
                }
            }
            return null;
        }

        public ArrayList GetParamValue(string paramName)
        {
            foreach (ParamNode param in this._params)
            {
                if (param.Name.ToLower() == paramName.ToLower())
                {
                    return param.Values;
                }
            }
            return new ArrayList();
        }
    }
}
