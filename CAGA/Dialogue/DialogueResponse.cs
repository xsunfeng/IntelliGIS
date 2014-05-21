using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CAGA.Dialogue
{
    public enum DialogueResponseType
    { 
        speechQuestion,
        speechInfo,
        speechError,
        
        listPlainOptions,
        listMapLayerOptions,
        listOptionsWithExamples,

        mapDocumentOpened,
        mapLayerRemoved,
        mapLayerAdded,
        mapLayerModified,
        bufferZoneAdded,
        mapExtentChanged,

        drawPolygonStarted,

        selectByAttributes,

        newAgendaItem,

        statisticResults,
        summaryResults,
        featureLayerInfo,

        debugInfo,
        debugError,
        debugWarning
    }

    class DialogueResponse
    {
        private DialogueResponseType _dlgRespType;

        public DialogueResponseType DlgRespType
        {
            get { return _dlgRespType; }
            set { _dlgRespType = value; }
        }
        private object _respContent;

        public object RespContent
        {
            get { return _respContent; }
            set { _respContent = value; }
        }

        public DialogueResponse(DialogueResponseType respType, object respContent)
        {
            this._dlgRespType = respType;
            this._respContent = respContent;
        }
    }

 
}
