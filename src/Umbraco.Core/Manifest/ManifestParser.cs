﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core.IO;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Core.Manifest
{
    /// <summary>
    /// Parses the Main.js file and replaces all tokens accordingly.
    /// </summary>
    internal class ManifestParser
    {
        private readonly DirectoryInfo _pluginsDir;
        //used to strip comments
        private static readonly Regex Comments = new Regex("(/\\*.*\\*/)", RegexOptions.Compiled);

        public ManifestParser(DirectoryInfo pluginsDir)
        {
            if (pluginsDir == null) throw new ArgumentNullException("pluginsDir");
            _pluginsDir = pluginsDir;
        }

        /// <summary>
        /// Parse the property editors from the json array
        /// </summary>
        /// <param name="jsonEditors"></param>
        /// <returns></returns>
        internal static IEnumerable<PropertyEditor> GetPropertyEditors(JArray jsonEditors)
        {
            return JsonConvert.DeserializeObject<IEnumerable<PropertyEditor>>(jsonEditors.ToString(), new PropertyEditorConverter());
        }
        
        /// <summary>
        /// Get all registered manifests
        /// </summary>
        /// <returns></returns>
        public IEnumerable<PackageManifest> GetManifests()
        {
            //get all Manifest.js files in the appropriate folders
            var manifestFileContents = GetAllManfifestFileContents(_pluginsDir);
            return CreateManifests(manifestFileContents.ToArray());
        }

        /// <summary>
        /// Get the file contents from all declared manifest files
        /// </summary>
        /// <param name="currDir"></param>
        /// <returns></returns>
        private IEnumerable<string> GetAllManfifestFileContents(DirectoryInfo currDir)
        {
            var depth = FolderDepth(_pluginsDir, currDir);
            
            if (depth < 1)
            {
                var dirs = currDir.GetDirectories();
                var result = new List<string>();
                foreach (var d in dirs)
                {
                    result.AddRange(GetAllManfifestFileContents(d));
                }
                return result;
            }

            //look for files here
            return currDir.GetFiles("Package.manifest")
                          .Select(f => File.ReadAllText(f.FullName))
                          .ToList();

            return Enumerable.Empty<string>();
        }

        /// <summary>
        /// Get the folder depth compared to the base folder
        /// </summary>
        /// <param name="baseDir"></param>
        /// <param name="currDir"></param>
        /// <returns></returns>
        internal static int FolderDepth(DirectoryInfo baseDir, DirectoryInfo currDir)
        {
            var removed = currDir.FullName.Remove(0, baseDir.FullName.Length).TrimStart('\\').TrimEnd('\\');
            return removed.Split(new char[] {'\\'}, StringSplitOptions.RemoveEmptyEntries).Length;
        }

        /// <summary>
        /// Creates a list of PropertyEditorManifest from the file contents of each manifest file
        /// </summary>
        /// <param name="manifestFileContents"></param>
        /// <returns></returns>
        /// <remarks>
        /// This ensures that comments are removed (but they have to be /* */ style comments
        /// and ensures that virtual paths are replaced with real ones
        /// </remarks>
        internal static IEnumerable<PackageManifest> CreateManifests(params string[] manifestFileContents)
        {
            var result = new List<PackageManifest>();
            foreach (var m in manifestFileContents)
            {
                if (m.IsNullOrWhiteSpace()) continue;

                //remove any comments first
                Comments.Replace(m, match => "");

                
                var deserialized = JsonConvert.DeserializeObject<JObject>(m);

                //validate the config
                var config = deserialized.Properties().Where(x => x.Name == "config").ToArray();
                if (config.Length > 1)
                {
                    throw new FormatException("The manifest is not formatted correctly contains more than one 'config' element");
                }

                //validate the init
                var init = deserialized.Properties().Where(x => x.Name == "init").ToArray();
                if (init.Length > 1)
                {
                    throw new FormatException("The manifest is not formatted correctly contains more than one 'init' element");
                }
                
                //validate the property editors section
                var propEditors = deserialized.Properties().Where(x => x.Name == "propertyEditors").ToArray();
                if (propEditors.Length > 1)
                {
                    throw new FormatException("The manifest is not formatted correctly contains more than one 'propertyEditors' element");
                }

                var jConfig = config.Any() ? (JObject) deserialized["config"] : new JObject();
                ReplaceVirtualPaths(jConfig);

                //replace virtual paths for each property editor
                if (deserialized["propertyEditors"] != null)
                {
                    foreach (JObject p in deserialized["propertyEditors"])
                    {
                        if (p["editor"] != null)
                        {
                            ReplaceVirtualPaths((JObject) p["editor"]);
                        }
                        if (p["preValues"] != null)
                        {
                            ReplaceVirtualPaths((JObject)p["preValues"]);
                        }
                    }
                }
                

                var manifest = new PackageManifest()
                    {
                        JavaScriptConfig = jConfig,
                        JavaScriptInitialize = init.Any() ? (JArray)deserialized["init"] : new JArray(),
                        PropertyEditors = propEditors.Any() ? (JArray)deserialized["propertyEditors"] : new JArray(),
                    };
                result.Add(manifest);
            }
            return result;
        }

        /// <summary>
        /// Replaces any virtual paths found in properties
        /// </summary>
        /// <param name="jObj"></param>
        private static void ReplaceVirtualPaths(JObject jObj)
        {
            foreach (var p in jObj.Properties().Select(x => x.Value))
            {                
                if (p.Type == JTokenType.Object)
                {
                    //recurse
                    ReplaceVirtualPaths((JObject) p);
                }                
                else
                {
                    var value = p as JValue;
                    if (value != null)
                    {
                        if (value.Type == JTokenType.String)
                        {
                            if (value.Value<string>().StartsWith("~/"))
                            {
                                //replace the virtual path
                                value.Value = IOHelper.ResolveUrl(value.Value<string>());
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Merges two json objects together
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="donor"></param>
        /// <param name="keepOriginal">set to true if we will keep the receiver value if the proeprty already exists</param>
        /// <remarks>
        /// taken from 
        /// http://stackoverflow.com/questions/4002508/does-c-sharp-have-a-library-for-parsing-multi-level-cascading-json/4002550#4002550
        /// </remarks>
        internal static void MergeJObjects(JObject receiver, JObject donor, bool keepOriginal = false)
        {
            foreach (var property in donor)
            {
                var receiverValue = receiver[property.Key] as JObject;
                var donorValue = property.Value as JObject;
                if (receiverValue != null && donorValue != null)
                {
                    MergeJObjects(receiverValue, donorValue);
                }
                else if (receiver[property.Key] == null || !keepOriginal)
                {
                    receiver[property.Key] = property.Value;
                }
            }
        }

        /// <summary>
        /// Merges the donor array values into the receiver array
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="donor"></param>
        internal static void MergeJArrays(JArray receiver, JArray donor)
        {
            foreach (var item in donor)
            {
                if (!receiver.Any(x => x.Equals(item)))
                {
                    receiver.Add(item);   
                }
            }
        }

        
    }
}