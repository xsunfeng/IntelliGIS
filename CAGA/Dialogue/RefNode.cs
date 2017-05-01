using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.CartoUI;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geoprocessing;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesFile;

namespace CAGA.Dialogue
{
    public enum refState
    {
        Unknown = 0,
        InPreparation = 1,
        Ready = 2,
        Failed = 3
    }
    class RefNode
    {
        private string _Type;

        private ParamNode _parent;
        public ParamNode parent
        {
            get { return _parent; }
            set { _parent = value; }
        }

        public string Type
        {
            get { return _Type; }
            set { _Type = value; }
        }
        private refState _State;

        public refState State
        {
            get { return _State; }
            set { _State = value; }
        }
        public string  sourceName;

        public string sourceLayer;

        public string sourceType;

        public string query;


        private ESRI.ArcGIS.Geometry.GeometryArray _features;

        public ESRI.ArcGIS.Geometry.GeometryArray features
        {
            get { return _features; }
            set { _features = value; }
        }

        private bool _flag;
        public bool Flag
        {
            get { return _flag; }
            set { _flag = value; }
        }


        public RefNode(ActionNode tmpAct)
        {
            // set the properties of the refnode based on the recipe retrieved for tmpAct
            // check for readiness of the node for execution, if yes, turn status to be ready

        }

        public void execute()
        {
            // check if the status is ready (all the attributes have been set and parent is known
            // execute and return the features and set them to _features
            // compare the features with what is expected in parent.  If it matches, transfer the values to the parent and set the paramNode to be complete.
            ESRI.ArcGIS.Carto.IMap map = null;
            ESRI.ArcGIS.Carto.IMapDocument tempDoc = new ESRI.ArcGIS.Carto.MapDocument();
            string filePath = this.sourceName;
            ESRI.ArcGIS.Carto.IMapDocument mapDoc = new ESRI.ArcGIS.Carto.MapDocument();
            if (mapDoc.get_IsPresent(filePath) && !mapDoc.get_IsPasswordProtected(filePath))
            {
                mapDoc.Open(filePath, string.Empty);
                // set the first map as the active view
                map = mapDoc.get_Map(0);
                for (int i = 0; i < map.LayerCount; i++)
                {
                    IFeatureLayer layer = map.get_Layer(i) as IFeatureLayer;
                    Console.WriteLine("layer=====" + layer.Name);
                    if (layer.Name == this.sourceLayer)
                    {
                        layer.Selectable = true;
                        IQueryFilter queryFilter = new QueryFilterClass();

                        // queryFilter.WhereClause = this.query;
                        queryFilter.WhereClause = "ZONE_ = 'A'";

                        IFeatureSelection pFeatureSelection = layer as IFeatureSelection;
                        pFeatureSelection.SelectFeatures(queryFilter, esriSelectionResultEnum.esriSelectionResultNew, false);

                        //pFeatureSelection.SelectionSet.
                        if (pFeatureSelection.SelectionSet.Count == 0)
                        {
                            Console.WriteLine("No features found for" + this.query);
                        }
                        else
                        {
                            //for (int j = 0; j < pFeatureSelection.SelectionSet.Count; j++)
                            //{
                            //    IFeature f = (pFeatureSelection as IFeatureLayer).FeatureClass.GetFeature(j);
                            //    this.parent.Values.Add(f);
                            //}

                            IFeatureClass featureClass = layer.FeatureClass;
                            //use an IEnumIDs to read the SelectionSet IDs
                            IEnumIDs enumIDs = pFeatureSelection.SelectionSet.IDs;
                            int fieldIndex1 = featureClass.FindField("SHAPE_Length");
                            int fieldIndex2 = featureClass.FindField("SHAPE_Area");
                            string s = "{0}ID \t\tSHAPE_Length \t\tSHAPE_Area";
                            IFeature feature;
                            IDataset dataset = (IDataset)featureClass;

                            int iD = enumIDs.Next();
                            while (iD != -1) //-1 is reutned after the last valid ID has been reached
                            {
                                feature = featureClass.GetFeature(iD);
                                s += "{0}" + iD + ":\t\t" + feature.get_Value(fieldIndex1) + "\t\t" + feature.get_Value(fieldIndex2);
                                iD = enumIDs.Next();
                            }

                            //report some information about the selection
                            // we need to store this result back to the VALUES of the Parent's node, which is a ParamNode.
                            Console.WriteLine("A SelectionSet containing: {1} Rows {0}has been created using the query: {2} {0}this selection set was created on the feature class: {3}{0}" + s + "{0}",
                                Environment.NewLine, pFeatureSelection.SelectionSet.Count, queryFilter.WhereClause, dataset.Name);
                        }
                    }
                }
            }
        }

    }
}
