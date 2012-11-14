using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Xml;

namespace CAGA.Dialogue
{
    class PlanGraph
    {
        private ActionNode _root;
        private SQLiteKBase _kbase;
        private Executor _exec;
        private ArrayList _agenda;  // store the current agenda
        private DialogueAct _currDlgAct;
        private ArrayList _respList;

        public ArrayList Agenda
        {
            get { return _agenda; }
            set { _agenda = value; }
        }
        private ArrayList _history; // store the previous agenda items

        public PlanGraph(SQLiteKBase kbase, Executor exec)
        {
            this._kbase = kbase;
            this._exec = exec;
            this._root = null;
            this._currDlgAct = null;
            this._agenda = new ArrayList();
            this._history = new ArrayList();
            this._respList = new ArrayList();
        }

        public SQLiteKBase Kbase
        {
            get { return _kbase; }
            set { _kbase = value; }
        }

        internal ActionNode Root
        {
            get { return _root; }
            set { _root = value; }
        }

        public void Close()
        {
            this._exec = null;
            this._kbase.Close();
            this._root = null;
            this._currDlgAct = null;
            this._agenda.Clear();
            this._history.Clear();
            this._respList.Clear();
        }

        public bool Explain(DialogueAct dlgAct)
        {
            bool isExplained = false;
            if (dlgAct.DlgActType == DialogueActType.Intend)
            {
                // search the actions from knowledge base
                // the simplest way: search based on name matching
                ArrayList tempActions = new ArrayList();
                foreach (string phrase in dlgAct.SpeechContext.Values)
                {
                    Hashtable tempAct = this._kbase.SearchAction(phrase);
                    if (tempAct != null)
                    {
                        tempActions.Add(tempAct);
                    }
                }

                // Explain the actions into the plangraph
                ArrayList explainedActs = new ArrayList();
                foreach (Hashtable tempAct in tempActions)
                {
                    ActionNode explainedAct = this._explainAction(tempAct, dlgAct);
                    if (explainedAct != null)
                    {
                        explainedActs.Add(explainedAct);
                        AddToAgenda(explainedAct, 0);
                    }
                }
                if (explainedActs.Count > 0)
                {
                    isExplained = true;
                }
            }
            else if (dlgAct.DlgActType == DialogueActType.Answer)
            {
                // the input is an answer to previous question, 
                // explain in the agenda
                foreach (PlanNode planNode in this._agenda.ToArray())
                {
                    isExplained = this._explainAnswer(planNode, dlgAct);
                    if (isExplained == true)
                    {
                        break;
                    }
                }
            }
            else if (dlgAct.DlgActType == DialogueActType.Accept || dlgAct.DlgActType == DialogueActType.Reject)
            {
                // the input is an affirmative or neagtive to previous question, 
                // explain in the agenda
                foreach (PlanNode planNode in this._agenda.ToArray())
                {
                    isExplained = this._explainAffOrNeg(planNode, dlgAct);
                    if (isExplained == true)
                    {
                        break;
                    }
                }
            }
            else if (dlgAct.DlgActType == DialogueActType.Feedback)
            {
                // the input is a feedback to previous action, 
                // explain in the agenda
                foreach (PlanNode planNode in this._agenda.ToArray())
                {
                    isExplained = this._explainFeedback(planNode, dlgAct);
                    if (isExplained == true)
                    {
                        break;
                    }
                }
            }
            if (isExplained == true)
            {
                this._currDlgAct = dlgAct;
            }
            return isExplained;
        }

