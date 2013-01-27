﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSix;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Publishing;
using Umbraco.Core.Services;
using MigrationsVersionFourNineZero = Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionFourNineZero;

namespace Umbraco.Core
{
	/// <summary>
	/// A bootstrapper for the Umbraco application which initializes all objects for the Core of the application 
	/// </summary>
	/// <remarks>
	/// This does not provide any startup functionality relating to web objects
	/// </remarks>
	public class CoreBootManager : IBootManager
	{

		private DisposableTimer _timer;
		private bool _isInitialized = false;
		private bool _isStarted = false;
		private bool _isComplete = false;

		protected ApplicationContext ApplicationContext { get; private set; }

		public virtual IBootManager Initialize()
		{
			if (_isInitialized)
				throw new InvalidOperationException("The boot manager has already been initialized");

			LogHelper.Info<CoreBootManager>("Umbraco application starting");
			_timer = DisposableTimer.Start(x => LogHelper.Info<CoreBootManager>("Umbraco application startup complete" + " (took " + x + "ms)"));

			//create database and service contexts for the app context
			var dbFactory = new DefaultDatabaseFactory(GlobalSettings.UmbracoConnectionName);
		    Database.Mapper = new PetaPocoMapper();
			var dbContext = new DatabaseContext(dbFactory);
			var serviceContext = new ServiceContext(
				new PetaPocoUnitOfWorkProvider(dbFactory), 
				new FileUnitOfWorkProvider(), 
				new PublishingStrategy());
			
			//create the ApplicationContext
			ApplicationContext = ApplicationContext.Current = new ApplicationContext(dbContext, serviceContext);

            //initialize the DatabaseContext
			dbContext.Initialize();

			InitializeResolvers();

			_isInitialized = true;

			return this;
		}

		/// <summary>
		/// Fires after initialization and calls the callback to allow for customizations to occur
		/// </summary>
		/// <param name="afterStartup"></param>
		/// <returns></returns>
		public virtual IBootManager Startup(Action<ApplicationContext> afterStartup)
		{
			if (_isStarted)
				throw new InvalidOperationException("The boot manager has already been initialized");

			if (afterStartup != null)
			{
				afterStartup(ApplicationContext.Current);	
			}

			_isStarted = true;

			return this;
		}

		/// <summary>
		/// Fires after startup and calls the callback once customizations are locked
		/// </summary>
		/// <param name="afterComplete"></param>
		/// <returns></returns>
		public virtual IBootManager Complete(Action<ApplicationContext> afterComplete)
		{
			if (_isComplete)
				throw new InvalidOperationException("The boot manager has already been completed");

			//freeze resolution to not allow Resolvers to be modified
			Resolution.Freeze();

			//stop the timer and log the output
			_timer.Dispose();

			if (afterComplete != null)
			{
				afterComplete(ApplicationContext.Current);	
			}

			_isComplete = true;

			return this;
		}

		/// <summary>
		/// Create the resolvers
		/// </summary>
		protected virtual void InitializeResolvers()
		{
			RepositoryResolver.Current = new RepositoryResolver(
				new RepositoryFactory());

			CacheRefreshersResolver.Current = new CacheRefreshersResolver(
				() => PluginManager.Current.ResolveCacheRefreshers());

			DataTypesResolver.Current = new DataTypesResolver(
				() => PluginManager.Current.ResolveDataTypes());

			MacroFieldEditorsResolver.Current = new MacroFieldEditorsResolver(
				() => PluginManager.Current.ResolveMacroRenderings());

			PackageActionsResolver.Current = new PackageActionsResolver(
				() => PluginManager.Current.ResolvePackageActions());

			ActionsResolver.Current = new ActionsResolver(
				() => PluginManager.Current.ResolveActions());

            MacroPropertyTypeResolver.Current = new MacroPropertyTypeResolver(
                PluginManager.Current.ResolveMacroPropertyTypes());

            //TODO: Y U NO WORK?
            //MigrationResolver.Current = new MigrationResolver(
            //    PluginManager.Current.ResolveMigrationTypes());
            
			//the database migration objects
			MigrationResolver.Current = new MigrationResolver(new List<Type>
				{
					typeof (MigrationsVersionFourNineZero.RemoveUmbracoAppConstraints),
					typeof (DeleteAppTables),
					typeof (EnsureAppsTreesUpdated),
					typeof (MoveMasterContentTypeData),
					typeof (NewCmsContentType2ContentTypeTable),
					typeof (RemoveMasterContentTypeColumn),
					typeof (RenameCmsTabTable),
					typeof (RenameTabIdColumn),
					typeof (UpdateCmsContentTypeAllowedContentTypeTable),
					typeof (UpdateCmsContentTypeTable),
					typeof (UpdateCmsContentVersionTable),
					typeof (UpdateCmsPropertyTypeGroupTable)
				});

			PropertyEditorValueConvertersResolver.Current = new PropertyEditorValueConvertersResolver(
				PluginManager.Current.ResolvePropertyEditorValueConverters());
			//add the internal ones, these are not public currently so need to add them manually
			PropertyEditorValueConvertersResolver.Current.AddType<DatePickerPropertyEditorValueConverter>();
			PropertyEditorValueConvertersResolver.Current.AddType<TinyMcePropertyEditorValueConverter>();
			PropertyEditorValueConvertersResolver.Current.AddType<YesNoPropertyEditorValueConverter>();
        }
	}
}
