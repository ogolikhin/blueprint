'use strict';

angular.module('pegah', [
  'ngRoute',
  'ngCookies',
  'ngResource',
  'ngSanitize',
  'ngAnimate',
  'kendo.directives'
])
  .config(function ($routeProvider, $locationProvider) {

    $routeProvider
      .otherwise({
        redirectTo: '/'
      });

    $locationProvider.html5Mode(true);

  })
  .controller("MyCtrl", function($scope){
          $scope.orientation = "horizontal";
          $scope.hello = "Hello from Controller!";
      });