        private bool _explainAnswer(PlanNode planNode, DialogueAct dlgAct)
        {
            if (planNode is ActionNode)
            {
                ActionNode actNode = (ActionNode)planNode;
                if (actNode.ActType == "ID")
                {
                    foreach (string phrase in dlgAct.SpeechContext.Keys)
                    {
                        if (phrase.ToLower() == ((ParamNode)actNode.Parent).Name.ToLower())
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private bool _explainAffOrNeg(PlanNode planNode, DialogueAct dlgAct)
        {
            if (planNode is ActionNode)
            {
                ActionNode actNode = (ActionNode)planNode;
                if (actNode.ActType == "ID" && actNode.ActState == ActionState.Executing)
                {
                    if (actNode.Parent != null && actNode.Parent is ParamNode)
                    {
                        ParamNode paramNode = actNode.Parent as ParamNode;
                        if (paramNode.ParamType == "boolean")
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private bool _explainFeedback(PlanNode planNode, DialogueAct dlgAct)
        {
            if (planNode is ActionNode)
            {
                ActionNode actNode = (ActionNode)planNode;
                if (actNode.ActState == ActionState.Executing)
                {
                    foreach (string phrase in dlgAct.SpeechContext.Keys)
                    {
                        if (phrase.ToLower() == actNode.Name.ToLower())
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private ActionNode _explainAction(Hashtable tempAct, DialogueAct dlgAct)
        {
            if (this._root == null)
            {
                // todo: check the top actions before assign it to the root node
                this._root = new ActionNode((string)tempAct["name"], (string)tempAct["act_type"], (string)tempAct["complexity"], (string)tempAct["description"], null);
                ((ActionNode)this._root).Agents.Add(dlgAct.Agent);
                ((ActionNode)this._root).ActState = ActionState.Initiated;
                return this._root;
            }
            else if (this._agenda.Count > 0)
            {
                foreach (PlanNode planNode in this._agenda.ToArray())
                {
                    ActionNode actNode = this._explainActionFromNode(planNode, tempAct, dlgAct);
                    if (actNode != null)
                    {
                        return actNode;
                    }
                }
            }
            return this._explainActionFromNode(this._root, tempAct, dlgAct);
        }



        private ActionNode _explainActionFromNode(PlanNode planNode, Hashtable tempAct, DialogueAct dlgAct)
        {
            if (planNode is ActionNode)
            {
                ActionNode actionNode = (ActionNode)planNode;
                if (actionNode.Name.ToLower() == tempAct["name"].ToString().ToLower())
                {
                    // If the action has not been initiated, initiate it and add the agent
                    if (actionNode.ActState == ActionState.Unknown)
                    {
                        actionNode.ActState = ActionState.Initiated;
                    }
                    if (actionNode.SearchAgent(dlgAct.Agent) == null)
                    {
                        actionNode.Agents.Add(dlgAct.Agent);
                    }
                    // If the action has been completed, or failed, start a new one, attached it to the same parent
                    if (actionNode.ActState == ActionState.Complete || actionNode.ActState == ActionState.Failed)
                    {
                        ActionNode newAction = new ActionNode((string)tempAct["name"], (string)tempAct["act_type"], (string)tempAct["complexity"], (string)tempAct["description"], actionNode.Parent);
                        newAction.Agents.Add(dlgAct.Agent);
                        newAction.ActState = ActionState.Initiated;
                        if (actionNode.Parent != null)
                        {
                            if (actionNode.Parent is ActionNode)
                            {
                                ((ActionNode)(actionNode.Parent)).SubActions.Add(newAction);
                            }
                            else if (actionNode.Parent is ParamNode)
                            {
                                ((ParamNode)(actionNode.Parent)).SubActions.Add(newAction);
                            }
                        }
                        return newAction;
                    }
                    return actionNode;
                }

                // search the params and subactions
                foreach (ParamNode paramNode in actionNode.Params)
                {
                    ActionNode actNode = this._explainActionFromNode(paramNode, tempAct, dlgAct);
                    if (actNode != null)
                    {
                        return actNode;
                    }
                }
                foreach (ActionNode subActNode in actionNode.SubActions)
                {
                    ActionNode actNode = this._explainActionFromNode(subActNode, tempAct, dlgAct);
                    if (actNode != null)
                    {
                        return actNode;
                    }
                }
                
            }
            else if (planNode is ParamNode)
            {
                ParamNode paramNode = (ParamNode)planNode;
                foreach (ActionNode subActNode in paramNode.SubActions)
                {
                    ActionNode actNode = this._explainActionFromNode(subActNode, tempAct, dlgAct);
                    if (actNode != null)
                    {
                        return actNode;
                    }
                }   
            }

            return null;
        }

        public int AddToAgenda(PlanNode planNode, int index)
        {
            int currIdx = this._agenda.IndexOf(planNode);
            if (currIdx >= 0)
            {
                if (index > currIdx)
                {
                    this._agenda.RemoveAt(currIdx);
                    this._agenda.Insert(index - 1, planNode);
                    return (index - 1);
                }
                else if (index < currIdx)
                {
                    this._agenda.RemoveAt(currIdx);
                    this._agenda.Insert(index, planNode);
                    return index;
                }
                else
                {
                    return index;
                }
            }
            else 
            {
                this._agenda.Insert(index, planNode);
                return index;
            }
        }

        public int RemoveFromAgenda(PlanNode planNode)
        {
            int index = this._agenda.IndexOf(planNode);
            if (index >= 0)
            {
                this._agenda.RemoveAt(index);
                this._history.Add(planNode);
            }
            return index;
        }

        public int GetIndexInAgenda(PlanNode planNode)
        {
            return this._agenda.IndexOf(planNode);
        }

        public ArrayList Elaborate()
        {
            this._respList.Clear();

            foreach (PlanNode planNode in this._agenda.ToArray())
            {
                this.ElaborateFromNode(planNode);
            }

            return this._respList;
        }

        public void ElaborateFromNode(PlanNode planNode)
        {
            if (planNode is ActionNode)
            {
                this._elaborateFromActionNode((ActionNode)planNode);
            }
            else if (planNode is ParamNode)
            {
                this._elaborateFromParamNode((ParamNode)planNode);
            }
        }

        private void _elaborateFromActionNode(ActionNode actionNode)
        {
            // check the parameters and constraints
            bool paramsRdy = this._parentParamsRdy(actionNode); // check whether the params are ready
            if (paramsRdy == false)
            {
                return;
            }

            // If the action is a basic one and can be executed, then execute it
            if (actionNode.Complexity == "basic")
            {
                // check the state of the action
                if (actionNode.ActState == ActionState.Initiated || actionNode.ActState == ActionState.Executing)
                {
                    

                    // perform the task
                    ArrayList execResp = this._exec.Execute(actionNode, this._currDlgAct);
                    // add the execution response to the response list
                    ArrayList newAgendaItems = new ArrayList();
                    foreach (DialogueResponse resp in execResp)
                    {
                        if (resp.DlgRespType == DialogueResponseType.newAgendaItem)
                        {
                            newAgendaItems.Add(resp.RespContent);
                        }
                        else
                        {
                            this._respList.Add(resp);
                        }
                    }

                    // remove the running/complete action from the agenda;
                    if (actionNode.ActState == ActionState.Complete)
                    {
                        this.RemoveFromAgenda(actionNode);
                    }
                    // check whether the parent's state is also complete if the action is complete
                    if (actionNode.ActState == ActionState.Complete)
                    {
                        this._updateParentState(actionNode);
                    }

                    // perform the new agenda item
                    foreach (string actionName in newAgendaItems)
                    {
                        if (actionName != "")
                        {
                            // only search parent level at the moment
                            ActionNode parentNode = (ActionNode)actionNode.Parent;
                            foreach (ActionNode subAct in parentNode.SubActions)
                            {
                                if (subAct.Name.ToLower() == actionName.ToLower())
                                {
                                    if (subAct.ActState == ActionState.Unknown)
                                    {
                                        subAct.ActState = ActionState.Initiated;
                                        int idx = this.GetIndexInAgenda(actionNode);
                                        this.AddToAgenda(subAct, idx + 1);
                                        this.ElaborateFromNode(subAct);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                return;
            }

            if (actionNode.Complexity == "complex")
            {
                // check the state of the action
                if (actionNode.ActState == ActionState.Initiated)
                {
                    // find and load the recipe
                    this._loadRecipe(actionNode);
                    
                }
                if (actionNode.ActState == ActionState.Planned)
                {
                    int idx = this.RemoveFromAgenda(actionNode);
                    foreach (ParamNode param in actionNode.Params)
                    {
                        idx = this.AddToAgenda(param, idx);
                        idx++;
                    }
                    foreach (ActionNode subAct in actionNode.SubActions)
                    {
                        idx = this.AddToAgenda(subAct, idx);
                        idx++;
                    } 

                    // elaborate on the params
                    foreach (ParamNode param in actionNode.Params)
                    {
                        param.ParamState = ParamState.InPreparation;
                        this._elaborateFromParamNode(param);
                    }

                    foreach (ActionNode subAct in actionNode.SubActions)
                    {
                        if (subAct.Optional == false)
                        {
                            subAct.ActState = ActionState.Initiated;
                            this._elaborateFromActionNode(subAct);
                        }
                        else
                        {
                            this.RemoveFromAgenda(subAct);
                        }
                    } 
                    
                }
            }
            
        }


        private void _elaborateFromParamNode(ParamNode paramNode)
        {
            bool paramsRdy = this._parentParamsRdy(paramNode) ; // check whether the previous params are ready
            if (paramsRdy == false)
            {
                return;
            }

            if (paramNode.ParamState == ParamState.InPreparation)
            {
                if (paramNode.SubActions.Count > 0)
                {
                    int idx = this.RemoveFromAgenda(paramNode);
                    foreach (ActionNode subAct in paramNode.SubActions)
                    {
                        idx = this.AddToAgenda(subAct, idx);
                        idx++;
                    }                    
                    foreach (ActionNode subAct in paramNode.SubActions)
                    {
                        // as soon as the parameter is ready, stop the elaboration
                        if (paramNode.ParamState == ParamState.Ready && subAct.Optional == true)
                        {
                            this.RemoveFromAgenda(subAct);
                        }
                        else
                        {
                            subAct.ActState = ActionState.Initiated;
                            this._elaborateFromActionNode(subAct); 
                        }
                    }                    
                }
            }
        }

        private void _loadRecipe(ActionNode actionNode)
        {
            ArrayList recipeList = this._kbase.SearchRecipe(actionNode.Name);
            if (recipeList.Count == 0)
            {
                this._respList.Add(new DialogueResponse(DialogueResponseType.speechError, "There is no recipe for action " + actionNode.Name + " in the knowledge base!"));
                return;
            }
            // todo: select from multiple recipes?
            // load the first one atm
            Hashtable recipeInfo = (Hashtable)recipeList[0];
            string recipeXMl = recipeInfo["content"].ToString();
            bool parsed = this._parseRecipeXML(recipeXMl, actionNode);
            // if succeed, set the action state to planned, set the states of children, remove the node from the agenda
            if (parsed == true)
            {
                actionNode.ActState = ActionState.Planned;
            }
        }

        private bool _parseRecipeXML(string recipeXML, ActionNode actionNode)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(recipeXML);
            XmlNodeList paramList = doc.GetElementsByTagName("PARA");
            foreach (XmlNode param in paramList)
            {
                string name = param.Attributes["Name"].Value;
                string paramType = param.Attributes["Type"].Value;
                bool multiple = true;
                if (param.Attributes["Multiple"] != null && param.Attributes["Multiple"].Value.ToLower() == "false")
                {
                    multiple = false;
                }
                string description = "";
                if (param.Attributes["Description"] != null)
                {
                    description = param.Attributes["Description"].Value;
                }
                ParamNode paramNode = new ParamNode(name, paramType, multiple, description, actionNode);
                foreach (XmlNode node in param.ChildNodes)
                {
                    if (node.Name == "ID_PARAS")
                    {
                        foreach (XmlNode subAct in node.ChildNodes)
                        {
                            if (subAct.Name == "ID_PARA")
                            {
                                Hashtable tempAct = this._kbase.SearchAction(subAct.Attributes["Name"].Value);
                                ActionNode subActNode = new ActionNode((string)tempAct["name"], (string)tempAct["act_type"], (string)tempAct["complexity"], (string)tempAct["description"], paramNode);
                                if (subAct.Attributes["Optional"] != null && subAct.Attributes["Optional"].Value.ToString().ToLower() == "true")
                                {
                                    subActNode.Optional = true;
                                }
                                paramNode.SubActions.Add(subActNode);
                            }
                        }
                    }
                }
                actionNode.Params.Add(paramNode);
            }
            XmlNodeList actionList = doc.GetElementsByTagName("SUBACT");
            foreach (XmlNode action in actionList)
            {
                Hashtable tempAct = this._kbase.SearchAction(action.Attributes["Name"].Value);
                ActionNode subActNode = new ActionNode((string)tempAct["name"], (string)tempAct["act_type"], (string)tempAct["complexity"], (string)tempAct["description"], actionNode);
                if (action.Attributes["Optional"] != null && action.Attributes["Optional"].Value.ToString().ToLower() == "true")
                {
                    subActNode.Optional = true;
                }
                actionNode.SubActions.Add(subActNode);
            }
            return true;
        }

        private bool _parentParamsRdy(PlanNode planNode)
        {
            bool paramsRdy = true; // check whether the previous params are ready
            ActionNode actionNode = planNode.Parent as ActionNode;
            if (actionNode != null)
            {
                foreach (ParamNode param in actionNode.Params)
                {
                    if (param.Name != planNode.Name)
                    {
                        if (param.ParamState != ParamState.Ready)
                        {
                            paramsRdy = false;
                        }
                        foreach (ActionNode subAct in param.SubActions)
                        {
                            if (subAct.ActState == ActionState.Executing)
                            {
                                paramsRdy = false;
                                break;
                            }
                        }
                        if (paramsRdy == false)
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return paramsRdy;
        }

        private void _updateParentState(PlanNode planNode)
        {
            if (planNode.Parent != null)
            {
                if (planNode.Parent is ActionNode)
                {
                    ActionNode parent = (ActionNode)planNode.Parent;
                    foreach (ParamNode param in parent.Params)
                    {
                        if (param.ParamState != ParamState.Ready)
                        {
                            return;
                        }
                    }
                    foreach (ActionNode subAct in parent.SubActions)
                    {
                        if (subAct.Optional == false && subAct.ActState != ActionState.Complete)
                        {
                            return;
                        }
                        if (subAct.Optional == true && subAct.ActState != ActionState.Complete && subAct.ActState != ActionState.Unknown)
                        {
                            return;
                        }
                    }
                    parent.ActState = ActionState.Complete;
                    this._updateParentState(parent);
                }
                else if (planNode.Parent is ParamNode)
                {
                    ParamNode parent = (ParamNode)planNode.Parent;
                    if (parent.Values.Count == 0)
                    {
                        return;
                    }
                    // assume the parameter is ready as long as it has values
                    // it might be extended to consider the conditions on the parameter
                    parent.ParamState = ParamState.Ready;
                    this._updateParentState(parent);
                }

            }
        }

    }
}
