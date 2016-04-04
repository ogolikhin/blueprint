agGrid.initialiseAgGridWithAngular1(angular);

var app = angular.module("example", ["agGrid", "dragDrop"]);

//CUSTOM DIRECTIVE FOR FULL-HEIGHT ACCORDION
app.directive('bpAccordion', function() {
  function link(scope, element, attrs) {
    scope.redistributeHeight = function() {
      var accordionContainer = element[0];
      var hiddenRadioButtons = accordionContainer.querySelectorAll("input[type=radio].state");
      var numberOfAccordionElements = hiddenRadioButtons.length;
      var numberOfPinnedElements = accordionContainer.querySelectorAll("input[type=checkbox].pin:checked").length;
      var isCurrentElementAlsoPinned = accordionContainer.querySelectorAll("input[type=radio].state:checked ~ input[type=checkbox].pin:checked").length;
      var numberOfOpenElements = numberOfPinnedElements + (isCurrentElementAlsoPinned ? 0 : 1);
      var numberOfClosedElements = numberOfAccordionElements - numberOfOpenElements;

      var children = accordionContainer.childNodes;
      for(var i = 0; i < children.length; i++) {
        if(children[i].nodeType == 1 && children[i].tagName.toUpperCase() == "LI") {
          var accordionElement = children[i];
          if(accordionElement.querySelectorAll("input[type=radio].state:checked, input[type=checkbox].pin:checked").length) {
            var accordionHeaderHeight = parseInt(scope.headerHeight, 10);
            var compensationForClosedHeaders = accordionHeaderHeight * (numberOfClosedElements / numberOfOpenElements);
            accordionElement.style.height = "calc(" + (100 / numberOfOpenElements).toString() + "% - " + compensationForClosedHeaders.toString() + "px)";
            accordionElement.querySelectorAll(".content")[0].style.height = "calc(100% - " + accordionHeaderHeight + "px)";
          } else {
            accordionElement.style.height = "auto";
            accordionElement.querySelectorAll(".content")[0].style.height = "0";
          }
        }
      }
    };

    if(element && element.length) {
      var accordionContainer = element[0];
      if (accordionContainer.hasChildNodes()) {
        var children = accordionContainer.childNodes;
        for(var i = 0; i < children.length; i++) {
          if(children[i].nodeType == 1 && children[i].tagName.toUpperCase() == "LI") {
            var accordionControllers = children[i].querySelectorAll("input[type=radio].state, input[type=checkbox].pin");
            for(var j = 0; j < accordionControllers.length; j++) {
              accordionControllers[j].addEventListener("click", scope.redistributeHeight);
            }
          }
        }
        scope.redistributeHeight();
      }
    }
  }
  return {
    restrict: 'A',
    scope: {
      headerHeight: "@"
    },
    link: link
  };
});

app.controller("exampleCtrl", function($scope, $http) {
  var rowData = null;

  var columnDefs = [
    {
      field: "name",
      width: 300,
      cellRenderer: {
        renderer: 'group',
        innerRenderer: innerCellRenderer,
        suppressPadding: true,
        checkbox: true
      },
      cellClass: function(params) {
        /* DEMO OF STYLING/FORMATTING [start] */
        var myclass = "myclass";
        if(params.node.data.id == "10001" || params.node.data.id == "10002") myclass += " myhightlight";
        if (params.node.parent) {
          if(!params.node.data.username) {
            myclass += " mychild";
            if(Math.random() < 0.5) {
              myclass += " excel";
            } else {
              myclass += " word";
            }
          } else {
            myclass += " mychild2";
          }
        }
        return myclass;
        /* DEMO OF STYLING/FORMATTING [end] */
      }
    }
  ];

  function innerCellRenderer(params) {
    return params.data.name;
  }

  function rowClicked(params) {
    var node = params.node;
    var path = node.data.name + ' [' + node.data.id + ']';
    console.log("You selected:" + path);
    /* DEMO OF SUB-NODES LAZY-LOADING [start] */
    if(!node.parent || (node.data.lazyloaded && node.data.username)) {
      if(node.expanded) {
        if(node.allChildrenCount == 0) {
          $http.get(node.data.url)
            .then(function(res){
              var updated = false;

              function inject(myRows) {
                for (var i = 0; i < myRows.length; i++) {
                  if(myRows[i].children) inject(myRows[i].children);
                  if(node.data.id == myRows[i].id) {
                    for(var j = 0; j < res.data.length; j++) {
                      if(!res.data[j].name && res.data[j].title) res.data[j].name = res.data[j].title;
                      if(res.data[j].username) { // this works just for users
                        res.data[j].folder = true;
                        res.data[j].url = node.data.url + "/" + (res.data[j].id - 14000) + "/posts";
                        res.data[j].id += 14000; //just not to have conflicting ids
                      } else {
                        res.data[j].id += 15000;
                      }
                      res.data[j].lazyloaded = true;
                      res.data[j].children = [];
                    }
                    myRows[i].children = res.data;
                    myRows[i].open = true;
                    updated = true;
                  }
                }
              }

              inject(rowData);
              if(updated) $scope.gridOptions.api.setRowData(rowData);
            });
        }
      }
    }
    node.data.open = node.expanded;
    console.log(node);
    /* DEMO OF SUB-NODES LAZY-LOADING [end] */
  }

  $scope.gridOptions = {
    columnDefs: columnDefs,
    rowData: rowData,
    rowClass: 'myrow',
    rowSelection: 'multiple',
    enableColResize: true,
    enableSorting: true,
    groupSelectsChildren: true,
    headerHeight: 0,
    suppressRowClickSelection: true,
    suppressHorizontalScroll: true,
    rowBuffer: 200,
    rowHeight: 30,
    getNodeChildDetails: function(item) {
      if (item.folder) {
        return {
          group: true,
          children: item.children,
          expanded: item.open
        };
      } else {
        return null;
      }
    },
    processRowPostCreate: function(item) {
      // ADDING OF D&D DIRECTIVE
      item.eRow.setAttribute("draggable", "true");
      item.eRow.setAttribute("droppable", "true");
    },
    icons: {
      groupExpanded: '<span class="myicon"><i class="fa fa-folder-open"/i></span>',
      groupContracted: '<span class="myicon"><i class="fa fa-folder"/></span>'
    },
    onRowClicked: rowClicked,
    angularCompileRows: true,
    angularCompileHeaders: true,
    onGridReady: function(params) {
      params.api.setHeaderHeight(0);
      params.api.sizeColumnsToFit();

      $http.get("./data.json")
        .then(function(res){
          rowData = res.data;
          $scope.gridOptions.api.setRowData(rowData);
        });
    }
  };


  $scope.onDrop = function($data){
    console.log("You dragged: " + $data);
  };
});
