using Umbraco.Core.ObjectResolution;

namespace Umbraco.Web.Routing
{

	/// <summary>
	/// Resolver for the last chance IPublishedContentFinder object.
	/// </summary>
	internal sealed class IPublishedContentLastChanceFinderResolver : SingleObjectResolverBase<IPublishedContentLastChanceFinderResolver, IPublishedContentFinder>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="IPublishedContentLastChanceFinderResolver"/> class with an instance of a finder.
		/// </summary>
		/// <param name="finder">A instance of a finder.</param>
		/// <remarks>The resolver is created by the <c>WebBootManager</c> and thus the constructor remains internal.</remarks>
		internal IPublishedContentLastChanceFinderResolver(IPublishedContentFinder finder)
			: base(finder)
		{ } 
	
		/// <summary>
		/// Sets the last chance finder.
		/// </summary>
		/// <param name="finder">The finder.</param>
		/// <remarks>For developers, at application startup.</remarks>
		public void SetFinder(IPublishedContentFinder finder)
		{
			Value = finder;
		}

		/// <summary>
		/// Gets the last chance finder.
		/// </summary>
		public IPublishedContentFinder Finder
		{
			get { return Value; }
		}

	}
}