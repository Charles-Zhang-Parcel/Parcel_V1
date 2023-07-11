﻿using System.Collections.Generic;
using System.Linq;
using Parcel.Shared.Algorithms;
using Parcel.Shared.Framework.Advanced;
using Parcel.Shared.Framework.ViewModels;

namespace Parcel.Shared.Serialization
{
    public class Subgraph
    {
        public CanvasSerialization Load(string filePath)
        {
            CanvasSerialization loaded = new GraphSerializer().Deserialize(filePath, new NodesCanvas());
            return loaded;
        }

        public Dictionary<string, object> Execute(NodesCanvas canvas, Dictionary<string, object> inputs)
        {
            // Populate inputs
            foreach (GraphInput inputNode in canvas.Nodes
                .Where(n => n is GraphInput).OfType<GraphInput>())
            {
                foreach (GraphInputOutputDefinition definition in inputNode.Definitions)
                {
                    if (inputs.ContainsKey(definition.Name))
                        definition.Payload = inputs[definition.Name];
                }
            }

            // Execute
            foreach (GraphOutput outputNode in canvas.Nodes
                .Where(n => n is GraphOutput).OfType<GraphOutput>())
                outputNode.IsPreview = true;    // Turn on preview for output nodes
            AlgorithmHelper.ExecuteGraph(canvas);
            
            // Fetch outputs
            Dictionary<string, object> outputs = new Dictionary<string, object>();
            foreach (GraphOutput outputNode in canvas.Nodes
                .Where(n => n is GraphOutput).OfType<GraphOutput>())
            {
                foreach (GraphInputOutputDefinition definition in outputNode.Definitions)
                    outputs[definition.Name] = definition.Payload;
            }

            return outputs;
        }
    }
}