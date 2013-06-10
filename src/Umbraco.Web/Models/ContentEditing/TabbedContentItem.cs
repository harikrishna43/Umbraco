﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Umbraco.Web.Models.ContentEditing
{
    public abstract class TabbedContentItem<T> : ContentItemBasic<T> 
        where T : ContentPropertyBasic
    {
        protected TabbedContentItem()
        {
            Tabs = new List<Tab<T>>();
        }        
        
        /// <summary>
        /// Defines the tabs containing display properties
        /// </summary>
        [DataMember(Name = "tabs")]
        public IEnumerable<Tab<T>> Tabs { get; set; }

        /// <summary>
        /// Override the properties property to ensure we don't serialize this
        /// and to simply return the properties based on the properties in the tabs collection
        /// </summary>
        /// <remarks>
        /// This property cannot be set
        /// </remarks>
        [JsonIgnore]
        public override IEnumerable<T> Properties
        {
            get { return Tabs.SelectMany(x => x.Properties); }
            set { throw new NotImplementedException(); }
        }
    }
}