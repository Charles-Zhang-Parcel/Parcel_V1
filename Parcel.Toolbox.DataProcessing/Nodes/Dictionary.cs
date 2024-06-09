﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using Parcel.Shared.DataTypes;
using Parcel.Shared.Framework;
using Parcel.Shared.Framework.ViewModels;
using Parcel.Shared.Framework.ViewModels.BaseNodes;

namespace Parcel.Toolbox.DataProcessing.Nodes
{
    public class DictionaryEntryDefinition: ObservableObject
    {
        #region Properties
        private string _name = "New Key";
        public string Name
        {
            get => _name;
            set => SetField(ref _name, value);
        }
        private DictionaryEntryType _type = DictionaryEntryType.Number;
        public DictionaryEntryType Type // TODO: This member is not made use of yet; Currently we are just parsing the string according to heuristics
        {
            get => _type;
            set => SetField(ref _type, value);
        }
        private object _value = 0;
        public object Value
        {
            get => _value;
            set => SetField(ref _value, DataGrid.Preformatting((string)value));
        }
        #endregion
    }
    
    public class Dictionary: ProcessorNode
    {
        #region Node Interface
        private readonly OutputConnector _dataTableOutput = new OutputConnector(typeof(DataGrid))
        {
            Title = "Dictionary"
        }; 
        public Dictionary()
        {
            // Serialization
            // VariantInputConnectorsSerialization = new NodeSerializationRoutine(SerializeEntries,
            // source => DeserializeEntries((List<Tuple<string, int, object>>)source));
            VariantInputConnectorsSerialization = null;

            Title = NodeTypeName = "Dictionary";
            Output.Add(_dataTableOutput);

            Definitions = new ObservableCollection<DictionaryEntryDefinition>()
            {
                new DictionaryEntryDefinition() { Name = "Entry 1" }
            };
            
            AddEntryCommand = new RequeryCommand(
                () => Definitions.Add(new DictionaryEntryDefinition() {Name = $"Entry {Definitions.Count+1}"} ),
                () => true);
            RemoveEntryCommand = new RequeryCommand(
                () => Definitions.RemoveAt(Definitions.Count - 1),
                () => Definitions.Count > 1);
        }
        #endregion

        #region View Binding/Internal Node Properties
        private ObservableCollection<DictionaryEntryDefinition> _definitions;
        public ObservableCollection<DictionaryEntryDefinition> Definitions { get => _definitions; set => SetField(ref _definitions, value); }
        public IProcessorNodeCommand AddEntryCommand { get; }
        public IProcessorNodeCommand RemoveEntryCommand { get; }
        #endregion
        
        #region Processor Interface
        protected override NodeExecutionResult Execute()
        {
            ExpandoObject expando = new ExpandoObject();
            foreach (DictionaryEntryDefinition definition in Definitions)
            {
                IDictionary<string, object> dict = (IDictionary<string, object>)expando;
                dict[definition.Name] = definition.Value;
            }
            DataGrid dataGrid = new DataGrid(expando);

            return new NodeExecutionResult(new NodeMessage($"{dataGrid.RowCount} Rows; {dataGrid.ColumnCount} Columns."), new Dictionary<OutputConnector, object>()
            {
                {_dataTableOutput, dataGrid}
            });
        }
        #endregion
        
        #region Routines
        private List<Tuple<string, int, object>> SerializeEntries()
            => Definitions.Select(def => new Tuple<string, int, object>(def.Name, (int) def.Type, def.Value))
                .ToList();
        private void DeserializeEntries(IEnumerable<Tuple<string, int, object>> source)
        {
            Definitions.Clear();
            Definitions.AddRange(source.Select(tuple => new DictionaryEntryDefinition()
            {
                Name = tuple.Item1,
                Type = (DictionaryEntryType) tuple.Item2,
                Value = tuple.Item3
            }));
        }
        #endregion
        
        #region Serialization
        protected override Dictionary<string, NodeSerializationRoutine> ProcessorNodeMemberSerialization { get; } =
            null;
        protected override NodeSerializationRoutine VariantInputConnectorsSerialization { get; }
        #endregion
    }
}