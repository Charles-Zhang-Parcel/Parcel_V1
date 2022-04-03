﻿using Parcel.Shared.DataTypes;
using Parcel.Shared.Framework;
using Parcel.Shared.Framework.ViewModels;
using Parcel.Shared.Framework.ViewModels.BaseNodes;

namespace Parcel.Toolbox.DataProcessing.Nodes
{
    public class CSV: ProcessorNode
    {
        #region Node Interface
        public readonly BaseConnector PathInput = new BaseConnector(typeof(string))
        {
            Title = "Path",
        };
        public readonly  BaseConnector HeaderInput = new BaseConnector(typeof(bool))
        {
            Title = "Contains Header"
        };
        public readonly BaseConnector DataTableOutput = new BaseConnector(typeof(DataGrid))
        {
            Title = "Data Table"
        }; 
        public CSV()
        {
            Title = "CSV";
            Input.Add(PathInput);
            Input.Add(HeaderInput);
            Output.Add(DataTableOutput);
        }
        #endregion
        
        #region Processor Interface
        public override NodeExecutionResult Execute()
        {
            CSVParameter parameter = new CSVParameter()
            {
                InputPath = PathInput.FetchInputValue<string>(),
                InputContainsHeader = HeaderInput.FetchInputValue<bool>()
            };
            DataProcessingHelper.CSV(parameter);
            
            ProcessorCache[DataTableOutput] = new ConnectorCacheDescriptor()
            {
                DataObject = new DataGrid(),
                DataType = CacheDataType.ParcelDataGrid 
            };
            return new NodeExecutionResult(true, null);
        }
        #endregion
    }
}