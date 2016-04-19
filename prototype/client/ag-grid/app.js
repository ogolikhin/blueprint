agGrid.initialiseAgGridWithAngular1(angular);

var app = angular.module("example", ["ngAnimate", "ui.bootstrap", "agGrid", "dragDrop", "angular-perfect-scrollbar-2", "720kb.tooltips"]);

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
      groupExpanded: '<span class="myicon"><i class="fa fa-folder-open"/></span>',
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

// CAN'T USE AS IT CAUSES SOME ISSUES WITH DIRECTIVES INSIDE SCROLLABLE ELEMENTS
/*
app.directive('notOnTouchDevices', function($compile) {
  return {
    restrict: 'A',
    replace: false,
    //terminal: true,
    priority: 1002,
    link: function link(scope, element, attrs) {
      if(document.body.className.indexOf('is-touch') > -1 && attrs.notOnTouchDevices !== "") {
        element.removeAttr(attrs.notOnTouchDevices);
        element.removeAttr("not-on-touch-devices"); //remove the attribute to avoid indefinite loop
      }
      $compile(element)(scope);
    }
  };
});
*/
// CAN'T USE AS IT CAUSES SOME ISSUES WITH DIRECTIVES INSIDE SCROLLABLE ELEMENTS
/*
 app.directive('notOnMobile', function($compile) {
 // Perform detection.
 // This code will only run once for the entire application (if directive is present at least once).
 // Can be moved into the compile function if detection result needs to be passed as attribute.
 var onMobile = false;

 return {
 compile: function compile(tElement, tAttrs) {
 (function(a,b){if(/(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|mobile.+firefox|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows ce|xda|xiino|android|ipad|playbook|silk/i.test(a)||/1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw\-(n|u)|c55\/|capi|ccwa|cdm\-|cell|chtm|cldc|cmd\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\-s|devi|dica|dmob|do(c|p)o|ds(12|\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\-|_)|g1 u|g560|gene|gf\-5|g\-mo|go(\.w|od)|gr(ad|un)|haie|hcit|hd\-(m|p|t)|hei\-|hi(pt|ta)|hp( i|ip)|hs\-c|ht(c(\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\-(20|go|ma)|i230|iac( |\-|\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\/)|klon|kpt |kwc\-|kyo(c|k)|le(no|xi)|lg( g|\/(k|l|u)|50|54|\-[a-w])|libw|lynx|m1\-w|m3ga|m50\/|ma(te|ui|xo)|mc(01|21|ca)|m\-cr|me(rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\-2|po(ck|rt|se)|prox|psio|pt\-g|qa\-a|qc(07|12|21|32|60|\-[2-7]|i\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\-|oo|p\-)|sdk\/|se(c(\-|0|1)|47|mc|nd|ri)|sgh\-|shar|sie(\-|m)|sk\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\-|v\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\-|tdg\-|tel(i|m)|tim\-|t\-mo|to(pl|sh)|ts(70|m\-|m3|m5)|tx\-9|up(\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|yas\-|your|zeto|zte\-/i.test(a.substr(0,4))){onMobile=b}})(navigator.userAgent||navigator.vendor||window.opera,true);
 if (!onMobile) tElement.attr(tAttrs.notOnMobile, '');
 else {
 var containers = document.querySelectorAll(".scrollable");
 for(var c = 0; c < containers.length; c++) containers[c].style.overflowY = 'auto';
 }

 tElement.removeAttr('not-on-mobile');

 return function postLink(scope, element) {

 $compile(element)(scope);
 };
 }
 };
 });
 */

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
            accordionElement.querySelectorAll(".content-wrapper")[0].style.height = "calc(100% - " + accordionHeaderHeight + "px)";
          } else {
            accordionElement.style.height = "auto";
            accordionElement.querySelectorAll(".content-wrapper")[0].style.height = "0";
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

//CUSTOM DIRECTIVE FOR TOOLTIP
app.directive('bpTooltip', function() {
  function link(scope, element, attrs) {
    function hasClass(el, className) {
      if (el.classList)
        return el.classList.contains(className)
      else
        return !!el.className.match(new RegExp('(\\s|^)' + className + '(\\s|$)'))
    }

    function addClass(el, className) {
      if (el.classList)
        el.classList.add(className)
      else if (!hasClass(el, className)) el.className += " " + className
    }

    function removeClass(el, className) {
      if (el.classList)
        el.classList.remove(className)
      else if (hasClass(el, className)) {
        var reg = new RegExp('(\\s|^)' + className + '(\\s|$)')
        el.className=el.className.replace(reg, ' ')
      }
    }

    var realLink = function() {
      if(element && element.length) {
        var elem = element[0];
        elem.removeAttribute('bp-tooltip');
        if(scope.tooltipContent) {
          var tooltip = document.createElement('DIV');
          tooltip.className = 'bp-tooltip';

          var tooltipContent = document.createElement('DIV');
          tooltipContent.className = 'bp-tooltip-content';
          tooltipContent.innerHTML = scope.tooltipContent;

          tooltip.appendChild(tooltipContent);

          elem.className += ' bp-tooltip-trigger';
          elem.appendChild(tooltip);

          elem.addEventListener('mousemove', function fn(e) {
            var tooltip = elem.querySelectorAll('.bp-tooltip')[0];
            if(e.clientX > document.body.clientWidth / 2) {
              tooltip.style.left = '';
              tooltip.style.right = (document.body.clientWidth - e.clientX - 15) + 'px';
              removeClass(tooltip, 'bp-tooltip-left-tip');
              addClass(tooltip, 'bp-tooltip-right-tip');
            } else {
              tooltip.style.right = '';
              tooltip.style.left = (e.clientX - 8) + 'px';
              removeClass(tooltip, 'bp-tooltip-right-tip');
              addClass(tooltip, 'bp-tooltip-left-tip');
            }
            if(e.clientY > document.body.clientHeight / 2) {
              tooltip.style.top = '';
              tooltip.style.bottom = (document.body.clientHeight - (e.clientY - 15)) + 'px';
              removeClass(tooltip, 'bp-tooltip-top-tip');
              addClass(tooltip, 'bp-tooltip-bottom-tip');
            } else {
              tooltip.style.bottom = '';
              tooltip.style.top = e.clientY + 26 + 'px';
              removeClass(tooltip, 'bp-tooltip-bottom-tip');
              addClass(tooltip, 'bp-tooltip-top-tip');
            }
          });
        }
      }
    };

    realLink();
  }
  return {
    restrict: 'A',
    scope: {
      tooltipContent: "@"
    },
    link: link
  };
});
