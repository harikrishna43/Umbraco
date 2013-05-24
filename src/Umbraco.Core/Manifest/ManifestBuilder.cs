﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Umbraco.Core.IO;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Core.Manifest
{
    /// <summary>
    /// This reads in the manifests and stores some definitions in memory so we can look them on the server side
    /// </summary>
    internal class ManifestBuilder
    {

        private static readonly ConcurrentDictionary<string, object> StaticCache = new ConcurrentDictionary<string, object>();
        
        private const string ManifestKey = "manifests";
        private const string PropertyEditorsKey = "propertyeditors";

        /// <summary>
        /// Returns all property editors found in the manfifests
        /// </summary>
        internal static IEnumerable<PropertyEditor> PropertyEditors
        {
            get
            {
                return (IEnumerable<PropertyEditor>) StaticCache.GetOrAdd(
                    PropertyEditorsKey,
                    s =>
                        {
                            var editors = new List<PropertyEditor>();
                            foreach (var manifest in GetManifests())
                            {
                                editors.AddRange(ManifestParser.GetPropertyEditors(manifest.PropertyEditors));
                            }
                            return editors;
                        });
            }
        } 

        /// <summary>
        /// Ensures the manifests are found and loaded into memory
        /// </summary>
        private static IEnumerable<PackageManifest> GetManifests()
        {
            return (IEnumerable<PackageManifest>) StaticCache.GetOrAdd(ManifestKey, s =>
                {
                    var parser = new ManifestParser(new DirectoryInfo(IOHelper.MapPath("~/App_Plugins")));
                    return parser.GetManifests();
                });
        }

    }
}