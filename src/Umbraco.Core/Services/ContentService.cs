using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using Umbraco.Core.Auditing;
using Umbraco.Core.Events;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Publishing;

namespace Umbraco.Core.Services
{
	/// <summary>
	/// Represents the Content Service, which is an easy access to operations involving <see cref="IContent"/>
	/// </summary>
	public class ContentService : IContentService
	{
		private readonly IDatabaseUnitOfWorkProvider _uowProvider;
		private readonly IPublishingStrategy _publishingStrategy;
        private readonly RepositoryFactory _repositoryFactory;

        public ContentService()
            : this(new RepositoryFactory())
        {}

        public ContentService(RepositoryFactory repositoryFactory)
            : this(new PetaPocoUnitOfWorkProvider(), repositoryFactory, new PublishingStrategy())
        {}

        public ContentService(IDatabaseUnitOfWorkProvider provider)
            : this(provider, new RepositoryFactory(), new PublishingStrategy())
        { }

        public ContentService(IDatabaseUnitOfWorkProvider provider, RepositoryFactory repositoryFactory)
            : this(provider, repositoryFactory, new PublishingStrategy())
        { }

	    public ContentService(IDatabaseUnitOfWorkProvider provider, RepositoryFactory repositoryFactory, IPublishingStrategy publishingStrategy)
		{
			_uowProvider = provider;
			_publishingStrategy = publishingStrategy;
            _repositoryFactory = repositoryFactory;
		}

	    /// <summary>
	    /// Creates an <see cref="IContent"/> object using the alias of the <see cref="IContentType"/>
	    /// that this Content is based on.
	    /// </summary>
        /// <param name="name">Name of the Content object</param>
	    /// <param name="parentId">Id of Parent for the new Content</param>
	    /// <param name="contentTypeAlias">Alias of the <see cref="IContentType"/></param>
	    /// <param name="userId">Optional id of the user creating the content</param>
	    /// <returns><see cref="IContent"/></returns>
	    public IContent CreateContent(string name, int parentId, string contentTypeAlias, int userId = 0)
		{
		    IContentType contentType = null;
            IContent content = null;

			var uow = _uowProvider.GetUnitOfWork();
		    using (var repository = _repositoryFactory.CreateContentTypeRepository(uow))
		    {
		        var query = Query<IContentType>.Builder.Where(x => x.Alias == contentTypeAlias);
		        var contentTypes = repository.GetByQuery(query);

		        if (!contentTypes.Any())
		            throw new Exception(string.Format("No ContentType matching the passed in Alias: '{0}' was found",
		                                              contentTypeAlias));

		        contentType = contentTypes.First();

		        if (contentType == null)
		            throw new Exception(string.Format("ContentType matching the passed in Alias: '{0}' was null",
		                                              contentTypeAlias));
		    }

            content = new Content(name, parentId, contentType);

			if (Creating.IsRaisedEventCancelled(new NewEventArgs<IContent>(content, contentTypeAlias, parentId), this))
				return content;

	        content.CreatorId = userId;
			content.WriterId = userId;

			Created.RaiseEvent(new NewEventArgs<IContent>(content, false, contentTypeAlias, parentId), this);

			Audit.Add(AuditTypes.New, "", content.CreatorId, content.Id);

		    return content;	
		}

        /// <summary>
        /// Creates an <see cref="IContent"/> object using the alias of the <see cref="IContentType"/>
        /// that this Content is based on.
        /// </summary>
        /// <param name="name">Name of the Content object</param>
        /// <param name="parent">Parent <see cref="IContent"/> object for the new Content</param>
        /// <param name="contentTypeAlias">Alias of the <see cref="IContentType"/></param>
        /// <param name="userId">Optional id of the user creating the content</param>
        /// <returns><see cref="IContent"/></returns>
        public IContent CreateContent(string name, IContent parent, string contentTypeAlias, int userId = 0)
        {
            IContentType contentType = null;
            IContent content = null;

            var uow = _uowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreateContentTypeRepository(uow))
            {
                var query = Query<IContentType>.Builder.Where(x => x.Alias == contentTypeAlias);
                var contentTypes = repository.GetByQuery(query);

                if (!contentTypes.Any())
                    throw new Exception(string.Format("No ContentType matching the passed in Alias: '{0}' was found",
                                                      contentTypeAlias));

                contentType = contentTypes.First();

                if (contentType == null)
                    throw new Exception(string.Format("ContentType matching the passed in Alias: '{0}' was null",
                                                      contentTypeAlias));
            }

            content = new Content(name, parent, contentType);

            if (Creating.IsRaisedEventCancelled(new NewEventArgs<IContent>(content, contentTypeAlias, parent), this))
                return content;

            content.CreatorId = userId;
            content.WriterId = userId;

            Created.RaiseEvent(new NewEventArgs<IContent>(content, false, contentTypeAlias, parent), this);

            Audit.Add(AuditTypes.New, "", content.CreatorId, content.Id);

            return content;
        }

		/// <summary>
		/// Gets an <see cref="IContent"/> object by Id
		/// </summary>
		/// <param name="id">Id of the Content to retrieve</param>
		/// <returns><see cref="IContent"/></returns>
		public IContent GetById(int id)
		{
			using (var repository = _repositoryFactory.CreateContentRepository(_uowProvider.GetUnitOfWork()))
			{
				return repository.Get(id);
			}
		}

		/// <summary>
		/// Gets an <see cref="IContent"/> object by its 'UniqueId'
		/// </summary>
		/// <param name="key">Guid key of the Content to retrieve</param>
		/// <returns><see cref="IContent"/></returns>
		public IContent GetById(Guid key)
		{
			using (var repository = _repositoryFactory.CreateContentRepository(_uowProvider.GetUnitOfWork()))
			{
				var query = Query<IContent>.Builder.Where(x => x.Key == key);
				var contents = repository.GetByQuery(query);
				return contents.SingleOrDefault();
			}
		}

		/// <summary>
		/// Gets a collection of <see cref="IContent"/> objects by the Id of the <see cref="IContentType"/>
		/// </summary>
		/// <param name="id">Id of the <see cref="IContentType"/></param>
		/// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
		public IEnumerable<IContent> GetContentOfContentType(int id)
		{
			using (var repository = _repositoryFactory.CreateContentRepository(_uowProvider.GetUnitOfWork()))
			{
				var query = Query<IContent>.Builder.Where(x => x.ContentTypeId == id);
				var contents = repository.GetByQuery(query);

				return contents;
			}
		}

		/// <summary>
		/// Gets a collection of <see cref="IContent"/> objects by Level
		/// </summary>
		/// <param name="level">The level to retrieve Content from</param>
		/// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
		public IEnumerable<IContent> GetByLevel(int level)
		{
			using (var repository = _repositoryFactory.CreateContentRepository(_uowProvider.GetUnitOfWork()))
			{
				var query = Query<IContent>.Builder.Where(x => x.Level == level);
				var contents = repository.GetByQuery(query);

				return contents;
			}
		}

        /// <summary>
        /// Gets a specific version of an <see cref="IContent"/> item.
        /// </summary>
        /// <param name="versionId">Id of the version to retrieve</param>
        /// <returns>An <see cref="IContent"/> item</returns>
        public IContent GetByVersion(Guid versionId)
        {
            using (var repository = _repositoryFactory.CreateContentRepository(_uowProvider.GetUnitOfWork()))
            {
                return repository.GetByVersion(versionId);
            }
        }

