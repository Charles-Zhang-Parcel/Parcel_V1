using System.Reflection;
using Parcel.Shared.DataTypes;
using Parcel.Shared.Framework;

namespace Parcel.Toolbox.MachineLearning
{
    public class ToolboxDefinition : IToolboxEntry
    {
        #region Interface
        public string ToolboxName => "Machine Learning";
        public string ToolboxAssemblyFullName => Assembly.GetExecutingAssembly().FullName;
        public ToolboxNodeExport[] ExportNodes => new ToolboxNodeExport[]
        {
        };
        public AutomaticNodeDescriptor[] AutomaticNodes => new AutomaticNodeDescriptor[]
        {
            // GenAI
            new AutomaticNodeDescriptor("ChatGPT 4", new []{CacheDataType.String, CacheDataType.String}, CacheDataType.String,
                objects => (string)objects[1])
            {
                InputNames = new[]{ "Endpoint", "Query" },
                OutputNames = new[]{ "Completion" }
            },
            new AutomaticNodeDescriptor("Google Gemini", new []{CacheDataType.String, CacheDataType.String}, CacheDataType.String,
                objects => (string)objects[1])
            {
                InputNames = new[]{ "Endpoint", "Query" },
                OutputNames = new[]{ "Completion" }
            },
            null, // Divisor line
        };
        #endregion
    }
}