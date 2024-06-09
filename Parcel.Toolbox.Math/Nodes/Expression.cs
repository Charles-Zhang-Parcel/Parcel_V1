﻿using System;
using System.Collections.Generic;
using System.Linq;
using Parcel.Shared.DataTypes;
using Parcel.Shared.Framework;
using Parcel.Shared.Framework.ViewModels;
using Parcel.Shared.Framework.ViewModels.BaseNodes;
using Parcel.Shared.Framework.ViewModels.Primitives;
using Parcel.Shared.Serialization;

namespace Parcel.Toolbox.Math.Nodes
{
    public class Expression: ProcessorNode
    {
        #region Public View Properties
        private string _value;
        public string Value
        {
            get => _value;
            set => SetField(ref _value, value);
        }
        #endregion
        
        #region Node Interface
        private readonly OutputConnector _resultOutput = new OutputConnector(typeof(double))
        {
            Title = "Result",
        };
        public Expression()
        {
            ProcessorNodeMemberSerialization = new Dictionary<string, NodeSerializationRoutine>()
            {
                {nameof(Value), new NodeSerializationRoutine( () => SerializationHelper.Serialize(_value), value => _value = SerializationHelper.GetString(value))},
            };
            VariantInputConnectorsSerialization = new NodeSerializationRoutine(() => SerializationHelper.Serialize(Input.Count), o =>
            {
                Input.Clear();
                int count = SerializationHelper.GetInt(o);
                for (int i = 0; i < count; i++)
                    AddInputs();
            });
            
            Title = NodeTypeName = "Expression";
            Output.Add(_resultOutput);
            
            AddInputs();
            AddInputs();
            
            AddEntryCommand = new RequeryCommand(
                AddInputs,
                () => Input.Count < 9);
            RemoveEntryCommand = new RequeryCommand(
                RemoveInputs,
                () => Input.Count > 0);
        }
        #endregion
        
        #region Input Entries
        public IProcessorNodeCommand AddEntryCommand { get; }
        public IProcessorNodeCommand RemoveEntryCommand { get; }
        #endregion
        
        #region Routines
        private void AddInputs()
        {
            Input.Add(new InputConnector(typeof(double)){Title = $"Input {Input.Count + 1}"});
        }
        private void RemoveInputs()
        {
            Input.RemoveAt(Input.Count - 1);
        }
        #endregion

        #region Processor Interface
        protected override NodeExecutionResult Execute()
        {
            int i = 1;
            string replaced = Value;
            foreach (InputConnector input in Input)
            {
                replaced = replaced.Replace($"${i}", $"{input.FetchInputValue<double>()}");
                i++;
            }

            object result = new CodingSeb.ExpressionEvaluator.ExpressionEvaluator().Evaluate(replaced);
            
            return new NodeExecutionResult(new NodeMessage($"{result}"), new Dictionary<OutputConnector, object>()
            {
                {_resultOutput, result}
            });
        }
        #endregion
        
        #region Serialization
        protected override Dictionary<string, NodeSerializationRoutine> ProcessorNodeMemberSerialization { get; }
        protected override NodeSerializationRoutine VariantInputConnectorsSerialization { get; }

        #endregion
        
        #region Auto Connect Interface
        public override bool ShouldHaveAutoConnection => Input.Count > 0 && Input.Any(i => i.Connections.Count == 0);
        public override Tuple<ToolboxNodeExport, Vector2D, InputConnector>[] AutoGenerateNodes
        {
            get
            {
                List<Tuple<ToolboxNodeExport, Vector2D, InputConnector>> auto =
                    new List<Tuple<ToolboxNodeExport, Vector2D, InputConnector>>();
                for (int i = 0; i < Input.Count; i++)
                {
                    if(Input[i].Connections.Count != 0) continue;

                    ToolboxNodeExport toolDef = new ToolboxNodeExport($"Input {i+1}", typeof(NumberNode));
                    auto.Add(new Tuple<ToolboxNodeExport, Vector2D, InputConnector>(toolDef, new Vector2D(-100, -50 + (i - 1) * 50), Input[i]));
                }
                return auto.ToArray();
            }
        }
        #endregion
    }
}