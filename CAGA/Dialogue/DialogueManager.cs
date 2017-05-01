using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CAGA.Map;

namespace CAGA.Dialogue
{
    class DialogueManager
    {
        private string _dlgID;
        private string _context;
        private System.Collections.ArrayList _participants;
        private bool _isRunning;
        private SQLiteKBase _kbase;
        private PlanGraph _planGraph;
        private Agent _initiator;
        private Executor _exec;
        private SimpleParser _parser;
        private DialogueAct _currDlgAct;
            
        public string DlgID
        {
            get { return _dlgID; }
            set { _dlgID = value; }
        }
        
        public string Context
        {
            get { return _context; }
            set { _context = value; }
        }
        
        public System.Collections.ArrayList Participants
        {
            get { return _participants; }
            set { _participants = value; }
        }
        
        internal PlanGraph PlanGraph
        {
            get { return _planGraph; }
            set { _planGraph = value; }
        }
 
        public bool IsRunning
        {
            get { return _isRunning; }
            set { _isRunning = value; }
        }

        public Agent Initiator
        {
            get { return _initiator; }
            set { _initiator = value; }
        }

        public DialogueManager(string context, ArcMapManager mapMgr, string kbase)
        {
            this._context = context;
            this._dlgID = Utility.GetUniqueKey();
            this._exec = new Executor(mapMgr);
            this._kbase = new SQLiteKBase(kbase); ;
            this._participants = new ArrayList();            
            this._initiator = null;
            this._parser = new SimpleParser();
        }

        public ArrayList Start()
        {
            Console.WriteLine("Map.DialogueManager: Start");
            Console.WriteLine("contex=" + this._context);
            ArrayList respList = new ArrayList();
            this._planGraph = new PlanGraph(this._kbase, this._exec);
            this._isRunning = true;
            this._currDlgAct = null;

            ArrayList actionList = this._kbase.GetTopActions(this._context);

            if (actionList.Count > 0)
            {
                PlainOptionListData respContent = new PlainOptionListData();
                respContent.Opening = "What can I do for you?";               
                foreach (Hashtable actionInfo in actionList)
                {
                    respContent.AddOption(new PlainOptionItemData(actionInfo["act_name"].ToString(), actionInfo["description"].ToString()));
                }
                respList.Add(new DialogueResponse(DialogueResponseType.listPlainOptions, respContent));
            }
            
            return respList;
        }

        public bool Stop()
        {
            Console.WriteLine("Map.DialogueManager: Stop");
            if (this._planGraph != null)
            {
                this._planGraph.Close();
                this._planGraph = null;
            }
            this._isRunning = false;
            this._currDlgAct = null;
            return true;
        }

        public ArrayList Update(SortedList speech, SortedList gesture=null, Agent agent=null)
        {
            Console.WriteLine("Map.DialogueManager: Update Begin");
            ArrayList respList = new ArrayList();
            if (speech == null && gesture == null)
            {
                respList.Add(new DialogueResponse(DialogueResponseType.speechError, "invalid input"));
                return respList;
            }
            
            if (agent != null)
            {
                agent = this.NewParticipant(agent.ID, agent.Name);
            }
            else 
            {
                if (this._initiator != null)
                {
                    agent = this._initiator;
                }
                else
                {
                    respList.Add(new DialogueResponse(DialogueResponseType.speechError, "There is no agent specified yet!"));
                    return respList;
                }
            }

            // Parse the input to a dialogue act (map one input to one dialogue act)
            this._currDlgAct = this._parser.Parse(speech, gesture, agent);

            // Explain the parsed dialogue act
            if (this._currDlgAct == null)
            {
                respList.Add(new DialogueResponse(DialogueResponseType.speechError, "The input cannot be recognized!"));
                return respList;
            }
            bool isExplained = this._planGraph.Explain(this._currDlgAct);
            this._planGraph.RefParser("Oleander");
            if (isExplained == false)
            {
                respList.Add(new DialogueResponse(DialogueResponseType.speechError, "The input cannot be interpreted!"));
                return respList;
            }


            // Elaborate the plan and process the response
            respList = this._planGraph.Elaborate();
            this._parser.SetPrevDlgResponse(respList);
            Console.WriteLine("Map.DialogueManager: Update End");
            return respList;
        }

        public Agent NewParticipant(string id="", string name="")
        {
            foreach(Agent p in this._participants)
            {
                if (p.ID == id)
                {
                    return p;
                }
            }
            Agent newP = new Agent(id, name);
            this._participants.Add(newP);
            // set the first participant as the initator
            if (this._participants.Count == 1)
            {
                this._initiator = newP;
            }
            
            return newP;
        }


    }
}
