using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SQLite;

namespace CAGA.Dialogue
{
    class SQLiteKBase
    {
        private string _dbFile;

        public string DbFile
        {
            get { return _dbFile; }
            set { _dbFile = value; }
        }
        private SQLiteConnection _connection;
        private DataTable _actionTable;

        
        public SQLiteKBase(string dbFile)
        {
            this._dbFile = dbFile;
            string connStr = "Data Source=" + this._dbFile + ";Version=3;Read Only=True;";
            this._connection = new SQLiteConnection(connStr);
            this._connection.Open();
            _fillActionTable();
        }


        private void _fillActionTable()
        {
            SQLiteCommand cmd = new SQLiteCommand(this._connection);
            cmd.CommandText = "select name, act_type, complexity, description from PlanGraph_action";
            SQLiteDataAdapter da = new SQLiteDataAdapter(cmd);
            this._actionTable = new DataTable();
            da.Fill(this._actionTable);
        }

        public Hashtable SearchAction(string phrase)
        {
            Hashtable actionInfo = null;
            foreach (DataRow row in _actionTable.Rows)
            {
                string act_name = row["name"].ToString();
                if (act_name.ToLower() == phrase.ToLower())
                {
                    actionInfo = new Hashtable();
                    actionInfo.Add("name", act_name);
                    actionInfo.Add("act_type",row["act_type"].ToString());
                    actionInfo.Add("complexity",row["complexity"].ToString());
                    actionInfo.Add("description", row["description"].ToString());                    
                    break;
                }

            }
            return actionInfo;
        }

        public ArrayList SearchRecipe(string actionName)
        {
            ArrayList recipeList = new ArrayList();
            SQLiteCommand cmd = new SQLiteCommand(this._connection);
            cmd.CommandText = "select a.name as act_name, r.id as recipe_id, r.content as content from PlanGraph_action a, PlanGraph_recipe r where a.id=r.action_id and a.name='" + actionName + "';";

            SQLiteDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                Hashtable recipeInfo = new Hashtable();
                recipeInfo.Add("act_name", reader["act_name"]);
                recipeInfo.Add("recipe_id", reader["recipe_id"]);
                recipeInfo.Add("content", reader["content"]);
                recipeList.Add(recipeInfo);
            }
            reader.Close();
            return recipeList;
        }

        public ArrayList GetTopActions(string context)
        {
            ArrayList actionList = new ArrayList();
            SQLiteCommand cmd = new SQLiteCommand(this._connection);
            cmd.CommandText = "select a.name as act_name, a.act_type as act_type, a.complexity as complexity, a.description as description from PlanGraph_action a, PlanGraph_context c, PlanGraph_context_top_actions ca where c.id=ca.context_id and ca.action_id=a.id and c.name='" + context + "';";

            SQLiteDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                Hashtable actionInfo = new Hashtable();
                actionInfo.Add("act_name", reader["act_name"]);
                actionInfo.Add("act_type", reader["act_type"]);
                actionInfo.Add("complexity", reader["complexity"]);
                actionInfo.Add("description", reader["description"]);
                actionList.Add(actionInfo);
            }
            reader.Close();
            return actionList;
        }

        public void Close()
        {
            this._connection.Close();
        }
    }
}
