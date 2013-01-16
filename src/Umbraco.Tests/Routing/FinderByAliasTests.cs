using NUnit.Framework;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web.Routing;

namespace Umbraco.Tests.Routing
{
	[TestFixture]
	public class FinderByAliasTests : BaseRoutingTest
	{
		public override void Initialize()
		{
			base.Initialize();
			Core.Configuration.UmbracoSettings.UseLegacyXmlSchema = false;
		}
        
        /// <summary>
		/// We don't need a db for this test, will run faster without one
		/// </summary>
		protected override bool RequiresDbSetup
		{
			get { return false; }
		}

		[TestCase("/this/is/my/alias", 1046)]
		[TestCase("/anotheralias", 1046)]
		[TestCase("/page2/alias", 1173)]
		[TestCase("/2ndpagealias", 1173)]
		[TestCase("/only/one/alias", 1174)]
		[TestCase("/ONLY/one/Alias", 1174)]
		public void Lookup_By_Url_Alias(string urlAsString, int nodeMatch)
		{
			var routingContext = GetRoutingContext(urlAsString);
			var url = routingContext.UmbracoContext.CleanedUmbracoUrl; //very important to use the cleaned up umbraco url
			var docRequest = new PublishedContentRequest(url, routingContext);
			var lookup = new FinderByAlias();
			
			var result = lookup.TryFindDocument(docRequest);

			Assert.IsTrue(result);
			Assert.AreEqual(docRequest.PublishedContentId, nodeMatch);
		}
	}
}