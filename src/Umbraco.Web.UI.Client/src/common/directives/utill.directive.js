/**
* @description Utillity directives for key and field events
**/
angular.module('umbraco.directives')

.directive('onKeyup', function () {
    return function (scope, elm, attrs) {
        elm.bind("keyup", function () {
            scope.$apply(attrs.onKeyup);
        });
    };
})

.directive('onKeyDown', function ($key) {
    return {
        link: function (scope, elm, attrs) {
            $key('keydown', scope, elm, attrs);
        }
    };
})

.directive('onBlur', function () {
    return function (scope, elm, attrs) {
        elm.bind("blur", function () {
            scope.$apply(attrs.onBlur);
        });
    };
})

.directive('onFocus', function () {
    return function (scope, elm, attrs) {
        elm.bind("focus", function () {
            scope.$apply(attrs.onFocus);
        });
    };
});