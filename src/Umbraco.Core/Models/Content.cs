﻿using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a Content object
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class Content : ContentBase, IContent
    {
        private IContentType _contentType;
        private ITemplate _template;
        private bool _published;
        private string _language;
        private DateTime? _releaseDate;
        private DateTime? _expireDate;
        private int _writer;
        private string _nodeName;//NOTE Once localization is introduced this will be the non-localized Node Name.

        /// <summary>
        /// Constructor for creating a Content object
        /// </summary>
        /// <param name="name">Name of the content</param>
        /// <param name="parent">Parent <see cref="IContent"/> object</param>
        /// <param name="contentType">ContentType for the current Content object</param>
        public Content(string name, IContent parent, IContentType contentType)
			: this(name, parent, contentType, new PropertyCollection())
		{			
		}

        /// <summary>
        /// Constructor for creating a Content object
        /// </summary>
        /// <param name="name">Name of the content</param>
        /// <param name="parent">Parent <see cref="IContent"/> object</param>
        /// <param name="contentType">ContentType for the current Content object</param>
        /// <param name="properties">Collection of properties</param>
		public Content(string name, IContent parent, IContentType contentType, PropertyCollection properties)
			: base(name, parent, contentType, properties)
		{
			Mandate.ParameterNotNull(contentType, "contentType");

			_contentType = contentType;
		}

        /// <summary>
        /// Constructor for creating a Content object
        /// </summary>
        /// <param name="name">Name of the content</param>
        /// <param name="parentId">Id of the Parent content</param>
        /// <param name="contentType">ContentType for the current Content object</param>
        public Content(string name, int parentId, IContentType contentType)
            : this(name, parentId, contentType, new PropertyCollection())
        {
        }

        /// <summary>
        /// Constructor for creating a Content object
        /// </summary>
        /// <param name="name">Name of the content</param>
        /// <param name="parentId">Id of the Parent content</param>
        /// <param name="contentType">ContentType for the current Content object</param>
        /// <param name="properties">Collection of properties</param>
        public Content(string name, int parentId, IContentType contentType, PropertyCollection properties) 
			: base(name, parentId, contentType, properties)
        {
            Mandate.ParameterNotNull(contentType, "contentType");

            _contentType = contentType;
        }

        private static readonly PropertyInfo TemplateSelector = ExpressionHelper.GetPropertyInfo<Content, ITemplate>(x => x.Template);
        private static readonly PropertyInfo PublishedSelector = ExpressionHelper.GetPropertyInfo<Content, bool>(x => x.Published);
        private static readonly PropertyInfo LanguageSelector = ExpressionHelper.GetPropertyInfo<Content, string>(x => x.Language);
        private static readonly PropertyInfo ReleaseDateSelector = ExpressionHelper.GetPropertyInfo<Content, DateTime?>(x => x.ReleaseDate);
        private static readonly PropertyInfo ExpireDateSelector = ExpressionHelper.GetPropertyInfo<Content, DateTime?>(x => x.ExpireDate);
        private static readonly PropertyInfo WriterSelector = ExpressionHelper.GetPropertyInfo<Content, int>(x => x.WriterId);
        private static readonly PropertyInfo NodeNameSelector = ExpressionHelper.GetPropertyInfo<Content, string>(x => x.NodeName);

        /// <summary>
        /// Gets or sets the template used by the Content.
        /// This is used to override the default one from the ContentType.
        /// </summary>
        /// <remarks>
        /// If no template is explicitly set on the Content object, 
        /// the Default template from the ContentType will be returned.
        /// </remarks>
        [DataMember]
        public virtual ITemplate Template
        {
            get
            {
                if (_template == null)
                    return _contentType.DefaultTemplate;

                return _template;
            }
            set
            {
                _template = value;
                OnPropertyChanged(TemplateSelector);
            }
        }

        /// <summary>
        /// Gets the current status of the Content
        /// </summary>
        [IgnoreDataMember]
        public ContentStatus Status
        {
            get
            {
                if(Trashed)
                    return ContentStatus.Trashed;

                if(ExpireDate.HasValue && ExpireDate.Value > DateTime.MinValue && DateTime.Now > ExpireDate.Value)
                    return ContentStatus.Expired;

                if(ReleaseDate.HasValue && ReleaseDate.Value > DateTime.MinValue && ReleaseDate.Value > DateTime.Now)
                    return ContentStatus.AwaitingRelease;

                if(Published)
                    return ContentStatus.Published;

                return ContentStatus.Unpublished;
            }
        }

        /// <summary>
        /// Boolean indicating whether this Content is Published or not
        /// </summary>
        /// <remarks>Setting Published to true/false should be private or internal</remarks>
        [DataMember]
        public bool Published
        {
            get { return _published; }
            internal set
            {
                _published = value;
                OnPropertyChanged(PublishedSelector);
            }
        }

        /// <summary>
        /// Language of the data contained within this Content object.
        /// </summary>
        /// <remarks>
        /// Left internal until multilingual support is implemented.
        /// </remarks>
        [DataMember]
        public string Language
        {
            get { return _language; }
            set
            {
                _language = value;
                OnPropertyChanged(LanguageSelector);
            }
        }

        /// <summary>
        /// The date this Content should be released and thus be published
        /// </summary>
        [DataMember]
        public DateTime? ReleaseDate
        {
            get { return _releaseDate; }
            set
            {
                _releaseDate = value;
                OnPropertyChanged(ReleaseDateSelector);
            }
        }

        /// <summary>
        /// The date this Content should expire and thus be unpublished
        /// </summary>
        [DataMember]
        public DateTime? ExpireDate
        {
            get { return _expireDate; }
            set
            {
                _expireDate = value;
                OnPropertyChanged(ExpireDateSelector);
            }
        }

        /// <summary>
        /// Id of the user who wrote/updated this Content
        /// </summary>
        [DataMember]
        public virtual int WriterId
        {
            get { return _writer; }
            set
            {
                _writer = value;
                OnPropertyChanged(WriterSelector);
            }
        }

        /// <summary>
        /// Name of the Node (non-localized).
        /// </summary>
        /// <remarks>
        /// This Property is kept internal until localization is introduced.
        /// </remarks>
        internal string NodeName
        {
            get { return _nodeName; }
            set
            {
                _nodeName = value;
                OnPropertyChanged(NodeNameSelector);
            }
        }

        /// <summary>
        /// Gets the ContentType used by this content object
        /// </summary>
        [IgnoreDataMember]
        public IContentType ContentType
        {
            get { return _contentType; }
        }

        /// <summary>
        /// Changes the <see cref="ContentType"/> for the current content object
        /// </summary>
        /// <param name="contentType">New ContentType for this content</param>
        /// <remarks>Leaves PropertyTypes intact after change</remarks>
        public void ChangeContentType(IContentType contentType)
        {
            ContentTypeId = contentType.Id;
            _contentType = contentType;
            ContentTypeBase = contentType;
            Properties.EnsurePropertyTypes(PropertyTypes);
            Properties.CollectionChanged += PropertiesChanged;
        }

        /// <summary>
        /// Changes the <see cref="ContentType"/> for the current content object and removes PropertyTypes,
        /// which are not part of the new ContentType.
        /// </summary>
        /// <param name="contentType">New ContentType for this content</param>
        /// <param name="clearProperties">Boolean indicating whether to clear PropertyTypes upon change</param>
        public void ChangeContentType(IContentType contentType, bool clearProperties)
        {
            if(clearProperties)
            {
                ContentTypeId = contentType.Id;
                _contentType = contentType;
                ContentTypeBase = contentType;
                Properties.EnsureCleanPropertyTypes(PropertyTypes);
                Properties.CollectionChanged += PropertiesChanged;
                return;
            }

            ChangeContentType(contentType);
        }

        /// <summary>
        /// Changes the Published state of the content object
        /// </summary>
        public void ChangePublishedState(PublishedState state)
        {
            Published = state == PublishedState.Published;
            PublishedState = state;
        }

        internal PublishedState PublishedState { get; set; }

        /// <summary>
        /// Changes the Trashed state of the content object
        /// </summary>
        /// <param name="isTrashed">Boolean indicating whether content is trashed (true) or not trashed (false)</param>
        /// <param name="parentId"> </param>
        public override void ChangeTrashedState(bool isTrashed, int parentId = -1)
        {
            Trashed = isTrashed;

            //If Content is trashed the parent id should be set to that of the RecycleBin
            if(isTrashed)
            {
                ParentId = -20;
            }
            else//otherwise set the parent id to the optional parameter, -1 being the fallback
            {
                ParentId = parentId;
            }

            //If the content is trashed and is published it should be marked as unpublished
            if (isTrashed && Published)
            {
                ChangePublishedState(PublishedState.Unpublished);
            }
        }

        /// <summary>
        /// Creates a clone of the current entity
        /// </summary>
        /// <returns></returns>
        public IContent Clone()
        {
            var clone = (Content)this.MemberwiseClone();
            clone.Key = Guid.Empty;
            clone.Version = Guid.NewGuid();
            clone.ResetIdentity();

            foreach (var property in clone.Properties)
            {
                property.ResetIdentity();
                property.Version = clone.Version;
            }

            return clone;
        }

        /// <summary>
        /// Indicates whether a specific property on the current <see cref="IContent"/> entity is dirty.
        /// </summary>
        /// <param name="propertyName">Name of the property to check</param>
        /// <returns>True if Property is dirty, otherwise False</returns>
        public override bool IsPropertyDirty(string propertyName)
        {
            bool existsInEntity = base.IsPropertyDirty(propertyName);
            if (existsInEntity)
                return true;

            return Properties.Any(x => x.IsPropertyDirty(propertyName));
        }

        /// <summary>
        /// Indicates whether the current entity is dirty.
        /// </summary>
        /// <returns>True if entity is dirty, otherwise False</returns>
        public override bool IsDirty()
        {
            bool dirtyEntity = base.IsDirty();

            bool dirtyProperties = Properties.Any(x => x.IsDirty());

            return dirtyEntity || dirtyProperties;
        }

        /// <summary>
        /// Resets dirty properties by clearing the dictionary used to track changes.
        /// </summary>
        /// <remarks>
        /// Please note that resetting the dirty properties could potentially
        /// obstruct the saving of a new or updated entity.
        /// </remarks>
        public override void ResetDirtyProperties()
        {
            base.ResetDirtyProperties();

            foreach (var property in Properties)
            {
                property.ResetDirtyProperties();
            }
        }

        /// <summary>
        /// Method to call when Entity is being saved
        /// </summary>
        /// <remarks>Created date is set and a Unique key is assigned</remarks>
        internal override void AddingEntity()
        {
            base.AddingEntity();

            if(Key == Guid.Empty)
                Key = Guid.NewGuid();
        }

        /// <summary>
        /// Method to call when Entity is being updated
        /// </summary>
        /// <remarks>Modified Date is set and a new Version guid is set</remarks>
        internal override void UpdatingEntity()
        {
            base.UpdatingEntity();
            Version = Guid.NewGuid();
        }
    }
}