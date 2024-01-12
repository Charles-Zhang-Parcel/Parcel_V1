using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Parcel.Shared.DataTypes;
using Parcel.Shared.Framework;
using Parcel.Shared.Framework.ViewModels;
using Parcel.Shared.Framework.ViewModels.BaseNodes;

namespace Parcel.Toolbox.DataProcessing.Nodes
{
    /// <summary>
    /// TODO: Editing for Data Table node is not working yet at this moment due to a transform as Expando object during presentation
    /// </summary>
    public class DataTableFieldDefinition: ObservableObject
    {
        #region Properties
        private string _name = "New Field";
        public string Name
        {
            get => _name;
            set => SetField(ref _name, value);
        }
        private DictionaryEntryType _type = DictionaryEntryType.Number;
        public DictionaryEntryType Type
        {
            get => _type;
            set => SetField(ref _type, value);
        }
        #endregion
    }
    
    public class DataTable: ProcessorNode
    {
        #region Node Interface
        private readonly OutputConnector _dataTableOutput = new OutputConnector(typeof(DataGrid))
        {
            Title = "Data Table"
        }; 
        public DataTable()
        {
            // Serialization
            ProcessorNodeMemberSerialization = new Dictionary<string, NodeSerializationRoutine>()
            {
                {nameof(Data), new NodeSerializationRoutine(() => SerializationHelper.Serialize(Data), o => Data = o as object[][])},
            };
            VariantInputConnectorsSerialization = new NodeSerializationRoutine(SerializeEntries,
                source => DeserializeEntries((List<Tuple<string, int>>)source));
            
            Definitions = new ObservableCollection<DataTableFieldDefinition>()
            {
                new DataTableFieldDefinition() {Name = "New Field"}
            };
            
            AddEntryCommand = new RequeryCommand(
                () => Definitions.Add(new DataTableFieldDefinition() {Name = $"New Field {Definitions.Count + 1}"} ),
                () => true);
            RemoveEntryCommand = new RequeryCommand(
                () => Definitions.RemoveAt(Definitions.Count - 1),
                () => Definitions.Count > 1);
            
            Title = NodeTypeName = "Data Table";
            Output.Add(_dataTableOutput);
        }
        #endregion

        #region Native Data
        public object[][] Data { get; set; }
        #endregion
        
        #region View Binding/Internal Node Properties
        private ObservableCollection<DataTableFieldDefinition> _definitions;
        public ObservableCollection<DataTableFieldDefinition> Definitions { get => _definitions;
            private set => SetField(ref _definitions, value); }
        public IProcessorNodeCommand AddEntryCommand { get; }
        public IProcessorNodeCommand RemoveEntryCommand { get; }
        #endregion

        #region Methods
        public DataGrid InitializeDataGrid()
        {
            // Create table
            DataGrid dataGrid = new DataGrid();
            // Update columns
            foreach (DataTableFieldDefinition definition in Definitions)
            {
                DataColumn column = dataGrid.AddColumn(definition.Name);
                // Add data to fix column type
                switch (definition.Type)
                {
                    case DictionaryEntryType.Number:
                        column.Add(0.0);
                        break;
                    case DictionaryEntryType.String:
                        column.Add(string.Empty);
                        break;
                    case DictionaryEntryType.Boolean:
                        column.Add(false);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                if (Data != null)
                    column.RemoveAt(0); // Remove redundant data
            }
            
            // Populate data
            if (Data != null)
            {
                for (int col = 0; col < Math.Min(Data.Length, Definitions.Count); col++)
                for (int row = 0; row < Data[col].Length; row++)
                    dataGrid.Columns[col].Add(Data[col][row]);
            }
            
            return dataGrid;
        }
        #endregion

        #region Processor Interface
        protected override NodeExecutionResult Execute()
        {
            DataGrid dataGrid = InitializeDataGrid();
            return new NodeExecutionResult(new NodeMessage($"{dataGrid.ColumnCount} Fields."), new Dictionary<OutputConnector, object>()
            {
                {_dataTableOutput, dataGrid}
            });
        }
        #endregion
        
        #region Serialization
        protected override Dictionary<string, NodeSerializationRoutine> ProcessorNodeMemberSerialization { get; }
        protected override NodeSerializationRoutine VariantInputConnectorsSerialization { get; }
        #endregion

        #region Routines
        private List<Tuple<string, int>> SerializeEntries()
            => Definitions.Select(def => new Tuple<string, int>(def.Name, (int) def.Type))
                .ToList();
        private void DeserializeEntries(IEnumerable<Tuple<string, int>> source)
        {
            Definitions.Clear();
            Definitions.AddRange(source.Select(tuple => new DataTableFieldDefinition()
            {
                Name = tuple.Item1,
                Type = (DictionaryEntryType) tuple.Item2
            }));
        }
        #endregion
    }
}