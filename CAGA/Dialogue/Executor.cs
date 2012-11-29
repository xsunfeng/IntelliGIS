using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using CAGA.Map;

namespace CAGA.Dialogue
{
    class Executor
    {
        private ArcMapManager _mapMgr;
        public Executor(ArcMapManager mapMgr)
        {
            this._mapMgr = mapMgr;
        }

        public ArrayList Execute(ActionNode actionNode, DialogueAct currDlgAct)
        {
            if (actionNode.Name.ToLower() == "Get Value From Input".ToLower())
            {
                return this.GetValueFromInput(actionNode, currDlgAct);
            }
            else if (actionNode.Name.ToLower() == "Ask For Value".ToLower())
            {
                return this.AskForValue(actionNode, currDlgAct);
            }
            else if (actionNode.Name.ToLower() == "Select From Candidates".ToLower())
            {
                return this.SelectFromCandidates(actionNode, currDlgAct);
            }
            else if (actionNode.Name.ToLower() == "Get Existing Value From Ancestor".ToLower())
            {
                return this.GetExistingValueFromAncestor(actionNode, currDlgAct);
            }
            else if (actionNode.Name.ToLower() == "Ask For More Value".ToLower())
            {
                return this.AskForMoreValue(actionNode, currDlgAct);
            }
            else if (actionNode.Name.ToLower() == "Get Current Map Extent".ToLower())
            {
                return this.GetCurrentMapExtent(actionNode, currDlgAct);
            }
            else if (actionNode.Name.ToLower() == "Draw Region".ToLower())
            {
                return this.DrawRegion(actionNode, currDlgAct);
            }
            else if (actionNode.Name.ToLower() == "Infer Value From Other Parameter".ToLower())
            {
                return this.InferValueFromOtherParameter(actionNode, currDlgAct);
            }
            else if (actionNode.Name.ToLower() == "Choose Analytic Functions".ToLower())
            {
                return this.ChooseAnalyticFunctions(actionNode, currDlgAct);
            }
            else if (actionNode.Name.ToLower() == "Perform Selection".ToLower())
            {
                return this.PerformSelection(actionNode, currDlgAct);
            }
            else if (actionNode.Name.ToLower() == "Calculate Field Statistics".ToLower())
            {
                return this.CalculateFieldStatistics(actionNode, currDlgAct);
            }
            else if (actionNode.Name.ToLower() == "Calculate Data Summary".ToLower())
            {
                return this.CalculateDataSummary(actionNode, currDlgAct);
            }
            return new ArrayList();
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

        private ArrayList GetValueFromInput(ActionNode actionNode, DialogueAct currDlgAct)
        {
            ArrayList respList = new ArrayList();
            // change its own state
            actionNode.ActState = ActionState.Executing;

            // do something: parse the value
            respList.Add(new DialogueResponse(DialogueResponseType.debugInfo, "Basic Action: GetValueFromInput"));
            
            ParamNode paramNode = (ParamNode)actionNode.Parent;
            foreach (string phrase in currDlgAct.SpeechContext.Keys)
            {
                if (phrase.ToLower() == paramNode.Name.ToLower())
                {
                    object newValue = _parseValueFromSpeech(paramNode, currDlgAct.SpeechContext[phrase]);
                    if (newValue != null)
                    {
                        this._addValueToParam(paramNode, newValue);
                        break;
                    }
                }
            }
            // change its own state
            actionNode.ActState = ActionState.Complete;
            // generate response 
            return respList;
        }

        private ArrayList AskForValue(ActionNode actionNode, DialogueAct currDlgAct)
        {
            ArrayList respList = new ArrayList();

            
            // if the action has not been executed, set it to executing, raid the question and return
            if (actionNode.ActState == ActionState.Initiated)
            {
                // change its own state
                actionNode.ActState = ActionState.Executing;

                // do something: raise a question to the user
                respList.Add(new DialogueResponse(DialogueResponseType.debugInfo, "Basic Action: AskForValue"));

                ParamNode paramNode = (ParamNode)actionNode.Parent;
                // only ask if there is no value assigned yet
                if (paramNode.Values.Count == 0)
                {
                    respList.Add(new DialogueResponse(DialogueResponseType.speechQuestion, this._generateQuestionString(paramNode)));
                    // change its own state
                    actionNode.ActState = ActionState.Executing;
                    return respList;
                }
                else
                {
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
                    foreach (string phrase in currDlgAct.SpeechContext.Keys)
                    {
                        if (phrase.ToLower() == paramNode.Name.ToLower())
                        {
                            object newValue = this._parseValueFromSpeech(paramNode, currDlgAct.SpeechContext[phrase]);
                            if (newValue != null)
                            {
                                this._addValueToParam(paramNode, newValue);
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
                    this._addValueToParam(paramNode, true);
                    respList.Add(new DialogueResponse(DialogueResponseType.debugInfo, paramNode.Name + ": true"));
                    actionNode.ActState = ActionState.Complete;
                    return respList;
                }
                else if (currDlgAct.DlgActType == DialogueActType.Accept && paramNode.ParamType == "boolean")
                {
                    this._addValueToParam(paramNode, true);
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

        private ArrayList SelectFromCandidates(ActionNode actionNode, DialogueAct currDlgAct)
        {
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
                    respContent.AddOption(new PlainOptionItemData("City of Oleader", "Matched feature classes: LandUse"));
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
                            this._addValueToParam(paramNode, newValue);

                            // change its own state
                            actionNode.ActState = ActionState.Complete;
                            // generate response 
                            if (paramNode.ParamType == "data_source")
                            {
                                // fixed at the moment
                                string dataSourcePath = @"..\..\..\Data\GISLAB\Data\";
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


        private ArrayList GetExistingValueFromAncestor(ActionNode actionNode, DialogueAct currDlgAct)
        {
            ArrayList respList = new ArrayList();
            // change its own state
            actionNode.ActState = ActionState.Executing;

            // do something:
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
                            this._addValueToParam(paramNode, value);
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
                            if (param.ParamType == paramNode.ParamType && param.ParamState == ParamState.Ready)
                            {
                                foreach (object value in param.Values)
                                {
                                    this._addValueToParam(paramNode, value);
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

        private ArrayList _searchValueFromAncestor(PlanNode planNode, string paramName)
        {
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
                            matchedValues.AddRange(param.Values);
                            break;
                        }
                    }
                }
                parent = parent.Parent;
            }
            return matchedValues;
        }

        private ArrayList AskForMoreValue(ActionNode actionNode, DialogueAct currDlgAct)
        {
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
                    respContent.AddOption(new MapLayerOptionItemData("Lot boundaries", @"C:\Work\Data\GISLAB\Data\Oleader\Lot Boundaries.lyr"));
                    respContent.AddOption(new MapLayerOptionItemData("Zoning", @"C:\Work\Data\GISLAB\Data\Oleader\Zoning.lyr"));
                    respContent.AddOption(new MapLayerOptionItemData("City Limit", @"C:\Work\Data\GISLAB\Data\Oleader\City Limit.lyr"));
                    
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
                            this._addValueToParam(paramNode, newValue);

                            // change its own state
                            actionNode.ActState = ActionState.Complete;
                            // generate response 
                            if (paramNode.ParamType == "feature_class")
                            {
                                // fixed at the moment
                                string dataSourcePath = @"C:\Work\Data\GISLAB\Data\Oleader\";
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

        private ArrayList GetCurrentMapExtent(ActionNode actionNode, DialogueAct currDlgAct)
        {
            ArrayList respList = new ArrayList();
            // change its own state
            actionNode.ActState = ActionState.Executing;

            // do something:
            ParamNode paramNode = (ParamNode)actionNode.Parent;
            object newValue = this._mapMgr.GetMapExtent();
            this._addValueToParam(paramNode, newValue);
            respList.Add(new DialogueResponse(DialogueResponseType.debugInfo, "Basic Action: GetCurrentMapExtent"));

            // change its own state
            actionNode.ActState = ActionState.Complete;
            // generate response 
            return respList;
        }

        private ArrayList DrawRegion(ActionNode actionNode, DialogueAct currDlgAct)
        {
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
                                    this._addValueToParam(paramNode, newValue);

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

        private ArrayList InferValueFromOtherParameter(ActionNode actionNode, DialogueAct currDlgAct)
        {
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
                        this._addValueToParam(paramNode, true);
                        respList.Add(new DialogueResponse(DialogueResponseType.debugInfo, paramNode.Name + ": true"));
                    }
                    else if (matchedParam.Values.Count == 1)
                    {
                        this._addValueToParam(paramNode, false);
                        respList.Add(new DialogueResponse(DialogueResponseType.debugInfo, paramNode.Name + ": false"));
                    }
                }
            }

            // change its own state
            actionNode.ActState = ActionState.Complete;
            // generate response 
            return respList;
        }

        private ArrayList ChooseAnalyticFunctions(ActionNode actionNode, DialogueAct currDlgAct)
        {
            ArrayList respList = new ArrayList();
            // change its own state
            actionNode.ActState = ActionState.Executing;

            // do something:
            respList.Add(new DialogueResponse(DialogueResponseType.debugInfo, "Basic Action: ChooseAnalyticFunctions"));
            ActionNode parentNode = ((ActionNode)(actionNode.Parent));
            bool multiple = false;
            bool need_summary = false;
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
            }
            string chosenActionName = ""; 
            if (need_summary == true)
            {
                if (multiple == false)
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

        private ArrayList PerformSelection(ActionNode actionNode, DialogueAct currDlgAct)
        {
            ArrayList respList = new ArrayList();
            // change its own state
            actionNode.ActState = ActionState.Executing;

            // do something:
            respList.Add(new DialogueResponse(DialogueResponseType.debugInfo, "Basic Action: PerformSelection"));

            ActionNode parentNode = ((ActionNode)(actionNode.Parent));
            string featureClass = "";
            string graphicsName = "";
            foreach (ParamNode param in parentNode.Params)
            {
                if (param.ParamState == ParamState.Ready && param.ParamType == "geometry_polygon")
                {
                    graphicsName = param.Values[0].ToString();
                }
                else if (param.ParamState == ParamState.Ready && param.ParamType == "feature_class")
                {
                    featureClass = param.Values[0].ToString();
                }
            }
            if (graphicsName != "" && featureClass != "")
            {
                this._mapMgr.SelectFeaturesByGraphics(graphicsName);
                respList.Add(new DialogueResponse(DialogueResponseType.speechInfo, "The " + featureClass + " within " + graphicsName + "are highlighted in the map!"));
                int count = this._mapMgr.GetTotalSelectedFeaturesInLayer(featureClass);
                respList.Add(new DialogueResponse(DialogueResponseType.speechInfo, "There are total of " + count + " " + featureClass + " selected."));
            }

            // change its own state
            actionNode.ActState = ActionState.Complete;
            // generate response 
            return respList;
        }

        private ArrayList CalculateFieldStatistics(ActionNode actionNode, DialogueAct currDlgAct)
        {
            ArrayList respList = new ArrayList();
            // change its own state
            actionNode.ActState = ActionState.Executing;

            // do something:
            respList.Add(new DialogueResponse(DialogueResponseType.debugInfo, "Basic Action: CalculateFieldStatistics"));
            ActionNode parentNode = ((ActionNode)(actionNode.Parent));

            string featureClass = "";
            ArrayList featureClassValues = this._searchValueFromAncestor(actionNode, "feature_class");
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

        private ArrayList CalculateDataSummary(ActionNode actionNode, DialogueAct currDlgAct)
        {
            ArrayList respList = new ArrayList();
            // change its own state
            actionNode.ActState = ActionState.Executing;

            // do something:
            respList.Add(new DialogueResponse(DialogueResponseType.debugInfo, "Basic Action: CalculateDataSummary"));

            ActionNode parentNode = ((ActionNode)(actionNode.Parent));

            string featureClass = "";
            ArrayList featureClassValues = this._searchValueFromAncestor(actionNode, "feature_class");
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

                Hashtable sumResults = this._mapMgr.GetDataSummary(featureClass, summaryFields, grpFieldName, true);
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

        private ArrayList BufferOp(ActionNode actionNode, DialogueAct currDlgAct)
        {
            ArrayList respList = new ArrayList();
            // change its own state
            actionNode.ActState = ActionState.Executing;

            // do something: calculate the buffer
            ActionNode parentAct = (ActionNode)actionNode.Parent;
            string inputLayer = parentAct.GetParamValue("input_layer")[0].ToString();
            Hashtable distInfo = (Hashtable)(parentAct.GetParamValue("distance")[0]);
            string distString = distInfo["value"].ToString();
            if (distInfo.ContainsKey("unit"))
            {
                distString += " " + distInfo["unit"].ToString();
            }

            // todo: convert the unit if needed
            
            if (inputLayer != "" && distString != "")
            {
                string outLayerFile = ((IGeoProcessor)this._mapMgr).Buffer(inputLayer, distString);
                if (outLayerFile.Length > 0)
                {
                    respList.Add(new DialogueResponse(DialogueResponseType.mapLayerAdded, outLayerFile));
                    // change its own state
                    actionNode.ActState = ActionState.Complete;
                    return respList;
                }
            }
            // change its own state
            actionNode.ActState = ActionState.Failed;
            
            // generate response 
            return respList;
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
            else if (paramNode.ParamType == "length")
            {
                question = "What is the " + String.Join(" ", paramNode.Name.Split('_')) + "?";
            }
            return question;
        }

        private object _parseValueFromSpeech(ParamNode paramNode, object speech)
        {
            if (paramNode.ParamType == "feature_class")
            {
                return speech.ToString();
            }
            else if (paramNode.ParamType == "data_source")
            {
                return speech.ToString();
            }
            else if (paramNode.ParamType == "geometry_polygon")
            {
                return speech.ToString();
            }
            else if (paramNode.ParamType == "data_field")
            {
                return speech.ToString();
            }
            else if (paramNode.ParamType == "statistics")
            {
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
            return null;
        }

        private void _addValueToParam(ParamNode paramNode, object newValue)
        {
            if (newValue != null)
            {
                if (paramNode.Multiple == false)
                {
                    paramNode.Values.Clear();
                }
                else
                {
                    foreach (object value in paramNode.Values)
                    {
                        // check whether the new value already exists
                        // only check string type at the moment
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
