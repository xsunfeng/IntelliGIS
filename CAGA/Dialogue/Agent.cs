using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CAGA.Dialogue
{
    class Agent
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

        public Agent(string id="", string name="")
        {
            if (id == "")
            {
                this._id = Utility.GetUniqueKey();
            }
            else 
            {
                this._id = id;
            }
            this._name = name;
        }
    }
}
