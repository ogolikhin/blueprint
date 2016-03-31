agGrid.initialiseAgGridWithAngular1(angular);

var app = angular.module("example", ["agGrid", "dragDrop"]);

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
			}
		}
    ];

	function innerCellRenderer(params) {
        return params.data.name;
    }

    function rowClicked(params) {
        var node = params.node;
        var path = node.data.name + ' [' + node.data.id + ']';
        console.log(path);
		
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
			item.eRow.setAttribute("draggable", "true");
			item.eRow.setAttribute("ui-draggable", "true");
			item.eRow.setAttribute("drag", item.node.data.id);
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
			console.log($scope.gridOptions);

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
