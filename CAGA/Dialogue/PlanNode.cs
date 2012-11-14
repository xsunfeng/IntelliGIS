using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CAGA.Dialogue
{
    class PlanNode
    {
        private string _id;

        public string ID
        {
            get { return _id; }
            set { _id = value; }
        }
        private string _name;

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        private string _description;

        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }
        private PlanNode _parent;

        internal PlanNode Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }

        public PlanNode(string name, string description="", PlanNode parent=null)
        {
            this._name = name;
            this._description = description;
            this._parent = parent;
        }

    }
}
