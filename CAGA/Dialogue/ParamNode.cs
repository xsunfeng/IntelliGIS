using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CAGA.Dialogue
{
    public enum ParamState
    {
        Unknown = 0,
        InPreparation = 1,
        Ready = 2,
        Failed = 3
    }

    class ParamNode : PlanNode
    {
        private string _paramType;

        public string ParamType
        {
            get { return _paramType; }
            set { _paramType = value; }
        }
        private ParamState _paramState;

        public ParamState ParamState
        {
            get { return _paramState; }
            set { _paramState = value; }
        }
        private bool _multiple;

        public bool Multiple
        {
            get { return _multiple; }
            set { _multiple = value; }
        }
        private ArrayList _subActions;

        public ArrayList SubActions
        {
            get { return _subActions; }
            set { _subActions = value; }
        }
        private ArrayList _values;

        public ArrayList Values
        {
            get { return _values; }
            set { _values = value; }
        }

        private bool _flag;
        public bool Flag
        {
            get { return _flag; }
            set { _flag = value; }
        }

        
        public ParamNode(string name, string paramType, bool multiple, string description="", PlanNode parent=null) : base(name, description, parent)
        {
            this._paramType = paramType;
            this._multiple = multiple;
            this._paramState = ParamState.Unknown;
            this._subActions = new ArrayList();
            this._values = new ArrayList();
        }
    }
}
