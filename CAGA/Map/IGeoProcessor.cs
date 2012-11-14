using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CAGA.Map
{

    interface IGeoProcessor
    {
        string Buffer(string inLayerName, string distString, string outLayerName = "");
    }
}
