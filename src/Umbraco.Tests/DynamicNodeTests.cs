using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Dynamics;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.template;

namespace Umbraco.Tests
{
	[TestFixture]
	public class DynamicNodeTests : BaseWebTest
	{
		public override void Initialize()
		{
			base.Initialize();

			DynamicDocumentDataSourceResolver.Current = new DynamicDocumentDataSourceResolver(
				new TestDynamicDocumentDataSource());
		}

		public override void TearDown()
		{
			base.TearDown();

			DynamicDocumentDataSourceResolver.Reset();
		}

		private DynamicDocument GetDynamicNode(int id)
		{
			var template = Template.MakeNew("test", new User(0));
			var ctx = GetUmbracoContext("/test", template);
			var contentStore = new XmlPublishedContentStore();
			var doc = contentStore.GetDocumentById(ctx, id);
			Assert.IsNotNull(doc);
			var dynamicNode = new DynamicDocument(doc);
			Assert.IsNotNull(dynamicNode);
			return dynamicNode;
		}

		[Test]
		public void Get_Children_With_Pluralized_Alias()
		{
			var dynamicNode = GetDynamicNode(1173);
			var asDynamic = dynamicNode.AsDynamic();

			Action<dynamic> doAssert = d =>
				{
					Assert.IsTrue(TypeHelper.IsTypeAssignableFrom<IEnumerable>(d));
					var casted = (IEnumerable<DynamicDocument>)d;
					Assert.AreEqual(2, casted.Count());
				};

			doAssert(asDynamic.Homes); //pluralized alias
			doAssert(asDynamic.homes); //pluralized alias
			doAssert(asDynamic.CustomDocuments); //pluralized alias			
			doAssert(asDynamic.customDocuments); //pluralized alias
		}

		[Test]
		public void GetPropertyValue_Non_Reflected()
		{
			var dynamicNode = GetDynamicNode(1174);
			var asDynamic = dynamicNode.AsDynamic();

			Assert.AreEqual("Custom data with same property name as the member name", asDynamic.GetPropertyValue("creatorName"));
			Assert.AreEqual("Custom data with same property name as the member name", asDynamic.GetPropertyValue("CreatorName"));
		}

		[Test]
		public void GetPropertyValue_Reflected()
		{
			var dynamicNode = GetDynamicNode(1174);
			var asDynamic = dynamicNode.AsDynamic();

			Assert.AreEqual("admin", asDynamic.GetPropertyValue("@creatorName"));
			Assert.AreEqual("admin", asDynamic.GetPropertyValue("@CreatorName"));
		}

		[Test]
		public void Get_User_Property_With_Same_Name_As_Member_Property()
		{
			var dynamicNode = GetDynamicNode(1174);
			var asDynamic = dynamicNode.AsDynamic();

			Assert.AreEqual("Custom data with same property name as the member name", asDynamic.creatorName);

			//because CreatorName is defined on DynamicNode, it will not return the user defined property
			Assert.AreEqual("admin", asDynamic.CreatorName);
		}

		[Test]
		public void Get_Member_Property()
		{
			var dynamicNode = GetDynamicNode(1173);
			var asDynamic = dynamicNode.AsDynamic();
			
			Assert.AreEqual(2, asDynamic.Level);
			Assert.AreEqual(2, asDynamic.level);

			Assert.AreEqual(1046, asDynamic.ParentId);
			Assert.AreEqual(1046, asDynamic.parentId);
		}

		[Test]
		public void Get_Children()
		{
			var dynamicNode = GetDynamicNode(1173);
			var asDynamic = dynamicNode.AsDynamic();

			var children = asDynamic.Children;
			Assert.IsTrue(TypeHelper.IsTypeAssignableFrom<IEnumerable>(children));

			var childrenAsList = asDynamic.ChildrenAsList; //test ChildrenAsList too
			Assert.IsTrue(TypeHelper.IsTypeAssignableFrom<IEnumerable>(childrenAsList));

			var castChildren = (IEnumerable<DynamicDocument>)children;
			Assert.AreEqual(4, castChildren.Count());

			var castChildrenAsList = (IEnumerable<DynamicDocument>)childrenAsList;
			Assert.AreEqual(4, castChildrenAsList.Count());
		}

