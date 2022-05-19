using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

namespace eBoks_OpenAPI_Aggregator
{
    static class Program
    {
        static void Main(string[] args)
        {
            // Get current project directory
            string projectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
            
            // Creating a ArrayList with the path to all .yaml files and a counter for how many files are present in directory
            ArrayList PathList = new ArrayList();

            // Looping through each .yaml file in the directory and adding to ArrayList
            // we are also performing some additonal commands in order to find files with both .yaml and .yml extension
            string[] formats = { ".yaml", ".yml" };
            foreach (string file in Directory.EnumerateFiles(projectDirectory, "*.*", SearchOption.AllDirectories).Where(x => formats.Any(x.EndsWith)))
            {
                PathList.Add(file);
            }

            // Creating a OpenAPI specification 
            var document = new OpenApiDocument
            {
                Info = new OpenApiInfo
                {
                    Version = "1.0.0",
                    Title = "e-Boks OpenAPI Aggregator",
                },
                Servers = new List<OpenApiServer>
            {
                new OpenApiServer {Url = "http://fictional.eboks.dk/api"}
            },
                Paths = new OpenApiPaths
                {
                },
                Components = new OpenApiComponents()
                {
                    Responses = new Dictionary<string, OpenApiResponse>(),
                    Schemas = new Dictionary<string, OpenApiSchema>()
                },
            };

            // Booleans that determine if a operation (post,put,etc) is public or not (x-public-dpp) 
            bool publicOp = true;

            // This dict is used to store and create the new variable names, when a duplicate component is found
            Dictionary<string, int> componentIterators = new Dictionary<string, int>();
            

            // This is the main-loop that is handles each individual .yaml file and adds them to the aggregated output file
            foreach (string YamlFilePath in PathList)
            {
                StreamReader stream = new StreamReader(YamlFilePath);
                var importedOpenApiDocument = new OpenApiStreamReader().Read(stream.BaseStream, out var diagnostic);

                // Creating arrays with key/index from input file
                var path_key_array = importedOpenApiDocument.Paths.Keys.ToArray();
                var path_value_array = importedOpenApiDocument.Paths.Values.ToArray();
                // Creating tag array from input file
                var tag_item_array = importedOpenApiDocument.Tags;


                // The publicOp boolean determines whether we add the x-public-dpp or not.
                foreach (var xvz in importedOpenApiDocument.Paths.Values)
                {
                    foreach (var cvb in xvz.Operations.Values)
                    {
                        if (publicOp == true)
                        {
                            cvb.Extensions.Add("x-public-dpp", new OpenApiBoolean(true));
                        }
                        else
                        {
                            cvb.Extensions.Add("x-public-dpp", new OpenApiBoolean(false));

                        }
                    }
                }


                // For loop inserting the paths into new specification
                for (int i = 0; i < path_key_array.Length; i++)
                {
                    document.Paths.Add(path_key_array[i], path_value_array[i]);
                }

                // For loop inserting the tags into new specification
                foreach (var tag_item in tag_item_array)
                {
                    document.Tags.Add(tag_item);
                }


                // Components --> Callback
                foreach (var callback_ite in importedOpenApiDocument.Components.Callbacks)
                {
                    if (document.Components.Callbacks.ContainsKey(callback_ite.Key))
                    {
                        if (componentIterators.ContainsKey(callback_ite.Key)) { componentIterators[callback_ite.Key] += 1; }
                        else { componentIterators.Add(callback_ite.Key, 2); }

                        string callbackNewKeyName = callback_ite.Key + componentIterators[callback_ite.Key];

                        callback_ite.Value.Reference.Id = callbackNewKeyName;
                        document.Components.Callbacks.Add(callbackNewKeyName, callback_ite.Value);
                    }
                    else
                    {
                        document.Components.Callbacks.Add(callback_ite.Key, callback_ite.Value);
                    }
                }

                // Components --> Examples
                foreach (var examples_ite in importedOpenApiDocument.Components.Examples)
                {
                    if (document.Components.Examples.ContainsKey(examples_ite.Key))
                    {
                        if (componentIterators.ContainsKey(examples_ite.Key)) { componentIterators[examples_ite.Key] += 1; }
                        else { componentIterators.Add(examples_ite.Key, 2); }

                        string examplesNewKeyName = examples_ite.Key + componentIterators[examples_ite.Key];

                        examples_ite.Value.Reference.Id = examplesNewKeyName;
                        document.Components.Examples.Add(examplesNewKeyName, examples_ite.Value);
                    }
                    else
                    {
                        document.Components.Examples.Add(examples_ite.Key, examples_ite.Value);
                    }
                }

                // Components --> Extensions
                foreach (var extensions_ite in importedOpenApiDocument.Components.Extensions)
                {

                    if (document.Components.Extensions.ContainsKey(extensions_ite.Key))
                    {
                        Console.WriteLine(extensions_ite.Key);
                        continue;
                    }
                    else
                    {
                        Console.WriteLine(extensions_ite.Key);
                        document.Components.Extensions.Add(extensions_ite.Key, extensions_ite.Value);
                    }

                }

                // Components --> Headers
                foreach (var headers_ite in importedOpenApiDocument.Components.Headers)
                {
                    if (document.Components.Headers.ContainsKey(headers_ite.Key))
                    {
                        if (componentIterators.ContainsKey(headers_ite.Key)) { componentIterators[headers_ite.Key] += 1; }
                        else { componentIterators.Add(headers_ite.Key, 2); }

                        string headersNewKeyName = headers_ite.Key + componentIterators[headers_ite.Key];

                        headers_ite.Value.Reference.Id = headersNewKeyName;
                        document.Components.Headers.Add(headersNewKeyName, headers_ite.Value);
                    }
                    else
                    {
                        document.Components.Headers.Add(headers_ite.Key, headers_ite.Value);
                    }
                }

                // Components --> Links
                foreach (var links_ite in importedOpenApiDocument.Components.Links)
                {
                    if (document.Components.Links.ContainsKey(links_ite.Key))
                    {
                        if (componentIterators.ContainsKey(links_ite.Key)) { componentIterators[links_ite.Key] += 1; }
                        else { componentIterators.Add(links_ite.Key, 2); }

                        string linksNewKeyName = links_ite.Key + componentIterators[links_ite.Key];


                        links_ite.Value.Reference.Id = linksNewKeyName;
                        document.Components.Links.Add(linksNewKeyName, links_ite.Value);
                    }
                    else
                    {
                        document.Components.Links.Add(links_ite.Key, links_ite.Value);
                    }
                }

                // Components --> Parameters
                foreach (var parameters_ite in importedOpenApiDocument.Components.Parameters)
                {
                    if (document.Components.Parameters.ContainsKey(parameters_ite.Key))
                    {
                        if (componentIterators.ContainsKey(parameters_ite.Key)) { componentIterators[parameters_ite.Key] += 1; }
                        else { componentIterators.Add(parameters_ite.Key, 2); }

                        string parametersNewKeyName = parameters_ite.Key + componentIterators[parameters_ite.Key];

                        parameters_ite.Value.Reference.Id = parametersNewKeyName;
                        document.Components.Parameters.Add(parametersNewKeyName, parameters_ite.Value);
                    }
                    else
                    {
                        document.Components.Parameters.Add(parameters_ite.Key, parameters_ite.Value);
                    }
                }

                // Components --> Responses
                // The responses must be for each of its own componoents, content, description etc forloop in for forloop might work
                foreach (var responses_ite in importedOpenApiDocument.Components.Responses)
                {
                    if (document.Components.Responses.ContainsKey(responses_ite.Key))
                    {
                        if (componentIterators.ContainsKey(responses_ite.Key)) { componentIterators[responses_ite.Key] += 1; }
                        else { componentIterators.Add(responses_ite.Key, 2); }

                        string responseNewKeyName = responses_ite.Key + componentIterators[responses_ite.Key];

                        responses_ite.Value.Reference.Id = responseNewKeyName;
                        document.Components.Responses.Add(responseNewKeyName, responses_ite.Value);
                    }
                    else
                    {
                        document.Components.Responses.Add(responses_ite.Key, responses_ite.Value);
                    }
                }



                // Components --> Schemas
                foreach (var schemas_ite in importedOpenApiDocument.Components.Schemas)
                {
                    if (document.Components.Schemas.ContainsKey(schemas_ite.Key))
                    {
                        if (componentIterators.ContainsKey(schemas_ite.Key)) { componentIterators[schemas_ite.Key] += 1; }
                        else { componentIterators.Add(schemas_ite.Key, 2); }

                        // If key already exists, we create a new key by firstly adding "2" to the end of the original key name
                        string schemesNewKeyName = schemas_ite.Key + componentIterators[schemas_ite.Key];

                        // If key already exists, we set its reference id to new key name we just created above (xNew)
                        // This will allow the serializer to differentiate between these components/schemes
                        schemas_ite.Value.Reference.Id = schemesNewKeyName;

                        // Adding new key Name and value to the final merged OpenAPI document 
                        document.Components.Schemas.Add(schemesNewKeyName, schemas_ite.Value);
                    }
                    else
                    {
                        // Key does not exist, so it will use original key name
                        document.Components.Schemas.Add(schemas_ite.Key, schemas_ite.Value);
                    }
                }



                // Components --> Request Bodies
                foreach (var request_ite in importedOpenApiDocument.Components.RequestBodies)
                {
                    if (document.Components.RequestBodies.ContainsKey(request_ite.Key))
                    {
                        if (componentIterators.ContainsKey(request_ite.Key)) { componentIterators[request_ite.Key] += 1; }
                        else { componentIterators.Add(request_ite.Key, 2); }

                        string requestNewKeyName = request_ite.Key + componentIterators[request_ite.Key];

                        request_ite.Value.Reference.Id = requestNewKeyName;
                        document.Components.RequestBodies.Add(requestNewKeyName, request_ite.Value);
                    }
                    else
                    {
                        document.Components.RequestBodies.Add(request_ite.Key, request_ite.Value);
                    }
                }

                // Components --> Secruity Schemes
                foreach (var sec_ite in importedOpenApiDocument.Components.SecuritySchemes)
                {
                    if (document.Components.SecuritySchemes.ContainsKey(sec_ite.Key))
                    {
                        if (componentIterators.ContainsKey(sec_ite.Key)) { componentIterators[sec_ite.Key] += 1; }
                        else { componentIterators.Add(sec_ite.Key, 2); }

                        string secSchemeNewKeyName = sec_ite.Key + componentIterators[sec_ite.Key];

                        sec_ite.Value.Reference.Id = secSchemeNewKeyName;
                        document.Components.SecuritySchemes.Add(secSchemeNewKeyName, sec_ite.Value);
                    }
                    else
                    {
                        document.Components.SecuritySchemes.Add(sec_ite.Key, sec_ite.Value);
                    }
                }

                // Closes the StreamReader per file loop
                stream.Dispose();
            }

            // Writing as V3 to file
            var xxx = document.SerializeAsYaml(OpenApiSpecVersion.OpenApi3_0);
            Console.WriteLine(xxx);
     
            
            using (StreamWriter sw = File.CreateText(projectDirectory + @"\eboks_agg.yml"))
            {
                sw.Write(xxx);
            }

        }
    }
}