        /// <summary>
        /// Gets a collection of an <see cref="IContent"/> objects versions by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        public IEnumerable<IContent> GetVersions(int id)
        {
            using (var repository = _repositoryFactory.CreateContentRepository(_uowProvider.GetUnitOfWork()))
            {
                var versions = repository.GetAllVersions(id);
                return versions;
            }
        }

		/// <summary>
		/// Gets a collection of <see cref="IContent"/> objects by Parent Id
		/// </summary>
		/// <param name="id">Id of the Parent to retrieve Children from</param>
		/// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
		public IEnumerable<IContent> GetChildren(int id)
		{
			using (var repository = _repositoryFactory.CreateContentRepository(_uowProvider.GetUnitOfWork()))
			{
				var query = Query<IContent>.Builder.Where(x => x.ParentId == id);
				var contents = repository.GetByQuery(query);

				return contents;
			}
		}

        /// <summary>
        /// Gets a collection of <see cref="IContent"/> objects by its name or partial name
        /// </summary>
        /// <param name="parentId">Id of the Parent to retrieve Children from</param>
        /// <param name="name">Full or partial name of the children</param>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        public IEnumerable<IContent> GetChildrenByName(int parentId, string name)
        {
            using (var repository = _repositoryFactory.CreateContentRepository(_uowProvider.GetUnitOfWork()))
            {
                var query = Query<IContent>.Builder.Where(x => x.ParentId == parentId && x.Name.Contains(name));
                var contents = repository.GetByQuery(query);

                return contents;
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="IContent"/> objects by Parent Id
        /// </summary>
        /// <param name="id">Id of the Parent to retrieve Descendants from</param>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        public IEnumerable<IContent> GetDescendants(int id)
        {
            var content = GetById(id);
            return GetDescendants(content);
        }

        /// <summary>
        /// Gets a collection of <see cref="IContent"/> objects by Parent Id
        /// </summary>
        /// <param name="content"><see cref="IContent"/> item to retrieve Descendants from</param>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        public IEnumerable<IContent> GetDescendants(IContent content)
        {
            using (var repository = _repositoryFactory.CreateContentRepository(_uowProvider.GetUnitOfWork()))
            {
                var query = Query<IContent>.Builder.Where(x => x.Path.StartsWith(content.Path) && x.Id != content.Id);
                var contents = repository.GetByQuery(query);

                return contents;
            }
        }

        /// <summary>
        /// Gets the published version of an <see cref="IContent"/> item
        /// </summary>
        /// <param name="id">Id of the <see cref="IContent"/> to retrieve version from</param>
        /// <returns>An <see cref="IContent"/> item</returns>
        public IContent GetPublishedVersion(int id)
        {
            var version = GetVersions(id);
            return version.FirstOrDefault(x => x.Published == true);
        }

		/// <summary>
		/// Gets a collection of <see cref="IContent"/> objects, which reside at the first level / root
		/// </summary>
		/// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
		public IEnumerable<IContent> GetRootContent()
		{
			using (var repository = _repositoryFactory.CreateContentRepository(_uowProvider.GetUnitOfWork()))
			{
				var query = Query<IContent>.Builder.Where(x => x.ParentId == -1);
				var contents = repository.GetByQuery(query);

				return contents;
			}
		}

		/// <summary>
		/// Gets a collection of <see cref="IContent"/> objects, which has an expiration date less than or equal to today.
		/// </summary>
		/// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
		public IEnumerable<IContent> GetContentForExpiration()
		{
			using (var repository = _repositoryFactory.CreateContentRepository(_uowProvider.GetUnitOfWork()))
			{
				var query = Query<IContent>.Builder.Where(x => x.Published == true && x.ExpireDate <= DateTime.Now);
				var contents = repository.GetByQuery(query);

				return contents;
			}
		}

		/// <summary>
		/// Gets a collection of <see cref="IContent"/> objects, which has a release date less than or equal to today.
		/// </summary>
		/// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
		public IEnumerable<IContent> GetContentForRelease()
		{
			using (var repository = _repositoryFactory.CreateContentRepository(_uowProvider.GetUnitOfWork()))
			{
				var query = Query<IContent>.Builder.Where(x => x.Published == false && x.ReleaseDate <= DateTime.Now);
				var contents = repository.GetByQuery(query);

				return contents;
			}
		}

		/// <summary>
		/// Gets a collection of an <see cref="IContent"/> objects, which resides in the Recycle Bin
		/// </summary>
		/// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
		public IEnumerable<IContent> GetContentInRecycleBin()
		{
			using (var repository = _repositoryFactory.CreateContentRepository(_uowProvider.GetUnitOfWork()))
			{
				var query = Query<IContent>.Builder.Where(x => x.Path.Contains("-20"));
				var contents = repository.GetByQuery(query);

				return contents;
			}
		}

        /// <summary>
        /// Checks whether an <see cref="IContent"/> item has any children
        /// </summary>
        /// <param name="id">Id of the <see cref="IContent"/></param>
        /// <returns>True if the content has any children otherwise False</returns>
        public bool HasChildren(int id)
        {
            using (var repository = _repositoryFactory.CreateContentRepository(_uowProvider.GetUnitOfWork()))
            {
                var query = Query<IContent>.Builder.Where(x => x.ParentId == id);
                int count = repository.Count(query);
                return count > 0;
            }
        }

        /// <summary>
        /// Checks whether an <see cref="IContent"/> item has any published versions
        /// </summary>
        /// <param name="id">Id of the <see cref="IContent"/></param>
        /// <returns>True if the content has any published version otherwise False</returns>
        public bool HasPublishedVersion(int id)
        {
            using (var repository = _repositoryFactory.CreateContentRepository(_uowProvider.GetUnitOfWork()))
            {
                var query = Query<IContent>.Builder.Where(x => x.Published == true && x.Id == id && x.Trashed == false);
                int count = repository.Count(query);
                return count > 0;
            }
        }

        /// <summary>
        /// Checks if the passed in <see cref="IContent"/> can be published based on the anscestors publish state.
        /// </summary>
        /// <param name="content"><see cref="IContent"/> to check if anscestors are published</param>
        /// <returns>True if the Content can be published, otherwise False</returns>
        public bool IsPublishable(IContent content)
        {
            //If the passed in content has yet to be saved we "fallback" to checking the Parent
            //because if the Parent is publishable then the current content can be Saved and Published
            if (content.HasIdentity == false)
            {
                IContent parent = GetById(content.ParentId);
                return IsPublishable(parent, true);
            }

            return IsPublishable(content, false);
        }

	    /// <summary>
	    /// Re-Publishes all Content
	    /// </summary>
	    /// <param name="userId">Optional Id of the User issueing the publishing</param>
	    /// <returns>True if publishing succeeded, otherwise False</returns>
	    public bool RePublishAll(int userId = 0)
	    {
	        return RePublishAllDo(false, userId);
	    }

	    /// <summary>
        /// Publishes a single <see cref="IContent"/> object
        /// </summary>
        /// <param name="content">The <see cref="IContent"/> to publish</param>
        /// <param name="userId">Optional Id of the User issueing the publishing</param>
        /// <returns>True if publishing succeeded, otherwise False</returns>
        public bool Publish(IContent content, int userId = 0)
        {
            return SaveAndPublishDo(content, false, userId);
        }

	    /// <summary>
	    /// Publishes a <see cref="IContent"/> object and all its children
	    /// </summary>
	    /// <param name="content">The <see cref="IContent"/> to publish along with its children</param>
	    /// <param name="userId">Optional Id of the User issueing the publishing</param>
	    /// <returns>True if publishing succeeded, otherwise False</returns>
	    public bool PublishWithChildren(IContent content, int userId = 0)
	    {
	        return PublishWithChildrenDo(content, false, userId);
	    }

	    /// <summary>
	    /// UnPublishes a single <see cref="IContent"/> object
	    /// </summary>
	    /// <param name="content">The <see cref="IContent"/> to publish</param>
	    /// <param name="userId">Optional Id of the User issueing the publishing</param>
	    /// <returns>True if unpublishing succeeded, otherwise False</returns>
	    public bool UnPublish(IContent content, int userId = 0)
	    {
	        return UnPublishDo(content, false, userId);
	    }

	    /// <summary>
	    /// Saves and Publishes a single <see cref="IContent"/> object
	    /// </summary>
	    /// <param name="content">The <see cref="IContent"/> to save and publish</param>
	    /// <param name="userId">Optional Id of the User issueing the publishing</param>
        /// <param name="raiseEvents">Optional boolean indicating whether or not to raise save events.</param>
	    /// <returns>True if publishing succeeded, otherwise False</returns>
        public bool SaveAndPublish(IContent content, int userId = 0, bool raiseEvents = true)
	    {
			return SaveAndPublishDo(content, false, userId, raiseEvents);
	    }

	    /// <summary>
	    /// Saves a single <see cref="IContent"/> object
	    /// </summary>
	    /// <param name="content">The <see cref="IContent"/> to save</param>
	    /// <param name="userId">Optional Id of the User saving the Content</param>
        /// <param name="raiseEvents">Optional boolean indicating whether or not to raise events.</param>
        public void Save(IContent content, int userId = 0, bool raiseEvents = true)
	    {
			Save(content, true, userId, raiseEvents);
	    }

	    /// <summary>
	    /// Saves a collection of <see cref="IContent"/> objects.
	    /// </summary>
	    /// <remarks>
	    /// If the collection of content contains new objects that references eachother by Id or ParentId,
	    /// then use the overload Save method with a collection of Lazy <see cref="IContent"/>.
	    /// </remarks>
	    /// <param name="contents">Collection of <see cref="IContent"/> to save</param>
	    /// <param name="userId">Optional Id of the User saving the Content</param>
        /// <param name="raiseEvents">Optional boolean indicating whether or not to raise events.</param>
        public void Save(IEnumerable<IContent> contents, int userId = 0, bool raiseEvents = true)
	    {
            if(raiseEvents)
			{
                if (Saving.IsRaisedEventCancelled(new SaveEventArgs<IContent>(contents), this))
				return;
            }

			var containsNew = contents.Any(x => x.HasIdentity == false);

			var uow = _uowProvider.GetUnitOfWork();
			using (var repository = _repositoryFactory.CreateContentRepository(uow))
			{
				if (containsNew)
				{
					foreach (var content in contents)
					{
					    content.WriterId = userId;

						//Only change the publish state if the "previous" version was actually published
						if (content.Published)
                            content.ChangePublishedState(PublishedState.Saved);

						repository.AddOrUpdate(content);
						uow.Commit();
					}
				}
				else
				{
					foreach (var content in contents)
					{
						content.WriterId = userId;
						repository.AddOrUpdate(content);
					}
					uow.Commit();
				}
			}
			
            if(raiseEvents)
                Saved.RaiseEvent(new SaveEventArgs<IContent>(contents, false), this);

			Audit.Add(AuditTypes.Save, "Bulk Save content performed by user", userId == -1 ? 0 : userId, -1);
	    }
		
	    /// <summary>
	    /// Deletes all content of specified type. All children of deleted content is moved to Recycle Bin.
	    /// </summary>
	    /// <remarks>This needs extra care and attention as its potentially a dangerous and extensive operation</remarks>
	    /// <param name="contentTypeId">Id of the <see cref="IContentType"/></param>
	    /// <param name="userId">Optional Id of the user issueing the delete operation</param>
	    public void DeleteContentOfType(int contentTypeId, int userId = 0)
	    {	        
			using (var uow = _uowProvider.GetUnitOfWork())
			{
				var repository = _repositoryFactory.CreateContentRepository(uow);
				//NOTE What about content that has the contenttype as part of its composition?
				var query = Query<IContent>.Builder.Where(x => x.ContentTypeId == contentTypeId);
				var contents = repository.GetByQuery(query);

				if (Deleting.IsRaisedEventCancelled(new DeleteEventArgs<IContent>(contents), this))
					return;

				foreach (var content in contents.OrderByDescending(x => x.ParentId))
				{
					//Look for children of current content and move that to trash before the current content is deleted
					var c = content;
					var childQuery = Query<IContent>.Builder.Where(x => x.Path.StartsWith(c.Path));
					var children = repository.GetByQuery(childQuery);

					foreach (var child in children)
					{
						if (child.ContentType.Id != contentTypeId)
							MoveToRecycleBin(child, userId);
					}

					//Permantly delete the content
					Delete(content, userId);
				}	
			}

			Audit.Add(AuditTypes.Delete,
					  string.Format("Delete Content of Type {0} performed by user", contentTypeId),
					  userId, -1);
	    }

	    /// <summary>
        /// Permanently deletes an <see cref="IContent"/> object.
        /// </summary>
        /// <remarks>
        /// This method will also delete associated media files, child content and possibly associated domains.
        /// </remarks>
        /// <remarks>Please note that this method will completely remove the Content from the database</remarks>
        /// <param name="content">The <see cref="IContent"/> to delete</param>
        /// <param name="userId">Optional Id of the User deleting the Content</param>
		public void Delete(IContent content, int userId = 0)
		{
	        if (Deleting.IsRaisedEventCancelled(new DeleteEventArgs<IContent>(content), this)) 
				return;

			//Make sure that published content is unpublished before being deleted
			if (HasPublishedVersion(content.Id))
			{
				UnPublish(content, userId);
			}

			//Delete children before deleting the 'possible parent'
			var children = GetChildren(content.Id);
			foreach (var child in children)
			{
				Delete(child, userId);
			}

			var uow = _uowProvider.GetUnitOfWork();
			using (var repository = _repositoryFactory.CreateContentRepository(uow))
			{
				repository.Delete(content);
				uow.Commit();
			}

			Deleted.RaiseEvent(new DeleteEventArgs<IContent>(content, false), this);

			Audit.Add(AuditTypes.Delete, "Delete Content performed by user", userId, content.Id);
		}

		/// <summary>
		/// Permanently deletes versions from an <see cref="IContent"/> object prior to a specific date.
		/// </summary>
		/// <param name="id">Id of the <see cref="IContent"/> object to delete versions from</param>
		/// <param name="versionDate">Latest version date</param>
		/// <param name="userId">Optional Id of the User deleting versions of a Content object</param>
		public void DeleteVersions(int id, DateTime versionDate, int userId = 0)
		{
			//TODO: We should check if we are going to delete the most recent version because if that happens it means the 
			// entity is completely deleted and we should raise the normal Deleting/Deleted event

			if (DeletingVersions.IsRaisedEventCancelled(new DeleteRevisionsEventArgs(id, dateToRetain: versionDate), this))
				return;

			var uow = _uowProvider.GetUnitOfWork();
			using (var repository = _repositoryFactory.CreateContentRepository(uow))
			{
				repository.DeleteVersions(id, versionDate);
				uow.Commit();
			}

			DeletedVersions.RaiseEvent(new DeleteRevisionsEventArgs(id, false, dateToRetain: versionDate), this);
			
			Audit.Add(AuditTypes.Delete, "Delete Content by version date performed by user", userId, -1);
		}

		/// <summary>
	    /// Permanently deletes specific version(s) from an <see cref="IContent"/> object.
	    /// </summary>
	    /// <param name="id">Id of the <see cref="IContent"/> object to delete a version from</param>
	    /// <param name="versionId">Id of the version to delete</param>
	    /// <param name="deletePriorVersions">Boolean indicating whether to delete versions prior to the versionId</param>
	    /// <param name="userId">Optional Id of the User deleting versions of a Content object</param>
	    public void DeleteVersion(int id, Guid versionId, bool deletePriorVersions, int userId = 0)
	    {
			//TODO: We should check if we are going to delete the most recent version because if that happens it means the 
			// entity is completely deleted and we should raise the normal Deleting/Deleted event

            if (deletePriorVersions)
            {
                var content = GetByVersion(versionId);
                DeleteVersions(id, content.UpdateDate, userId);
            }

			if (DeletingVersions.IsRaisedEventCancelled(new DeleteRevisionsEventArgs(id, specificVersion: versionId), this))
				return;

			var uow = _uowProvider.GetUnitOfWork();
			using (var repository = _repositoryFactory.CreateContentRepository(uow))
			{
				repository.DeleteVersion(versionId);
				uow.Commit();
			}

			DeletedVersions.RaiseEvent(new DeleteRevisionsEventArgs(id, false, specificVersion:versionId), this);

			Audit.Add(AuditTypes.Delete, "Delete Content by version performed by user", userId, -1);
	    }

	    /// <summary>
	    /// Deletes an <see cref="IContent"/> object by moving it to the Recycle Bin
	    /// </summary>
	    /// <remarks>Move an item to the Recycle Bin will result in the item being unpublished</remarks>
	    /// <param name="content">The <see cref="IContent"/> to delete</param>
	    /// <param name="userId">Optional Id of the User deleting the Content</param>
	    public void MoveToRecycleBin(IContent content, int userId = 0)
	    {
			if (Trashing.IsRaisedEventCancelled(new MoveEventArgs<IContent>(content, -20), this))
				return;

			//Make sure that published content is unpublished before being moved to the Recycle Bin
			if (HasPublishedVersion(content.Id))
			{
				UnPublish(content, userId);
			}

            //Unpublish descendents of the content item that is being moved to trash
	        var descendants = GetDescendants(content).ToList();
	        foreach (var descendant in descendants)
	        {
                UnPublish(descendant, userId);
	        }

			var uow = _uowProvider.GetUnitOfWork();
			using (var repository = _repositoryFactory.CreateContentRepository(uow))
			{
			    content.WriterId = userId;
				content.ChangeTrashedState(true);
				repository.AddOrUpdate(content);

                //Loop through descendants to update their trash state, but ensuring structure by keeping the ParentId
			    foreach (var descendant in descendants)
			    {
                    descendant.WriterId = userId;
                    descendant.ChangeTrashedState(true, descendant.ParentId);
                    repository.AddOrUpdate(descendant);
			    }

				uow.Commit();
			}

			Trashed.RaiseEvent(new MoveEventArgs<IContent>(content, false, -20), this);

			Audit.Add(AuditTypes.Move, "Move Content to Recycle Bin performed by user", userId, content.Id);
	    }

	    /// <summary>
	    /// Moves an <see cref="IContent"/> object to a new location by changing its parent id.
	    /// </summary>
	    /// <remarks>
	    /// If the <see cref="IContent"/> object is already published it will be
	    /// published after being moved to its new location. Otherwise it'll just
	    /// be saved with a new parent id.
	    /// </remarks>
	    /// <param name="content">The <see cref="IContent"/> to move</param>
	    /// <param name="parentId">Id of the Content's new Parent</param>
	    /// <param name="userId">Optional Id of the User moving the Content</param>
	    public void Move(IContent content, int parentId, int userId = 0)
	    {
	        //This ensures that the correct method is called if this method is used to Move to recycle bin.
	        if (parentId == -20)
	        {
	            MoveToRecycleBin(content, userId);
	            return;
	        }

	        if (Moving.IsRaisedEventCancelled(new MoveEventArgs<IContent>(content, parentId), this))
	            return;

	        content.WriterId = userId;
	        var parent = GetById(parentId);
	        content.Path = string.Concat(parent.Path, ",", content.Id);
	        content.Level = parent.Level + 1;

	        //If Content is being moved away from Recycle Bin, its state should be un-trashed
	        if (content.Trashed && parentId != -20)
	        {
	            content.ChangeTrashedState(false, parentId);
	        }
	        else
	        {
	            content.ParentId = parentId;
	        }

	        //If Content is published, it should be (re)published from its new location
	        if (content.Published)
	        {
                //If Content is Publishable its saved and published
                //otherwise we save the content without changing the publish state, and generate new xml because the Path, Level and Parent has changed.
                if (IsPublishable(content))
                {
                    SaveAndPublish(content, userId);
                }
                else
                {
                    Save(content, false, userId, true);

                    using (var uow = _uowProvider.GetUnitOfWork())
                    {
                        var xml = content.ToXml();
                        var poco = new ContentXmlDto {NodeId = content.Id, Xml = xml.ToString(SaveOptions.None)};
                        var exists =
                            uow.Database.FirstOrDefault<ContentXmlDto>("WHERE nodeId = @Id", new {Id = content.Id}) !=
                            null;
                        int result = exists
                                         ? uow.Database.Update(poco)
                                         : Convert.ToInt32(uow.Database.Insert(poco));
                    }
                }
	        }
	        else
	        {
	            Save(content, userId);
	        }

	        //Ensure that Path and Level is updated on children
	        var children = GetChildren(content.Id);
	        if (children.Any())
	        {
	            foreach (var child in children)
	            {
	                Move(child, content.Id, userId);
	            }
	        }

	        Moved.RaiseEvent(new MoveEventArgs<IContent>(content, false, parentId), this);

			Audit.Add(AuditTypes.Move, "Move Content performed by user", userId, content.Id);
		}

        /// <summary>
		/// Empties the Recycle Bin by deleting all <see cref="IContent"/> that resides in the bin
		/// </summary>
		public void EmptyRecycleBin()
		{
			//TODO: Why don't we have a base class to share between MediaService/ContentService as some of this is exacty the same?

			var uow = _uowProvider.GetUnitOfWork();
			using (var repository = _repositoryFactory.CreateContentRepository(uow))
			{
				var query = Query<IContent>.Builder.Where(x => x.ParentId == -20);
				var contents = repository.GetByQuery(query);

				foreach (var content in contents)
				{
					if (Deleting.IsRaisedEventCancelled(new DeleteEventArgs<IContent>(content), this))
						continue;

					repository.Delete(content);

					Deleted.RaiseEvent(new DeleteEventArgs<IContent>(content, false), this);
				}
				uow.Commit();
			}

            Audit.Add(AuditTypes.Delete, "Empty Recycle Bin performed by user", 0, -20);
		}

        /// <summary>
        /// Copies an <see cref="IContent"/> object by creating a new Content object of the same type and copies all data from the current 
        /// to the new copy which is returned.
        /// </summary>
        /// <param name="content">The <see cref="IContent"/> to copy</param>
        /// <param name="parentId">Id of the Content's new Parent</param>
        /// <param name="relateToOriginal">Boolean indicating whether the copy should be related to the original</param>
        /// <param name="userId">Optional Id of the User copying the Content</param>
        /// <returns>The newly created <see cref="IContent"/> object</returns>
        public IContent Copy(IContent content, int parentId, bool relateToOriginal, int userId = 0)
        {
			var copy = ((Content)content).Clone();
			copy.ParentId = parentId;

            // A copy should never be set to published automatically even if the original was.
            copy.ChangePublishedState(PublishedState.Unpublished);

			if (Copying.IsRaisedEventCancelled(new CopyEventArgs<IContent>(content, copy, parentId), this))
				return null;

			var uow = _uowProvider.GetUnitOfWork();
			using (var repository = _repositoryFactory.CreateContentRepository(uow))
			{
				content.WriterId = userId;
				repository.AddOrUpdate(copy);
				uow.Commit();

                //Special case for the Upload DataType
				var uploadDataTypeId = new Guid("5032a6e6-69e3-491d-bb28-cd31cd11086c");
				if (content.Properties.Any(x => x.PropertyType.DataTypeId == uploadDataTypeId))
				{
					bool isUpdated = false;
					var fs = FileSystemProviderManager.Current.GetFileSystemProvider<MediaFileSystem>();

					//Loop through properties to check if the content contains media that should be deleted
					foreach (var property in content.Properties.Where(x => x.PropertyType.DataTypeId == uploadDataTypeId
						&& string.IsNullOrEmpty(x.Value.ToString()) == false))
					{
						if (fs.FileExists(IOHelper.MapPath(property.Value.ToString())))
						{
							var currentPath = fs.GetRelativePath(property.Value.ToString());
							var propertyId = copy.Properties.First(x => x.Alias == property.Alias).Id;
							var newPath = fs.GetRelativePath(propertyId, System.IO.Path.GetFileName(currentPath));

							fs.CopyFile(currentPath, newPath);
							copy.SetValue(property.Alias, fs.GetUrl(newPath));

							//Copy thumbnails
							foreach (var thumbPath in fs.GetThumbnails(currentPath))
							{
								var newThumbPath = fs.GetRelativePath(propertyId, System.IO.Path.GetFileName(thumbPath));
								fs.CopyFile(thumbPath, newThumbPath);
							}
							isUpdated = true;
						}
					}

					if (isUpdated)
					{
						repository.AddOrUpdate(copy);
						uow.Commit();
					}
				}

                //Special case for the Tags DataType
                var tagsDataTypeId = new Guid("4023e540-92f5-11dd-ad8b-0800200c9a66");
                if (content.Properties.Any(x => x.PropertyType.DataTypeId == tagsDataTypeId))
                {
                    var tags = uow.Database.Fetch<TagRelationshipDto>("WHERE nodeId = @Id", new {Id = content.Id});
                    foreach (var tag in tags)
                    {
                        uow.Database.Insert(new TagRelationshipDto {NodeId = copy.Id, TagId = tag.TagId});
                    }
                }
			}

			//NOTE This 'Relation' part should eventually be delegated to a RelationService
			if (relateToOriginal)
			{
				RelationType relationType = null;
				using (var relationTypeRepository = _repositoryFactory.CreateRelationTypeRepository(uow))
				{
					relationType = relationTypeRepository.Get(1);
				}

				using (var relationRepository = _repositoryFactory.CreateRelationRepository(uow))
				{
					var relation = new Relation(content.Id, copy.Id, relationType);
					relationRepository.AddOrUpdate(relation);
					uow.Commit();
				}

				Audit.Add(AuditTypes.Copy,
						  string.Format("Copied content with Id: '{0}' related to original content with Id: '{1}'",
										copy.Id, content.Id), copy.WriterId, copy.Id);
			}

			//Look for children and copy those as well
			var children = GetChildren(content.Id);
			foreach (var child in children)
			{
				Copy(child, copy.Id, relateToOriginal, userId);
			}

			Copied.RaiseEvent(new CopyEventArgs<IContent>(content, copy, false, parentId), this);

			Audit.Add(AuditTypes.Copy, "Copy Content performed by user", content.WriterId, content.Id);

			return copy;
		}

		/// <summary>
		/// Sends an <see cref="IContent"/> to Publication, which executes handlers and events for the 'Send to Publication' action.
		/// </summary>
		/// <param name="content">The <see cref="IContent"/> to send to publication</param>
		/// <param name="userId">Optional Id of the User issueing the send to publication</param>
		/// <returns>True if sending publication was succesfull otherwise false</returns>
		internal bool SendToPublication(IContent content, int userId = 0)
		{

			if (SendingToPublish.IsRaisedEventCancelled(new SendToPublishEventArgs<IContent>(content), this))
				return false;

			//TODO: Do some stuff here.. RunActionHandlers

			SentToPublish.RaiseEvent(new SendToPublishEventArgs<IContent>(content, false), this);

			Audit.Add(AuditTypes.SendToPublish, "Send to Publish performed by user", content.WriterId, content.Id);

			return true;
		}

	    /// <summary>
	    /// Rollback an <see cref="IContent"/> object to a previous version.
	    /// This will create a new version, which is a copy of all the old data.
	    /// </summary>
	    /// <remarks>
	    /// The way data is stored actually only allows us to rollback on properties
	    /// and not data like Name and Alias of the Content.
	    /// </remarks>
	    /// <param name="id">Id of the <see cref="IContent"/>being rolled back</param>
	    /// <param name="versionId">Id of the version to rollback to</param>
	    /// <param name="userId">Optional Id of the User issueing the rollback of the Content</param>
	    /// <returns>The newly created <see cref="IContent"/> object</returns>
	    public IContent Rollback(int id, Guid versionId, int userId = 0)
	    {
	        var content = GetByVersion(versionId);

			if (RollingBack.IsRaisedEventCancelled(new RollbackEventArgs<IContent>(content), this))
				return content;

			var uow = _uowProvider.GetUnitOfWork();
			using (var repository = _repositoryFactory.CreateContentRepository(uow))
			{
				content.WriterId = userId;
			    content.CreatorId = userId;

				repository.AddOrUpdate(content);
				uow.Commit();
			}

			RolledBack.RaiseEvent(new RollbackEventArgs<IContent>(content, false), this);

			Audit.Add(AuditTypes.RollBack, "Content rollback performed by user", content.WriterId, content.Id);

		    return content;
	    }

        #region Internal Methods
        /// <summary>
        /// Internal method to Re-Publishes all Content for legacy purposes.
        /// </summary>
        /// <param name="userId">Optional Id of the User issueing the publishing</param>
        /// <param name="omitCacheRefresh">Optional boolean to avoid having the cache refreshed when calling this RePublish method. By default this method will not update the cache.</param>
        /// <returns>True if publishing succeeded, otherwise False</returns>
        internal bool RePublishAll(bool omitCacheRefresh = true, int userId = 0)
        {
            return RePublishAllDo(omitCacheRefresh, userId);
        }

        /// <summary>
        /// Internal method that Publishes a single <see cref="IContent"/> object for legacy purposes.
        /// </summary>
        /// <param name="content">The <see cref="IContent"/> to publish</param>
        /// <param name="omitCacheRefresh">Optional boolean to avoid having the cache refreshed when calling this Publish method. By default this method will not update the cache.</param>
        /// <param name="userId">Optional Id of the User issueing the publishing</param>
        /// <returns>True if publishing succeeded, otherwise False</returns>
        internal bool Publish(IContent content, bool omitCacheRefresh = true, int userId = 0)
        {
            return SaveAndPublishDo(content, omitCacheRefresh, userId);
        }

        /// <summary>
        /// Internal method that Publishes a <see cref="IContent"/> object and all its children for legacy purposes.
        /// </summary>
        /// <param name="content">The <see cref="IContent"/> to publish along with its children</param>
        /// <param name="omitCacheRefresh">Optional boolean to avoid having the cache refreshed when calling this Publish method. By default this method will not update the cache.</param>
        /// <param name="userId">Optional Id of the User issueing the publishing</param>
        /// <returns>True if publishing succeeded, otherwise False</returns>
        internal bool PublishWithChildren(IContent content, bool omitCacheRefresh = true, int userId = 0)
        {
            return PublishWithChildrenDo(content, omitCacheRefresh, userId);
        }

        /// <summary>
        /// Internal method that UnPublishes a single <see cref="IContent"/> object for legacy purposes.
        /// </summary>
        /// <param name="content">The <see cref="IContent"/> to publish</param>
        /// <param name="omitCacheRefresh">Optional boolean to avoid having the cache refreshed when calling this Unpublish method. By default this method will not update the cache.</param>
        /// <param name="userId">Optional Id of the User issueing the publishing</param>
        /// <returns>True if unpublishing succeeded, otherwise False</returns>
        internal bool UnPublish(IContent content, bool omitCacheRefresh = true, int userId = 0)
        {
            return UnPublishDo(content, omitCacheRefresh, userId);
        }

	    /// <summary>
	    /// Saves and Publishes a single <see cref="IContent"/> object
	    /// </summary>
	    /// <param name="content">The <see cref="IContent"/> to save and publish</param>
	    /// <param name="omitCacheRefresh">Optional boolean to avoid having the cache refreshed when calling this Publish method. By default this method will not update the cache.</param>
	    /// <param name="userId">Optional Id of the User issueing the publishing</param>
        /// <param name="raiseEvents">Optional boolean indicating whether or not to raise save events.</param>
	    /// <returns>True if publishing succeeded, otherwise False</returns>
	    internal bool SaveAndPublish(IContent content, bool omitCacheRefresh = true, int userId = 0, bool raiseEvents = true)
        {
            return SaveAndPublishDo(content, omitCacheRefresh, userId, raiseEvents);
        }

        /// <summary>
        /// Gets a collection of <see cref="IContent"/> descendants by the first Parent.
        /// </summary>
        /// <param name="content"><see cref="IContent"/> item to retrieve Descendants from</param>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        internal IEnumerable<IContent> GetPublishedDescendants(IContent content)
        {
            using (var repository = _repositoryFactory.CreateContentRepository(_uowProvider.GetUnitOfWork()))
            {
                var query = Query<IContent>.Builder.Where(x => x.Id != content.Id && x.Path.StartsWith(content.Path) && x.Published == true && x.Trashed == false);
                var contents = repository.GetByQuery(query);

                return contents;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Re-Publishes all Content
        /// </summary>
        /// <param name="userId">Optional Id of the User issueing the publishing</param>
        /// <param name="omitCacheRefresh">Optional boolean to avoid having the cache refreshed when calling this RePublish method. By default this method will update the cache.</param>
        /// <returns>True if publishing succeeded, otherwise False</returns>
        private bool RePublishAllDo(bool omitCacheRefresh = false, int userId = 0)
        {
            var list = new List<IContent>();
            var updated = new List<IContent>();

            //Consider creating a Path query instead of recursive method:
            //var query = Query<IContent>.Builder.Where(x => x.Path.StartsWith("-1"));

            var rootContent = GetRootContent();
            foreach (var content in rootContent)
            {
                if (content.IsValid())
                {
                    list.Add(content);
                    list.AddRange(GetDescendants(content));
                }
            }

            //Publish and then update the database with new status
            var published = _publishingStrategy.PublishWithChildren(list, userId);
            if (published)
            {
                var uow = _uowProvider.GetUnitOfWork();
                using (var repository = _repositoryFactory.CreateContentRepository(uow))
                {
                    //Only loop through content where the Published property has been updated
                    foreach (var item in list.Where(x => ((ICanBeDirty)x).IsPropertyDirty("Published")))
                    {
                        item.WriterId = userId;
                        repository.AddOrUpdate(item);
                        updated.Add(item);
                    }

                    uow.Commit();

                    foreach (var c in updated)
                    {
                        var xml = c.ToXml();
                        var poco = new ContentXmlDto { NodeId = c.Id, Xml = xml.ToString(SaveOptions.None) };
                        var exists = uow.Database.FirstOrDefault<ContentXmlDto>("WHERE nodeId = @Id", new { Id = c.Id }) !=
                                     null;
                        int result = exists
                                         ? uow.Database.Update(poco)
                                         : Convert.ToInt32(uow.Database.Insert(poco));
                    }
                }
                //Updating content to published state is finished, so we fire event through PublishingStrategy to have cache updated
                if (omitCacheRefresh == false)
                    _publishingStrategy.PublishingFinalized(updated, true);
            }

            Audit.Add(AuditTypes.Publish, "RePublish All performed by user", userId, -1);

            return published;
        }

        /// <summary>
        /// Publishes a <see cref="IContent"/> object and all its children
        /// </summary>
        /// <param name="content">The <see cref="IContent"/> to publish along with its children</param>
        /// <param name="omitCacheRefresh">Optional boolean to avoid having the cache refreshed when calling this Publish method. By default this method will update the cache.</param>
        /// <param name="userId">Optional Id of the User issueing the publishing</param>
        /// <returns>True if publishing succeeded, otherwise False</returns>
        private bool PublishWithChildrenDo(IContent content, bool omitCacheRefresh = false, int userId = 0)
        {
            //Check if parent is published (although not if its a root node) - if parent isn't published this Content cannot be published
            if (content.ParentId != -1 && content.ParentId != -20 && IsPublishable(content) == false)
            {
                LogHelper.Info<ContentService>(
                    string.Format(
                        "Content '{0}' with Id '{1}' could not be published because its parent or one of its ancestors is not published.",
                        content.Name, content.Id));
                return false;
            }

            //Content contains invalid property values and can therefore not be published - fire event?
            if (!content.IsValid())
            {
                LogHelper.Info<ContentService>(
                    string.Format("Content '{0}' with Id '{1}' could not be published because of invalid properties.",
                                  content.Name, content.Id));
                return false;
            }

            //Consider creating a Path query instead of recursive method:
            //var query = Query<IContent>.Builder.Where(x => x.Path.StartsWith(content.Path));

            var updated = new List<IContent>();
            var list = new List<IContent>();
            list.Add(content);
            list.AddRange(GetDescendants(content));

            //Publish and then update the database with new status
            var published = _publishingStrategy.PublishWithChildren(list, userId);
            if (published)
            {
                var uow = _uowProvider.GetUnitOfWork();
                using (var repository = _repositoryFactory.CreateContentRepository(uow))
                {
                    //Only loop through content where the Published property has been updated
                    foreach (var item in list.Where(x => ((ICanBeDirty)x).IsPropertyDirty("Published")))
                    {
                        item.WriterId = userId;
                        repository.AddOrUpdate(item);
                        updated.Add(item);
                    }

                    uow.Commit();

                    foreach (var c in updated)
                    {
                        var xml = c.ToXml();
                        var poco = new ContentXmlDto { NodeId = c.Id, Xml = xml.ToString(SaveOptions.None) };
                        var exists = uow.Database.FirstOrDefault<ContentXmlDto>("WHERE nodeId = @Id", new { Id = c.Id }) !=
                                     null;
                        int result = exists
                                         ? uow.Database.Update(poco)
                                         : Convert.ToInt32(uow.Database.Insert(poco));
                    }
                }
                //Save xml to db and call following method to fire event:
                if (omitCacheRefresh == false)
                    _publishingStrategy.PublishingFinalized(updated, false);

                Audit.Add(AuditTypes.Publish, "Publish with Children performed by user", userId, content.Id);
            }

            return published;
        }

        /// <summary>
        /// UnPublishes a single <see cref="IContent"/> object
        /// </summary>
        /// <param name="content">The <see cref="IContent"/> to publish</param>
        /// <param name="omitCacheRefresh">Optional boolean to avoid having the cache refreshed when calling this Unpublish method. By default this method will update the cache.</param>
        /// <param name="userId">Optional Id of the User issueing the publishing</param>
        /// <returns>True if unpublishing succeeded, otherwise False</returns>
        private bool UnPublishDo(IContent content, bool omitCacheRefresh = false, int userId = 0)
        {
            var unpublished = _publishingStrategy.UnPublish(content, userId);
            if (unpublished)
            {
                var uow = _uowProvider.GetUnitOfWork();
                using (var repository = _repositoryFactory.CreateContentRepository(uow))
                {
                    content.WriterId = userId;
                    repository.AddOrUpdate(content);

                    //Remove 'published' xml from the cmsContentXml table for the unpublished content
                    uow.Database.Delete<ContentXmlDto>("WHERE nodeId = @Id", new { Id = content.Id });

                    uow.Commit();
                }
                //Delete xml from db? and call following method to fire event through PublishingStrategy to update cache
                if (omitCacheRefresh == false)
                    _publishingStrategy.UnPublishingFinalized(content);

                Audit.Add(AuditTypes.UnPublish, "UnPublish performed by user", userId, content.Id);
            }

            return unpublished;
        }

	    /// <summary>
	    /// Saves and Publishes a single <see cref="IContent"/> object
	    /// </summary>
	    /// <param name="content">The <see cref="IContent"/> to save and publish</param>
	    /// <param name="omitCacheRefresh">Optional boolean to avoid having the cache refreshed when calling this Publish method. By default this method will update the cache.</param>
	    /// <param name="userId">Optional Id of the User issueing the publishing</param>
        /// <param name="raiseEvents">Optional boolean indicating whether or not to raise save events.</param>
	    /// <returns>True if publishing succeeded, otherwise False</returns>
	    private bool SaveAndPublishDo(IContent content, bool omitCacheRefresh = false, int userId = 0, bool raiseEvents = true)
        {
            if(raiseEvents)
            {
                if (Saving.IsRaisedEventCancelled(new SaveEventArgs<IContent>(content), this))
                return false;
            }

            //Has this content item previously been published? If so, we don't need to refresh the children
	        var previouslyPublished = HasPublishedVersion(content.Id);
	        var validForPublishing = true;

            //Check if parent is published (although not if its a root node) - if parent isn't published this Content cannot be published
            if (content.ParentId != -1 && content.ParentId != -20 && IsPublishable(content) == false)
            {
                LogHelper.Info<ContentService>(
                    string.Format(
                        "Content '{0}' with Id '{1}' could not be published because its parent is not published.",
                        content.Name, content.Id));
                validForPublishing = false;
            }

            //Content contains invalid property values and can therefore not be published - fire event?
            if (!content.IsValid())
            {
                LogHelper.Info<ContentService>(
                    string.Format(
                        "Content '{0}' with Id '{1}' could not be published because of invalid properties.",
                        content.Name, content.Id));
                validForPublishing = false;
            }

            //Publish and then update the database with new status
	        bool published = validForPublishing && _publishingStrategy.Publish(content, userId);

            var uow = _uowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreateContentRepository(uow))
            {
                //Since this is the Save and Publish method, the content should be saved even though the publish fails or isn't allowed
                content.WriterId = userId;

                repository.AddOrUpdate(content);

                uow.Commit();

                var xml = content.ToXml();
                //Preview Xml
                var previewPoco = new PreviewXmlDto
                {
                    NodeId = content.Id,
                    Timestamp = DateTime.Now,
                    VersionId = content.Version,
                    Xml = xml.ToString(SaveOptions.None)
                };
                var previewExists =
                    uow.Database.ExecuteScalar<int>("SELECT COUNT(nodeId) FROM cmsPreviewXml WHERE nodeId = @Id AND versionId = @Version",
                                                               new { Id = content.Id, Version = content.Version }) != 0;
                int previewResult = previewExists
                                        ? uow.Database.Update<PreviewXmlDto>(
                                            "SET xml = @Xml, timestamp = @Timestamp WHERE nodeId = @Id AND versionId = @Version",
                                            new
                                                {
                                                    Xml = previewPoco.Xml,
                                                    Timestamp = previewPoco.Timestamp,
                                                    Id = previewPoco.NodeId,
                                                    Version = previewPoco.VersionId
                                                })
                                        : Convert.ToInt32(uow.Database.Insert(previewPoco));

                if (published)
                {
                    //Content Xml
                    var contentPoco = new ContentXmlDto { NodeId = content.Id, Xml = xml.ToString(SaveOptions.None) };
                    var contentExists = uow.Database.ExecuteScalar<int>("SELECT COUNT(nodeId) FROM cmsContentXml WHERE nodeId = @Id", new { Id = content.Id }) != 0;
                    int contentResult = contentExists
                                            ? uow.Database.Update(contentPoco)
                                            : Convert.ToInt32(uow.Database.Insert(contentPoco));

                }
            }

            if(raiseEvents)
                Saved.RaiseEvent(new SaveEventArgs<IContent>(content, false), this);

            //Save xml to db and call following method to fire event through PublishingStrategy to update cache
            if (published && omitCacheRefresh == false)
                _publishingStrategy.PublishingFinalized(content);

            //We need to check if children and their publish state to ensure that we 'republish' content that was previously published
            if (published && omitCacheRefresh == false && previouslyPublished == false && HasChildren(content.Id))
            {
                var descendants = GetPublishedDescendants(content);

                _publishingStrategy.PublishingFinalized(descendants, false);
            }

            Audit.Add(AuditTypes.Publish, "Save and Publish performed by user", userId, content.Id);

            return published;
        }

	    /// <summary>
	    /// Saves a single <see cref="IContent"/> object
	    /// </summary>
	    /// <param name="content">The <see cref="IContent"/> to save</param>
	    /// <param name="changeState">Boolean indicating whether or not to change the Published state upon saving</param>
	    /// <param name="userId">Optional Id of the User saving the Content</param>
        /// <param name="raiseEvents">Optional boolean indicating whether or not to raise events.</param>
	    private void Save(IContent content, bool changeState, int userId = 0, bool raiseEvents = true)
        {
            if(raiseEvents)
            {
                if (Saving.IsRaisedEventCancelled(new SaveEventArgs<IContent>(content), this))
                return;
            }

            var uow = _uowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreateContentRepository(uow))
            {
                content.WriterId = userId;

                //Only change the publish state if the "previous" version was actually published
                if (changeState && content.Published)
                    content.ChangePublishedState(PublishedState.Saved);

                repository.AddOrUpdate(content);
                uow.Commit();

                //Preview Xml
                var xml = content.ToXml();
                var previewPoco = new PreviewXmlDto
                                      {
                                          NodeId = content.Id,
                                          Timestamp = DateTime.Now,
                                          VersionId = content.Version,
                                          Xml = xml.ToString(SaveOptions.None)
                                      };
                var previewExists =
                    uow.Database.ExecuteScalar<int>("SELECT COUNT(nodeId) FROM cmsPreviewXml WHERE nodeId = @Id AND versionId = @Version",
                                                               new { Id = content.Id, Version = content.Version }) != 0;
                int previewResult = previewExists
                                        ? uow.Database.Update<PreviewXmlDto>(
                                            "SET xml = @Xml, timestamp = @Timestamp WHERE nodeId = @Id AND versionId = @Version",
                                            new
                                                {
                                                    Xml = previewPoco.Xml,
                                                    Timestamp = previewPoco.Timestamp,
                                                    Id = previewPoco.NodeId,
                                                    Version = previewPoco.VersionId
                                                })
                                        : Convert.ToInt32(uow.Database.Insert(previewPoco));
            }

            if(raiseEvents)
                Saved.RaiseEvent(new SaveEventArgs<IContent>(content, false), this);

            Audit.Add(AuditTypes.Save, "Save Content performed by user", userId, content.Id);
        }

        /// <summary>
        /// Checks if the passed in <see cref="IContent"/> can be published based on the anscestors publish state.
        /// </summary>
        /// <remarks>
        /// Check current is only used when falling back to checking the Parent of non-saved content, as
        /// non-saved content doesn't have a valid path yet.
        /// </remarks>
        /// <param name="content"><see cref="IContent"/> to check if anscestors are published</param>
        /// <param name="checkCurrent">Boolean indicating whether the passed in content should also be checked for published versions</param>
        /// <returns>True if the Content can be published, otherwise False</returns>
        private bool IsPublishable(IContent content, bool checkCurrent)
        {
            var ids = content.Path.Split(',').Select(int.Parse).ToList();
            foreach (var id in ids)
            {
                //If Id equals that of the recycle bin we return false because nothing in the bin can be published
                if (id == -20)
                    return false;

                //We don't check the System Root, so just continue
                if (id == -1) continue;

                //If the current id equals that of the passed in content and if current shouldn't be checked we skip it.
                if (checkCurrent == false && id == content.Id) continue;

                //Check if the content for the current id is published - escape the loop if we encounter content that isn't published
                var hasPublishedVersion = HasPublishedVersion(id);
                if (hasPublishedVersion == false)
                    return false;
            }

            return true;
        }

        #endregion

        #region Event Handlers
        /// <summary>
		/// Occurs before Delete
		/// </summary>		
		public static event TypedEventHandler<IContentService, DeleteEventArgs<IContent>> Deleting;

		/// <summary>
		/// Occurs after Delete
		/// </summary>
		public static event TypedEventHandler<IContentService, DeleteEventArgs<IContent>> Deleted;

		/// <summary>
		/// Occurs before Delete Versions
		/// </summary>		
		public static event TypedEventHandler<IContentService, DeleteRevisionsEventArgs> DeletingVersions;

		/// <summary>
		/// Occurs after Delete Versions
		/// </summary>
		public static event TypedEventHandler<IContentService, DeleteRevisionsEventArgs> DeletedVersions;

		/// <summary>
		/// Occurs before Save
		/// </summary>
		public static event TypedEventHandler<IContentService, SaveEventArgs<IContent>> Saving;
		
		/// <summary>
		/// Occurs after Save
		/// </summary>
		public static event TypedEventHandler<IContentService, SaveEventArgs<IContent>> Saved;
		
		/// <summary>
		/// Occurs before Create
		/// </summary>
		public static event TypedEventHandler<IContentService, NewEventArgs<IContent>> Creating;

		/// <summary>
		/// Occurs after Create
		/// </summary>
        /// <remarks>
        /// Please note that the Content object has been created, but not saved
        /// so it does not have an identity yet (meaning no Id has been set).
        /// </remarks>
		public static event TypedEventHandler<IContentService, NewEventArgs<IContent>> Created;

		/// <summary>
		/// Occurs before Copy
		/// </summary>
		public static event TypedEventHandler<IContentService, CopyEventArgs<IContent>> Copying;

		/// <summary>
		/// Occurs after Copy
		/// </summary>
		public static event TypedEventHandler<IContentService, CopyEventArgs<IContent>> Copied;

		/// <summary>
		/// Occurs before Content is moved to Recycle Bin
		/// </summary>
		public static event TypedEventHandler<IContentService, MoveEventArgs<IContent>> Trashing;

		/// <summary>
		/// Occurs after Content is moved to Recycle Bin
		/// </summary>
		public static event TypedEventHandler<IContentService, MoveEventArgs<IContent>> Trashed;

		/// <summary>
		/// Occurs before Move
		/// </summary>
		public static event TypedEventHandler<IContentService, MoveEventArgs<IContent>> Moving;

		/// <summary>
		/// Occurs after Move
		/// </summary>
		public static event TypedEventHandler<IContentService, MoveEventArgs<IContent>> Moved;

		/// <summary>
		/// Occurs before Rollback
		/// </summary>
		public static event TypedEventHandler<IContentService, RollbackEventArgs<IContent>> RollingBack;

		/// <summary>
		/// Occurs after Rollback
		/// </summary>
		public static event TypedEventHandler<IContentService, RollbackEventArgs<IContent>> RolledBack;

		/// <summary>
		/// Occurs before Send to Publish
		/// </summary>
		public static event TypedEventHandler<IContentService, SendToPublishEventArgs<IContent>> SendingToPublish;

		/// <summary>
		/// Occurs after Send to Publish
		/// </summary>
		public static event TypedEventHandler<IContentService, SendToPublishEventArgs<IContent>> SentToPublish;
		#endregion
	}
}