		[Test]
		public void Ancestor_Or_Self()
		{
			var dynamicNode = GetDynamicNode(1173);
			var asDynamic = dynamicNode.AsDynamic();

			var result = asDynamic.AncestorOrSelf();

			Assert.IsNotNull(result);

			Assert.AreEqual(1046, result.Id);
		}

		[Test]
		public void Ancestors_Or_Self()
		{
			var dynamicNode = GetDynamicNode(1174);
			var asDynamic = dynamicNode.AsDynamic();

			var result = asDynamic.AncestorsOrSelf();

			Assert.IsNotNull(result);

			var list = (IEnumerable<DynamicDocument>)result;
			Assert.AreEqual(3, list.Count());
			Assert.IsTrue(list.Select(x => x.Id).ContainsAll(new[] { 1174, 1173, 1046 }));
		}

		[Test]
		public void Ancestors()
		{
			var dynamicNode = GetDynamicNode(1174);
			var asDynamic = dynamicNode.AsDynamic();

			var result = asDynamic.Ancestors();

			Assert.IsNotNull(result);

			var list = (IEnumerable<DynamicDocument>)result;
			Assert.AreEqual(2, list.Count());
			Assert.IsTrue(list.Select(x => x.Id).ContainsAll(new[] { 1173, 1046 }));
		}

		[Test]
		public void Descendants_Or_Self()
		{
			var dynamicNode = GetDynamicNode(1046);
			var asDynamic = dynamicNode.AsDynamic();

			var result = asDynamic.DescendantsOrSelf();

			Assert.IsNotNull(result);

			var list = (IEnumerable<DynamicDocument>)result;
			Assert.AreEqual(7, list.Count());
			Assert.IsTrue(list.Select(x => x.Id).ContainsAll(new[] { 1046, 1173, 1174, 1176, 1175 }));
		}

		[Test]
		public void Descendants()
		{
			var dynamicNode = GetDynamicNode(1046);
			var asDynamic = dynamicNode.AsDynamic();

			var result = asDynamic.Descendants();

			Assert.IsNotNull(result);

			var list = (IEnumerable<DynamicDocument>)result;
			Assert.AreEqual(6, list.Count());
			Assert.IsTrue(list.Select(x => x.Id).ContainsAll(new[] { 1173, 1174, 1176, 1175 }));
		}

		[Test]
		public void Up()
		{
			var dynamicNode = GetDynamicNode(1173);
			var asDynamic = dynamicNode.AsDynamic();

			var result = asDynamic.Up();

			Assert.IsNotNull(result);

			Assert.AreEqual(1046, result.Id);
		}

		[Test]
		public void Down()
		{
			var dynamicNode = GetDynamicNode(1173);
			var asDynamic = dynamicNode.AsDynamic();

			var result = asDynamic.Down();

			Assert.IsNotNull(result);

			Assert.AreEqual(1174, result.Id);
		}

		[Test]
		public void Next()
		{
			var dynamicNode = GetDynamicNode(1173);
			var asDynamic = dynamicNode.AsDynamic();

			var result = asDynamic.Next();

			Assert.IsNotNull(result);

			Assert.AreEqual(1175, result.Id);
		}

		[Test]
		public void Next_Without_Sibling()
		{
			var dynamicNode = GetDynamicNode(1178);
			var asDynamic = dynamicNode.AsDynamic();

			Assert.IsNull(asDynamic.Next());
		}

		[Test]
		public void Previous_Without_Sibling()
		{
			var dynamicNode = GetDynamicNode(1173);
			var asDynamic = dynamicNode.AsDynamic();

			Assert.IsNull(asDynamic.Previous());
		}

		[Test]
		public void Previous()
		{
			var dynamicNode = GetDynamicNode(1176);
			var asDynamic = dynamicNode.AsDynamic();

			var result = asDynamic.Previous();

			Assert.IsNotNull(result);

			Assert.AreEqual(1174, result.Id);
		}

		#region Classes used in test

		private class TestDynamicDocumentDataSource : IDynamicDocumentDataSource
		{
			public Guid GetDataType(string contentTypeAlias, string propertyTypeAlias)
			{
				//just return an empty Guid since we don't want to match anything currently.
				return Guid.Empty;
			}
		}

		#endregion
	}
}