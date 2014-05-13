using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CAGA.Map
{
    public enum MapFunctionMode
    {
        None = 0,
        Info = 1,
        ZoomIn = 2,
        ZoomOut = 3,
        Pan = 4,
        Label = 5,
        SelectByRectangle = 6,
        SelectByPolygon = 7,
        SelectByCircle = 8,
        SelectByLine = 9,
        DrawPolygon = 10,
    }

    abstract public class MapManager
    {
        public abstract void Initialize();
        public abstract void Dispose();
        
        public abstract void AddTOC(System.Windows.Controls.Panel tocContainer);

        public abstract bool LoadMap(string filePath);
        public abstract bool SaveMap(string filePath);
        public abstract string GetMapFile();

        public abstract bool AddLayer(string filePath, System.Int32 index);
        public abstract void RemoveLayer();

        public abstract Hashtable GetMapExtent();

        public abstract void SetFunctionMode(MapFunctionMode mode);

        
        public abstract void ZoomToMaxExtent();
        public abstract void ZoomToPrev();
        public abstract void ZoomToNext();
        public abstract void ZoomToLayer();
        
        public abstract void ClearMapSelection();
        public abstract void SelectFeaturesByGraphics();
        
        public abstract void ShowAttributeTable();
    }
}
