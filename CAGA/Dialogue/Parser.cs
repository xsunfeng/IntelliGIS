using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CAGA.Dialogue
{
    abstract class  Parser
    {
        public abstract DialogueAct Parse(SortedList speech, SortedList gesture, Agent agent);
    }
}
