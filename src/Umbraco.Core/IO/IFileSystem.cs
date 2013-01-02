﻿using System;
using System.Collections.Generic;
using System.IO;
using Umbraco.Core.CodeAnnotations;

namespace Umbraco.Core.IO
{
	[UmbracoExperimentalFeature("http://issues.umbraco.org/issue/U4-1156", "Will be declared public after 4.10")]
    internal interface IFileSystem
    {
        IEnumerable<string> GetDirectories(string path);

        void DeleteDirectory(string path);

        void DeleteDirectory(string path, bool recursive);

        bool DirectoryExists(string path);


        void AddFile(string path, Stream stream);

        void AddFile(string path, Stream stream, bool overrideIfExists);

        IEnumerable<string> GetFiles(string path);

        IEnumerable<string> GetFiles(string path, string filter);

        Stream OpenFile(string path);

        void DeleteFile(string path);

        bool FileExists(string path);


        string GetRelativePath(string fullPathOrUrl);

        string GetFullPath(string path);

        string GetUrl(string path);

        DateTimeOffset GetLastModified(string path);

        DateTimeOffset GetCreated(string path);
    }
}
