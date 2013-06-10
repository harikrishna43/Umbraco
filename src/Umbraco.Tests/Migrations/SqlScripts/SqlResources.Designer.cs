﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18034
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Umbraco.Tests.Migrations.SqlScripts {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class SqlResources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal SqlResources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Umbraco.Tests.Migrations.SqlScripts.SqlResources", typeof(SqlResources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to /*******************************************************************************************
        ///
        ///
        ///
        ///
        ///
        ///
        ///
        ///    Umbraco database installation script for MySQL
        /// 
        ///IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT
        /// 
        ///    Database version: 4.8.0.4
        ///    
        ///    Please increment this version number if ANY change is made to this script,
        ///    so compatibility with scripts for other database systems can be verified easily.
        ///    The first 3 digits depict the Umbraco version, t [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string MySqlTotal_480 {
            get {
                return ResourceManager.GetString("MySqlTotal_480", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to CREATE TABLE [umbracoUserType] (
        ///  [id] int NOT NULL  IDENTITY (5,1)
        ///, [userTypeAlias] nvarchar(50) NULL
        ///, [userTypeName] nvarchar(255) NOT NULL
        ///, [userTypeDefaultPermissions] nvarchar(50) NULL
        ///);
        ///GO
        ///CREATE TABLE [umbracoUserLogins] (
        ///  [contextID] uniqueidentifier NOT NULL
        ///, [userID] int NOT NULL
        ///, [timeout] bigint NOT NULL
        ///);
        ///GO
        ///CREATE TABLE [umbracoUser2NodePermission] (
        ///  [userId] int NOT NULL
        ///, [nodeId] int NOT NULL
        ///, [permission] nchar(1) NOT NULL
        ///);
        ///GO
        ///CREATE TABLE [umbracoUser2Nod [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string SqlCe_SchemaAndData_4110 {
            get {
                return ResourceManager.GetString("SqlCe_SchemaAndData_4110", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to CREATE TABLE [umbracoRelation] 
        ///( 
        ///[id] [int] NOT NULL IDENTITY(1, 1), 
        ///[parentId] [int] NOT NULL, 
        ///[childId] [int] NOT NULL, 
        ///[relType] [int] NOT NULL, 
        ///[datetime] [datetime] NOT NULL CONSTRAINT [DF_umbracoRelation_datetime] DEFAULT (getdate()), 
        ///[comment] [nvarchar] (1000)  NOT NULL 
        ///) 
        /// 
        ///; 
        ///ALTER TABLE [umbracoRelation] ADD CONSTRAINT [PK_umbracoRelation] PRIMARY KEY  ([id]) 
        ///; 
        ///CREATE TABLE [cmsDocument] 
        ///( 
        ///[nodeId] [int] NOT NULL, 
        ///[published] [bit] NOT NULL, 
        ///[documentUser] [int] NOT [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string SqlCeTotal_480 {
            get {
                return ResourceManager.GetString("SqlCeTotal_480", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to /*******************************************************************************************
        ///
        ///
        ///
        ///
        ///
        ///
        ///
        ///    Umbraco database installation script for SQL Server
        /// 
        ///IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT
        /// 
        ///    Database version: 4.8.0.0
        ///    
        ///    Please increment this version number if ANY change is made to this script,
        ///    so compatibility with scripts for other database systems can be verified easily.
        ///    The first 3 digits depict the Umbraco versi [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string SqlServerTotal_480 {
            get {
                return ResourceManager.GetString("SqlServerTotal_480", resourceCulture);
            }
        }
    }
}
