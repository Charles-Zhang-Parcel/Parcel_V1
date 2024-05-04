using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Parcel.Shared.DataTypes;
using Parcel.Shared.Framework.ViewModels.Primitives;

namespace Parcel.Shared.Framework.ViewModels.BaseNodes
{
    /// <summary>
    /// An encapsulation of a base node instance that's generated directly from methods;
    /// We will start with only a single output but there shouldn't be much difficulty outputting more outputs
    /// </summary>
    public class AutomaticProcessorNode: ProcessorNode
    {
        #region Constructor
        public AutomaticProcessorNode()
        {
            ProcessorNodeMemberSerialization = new Dictionary<string, NodeSerializationRoutine>()
            {
                {nameof(AutomaticNodeType), new NodeSerializationRoutine(() => AutomaticNodeType, value => AutomaticNodeType = value as string)},
                {nameof(ToolboxFullName), new NodeSerializationRoutine(() => ToolboxFullName, value => ToolboxFullName = value as string)},
                {nameof(InputTypes), new NodeSerializationRoutine(() => InputTypes, value => InputTypes = value as CacheDataType[])},
                {nameof(OutputTypes), new NodeSerializationRoutine(() => OutputTypes, value => OutputTypes = value as CacheDataType[])},
                {nameof(InputNames), new NodeSerializationRoutine(() => InputNames, value => InputNames = value as string[])},
                {nameof(OutputNames), new NodeSerializationRoutine(() => OutputNames, value => OutputNames = value as string[])},
            };
        }
        public AutomaticProcessorNode(AutomaticNodeDescriptor descriptor, IToolboxEntry toolbox)
            :this()
        {
            // Serialization
            AutomaticNodeType = descriptor.NodeName;
            ToolboxFullName = toolbox.GetType().AssemblyQualifiedName;
            InputTypes = descriptor.InputTypes;
            OutputTypes = descriptor.OutputTypes;
            InputNames = descriptor.InputNames;
            OutputNames = descriptor.OutputNames;
            
            // Population
            PopulateInputsOutputs();
        }
        #endregion

        #region Routines
        private Func<object[], object[]> RetrieveCallMarshal()
        {
            try
            {
                IToolboxEntry toolbox = (IToolboxEntry) Activator.CreateInstance(Type.GetType(ToolboxFullName));
                AutomaticNodeDescriptor descriptor = toolbox.AutomaticNodes.Single(an => an != null && an.NodeName == AutomaticNodeType);
                return descriptor.CallMarshal;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Failed to retrieve node: {e.Message}.");
            }
        }
        private void PopulateInputsOutputs()
        {
            Title = NodeTypeName = AutomaticNodeType;
            for (int index = 0; index < InputTypes.Length; index++)
            {
                CacheDataType inputType = InputTypes[index];
                string preferredTitle = InputNames?[index];
                switch (inputType)
                {
                    case CacheDataType.Boolean:
                        Input.Add(new PrimitiveBooleanInputConnector() {Title = preferredTitle ?? "Bool"});
                        break;
                    case CacheDataType.Number:
                        Input.Add(new PrimitiveNumberInputConnector() {Title = preferredTitle ?? "Number"});
                        break;
                    case CacheDataType.String:
                        Input.Add(new PrimitiveStringInputConnector() {Title = preferredTitle ?? "String"});
                        break;
                    case CacheDataType.DateTime:
                        Input.Add(new PrimitiveDateTimeInputConnector() {Title = preferredTitle ?? "Date"});
                        break;
                    case CacheDataType.ParcelDataGrid:
                        Input.Add(new InputConnector(typeof(DataGrid)) {Title = preferredTitle ?? "Data"});
                        break;
                    case CacheDataType.Generic:
                        Input.Add(new InputConnector(typeof(object)) { Title = preferredTitle ?? "Entity" });
                        break;
                    case CacheDataType.BatchJob:
                    case CacheDataType.ServerConfig:
                        throw new NotImplementedException();
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            for (int index = 0; index < OutputTypes.Length; index++)
            {
                CacheDataType outputType = OutputTypes[index];
                string preferredTitle = OutputNames?[index];
                switch (outputType)
                {
                    case CacheDataType.Boolean:
                        Output.Add(new OutputConnector(typeof(bool)) {Title = preferredTitle ?? "Truth"});
                        break;
                    case CacheDataType.Number:
                        Output.Add(new OutputConnector(typeof(double)) {Title = preferredTitle ?? "Number"});
                        break;
                    case CacheDataType.String:
                        Output.Add(new OutputConnector(typeof(string)) {Title = preferredTitle ?? "Value"});
                        break;
                    case CacheDataType.DateTime:
                        Output.Add(new OutputConnector(typeof(DateTime)) {Title = preferredTitle ?? "Date"});
                        break;
                    case CacheDataType.ParcelDataGrid:
                        Output.Add(new OutputConnector(typeof(DataGrid)) {Title = preferredTitle ?? "Data"});
                        break;
                    case CacheDataType.Generic:
                        Output.Add(new OutputConnector(typeof(object)) { Title = preferredTitle ?? "Entity" });
                        break;
                    case CacheDataType.BatchJob:
                    case CacheDataType.ServerConfig:
                        throw new NotImplementedException();
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        #endregion

        #region Properties
        private string AutomaticNodeType { get; set; }
        private string ToolboxFullName { get; set; }
        private CacheDataType[] InputTypes { get; set; }
        private CacheDataType[] OutputTypes { get; set; }
        private string[] InputNames { get; set; }
        private string[] OutputNames { get; set; }
        #endregion

        #region Processor Interface
        protected override NodeExecutionResult Execute()
        {
            Dictionary<OutputConnector, object> cache = new Dictionary<OutputConnector, object>();
            
            Func<object[], object[]> marshal = RetrieveCallMarshal();
            object[] outputs = marshal.Invoke(Input.Select(i => i.FetchInputValue<object>()).ToArray());
            for (int index = 0; index < outputs.Length; index++)
            {
                object output = outputs[index];
                OutputConnector connector = Output[index];
                cache[connector] = output;
            }

            return new NodeExecutionResult(new NodeMessage(), cache);
        }
        #endregion

        #region Serialization
        protected sealed override Dictionary<string, NodeSerializationRoutine> ProcessorNodeMemberSerialization { get; }
        internal override void PostDeserialization()
        {
            base.PostDeserialization();
            PopulateInputsOutputs();
        }
        protected override NodeSerializationRoutine VariantInputConnectorsSerialization { get; } = null;
        #endregion

        #region Auto-Connect Interface
        public override Tuple<ToolboxNodeExport, Vector2D, InputConnector>[] AutoGenerateNodes
        {
            get
            {
                List<Tuple<ToolboxNodeExport, Vector2D, InputConnector>> auto =
                    new List<Tuple<ToolboxNodeExport, Vector2D, InputConnector>>();
                for (int i = 0; i < Input.Count; i++)
                {
                    if(!InputConnectorShouldRequireAutoConnection(Input[i])) continue;

                    Type nodeType = CacheTypeHelper.ConvertToNodeType(InputTypes[i]);
                    ToolboxNodeExport toolDef = new ToolboxNodeExport(Input[i].Title, nodeType);
                    auto.Add(new Tuple<ToolboxNodeExport, Vector2D, InputConnector>(toolDef, new Vector2D(-100, -50 + (i - 1) * 50), Input[i]));
                }
                return auto.ToArray();
            }
        }

        public override bool ShouldHaveAutoConnection => Input.Count > 0 && Input.Any(InputConnectorShouldRequireAutoConnection);
        #endregion
    }
}