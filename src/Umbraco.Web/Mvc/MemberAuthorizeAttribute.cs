﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Umbraco.Web.Security;
using umbraco.cms.businesslogic.member;
using AuthorizeAttribute = System.Web.Mvc.AuthorizeAttribute;
using Umbraco.Core;

namespace Umbraco.Web.Mvc
{
    /// <summary>
    /// Attribute for attributing controller actions to restrict them
    /// to just authenticated members, and optionally of a particular type and/or group
    /// </summary>
    public sealed class MemberAuthorizeAttribute : AuthorizeAttribute
    {

        private readonly ApplicationContext _applicationContext;
        private readonly UmbracoContext _umbracoContext;

        public MemberAuthorizeAttribute(UmbracoContext umbracoContext)
        {
            if (umbracoContext == null) throw new ArgumentNullException("umbracoContext");
            _umbracoContext = umbracoContext;
            _applicationContext = _umbracoContext.Application;
        }

        public MemberAuthorizeAttribute()
            : this(UmbracoContext.Current)
        {

        }

        /// <summary>
        /// Flag for whether to allow all site visitors or just authenticated members
        /// </summary>
        /// <remarks>
        /// This is the same as applying the [AllowAnonymous] attribute
        /// </remarks>
        public bool AllowAll { get; set; }

        /// <summary>
        /// Comma delimited list of allowed member types
        /// </summary>
        public string AllowType { get; set; }

        /// <summary>
        /// Comma delimited list of allowed member groups
        /// </summary>
        public string AllowGroup { get; set; }

        /// <summary>
        /// Comma delimited list of allowed members
        /// </summary>
        public string AllowMembers { get; set; }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (AllowMembers.IsNullOrWhiteSpace())
                AllowMembers = "";
            if (AllowGroup.IsNullOrWhiteSpace())
                AllowGroup = "";
            if (AllowType.IsNullOrWhiteSpace())
                AllowType = "";

            var members = new List<int>();
            foreach (var s in AllowMembers.Split(','))
            {
                int id;
                if (int.TryParse(s, out id))
                {
                    members.Add(id);
                }
            }

            return _umbracoContext.Security.IsMemberAuthorized(AllowAll,
                                                  AllowType.Split(','),
                                                  AllowGroup.Split(','),
                                                  members);
        }

        /// <summary>
        /// Override method to throw exception instead of returning a 401 result
        /// </summary>
        /// <param name="filterContext"></param>
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            throw new HttpException(403, "Resource restricted: either member is not logged on or is not of a permitted type or group.");
        }

    }
}
