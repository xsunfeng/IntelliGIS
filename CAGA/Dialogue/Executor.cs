using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using CAGA.Map;
using System.Speech.Synthesis;
using System.Globalization;


namespace CAGA.Dialogue
{
    class Executor
    {
        private ArcMapManager _mapMgr;
        private SpeechSynthesizer reader;
        public Executor(ArcMapManager mapMgr)
        {
            this._mapMgr = mapMgr;
            reader = new SpeechSynthesizer();
            this.LoadKnowledgeBase();
        }

        public ArrayList Execute(ActionNode actionNode, DialogueAct currDlgAct, string indent)
        {
            Console.WriteLine(actionNode.Name.ToLower());
            ArrayList result = new ArrayList();
            switch (actionNode.Name.ToLower())
            {
                case "get value from input": //2
                    result = this.GetValueFromInput(actionNode, currDlgAct, indent);
                    break;
                case "ask for value": //3
                    result = this.AskForValue(actionNode, currDlgAct, indent);
                    break;
                case "select from candidates": //11
                    result = this.SelectFromCandidates(actionNode, currDlgAct, indent);
                    break;
                case "draw region": //13
                    result = this.DrawRegion(actionNode, currDlgAct, indent);
                    break;
                case "get existing value from ancestor": //14
                    result = this.GetExistingValueFromAncestor(actionNode, currDlgAct, indent);
                    break;
                case "ask for more value": //15
                    result = this.AskForMoreValue(actionNode, currDlgAct, indent);
                    break;
                case "get current map extent": //16
                    result = this.GetCurrentMapExtent(actionNode, currDlgAct, indent);
                    break;
                case "infer value from other parameter": //18
                    result = this.InferValueFromOtherParameter(actionNode, currDlgAct, indent);
                    break;
                case "choose analytic functions": //19
                    result = this.ChooseAnalyticFunctions(actionNode, currDlgAct, indent);
                    break;
                case "perform selection": //20
                    result = this.PerformSelection(actionNode, currDlgAct, indent);
                    break;
                case "calculate field statistics": //23
                    result = this.CalculateFieldStatistics(actionNode, currDlgAct, indent);
                    break;
                case "calculate data summary": //24
                    result = this.CalculateDataSummary(actionNode, currDlgAct, indent);
                    break;
                case "identify region type": //26
                    result = this.IdentifyRegionType(actionNode, currDlgAct, indent);
                    break;
                case "identify symbolization method": //26
                    result = this.IdentifySymbolizationMethod(actionNode, currDlgAct, indent);
                    break;
                case "choose specification method": //27
                    result = this.ChooseSpecificationMethod(actionNode, currDlgAct, indent);
                    break;
                case "choose classification schema": //27
                    result = this.ChooseClassificationSchema(actionNode, currDlgAct, indent);
                    break;
                case "choose symbolization method": //27
                    result = this.ChooseSymbolizationMethod(actionNode, currDlgAct, indent);
                    break;
                case "specify region by attributes": //28
                    result = this.SpecifyRegionByAttributes(actionNode, currDlgAct, indent);
                    break;
                case "specify region by drawing": //29
                    result = this.SpecifyRegionByDrawing(actionNode, currDlgAct, indent);
                    break;
                case "perform symbolization using graduated symbols": //30
                    result = this.PerformSymbolizationUsingGraduatedSymbols(actionNode, currDlgAct, indent);
                    break;
                case "perform symbolization using graduated colors": //30
                    result = this.PerformSymbolizationUsingGraduatedColors(actionNode, currDlgAct, indent);
                    break;
                case "ask for partiality": //31
                    result = this.AskForPartiality(actionNode, currDlgAct, indent);
                    break;
                case "perform overlay": //32
                    result = this.PerformOverlay(actionNode, currDlgAct, indent);
                    break;
                case "draw buffer": //32
                    result = this.DrawBuffer(actionNode, currDlgAct, indent);
                    break;
                case "get value from knowledge base": //32
                    result = this.GetValueFromKnowledgeBase(actionNode, currDlgAct, indent);
                    break;
                case "select the potential hispanic food store costumers": //32
                    result = this.SelectthePotentialHispanicFoodStoreCostumers(actionNode, currDlgAct, indent);
                    break;
                //case "Calculate Service Area":
                //    result = this.CalculateSericeArea(actionNode); 
                default:
                    break;
            }
            return result;
        }


        private ArrayList BasicActionTemplate(ActionNode actionNode, DialogueAct currDlgAct)
        {
            ArrayList respList = new ArrayList();
            // change its own state
            actionNode.ActState = ActionState.Executing;

            // do something:
            respList.Add(new DialogueResponse(DialogueResponseType.debugInfo, "Basic Action: "));

            // change its own state
            actionNode.ActState = ActionState.Complete;
            // generate response 
            return respList;
        }

        private ArrayList GetValueFromKnowledgeBase(ActionNode actionNode, DialogueAct currDlgAct, string indent)
        {
            ArrayList respList = new ArrayList();
            // change its own state
            actionNode.ActState = ActionState.Executing;
            respList.Add(new DialogueResponse(DialogueResponseType.debugInfo, "Basic Action: GetValueFromKnowledgeBase"));
            ParamNode paramNode = (ParamNode)actionNode.Parent;
            if(_knowledgeBase.ContainsKey(paramNode.Name.ToLower()))
            {
                this._addValueToParam(paramNode, _knowledgeBase[paramNode.Name.ToLower()], indent);
                string name = "";
                name += String.Join(" ", paramNode.Name.Split('_'));
                if (_knowledgeBase[paramNode.Name.ToLower()] is string)
                {
                    respList.Add(new DialogueResponse(DialogueResponseType.speechInfo, "The default value of " + name + " is " + _knowledgeBase[paramNode.Name.ToLower()]));
                }
                else if (_knowledgeBase[paramNode.Name.ToLower()] is Hashtable)
                {
                    respList.Add(new DialogueResponse(DialogueResponseType.speechInfo, "The default value of " + name + " is " + ((Hashtable)_knowledgeBase[paramNode.Name.ToLower()])["value"] + " " + ((Hashtable)_knowledgeBase[paramNode.Name.ToLower()])["unit"]));
                }              
            }
            // change its own state
            actionNode.ActState = ActionState.Complete;
            // generate response 
            return respList;
        }
        private Hashtable _knowledgeBase = new Hashtable();
        private void LoadKnowledgeBase()
        {
            Hashtable newValue = new Hashtable();
            newValue.Add("value","30");
            newValue.Add("unit", "miles per hour");
            _knowledgeBase.Add("speed_limit", newValue);

            _knowledgeBase.Add("symbol_size", "12");
        }

        private ArrayList BasicActionTemplate(ActionNode actionNode, DialogueAct currDlgAct, string indent)
        {
            ArrayList respList = new ArrayList();
            // change its own state
            actionNode.ActState = ActionState.Executing;

            // do something:
            respList.Add(new DialogueResponse(DialogueResponseType.debugInfo, "Basic Action: "));
            // change its own state
            actionNode.ActState = ActionState.Complete;
            // generate response 
            return respList;
        }

