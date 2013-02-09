﻿using System;
using System.IO;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Events;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.Persistence;
using Umbraco.Core.Publishing;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;
using umbraco.editorControls.tinyMCE3;
using umbraco.interfaces;
using System.Linq;

namespace Umbraco.Tests.Publishing
{
    [TestFixture]
    public class PublishingStrategyTests : BaseDatabaseFactoryTest
    {
        [SetUp]
        public override void Initialize()
        {
            UmbracoSettings.SettingsFilePath = IOHelper.MapPath(SystemDirectories.Config + Path.DirectorySeparatorChar, false);

            //this ensures its reset
            PluginManager.Current = new PluginManager();

            //for testing, we'll specify which assemblies are scanned for the PluginTypeResolver
            PluginManager.Current.AssembliesToScan = new[]
				{
                    typeof(IDataType).Assembly,
                    typeof(tinyMCE3dataType).Assembly
				};

            DataTypesResolver.Current = new DataTypesResolver(
                () => PluginManager.Current.ResolveDataTypes());

            base.Initialize();

            CreateTestData();
        }

        [TearDown]
        public override void TearDown()
        {
			base.TearDown();
            
            //ensure event handler is gone
            PublishingStrategy.Publishing -= PublishingStrategyPublishing;

            //TestHelper.ClearDatabase();

            //reset the app context
            DataTypesResolver.Reset();
            RepositoryResolver.Reset();
            ApplicationContext.Current = null;
            
            string path = TestHelper.CurrentAssemblyDirectory;
            AppDomain.CurrentDomain.SetData("DataDirectory", null);

            UmbracoSettings.ResetSetters();
        }

        private IContent _homePage;

        /// <summary>
        /// in these tests we have a heirarchy of 
        /// - home
        /// -- text page 1
        /// -- text page 2
        /// --- text page 3
        /// 
        /// For this test, none of them are published, then we bulk publish them all, however we cancel the publishing for
        /// "text page 2". This internally will ensure that text page 3 doesn't get published either because text page 2 has 
        /// never been published.
        /// </summary>
        [Test]
        public void Publishes_Many_Ignores_Cancelled_Items_And_Children()
        {
            var strategy = new PublishingStrategy();
            

            PublishingStrategy.Publishing +=PublishingStrategyPublishing;

            //publish root and nodes at it's children level
            var listToPublish = ServiceContext.ContentService.GetDescendants(_homePage.Id).Concat(new[] {_homePage});
            var result = strategy.PublishWithChildrenInternal(listToPublish, 0);
            
            Assert.AreEqual(listToPublish.Count() - 2, result.Count(x => x.Success));
            Assert.IsTrue(result.Where(x => x.Success).Select(x => x.Result.ContentItem.Id)
                                .ContainsAll(listToPublish.Where(x => x.Name != "Text Page 2" && x.Name != "Text Page 3").Select(x => x.Id)));
        }

        static void PublishingStrategyPublishing(IPublishingStrategy sender, PublishEventArgs<IContent> e)
        {
            foreach (var i in e.PublishedEntities)
            {
                if (i.Name == "Text Page 2")
                {
                    e.Cancel = true;
                }
            }
        }

        [Test]
        public void Publishes_Many_Ignores_Unpublished_Items()
        {
            var strategy = new PublishingStrategy();
            
            //publish root and nodes at it's children level
            var result1 = strategy.Publish(_homePage, 0);
            Assert.IsTrue(result1);
            Assert.IsTrue(_homePage.Published);
            foreach (var c in ServiceContext.ContentService.GetChildren(_homePage.Id))
            {
                var r = strategy.Publish(c, 0);    
                Assert.IsTrue(r);
                Assert.IsTrue(c.Published);
            }

            //ok, all are published except the deepest descendant, we will pass in a flag to not include it to 
            //be published
            var result = strategy.PublishWithChildrenInternal(
                ServiceContext.ContentService.GetDescendants(_homePage).Concat(new[] {_homePage}), 0, false);
            Assert.AreEqual(0, result.Count());
        }

        [Test]
        public void Publishes_Many_Does_Not_Ignore_Unpublished_Items()
        {
            var strategy = new PublishingStrategy();

            //publish root and nodes at it's children level
            var result1 = strategy.Publish(_homePage, 0);
            Assert.IsTrue(result1);
            Assert.IsTrue(_homePage.Published);
            foreach (var c in ServiceContext.ContentService.GetChildren(_homePage.Id))
            {
                var r = strategy.Publish(c, 0);
                Assert.IsTrue(r);
                Assert.IsTrue(c.Published);
            }

            //ok, all are published except the deepest descendant, we will pass in a flag to include it to 
            //be published
            var result = strategy.PublishWithChildrenInternal(
                ServiceContext.ContentService.GetDescendants(_homePage).Concat(new[] { _homePage }), 0, true);
            Assert.AreEqual(1, result.Count());
            Assert.IsTrue(result.First().Success);
            Assert.IsTrue(result.First().Result.ContentItem.Published);
        }

        [Test]
        public void Can_Publish_And_Update_Xml_Cache()
        {
            //TODO Create new test
        }

        public void CreateTestData()
        {
            //NOTE Maybe not the best way to create/save test data as we are using the services, which are being tested.

            //Create and Save ContentType "umbTextpage" -> 1045
            ContentType contentType = MockedContentTypes.CreateSimpleContentType("umbTextpage", "Textpage");
            ServiceContext.ContentTypeService.Save(contentType);

            //Create and Save Content "Homepage" based on "umbTextpage" -> 1046
            _homePage = MockedContent.CreateSimpleContent(contentType);
            ServiceContext.ContentService.Save(_homePage, 0);

            //Create and Save Content "Text Page 1" based on "umbTextpage" -> 1047
            Content subpage = MockedContent.CreateSimpleContent(contentType, "Text Page 1", _homePage.Id);
            ServiceContext.ContentService.Save(subpage, 0);

            //Create and Save Content "Text Page 2" based on "umbTextpage" -> 1048
            Content subpage2 = MockedContent.CreateSimpleContent(contentType, "Text Page 2", _homePage.Id);
            ServiceContext.ContentService.Save(subpage2, 0);

            //Create and Save Content "Text Page 3" based on "umbTextpage" -> 1048
            Content subpage3 = MockedContent.CreateSimpleContent(contentType, "Text Page 3", subpage2.Id);
            ServiceContext.ContentService.Save(subpage3, 0);

        }
    }
}