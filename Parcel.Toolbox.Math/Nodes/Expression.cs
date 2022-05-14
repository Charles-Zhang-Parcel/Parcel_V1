﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Parcel.Shared.Framework;
using Parcel.Shared.Framework.ViewModels;
using Parcel.Shared.Framework.ViewModels.BaseNodes;
using Parcel.Shared.Framework.ViewModels.Primitives;

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
                {nameof(Value), new NodeSerializationRoutine( () => _value, value => _value = value as string)}
            };
            
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
        #endregion
        
        #region Auto Connect Interface
        public override bool ShouldHaveConnection => Input.Count > 0 && Input.Any(i => i.Connections.Count == 0);
        public override Tuple<ToolboxNodeExport, Vector, InputConnector>[] AutoGenerateNodes
        {
            get
            {
                List<Tuple<ToolboxNodeExport, Vector, InputConnector>> auto =
                    new List<Tuple<ToolboxNodeExport, Vector, InputConnector>>();
                for (int i = 0; i < Input.Count; i++)
                {
                    if(Input[i].Connections.Count != 0) continue;

                    ToolboxNodeExport toolDef = new ToolboxNodeExport($"Input {i+1}", typeof(NumberNode));
                    auto.Add(new Tuple<ToolboxNodeExport, Vector, InputConnector>(toolDef, new Vector(-100, -50 + (i - 1) * 50), Input[i]));
                }
                return auto.ToArray();
            }
        }
        #endregion
    }
}