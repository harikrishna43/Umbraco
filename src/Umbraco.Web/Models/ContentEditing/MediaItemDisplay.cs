﻿using System;
using System.Runtime.Serialization;
using Umbraco.Core.Models;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// A model representing a content item to be displayed in the back office
    /// </summary>    
    public class MediaItemDisplay : TabbedContentItem<ContentPropertyDisplay, IMedia>
    {
        
    }
}