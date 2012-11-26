﻿using System;
using System.Configuration;
using System.Data.Common;
using Umbraco.Core.Configuration;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core
{
    /// <summary>
    /// The Umbraco Database context
    /// </summary>
    /// <remarks>
    /// One per AppDomain, represents the Umbraco database
    /// </remarks>
    public class DatabaseContext
    {
        private bool _configured;

        #region Singleton
        private static readonly Lazy<DatabaseContext> lazy = new Lazy<DatabaseContext>(() => new DatabaseContext());

        /// <summary>
        /// Gets the current Database Context.
        /// </summary>
        public static DatabaseContext Current { get { return lazy.Value; } }

        private DatabaseContext()
        {
        }
        #endregion

        /// <summary>
        /// Gets the <see cref="Database"/> object for doing CRUD operations
        /// against custom tables that resides in the Umbraco database.
        /// </summary>
        /// <remarks>
        /// This should not be used for CRUD operations or queries against the
        /// standard Umbraco tables!
        /// </remarks>
        public Database Database
        {
            get { return DatabaseFactory.Current.Database; }
        }

        /// <summary>
        /// Boolean indicating whether the database has been configured
        /// </summary>
        public bool IsDatabaseConfigured
        {
            get { return _configured; }
        }

        /// <summary>
        /// Internal method to initialize the database configuration.
        /// </summary>
        /// <remarks>
        /// If an Umbraco connectionstring exists the database can be configured on app startup,
        /// but if its a new install the entry doesn't exist and the db cannot be configured.
        /// So for new installs the Initialize() method should be called after the connectionstring
        /// has been added to the web.config.
        /// </remarks>
        internal void Initialize()
        {
            if (ConfigurationManager.ConnectionStrings[GlobalSettings.UmbracoConnectionName] != null)
            {
                var providerName = "System.Data.SqlClient";
                if (!string.IsNullOrEmpty(ConfigurationManager.ConnectionStrings[GlobalSettings.UmbracoConnectionName].ProviderName))
                        providerName = ConfigurationManager.ConnectionStrings[GlobalSettings.UmbracoConnectionName].ProviderName;

                if (providerName.StartsWith("MySql"))
                {
                    SyntaxConfig.SqlSyntaxProvider = MySqlSyntax.Provider;
                }
                else if (providerName.Contains("SqlServerCe"))
                {
                    SyntaxConfig.SqlSyntaxProvider = SqlCeSyntax.Provider;
                }
                else
                {
                    SyntaxConfig.SqlSyntaxProvider = SqlServerSyntax.Provider;
                }

                _configured = true;
            }
            else
            {
                _configured = false;
            }
        }
    }
}