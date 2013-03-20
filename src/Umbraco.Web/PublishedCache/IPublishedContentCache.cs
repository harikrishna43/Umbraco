﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Umbraco.Core.Models;

namespace Umbraco.Web.PublishedCache
{
    internal interface IPublishedContentCache : IPublishedCache
    {
        /// <summary>
        /// Gets content identified by a route.
        /// </summary>
        /// <param name="umbracoContext">The context.</param>
        /// <param name="route">The route</param>
        /// <param name="hideTopLevelNode">A value forcing the HideTopLevelNode setting.</param>
        /// <returns>The content, or null.</returns>
        /// <remarks>
        /// <para>A valid route is either a simple path eg <c>/foo/bar/nil</c> or a root node id and a path, eg <c>123/foo/bar/nil</c>.</para>
        /// <para>If <param name="hideTopLevelNode" /> is <c>null</c> then the settings value is used.</para>
        /// </remarks>
        IPublishedContent GetByRoute(UmbracoContext umbracoContext, string route, bool? hideTopLevelNode = null);

        /// <summary>
        /// Gets the route for a content identified by its unique identifier.
        /// </summary>
        /// <param name="umbracoContext">The context.</param>
        /// <param name="contentId">The content unique identifier.</param>
        /// <returns>The route.</returns>
        string GetRouteById(UmbracoContext umbracoContext, int contentId);

        // FIXME do we want that one?
        IPublishedContent GetByUrlAlias(UmbracoContext umbracoContext, int rootNodeId, string alias);
    }
}
