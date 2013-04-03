namespace Umbraco.Core.Cache
{

    /// <summary>
    /// Constants storing cache keys used in caching
    /// </summary>
    public static class CacheKeys
    {
        public const string ContentItemCacheKey = "contentItem";

        public const string MediaCacheKey = "UL_GetMedia";

        public const string MacroCacheKey = "UmbracoMacroCache";
        public const string MacroHtmlCacheKey = "macroHtml_";
        public const string MacroControlCacheKey = "macroControl_";
        public const string MacroHtmlDateAddedCacheKey = "macroHtml_DateAdded_";
        public const string MacroControlDateAddedCacheKey = "macroControl_DateAdded_";

        public const string MemberLibraryCacheKey = "UL_GetMember";
        public const string MemberBusinessLogicCacheKey = "MemberCacheItem_";

        public const string TemplateFrontEndCacheKey = "template";
        public const string TemplateBusinessLogicCacheKey = "UmbracoTemplateCache";

        public const string UserCacheKey = "UmbracoUser";

        public const string ContentTypeCacheKey = "UmbracoContentType";

        public const string ContentTypePropertiesCacheKey = "ContentType_PropertyTypes_Content:";
        
        public const string PropertyTypeCacheKey = "UmbracoPropertyTypeCache";

        public const string LanguageCacheKey = "UmbracoLanguageCache";

        public const string DomainCacheKey = "UmbracoDomainList";

        public const string StylesheetCacheKey = "UmbracoStylesheet";
        public const string StylesheetPropertyCacheKey = "UmbracoStylesheetProperty";

        public const string DataTypeCacheKey = "UmbracoDataTypeDefinition";

    }
}