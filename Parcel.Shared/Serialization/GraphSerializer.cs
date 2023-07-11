using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Parcel.Shared.Framework.ViewModels;
using Parcel.Shared.Framework.ViewModels.BaseNodes;

namespace Parcel.Shared.Serialization
{
    internal class GraphSerializer
    {
        #region Interface
        public void Serialize(string filePath, CanvasSerialization canvas)
        {
            // Book-keeping structures
            Dictionary<BaseNode, NodeData> nodeMapping = new Dictionary<BaseNode, NodeData>();

            // Serialize nodes
            List<NodeData> nodes = canvas.Nodes.Select(n =>
            {
                NodeData serialized = n.Serialize();
                nodeMapping[n] = serialized;
                return serialized;
            }).ToList();
            
            // Serialize connections
            List<ConnectionData> connections = canvas.Connections.Select(c =>
            {
                return new ConnectionData()
                {
                    Source = nodeMapping[c.Input.Node],
                    SourcePin = c.Input.Node.GetOutputPinID(c.Input as OutputConnector),
                    Destination = nodeMapping[c.Output.Node],
                    DestinationPin = c.Output.Node.GetInputPinID(c.Output as InputConnector)
                };
            }).ToList();
            
            // Serialize
            NodeGraph graph = new NodeGraph()
            {
                Nodes = nodes,
                Connections = connections
            };
            using Stream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
            using BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8, false);
            WriteToStream(writer, graph);
        }
        public CanvasSerialization Deserialize(string filePath, NodesCanvas canvas)
        {
            // Load raw graph data
            using Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using BinaryReader reader = new BinaryReader(stream, Encoding.UTF8, false);
            NodeGraph graph = ReadFromStream(reader);
            
            // Book-keeping structures
            Dictionary<NodeData, BaseNode> nodeMapping = new Dictionary<NodeData, BaseNode>();
            
            // Deserialize nodes
            List<BaseNode> nodes = graph.Nodes.Select(n =>
            {
                BaseNode deserialized = n.Deserialize(canvas);
                nodeMapping[n] = deserialized;
                return deserialized;
            }).ToList();
            // Deserialize connections
            List<BaseConnection> connections = graph.Connections.Select(c =>
            {
                return new BaseConnection()
                {
                    Graph = canvas,
                    Input = nodeMapping[c.Source].GetOutputPin(c.SourcePin),
                    Output = nodeMapping[c.Destination].GetInputPin(c.DestinationPin)
                };
            }).ToList();
            
            // Reconstruct canvas
            CanvasSerialization loaded = new CanvasSerialization()
            {
                Nodes = nodes,
                Connections = connections
            };
            return loaded;
        }
        #endregion

        #region Binary Serialization
        private void WriteToStream(BinaryWriter writer, NodeGraph graph)
        {
            writer.Write(graph.Version);
            writer.Write(graph.Title);
            writer.Write(graph.Author);
            writer.Write(graph.Description);
            writer.Write(graph.CreationTime.ToString("yyyy-MM-dd"));
            writer.Write(graph.UpdateTime.ToString("yyyy-MM-dd"));
            writer.Write(graph.Revision);

            writer.Write(graph.Nodes.Count);
            foreach (NodeData node in graph.Nodes)
            {
                writer.Write(node.NodeType);
                writer.Write(node.NodeMembers.Count());
                foreach (KeyValuePair<string, object> member in node.NodeMembers)
                    throw new NotImplementedException();
            }
        }
        private NodeGraph ReadFromStream(BinaryReader reader)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}