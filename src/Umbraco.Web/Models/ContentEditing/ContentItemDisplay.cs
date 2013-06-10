﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// A model representing a content item to be displayed in the back office
    /// </summary>    
    public class ContentItemDisplay : TabbedContentItem<ContentPropertyDisplay>
    {
        
        [DataMember(Name = "publishDate")]
        public DateTime? PublishDate { get; set; }

    }
}