        private ArrayList GetValueFromInput(ActionNode actionNode, DialogueAct currDlgAct, string indent)
        {
            Console.WriteLine(indent + "Executor.GetValueFromInput actionNode: " + actionNode.Name);
            ArrayList respList = new ArrayList();
            actionNode.ActState = ActionState.Executing;
            respList.Add(new DialogueResponse(DialogueResponseType.debugInfo, "Basic Action: GetValueFromInput"));            
            ParamNode paramNode = (ParamNode)actionNode.Parent;
            Console.WriteLine(indent + "currDlgAct.SpeechContext.Keys");
            foreach (string phrase in currDlgAct.SpeechContext.Keys)
            {
                Console.WriteLine(indent + "phrase:" + phrase);
                if (phrase.ToLower() == paramNode.Name.ToLower())
                {
                    object newValue = _parseValueFromSpeech(paramNode, currDlgAct.SpeechContext[phrase]);
                    Console.WriteLine(indent + "currDlgAct.SpeechContext[phrase]:" + currDlgAct.SpeechContext[phrase]);
                    if (newValue != null)
                    {
                        this._addValueToParam(paramNode, newValue, indent);
                        break;
                    }
                }
            }

            paramNode = (ParamNode)actionNode.Parent;
            foreach (string phrase in currDlgAct.SpeechContext.Keys)
            {
                if (phrase.ToLower() == paramNode.Name.ToLower())
                {
                    object newValue = this._parseValueFromSpeech(paramNode, currDlgAct.SpeechContext[phrase]);
                    if (newValue != null)
                    {
                        // generate response 
                        if (paramNode.ParamType == "data_source")
                        {
                            // fixed at the moment
                            string dataSourcePath = @"C:\GISLAB1\Data";
                            foreach (string value in paramNode.Values)
                            {
                                string filePath = System.IO.Path.Combine(dataSourcePath, value + ".mxd");
                                if (System.IO.File.Exists(filePath))
                                {
                                    respList.Add(new DialogueResponse(DialogueResponseType.mapDocumentOpened, filePath));
                                    respList.Add(new DialogueResponse(DialogueResponseType.speechInfo, "The map of " + value + " is loaded!"));
                                    if (currDlgAct.SpeechContext.ContainsKey("feature_class"))
                                    {
                                        respList.Add(new DialogueResponse(DialogueResponseType.featureLayerInfo, currDlgAct.SpeechContext["feature_class"]));
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            paramNode = (ParamNode)actionNode.Parent;
            foreach (string phrase in currDlgAct.SpeechContext.Keys)
            {
                if (phrase.ToLower() == paramNode.Name.ToLower())
                {
                    object newValue = this._parseValueFromSpeech(paramNode, currDlgAct.SpeechContext[phrase]);
                    if (newValue != null)
                    {
                        // generate response 
                        //if (paramNode.Name == "feature_class")
                        //{
                        //    respList.Add(new DialogueResponse(DialogueResponseType.featureLayerInfo, newValue.ToString()));
                        //}
                    }
                }
            }   

            // change its own state
            actionNode.ActState = ActionState.Complete;
            // generate response 
            return respList;
        }

        private ArrayList AskForValue(ActionNode actionNode, DialogueAct currDlgAct, string indent)
        {
            Console.WriteLine(indent + "Executor.AskForValue actionNode: " + actionNode.Name);

            ArrayList respList = new ArrayList();
           
            // if the action has not been executed, set it to executing, raid the question and return
            if (actionNode.ActState == ActionState.Initiated)
            {
                // change its own state
                actionNode.ActState = ActionState.Executing;

                // do something: raise a question to the user
                respList.Add(new DialogueResponse(DialogueResponseType.debugInfo, "Basic Action: AskForValue"));
                Console.WriteLine(indent + "Ask Question");
                ParamNode paramNode = (ParamNode)actionNode.Parent;
                // only ask if there is no value assigned yet
                if (paramNode.Values.Count == 0){
                    respList.Add(new DialogueResponse(DialogueResponseType.speechQuestion, this._generateQuestionString(paramNode)));
                    // change its own state
                    actionNode.ActState = ActionState.Executing;
                    return respList;
                }else{
                    // change its own state
                    actionNode.ActState = ActionState.Complete;
                    // generate response 
                    return respList;
                }
            }
            // if the action is executing, try to check whether the current input answers the question
            else if (actionNode.ActState == ActionState.Executing)
            {
                ParamNode paramNode = (ParamNode)actionNode.Parent;
                if (currDlgAct.DlgActType == DialogueActType.Answer)
                {
                    Console.WriteLine(indent + "currDlgAct.SpeechContext.Keys");
                    foreach (string phrase in currDlgAct.SpeechContext.Keys)
                    {
                        Console.WriteLine(indent + "phrase:" + phrase);
                        Console.WriteLine("actionNode=" + actionNode.Name);
                        Console.WriteLine("(ParamNode)actionNode.Parent=" + ((ParamNode)actionNode.Parent).Name);
                        Console.WriteLine("(ParamNode)actionNode.Parent.paramtype=" + ((ParamNode)actionNode.Parent).ParamType);
                        if (phrase.ToLower() == paramNode.Name.ToLower() || phrase.ToLower() == paramNode.ParamType.ToLower())
                        {
                            object newValue = this._parseValueFromSpeech(paramNode, currDlgAct.SpeechContext[phrase]);
                            Console.WriteLine(indent + "currDlgAct.SpeechContext["+phrase+"]:" + currDlgAct.SpeechContext[phrase]);
                            if (newValue != null)
                            {
                                this._addValueToParam(paramNode, newValue, indent);
                                // change its own state
                                actionNode.ActState = ActionState.Complete;
                                // generate response 
                                return respList;
                            }                
                        }
                    }
                }
                else if (currDlgAct.DlgActType == DialogueActType.Accept && paramNode.ParamType == "boolean")
                {
                    this._addValueToParam(paramNode, true, indent);
                    respList.Add(new DialogueResponse(DialogueResponseType.debugInfo, paramNode.Name + ": true"));
                    actionNode.ActState = ActionState.Complete;
                    return respList;
                }
                else if (currDlgAct.DlgActType == DialogueActType.Accept && paramNode.ParamType == "boolean")
                {
                    this._addValueToParam(paramNode, true, indent);
                    respList.Add(new DialogueResponse(DialogueResponseType.debugInfo, paramNode.Name + ": false"));
                    actionNode.ActState = ActionState.Complete;
                    return respList;
                }
            }

            // change its own state
            actionNode.ActState = ActionState.Complete;
            // generate response 
            return respList;
        }

        private ArrayList SelectFromCandidates(ActionNode actionNode, DialogueAct currDlgAct, string indent)
        {
            Console.WriteLine(indent + "Executor.SelectFromCandidates actionNode:" + actionNode.Name);
            ArrayList respList = new ArrayList();

            if (actionNode.ActState == ActionState.Initiated)
            {
                // change its own state
                actionNode.ActState = ActionState.Executing;

                // do something: generate the candiate list           
                respList.Add(new DialogueResponse(DialogueResponseType.debugInfo, "Basic Action: SelectFromCandidates"));

                ParamNode paramNode = (ParamNode)actionNode.Parent;

                if (paramNode.Name == "admin_area")
                {
                    // fixed at the moment, future work will search the database to generat the list
                    PlainOptionListData respContent = new PlainOptionListData();
                    respContent.Opening = this._generateQuestionString(paramNode);
                    respContent.AddOption(new PlainOptionItemData("City of Oleander", "Matched feature classes: LandUse"));
                    respContent.AddOption(new PlainOptionItemData("City of Rochester", "Matched feature classes: LandUse, Parcels"));
                    respContent.AddOption(new PlainOptionItemData("City of Baltimore", "Matched feature classes: Parcels, Dwelling units"));
                    respList.Add(new DialogueResponse(DialogueResponseType.listPlainOptions, respContent));
                    return respList;
                }                
            }
            // if the action is executing, try to check whether the current input answers the question
            else if (actionNode.ActState == ActionState.Executing)
            {
                ParamNode paramNode = (ParamNode)actionNode.Parent;
                foreach (string phrase in currDlgAct.SpeechContext.Keys)
                {
                    if (phrase.ToLower() == paramNode.Name.ToLower())
                    {
                        object newValue = this._parseValueFromSpeech(paramNode, currDlgAct.SpeechContext[phrase]);
                        if (newValue != null)
                        {
                            this._addValueToParam(paramNode, newValue, indent);

                            // change its own state
                            actionNode.ActState = ActionState.Complete;

                            // generate response 
                            
                            if (paramNode.ParamType == "data_source")
                            {
                                // fixed at the moment
                                string dataSourcePath = @"C:\GISLAB1\Data";
                                foreach (string value in paramNode.Values)
                                {
                                    string filePath = System.IO.Path.Combine(dataSourcePath, value + ".mxd");
                                    if (System.IO.File.Exists(filePath))
                                    {
                                        respList.Add(new DialogueResponse(DialogueResponseType.mapDocumentOpened, filePath));
                                        respList.Add(new DialogueResponse(DialogueResponseType.speechInfo, "The map of " + value + " is loaded!"));
                                        break;
                                    }
                                }
                            }
                            return respList;
                        }
                    }
                }   
            }

            
            // change its own state
            actionNode.ActState = ActionState.Complete;
            // generate response 
            return respList;
        }


        private ArrayList GetExistingValueFromAncestor(ActionNode actionNode, DialogueAct currDlgAct, string indent)
        {
            Console.WriteLine(indent + "Executor.GetExistingValueFromAncestor actionNode:" + actionNode.Name);
            ArrayList respList = new ArrayList();
            actionNode.ActState = ActionState.Executing;
            respList.Add(new DialogueResponse(DialogueResponseType.debugInfo, "Basic Action: GetExistingValueFromAncestor"));         
            ParamNode paramNode = (ParamNode)actionNode.Parent;
            PlanNode parent = paramNode;
            while (parent != null)
            {
                if (parent.Parent is ParamNode)
                {
                    ParamNode ancestorParam = parent.Parent as ParamNode;
                    if (ancestorParam.ParamType == paramNode.ParamType && ancestorParam.ParamState == ParamState.Ready)
                    {
                        foreach (object value in ancestorParam.Values)
                        {
                            this._addValueToParam(paramNode, value, indent);
                        }
                    }
                }
                else if (parent.Parent is ActionNode)
                {
                    ActionNode ancestorAction = parent.Parent as ActionNode;
                    foreach (ParamNode param in ancestorAction.Params)
                    {
                        if (param != paramNode)
                        {
                            if (param.Name == paramNode.Name && param.ParamState == ParamState.Ready)
                            {
                                foreach (object value in param.Values)
                                {
                                    this._addValueToParam(paramNode, value, indent);
                                }
                            }
                        }
                    }
                }
                parent = parent.Parent;
            }

            // change its own state
            actionNode.ActState = ActionState.Complete;
            // generate response 
            return respList;
        }

        //Add this func 5/21/2014
        private void _updateValueOfAncestor(PlanNode planNode, string paramName, string value)
        {
            Console.WriteLine("更新父辈" + planNode);
            ArrayList matchedValues = new ArrayList();
            if (planNode == null)return;
            PlanNode parent = planNode.Parent;
            while (parent != null)
            {
                if (parent is ParamNode)
                {
                    ParamNode ancestorParam = parent as ParamNode;
                    if (parent.Name.ToLower() == paramName.ToLower() && ancestorParam.ParamState == ParamState.Ready)
                    {
                        Console.WriteLine("parent is ParamNode: " + parent.Name);
                        Console.WriteLine("paramName=" + paramName + ", ancestorParam=" + ancestorParam.Values[0]);
                        ((ParamNode)parent).Values[0] = value;
                        break;
                    }
                }
                else if (parent is ActionNode)
                {
                    ActionNode ancestorAction = parent as ActionNode;
                    foreach (ParamNode param in ancestorAction.Params)
                    {
                        if (param.Name.ToLower() == paramName.ToLower() && param.ParamState == ParamState.Ready)
                        {
                            Console.WriteLine("parent is ActionNode: " + parent.Name);
                            Console.WriteLine("paramName=" + paramName + ", param=" + param.Values[0]);
                            ((ParamNode)param).Values[0] = value;
                            break;
                        }
                    }
                }
                parent = parent.Parent;
            }

            parent = planNode.Parent;
            Console.WriteLine("更新之后");
            while (parent != null)
            {
                if (parent is ParamNode)
                {
                    ParamNode ancestorParam = parent as ParamNode;
                    if (parent.Name.ToLower() == paramName.ToLower() && ancestorParam.ParamState == ParamState.Ready)
                    {
                        Console.WriteLine("parent is ParamNode: " + parent.Name);
                        Console.WriteLine("paramName=" + paramName + ", ancestorParam=" + ancestorParam.Values[0]);
                        break;
                    }
                }
                else if (parent is ActionNode)
                {
                    ActionNode ancestorAction = parent as ActionNode;
                    foreach (ParamNode param in ancestorAction.Params)
                    {
                        if (param.Name.ToLower() == paramName.ToLower() && param.ParamState == ParamState.Ready)
                        {
                            Console.WriteLine("parent is ActionNode: " + parent.Name);
                            Console.WriteLine("paramName=" + paramName + ", param=" + param.Values[0]);
                            break;
                        }
                    }
                }
                parent = parent.Parent;
            }
            return;
        }

        private ArrayList _searchValueFromAncestor(PlanNode planNode, string paramName)
        {
            Console.WriteLine("从先辈那里找" + planNode);
            ArrayList matchedValues = new ArrayList();
            if (planNode == null)
                return matchedValues;
            PlanNode parent = planNode.Parent;
            while (parent != null)
            {
                if (parent is ParamNode)
                {
                    ParamNode ancestorParam = parent as ParamNode;
                    if (parent.Name.ToLower() == paramName.ToLower() && ancestorParam.ParamState == ParamState.Ready)
                    {
                        Console.WriteLine("parent is ParamNode: " + parent.Name);
                        Console.WriteLine("paramName="+paramName+", ancestorParam=" + ancestorParam.Values[0]);
                        matchedValues.AddRange(ancestorParam.Values);
                        break;
                    }
                }
                else if (parent is ActionNode)
                {
                    ActionNode ancestorAction = parent as ActionNode;
                    foreach (ParamNode param in ancestorAction.Params)
                    {
                        if (param.Name.ToLower() == paramName.ToLower() && param.ParamState == ParamState.Ready)
                        {
                            Console.WriteLine("parent is ActionNode: " + parent.Name);
                            Console.WriteLine("paramName=" + paramName + ", param=" + param.Values[0]);
                            matchedValues.AddRange(param.Values);
                            break;
                        }
                    }
                }
                parent = parent.Parent;
            }
            return matchedValues;
        }

        private ArrayList AskForMoreValue(ActionNode actionNode, DialogueAct currDlgAct, string indent)
        {
            Console.WriteLine(indent + "Executor.AskForMoreValue actionNode:" + actionNode.Name);
            ArrayList respList = new ArrayList();

            if (actionNode.ActState == ActionState.Initiated)
            {
                // change its own state
                actionNode.ActState = ActionState.Executing;

                // do something: generate the candiate list           
                respList.Add(new DialogueResponse(DialogueResponseType.debugInfo, "Basic Action: AskForMoreValue"));

                ParamNode paramNode = (ParamNode)actionNode.Parent;

                if (paramNode.Name == "feature_class")
                {
                    // fixed at the moment, future work will search the database to generate the list
                    MapLayerOptionListData respContent = new MapLayerOptionListData();
                    //respContent.Opening = this._generateQuestionString(paramNode);
                    respContent.Opening = "You may want to consider adding the following layers to the map as background:";
                    respContent.AddOption(new MapLayerOptionItemData("Lot boundaries", @"C:\GISLAB1\Data\Oleander\Lot Boundaries.lyr"));
                    respContent.AddOption(new MapLayerOptionItemData("Parcels", @"C:\GISLAB1\Data\Oleander\Parcels.lyr"));
                    respContent.AddOption(new MapLayerOptionItemData("Flood areas", @"C:\GISLAB1\Data\Oleander\MajorRoads.lyr"));
                    
                    respList.Add(new DialogueResponse(DialogueResponseType.listMapLayerOptions, respContent));
                    return respList;
                }
            }
            // if the action is executing, try to check whether the current input answers the question
            else if (actionNode.ActState == ActionState.Executing)
            {
                ParamNode paramNode = (ParamNode)actionNode.Parent;
                foreach (string phrase in currDlgAct.SpeechContext.Keys)
                {
                    if (phrase.ToLower() == paramNode.Name.ToLower())
                    {
                        object newValue = this._parseValueFromSpeech(paramNode, currDlgAct.SpeechContext[phrase]);
                        if (newValue != null)
                        {
                            this._addValueToParam(paramNode, newValue, indent);

                            // change its own state
                            actionNode.ActState = ActionState.Complete;
                            // generate response 
                            if (paramNode.Name == "feature_class")
                            {
                                // fixed at the moment
                                string dataSourcePath = @"C:\GISLAB1\Data\Oleander\";
                                string filePath = System.IO.Path.Combine(dataSourcePath + (string)newValue + ".lyr");
                                if (System.IO.File.Exists(filePath))
                                {
                                    respList.Add(new DialogueResponse(DialogueResponseType.mapLayerAdded, filePath));
                                }
                            }
                            return respList;
                        }
                    }
                }
            }
            
            // change its own state
            actionNode.ActState = ActionState.Complete;
            // generate response 
            return respList;
        }

        private ArrayList IdentifyRegionType(ActionNode actionNode, DialogueAct currDlgAct, string indent)
        {
            Console.WriteLine(indent + "Executor.IdentifyRegionType actionNode:" + actionNode.Name);
            ArrayList respList = new ArrayList();
            if (actionNode.ActState == ActionState.Initiated)
            {
                // change its own state
                actionNode.ActState = ActionState.Executing;

                // do something: generate the candiate list           
                respList.Add(new DialogueResponse(DialogueResponseType.debugInfo, "Basic Action: IdentifyRegionType"));

                ParamNode paramNode = (ParamNode)actionNode.Parent;

                if (paramNode.Name == "region_type")
                {
                    // fixed at the moment
                    OptionWithExampleListData respContent = new OptionWithExampleListData();
                    respContent.Opening = this._generateQuestionString(paramNode);
                    respContent.Opening = "Please describe exactly the region you are interested in. You may choose one of the three methods:";
                    respContent.AddOption(new OptionWithExampleItemData("Draw the region manually on the map", "The region is drawn manually", "/CAGA;component/Images/region_drawing.png"));
                    respContent.AddOption(new OptionWithExampleItemData("Define a region from selection of area features", "The region is a set of areal features", "/CAGA;component/Images/region_attributes.png"));
                    respContent.AddOption(new OptionWithExampleItemData("Define region from neighborhood of selected features", "The region is a buffer zone around some feature", "/CAGA;component/Images/region_buffer.png"));
                    respList.Add(new DialogueResponse(DialogueResponseType.listOptionsWithExamples, respContent));
                    return respList;
                }
            }
            // if the action is executing, try to check whether the current input answers the question
            else if (actionNode.ActState == ActionState.Executing)
            {
                ParamNode paramNode = (ParamNode)actionNode.Parent;
                foreach (string phrase in currDlgAct.SpeechContext.Keys)
                {
                    if (phrase.ToLower() == paramNode.Name.ToLower())
                    {
                        object newValue = this._parseValueFromSpeech(paramNode, currDlgAct.SpeechContext[phrase]);
                        if (newValue != null)
                        {
                            this._addValueToParam(paramNode, newValue, indent);

                            // change its own state
                            actionNode.ActState = ActionState.Complete;
                            return respList;
                        }
                    }
                }
            }

            // change its own state
            actionNode.ActState = ActionState.Complete;
            // generate response 
            return respList;
        }

        private ArrayList IdentifySymbolizationMethod(ActionNode actionNode, DialogueAct currDlgAct, string indent)
        {
            Console.WriteLine(indent + "Executor.IdentifySymbolizationMethod actionNode:" + actionNode.Name);
            ArrayList respList = new ArrayList();
            if (actionNode.ActState == ActionState.Initiated)
            {
                // change its own state
                actionNode.ActState = ActionState.Executing;

                // do something: generate the candiate list           
                respList.Add(new DialogueResponse(DialogueResponseType.debugInfo, "Basic Action: ChooseSymbolizationMethod"));

                ParamNode paramNode = (ParamNode)actionNode.Parent;

                if (paramNode.Name == "symbolization_method")
                {
                    // fixed at the moment
                    OptionWithExampleListData respContent = new OptionWithExampleListData();
                    respContent.Opening = this._generateQuestionString(paramNode);
                    respContent.Opening = "How would you like to display the data:";
                    respContent.AddOption(new OptionWithExampleItemData("Graduated Symbols", "The Quantile is drawn manually", "/CAGA;component/Images/graduated_symbols.PNG"));
                    respContent.AddOption(new OptionWithExampleItemData("Graduated Colors", "The Equal Interval is a set of areal features", "C:\\Users\\fzs122\\Documents\\GitHub\\CAGA\\CAGA\\Images\\graduated_colors.PNG"));
                    respContent.AddOption(new OptionWithExampleItemData("Natural Breaks", "The Natual Break is a buffer zone around some feature", "/CAGA;component/Images/graduated_colors.PNG"));
                    respList.Add(new DialogueResponse(DialogueResponseType.listOptionsWithExamples, respContent));
                    return respList;
                }
            }


            // if the action is executing, try to check whether the current input answers the question
            else if (actionNode.ActState == ActionState.Executing)
            {
                ParamNode paramNode = (ParamNode)actionNode.Parent;
                foreach (string phrase in currDlgAct.SpeechContext.Keys)
                {
                    if (phrase.ToLower() == paramNode.Name.ToLower())
                    {
                        object newValue = this._parseValueFromSpeech(paramNode, currDlgAct.SpeechContext[phrase]);
                        if (newValue != null)
                        {
                            Console.WriteLine((string)newValue);
                            this._addValueToParam(paramNode, newValue, indent);

                            // change its own state
                            actionNode.ActState = ActionState.Complete;
                            return respList;
                        }
                    }
                }
            }

            // change its own state
            actionNode.ActState = ActionState.Complete;
            // generate response 
            return respList;
        }

        private ArrayList ChooseClassificationSchema(ActionNode actionNode, DialogueAct currDlgAct, string indent)
        {
            Console.WriteLine(indent + "Executor.IdentifyRegionType actionNode:" + actionNode.Name);
            ArrayList respList = new ArrayList();
            if (actionNode.ActState == ActionState.Initiated)
            {
                // change its own state
                actionNode.ActState = ActionState.Executing;

                // do something: generate the candiate list           
                respList.Add(new DialogueResponse(DialogueResponseType.debugInfo, "Basic Action: ChooseClassificationSchema"));

                ParamNode paramNode = (ParamNode)actionNode.Parent;

                if (paramNode.Name == "classification_schema")
                {
                    // fixed at the moment
                    OptionWithExampleListData respContent = new OptionWithExampleListData();
                    respContent.Opening = this._generateQuestionString(paramNode);
                    respContent.Opening = "Please choose the classification Schema you would like to use:";
                    respContent.AddOption(new OptionWithExampleItemData("Quantile", "The Quantile is drawn manually", "/CAGA;component/Images/region_drawing.png"));
                    respContent.AddOption(new OptionWithExampleItemData("Equal Interval", "The Equal Interval is a set of areal features", "/CAGA;component/Images/region_attributes.png"));
                    respContent.AddOption(new OptionWithExampleItemData("Natural Breaks", "The Natual Break is a buffer zone around some feature", "/CAGA;component/Images/region_buffer.png"));
                    respList.Add(new DialogueResponse(DialogueResponseType.listOptionsWithExamples, respContent));
                    return respList;
                }
            }
            // if the action is executing, try to check whether the current input answers the question
            else if (actionNode.ActState == ActionState.Executing)
            {
                ParamNode paramNode = (ParamNode)actionNode.Parent;
                foreach (string phrase in currDlgAct.SpeechContext.Keys)
                {
                    Console.WriteLine(phrase + " --- " + currDlgAct.SpeechContext[phrase]);
                    if (phrase.ToLower() == paramNode.Name.ToLower())
                    {
                        object newValue = this._parseValueFromSpeech(paramNode, currDlgAct.SpeechContext[phrase]);
                        if (newValue != null)
                        {
                            this._addValueToParam(paramNode, newValue, indent);

                            // change its own state
                            actionNode.ActState = ActionState.Complete;
                            return respList;
                        }
                    }
                }
            }

            // change its own state
            actionNode.ActState = ActionState.Complete;
            // generate response 
            return respList;
        }

        private ArrayList ChooseSymbolizationMethod(ActionNode actionNode, DialogueAct currDlgAct, string indent)
        {
            Console.WriteLine(indent + "Executor.ChooseSymbolizationMethod actionNode:" + actionNode.Name);
            ArrayList respList = new ArrayList();
            // change its own state
            actionNode.ActState = ActionState.Executing;

            // do something:
            respList.Add(new DialogueResponse(DialogueResponseType.debugInfo, "Basic Action: ChooseSymbolizationMethod"));

            ActionNode parent = (ActionNode)actionNode.Parent;
            if (parent != null)
            {
                foreach (ParamNode param in parent.Params)
                {
                    if (param.ParamType == "symbolization_method" && param.ParamState == ParamState.Ready)
                    {
                        string symbolization_method = param.Values[0].ToString();
                        respList.Add(new DialogueResponse(DialogueResponseType.debugInfo, "symbolization_method:" + symbolization_method));
                        
                        string chosenActionName = "";
                        if (symbolization_method == "graduated symbols")
                        {
                            // select features in the region
                            chosenActionName = "Symbolize using Graduated Symbols";
                        }
                        else if (symbolization_method == "graduated colors")
                        {
                            // select features in the region
                            chosenActionName = "Symbolize using Graduated Colors";
                        }
                        else if (symbolization_method == "charts")
                        {
                            // select features in the region
                            chosenActionName = "Symbolize using Charts";
                        }
                        if (chosenActionName != "")
                        {
                            respList.Add(new DialogueResponse(DialogueResponseType.newAgendaItem, chosenActionName));
                        }

                        // change its own state
                        actionNode.ActState = ActionState.Complete;
                        return respList;
                    }
                }
            }

            // change its own state
            actionNode.ActState = ActionState.Complete;
            // generate response 
            return respList;
        }

        private ArrayList ChooseSpecificationMethod(ActionNode actionNode, DialogueAct currDlgAct, string indent)
        {
            Console.WriteLine(indent + "Executor.ChooseSpecificationMethod actionNode:" + actionNode.Name);
            ArrayList respList = new ArrayList();
            // change its own state
            actionNode.ActState = ActionState.Executing;

            // do something:
            respList.Add(new DialogueResponse(DialogueResponseType.debugInfo, "Basic Action: ChooseSpecificationMethod"));

            ActionNode parent = (ActionNode)actionNode.Parent;
            if (parent != null)
            {
                foreach (ParamNode param in parent.Params)
                {
                    if (param.ParamType == "region_type" && param.ParamState == ParamState.Ready)
                    {
                        string region_type = param.Values[0].ToString();
                        respList.Add(new DialogueResponse(DialogueResponseType.debugInfo, "Region Type:" + region_type));

                        string chosenActionName = ""; 
                        if (region_type == "features")
                        {
                            // select features in the region
                            chosenActionName = "Specify Region By Attributes";
                        }
                        else if (region_type == "drawing")
                        {
                            // select features in the region
                            chosenActionName = "Specify Region By Drawing";
                        }
                        else if (region_type == "buffer")
                        {
                            // select features in the region
                            chosenActionName = "Specify Region By Buffer";
                        }
                        if (chosenActionName != "")
                        {
                            respList.Add(new DialogueResponse(DialogueResponseType.newAgendaItem, chosenActionName));
                        }

                        // change its own state
                        actionNode.ActState = ActionState.Complete;
                        return respList;
                    }
                }
            }

            // change its own state
            actionNode.ActState = ActionState.Complete;
            // generate response 
            return respList;
        }

        private ArrayList SpecifyRegionByAttributes(ActionNode actionNode, DialogueAct currDlgAct, string indent)
        {
            Console.WriteLine(indent + "Executor.SpecifyRegionByAttributes actionNode:" + actionNode.Name);
            ArrayList respList = new ArrayList();

            if (actionNode.ActState == ActionState.Initiated)
            {
                // change its own state
                actionNode.ActState = ActionState.Executing;

                // do something: generate the candiate list           
                respList.Add(new DialogueResponse(DialogueResponseType.debugInfo, "Basic Action: SpecifyRegionByAttributes"));
                respList.Add(new DialogueResponse(DialogueResponseType.speechInfo, "Please specify the region by filtering out the attributes."));
                respList.Add(new DialogueResponse(DialogueResponseType.selectByAttributes, null));
                return respList;
            }
            // if the action is executing, try to check whether the current input answers the question
            else if (actionNode.ActState == ActionState.Executing)
            {
                if (currDlgAct.DlgActType == DialogueActType.Feedback)
                {
                    // find the parameter node from the ancestor
                    PlanNode parent = actionNode.Parent;
                    ParamNode paramNode = null;
                    while (parent != null)
                    {
                        if (parent.Parent is ParamNode)
                        {
                            paramNode = parent.Parent as ParamNode;
                            break;
                        }
                        else if (parent.Parent is ActionNode)
                        {
                            parent = parent.Parent;
                        }
                    }

                    if (paramNode != null)
                    {
                        foreach (string phrase in currDlgAct.SpeechContext.Keys)
                        {
                            if (phrase.ToLower() == actionNode.Name.ToLower())
                            {
                                object newValue = this._parseValueFromSpeech(paramNode, currDlgAct.SpeechContext[phrase]);
                                if (newValue != null)
                                {
                                    Hashtable v = new Hashtable();
                                    v.Add("type", "features");
                                    v.Add("value", newValue);
                                    this._addValueToParam(paramNode, v, indent);

                                    // change its own state
                                    actionNode.ActState = ActionState.Complete;

                                    // generate response 
                                    if (paramNode.ParamType == "geometry_polygon")
                                    {
                                        // fixed at the moment
                                        respList.Add(new DialogueResponse(DialogueResponseType.speechInfo, "The region is identified as a set of features in the layer " + newValue.ToString()));
                                        return respList;
                                    }
                                }
                            }
                        }
                    }

                }
            }

            // change its own state
            actionNode.ActState = ActionState.Complete;
            // generate response 
            return respList;
        }

        private ArrayList SpecifyRegionByDrawing(ActionNode actionNode, DialogueAct currDlgAct, string indent)
        {
            Console.WriteLine(indent + "Executor.SpecifyRegionByDrawing actionNode:" + actionNode.Name);
            ArrayList respList = new ArrayList();
            if (actionNode.ActState == ActionState.Initiated)
            {
                // change its own state
                actionNode.ActState = ActionState.Executing;

                // do something: generate the candiate list           
                respList.Add(new DialogueResponse(DialogueResponseType.debugInfo, "Basic Action: SpecifyRegionByDrawing"));
                respList.Add(new DialogueResponse(DialogueResponseType.speechInfo, "Please adjust the map so that you can see the whole region of your interest."));
                respList.Add(new DialogueResponse(DialogueResponseType.speechInfo, "Ask me for a drawing tool when you are ready."));
                return respList;
            }
            // if the action is executing, try to check whether the current input answers the question
            else if (actionNode.ActState == ActionState.Executing)
            {
                if (currDlgAct.DlgActType == DialogueActType.Intend)
                {
                    respList.Add(new DialogueResponse(DialogueResponseType.debugInfo, "Start drawing the region"));
                    respList.Add(new DialogueResponse(DialogueResponseType.drawPolygonStarted, "Region 1"));
                    respList.Add(new DialogueResponse(DialogueResponseType.speechInfo, "Use the mouse to draw the region, double click when you want to finish drawing."));

                    return respList;
                }
                else if (currDlgAct.DlgActType == DialogueActType.Feedback)
                {
                    // find the parameter node from the ancestor
                    PlanNode parent = actionNode.Parent;
                    ParamNode paramNode = null;
                    while (parent != null)
                    {
                        if (parent.Parent is ParamNode)
                        {
                            paramNode = parent.Parent as ParamNode;
                            break;
                        }
                        else if (parent.Parent is ActionNode)
                        {
                            parent = parent.Parent;
                        }
                    }

                    if (paramNode != null)
                    {
                        Console.WriteLine(indent + "paramNode=" + paramNode.Name);
                        foreach (string phrase in currDlgAct.SpeechContext.Keys)
                        {
                            Console.WriteLine(indent + "phrase=" + phrase);
                            if (phrase.ToLower() == actionNode.Name.ToLower())
                            {
                                object newValue = this._parseValueFromSpeech(paramNode, currDlgAct.SpeechContext[phrase]);
                                if (newValue != null)
                                {
                                    Hashtable v = new Hashtable();
                                    v.Add("type", "drawing");
                                    v.Add("value", newValue);
                                    this._addValueToParam(paramNode, v, indent);
                                    
                                    // change its own state
                                    actionNode.ActState = ActionState.Complete;

                                    // generate response 
                                    if (paramNode.ParamType == "geometry_polygon")
                                    {
                                        // fixed at the moment
                                        respList.Add(new DialogueResponse(DialogueResponseType.speechInfo, "Thanks, you may refer to this region as " + newValue.ToString()));
                                        return respList;
                                    }
                                }
                            }
                        }
                    }

                }
            }

            // change its own state
            actionNode.ActState = ActionState.Complete;
            // generate response 
            return respList;
        }

        //private ArrayList CalculateServiceArea(ActionNode actionNode)
        //{
        //    ActionNode pNode = actionNode.Parent as ActionNode;

        //    foreach (ParamNode param in pNode.Params)
        //        if (param.Name == "Store Locations")
        //        {
        //            IFeatureClass Stores = param.Values.

        //            string str = (string)param.Values[0];
        //            source_layer = str;
        //            Console.WriteLine(indent + "source_layer=" + source_layer);
        //        } 
        //    else 
        //    if (param.ParamType == "length")
        //    {
        //        distString = (string)((Hashtable)param.Values[0])["value"] + " " + (string)((Hashtable)param.Values[0])["unit"];
        //        Console.WriteLine(indent + "distString=" + distString);
        //    }
        //    ParamNode para1 = pNode.Params;

        //}

        private ArrayList DrawBuffer(ActionNode actionNode, DialogueAct currDlgAct, string indent)
        {

            Console.WriteLine(indent + "Executor.DrawBuffer actionNode:" + actionNode.Name);
            ArrayList respList = new ArrayList();

            if (actionNode.ActState == ActionState.Initiated || actionNode.ActState == ActionState.Executing)
            {
                PlanNode parent = actionNode.Parent;
                ParamNode paramNode = null;
                while (parent != null)
                {
                    if (parent.Parent is ParamNode)
                    {
                        paramNode = parent.Parent as ParamNode;
                        break;
                    }
                    else if (parent.Parent is ActionNode)
                    {
                        parent = parent.Parent;
                    }
                }

                actionNode.ActState = ActionState.Executing;
                parent = actionNode.Parent;
                Console.WriteLine(indent + "parent:" + parent.Name);

                //string source_layer = "Fire Stations";
                string source_layer = "";
                string distString = "";
                string feature_class = "";

                foreach (ParamNode param in ((ActionNode)parent).Params)
                {
                    Console.WriteLine(indent + "param.name:" + param.Name);
                    Console.WriteLine(indent + "param.Values[0]:" + param.Values[0]);
                    if (param.Name == "feature_class")
                    {
                        string str = (string)param.Values[0];
                        source_layer = str;
                        Console.WriteLine(indent + "source_layer=" + source_layer);
                    }
                    //if (source_layer_found && distance_found) break;
                    if (param.ParamType == "length")
                    {
                        distString = (string)((Hashtable)param.Values[0])["value"] + " " + (string)((Hashtable)param.Values[0])["unit"];
                        Console.WriteLine(indent + "distString=" + distString);
                    }
                }

                if (source_layer != "" && distString != "")
                {
                    Console.WriteLine(indent + "source_layer=" + source_layer);
                    Console.WriteLine(indent + "distString=" + distString);
                    Console.WriteLine(indent + "feature_class=" + feature_class);

                    string outLayerFile = ((IGeoProcessor)this._mapMgr).Buffer(source_layer, distString);

                    if (outLayerFile.Length > 0)
                    {
                        Console.WriteLine(indent + "outLayerFile=" + outLayerFile);
                        int index = _mapMgr.GetIndexNumberFromLayerName(source_layer);
                        Console.WriteLine(indent + "index=" + index);
                        _mapMgr.AddLayer(outLayerFile, index + 1);
                        //_mapMgr.AddLayer(outLayerFile);
                        //_mapMgr.MoveLayer(source_layer+"_buffer", 1);
                        respList.Add(new DialogueResponse(DialogueResponseType.bufferZoneAdded, outLayerFile));
                        actionNode.ActState = ActionState.Complete;
                        Console.WriteLine(indent + "Complete");

                        Hashtable v = new Hashtable();
                        //v.Add("type", "buffer");
                        v.Add("service", source_layer + "_buffer");
                        v.Add("service_filepath", outLayerFile);
                        v.Add("paramNode name", paramNode.Name);
                        this._addValueToParam(paramNode, v, indent);

                        // change its own state
                        actionNode.ActState = ActionState.Complete;

                        // generate response 
                        if (paramNode.ParamType == "geometry_polygon")
                        {
                            // fixed at the moment
                            respList.Add(new DialogueResponse(DialogueResponseType.speechInfo, "You may refer to this region as " + source_layer + " buffer"));
                            return respList;
                        }

                        // generate response 
                        return respList;
                    }

                }
            }
            // change its own state
            actionNode.ActState = ActionState.Failed;

            // generate response 
            Console.WriteLine(indent + "Failed");
            return respList;
        }

        //private ArrayList DrawBuffer(ActionNode actionNode, DialogueAct currDlgAct, string indent)
        //{

        //    Console.WriteLine(indent + "Executor.DrawBuffer actionNode:" + actionNode.Name);
        //    ArrayList respList = new ArrayList();

        //    if (actionNode.ActState == ActionState.Initiated || actionNode.ActState == ActionState.Executing)
        //    {
        //        PlanNode parent = actionNode.Parent;
        //        ParamNode paramNode = null;
        //        while (parent != null)
        //        {
        //            if (parent.Parent is ParamNode)
        //            {
        //                paramNode = parent.Parent as ParamNode;
        //                break;
        //            }
        //            else if (parent.Parent is ActionNode)
        //            {
        //                parent = parent.Parent;
        //            }
        //        }

        //        actionNode.ActState = ActionState.Executing;
        //        parent = actionNode.Parent;
        //        Console.WriteLine(indent + "parent:" + parent.Name);

        //        //string source_layer = "Fire Stations";
        //        string source_layer = "";
        //        string distString = "";
        //        string feature_class = "";
        //        string source_layer_filter = "";
        //        string speed_limit = "";
        //        string time_limit = "";
        //        bool isDistance = false;
        //        bool isTimeXSpeed = false;

        //        foreach (ParamNode param in ((ActionNode)parent).Params)
        //        {
        //            Console.WriteLine(indent + "param.name:" + param.Name);
        //            Console.WriteLine(indent + "param.Values[0]:" + param.Values[0]);
        //            if (param.Name == "source_layer")
        //            {
        //                string str = (string)param.Values[0];
        //                if (str.StartsWith("Blue"))
        //                {
        //                    string[] tmp = str.Split(' ');
        //                    int len = tmp[0].Length;
        //                    source_layer = str.Substring(len + 1);
        //                    source_layer_filter = "551";
        //                }
        //                else if (str.StartsWith("Red"))
        //                {
        //                    string[] tmp = str.Split(' ');
        //                    int len = tmp[0].Length;
        //                    source_layer = str.Substring(len + 1);
        //                    source_layer_filter = "552";
        //                }
        //                else if (str.StartsWith("Green"))
        //                {
        //                    string[] tmp = str.Split(' ');
        //                    int len = tmp[0].Length;
        //                    source_layer = str.Substring(len + 1);
        //                    source_layer_filter = "553";
        //                }
        //                else
        //                {
        //                    source_layer = str;
        //                }

        //                Console.WriteLine(indent + "source_layer=" + source_layer);
        //            }
        //            //if (source_layer_found && distance_found) break;
        //            if (param.Name == "distance")
        //            {
        //                distString = (string)((Hashtable)param.Values[0])["value"] + " " + (string)((Hashtable)param.Values[0])["unit"];
        //                Console.WriteLine(indent + "distString=" + distString);
        //                isDistance = true;
        //            }
        //            if (param.ParamType == "speed")
        //            {
        //                speed_limit = (string)((Hashtable)param.Values[0])["value"];
        //                Console.WriteLine(indent + "speed_limit=" + speed_limit);
        //                isTimeXSpeed = true;
        //            }
        //            if (param.ParamType == "time")
        //            {
        //                time_limit = (string)((Hashtable)param.Values[0])["value"];
        //                Console.WriteLine(indent + "time_limit=" + time_limit);
        //                isTimeXSpeed = true;
        //            }
        //        }
        //        if (isTimeXSpeed)
        //        {
        //            ;
        //            double a = double.Parse(speed_limit); //mph
        //            double b = double.Parse(time_limit); //min
        //            double c = a * b / 60;
        //            distString = c + " miles";
        //        }

        //        foreach (string key in currDlgAct.SpeechContext.Keys)
        //        {
        //            if (key == "feature_class")
        //            {
        //                _updateValueOfAncestor(actionNode, key, (string)currDlgAct.SpeechContext[key]);
        //                feature_class = (string)currDlgAct.SpeechContext[key];
        //            }
        //        }

        //        if (source_layer != "" && distString != "")
        //        {
        //            Console.WriteLine(indent + "source_layer=" + source_layer);
        //            Console.WriteLine(indent + "distString=" + distString);
        //            Console.WriteLine(indent + "feature_class=" + feature_class);

        //            if (source_layer_filter != "")
        //            {
        //                _mapMgr.SelectFeaturesByAttributes(source_layer, @"""StationNum"" = " + source_layer_filter);
        //            }
        //            Console.WriteLine(indent + "source_layer_filter=" + source_layer_filter);
        //            //_mapMgr.SelectFeaturesByAttributes(source_layer, @"""StationNum"" = 552");
        //            //Console.WriteLine("_mapMgr.GetTotalSelectedFeaturesInLayer=" + _mapMgr.GetTotalSelectedFeaturesInLayer(source_layer));
        //            //_mapMgr.ClearMapSelection();
        //            string outLayerFile = ((IGeoProcessor)this._mapMgr).Buffer(source_layer, distString);

        //            if (outLayerFile.Length > 0)
        //            {
        //                Console.WriteLine(indent + "outLayerFile=" + outLayerFile);
        //                //_mapMgr.AddLayer(outLayerFile);
        //                //_mapMgr.MoveLayer(source_layer+"_buffer", 2);
        //                respList.Add(new DialogueResponse(DialogueResponseType.bufferZoneAdded, outLayerFile));
        //                actionNode.ActState = ActionState.Complete;
        //                Console.WriteLine(indent + "Complete");

        //                Hashtable v = new Hashtable();
        //                v.Add("type", "buffer");
        //                v.Add("source_layer", source_layer + "_buffer");
        //                if (feature_class != "") v.Add("feature_class", feature_class);
        //                this._addValueToParam(paramNode, v, indent);

        //                // change its own state
        //                actionNode.ActState = ActionState.Complete;

        //                // generate response 
        //                if (paramNode.ParamType == "geometry_polygon")
        //                {
        //                    // fixed at the moment
        //                    respList.Add(new DialogueResponse(DialogueResponseType.speechInfo, "You may refer to this region as " + source_layer + " buffer"));
        //                    return respList;
        //                }

        //                // generate response 
        //                return respList;
        //            }

        //        }
        //    }
        //    // change its own state
        //    actionNode.ActState = ActionState.Failed;

        //    // generate response 
        //    Console.WriteLine(indent + "Failed");
        //    return respList;
        //}


        private ArrayList PerformSymbolizationUsingGraduatedSymbols(ActionNode actionNode, DialogueAct currDlgAct, string indent)
        {

            Console.WriteLine(indent + "Executor.PerformSymbolizationUsingGraduatedSymbols actionNode:" + actionNode.Name);
            Console.WriteLine(indent + "actionNode.ActState:" + actionNode.ActState);
            ArrayList respList = new ArrayList();

            if (actionNode.ActState == ActionState.Initiated || actionNode.ActState == ActionState.Executing)
            {
                actionNode.ActState = ActionState.Executing;
                PlanNode parent = actionNode.Parent;
                Console.WriteLine(indent + "parent:" + parent.Name);

                string number_of_class = "";
                string data_field = "";
                string calssification_schema = "";
                string feature_class = "";
                string isLarger = "12";
                string symbolization_method = "";

                foreach (ParamNode param in ((ActionNode)parent).Params)
                {
                    Console.WriteLine(indent + "param.name:" + param.Name);
                    Console.WriteLine(indent + "param.Values[0]:" + param.Values[0]);
                    switch (param.Name) { 
                        case "number_of_class":
                            number_of_class = (string)param.Values[0];
                            Console.WriteLine(indent + "number_of_class:" + number_of_class);
                            break;
                        case "data_field":
                            data_field = (string)param.Values[0];
                            Console.WriteLine(indent + "data_field:" + data_field);
                            break;
                        case "classification_schema":
                            calssification_schema = (string)param.Values[0];
                            Console.WriteLine(indent + "calssification_schema:" + calssification_schema);
                            break;
                        case "feature_class":
                            feature_class = (string)param.Values[0];
                            Console.WriteLine(indent + "feature_class:" + feature_class);
                            break;
                        case "symbol_size":
                            isLarger = (string)param.Values[0];
                            Console.WriteLine(indent + "larger:" + isLarger);
                            break;
                    }
                }
                _mapMgr.DefineClassBreaksRenderer2(feature_class, data_field, Int32.Parse(number_of_class), "none", calssification_schema);
                actionNode.ActState = ActionState.Complete;
                // generate response 
                Console.WriteLine(indent + "Completed");
                return respList;
            }
            // change its own state
            actionNode.ActState = ActionState.Failed;

            // generate response 
            Console.WriteLine(indent + "Failed");
            return respList;
        }

        private ArrayList PerformSymbolizationUsingGraduatedColors(ActionNode actionNode, DialogueAct currDlgAct, string indent)
        {

            Console.WriteLine(indent + "Executor.PerformSymbolizationUsingGraduatedColors actionNode:" + actionNode.Name);
            Console.WriteLine(indent + "actionNode.ActState:" + actionNode.ActState);
            ArrayList respList = new ArrayList();

            if (actionNode.ActState == ActionState.Initiated || actionNode.ActState == ActionState.Executing)
            {
                actionNode.ActState = ActionState.Executing;
                PlanNode parent = actionNode.Parent;
                Console.WriteLine(indent + "parent:" + parent.Name);

                string number_of_class = "";
                string data_field = "";
                string calssification_schema = "";
                string feature_class = "";
                string normalizeField = "none";
                normalizeField = "SqMiles";

                Console.WriteLine(indent + "~~~~~~~~~~~~~~~~~~~~~~~");
                foreach (ParamNode param in ((ActionNode)parent).Params)
                {
                    Console.WriteLine("param.name:" + param.Name);
                    if (param.Values[0] is string){
                        Console.WriteLine((string)param.Values[0]);
                    }else if (param.Values[0] is Hashtable){
                        foreach (DictionaryEntry pair in (Hashtable)param.Values[0]) {
                            Console.WriteLine("key = "+pair.Key+";value = "+ pair.Value);
                        }
                    }
                }
                Console.WriteLine(indent + "~~~~~~~~~~~~~~~~~~~~~~~");

                foreach (ParamNode param in ((ActionNode)parent).Params)
                {
                    Console.WriteLine(indent + "param.name:" + param.Name);
                    Console.WriteLine(indent + "param.Values[0]:" + param.Values[0]);
                    switch (param.Name)
                    {
                        case "number_of_class":
                            number_of_class = (string)param.Values[0];
                            Console.WriteLine(indent + "number_of_class:" + number_of_class);
                            break;
                        case "data_field":
                            data_field = (string)param.Values[0];
                            Console.WriteLine(indent + "data_field:" + data_field);
                            break;
                        case "classification_schema":
                            calssification_schema = (string)param.Values[0];
                            Console.WriteLine(indent + "calssification_schema:" + calssification_schema);
                            break;
                        case "feature_class":
                            feature_class = (string)param.Values[0];
                            Console.WriteLine(indent + "feature_class:" + feature_class);
                            break;            
                            break;
                    }
                }

                PlanNode tmpNode = actionNode.Parent;
                while (tmpNode != null && !(tmpNode is ParamNode)) tmpNode = tmpNode.Parent;
                if (tmpNode != null)
                {
                    Hashtable v = new Hashtable();
                    v.Add("Population Distribution of Hispanic", feature_class);
                    v.Add("data_field", data_field);
                    v.Add("normalizeField", normalizeField);
                    this._addValueToParam((ParamNode)tmpNode, v, indent);
                }   

                _mapMgr.DefineClassBreaksRenderer2(feature_class, data_field, Int32.Parse(number_of_class), normalizeField, calssification_schema);

                actionNode.ActState = ActionState.Complete;
                // generate response 
                Console.WriteLine(indent + "Completed");
                return respList;
            }
            // change its own state
            actionNode.ActState = ActionState.Failed;
            // generate response 
            Console.WriteLine(indent + "Failed");
            return respList;
        }

        private ArrayList AskForPartiality(ActionNode actionNode, DialogueAct currDlgAct, string indent)
        {
            Console.WriteLine(indent + "Executor.AskForPartiality actionNode:" + actionNode.Name);
            ArrayList respList = new ArrayList();

            if (actionNode.ActState == ActionState.Initiated)
            {
                // change its own state
                actionNode.ActState = ActionState.Executing;

                // do something: generate the candiate list           
                respList.Add(new DialogueResponse(DialogueResponseType.debugInfo, "Basic Action: AskForPartiality"));

                ParamNode paramNode = (ParamNode)actionNode.Parent;

                if (paramNode.Name == "partiality")
                {
                    // only respond if the need_summary is true
                    ActionNode parentNode = ((ActionNode)(paramNode.Parent));
                    bool need_summary = false;
                    foreach (ParamNode param in parentNode.Params)
                    {
                        if (param.Name == "need_summary")
                        {
                            if (param.Values.Count > 0)
                            {
                                need_summary = ((bool)param.Values[0]);
                            }
                        }
                    }
                    if (need_summary == true)
                    {
                        // fixed at the moment
                        OptionWithExampleListData respContent = new OptionWithExampleListData();
                        respContent.Opening = this._generateQuestionString(paramNode);
                        respContent.Opening = "Do you want to map them partially or fully inside? The comparison of difference between partially and fully inside are shown in the popup window.";
                        respContent.AddOption(new OptionWithExampleItemData("the parts inside", "Only the parts of features inside the region should be taken into account", "/CAGA;component/Images/partial2.png"));
                        respContent.AddOption(new OptionWithExampleItemData("the features as a whole", "The inside features as a whole should be taken into account", "/CAGA;component/Images/full2.png"));
                        respList.Add(new DialogueResponse(DialogueResponseType.listOptionsWithExamples, respContent));
                        return respList;
                    }                    
                }
            }
            // if the action is executing, try to check whether the current input answers the question
            else if (actionNode.ActState == ActionState.Executing)
            {
                ParamNode paramNode = (ParamNode)actionNode.Parent;
                foreach (string phrase in currDlgAct.SpeechContext.Keys)
                {
                    if (phrase.ToLower() == paramNode.Name.ToLower())
                    {
                        object newValue = this._parseValueFromSpeech(paramNode, currDlgAct.SpeechContext[phrase]);
                        if (newValue != null)
                        {
                            this._addValueToParam(paramNode, newValue, indent);

                            // change its own state
                            actionNode.ActState = ActionState.Complete;
                            return respList;
                        }
                    }
                }
            }

            // change its own state
            actionNode.ActState = ActionState.Complete;
            // generate response 
            return respList;
        }

        private ArrayList SelectthePotentialHispanicFoodStoreCostumers(ActionNode actionNode, DialogueAct currDlgAct, string indent)
        {
            Console.WriteLine(indent + "Executor.SelectthePotentialHispanicFoodStoreCostumers actionNode:" + actionNode.Name);
            ArrayList respList = new ArrayList();
            // change its own state
            actionNode.ActState = ActionState.Executing;

            // do something:
            string service = "";
            string PopulationDistributionofHispanic = "";
            float value = 0;
            string compare = "";
            string data_field = "";
            string normalizeField = "";
            string service_filepath = "";

            if (actionNode.Parent is ActionNode) {
                
                ActionNode parent = (ActionNode)actionNode.Parent;
                foreach (ParamNode obj in parent.Params)
                {
                    if(obj.Values[0] is Hashtable){
                        foreach (DictionaryEntry item in (Hashtable)obj.Values[0])
                        {
                            Console.WriteLine(indent + "key=" + item.Key + ",value=" + item.Value);
                        }
                    }
                    else Console.WriteLine(indent + "string:" + obj.Values[0].ToString());
                
                }
                foreach ( ParamNode param in parent.Params)
                {
                    //Console.WriteLine("叭叭叭" + param.Name);
                    if (param.Name == "service")
                    {
                        service = (string)((Hashtable)param.Values[0])["service"];
                        service_filepath = (string)((Hashtable)param.Values[0])["service_filepath"];
                        Console.WriteLine(indent + "******叭叭叭******");
                        Console.WriteLine(indent + "service=" + service);
                        Console.WriteLine(indent + "service_filepath=" + service_filepath);
                    }
                    if(param.Name == "Population Distribution of Hispanic"){
                        PopulationDistributionofHispanic = (string)((Hashtable)param.Values[0])["Population Distribution of Hispanic"];
                        data_field = (string)((Hashtable)param.Values[0])["data_field"];
                        normalizeField = (string)((Hashtable)param.Values[0])["normalizeField"];
                        Console.WriteLine(indent + "******叭叭叭******");
                        Console.WriteLine(indent + "PopulationDistributionofHispanic=" + PopulationDistributionofHispanic);
                        Console.WriteLine(indent + "data_field=" + data_field);
                        Console.WriteLine(indent + "normalizeField=" + normalizeField);
                    }
                    if(param.Name == "criteria"){
                        if((string)((Hashtable)param.Values[0])["compare"]=="1")compare = ">";
                        if((string)((Hashtable)param.Values[0])["compare"]=="-1")compare = "<";
                        value = float.Parse((string)(((Hashtable)param.Values[0])["value"]))/100;
                        Console.WriteLine(indent + "******叭叭叭******");
                        Console.WriteLine(indent + "value="+value);
                    }
                }

                //string whereClause = data_field + compare + normalizeField + "*" + value.ToString();
                //Console.WriteLine(indent + whereClause);
                //_mapMgr.SelectFeaturesByAttributes(PopulationDistributionofHispanic, whereClause);
                //_mapMgr.CreateLayerFromSel(PopulationDistributionofHispanic, PopulationDistributionofHispanic + "_query");

                //ArrayList inLayerNames = new ArrayList();
                //inLayerNames.Add(service);
                //inLayerNames.Add(service);
                ////inLayerNames.Add(needs + "_query");
                //this._mapMgr.SelectFeaturesByLocation(needs + "_query", service, "UNION");


                string dataSourcePath = @"C:\GISLAB1\Data\";
                string filePath = System.IO.Path.Combine(dataSourcePath + "Census_Counts_Union selection" + ".lyr");
                if (System.IO.File.Exists(filePath))
                {
                    _mapMgr.AddLayer(filePath, 1);
                }
                _mapMgr.SelectFeaturesByAttributes("Census_Counts_Union selection", "");
                
                //_mapMgr.Overlay(inLayerNames, "Potential_Hispanic_Food_Store_Sites", "UNION");
                //_mapMgr.CreateLayerFromSel(needs + "_query", "result");
                //whereClause = "FID_" + service+"<0";
                //_mapMgr.ClearMapSelection();
                //_mapMgr.SelectFeaturesByAttributes("result", whereClause);


                //whereClause
                //_mapMgr.SelectFeaturesByAttributes(needs, whereClause);
               

                //object newValue = this._mapMgr.GetMapExtent();
                //this._addValueToParam(paramNode, newValue, indent);
                //respList.Add(new DialogueResponse(DialogueResponseType.debugInfo, "Basic Action: SelectthePotentialHispanicFoodStoreSites"));           
            }
            // change its own state
            actionNode.ActState = ActionState.Complete;
            // generate response 
            return respList;
        }

        private ArrayList GetCurrentMapExtent(ActionNode actionNode, DialogueAct currDlgAct, string indent)
        {
            Console.WriteLine(indent + "Executor.GetCurrentMapExtent actionNode:" + actionNode.Name);
            ArrayList respList = new ArrayList();
            // change its own state
            actionNode.ActState = ActionState.Executing;

            // do something:
            ParamNode paramNode = (ParamNode)actionNode.Parent;
            object newValue = this._mapMgr.GetMapExtent();
            this._addValueToParam(paramNode, newValue, indent);
            respList.Add(new DialogueResponse(DialogueResponseType.debugInfo, "Basic Action: GetCurrentMapExtent"));

            // change its own state
            actionNode.ActState = ActionState.Complete;
            // generate response 
            return respList;
        }

        private ArrayList DrawRegion(ActionNode actionNode, DialogueAct currDlgAct, string indent)
        {
            Console.WriteLine(indent + "Executor.DrawRegion actionNode:" + actionNode.Name);
            ArrayList respList = new ArrayList();

            if (actionNode.ActState == ActionState.Initiated)
            {
                // change its own state
                actionNode.ActState = ActionState.Executing;

                // do something: generate the candiate list           
                respList.Add(new DialogueResponse(DialogueResponseType.debugInfo, "Basic Action: DrawRegion"));
                respList.Add(new DialogueResponse(DialogueResponseType.speechInfo, "Please adjust the map so that you can see the whole region of your interest."));
                respList.Add(new DialogueResponse(DialogueResponseType.speechInfo, "Ask me for a drawing tool when you are ready."));
                return respList;
            }
            // if the action is executing, try to check whether the current input answers the question
            else if (actionNode.ActState == ActionState.Executing)
            {
                if (currDlgAct.DlgActType == DialogueActType.Intend)
                {
                    respList.Add(new DialogueResponse(DialogueResponseType.debugInfo, "Start drawing the region"));
                    respList.Add(new DialogueResponse(DialogueResponseType.drawPolygonStarted, "Region 1"));
                    respList.Add(new DialogueResponse(DialogueResponseType.speechInfo, "Use the mouse to draw the region, double click when you want to finish drawing."));
                    
                    return respList;
                }
                else if (currDlgAct.DlgActType == DialogueActType.Feedback)
                {
                    // find the parameter node from the ancestor
                    PlanNode parent = actionNode.Parent;
                    ParamNode paramNode = null;
                    while (parent != null)
                    {
                        if (parent.Parent is ParamNode)
                        {
                            paramNode = parent.Parent as ParamNode;
                            break;
                        }
                        else if (parent.Parent is ActionNode)
                        {
                            parent = parent.Parent;
                        }
                    }

                    if (paramNode != null)
                    {
                        foreach (string phrase in currDlgAct.SpeechContext.Keys)
                        {
                            if (phrase.ToLower() == actionNode.Name.ToLower())
                            {
                                object newValue = this._parseValueFromSpeech(paramNode, currDlgAct.SpeechContext[phrase]);
                                if (newValue != null)
                                {
                                    this._addValueToParam(paramNode, newValue, indent);

                                    // change its own state
                                    actionNode.ActState = ActionState.Complete;

                                    // generate response 
                                    if (paramNode.ParamType == "geometry_polygon")
                                    {
                                        // fixed at the moment
                                        respList.Add(new DialogueResponse(DialogueResponseType.speechInfo, "Thanks, you may refer to this region as " + newValue.ToString()));
                                        return respList;
                                    }
                                }
                            }
                        }
                    }
                    
                }
            }
            
            // change its own state
            actionNode.ActState = ActionState.Complete;
            // generate response 
            return respList;
        }

        private ArrayList InferValueFromOtherParameter(ActionNode actionNode, DialogueAct currDlgAct, string indent)
        {
            Console.WriteLine(indent + "Executor.InferValueFromOtherParameter actionNode:" + actionNode.Name);
            ArrayList respList = new ArrayList();
            // change its own state
            actionNode.ActState = ActionState.Executing;

            // do something:
            respList.Add(new DialogueResponse(DialogueResponseType.debugInfo, "Basic Action: InferValueFromOtherParameter"));
            ParamNode paramNode = (ParamNode)actionNode.Parent;
            string [] paramNames = paramNode.Name.Split('.');
            if (paramNames.Length > 1 && paramNames[1] == "multiple" && paramNode.ParamType == "boolean")
            {
                PlanNode parent = paramNode;
                string ancestorParamName = paramNames[0];
                ParamNode matchedParam = null;
                while (parent != null)
                {
                    if (parent.Parent is ParamNode)
                    {
                        ParamNode ancestorParam = parent.Parent as ParamNode;
                        if (ancestorParam.Name.ToLower() == ancestorParamName.ToLower() && ancestorParam.ParamState == ParamState.Ready)
                        {
                            matchedParam = ancestorParam;
                            break;
                        }
                    }
                    else if (parent.Parent is ActionNode)
                    {
                        ActionNode ancestorAction = parent.Parent as ActionNode;
                        foreach (ParamNode ancestorParam in ancestorAction.Params)
                        {
                            if (ancestorParam.Name.ToLower() == ancestorParamName.ToLower() && ancestorParam.ParamState == ParamState.Ready)
                            {
                                matchedParam = ancestorParam;
                                break;
                            }
                        }
                        if (matchedParam != null)
                        {
                            break;
                        }
                    }
                    parent = parent.Parent;
                }

                if (matchedParam != null)
                {
                    if (matchedParam.Values.Count > 1)
                    {
                        this._addValueToParam(paramNode, true, indent);
                        respList.Add(new DialogueResponse(DialogueResponseType.debugInfo, paramNode.Name + ": true"));
                    }
                    else if (matchedParam.Values.Count == 1)
                    {
                        this._addValueToParam(paramNode, false, indent);
                        respList.Add(new DialogueResponse(DialogueResponseType.debugInfo, paramNode.Name + ": false"));
                    }
                }
            }

            // change its own state
            actionNode.ActState = ActionState.Complete;
            // generate response 
            return respList;
        }

        private ArrayList ChooseAnalyticFunctions(ActionNode actionNode, DialogueAct currDlgAct, string indent)
        {
            Console.WriteLine(indent + "Executor.ChooseAnalyticFunctions actionNode:" + actionNode.Name);

            ArrayList respList = new ArrayList();
            // change its own state
            actionNode.ActState = ActionState.Executing;

            // do something:
            respList.Add(new DialogueResponse(DialogueResponseType.debugInfo, "Basic Action: ChooseAnalyticFunctions"));
            ActionNode parentNode = ((ActionNode)(actionNode.Parent));
            bool multiple = false;
            bool need_summary = false;
            bool partiality = false;
            foreach (ParamNode param in parentNode.Params)
            {
                if (param.Name == "region.multiple")
                {
                    if (param.Values.Count > 0)
                    {
                        multiple = ((bool)param.Values[0]);
                    }
                }
                else if (param.Name == "need_summary")
                {
                    if (param.Values.Count > 0)
                    {
                        need_summary = ((bool)param.Values[0]);
                    }
                }
                else if (param.Name == "partiality")
                {
                    if (param.Values.Count > 0)
                    {
                        partiality = ((bool)param.Values[0]);
                    }
                }

            }
            string chosenActionName = ""; 
            if (need_summary == true)
            {
                if (partiality == false)
                {
                    // select features in the region
                    chosenActionName = "Select Features Inside Region";
                }
                else
                {
                    // overlay features with the region
                    chosenActionName = "Overlay Features and Region";
                }
            }
            respList.Add(new DialogueResponse(DialogueResponseType.newAgendaItem, chosenActionName));
            
            // change its own state
            actionNode.ActState = ActionState.Complete;
            // generate response 
            return respList;
        }

        private ArrayList PerformSelection(ActionNode actionNode, DialogueAct currDlgAct, string indent)
        {
            Console.WriteLine(indent + "Executor.PerformSelection actionNode:" + actionNode.Name);

            ArrayList respList = new ArrayList();
            // change its own state
            actionNode.ActState = ActionState.Executing;

            // do something:
            respList.Add(new DialogueResponse(DialogueResponseType.debugInfo, "Basic Action: PerformSelection"));

            ActionNode parentNode = ((ActionNode)(actionNode.Parent));
            string featureClass = "";
            Hashtable region = null;

            foreach (ParamNode param in parentNode.Params)
            {
                if (param.ParamState == ParamState.Ready && param.ParamType == "geometry_polygon")
                {
                    Console.WriteLine(indent + "**************************************************");
                    Console.WriteLine(indent + param.Name + " " + param.ParamType);
                    Console.WriteLine(indent + "**************************************************");
                    region = param.Values[0] as Hashtable;
                    foreach (DictionaryEntry item in region)
                    {
                        Console.WriteLine(indent + "key=" + item.Key+",value="+item.Value);
                    }
                }
                else if (param.ParamState == ParamState.Ready && param.ParamType == "feature_class")
                {
                    Console.WriteLine(indent + "**************************************************");
                    Console.WriteLine(indent + param.Name + " " + param.ParamType);
                    Console.WriteLine(indent + "**************************************************");
                    featureClass = param.Values[0].ToString();
                    Console.WriteLine(indent + "string=" + featureClass);
                }
            }

            
            if (region != null && featureClass != "")
            {
                featureClass = String.Join(" ", featureClass.Split('_'));
                if (region["type"].ToString() == "drawing")
                {
                    string graphicsName = region["value"].ToString();
                    Console.WriteLine(indent + "graphicsName=" + graphicsName);
                    this._mapMgr.SelectFeaturesByGraphics(graphicsName);
                    respList.Add(new DialogueResponse(DialogueResponseType.speechInfo, "The " + featureClass + " within " + graphicsName + " are highlighted in the map!"));
                    int count = this._mapMgr.GetTotalSelectedFeaturesInLayer(featureClass);
                    respList.Add(new DialogueResponse(DialogueResponseType.speechInfo, "There are " + count + " " + featureClass + " selected."));
                    respList.Add(new DialogueResponse(DialogueResponseType.debugInfo, "selecting by drawing"));
                }
                else if (region["type"].ToString() == "features")
                {                    
                    string in_layer = featureClass;
                    string select_features = region["value"].ToString();
                    Console.WriteLine(indent + "in_layer=" + in_layer + " select_features=" + select_features);
                    this._mapMgr.SelectFeaturesByLocation(in_layer, select_features);
                    respList.Add(new DialogueResponse(DialogueResponseType.speechInfo, "The " + featureClass + " within " + select_features + "are highlighted in the map!"));
                    int count = this._mapMgr.GetTotalSelectedFeaturesInLayer(featureClass);
                    respList.Add(new DialogueResponse(DialogueResponseType.speechInfo, "There are " + count + " " + featureClass + " selected."));
                    respList.Add(new DialogueResponse(DialogueResponseType.debugInfo, "selecting by attributes"));
                }
                else if (region["type"].ToString() == "buffer")
                {
                    string feature_class = featureClass;
                    if (region.ContainsKey("feature_class"))
                    {
                        feature_class = region["feature_class"].ToString();
                    }
                    string source_layer = region["source_layer"].ToString();
                    Console.WriteLine(indent + "source_layer=" + source_layer + " feature_class=" + feature_class);
                    this._mapMgr.SelectFeaturesByLocation(feature_class, source_layer);
                    string[] tmp = source_layer.Split('_');
                    respList.Add(new DialogueResponse(DialogueResponseType.speechInfo, "The " + feature_class + " within " + tmp[0]+" "+tmp[1] + " are highlighted in the map!"));
                    int count = this._mapMgr.GetTotalSelectedFeaturesInLayer(feature_class);
                    respList.Add(new DialogueResponse(DialogueResponseType.speechInfo, "There are " + count + " selected."));
                    respList.Add(new DialogueResponse(DialogueResponseType.debugInfo, "selecting by buffer"));
                }

            }

            // change its own state
            actionNode.ActState = ActionState.Complete;
            // generate response 
            return respList;
        }


        private ArrayList PerformOverlay(ActionNode actionNode, DialogueAct currDlgAct, string indent)
        {
            Console.WriteLine(indent + "Executor.PerformOverlay actionNode:" + actionNode.Name);

            ArrayList respList = new ArrayList();
            // change its own state
            actionNode.ActState = ActionState.Executing;

            // do something:
            respList.Add(new DialogueResponse(DialogueResponseType.debugInfo, "Basic Action: PerformOverlay"));

            ParamNode paramNode = (ParamNode)actionNode.Parent;
            ActionNode parentNode = ((ActionNode)(actionNode.Parent.Parent));
            string featureClass = "";
            Hashtable region = null;

            foreach (ParamNode param in parentNode.Params)
            {
                if (param.ParamState == ParamState.Ready && param.Name == "region")
                {
                    region = param.Values[0] as Hashtable;
                }
                else if (param.ParamState == ParamState.Ready && param.Name == "feature_class")
                {
                    featureClass = param.Values[0].ToString();
                }
            }

            
            if (region != null && featureClass != "")
            {
                if (region["type"].ToString() == "drawing")
                {
                    string graphicsName = region["value"].ToString();
                    //this._mapMgr.SelectFeaturesByGraphics(graphicsName);
                    //respList.Add(new DialogueResponse(DialogueResponseType.speechInfo, "The " + featureClass + " within " + graphicsName + "are highlighted in the map!"));
                    //int count = this._mapMgr.GetTotalSelectedFeaturesInLayer(featureClass);
                    //respList.Add(new DialogueResponse(DialogueResponseType.speechInfo, "There are total of " + count + " " + featureClass + " selected."));
                    respList.Add(new DialogueResponse(DialogueResponseType.debugInfo, "overlay " + featureClass + " and " + graphicsName));
                }
                else if (region["type"].ToString() == "features")
                {
                    string in_layer = featureClass;
                    string select_features = region["value"].ToString();

                    ArrayList inputLayers = new ArrayList();
                    inputLayers.Add(in_layer);
                    inputLayers.Add(select_features);

                    // use cached output file for demo purpose, reduce the time to calculate the overlay
                    string cachedOutputFile = in_layer + "_" + select_features + "_overlay";
                    string outputFile = this._mapMgr.Overlay(inputLayers, cachedOutputFile);
                    if (outputFile.Length > 0)
                    {
                        this._addValueToParam(paramNode, cachedOutputFile, indent);
                        respList.Add(new DialogueResponse(DialogueResponseType.speechInfo, "The overlay of " + featureClass + " and " + select_features + "is added in the map!"));
                        respList.Add(new DialogueResponse(DialogueResponseType.mapLayerAdded, outputFile));
                        respList.Add(new DialogueResponse(DialogueResponseType.debugInfo, "overlay " + featureClass + " and " + select_features));
                        // change its own state
                        actionNode.ActState = ActionState.Complete;
                        return respList;
                    }

                    //this._mapMgr.SelectFeaturesByLocation(in_layer, select_features);
                    //respList.Add(new DialogueResponse(DialogueResponseType.speechInfo, "The " + featureClass + " within " + select_features + "are highlighted in the map!"));
                    //int count = this._mapMgr.GetTotalSelectedFeaturesInLayer(featureClass);
                    //respList.Add(new DialogueResponse(DialogueResponseType.speechInfo, "There are total of " + count + " " + featureClass + " selected."));
                    
                }
            }

            // change its own state
            actionNode.ActState = ActionState.Complete;
            // generate response 
            return respList;
        }

        private ArrayList CalculateFieldStatistics(ActionNode actionNode, DialogueAct currDlgAct, string indent)
        {
            Console.WriteLine(indent + "Executor.CalculateFieldStatistics actionNode:" + actionNode.Name);

            ArrayList respList = new ArrayList();
            // change its own state
            actionNode.ActState = ActionState.Executing;

            // do something:
            respList.Add(new DialogueResponse(DialogueResponseType.debugInfo, "Basic Action: CalculateFieldStatistics"));
            ActionNode parentNode = ((ActionNode)(actionNode.Parent));

            string featureClass = "";
            // search for overlay first
            ArrayList featureClassValues = this._searchValueFromAncestor(actionNode, "overlay");
            if (featureClassValues.Count == 0)
            {
                featureClassValues = this._searchValueFromAncestor(actionNode, "feature_class");
            }
            if (featureClassValues.Count > 0)
            {
                featureClass = featureClassValues[0].ToString();
            }

            string dataFieldName = "";
            ArrayList dataFieldValues = this._searchValueFromAncestor(actionNode, "data_field");
            if (dataFieldValues.Count > 0)
            {
                dataFieldName = dataFieldValues[0].ToString();
            }

            string statistics = "";
            ArrayList statisticsValues = this._searchValueFromAncestor(actionNode, "statistics");
            if (statisticsValues.Count > 0)
            {
                statistics = statisticsValues[0].ToString();
            }

            // mapping dataField name to field name in the data set
            string dataField = this._dataFieldMapping(dataFieldName);
            
            if (featureClass != "" && dataField != "")
            {
                Hashtable statResults = this._mapMgr.GetFieldStatistics(featureClass, dataField, true);
                respList.Add(new DialogueResponse(DialogueResponseType.speechInfo, "Here are the statistic results of " + dataFieldName));
                respList.Add(new DialogueResponse(DialogueResponseType.statisticResults, statResults));
                
            }

            // change its own state
            actionNode.ActState = ActionState.Complete;
            // generate response 
            return respList;
        }

        private ArrayList CalculateDataSummary(ActionNode actionNode, DialogueAct currDlgAct, string indent)
        {
            Console.WriteLine(indent + "Executor.CalculateDataSummary actionNode:" + actionNode.Name);

            ArrayList respList = new ArrayList();
            // change its own state
            actionNode.ActState = ActionState.Executing;

            // do something:
            respList.Add(new DialogueResponse(DialogueResponseType.debugInfo, "Basic Action: CalculateDataSummary"));

            ActionNode parentNode = ((ActionNode)(actionNode.Parent));

            string featureClass = "";
            bool selectedOnly = false;
            // search for overlay first
            ArrayList featureClassValues = this._searchValueFromAncestor(actionNode, "overlay");
            if (featureClassValues.Count == 0)
            {
                featureClassValues = this._searchValueFromAncestor(actionNode, "feature_class");
                selectedOnly = true;
            }
            if (featureClassValues.Count > 0)
            {
                featureClass = featureClassValues[0].ToString();
            }

            string sumFieldName = "";
            ArrayList sumFieldValues = this._searchValueFromAncestor(actionNode, "summarize_field");
            if (sumFieldValues.Count > 0)
            {
                sumFieldName = sumFieldValues[0].ToString();
            }
            sumFieldName = this._dataFieldMapping(sumFieldName);

            string statistics = "";
            ArrayList statisticsValues = this._searchValueFromAncestor(actionNode, "statistics");
            if (statisticsValues.Count > 0)
            {
                statistics = statisticsValues[0].ToString();
            }

            string grpFieldName = "";
            ArrayList grpFieldValues = this._searchValueFromAncestor(actionNode, "group_by_field");
            if (grpFieldValues.Count > 0)
            {
                grpFieldName = grpFieldValues[0].ToString();
            }
            grpFieldName = this._dataFieldMapping(grpFieldName);

            if (featureClass != "" && sumFieldName != "" && grpFieldName != "")
            {
                string summaryFields = "Minimum." + grpFieldName + ", ";
                switch (statistics)
                {
                    case "count":
                        summaryFields += "Count." + sumFieldName;
                        break;
                    case "sum":
                        summaryFields += "Sum." + sumFieldName;
                        break;
                    case "minimum":
                        summaryFields += "Minimum." + sumFieldName;
                        break;
                    case "maximum":
                        summaryFields += "Maximum." + sumFieldName;
                        break;
                    case "mean":
                        summaryFields += "Average." + sumFieldName;
                        break;
                    case "std":
                        summaryFields += "StdDev." + sumFieldName;
                        break;
                    default:
                        summaryFields += "Count." + sumFieldName;
                        summaryFields += ", ";
                        summaryFields += "Sum." + sumFieldName;
                        summaryFields += ", ";
                        summaryFields += "Minimum." + sumFieldName;
                        summaryFields += ", ";
                        summaryFields += "Maximum." + sumFieldName;
                        summaryFields += ", ";
                        summaryFields += "Average." + sumFieldName;
                        summaryFields += ", ";
                        summaryFields += "StdDev." + sumFieldName;
                        break;
                }

                Hashtable sumResults = this._mapMgr.GetDataSummary(featureClass, summaryFields, grpFieldName, selectedOnly);
                respList.Add(new DialogueResponse(DialogueResponseType.speechInfo, "Here are the summary of " + sumFieldName + " grouped by " + grpFieldName));
                respList.Add(new DialogueResponse(DialogueResponseType.summaryResults, sumResults));
            }

            // change its own state
            actionNode.ActState = ActionState.Complete;
            // generate response 
            return respList;
        }

        private string _dataFieldMapping(string dataFieldName)
        {
            // mapping dataField name to field name in the data set
            // fixed at the moment
            string dataField = "";
            if (dataFieldName == "dwelling units")
            {
                dataField = "DU";
            }
            else if (dataFieldName == "area")
            {
                dataField = "Acreage";
            }
            else if (dataFieldName == "land use")
            {
                dataField = "UseCode";
            }
            return dataField;
        }

        private string _generateQuestionString(ParamNode paramNode)
        {
            string question = "";
            if (paramNode.ParamType == "feature_class")
            {
                question = "What ";
                if (paramNode.Description != "")
                {
                    question += paramNode.Description;
                }
                else
                {
                    question += String.Join(" ", paramNode.Name.Split('_'));
                }
                question += " are you working on?";
            }
            if (paramNode.ParamType == "query")
            {
                question += "What the percentage do you consider to be potential customers";
            }
            if (paramNode.ParamType == "source_layer")
            {
                question = "What ";
                if (paramNode.Description != "")
                {
                    question += paramNode.Description;
                }
                else
                {
                    question += String.Join(" ", paramNode.Name.Split('_'));
                }
                question += " are you working on?";
            }
            else if (paramNode.ParamType == "data_source")
            {
                question = "Which ";
                if (paramNode.Description != "")
                {
                    question += paramNode.Description;
                }
                else
                {
                    question += String.Join(" ", paramNode.Name.Split('_'));
                }
                question += " are you working on?";
            }
            else if (paramNode.ParamType == "region_type")
            {
                question = "What is ";
                if (paramNode.Description != "")
                {
                    question += paramNode.Description;
                }
                else
                {
                    question += String.Join(" ", paramNode.Name.Split('_'));
                }
                question += " you are interested in?";
            }
            else if (paramNode.ParamType == "data_field")
            {
                question = "Which ";
                if (paramNode.Description != "")
                {
                    question += paramNode.Description;
                }
                else
                {
                    question += String.Join(" ", paramNode.Name.Split('_'));
                }
                question += " do you want to use?";
            }
            else if (paramNode.ParamType == "boolean")
            {
                question = "Do you ";
                if (paramNode.Description != "")
                {
                    question += paramNode.Description;
                }
                else
                {
                    question += String.Join(" ", paramNode.Name.Split('_'));
                }
                question += "?";
            }
            else if (paramNode.ParamType == "length" || paramNode.ParamType == "speed" || paramNode.ParamType == "time")
            {
                question = "What is the " + paramNode.Description + " do you think ?";
            }
            else if (paramNode.ParamType == "classification_schema")
            {
                question = "Which classification schema would you like to choose?";
            }
            else if (paramNode.ParamType == "quantity")
            {
                question = "How many ";
                if (paramNode.Description != "")
                {
                    question += paramNode.Description;
                }
                else
                {
                    question += String.Join(" ", paramNode.Name.Split('_'));
                }
                question += " would you like to specify?";
            }
            return question;
        }

        public object _parseValueFromSpeech(ParamNode paramNode, object speech)
        {
            if (paramNode.ParamType == "feature_class"
                || paramNode.ParamType == "data_source"
                || paramNode.ParamType == "region_type"
                || paramNode.ParamType == "geometry_polygon"
                || paramNode.ParamType == "data_field"
                || paramNode.ParamType == "statistics"
                || paramNode.ParamType == "classification_schema"
                || paramNode.ParamType == "symbolization_method"
                || paramNode.ParamType == "quantity"
                || paramNode.ParamType == "symbol_size"
                ){
                return speech.ToString();
            }
            else if (paramNode.ParamType == "length")
            {
                if (speech is SortedList)
                {
                    Hashtable lengthInfo = new Hashtable();
                
                    if (((SortedList)speech).ContainsKey("value"))
                    {
                        lengthInfo.Add("value", ((SortedList)speech)["value"]);
                    }
                    if (((SortedList)speech).ContainsKey("unit"))
                    {
                        lengthInfo.Add("unit", ((SortedList)speech)["unit"]);
                    }
                    return lengthInfo;
                }
            }
            else if (paramNode.ParamType == "query")
            {
                if (speech is SortedList)
                {
                    Hashtable lengthInfo = new Hashtable();

                    if (((SortedList)speech).ContainsKey("compare"))
                    {
                        lengthInfo.Add("compare", ((SortedList)speech)["compare"]);
                    }
                    if (((SortedList)speech).ContainsKey("value"))
                    {
                        lengthInfo.Add("value", ((SortedList)speech)["value"]);
                    }
                    return lengthInfo;
                }
            }
            else if (paramNode.ParamType == "time")
            {
                if (speech is SortedList)
                {
                    Hashtable lengthInfo = new Hashtable();

                    if (((SortedList)speech).ContainsKey("value"))
                    {
                        lengthInfo.Add("value", ((SortedList)speech)["value"]);
                    }
                    if (((SortedList)speech).ContainsKey("unit"))
                    {
                        lengthInfo.Add("unit", ((SortedList)speech)["unit"]);
                    }
                    return lengthInfo;
                }
            }
            else if (paramNode.ParamType == "speed")
            {
                if (speech is SortedList)
                {
                    Hashtable lengthInfo = new Hashtable();

                    if (((SortedList)speech).ContainsKey("value"))
                    {
                        lengthInfo.Add("value", ((SortedList)speech)["value"]);
                    }
                    if (((SortedList)speech).ContainsKey("unit"))
                    {
                        lengthInfo.Add("unit", ((SortedList)speech)["unit"]);
                    }
                    return lengthInfo;
                }
            }
            else if (paramNode.ParamType == "boolean")
            {
                if (paramNode.Name == "partiality")
                {
                    if (speech.ToString() == "partial")
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return null;
        }

        public void _addValueToParam(ParamNode paramNode, object newValue, string indent)
        {
            Console.WriteLine(indent + "Executor._addValueToParam " + paramNode.Name);
            if (newValue != null)
            {
                if (paramNode.Multiple == false)
                {
                    paramNode.Values.Clear();
                }
                else
                {
                    if(newValue is string)Console.WriteLine(indent + "newValue:" + newValue.ToString());
                    else if (newValue is Hashtable) {
                        foreach (DictionaryEntry item in (Hashtable)newValue)
                        {
                            Console.WriteLine(indent + "key=" + item.Key + ",value=" + item.Value);
                        }
                    }
                    foreach (object value in paramNode.Values)
                    {
                        // check whether the new value already exists
                        // only check string type at the moment
                        Console.WriteLine(indent + "Value:" + value.ToString());
                        if (newValue is string)
                        {

                            if (newValue.ToString() == value.ToString())
                            {
                                return;
                            }
                        }
                    }
                }
                paramNode.Values.Add(newValue);              
            }
        }

    }
}
