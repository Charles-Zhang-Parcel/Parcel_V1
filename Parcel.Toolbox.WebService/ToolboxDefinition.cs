using System.Reflection;
using Parcel.Shared.DataTypes;
using Parcel.Shared.Framework;

namespace Parcel.Toolbox.WebService
{
    public class ToolboxDefinition : IToolboxEntry
    {
        #region Interface
        public string ToolboxName => "Web Service";
        public string ToolboxAssemblyFullName => Assembly.GetExecutingAssembly().FullName;
        public ToolboxNodeExport[] ExportNodes => Array.Empty<ToolboxNodeExport>();
        public AutomaticNodeDescriptor[] AutomaticNodes => new AutomaticNodeDescriptor[] 
        {
            new AutomaticNodeDescriptor("Get Weather At Location", new []{CacheDataType.String, CacheDataType.DateTime}, CacheDataType.String,
                objects =>
                {
                    string location = (string)objects[0];
                    double date = (double)objects[1];

                    HttpClient client = new HttpClient() {BaseAddress = new Uri("https://api.open-meteo.com/v1")};
                    HttpResponseMessage response = client.GetAsync("forecast?latitude=52.52&longitude=13.41&current=temperature_2m,wind_speed_10m&hourly=temperature_2m,relative_humidity_2m,wind_speed_10m").Result;
                    string json = response.Content.ReadAsStringAsync().Result;
                    // Extract actual weather data
                    // ...
                    return json;
                })
            {
                InputNames = new string[]{ "Location", "Date" }
            },

            // Demo
            new AutomaticNodeDescriptor("Fetch from Database", new []{CacheDataType.String}, CacheDataType.ParcelDataGrid,
                objects => null)
            {
                InputNames = new string[]{ "Query" },
                OutputNames = new string[] { "Table" }
            },
            new AutomaticNodeDescriptor("Return", new []{CacheDataType.ParcelDataGrid}, CacheDataType.ParcelDataGrid,
                objects => null)
            {
                InputNames = new string[]{ "Data" },
                OutputNames = new string[] { "Data" }
            },
        };
        #endregion
    }
}