using Parcel.Shared.DataTypes;
using Parcel.Shared.Framework;
using System.Reflection;

namespace Parcel.Toolbox.Demo
{
    public class ToolboxDefinition: IToolboxEntry
    {
        #region Interface
        public string ToolboxName => "Demo";
        public string ToolboxAssemblyFullName => Assembly.GetExecutingAssembly().FullName;
        public ToolboxNodeExport[] ExportNodes => new ToolboxNodeExport[]
        {
        };
        public AutomaticNodeDescriptor[] AutomaticNodes => new AutomaticNodeDescriptor[]
        {
            // Plotting
            new AutomaticNodeDescriptor("Line Chart", new []{CacheDataType.ParcelDataGrid}, CacheDataType.Generic,
                objects => null)
            {
                InputNames = new[]{ "Data" },
                OutputNames = new[]{ "Chart" }
            },
            null, // Divisor line
            new AutomaticNodeDescriptor("Export Element", new []{CacheDataType.String, CacheDataType.Generic}, Array.Empty<CacheDataType>(),
                objects => null)
            {
                InputNames = new[]{ "Name", "Entity" }
            },
        };
        #endregion
    }
}