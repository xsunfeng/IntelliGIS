using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CAGA
{
    class DialogueAgent
    {
        private string baseURI;
        private string dialogueID;
        private string context;
        public bool IsRunning;

        public DialogueAgent(string uri)
        {
            this.baseURI = uri;
            this.IsRunning = false;
        }

        public bool Start(string context)
        {
            this.context = context;
            string uri = baseURI + @"start/";
            string method = "POST";
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
 
            using (JsonWriter jsonWriter = new JsonTextWriter(sw))
            {
                jsonWriter.Formatting = Formatting.Indented;
 
                jsonWriter.WriteStartObject();
                jsonWriter.WritePropertyName("context");
                jsonWriter.WriteValue(this.context);
                jsonWriter.WritePropertyName("participants");
                jsonWriter.WriteStartArray();
                jsonWriter.WriteStartObject();
                jsonWriter.WritePropertyName("name");
                // use the current window user
                jsonWriter.WriteValue(Environment.UserName);
                jsonWriter.WritePropertyName("id");
                // use the computer name + windows user name
                jsonWriter.WriteValue(Environment.MachineName + "-" + Environment.UserName);
                jsonWriter.WriteEndObject();
                jsonWriter.WriteEnd();
                jsonWriter.WriteEndObject();
            }
            string json = sb.ToString();
            
            HttpWebRequest req = WebRequest.Create(uri) as HttpWebRequest;
            req.KeepAlive = false;
            req.Method = method;
            byte[] buffer = Encoding.ASCII.GetBytes(json);
            req.ContentLength = buffer.Length;
            req.ContentType = "application/json";
            Stream PostData = req.GetRequestStream();
            PostData.Write(buffer, 0, buffer.Length);
            PostData.Close();

            HttpWebResponse resp = req.GetResponse() as HttpWebResponse;
            StreamReader tr = new StreamReader(resp.GetResponseStream());
            json = tr.ReadToEnd();            
            Dictionary<string, string> respValues = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            if (respValues.ContainsKey("status") && respValues["status"] == "success")
            {
                this.dialogueID = respValues["dlg_id"];
                this.IsRunning = true;
                return true;
            }
            return false;
        }

        public bool Stop()
        {

            string uri = baseURI + @"stop/?dlg_id=" + this.dialogueID;

            this.IsRunning = false;
            this.dialogueID = "";
            
            HttpWebRequest req = WebRequest.Create(uri) as HttpWebRequest;
            req.KeepAlive = false;
            req.Method = "GET";
            
            HttpWebResponse resp = req.GetResponse() as HttpWebResponse;
            StreamReader tr = new StreamReader(resp.GetResponseStream());
            string json = tr.ReadToEnd();
            Dictionary<string, string> respValues = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            if (respValues.ContainsKey("status") && respValues["status"] == "success")
            {
                return true;
            }
            return false;
        }

        public JObject Update(SortedList speech)
        {
            if (this.IsRunning == false)
            {
                return null;
            }

            string uri = baseURI + @"update/";
            string method = "POST";

            Hashtable sl = new Hashtable();
            sl.Add("dlg_id", this.dialogueID);
            Hashtable s2 = new Hashtable();
            s2.Add("speech", speech);
            sl.Add("message", s2);
            
            string json = JsonConvert.SerializeObject(sl);

            HttpWebRequest req = WebRequest.Create(uri) as HttpWebRequest;
            req.KeepAlive = false;
            req.Method = method;
            byte[] buffer = Encoding.ASCII.GetBytes(json);
            req.ContentLength = buffer.Length;
            req.ContentType = "application/json";
            Stream PostData = req.GetRequestStream();
            PostData.Write(buffer, 0, buffer.Length);
            PostData.Close();

            HttpWebResponse resp = req.GetResponse() as HttpWebResponse;
            StreamReader tr = new StreamReader(resp.GetResponseStream());
            json = tr.ReadToEnd();
            JObject output = JObject.Parse(json);
            
            return output;
        }
    }
}
