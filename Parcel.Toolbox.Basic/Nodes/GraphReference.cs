﻿using System;
using System.Collections.Generic;
using System.Linq;
using Parcel.Shared.DataTypes;
using Parcel.Shared.Framework;
using Parcel.Shared.Framework.Advanced;
using Parcel.Shared.Framework.ViewModels;
using Parcel.Shared.Framework.ViewModels.BaseNodes;
using Parcel.Shared.Serialization;

namespace Parcel.Toolbox.Basic.Nodes
{
    public class GraphReference: ProcessorNode
    {
        #region Node Interface
        public GraphReference()
        {
            ProcessorNodeMemberSerialization = new Dictionary<string, NodeSerializationRoutine>()
            {
                {nameof(GraphPath), new NodeSerializationRoutine(() => SerializationHelper.Serialize(GraphPath), value => GraphPath = SerializationHelper.GetString(value))}
            };
            
            Title = NodeTypeName = "Graph Reference";
        }
        #endregion
        
        #region View Binding/Internal Node Properties
        private string _graphPath = null;
        public string GraphPath
        {
            get => _graphPath;
            set
            {
                SetField(ref _graphPath, value);
                UpdateGraphReference();
            }
        }
        #endregion
        
        #region Routines
        private void UpdateGraphReference()
        {
            if (System.IO.File.Exists(GraphPath))
            {
                CanvasSerialization subgraph = new Subgraph().Load(_graphPath);
                
                // Clear pins
                Input.Clear();
                Output.Clear();
                InputDefinitions.Clear();
                OutputDefinitions.Clear();
                
                // Find inputs
                foreach (GraphInputOutputDefinition definition in subgraph.Nodes
                    .Where(n => n is GraphInput).OfType<GraphInput>()
                    .SelectMany(i => i.Definitions))
                {
                    Dictionary<string, GraphInputOutputDefinition> unique =
                        new Dictionary<string, GraphInputOutputDefinition>();
                    if (!unique.ContainsKey(definition.Name))
                    {
                        unique[definition.Name] = definition;
                        Input.Add(new InputConnector(CacheTypeHelper.ConvertToObjectType(definition.Type)) {Title = definition.Name});
                        InputDefinitions.Add(definition);
                    }
                }
                
                // Find outputs
                foreach (GraphInputOutputDefinition definition in subgraph.Nodes
                    .Where(n => n is GraphOutput).OfType<GraphOutput>()
                    .SelectMany(i => i.Definitions))
                {
                    Dictionary<string, GraphInputOutputDefinition> unique =
                        new Dictionary<string, GraphInputOutputDefinition>();
                    if (!unique.ContainsKey(definition.Name))
                    {
                        unique[definition.Name] = definition;
                        Output.Add(new OutputConnector(CacheTypeHelper.ConvertToObjectType(definition.Type)) {Title = definition.Name});
                        OutputDefinitions.Add(definition);
                    }
                }
                
                Message.Content = $"{subgraph.Nodes.Count} Nodes";
                Message.Type = NodeMessageType.Normal;
            }
        }
        private static Type GetInputNodeType(GraphInputOutputDefinition definition)
            => CacheTypeHelper.ConvertToNodeType(definition.Type);
        #endregion

        #region Private States
        private List<GraphInputOutputDefinition> InputDefinitions { get; set; } =
            new List<GraphInputOutputDefinition>();
        private List<GraphInputOutputDefinition> OutputDefinitions { get; set; } =
            new List<GraphInputOutputDefinition>();
        #endregion
        
        #region Processor Interface
        protected override NodeExecutionResult Execute()
        {
            Dictionary<string, object> parameterSet = new Dictionary<string, object>();
            foreach (InputConnector inputConnector in Input)
                parameterSet[inputConnector.Title] = inputConnector.FetchInputValue<object>();
            
            GraphReferenceParameter parameter = new GraphReferenceParameter()
            {
                InputGraph = GraphPath,
                InputParameterSet = parameterSet
            };
            BasicHelper.GraphReference(parameter);

            Dictionary<OutputConnector, object> cache = new Dictionary<OutputConnector, object>();
            foreach ((string key, object value) in parameter.OutputParameterSet)
            {
                OutputConnector output = Output.SingleOrDefault(o => o.Title == key);
                if (output != null) 
                    cache[output] = value;   
            }

            return new NodeExecutionResult(new NodeMessage($"{parameterSet.Count} Inputs -> {parameter.OutputParameterSet.Count} Outputs"), cache);
        }
        #endregion

        #region Serialization
        protected override Dictionary<string, NodeSerializationRoutine> ProcessorNodeMemberSerialization { get; }
        protected override NodeSerializationRoutine VariantInputConnectorsSerialization { get; } = null;
        #endregion

        #region Auto Generate Interface
        public override Tuple<ToolboxNodeExport, Vector2D, InputConnector>[] AutoGenerateNodes
        {
            get
            {
                List<Tuple<ToolboxNodeExport, Vector2D, InputConnector>> auto =
                    new List<Tuple<ToolboxNodeExport, Vector2D, InputConnector>>();
                
                // Add nodes for additional variable-inputs
                for (int i = 0; i < Input.Count; i++)
                {
                    if(Input[i].Connections.Count != 0) continue;

                    ToolboxNodeExport toolDef = new ToolboxNodeExport(Input[i].Title, GetInputNodeType(InputDefinitions[i]));
                    auto.Add(new Tuple<ToolboxNodeExport, Vector2D, InputConnector>(toolDef, new Vector2D(-100, -50 + (i - 1) * 50), Input[i]));
                }
                
                return auto.ToArray();
            }
        }
        public override bool ShouldHaveAutoConnection => Input.Count > 0 && Input.Any(i => i.Connections.Count == 0);
        #endregion
    }
}