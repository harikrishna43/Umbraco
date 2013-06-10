/**
    * @ngdoc factory 
    * @name umbraco.resources.contentTypeResource
    * @description Loads in data for content types
    **/
function contentTypeResource($q, $http) {

    /** internal method to get the api url */
    function getChildContentTypesUrl(contentId) {
        return Umbraco.Sys.ServerVariables.contentTypeApiBaseUrl + "GetAllowedChildrenForContent?contentId=" + contentId;
    }

    return {

        //return a content type with a given ID
        getContentType: function (id) {

            return {
                name: "News Article",
                alias: "newsArticle",
                id: id,
                tabs: []
            };

        },
        //return all available types
        all: function () {
            return [];
        },

        //return children inheriting a given type
        children: function (id) {
            return [];
        },

        //return all content types a type inherits from
        parents: function (id) {
            return [];
        },

        //return all types allowed under given document
        getAllowedTypes: function (contentId) {

            var deferred = $q.defer();

            //go and get the tree data
            $http.get(getChildContentTypesUrl(contentId)).
                success(function (data, status, headers, config) {
                    deferred.resolve(data);
                }).
                error(function (data, status, headers, config) {
                    deferred.reject('Failed to retreive data for content id ' + contentId);
                });

            return deferred.promise;
        }

    };
}
angular.module('umbraco.resources').factory('contentTypeResource', contentTypeResource);