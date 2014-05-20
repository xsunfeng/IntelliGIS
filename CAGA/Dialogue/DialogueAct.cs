using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

namespace CAGA.Dialogue
{
    public enum DialogueActType
    {
        Unknown = 0,
        Intend = 1,
        Commit = 2,
        Assert = 3,
        OpenOption = 4,
        Accept = 5,
        Reject = 6,
        WhatQuestion = 7,
        IfQuestion = 8,
        Answer = 9,
        Feedback = 10,
        Correct = 11
    }

    class DialogueAct
    {
        private DialogueActType _dlgActType;

        public DialogueActType DlgActType
        {
            get { return _dlgActType; }
            set { _dlgActType = value; }
        }
        private SortedList _speechContext;

        public SortedList SpeechContext
        {
            get { return _speechContext; }
            set { _speechContext = value; }
        }
        private SortedList _gestureContext;

        public SortedList GestureContext
        {
            get { return _gestureContext; }
            set { _gestureContext = value; }
        }

        public DialogueAct(Agent agent=null, SortedList speechContext = null, SortedList gestureContext = null, DialogueActType dlgActType = DialogueActType.Unknown)
        {
            this._dlgActType = dlgActType;
            this._speechContext = speechContext;
            this._gestureContext = gestureContext;
            this._agent = agent;
        }

        private Agent _agent;

        internal Agent Agent
        {
            get { return _agent; }
            set { _agent = value; }
        }
    }
}
