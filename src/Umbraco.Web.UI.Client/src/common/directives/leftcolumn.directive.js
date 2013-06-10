﻿/**
* @ngdoc directive 
* @name umbraco.directive:leftColumn
* @restrict E
**/
function leftColumnDirective() {
    return {
        restrict: "E",    // restrict to an element
        replace: true,   // replace the html element with the template
        template: '<div ng-include="leftColumnViewFile"></div>',
        link: function (scope, el, attrs) {
            //set the loginViewFile
            scope.leftColumnViewFile = "views/directives/umb-leftcolumn.html";
        }
    };
}

angular.module('umbraco.directives').directive("umbLeftColumn", leftColumnDirective);
