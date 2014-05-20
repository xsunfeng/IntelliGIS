using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SQLite;

namespace CAGA.Dialogue
{
    class SimpleParser : Parser
    {
        private ArrayList _prevDlgResp;
        public SimpleParser()
        {
            this._prevDlgResp = null;
        }

        public override DialogueAct Parse(SortedList speech, SortedList gesture, Agent agent)
        {
            Console.WriteLine("Dialogue/SimpleParser: Parse");
            Console.WriteLine("Speech");
            foreach (DictionaryEntry item in speech){
                Console.WriteLine("key=" + item.Key + ", value=" + item.Value);
            }
            DialogueAct dlgAct = null;
            // Test for actions
            if (speech.ContainsKey("intention"))
            {
                dlgAct = new DialogueAct(agent, speech, gesture, DialogueActType.Intend);
                return dlgAct;
            }
            if (speech.ContainsKey("correction"))
            {
                dlgAct = new DialogueAct(agent, speech, gesture, DialogueActType.Correct);
                return dlgAct;
            }
            if (this._prevDlgResp != null && this._prevDlgResp.Count > 0)
            {
                // Test for answer to previous questions
                foreach (DialogueResponse resp in this._prevDlgResp)
                {
                    if (resp.DlgRespType == DialogueResponseType.speechQuestion || resp.DlgRespType == DialogueResponseType.listPlainOptions || resp.DlgRespType == DialogueResponseType.listMapLayerOptions || resp.DlgRespType == DialogueResponseType.listOptionsWithExamples)
                    {
                        if (speech.ContainsKey("affirmative"))
                        {
                            dlgAct = new DialogueAct(agent, speech, gesture, DialogueActType.Accept);
                        }
                        else if (speech.ContainsKey("negative"))
                        {
                            dlgAct = new DialogueAct(agent, speech, gesture, DialogueActType.Reject);
                        }
                        else
                        {
                            dlgAct = new DialogueAct(agent, speech, gesture, DialogueActType.Answer); 
                        }
                        return dlgAct;
                    }
                }

                // Test for feedback to previous actions
                foreach (DialogueResponse resp in this._prevDlgResp)
                {
                    if (resp.DlgRespType == DialogueResponseType.drawPolygonStarted)
                    {
                        dlgAct = new DialogueAct(agent, speech, gesture, DialogueActType.Feedback);
                        return dlgAct;
                    }
                    else if (resp.DlgRespType == DialogueResponseType.selectByAttributes)
                    {
                        dlgAct = new DialogueAct(agent, speech, gesture, DialogueActType.Feedback);
                        return dlgAct;
                    }
                }
            }
            return dlgAct;
        }

        public void SetPrevDlgResponse(ArrayList dlgResp)
        {
            this._prevDlgResp = dlgResp;
        }

    }
}
