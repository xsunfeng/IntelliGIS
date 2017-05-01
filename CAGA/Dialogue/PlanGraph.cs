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
        private Stack<ActionNode> _actionNodeStack;

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
            this._actionNodeStack = new Stack<ActionNode>();
            this._agenda = new ArrayList();
            this._history = new ArrayList();
            this._respList = new ArrayList();
        }

        public Stack<ActionNode> ActionNodeStack
        {
            get { return _actionNodeStack; }
            set { _actionNodeStack = value; }
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

        public RefNode RefParser(string keyword) {
            Hashtable tempAct = this._kbase.SearchAction(keyword);
            ActionNode actionNode = new ActionNode((string)tempAct["name"], (string)tempAct["act_type"], (string)tempAct["complexity"], (string)tempAct["description"]); // act_type, complexity, description, name
            RefNode refNode = new RefNode(actionNode);

            ArrayList recipeList = this._kbase.SearchRecipe(keyword);
            if (recipeList.Count == 0) {
                this._respList.Add(new DialogueResponse(DialogueResponseType.speechError, "The reference node is not in the knowledge base!"));
                return refNode;
            }
            Hashtable recipeInfo = (Hashtable)recipeList[0];
            string recipeXML = recipeInfo["content"].ToString();
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(recipeXML);
            Hashtable _arcmap = new Hashtable();

            string _name = "", _type = "", _layer = "", _attribute = "", _value = "";
            // parse ARCMAP tag
            XmlNodeList paramList = doc.GetElementsByTagName("ARCMAP");
            foreach (XmlNode param in paramList)
            {
                _name = param.Attributes["Name"].Value;
                _type = param.Attributes["Type"].Value;
                _layer = param.Attributes["Layer"].Value;
                Console.WriteLine("name=" + _name + "; type=" + _type + "; layer=" + _layer);
            }

            // parse ARCMAP tag
            paramList = doc.GetElementsByTagName("SelectByAttribute");
            foreach (XmlNode param in paramList)
            {
                _attribute = param.Attributes["Attribute"].Value;
                _value = param.Attributes["Value"].Value;
                Console.WriteLine("attribute=" + _attribute + "; type=" + _value);
            }

            

            refNode.Type = _type;
            refNode.sourceName = _name;
            refNode.sourceLayer = _layer;
            refNode.query = _attribute + " = '" + _value + "'";
            refNode.execute();

            return refNode;

        }

        public bool Explain(DialogueAct dlgAct)
        {
            string indent = "";
            Console.WriteLine(indent + "Dialogue.PlanGraph Explain " + dlgAct.DlgActType);
            bool isExplained = false;
            if (dlgAct.DlgActType == DialogueActType.Intend)
            {
                // search the actions from knowledge base
                // the simplest way: search based on name matching
                ArrayList tempActions = new ArrayList();
                SortedList tmpSortedList = new SortedList();
                foreach (object phrase in dlgAct.SpeechContext.Values){                   
                    if (phrase is string) {
                        Console.WriteLine(indent + "string:" + phrase);
                        Hashtable tempAct = this._kbase.SearchAction((string)phrase);
                        if (tempAct != null)
                        {
                            tempActions.Add(tempAct);
                        }
                    }
                    else if (phrase is SortedList)
                    {
                        foreach (DictionaryEntry item in (SortedList)phrase)
                        {
                            tmpSortedList.Add(item.Key, item.Value);
                            Console.WriteLine(indent + "key:" + item.Key+",value="+item.Value);
                        }                      
                    }
                }
                foreach (DictionaryEntry item in tmpSortedList)
                {
                    dlgAct.SpeechContext.Add(item.Key, item.Value);
                }
                // Explain the actions into the plangraph
                ArrayList explainedActs = new ArrayList();
                foreach (Hashtable tempAct in tempActions)
                {
                    ActionNode explainedAct = this._explainAction(tempAct, dlgAct, indent + "  ");
                    if (explainedAct != null)
                    {
                        explainedActs.Add(explainedAct);
                        AddToAgenda(explainedAct, 0, indent+"  ");
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
                    isExplained = this._explainAnswer(planNode, dlgAct, indent + "  ");
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
                    isExplained = this._explainAffOrNeg(planNode, dlgAct, indent + "  ");
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
                    isExplained = this._explainFeedback(planNode, dlgAct, indent + "  ");
                    if (isExplained == true)
                    {
                        break;
                    }
                }
            }
            else if (dlgAct.DlgActType == DialogueActType.Correct)
            {
                Console.WriteLine("DialogueActType.Correct");

                if (this._actionNodeStack.Count > 0) {
                    ActionNode tmpAction = _actionNodeStack.Peek();
                    Console.WriteLine("preAction=" + tmpAction.Name);
                    foreach (ParamNode param in tmpAction.Params)
                    {
                        Console.WriteLine("param=" + param.Name);
                        Console.WriteLine("DialogueActType.Correct");
                        if ((param.ParamState == ParamState.Ready) && (dlgAct.SpeechContext.ContainsKey(param.Name)))
                        {
                            object newValue = _exec._parseValueFromSpeech(param, dlgAct.SpeechContext[param.Name]);
                            if (newValue != null)
                            {
                                _exec._addValueToParam(param, newValue, indent);
                                isExplained = true;
                            }
                        }
                    }
                    if (isExplained == true)
                    {
                        foreach (ActionNode subAct in tmpAction.SubActions)
                        {
                            Console.WriteLine("subAct=" + subAct.Name);
                            subAct.ActState = ActionState.Executing;
                            AddToAgenda(subAct, 0, indent + "  ");
                        }
                        foreach (PlanNode planNode in this._agenda.ToArray())
                        {
                            if (planNode is ActionNode){
                                ((ActionNode)planNode).ActState = ActionState.Initiated;
                            }
                            if (planNode is ParamNode)
                            {
                                ((ParamNode)planNode).ParamState = ParamState.InPreparation;
                            }
                        }
                    }
                }
            }
            if (isExplained == true)
            {
                this._currDlgAct = dlgAct;
            }

            Console.WriteLine(indent + "Agenda:");
            foreach (PlanNode planNode in this._agenda.ToArray())
            {
                Console.WriteLine(indent + "-" + planNode.Name);
            }
            Console.WriteLine(indent + "IsExplained? " + isExplained);
            return isExplained;
        }

        private bool _explainAnswer(PlanNode planNode, DialogueAct dlgAct, string indent)
        {
            Console.WriteLine(indent + "Dialogue.PlanGraph  _explainAnswer  " + planNode.Name);           
            if (planNode is ActionNode)
            {
                ActionNode actNode = (ActionNode)planNode;
                if (actNode.ActType == "ID")
                {
                    foreach (string phrase in dlgAct.SpeechContext.Keys)
                    {
                        Console.WriteLine("phrase=" + phrase);
                        Console.WriteLine("(ParamNode)actNode=" + actNode.Name);
                        Console.WriteLine("(ParamNode)actNode.Parent=" + ((ParamNode)actNode.Parent).Name);

                        if (phrase.ToLower() == ((ParamNode)actNode.Parent).Name.ToLower())
                        {
                            return true;
                        }
                        if (phrase.ToLower() == ((ParamNode)actNode.Parent).ParamType.ToLower())
                        {
                            return true;
                        }
                    }
                }
            }

            Console.WriteLine(indent + "false");
            return false;
        }

        private bool _explainAffOrNeg(PlanNode planNode, DialogueAct dlgAct, string indent)
        {
            Console.WriteLine(indent + "Dialogue.PlanGraph  _explainAffOrNeg  " + planNode.Name);
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

        private bool _explainFeedback(PlanNode planNode, DialogueAct dlgAct, string indent)
        {
            Console.WriteLine(indent + "Dialogue.PlanGraph  _explainFeedback  " + planNode.Name);
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

        private ActionNode _explainAction(Hashtable tempAct, DialogueAct dlgAct, string indent)
        {
            Console.WriteLine(indent + "Dialogue.PlanGraph  _explainAction  ");
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
                    ActionNode actNode = this._explainActionFromNode(planNode, tempAct, dlgAct, indent+"  ");
                    if (actNode != null)
                    {
                        return actNode;
                    }
                }
            }
            return this._explainActionFromNode(this._root, tempAct, dlgAct, indent + "  ");
        }



        private ActionNode _explainActionFromNode(PlanNode planNode, Hashtable tempAct, DialogueAct dlgAct, string indent)
        {
            Console.WriteLine(indent + "Dialogue.PlanGraph  _explainActionFromNode  " + planNode.Name);
            ActionNode pNode = (ActionNode)planNode;
            ActionNode newAction = new ActionNode((string)tempAct["name"], (string)tempAct["act_type"], (string)tempAct["complexity"], (string)tempAct["description"]);

            if (planNode is ActionNode)
            {
                Console.WriteLine(indent + "IsActionNode  actionNode:" + planNode.Name.ToLower() + "  tempAct" + tempAct["name"].ToString());
                //               ActionNode actionNode = (ActionNode)planNode;
                switch (newAction.ActType)
                {
                    case "ACT":
                        {
                            if (pNode.Name.ToLower() == tempAct["name"].ToString().ToLower())
                            {
                                // If the action has not been initiated, initiate it and add the agent
                                if (pNode.ActState == ActionState.Unknown)
                                {
                                    pNode.ActState = ActionState.Initiated;
                                }
                                if (pNode.SearchAgent(dlgAct.Agent) == null)
                                {
                                    pNode.Agents.Add(dlgAct.Agent);
                                }
                                // If the action has been completed, or failed, start a new one, attached it to the same parent
                                if (pNode.ActState == ActionState.Complete || pNode.ActState == ActionState.Failed)
                                {
                                    //                       ActionNode newAction = new ActionNode((string)tempAct["name"], (string)tempAct["act_type"], (string)tempAct["complexity"], (string)tempAct["description"], actionNode.Parent);
                                    newAction.Parent = pNode.Parent;
                                    newAction.Agents.Add(dlgAct.Agent);
                                    newAction.ActState = ActionState.Initiated;
                                    if (pNode.Parent != null)
                                    {
                                        if (pNode.Parent is ActionNode)
                                        {
                                            //There seems to be a logical error, because the newAction is now added as a subaction of pNode.Parent.  This is equivalent to modifying the recipe for pNode.Parent action.  The correct way to handle this should be:  
                                            // if the tempAct matches with one of the subactions that has not been initiated (potential intention), then replace that subact with newAction
                                            ActionNode pParent = (ActionNode)(pNode.Parent);
                                            foreach (ActionNode subact in pParent.SubActions)
                                                if ((subact.Name == newAction.Name) & (subact.ActState == CAGA.Dialogue.ActionState.Unknown))
                                                {
                                                    ((ActionNode)(pNode.Parent)).SubActions.Add(newAction);
                                                    newAction.Parent = pParent;
                                                    ((ActionNode)(pNode.Parent)).SubActions.Remove(subact);
                                                }


                                        }
                                        else if (pNode.Parent is ParamNode)
                                        {
                                            ((ParamNode)(pNode.Parent)).SubActions.Add(newAction);
                                        }
                                    }
                                    return newAction;
                                }
                                return pNode;
                            }

                            // search the params and subactions
                            foreach (ParamNode paramNode in pNode.Params)
                            {
                                ActionNode actNode = this._explainActionFromNode(paramNode, tempAct, dlgAct, indent + "  ");
                                if (actNode != null)
                                {
                                    return actNode;
                                }
                            }
                            foreach (ActionNode subActNode in pNode.SubActions)
                            {
                                ActionNode actNode = this._explainActionFromNode(subActNode, tempAct, dlgAct, indent + "  ");
                                if (actNode != null)
                                {
                                    return actNode;
                                }
                            }
                            return null;
                        }
                    case "REF":
                        // If the parent is a Action node and the new act is a reference, try to explain it as a parameter of its subaction
                        {
                            //
                            return null;
                        }
                }
                return null;
            }
            else if (planNode is ParamNode)
            {
                switch (newAction.ActType)
                {
                    case "ACT":
                        {
                            ParamNode paramNode = (ParamNode)planNode;
                            foreach (ActionNode subActNode in paramNode.SubActions)
                            {
                                ActionNode actNode = this._explainActionFromNode(subActNode, tempAct, dlgAct, indent + "  ");
                                if (actNode != null)
                                {
                                    return actNode;
                                }
                            }
                            return null;
                        }
                    case "REF":
                        {
                            // explain the REF for potential match with the parameter, if the parent still expecting a parameter (paraStatus=unknown)
                            //retrieval the REF type, if it matches with the Parameter type, move forward
                            // Create a RefNode based on the newAction
                            // RefNode has a parant of planNode
                            // RefNode.execute to calculate the referenced entities and set the parameter of the planNode
                            RefNode newRef = new RefNode(newAction);
                            newRef.parent = (ParamNode)planNode;
                            newRef.execute();
                            return null;
                        }
                }
                return null;
            }
            return null;
        }

        public int AddToAgenda(PlanNode planNode, int index, string indent)
        {
            ﻿Console.WriteLine(indent + "Dialogue.PlanGraph  AddToAgenda  " + planNode.Name);
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

        public int RemoveFromAgenda(PlanNode planNode, string indent)
        {
            Console.WriteLine(indent + "Dialogue.PlanGraph  RemoveFromAgenda  " + planNode.Name);
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
            Console.WriteLine("Dialogue.PlanGraph  Elaborate");
            this._respList.Clear();

            foreach (PlanNode planNode in this._agenda.ToArray())
            {
                this.ElaborateFromNode(planNode, "");
            }

            return this._respList;
        }

        public void ElaborateFromNode(PlanNode planNode, string indent)
        {
            if (planNode is ActionNode)
            {
                this._elaborateFromActionNode((ActionNode)planNode, indent + "  ");
            }
            else if (planNode is ParamNode)
            {
                this._elaborateFromParamNode((ParamNode)planNode, indent + "  ");
            }
        }

        private void _elaborateFromActionNode(ActionNode actionNode, string indent)
        {
            Console.WriteLine(indent + "Dialogue/PlanGraph _elaborateFromActionNode " + actionNode.Name);
            // check the parameters and constraints
            bool paramsRdy = this._parentParamsRdy(actionNode, indent + "  "); // check whether the params are ready
            Console.WriteLine(indent + "paramsRdy = " + paramsRdy);
            if (paramsRdy == false)return;

            // If the action is a basic one and can be executed, then execute it
            if (actionNode.Complexity == "basic")
            {
                Console.WriteLine(indent + actionNode.Name + " is Basic");
                // check the state of the action
                if (actionNode.ActState == ActionState.Initiated || actionNode.ActState == ActionState.Executing)
                {
                    Console.WriteLine(indent + "actionNode.ActState before: " + actionNode.ActState);

                    //// perform the task
                    ArrayList execResp = this._exec.Execute(actionNode, this._currDlgAct, indent + "  ");
                    Console.WriteLine(indent + "");
                    if ((actionNode.Parent != null)&&(actionNode.Parent is ActionNode))_actionNodeStack.Push((ActionNode)actionNode.Parent);

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
                    Console.WriteLine(indent + "actionNode.ActState after: " + actionNode.ActState);
                    // remove the running/complete action from the agenda;
                    if (actionNode.ActState == ActionState.Complete)
                    {
                        this.RemoveFromAgenda(actionNode, indent + "  ");
                    }
                    // check whether the parent's state is also complete if the action is complete
                    if (actionNode.ActState == ActionState.Complete)
                    {
                        this._updateParentState(actionNode, indent + "  ");
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
                                        this.AddToAgenda(subAct, idx + 1, indent + "  ");
                                        this.ElaborateFromNode(subAct, indent + "  ");
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                Console.WriteLine(indent + "Agenda:");
                foreach (PlanNode planNode in this._agenda.ToArray())
                {
                    Console.WriteLine(indent + "-" + planNode.Name);
                }
                return;
            }

            if (actionNode.Complexity == "complex")
            {
                if (_actionNodeStack.Count != 0) {
                    Console.WriteLine(indent + "!!!!!!!!!!!!_prevActionNode is " + _actionNodeStack.Peek().Name);
                    ActionNode tmpAction = _actionNodeStack.Peek();
                    foreach (ParamNode param in tmpAction.Params)
                    {
                        if (param.ParamState==ParamState.Ready)
                        Console.WriteLine(indent + "param " + param.Name+" is "+ param.Values[0].ToString());
                    }
                }              
                Console.WriteLine(indent + actionNode.Name + " is Complex");
               

                // check the state of the action
                if (actionNode.ActState == ActionState.Initiated)
                {
                    // find and load the recipe
                    this._loadRecipe(actionNode, indent + "  ");
                    
                }
                if (actionNode.ActState == ActionState.Planned)
                {
                    int idx = this.RemoveFromAgenda(actionNode, indent + "  ");
                    foreach (ParamNode param in actionNode.Params)
                    {
                        idx = this.AddToAgenda(param, idx, indent + "  ");
                        idx++;
                    }
                    foreach (ActionNode subAct in actionNode.SubActions)
                    {
                        idx = this.AddToAgenda(subAct, idx, indent + "  ");
                        idx++;
                    }
                    Console.WriteLine(indent + "Agenda:");
                    foreach (PlanNode planNode in this._agenda.ToArray())
                    {
                        Console.WriteLine(indent + "-" + planNode.Name);
                    }

                    // elaborate on the params
                    foreach (ParamNode param in actionNode.Params)
                    {
                        param.ParamState = ParamState.InPreparation;
                        this._elaborateFromParamNode(param, indent + "  ");
                    }

                    foreach (ActionNode subAct in actionNode.SubActions)
                    {
                        if (subAct.Optional == false)
                        {
                            
                            subAct.ActState = ActionState.Initiated;
                            this._elaborateFromActionNode(subAct, indent + "  ");
                        }
                        else
                        {
                            this.RemoveFromAgenda(subAct, indent + "  ");
                        }
                    } 
                    
                }
            }
            
        }


        private void _elaborateFromParamNode(ParamNode paramNode, string indent)
        {
            Console.WriteLine(indent + "Dialogue/PlanGraph _elaborateFromParamNode " + paramNode.Name);
            bool paramsRdy = this._parentParamsRdy(paramNode, indent + "  "); // check whether the previous params are ready
            Console.WriteLine(indent + "paramsRdy = " + paramsRdy);
            if (paramsRdy == false)return;

            if (paramNode.ParamState == ParamState.InPreparation)
            {
                if (paramNode.SubActions.Count > 0)
                {
                    int idx = this.RemoveFromAgenda(paramNode, indent + "  ");
                    foreach (ActionNode subAct in paramNode.SubActions)
                    {
                        idx = this.AddToAgenda(subAct, idx, indent + "  ");
                        idx++;
                    }                    
                    foreach (ActionNode subAct in paramNode.SubActions)
                    {
                        // as soon as the parameter is ready, stop the elaboration
                        if (paramNode.ParamState == ParamState.Ready && subAct.Optional == true)
                        {
                            this.RemoveFromAgenda(subAct, indent + "  ");
                        }
                        else
                        {
                            subAct.ActState = ActionState.Initiated;
                            this._elaborateFromActionNode(subAct, indent + "  "); 
                        }
                    }                    
                }
            }
        }

        private void _loadRecipe(ActionNode actionNode, string indent)
        {
            Console.WriteLine(indent + "Dialogue/PlanGraph _loadRecipe " + actionNode.Name);
            ArrayList recipeList = this._kbase.SearchRecipe(actionNode.Name);
            Console.WriteLine("Howmany="+recipeList.Count);

            if (recipeList.Count == 0)
            {
                this._respList.Add(new DialogueResponse(DialogueResponseType.speechError, "There is no recipe for action " + actionNode.Name + " in the knowledge base!"));
                return;
            }
            // todo: select from multiple recipes?
            // load the first one atm
            actionNode.RecipeId = 0;
            actionNode.RecipeList = recipeList;

            Hashtable recipeInfo = (Hashtable)recipeList[0];
            string recipeXMl = recipeInfo["content"].ToString();
            bool parsed = this._parseRecipeXML(recipeXMl, actionNode, indent + "  ");
            // if succeed, set the action state to planned, set the states of children, remove the node from the agenda
            if (parsed == true)
            {
                actionNode.ActState = ActionState.Planned;
            }
        }

        private void _loadNextRecipe(ActionNode actNode)
        {
            int currentRecipeId = actNode.RecipeId;
            Console.WriteLine("currentRecipeId=" + currentRecipeId + ",actNode.RecipeList.Count=" + actNode.RecipeList.Count);
            int newRecipeId = (currentRecipeId + 1) % (actNode.RecipeList.Count);
            Hashtable recipeInfo = (Hashtable)actNode.RecipeList[newRecipeId];
            string recipeXMl = recipeInfo["content"].ToString();
            bool parsed = this._parseRecipeXML(recipeXMl, actNode, "  ");
            /*if (parsed == true)
            {
                actNode.ActState = ActionState.Planned;
            }
            foreach(ParamNode tmpParam in tmpActNode.Params){
                if (tmpParam.ParamState == ParamState.Ready)
                {
                    foreach (ParamNode param in actNode.Params) {
                        if (param.Name == tmpParam.Name) {
                            _exec._addValueToParam(param,tmpParam.Values[0],"");
                        }
                    }
                }
            }
            foreach (ActionNode tmpSubAct in tmpActNode.SubActions)
            {
                foreach (ActionNode subAct in actNode.SubActions)
                {
                    if (subAct.Name == tmpSubAct.Name)
                    {
                        subAct.ActState = tmpSubAct.ActState;
                    }
                }
            }*/
        }

        private bool _parseRecipeXML(string recipeXML, ActionNode actionNode, string indent)
        {
            Console.WriteLine(indent + "Dialogue/PlanGraph _parseRecipeXML " + actionNode.Name);
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

                bool hasParam = false;
                foreach (ParamNode tmpParam in actionNode.Params)
                {
                    //Console.WriteLine(indent + "tmpParam= " + tmpParam.Name + ",paramNode=" + paramNode.Name);
                    if (paramNode.Name == tmpParam.Name)
                    {
                        hasParam = true;
                        paramNode.Flag = true;
                    }
                }
                if (!hasParam) {
                    foreach (XmlNode node in param.ChildNodes)
                    {
                        if (node.Name == "ID_PARAS")
                        {
                            foreach (XmlNode subAct in node.ChildNodes)
                            {
                                if (subAct.Name == "ID_PARA")
                                {
                                    Hashtable tempAct = this._kbase.SearchAction(subAct.Attributes["Name"].Value);
                                    Console.WriteLine("name=" + (string)tempAct["name"]);
                                    //Console.WriteLine("act_type=" + (string)tempAct["act_type"]);
                                    //Console.WriteLine("complexity=" + (string)tempAct["complexity"]);
                                    //Console.WriteLine("description=" + (string)tempAct["description"]);
                                    ActionNode subActNode = new ActionNode((string)tempAct["name"], (string)tempAct["act_type"], (string)tempAct["complexity"], (string)tempAct["description"], paramNode);
                                    if (subAct.Attributes["Optional"] != null && subAct.Attributes["Optional"].Value.ToString().ToLower() == "true")
                                    {
                                        subActNode.Optional = true;
                                    }
                                    paramNode.Flag = true;
                                    paramNode.SubActions.Add(subActNode);
                                }
                            }
                        }
                    }
                    actionNode.Params.Add(paramNode);
                }
            }

            for (int index = actionNode.Params.Count - 1; index >= 0; index--)
            {
                // Get the item.
                ParamNode tmpParam = (ParamNode)actionNode.Params[index];

                // Check to remove.
                if (tmpParam.Flag == false)
                {
                    Console.WriteLine("what deleted is " + tmpParam.Name);
                    // Remove.
                    actionNode.Params.RemoveAt(index);
                }
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

                bool hasAction = false;
                foreach (ActionNode tmpActNode in actionNode.SubActions)
                {
                    Console.WriteLine("tmpActNode.Name=" + tmpActNode.Name + ",subActNode.Name" + subActNode.Name);
                    if (tmpActNode.Name == subActNode.Name)
                    {
                        hasAction = true;
                    }
                }
                if (!hasAction)actionNode.SubActions.Add(subActNode);
            }
            return true;
        }

        private bool _parentParamsRdy(PlanNode planNode, string indent)
        {
            Console.WriteLine(indent + "Dialogue/PlanGraph _parentParamsRdy " + planNode.Name);
            bool paramsRdy = true; // check whether the previous params are ready
            ActionNode actionNode = planNode.Parent as ActionNode;
            if (actionNode != null)
            {
                Console.WriteLine(indent + "Parent: " + actionNode.Name);
                foreach (ParamNode param in actionNode.Params)
                {
                    Console.WriteLine(indent + "param: " + param.Name);
                    if (param.Name != planNode.Name)
                    {
                        Console.WriteLine(indent + "param:" + param.Name+" ParamState:" + param.ParamState);
                        if (param.ParamState != ParamState.Ready)
                        {
                            paramsRdy = false;
                        }
                        foreach (ActionNode subAct in param.SubActions)
                        {
                            Console.WriteLine(indent + "subAct:" + subAct.Name+" ActState:" + subAct.ActState);
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
            else {
                Console.WriteLine(indent + "Parent: is NULL");
            }
            return paramsRdy;
        }

        /// <summary>
        /// When Completed, Update Parent recursively
        /// </summary>
        /// <param name="planNode"></param>
        /// <param name="indent"></param>
        private void _updateParentState(PlanNode planNode, string indent)
        {
            Console.WriteLine(indent + "Dialogue.PlanGraph _updateParentState " + planNode.Name);
            if (planNode.Parent != null)
            {
                Console.WriteLine(indent + "Parent: " + planNode.Parent.Name);
                if (planNode.Parent is ActionNode)
                {
                    ActionNode parent = (ActionNode)planNode.Parent;
                    foreach (ParamNode param in parent.Params)
                    {
                        if (param.ParamState != ParamState.Ready)
                        {
                            Console.WriteLine(indent + "ActionNode Parent not Completed becasue of param " + param.Name + " " + param.ParamState.ToString());
                            return;
                        }
                    }
                    foreach (ActionNode subAct in parent.SubActions)
                    {
                        if (subAct.Optional == false && subAct.ActState != ActionState.Complete)
                        {
                            Console.WriteLine(indent + "ActionNode Parent not Completed becasue of subAct " + subAct.Name + " " + subAct.ActState.ToString() + " Optional=false");
                            return;
                        }
                        if (subAct.Optional == true && subAct.ActState != ActionState.Complete && subAct.ActState != ActionState.Unknown)
                        {
                            Console.WriteLine(indent + "ActionNode Parent not Completed becasue of subAct " + subAct.Name + " " + subAct.ActState.ToString() + " Optional=ture");
                            return;
                        }
                    }
                    parent.ActState = ActionState.Complete;
                    Console.WriteLine(indent + "ActionNode Parent Completed");
                    this._updateParentState(parent, indent + "  ");
                }
                else if (planNode.Parent is ParamNode)
                {
                    ParamNode parent = (ParamNode)planNode.Parent;
                    if (parent.Values.Count == 0)
                    {
                        Console.WriteLine(indent + "ParamNode Parent notReady becasue of 0 value");
                        return;
                    }
                    else {
                        foreach (object obj in parent.Values)
                        {
                            if (obj is string)
                            {
                                Console.WriteLine(indent + "******string******");
                                Console.WriteLine(indent + "obj="+obj);
                            }
                            else if (obj is Hashtable)
                            {
                                Console.WriteLine(indent + "******hashtable******");
                                foreach (DictionaryEntry item in (Hashtable)obj)
                                {
                                    Console.WriteLine(indent + "key=" + item.Key + ",value=" + item.Value);
                                }
                            }
                        }
                    }

                    // assume the parameter is ready as long as it has values
                    // it might be extended to consider the conditions on the parameter
                    parent.ParamState = ParamState.Ready;
                    this._updateParentState(parent, indent + "  ");
                }

            }
        }

    }
}
