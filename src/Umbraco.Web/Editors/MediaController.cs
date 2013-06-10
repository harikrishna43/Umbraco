﻿using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Editors;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Models.Mapping;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using System.Linq;
using Umbraco.Web.WebApi.Binders;
using Umbraco.Web.WebApi.Filters;

namespace Umbraco.Web.Editors
{

    //internal interface IUmbracoApiService<T>
    //{
    //    T Get(int id);
    //    IEnumerable<T> GetChildren(int id);
    //    HttpResponseMessage Delete(int id);
    //    //copy
    //    //move
    //    //update
    //    //create
    //}

    [PluginController("UmbracoApi")]
    public class MediaController : UmbracoAuthorizedApiController
    {
        private readonly MediaModelMapper _mediaModelMapper;

        /// <summary>
        /// Constructor
        /// </summary>
        public MediaController()
            : this(UmbracoContext.Current, new MediaModelMapper(UmbracoContext.Current.Application, new ProfileModelMapper()))
        {            
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="umbracoContext"></param>
        /// <param name="mediaModelMapper"></param>
        internal MediaController(UmbracoContext umbracoContext, MediaModelMapper mediaModelMapper)
            : base(umbracoContext)
        {
            _mediaModelMapper = mediaModelMapper;
        }

        /// <summary>
        /// Gets the content json for the content id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public MediaItemDisplay GetById(int id)
        {
            var foundContent = Services.MediaService.GetById(id);
            if (foundContent == null)
            {
                ModelState.AddModelError("id", string.Format("media with id: {0} was not found", id));
                var errorResponse = Request.CreateErrorResponse(
                    HttpStatusCode.NotFound,
                    ModelState);
                throw new HttpResponseException(errorResponse);
            }
            return _mediaModelMapper.ToMediaItemDisplay(foundContent);
        }

        /// <summary>
        /// Returns the root media objects
        /// </summary>
        public IEnumerable<ContentItemBasic<ContentPropertyBasic, IMedia>> GetRootMedia()
        {
            return Services.MediaService.GetRootMedia()
                           .Select(x => _mediaModelMapper.ToMediaItemSimple(x));
        }

        /// <summary>
        /// Returns the child media objects
        /// </summary>
        public IEnumerable<ContentItemBasic<ContentPropertyBasic, IMedia>> GetChildren(int parentId)
        {
            return Services.MediaService.GetChildren(parentId)
                           .Select(x => _mediaModelMapper.ToMediaItemSimple(x));
        }

        /// <summary>
        /// Saves content
        /// </summary>
        /// <returns></returns>
        [ContentItemValidationFilter(typeof(ContentItemValidationHelper<IMedia>))]
        [FileUploadCleanupFilter]
        public MediaItemDisplay PostSave(
            [ModelBinder(typeof(ContentItemBinder))]
                ContentItemSave<IMedia> mediaItem)
        {
            //If we've reached here it means:
            // * Our model has been bound
            // * and validated
            // * any file attachments have been saved to their temporary location for us to use
            // * we have a reference to the DTO object and the persisted object

            //Now, we just need to save the data

            //Save the property values
            foreach (var p in mediaItem.ContentDto.Properties)
            {
                //get the dbo property
                var dboProperty = mediaItem.PersistedContent.Properties[p.Alias];

                //create the property data to send to the property editor
                var d = new Dictionary<string, object>();
                //add the files if any
                var files = mediaItem.UploadedFiles.Where(x => x.PropertyId == p.Id).ToArray();
                if (files.Any())
                {
                    d.Add("files", files);
                }
                var data = new ContentPropertyData(p.Value, d);

                //get the deserialized value from the property editor
                dboProperty.Value = p.PropertyEditor.ValueEditor.DeserializeValue(data, dboProperty.Value);
            }

            //save the item
            Services.MediaService.Save(mediaItem.PersistedContent);

            //return the updated model
            return _mediaModelMapper.ToMediaItemDisplay(mediaItem.PersistedContent);
        }
    }
}
