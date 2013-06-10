angular.module('umbraco.mocks.resources')
.factory('mediaResource', function () {
    var mediaArray = [];
    return {
        rootMedia: function(){
          return [
            {src: "/media/boston.jpg", thumbnail: "/media/boston.jpg" },
            {src: "/media/bird.jpg", thumbnail: "/media/bird.jpg" },
            {src: "/media/frog.jpg", thumbnail: "/media/frog.jpg" },
            {src: "/media/pete.png", thumbnail: "/media/pete.png" }
          ];
      }
  };
});