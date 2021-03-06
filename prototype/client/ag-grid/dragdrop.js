(function(angular) {
  'use strict';

  var module = angular.module('dragDrop', []);

  module.directive('draggable', function() {
    return function(scope, element) {
      // this gives us the native JS object
      var el = element[0];

      el.draggable = true;

      el.addEventListener(
        'dragstart',
        function(e) {
          var elemId = this.id;
          if(!elemId) {
            elemId = "dnd" + parseInt(Math.random() * 10000).toString();
            var att = document.createAttribute("id");
            att.value = elemId;
            this.setAttributeNode(att);
          }
          e.dataTransfer.effectAllowed = 'move';
          e.dataTransfer.setData('Text', elemId);
          this.classList.add('drag');
          console.log(scope);
          return false;
        },
        false
      );

      el.addEventListener(
        'dragend',
        function(e) {
          this.classList.remove('drag');
          return false;
        },
        false
      );
    }
  });

  module.directive('droppable', function() {
    return {
      scope: {},
      link: function(scope, element) {
        // again we need the native object
        var el = element[0];

        el.addEventListener(
          'dragover',
          function(e) {
            e.dataTransfer.dropEffect = 'move';
            // allows us to drop
            if (e.preventDefault) e.preventDefault();
            this.classList.add('over');
            return false;
          },
          false
        );

        el.addEventListener(
          'dragenter',
          function(e) {
            this.classList.add('over');
            return false;
          },
          false
        );

        el.addEventListener(
          'dragleave',
          function(e) {
            this.classList.remove('over');
            return false;
          },
          false
        );

        el.addEventListener(
          'drop',
          function(e) {
            // Stops some browsers from redirecting.
            if (e.stopPropagation) e.stopPropagation();

            this.classList.remove('over');
            console.log(scope)
            var item = document.getElementById(e.dataTransfer.getData('Text'));
            this.appendChild(item);

            return false;
          },
          false
        );
      }
    }
  });

}(angular));
