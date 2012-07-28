﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Logging;
using umbraco.BusinessLogic;
using umbraco.BusinessLogic.Utils;
using umbraco.interfaces;

namespace umbraco.businesslogic
{
	/// <summary>
	/// ApplicationStartupHandler provides an easy to use base class to install event handlers in umbraco.
	/// Class inhiriting from ApplicationStartupHandler are automaticly registered and instantiated by umbraco on application start.
	/// To use, inhirite the ApplicationStartupHandler Class and add an empty constructor. 
	/// </summary>
	public abstract class ApplicationStartupHandler : IApplicationStartupHandler
	{

		public static void RegisterHandlers()
		{
			if (ApplicationContext.Current == null || !ApplicationContext.Current.IsConfigured)
				return;

			//now we just create the types... this is kind of silly since these objects don't actually do anything
			//except run their constructors.

			var instances = PluginTypeResolver.Current.CreateInstances<IApplicationStartupHandler>(
				PluginTypeResolver.Current.ResolveApplicationStartupHandlers());

		}
	}

}
