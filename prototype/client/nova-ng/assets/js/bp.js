/**
 * Router, Controller, Filter and Directives for Resource Explorer
 * @author pegah
 */
var bpApp = angular.module('bpApp',['ngCookies','ngRoute','ui.bootstrap','ngJScrollPane','agGrid'])
  .config(function($routeProvider, $locationProvider) {
    //$locationProvider.html5Mode(true);
    $routeProvider
      .when('/', {
        templateUrl: 'device.html',
        controller: 'DeviceController',
        reloadOnSearch:false    
      })
      .when('/Device/:deviceId/Package/:packageId/Search/:search', {
        templateUrl: 'device.html',
        controller: 'DeviceController',
        reloadOnSearch:false        
      })
      .when('/DevTool/:devtoolId/Package/:packageId/Search/:search', {
        templateUrl: 'device.html',
        controller: 'DeviceController',
        reloadOnSearch:false    
      })
      // .when('/Device/:deviceId/Package/:packageId/Search/:search/link', {
      //   templateUrl: 'device.html',
      //   controller: 'DeviceController',
      //   reloadOnSearch:false
      // })
      // .when('/DevTool/:devtoolId/Package/:packageId/Search/:search/link', {
      //   templateUrl: 'device.html',
      //   controller: 'DeviceController',
      //   reloadOnSearch:false
      // })
      .when('/Device/:deviceId/Search/:search', {
        templateUrl: 'device.html',
        controller: 'DeviceController',
        reloadOnSearch:false      
      })
      .when('/DevTool/:devtoolId/Search/:search', {
        templateUrl: 'device.html',
        controller: 'DeviceController',
        reloadOnSearch:false 
      })
      // .when('/Device/:deviceId/Search/:search/link', {
      //   templateUrl: 'device.html',
      //   controller: 'DeviceController',
      //   reloadOnSearch:false
      // })
      // .when('/DevTool/:devtoolId/Search/:search/link', {
      //   templateUrl: 'device.html',
      //   controller: 'DeviceController',
      //   reloadOnSearch:false
      // })
      .when('/Package/:packageId/Search/:search', {
        templateUrl: 'device.html',
        controller: 'DeviceController',
        reloadOnSearch:false        
      })
    // .when('/Package/:packageId/Search/:search/link', {
    //     templateUrl: 'device.html',
    //     controller: 'DeviceController',
    //     reloadOnSearch:false
    //   })
      .when('/Device/:deviceId/Package/:packageId', {
        templateUrl: 'device.html',
        controller: 'DeviceController',
        reloadOnSearch:false      
      })
      .when('/DevTool/:devtoolId/Package/:packageId', {
        templateUrl: 'device.html',
        controller: 'DeviceController',
        reloadOnSearch:false        
      })	  
      // .when('/Device/:deviceId/Package/:packageId/link', {
      //   templateUrl: 'device.html',
      //   controller: 'DeviceController',
      //   reloadOnSearch:false
      // })
      // .when('/DevTool/:devtoolId/Package/:packageId/link', {
      //   templateUrl: 'device.html',
      //   controller: 'DeviceController',
      //   reloadOnSearch:false
      // })
      .when('/Device/:deviceId', {
        templateUrl: 'device.html',
        controller: 'DeviceController',
        reloadOnSearch:false       
      })
      .when('/Search/:search', {
        templateUrl: 'device.html',
        controller: 'DeviceController',
        reloadOnSearch:false 
      })	  
      .when('/DevTool/:devtoolId', {
        templateUrl: 'device.html',
        controller: 'DeviceController',
        reloadOnSearch:false        
      })
	  .when('/Package/:packageId', {
        templateUrl: 'device.html',
        controller: 'DeviceController',
        reloadOnSearch:false 
      })
   //    .when('/Device/:deviceId/link', {
   //      templateUrl: 'device.html',
   //      controller: 'DeviceController',
   //    	reloadOnSearch: false
   //    })
   //    .when('/Search/:search/link', {
   //      templateUrl: 'device.html',
   //      controller: 'DeviceController',
   //    	reloadOnSearch: false
   //    })	  
   //    .when('/DevTool/:devtoolId/link', {
   //      templateUrl: 'device.html',
   //      controller: 'DeviceController',
   //    	reloadOnSearch: false
   //    })
	  // .when('/Package/:packageId/link', {
   //      templateUrl: 'device.html',
   //      controller: 'DeviceController',
   //    	reloadOnSearch: false
   //    })
   //    .when('/link', {
   //    	templateUrl: 'device.html',
   //    	controller: 'DeviceController',
   //    	reloadOnSearch: false
   //    })
      .when('/All', {
        templateUrl: 'device.html',
        controller: 'DeviceController',
        reloadOnSearch:false 
      }) 
      // .when('/All/link', {
      //   templateUrl: 'device.html',
      //   controller: 'DeviceController',
      // 	reloadOnSearch: false
      // })
      .when('/refresh', {
        templateUrl: 'refresh.html',
        controller: 'RefreshController',
        reloadOnSearch:false
      })
      .when('/ace', {
        templateUrl: 'ace.html',
        controller: 'AceController',
        reloadOnSearch:false       
      });
  });

  
bpApp.service('MetaService', function() {
   var title = '';
   var metaDescription = '';
   var metaKeywords = '';
   return {
		set: function(newTitle, newMetaDescription, newKeywords) {
			metaKeywords = newKeywords;
			metaDescription = newMetaDescription;
			title = newTitle; 
		},
		metaTitle: function() {return title},
		metaDescription: function() { return metaDescription; },
		metaKeywords: function() { return metaKeywords; }
   };
});

bpApp.service('HiLite', function() {
	var targetNode = null; //document.getElementById(id) || document.body;
	var hiliteTag = "EM"; //tag || "EM";
	var skipTags = new RegExp("^(?:" + hiliteTag + "|SCRIPT|FORM|SPAN)$");
	var colors = ["#ff6", "#a0ffff", "#9f9", "#f99", "#f6f"];
	var wordColor = [];
	var colorIdx = 0;
	var matchRegex = "";
	var openLeft = true;
	var openRight = true;
	return  {
		setMatchType: function(type) {
			switch (type) {
				case "left":
					this.openLeft = false;
					this.openRight = true;
					break;
				case "right":
					this.openLeft = true;
					this.openRight = false;
					break;
				case "open":
					this.openLeft = this.openRight = true;
					break;
				default:
					this.openLeft = this.openRight = false;
			}
		},
		setRegex: function(input) {
			input = input.replace(/^[^\w]+|[^\w]+$/g, "").replace(/[^\w'-]+/g, "|");
			var re = "(" + input + ")";
			if (!this.openLeft) re = "\\b" + re;
			if (!this.openRight) re = re + "\\b";
			matchRegex = new RegExp(re, "i");
		},
		getRegex: function() {
			var retval = matchRegex.toString();
			retval = retval.replace(/(^\/(\\b)?|\(|\)|(\\b)?\/i$)/g, "");
			retval = retval.replace(/\|/g, " ");
			return retval;
		},
		hiliteWords: function(node) {
			if (node === undefined || !node) return;
			this.targetNode = node;
			if (!matchRegex) return;
			if (skipTags.test(node.nodeName)) return;

			if (node.hasChildNodes()) {
			  for (var i=0; i < node.childNodes.length; i++)
				this.hiliteWords(node.childNodes[i]);
			}
			if (node.nodeType == 3) { // NODE_TEXT
			  if ((nv = node.nodeValue) && (regs = matchRegex.exec(nv))) {
				if(!wordColor[regs[0].toLowerCase()]) {
				  wordColor[regs[0].toLowerCase()] = colors[colorIdx++ % colors.length];
				}

				var match = document.createElement(hiliteTag);
				match.appendChild(document.createTextNode(regs[0]));
				match.style.backgroundColor = wordColor[regs[0].toLowerCase()];
				match.style.fontStyle = "inherit";
				match.style.color = "#000";

				var after = node.splitText(regs.index);
				after.nodeValue = after.nodeValue.substring(regs[0].length);
				node.parentNode.insertBefore(match, after);
			  }
			}
		},
		remove: function() {
			var arr = document.getElementsByTagName(hiliteTag);
			while( arr.length && (el = arr[0])) {
				var parent = el.parentNode;
				parent.replaceChild(el.firstChild, el);
				parent.normalize();
			}
		},
		apply: function(input) {
			this.remove();
			if (input === undefined || !input) return;
			this.setRegex(input);
			this.hiliteWords(targetNode);
		}		
	};
});

  
bpApp.factory('uuid2', [
	function() {
		function s4() {
			return Math.floor((1 + Math.random()) * 0x10000).toString(16).substring(1);
		}
		return {
			newuuid: function() {
				// http://www.ietf.org/rfc/rfc4122.txt
				var s = [];
				var hexDigits = "0123456789abcdef";
				for (var i = 0; i < 36; i++) {
					s[i] = hexDigits.substr(Math.floor(Math.random() * 0x10), 1);
				}
				s[14] = "4"; // bits 12-15 of the time_hi_and_version field to 0010
				s[19] = hexDigits.substr((s[19] & 0x3) | 0x8, 1); // bits 6-7 of the clock_seq_hi_and_reserved to 01
				s[8] = s[13] = s[18] = s[23] = "-";
				return s.join("");
			},
			newguid: function() {
				return s4() + s4() + '-' + s4() + '-' + s4() + '-' +
					s4() + '-' + s4() + s4() + s4();
			}
		}
	}]
);
  
/*  
bpApp.factory('socket', function ($rootScope) {
  var socket = io.connect(); //"https://tgdccscloud1.toro.design.ti.com:443/");
  return {
    on: function (eventName, callback) {
      socket.on(eventName, function () {  
        var args = arguments;
        $rootScope.$apply(function () {
          callback.apply(socket, args);
        });
      });
    },
    emit: function (eventName, data, callback) {
      socket.emit(eventName, data, function () {
        var args = arguments;
        $rootScope.$apply(function () {
          if (callback) {
            callback.apply(socket, args);
          }
        });
      })
    }
  };
});  

*/

bpApp.controller('RefreshController', function ($scope, $http) {
	$http({
		url : "/api/packages",
		method : "GET"
	}).success(function(data){
		$scope.packages = data;
		$("#package-selector").select2({
			placeholder: "Select a State"
		});
		$("#package-selector-filter").select2({
			placeholder: "Select a State"
		});
	});
	$scope.filteredData = new Array();
	var dataArray = new Array();
//	$scope.lastRefreshTime = "Never";
	$scope.infoFilter = true;
	$scope.warnFilter = true;
	$scope.errorFilter = true;
	$scope.refreshFilter = true;
	$scope.task2Filter = true;
	$scope.task3Filter = true;
	$scope.task4Filter = true;

	$scope.toggleFilter = function(filter){
		$scope[filter] = !$scope[filter];
		filterData();
	}
	$scope.toggleTask = function(filter) {
		$scope[filter] = !$scope[filter];
		console.log($scope.refreshFilter);
		console.log($scope.task2Filter);
		console.log($scope.task3Filter);
		console.log($scope.task4Filter);
	}

	function filterData() {
		console.log("Filtering Data");

		// Filters data based on criteria set in filterFn()
		//$scope.filteredData = dataArray.filter(filterFn);
		$scope.filteredData.splice(0,$scope.filteredData.length);
		dataArray.forEach(function(e) {
			if (filterFn(e.lastEventId)){
				$scope.filteredData.push(e.data);
			}
		});
	}

	function filterFn(level) {
		if (
			($scope.infoFilter && level == "info") ||
			($scope.warnFilter && level == "warn") ||
			($scope.errorFilter && level == "error")
		) {
			return true;
		}
		return false;
	}

	$scope.doRefresh = function(){

		$scope.status = "Refreshing All Packages. Please wait...";
		console.log("Refreshing..");
		var packages = $("#package-selector").select2("val");
		var tasks = [$scope.refreshFilter,$scope.task2Filter,$scope.task3Filter,$scope.task4Filter];
		if (packages == '') {
			$scope.status = "You must select at least 1 package";
		} else if (tasks == [false,false,false,false]) {
			$scope.status = "You must select at least 1 task";
		} else {
			var url = '/api/admin?packages=' + packages + '&tasks=' + tasks;
			console.log(url);
			//packages.forEach(function (package) {
			//	url +=
			//});
			dataArray.splice(0,dataArray.length);
			$scope.filteredData.splice(0,$scope.filteredData.length);
			var source = new EventSource(url);
			source.onmessage = function(e) {
				dataArray.push(e);
				if (filterFn(e.lastEventId)) {
						setTimeout(function() {
						$scope.filteredData.push(e.data);
						$scope.$apply();
					},100);
				}
				if (e.lastEventId === "end") {
					source.close();
					var date = new Date();
					console.log("Closing Source");
					$scope.status = e.data;
					$scope.lastRefreshTime = date.toString();
					$scope.$apply();

				}
			};
			source.onerror = function(e) {
				console.log("Error opening connection!");
			};
		}

	}
});
bpApp.controller('AceController', function($scope, $http, $location, $cookies, $routeParams, $cookieStore) {  
	var link =  $scope.selectedTreeNode.parentContent.link;
	$http({
		url : link,
		method : "GET"// ,
	}).success(function(data, status, headers, config) {
		$scope.aceContent = data;
//		$scope.fileName = $cookieStore.get('aceLink');
	}).error(function(data, status, headers, config) {
		$scope.status = status;
	});	
});

bpApp.controller('ScrollController', function($scope,$timeout) {
	$timeout(function() {
		if (typeof(scrollingContent) != "undefined" && scrollingContent != null) {
			scrollingContent.jScrollPane( { autoReinitialise: true })
			.parent(".jScrollPaneContainer").css({
				width:	'100%'
			,	height:	'100%'
			});
		}
		if (typeof(scrollingContent2) != "undefined" && scrollingContent2 != null) {
			scrollingContent2.jScrollPane( { autoReinitialise: true })
			.parent(".jScrollPaneContainer").css({
				width:	'100%'
			,	height:	'100%'
			});
		}
	}, 500);
	
	$scope.$watch('selectedTreeNode.parentContent', function() {
		scrollingContent.jScrollPane( { autoReinitialise: true })
		.parent(".jScrollPaneContainer").css({
			width:	'100%'
		,	height:	'100%'
		});
		scrollingContent2.jScrollPane( { autoReinitialise: true })
		.parent(".jScrollPaneContainer").css({
			width:	'100%'
		,	height:	'100%'
		});		
    });
			  
	$scope.$watch('selectedTreeNode.content', function() {
		scrollingContent.jScrollPane( { autoReinitialise: true })
		.parent(".jScrollPaneContainer").css({
			width:	'100%'
		,	height:	'100%'
		});
		scrollingContent2.jScrollPane( { autoReinitialise: true })
		.parent(".jScrollPaneContainer").css({
			width:	'100%'
		,	height:	'100%'
		});		
     });
});


bpApp.controller('CookieController', function($rootScope, $scope, $http, $location, $cookies) { //, socket) {
    $rootScope.ccsCloud = ''; //'https://tgdccscloud1.toro.design.ti.com'; 
	$rootScope.uid = null;
	$rootScope.importLater = null;

	$http({
		url: $rootScope.ccsCloud+'/api/queryUserStatus/', 
		method: 'GET'
	}).success(function(data, status, headers, config) {
		var res = angular.fromJson(data);
		$rootScope.uid = res.uid;
	}).error(function(data, status, headers, config) {
	});           

	/*
	if (typeof($cookies.TIPASSID) != "undefined") {
		var c = $cookies.TIPASSID.split('|');
		for (var i = 0; i < c.length; i++) {
			var v = c[i].split('=');
			if (v[0] === 'uid') {
				$scope.uid = v[1];
				break;
			}
		}
	}
	*/	

	socket.on('login', function (data) {
		$rootScope.uid = data.uid;
		$.fancybox.close();
		if ($rootScope.importLater != null) {
			$http({
				withCredentials : true,
				url: $rootScope.importLater,
				method: "GET"
			}).success(function(data, status, headers, config) {
				//console.log('import success');
				//$scope.overview = angular.fromJson(data);	    
				$rootScope.importLater = null;
			}).error(function(data, status, headers, config) {
				//$scope.status = status;
				//console.log('import failed');
				$rootScope.importLater = null;
			});
		}
	});

	socket.on('logout', function (message) {
		$rootScope.uid = null;
	});	
	
	$scope.logout = function() {
		$http({
			url: $rootScope.ccsCloud+"/logout", 
			method: "GET"//,
		}).success(function(data, status, headers, config) {
		}).error(function(data, status, headers, config) {
		});	
	}
	
});


/**
 * To populate the navigation dropdown menu for "wares"
 *
bpApp.controller('RexController', function($scope, $http, $location, $cookies) {  
  $http({
	    url: "api/devices/families", 
	    method: "GET"//,
	}).success(function(data, status, headers, config) {
	    var res = angular.fromJson(data);
	    $scope.tree = res; 
	}).error(function(data, status, headers, config) {
	    $scope.status = status;
	});
	
  
});
*/

/**
 * Controller to populate tiles section and its paging  
 */
bpApp.controller('DeviceController', function($scope, $rootScope, $timeout, $http, $location, $cookies,  $cookieStore, $routeParams, MetaService, HiLite) {    
    //$scope.currentPage = 0;
    //$scope.pageSize = 4;
    //$scope.numberOfPages = 1;

    //$scope.ware = $routeParams['deviceId'];
    //$scope.section = $routeParams['sectionId'];
    //$scope.familyId = $routeParams['familyId'];
    //$scope.variantId = $routeParams['variantId'];
	$scope.deviceId = $routeParams['deviceId'];
	$scope.devtoolId = $routeParams['devtoolId'];
	$scope.search = $routeParams['search'];
	$scope.packageId = $routeParams['packageId'];
	
	$rootScope.metaservice = MetaService;
	
	$scope.title = "Blueprint";
	$scope.keywords = "Requirements, UseCases";
	$scope.description = "Our requirements management software helps to de-risk and accelerate enterprise projects so that they are completed on time, and on budget.";
	
	if (typeof($scope.deviceId) != "undefined") {
		$scope.title = $scope.deviceId + " | " + $scope.title;
		$scope.keywords = $scope.deviceId + ", requirements";
		$scope.description = "Our requirements management software helps to de-risk and accelerate enterprise projects so that they are completed on time, and on budget. for " + $scope.deviceId;
	}
	if (typeof($scope.devtoolId) != "undefined") {
		$scope.title = $scope.devtoolId + " | " + $scope.title;
		$scope.keywords = $scope.devtoolId + ", requirements";
		$scope.description = "Our requirements management software helps to de-risk and accelerate enterprise projects so that they are completed on time, and on budget. for " + $scope.devtoolId;
	}
	
	$rootScope.metaservice.set($scope.title,$scope.description,$scope.keywords);

    
	//if ($location.path() === '/All') Remove 'Browse All Resources' and show all by default, OPS, 10/9/2014
		$scope.showTree = true;

    //if (typeof($scope.section) == "undefined")
    //	$scope.section = 'Overview';
    
	/*
    var tabs = new Array();
    tabs[$scope.section] = 'active';
    $scope.tabSelected = tabs;
    var tabLinks = "#/Device/"+$scope.ware;
    if (typeof($scope.familyId) != "undefined")
    	tabLinks = tabLinks + "/Family/"+$scope.familyId;
    if (typeof($scope.variantId) != "undefined")    
    	tabLinks = tabLinks + "/Variant/"+$scope.variantId;    
    $scope.tabLinks = tabLinks;
    */
	
	/*
    var tileSelected = new Array();
    
	$http({
	    url: "api/devices/families", 
	    method: "GET"//,
	}).success(function(data, status, headers, config) {
	    var res = angular.fromJson(data);
	    $scope.tree = res; 
	}).error(function(data, status, headers, config) {
	    $scope.status = status;
	});	
	
    $http({
	    url: "api/devices/" + $scope.ware + "/subfamilies",
	    method: "GET"
	}).success(function(data, status, headers, config) {
	    $scope.family = angular.fromJson(data);	    
	    $scope.numberOfPages = Math.ceil($scope.family.length/$scope.pageSize);	 
	    for(var i = 0; i < $scope.family.length; i++) {
	        if ($scope.family[i].name == $scope.familyId) {
	        	tileSelected[$scope.family[i].name] = 'tileSelected';
	        	$scope.currentPage = Math.floor(i/$scope.pageSize);
	        }
	    }	
	    $scope.tileSelected = tileSelected;
	    
	}).error(function(data, status, headers, config) {
	    $scope.status = status;
	});
	*/

	
	console.log($cookieStore.get());
	
	var ddUrl = "api/devicesanddevtools";
	if (typeof($scope.packageId) != "undefined")  {
		ddUrl += "?package="+$scope.packageId;
	}
	
	$http({
		url: ddUrl,
		method: "GET"
	}).success(function(data, status, headers, config) {
		$scope.deviceAndDevtool = angular.fromJson(data);
	}).error(function(data, status, headers, config) {
	    $scope.status = status;
	});
	
	
	$http({
		url: "api/packages",
		method: "GET"
	}).success(function(data, status, headers, config) {
		$scope.packages = angular.fromJson(data);
	}).error(function(data, status, headers, config) {
	    $scope.status = status;
	});	

	/*
	var theDevice = $scope.ware;    
    if (typeof($scope.section) == "undefined")
    	$scope.section = 'Overview';    
    if (typeof($scope.familyId) != "undefined") {
    	theDevice = $scope.familyId;
    }
    if (typeof($scope.variantId) != "undefined") {
    	theDevice = $scope.variantId;
    }
	*/

/*
	if (typeof($scope.deviceId) != "undefined")
		$scope.deviceDevToolsDropDownmodel = "<div class='select2-user-result'>&nbsp;&nbsp;&nbsp;" + $scope.deviceId + "</div>"; 
	else if (typeof($scope.devtoolId) != "undefined")
		$scope.deviceDevToolsDropDownmodel = $scope.devtoolId;
*/	
	
	$scope.browseAll = function() {
		var p = "/All";
		if (typeof($scope.packageId) != "undefined" && $scope.packageId !== "") {
			p = "/Package/"+$scope.packageId;							
		}					
		$location.url(p);	
	};

	
	$scope.allPackages = function() {
		var p = "";
		if (typeof($scope.deviceId) != "undefined" && $scope.deviceId !== "") {
			p += "/Device/"+$scope.deviceId;							
		}					
		if (typeof($scope.devtoolId) != "undefined" && $scope.devtoolId !== "") {
			p += "/DevTool/"+$scope.devtoolId;							
		}
		if (typeof($scope.search) != "undefined" && $scope.search !== "") {
			p = "/Search/"+$scope.search;							
		}
		$location.path(p);	
	};	
	
	var timer=false;	
	$scope.searchTree = function(keyEvent) {
		if ($scope.search !== "") {
			//HiLite.setRegex($scope.search);
			//HiLite.hiliteWords(document.getElementById("content"));
		}
		else {
			//HiLite.remove();
		}
		//angular.element('#jstree').jstree(true).search($scope.search);
		/*
		//angular.element('#jstree').jstree(true).search($scope.search); // probably need to comment this but would be real nice to use this instead...	
		//console.log($scope.search);
		$scope.selectedPath = {
				path: '',
				device: $routeParams['deviceId'],
				devtool: $routeParams['devtoolId'],
				search: $scope.search
			};    
		$cookieStore.put('pegah', $scope.selectedPath);		
		
		if (timer) {
			$timeout.cancel(timer);
		}
		timer = $timeout(function() {
				if ($scope.search.length > 2 || $scope.search.length==0 ) {
					angular.element('#jstree').jstree(true).refresh(-1);
				}
			}, 500);
		*/
		if (keyEvent.which === 13) {
			var redirectTo = '';
			if (typeof($scope.deviceId) != "undefined") {
				redirectTo += "/Device/"+$scope.deviceId;
			}
			if (typeof($scope.devtoolId) != "undefined") {
				redirectTo += "/DevTool/"+$scope.devtoolId;
			}
			if (typeof($scope.packageId) != "undefined") {
				redirectTo += "/Package/"+$scope.packageId;
			}			
			if (typeof($scope.search) != "undefined" && $scope.search !== "")
				redirectTo += "/Search/"+$scope.search;	

			//redirectTo += "/link?link=";
			redirectTo+="?link=";
			//$location.path(redirectTo);
			//use $location.url to remove link parameter
			$location.url(redirectTo);
		}		
	};

	$scope.reloadPage = function(href){
		window.location = href;
		window.location.reload();
	}

	$scope.clearFilter = function() {
		$scope.search = '';		
		var redirectTo = '';
		if (typeof($scope.deviceId) != "undefined") {
			redirectTo += "/Device/"+$scope.deviceId;
		}
		if (typeof($scope.devtoolId) != "undefined") {
			redirectTo += "/DevTool/"+$scope.devtoolId;
		}
		if (typeof($scope.packageId) != "undefined") {
			redirectTo += "/Package/"+$scope.packageId;
		}				
		$scope.selectedPath = {
				path: '',
				device: $routeParams['deviceId'],
				devtool: $routeParams['devtoolId'],
				packageId: $routeParams['packageId'],
				search: $scope.search
			};   
		//$cookieStore.put('pegah', $scope.selectedPath);		
		angular.element('#jstree').jstree(true).refresh(-1);				
		$location.path(redirectTo);

	};	
	
  $scope.deviceDevToolsDropDown = {
    query: function (query) {
      var data = {results: []};
		var myresults = new Array();

        myresults[0] = {text: 'Show All', id: '#/All'};

		var separator2 = {};
		separator2['text'] = 'Development Tools';
		myresults.push(separator2);		
		if ($scope.deviceAndDevtool != undefined) {
			populateDropdown();
		} else {
			$scope.$watch(function(scope) { return (scope.deviceAndDevtool === undefined) },
				function() {
					populateDropdown();
				 }
			);
		}
		function populateDropdown() {
			for(var i = 0; i < $scope.deviceAndDevtool.devtools.length; i++) {
			    if (typeof($scope.deviceAndDevtool.devtools[i].name) != "undefined") {
					if ($scope.deviceAndDevtool.devtools[i].name.toUpperCase().indexOf(query.term.toUpperCase()) > -1) {
						var obj = {};
						obj['text'] = $scope.deviceAndDevtool.devtools[i].name;
						obj['id'] = "#/DevTool/"+$scope.deviceAndDevtool.devtools[i].name;
						obj['image'] = $scope.deviceAndDevtool.devtools[i].image;
						if (typeof($scope.packageId) != "undefined" && $scope.packageId !== "") {
							obj['id'] += "/Package/"+$scope.packageId;
						}
						//if (typeof($scope.search) != "undefined" && $scope.search !== "") {
						//	obj['id'] += "/Search/"+$scope.search;
						//}
						//obj['id'] += "/link?link=";
						obj['id'] += "?link=";
						myresults.push(obj);
					}
				}
			}

			var separator1 = {};
			separator1['text'] = 'Devices';
			myresults.push(separator1);

		    for(var i = 0; i < $scope.deviceAndDevtool.devices.length; i++) {
			    if (typeof($scope.deviceAndDevtool.devices[i].name) != "undefined") {
					if ($scope.deviceAndDevtool.devices[i].name.toUpperCase().indexOf(query.term.toUpperCase()) > -1) {
						var obj = {};
						obj['text'] = $scope.deviceAndDevtool.devices[i].name;
						obj['id'] = "#/Device/"+$scope.deviceAndDevtool.devices[i].name;
						//obj['image'] = $scope.deviceAndDevtool.devices[i].image;
						if (typeof($scope.packageId) != "undefined" && $scope.packageId !== "") {
							obj['id'] += "/Package/"+$scope.packageId;
						}
						//if (typeof($scope.search) != "undefined" && $scope.search !== "") {
						//	obj['id'] += "/Search/"+$scope.search;
						//}
						// obj['id'] += "/link?link=";
						obj['id'] += "?link=";
						myresults.push(obj);
					}
				}
			}
			query.callback({results: myresults});
		}
	},
    formatResult: function (data, term) {
		if (!data.id) return "<div class='select2-user-result'><strong>" +data.text + "</strong></div>";
		if (data.text === 'Show All') return "<div class='select2-user-result'><strong>" +data.text + "</strong></div>";
		var image = "";
		if (typeof(data.image) != "undefined" && data.image) {
			image = "<img src='content/"+data.image+"' width='50' /> ";
		}
		else 
			image = "<img src='//www.ti.com/graphics/folders/partimages/MSP430F5259.jpg' width='50' /> "; //hack
	   return "<div class='select2-user-result'>&nbsp;&nbsp;&nbsp;" + image +data.text + "</div>";
    },
    formatSelection: function (data) {
		if (!data.id) return "<div class='select2-user-result'><strong>" +data.text + "</strong></div>";
		//$scope.ware = data.text;
		$scope.deviceObj = data;
		if (data.text === 'Show All') return "<div class='select2-user-result'><strong>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" +data.text + "</strong></div>";
		var image = "";
		if (typeof(data.image) != "undefined" && data.image) {
			image = "<img src='content/"+data.image+"' height='15' /> ";
			$cookieStore.put('select2Image', data.image);
		}
		else {
			image = "<img src='//www.ti.com/graphics/folders/partimages/MSP430F5259.jpg' height='15' /> "; //hack
			$cookieStore.put('select2Image', '');
		}

	   return "<div class='select2-user-result'>&nbsp;&nbsp;&nbsp;" + image +data.text + "</div>";
    },
    initSelection : function (element, callback) {
		callback($scope.deviceObj);
    },	
	escapeMarkup: function (m) { return m; }
  };	
  
  
  $scope.additionalFiltersDropDown = {
    query: function (query) {
      var data = {results: []};
		var myresults = new Array();

		var d_d = "#";
		if (typeof($scope.deviceId) != "undefined" && $scope.deviceId !== "") {
			d_d += "/Device/"+$scope.deviceId;							
		}					
		if (typeof($scope.devtoolId) != "undefined" && $scope.devtoolId !== "") {
			d_d += "/DevTool/"+$scope.devtoolId;							
		}
		var s = "";
		if (typeof($scope.search) != "undefined" && $scope.search !== "") {
			s = "/Search/"+$scope.search;							
		}									

		var separator2 = {};
		separator2['text'] = 'Device Families';
		myresults.push(separator2);
		myresults.push({text: 'All', id: d_d+s});		

	    
		for(var i = 0; i < $scope.packages.length; i++) {
			if ($scope.packages[i].name.toUpperCase().indexOf(query.term.toUpperCase()) > -1) {
				var obj = {};
				obj['text'] = $scope.packages[i].name;
				obj['id'] = d_d + "/Package/"+$scope.packages[i].name + s;
				// obj['id'] += "/link?link=";
				obj['id'] += "?link=";
				myresults.push(obj);
			}
	    }		
		
      query.callback({results: myresults});
    },
    formatResult: function (data, term) {
		if (!data.id) return "<div class='select2-user-result'><strong>" +data.text + "</strong></div>";
		if (data.text === 'Show All') return "<div class='select2-user-result'><strong>" +data.text + "</strong></div>";
		var image = "";
		/*
		if (typeof(data.image) != "undefined" && data.image) {
			image = "<img src='content/"+data.image+"' width='50' /> ";
		}
		else 
			image = "<img src='//www.ti.com/graphics/folders/partimages/MSP430F5259.jpg' width='50' /> "; //hack
		*/
	   return "<div class='select2-user-result'>&nbsp;&nbsp;&nbsp;" + image +data.text + "</div>";
    },
    formatSelection: function (data) {
		if (!data.id) return "<div class='select2-user-result'><strong>" +data.text + "</strong></div>";
		//$scope.ware = data.text;
		$scope.deviceObj = data;
		if (data.text === 'Show All') return "<div class='select2-user-result'><strong>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" +data.text + "</strong></div>";
		var image = "";
		/*
		if (typeof(data.image) != "undefined" && data.image) {
			image = "<img src='content/"+data.image+"' height='15' /> ";
			$cookieStore.put('select2Image', data.image);
		}
		else {
			image = "<img src='//www.ti.com/graphics/folders/partimages/MSP430F5259.jpg' height='15' /> "; //hack
			$cookieStore.put('select2Image', '');
		}
		*/

	   return "<div class='select2-user-result'>&nbsp;&nbsp;&nbsp;" + image +data.text + "</div>";
    },
    initSelection : function (element, callback) {
		callback($scope.deviceObj);
    },	
	escapeMarkup: function (m) { return m; }
  };  
	
  /*
  $scope.deviceDropDown = {
    query: function (query) {
      var data = {results: []};
		var myresults = new Array();
	    for(var i = 0; i < $scope.tree.length; i++) {
		    if (typeof($scope.tree[i].name) != "undefined") {
				if ($scope.tree[i].name.toUpperCase().indexOf(query.term.toUpperCase()) > -1) {
					var obj = {};
					obj['text'] = $scope.tree[i].name;
					obj['id'] = "#/Device/"+$scope.tree[i].name;
					if (typeof($scope.search) != "undefined" && $scope.search !== "") {
						if (typeof($scope.ware) != "undefined") {
							obj['id'] = "#/Device/"+$scope.ware+"/Search/"+$scope.search;
						}
						else {
							obj['id'] = "#/Search/"+$scope.search;							
						}
					}					
					myresults.push(obj);
				}
			}
	    }	  
      query.callback({results: myresults});
    },
    formatResult: function (data, term) {
       return "<div class='select2-user-result'>" + data.text + "</div>";
    },
    formatSelection: function (data) {
		$scope.ware = data.text;
		$scope.deviceObj = data;
        return "<div class='select2-user-result'>" + data.text + "</div>";
    },
    initSelection : function (element, callback) {
		callback($scope.deviceObj);
    },	
	escapeMarkup: function (m) { return m; }
  };
  */
	
  /*
  $scope.subFamilyDropDown = {
    query: function (query) {
      var data = {results: []};
		var myresults = new Array();
	    for(var i = 0; i < $scope.family.length; i++) {
		    if (typeof($scope.family[i].name) != "undefined") {
				if ($scope.family[i].name.toUpperCase().indexOf(query.term.toUpperCase()) > -1) {
					var obj = {};
					obj['text'] = $scope.family[i].name;
					obj['image'] = $scope.family[i].image;
					obj['id'] = "#/Device/"+$scope.ware+"/Family/"+$scope.family[i].name;
					if (typeof($scope.search) != "undefined" && $scope.search !== "") {
						obj['id'] = "#/Device/"+$scope.ware+"/Family/"+$scope.family[i].name+"/Search/"+$scope.search;
					}					
					myresults.push(obj);
				}
			}
	    }	  
      query.callback({results: myresults});
    },
    formatResult: function (data, term) {
       return "<div class='select2-user-result'>" + "<img src='content/"+data.image+"' width='50' /> "+data.text + "</div>";
    },
    formatSelection: function (data) {
		$scope.familyId = data.text;
		$scope.familyObj = data;
        return "<div class='select2-user-result'>" + "<img src='content/"+data.image+"' width='18' /> "+data.text + "</div>";
    },
    initSelection : function (element, callback) {
		var id = $(element).val();
		if (id !== "") {
			callback({id:$scope.familyObj.id, text:$scope.familyObj.text, image:$scope.familyObj.image});
		}
    },	
	escapeMarkup: function (m) { return m; }
  }	
  */

	
});

bpApp.controller('SearchController', function($scope, $http, $location, $cookies,  $cookieStore, $routeParams) { 

  $scope.searchKeywordDropDown = {
    query: function (query) {
		var myresults = new Array();
		var obj = {};
		obj['text'] = query.term;
		obj['id'] = "#";
		if (typeof($scope.deviceId) != "undefined") {
			obj['id'] += "/Device/"+$scope.deviceId;
		}
		if (typeof($scope.devtoolId) != "undefined") {
			obj['id'] += "/DevTool/"+$scope.devtoolId;
		}
		if (typeof($scope.packageId) != "undefined") {
			obj['id'] += "/Package/"+$scope.packageId;
		}		
		if (query.term !== "") {
			obj['id'] += "/Search/"+query.term;	
		}
		//obj['id'] += "/link?link=";		
		obj['id'] += "?link=";		
		myresults.push(obj);
        query.callback({results: myresults});
    },
    formatResult: function (data, term) {
       return "<div class='select2-user-result'>" + data.text + "</div>";
    },
    formatSelection: function (data) {
		$scope.searchId = data.text;
		$scope.searchObj = data;
        return "<div class='select2-user-result'>" + data.text + "</div>";
    },
	/*
    initSelection : function (element, callback) {
        callback($scope.variantObj);
    },
	*/
	escapeMarkup: function (m) { return m; }
  }	

});



/**
 * Controller to populate device variants' dropdown
 *
bpApp.controller('DeviceListBoxController', function($scope, $http, $location, $cookies, $routeParams) {

    $scope.ware = $routeParams['deviceId'];
    $scope.section = $routeParams['sectionId'];
    $scope.familyId = $routeParams['familyId'];
    $scope.variantId = $routeParams['variantId'];
	$scope.search = $routeParams['search'];
    
    
    if (typeof($scope.section) == "undefined")
    	$scope.section = 'Overview';    
    
    var optionSelected = new Array(); 
    $http({
	    url: "api/devices/" + $scope.familyId +"/variants",
	    method: "GET"
	}).success(function(data, status, headers, config) {
	    $scope.devices = angular.fromJson(data);
	    for(var i = 0; i < $scope.devices.length; i++) {
	        if ($scope.devices[i].name == $scope.variantId) {
	        	optionSelected[$scope.devices[i].name] = 'selected';
	        }
	    }	
	    $scope.optionSelected = optionSelected;
	    
	}).error(function(data, status, headers, config) {
	    $scope.status = status;
	});
	
	
  $scope.variantDropDown = {
    query: function (query) {
      var data = {results: []};
		var myresults = new Array();
	    for(var i = 0; i < $scope.devices.length; i++) {
		    if (typeof($scope.devices[i].name) != "undefined") {
				if ($scope.devices[i].name.toUpperCase().indexOf(query.term.toUpperCase()) > -1) {
					var obj = {};
					obj['text'] = $scope.devices[i].name;
					obj['image'] = $scope.devices[i].image;
					obj['id'] = "#/Device/"+$scope.ware+"/Family/"+$scope.familyId+"/Variant/"+$scope.devices[i].name;
					if (typeof($scope.search) != "undefined") {
						obj['id'] = "#/Device/"+$scope.ware+"/Family/"+$scope.familyId+"/Variant/"+$scope.devices[i].name+"/Search/"+$scope.search;
					}						
					myresults.push(obj);
				}
			}
	    }	  
      query.callback({results: myresults});
    },
    formatResult: function (data, term) {
       return "<div class='select2-user-result'>" + data.text + "</div>";
    },
    formatSelection: function (data) {
		$scope.variantId = data.text;
		$scope.variantObj = data;
        return "<div class='select2-user-result'>" + data.text + "</div>";
    },
    initSelection : function (element, callback) {
        callback($scope.variantObj);
    },
	escapeMarkup: function (m) { return m; }
  }		
    
});
*/

// <<< OPS
function openLinkInTab(uri, target) {
    if (target == null) {
        target = '_blank';
    }
    var link = angular.element('<a href="' + uri + '" target="' + target + '"></a>');
    angular.element(document.body).append(link);
    link[0].click();
    link.remove();
}
// >>>

/**
 * Controller to populate tab folder sections
 * (in progress)
 */
bpApp.controller('OverviewController', function($sce, $scope, $rootScope, $http, $location, $cookies, $cookieStore, $routeParams, $modal, $timeout, $interval, uuid2) {


    var columnDefs = [
        {headerName: "Make", field: "make"},
        {headerName: "Model", field: "model"},
        {headerName: "Price", field: "price"}
    ];

    var rowData = [
        {make: "Toyota", model: "Celica", price: 35000},
        {make: "Ford", model: "Mondeo", price: 32000},
        {make: "Porsche", model: "Boxter", price: 72000}
    ];

    $scope.gridOptions = {
        columnDefs: columnDefs,
        rowData: rowData
    };



    //$scope.ware = $routeParams['deviceId'];
    //$scope.section = $routeParams['sectionId'];    
    //$scope.familyId = $routeParams['familyId'];
    //$scope.variantId = $routeParams['variantId'];
    $scope.deviceId = $routeParams['deviceId'];
    $scope.devtoolId = $routeParams['devtoolId'];
	$scope.packageId = $routeParams['packageId'];
	
	//$scope.search = $routeParams['search'];
	//console.log($scope.search);
	$scope.selectedNode = {};
	$scope.selectedNode.waiting = false;
	$scope.selectedNode.show_progress = true;
	
	$scope.paneConfig = {
        scrollbarMargin:	15,
		scrollbarWidth:		15,
		arrowSize: 16,
		showArrows: false
    }
    			
    var thePath = '';
    //if (typeof($scope.ware) != "undefined")
    //	thePath = $scope.ware;
    
    var theDevice = $scope.deviceId;
	var theDevTool = $scope.devtoolId;
	var thePackage = $scope.packageId;
    
    //if (typeof($scope.section) == "undefined")
    //	$scope.section = 'Overview';
    
    //if (typeof($scope.familyId) != "undefined") {
    //	theDevice = $scope.familyId;
    //	thePath = thePath + "/" + $scope.familyId;
    //}
    //if (typeof($scope.variantId) != "undefined") {
    //	theDevice = $scope.variantId;
    //	thePath = thePath + "/" + $scope.variantId;
    //}
    
    $scope.selectedPath = {
            path: thePath,
            //section: $scope.section,
            device: theDevice,
			devtool: theDevTool,
			packageId: thePackage,
			search: $scope.search
        };    
    //$cookieStore.put('pegah', $scope.selectedPath);
			
	//var theUrl = "api/resources?maincategory="+$scope.section+"&device="+theDevice;
	//if (!$cookieStore.get('showTree')) {
	var theUrl = "api/resources?device="+theDevice;
	//}
	
    if (typeof(theDevice) != "undefined") {
		theUrl = "api/resources?device="+theDevice;
		if (typeof($scope.packageId) != "undefined") {
			theUrl += "&packageId=" + $scope.packageId;
		}		
		if (typeof($scope.search) != "undefined") {
			theUrl += "&search=" + $scope.search;
		}
	}
    else if (typeof(theDevTool) != "undefined") {
		theUrl = "api/resources?devtool="+theDevTool;
		if (typeof($scope.packageId) != "undefined") {
			theUrl += "&packageId=" + $scope.packageId;
		}		
		if (typeof($scope.search) != "undefined") {
			theUrl += "&search=" + $scope.search;
		}
	}	
	else if (typeof(thePackage) != "undefined") {
		theUrl = "api/resources?package="+thePackage;		
		if (typeof($scope.search) != "undefined") {
			theUrl += "&search=" + $scope.search;
		}
	}	
	else if (typeof($scope.search) != "undefined") {			
		theUrl = "api/resources?search=" + $scope.search;
	}
          
    $http({
	    //url: "api/resources?maincategory="+$scope.section+"&device="+theDevice, // /" + $scope.ware,
	    url: theUrl, // /" + $scope.ware,
	    method: "GET"
	}).success(function(data, status, headers, config) {
	    $scope.overview = angular.fromJson(data);
	}).error(function(data, status, headers, config) {
	    $scope.status = status;
	});	
	

 //    console.log($routeParams);
 //    $scope.linkPath = $routeParams['link'];
	// if (typeof($scope.linkPath) != undefined) {
	// 	var pathList = $scope.linkPath.split("/");
	// 	for (i in pathList) {
	// 		console.log($("a[title='"+pathList[i]+"']"));
	// 	}
	// }	
	
	/*
	if (typeof($cookies.TIPASSID) != "undefined") {
		var c = $cookies.TIPASSID.split('|');
		for (var i = 0; i < c.length; i++) {
			var v = c[i].split('=');
			if (v[0] === 'uid') {
				$scope.uid = v[1];
				break;
			}
		}
	}
	else
		$scope.uid = null;
	*/
/*	
	$scope.import = function(importLink) {
		$rootScope.importLater = null;
		var importL = importLink.replace(/\\/g,"/");
		if ($rootScope.uid) {
			$http({
				withCredentials : true,
				url: importL,
				method: "GET"
			}).success(function(data, status, headers, config) {
				console.log('import success');
				//$scope.overview = angular.fromJson(data);	    
			}).error(function(data, status, headers, config) {
				//$scope.status = status;
				console.log('import failed');
			});
		}
		else {
			$rootScope.importLater = importL;
		}
	};
*/


/*
  function openInNewTab() {
    var uri = 'http:ftware/whenType.pdf';
    var link = angular.element('<a href="' + uri + '" target="_blank"></a>');

    angular.element(document.body).append(link);

    link[0].click();
    link.remove();
  }
*/

     $scope.import = function(node) {
		var importLink = node.importProject;
		var list_of_devices = node.devicesVariants;
		var license = node.license;		
		var agreed = $cookieStore.get('agreed');
		
		if (typeof(license) != undefined && license != null && (typeof(agreed) == "undefined" || !agreed)) {
			$scope.licenseUrl = $sce.trustAsResourceUrl('content/'+license);
			var modalInstance1 = $modal.open({
				templateUrl: 'downloadLicense',
				windowClass: 'app-modal-window',				
				scope: $scope,
				controller: function ( $scope, $modalInstance ) {
					$scope.agree = function () {
							$cookieStore.put('agreed', true);
							agreed = true;
							$modalInstance.close(agreed);
						};
						$scope.disagree = function () {
							agreed = false;
							$modalInstance.close(agreed);
						};
					}
				});	
			  modalInstance1.result.then(function (agreed) {
				if (!agreed) return;
				  
				$rootScope.importLater = null;
				var importL = importLink.replace(/\\/g,"/");
				if( (typeof($scope.deviceId) == "undefined") && ( typeof(list_of_devices) != "undefined") ){
					$scope.Options = list_of_devices;
					var modalInstance = $modal.open({
						templateUrl: 'importVariantSelection',
						size: 'sm',
						scope: $scope,
						controller: function ( $scope, $modalInstance ) {
							$scope.selectedOption = $scope.Options[0];
							$scope.ok = function () {
								$modalInstance.close($scope.selectedOption);
							};
							$scope.cancel = function () {
								$modalInstance.dismiss('cancel');
							};
							$scope.getSelectedOptionClass = function ( option ) {
								//<!--return "listButton";-->
								return option;
							};
							$scope.optionClicked = function ( Option ) {
								$scope.selectedOption = Option;
							};
						}
					});
					modalInstance.result.then(function (selectedItem) {
						importL = importL.replace('{coreId}', selectedItem );
						openLinkInTab(importL, 'ccscloud');
					}, function () {
					});
				}	
				else {
					openLinkInTab(importL, 'ccscloud');
				}				  
				  
			});			
		} else {
			$rootScope.importLater = null;
			var importL = importLink.replace(/\\/g,"/");
			if( (typeof($scope.deviceId) == "undefined") && ( typeof(list_of_devices) != "undefined") ){
				$scope.Options = list_of_devices;
				var modalInstance = $modal.open({
					templateUrl: 'importVariantSelection',
					size: 'sm',
					scope: $scope,
					controller: function ( $scope, $modalInstance ) {
						$scope.selectedOption = $scope.Options[0];
						$scope.ok = function () {
							$modalInstance.close($scope.selectedOption);
						};
						$scope.cancel = function () {
							$modalInstance.dismiss('cancel');
						};
						$scope.getSelectedOptionClass = function ( option ) {
							//<!--return "listButton";-->
							return option;
						};
						$scope.optionClicked = function ( Option ) {
							$scope.selectedOption = Option;
						};
					}
				});

				modalInstance.result.then(function (selectedItem) {
					importL = importL.replace('{coreId}', selectedItem );
					openLinkInTab(importL, 'ccscloud');
				}, function () {
				});
			}	
			else {
				openLinkInTab(importL, 'ccscloud');
			}

		}		
    };
	
     $scope.importEnergia = function(importLink, createLink, list_of_devices) { // createLink is deprecated, OPS 8/14/15
        $rootScope.importLater = null;
        var importL = importLink.replace(/\\/g,"/");
		var unknownBoard = importLink.indexOf("{energiaBoardId}") > 0;
        if (unknownBoard && list_of_devices != null) {
            $scope.Options = list_of_devices;
            var modalInstance = $modal.open({
                templateUrl: 'importEnergiaBoardSelection',
                size: 'sm',
                scope: $scope,
                controller: function ( $scope, $modalInstance ) {
                    $scope.selectedOption = $scope.Options[0].id;
                    $scope.ok = function () {
                        $modalInstance.close($scope.selectedOption);
                    };
                    $scope.cancel = function () {
                        $modalInstance.dismiss('cancel');
                    };
                    $scope.getSelectedOptionClass = function ( option ) {
                        return option;
                    };
                    $scope.optionClicked = function ( Option ) {
                        $scope.selectedOption = Option;
                    };
                }
            });

            modalInstance.result.then(function (selectedItem) {
				importL = importL.replace('{energiaBoardId}', selectedItem );
                openLinkInTab(importL, 'ccscloud');
            }, function () {
            });
        }	
        else {
            openLinkInTab(importL, 'ccscloud');
        }		
    };	
	
    $scope.download = function(downloadLink) {
		$scope.selectedNode.show_progress = true;
		var agreed = $cookieStore.get('agreed');
		if (typeof(agreed) != "undefined" && agreed) {
			openLinkInTab(downloadLink, 'download');
			return;
		}
		
		var modalInstance = $modal.open({
			templateUrl: 'downloadLicense',
			windowClass: 'app-modal-window',				
			scope: $scope,
			controller: function ( $scope, $modalInstance ) {
				$scope.agree = function () {
					$cookieStore.put('agreed', true);
					openLinkInTab(downloadLink, 'download');
					$modalInstance.dismiss('cancel');
				};
				$scope.disagree = function () {
					$modalInstance.dismiss('cancel');
				};
			}
		});
    };	

	var timer = false;
	var timerTask = false;
    $scope.downloadFile = function(node) {
		$scope.selectedNode.show_progress = true;
		var downloadLink = node.downloadLink;
		var license = node.license;
		var showIt = true;
		var agreed = $cookieStore.get('agreed');
		if (typeof(license) != undefined && license != null && (typeof(agreed) == "undefined" || !agreed)) {
			$scope.licenseUrl = $sce.trustAsResourceUrl('content/'+license);
			var modalInstance = $modal.open({
				templateUrl: 'downloadLicense',
				windowClass: 'app-modal-window',				
				scope: $scope,
				controller: function ( $scope, $modalInstance ) {
					$scope.agree = function () {
							$cookieStore.put('agreed', true);
							agreed = true;
							$modalInstance.close(agreed);
						};
						$scope.disagree = function () {
							agreed = false;
							$modalInstance.close(agreed);
						};
					}
				});	
			  modalInstance.result.then(function (agreed) {
				  if (!agreed) return;
				  if (timerTask) {
				$interval.cancel(timerTask);
				timerTask = false;	
			}
			if (timer) {
				$timeout.cancel(timer);
				timer = false;				
			}
			$scope.id = uuid2.newuuid();
			$scope.progress = 0;
			
			timer = $timeout(function() {
				$scope.selectedNode.waiting = true;
				$scope.waitingMessage = "Preparing Download";
				$scope.canceled = false;
				timerTask = $interval(function() {
					$http({
						method:'GET', 
						url:"api/downloadprogress/"+$scope.id
					}).success(function(data, status, headers, config) { 
						var res = angular.fromJson(data);
						$scope.progress = res.progress;
					}).error(function(data, status, headers, config) {
						//ignore
					});
				}, 2000);
			}, 1000);
				
			$http({
				method:'GET', 
				url:downloadLink+"&progressId="+$scope.id
			}).success(function(data, status, headers, config) { 
				if (timerTask) {
					$interval.cancel(timerTask);
					timerTask = false;	
				}
				if (timer) {
					$timeout.cancel(timer);
					timer = false;				
				}			
				$scope.selectedNode.waiting = false;
				$scope.waitingMessage = "";
				if (!$scope.canceled) {
					var res = angular.fromJson(data);
					var element = document.createElement('a'); 
					element.href = res.link;
					element.setAttribute('download', 'true');
					element.setAttribute('target', '_self');
					document.body.appendChild(element);
					element.click();
				}
			}).error(function(data, status, headers, config) {
				if (timerTask) {
					$interval.cancel(timerTask);
					timerTask = false;	
				}
				if (timer) {
					$timeout.cancel(timer);
					timer = false;				
				}
				$scope.selectedNode.waiting = false;
				$scope.waitingMessage = "";
			});
				});				
		}
		else {			
			if (timerTask) {
				$interval.cancel(timerTask);
				timerTask = false;	
			}
			if (timer) {
				$timeout.cancel(timer);
				timer = false;				
			}
			$scope.id = uuid2.newuuid();
			$scope.progress = 0;
			
			timer = $timeout(function() {
				$scope.selectedNode.waiting = true;
				$scope.waitingMessage = "Preparing Download";
				$scope.canceled = false;
				timerTask = $interval(function() {
					$http({
						method:'GET', 
						url:"api/downloadprogress/"+$scope.id
					}).success(function(data, status, headers, config) { 
						var res = angular.fromJson(data);
						$scope.progress = res.progress;
					}).error(function(data, status, headers, config) {
						//ignore
					});
				}, 2000);
			}, 1000);
				
			$http({
				method:'GET', 
				url:downloadLink+"&progressId="+$scope.id
			}).success(function(data, status, headers, config) { 
				if (timerTask) {
					$interval.cancel(timerTask);
					timerTask = false;	
				}
				if (timer) {
					$timeout.cancel(timer);
					timer = false;				
				}			
				$scope.selectedNode.waiting = false;
				$scope.waitingMessage = "";
				if (!$scope.canceled) {
					var res = angular.fromJson(data);
					var element = document.createElement('a'); 
					element.href = res.link;
					element.setAttribute('download', 'true');
					element.setAttribute('target', '_self');
					document.body.appendChild(element);
					element.click();
				}
			}).error(function(data, status, headers, config) {
				if (timerTask) {
					$interval.cancel(timerTask);
					timerTask = false;	
				}
				if (timer) {
					$timeout.cancel(timer);
					timer = false;				
				}
				$scope.selectedNode.waiting = false;
				$scope.waitingMessage = "";
			});
		
		}
    };
	
	$scope.cancelDownload = function() {
		$scope.canceled = true;
		$scope.selectedNode.waiting = false;
		$scope.waitingMessage = "";	
		if (timerTask) {
			$interval.cancel(timerTask);
			timerTask = false;	
		}
		if (timer) {
			$timeout.cancel(timer);
			timer = false;				
		}		
	}
	
	$scope.goUp = function(node) {
		if (node == null) return;
		$scope.selectedNode.waiting = true;
		$scope.selectedTreeNode.showAce = false;
		$scope.selectedTreeNode.showFrame = false;	
		$scope.selectedTreeNode = node;		

		if (node.url.substring(0,1) === '/') {
			node.url = node.url.substring(1); 
		}
		var lastSlash = node.url.lastIndexOf('/');
		
		var parentUrl = node.url.substring(0,lastSlash);
		var parentNodeName = (node.url.substring(lastSlash+1)).replace(/%20/g, " ");	
		var lastAmp = parentNodeName.indexOf('&');
		if (lastAmp != -1) {
			parentNodeName = parentNodeName.substring(0,lastAmp);
		}
		
		$http({
			url:  node.url, 
			method: "GET"
		}).success(function(data, status, headers, config) {
			$scope.selectedTreeNode.content = angular.fromJson(data);
			if (parentUrl !== 'api') {
				$http({
					url: parentUrl,
					method: "GET"
				}).success(function(data1, status1, headers1, config1) {
					var parentNodes = angular.fromJson(data1);
					
					for(var i = 0; i < parentNodes.length; i++) {			
						if (parentNodes[i].text === parentNodeName) {
							$scope.selectedTreeNode.parentContent = parentNodes[i];
							break;
						}
					}				
					if ($scope.selectedTreeNode.parentContent.overviewLink != null) {                
						var iframeSrc = $scope.selectedTreeNode.parentContent.overviewLink;
						$scope.selectedTreeNode.weblink = $sce.trustAsResourceUrl(iframeSrc);
					}					
					$scope.selectedTreeNode.show = true;
					$scope.selectedNode.waiting = false;
				}).error(function(data1, status1, headers1, config1) {
					var lastEquals = parentNodeName.lastIndexOf('=');
					if (lastEquals >= 0)
						$scope.selectedTreeNode.parentContent.text = parentNodeName.substring(lastEquals+1);
					else 
						$scope.selectedTreeNode.parentContent.text = '/'
					$scope.selectedTreeNode.show = true;
					$scope.selectedNode.waiting = false;
				});
			}
			else {
				$scope.selectedTreeNode.show = true;
				$scope.selectedNode.waiting = false;
			}
			
		}).error(function(data, status, headers, config) {
			$scope.selectedTreeNode.show = true;
			$scope.selectedNode.waiting = false;
		});	
		
		if (angular.element("a[title='"+parentNodeName+"']").length == 1)
			angular.element("a[title='"+parentNodeName+"']").click().addClass('jstree-clicked');
	};

	$scope.hoverOnSlider = function(package) {
		$scope.theContent = package;
		$scope.showContent = true;
	}
	$scope.goToPackage = function(package) {
		$scope.selectedTreeNode.id = package.name;
	}


    $scope.openLink = function(currentNode, childNode, type) {
		$scope.selectedNode.waiting = true;
		$scope.selectedNode.show_progress = false;
		$scope.selectedTreeNode = currentNode;
		$scope.selectedTreeNode.parentContent = childNode;
		var span = currentNode.url.indexOf(' - (');
		if (span >0) {
			currentNode.url = currentNode.url.substring(0,span);
		}
		var pathI = $scope.selectedTreeNode.url.indexOf('path');
		if (pathI != -1) {
			var searchI = $scope.selectedTreeNode.url.indexOf('search');
			var packageI = $scope.selectedTreeNode.url.indexOf('package');
			
			var minIndex = Math.min(packageI, searchI);
			var maxIndex = Math.max(packageI, searchI);

			// no package or search, or they are both before path 
			if ((minIndex == -1 && maxIndex == -1) || (minIndex < pathI && maxIndex < pathI)) {	
				$scope.selectedTreeNode.url = currentNode.url +"/"+childNode.text ;
			// path is between package and search
			} else if (pathI > minIndex && pathI < maxIndex) {
				$scope.selectedTreeNode.url = currentNode.url.substring(0,pathI) + currentNode.url.substring(pathI,maxIndex-1) + "/"+childNode.text + "&" + currentNode.url.substring(maxIndex);
			// path before max Index, but the other parameter isn't there
			} else if (minIndex == -1 && maxIndex != -1) { 
				$scope.selectedTreeNode.url = currentNode.url.substring(0,maxIndex-1) +"/"+ childNode.text + "&" + currentNode.url.substring(maxIndex);
			} else {
				//both search and package are ahead of path
				$scope.selectedTreeNode.url = currentNode.url.substring(0,pathI)+currentNode.url.substring(pathI,minIndex-1) + "/" + childNode.text + "&" + currentNode.url.substring(minIndex);
			}
			
		}
		else {
			if ($scope.selectedTreeNode.url.indexOf('?') == -1)
				$scope.selectedTreeNode.url = $scope.selectedTreeNode.url + '?path=' + childNode.text;
			else
				$scope.selectedTreeNode.url = $scope.selectedTreeNode.url + '&path=' + childNode.text;
		}		
		$scope.selectedTreeNode.showAce = false;
		$scope.selectedTreeNode.showFrame = false;
				
		if ($scope.selectedTreeNode.parentContent != null) { // && scope.selectedNode.parentContent.resourceType=='file') {	
			$scope.title = "Blueprint";
			$scope.keywords = "Requirements";
			$scope.description = "Our requirements management software helps to de-risk and accelerate enterprise projects so that they are completed on time, and on budget. for ";
			if ($scope.selectedTreeNode.parentContent.text != undefined) {
				$scope.title = $scope.selectedTreeNode.parentContent.text + " | " + "Blueprint";
			}
			if ($scope.selectedTreeNode.parentContent.description != undefined) {
				$scope.description = $scope.selectedTreeNode.parentContent.description;
			}
			if ($scope.selectedTreeNode.parentContent.tags != undefined) {
				$scope.keywords = $scope.selectedTreeNode.parentContent.tags + ", requirements";
			}
			$scope.$root.metaservice.set($scope.title,$scope.description,$scope.keywords);
			if (childNode.type === 'folder') {                   	
				$http({
					url:  currentNode.url, //+"/"+childNode.text, // /" + $scope.ware,
					method: "GET"
				}).success(function(data, status, headers, config) {
					$scope.selectedTreeNode.content = angular.fromJson(data);
					$scope.selectedTreeNode.show = true;
					$scope.selectedNode.waiting = false;
					if (currentNode.parentContent.overviewLink != null) {         
						$scope.selectedTreeNode.showAce = false;
						$scope.selectedTreeNode.showFrame = true;			
						var iframeSrc = currentNode.parentContent.overviewLink;
						$scope.selectedTreeNode.weblink = $sce.trustAsResourceUrl(iframeSrc);
						$scope.selectedTreeNode.content = null;
						$scope.selectedTreeNode.show = true;
						$scope.selectedNode.waiting = false;
					}					
				}).error(function(data, status, headers, config) {
				});			
			}
            else if ($scope.selectedTreeNode.parentContent.resourceType ==='file' 
					|| $scope.selectedTreeNode.parentContent.resourceType == 'project.energia') {
                var link =  $scope.selectedTreeNode.parentContent.link;
                if ($scope.selectedTreeNode.parentContent.link.substr(-2) === '.c'
					|| $scope.selectedTreeNode.parentContent.link.substr(-4) === '.cpp'
					|| $scope.selectedTreeNode.parentContent.link.substr(-4) === '.asm'
					|| $scope.selectedTreeNode.parentContent.link.substr(-4) === '.cmd'
					|| $scope.selectedTreeNode.parentContent.link.substr(-4) === '.ino'
					|| $scope.selectedTreeNode.parentContent.link.substr(-2) === '.h') {
                    $scope.selectedTreeNode.showAce = true;
                    $scope.selectedTreeNode.showFrame = false;
                    $http({
                        url : link,
                        method : "GET"// ,
                    }).success(function(data, status, headers, config) {
                        $scope.selectedTreeNode.aceContent = data;
                        $scope.selectedTreeNode.content = null;
                        $scope.selectedTreeNode.show = true;
                        $scope.selectedNode.waiting = false;
                    }).error(function(data, status, headers, config) {
                        $scope.selectedTreeNode.content = null;
                        $scope.selectedTreeNode.show = true;
                        $scope.selectedNode.waiting = false;
					});
                }
                else {
                    $scope.selectedTreeNode.showAce = false;
                    $scope.selectedTreeNode.showFrame = true;
					if ($scope.selectedTreeNode.parentContent.link.substr(-4) === '.txt' 
						|| $scope.selectedTreeNode.parentContent.link.substr(-4) === '.pdf'
						|| $scope.selectedTreeNode.parentContent.link.substr(-4) === '.htm'
						|| $scope.selectedTreeNode.parentContent.link.substr(-5) === '.html') {
						var iframeSrc = $scope.selectedTreeNode.parentContent.link;
						$scope.selectedTreeNode.weblink = $sce.trustAsResourceUrl(iframeSrc);
					} 
					else {
						$scope.selectedTreeNode.showFrame = false;
					}
                    $scope.selectedTreeNode.content = null;
                    $scope.selectedTreeNode.show = true;
                    $scope.selectedNode.waiting = false;
                }				
            }
            else if ($scope.selectedTreeNode.parentContent.resourceType ==='web.app' || $scope.selectedTreeNode.parentContent.resourceType ==='folder') {
                var link =  $scope.selectedTreeNode.parentContent.link;
                $scope.selectedTreeNode.showAce = false;
                $scope.selectedTreeNode.showFrame = true;
                var iframeSrc = $scope.selectedTreeNode.parentContent.link;
                $scope.selectedTreeNode.weblink = $sce.trustAsResourceUrl(iframeSrc);
                $scope.selectedTreeNode.content = null;
                $scope.selectedTreeNode.show = true;
                $scope.selectedNode.waiting = false;
            }
			else if ($scope.selectedTreeNode.parentContent.type ==='weblink') {
				$scope.selectedTreeNode.showAce = false;
				$scope.selectedTreeNode.showFrame = true;			
				var iframeSrc = $scope.selectedTreeNode.parentContent.link;
				$scope.selectedTreeNode.weblink = $sce.trustAsResourceUrl(iframeSrc);
				$scope.selectedTreeNode.content = null;
				$scope.selectedTreeNode.show = true;
				$scope.selectedNode.waiting = false;
			}
			else if ($scope.selectedTreeNode.parentContent.overviewLink != null) {                
				$scope.selectedTreeNode.showAce = false;
				$scope.selectedTreeNode.showFrame = true;			
				var iframeSrc = $scope.selectedTreeNode.parentContent.overviewLink;
				$scope.selectedTreeNode.weblink = $sce.trustAsResourceUrl(iframeSrc);
				$scope.selectedTreeNode.content = null;
				$scope.selectedTreeNode.show = true;
				$scope.selectedNode.waiting = false;
			}
			
		}
		if (angular.element("a[title='"+childNode.text+"']").length == 1)
			angular.element("a[title='"+childNode.text+"']").click().addClass('jstree-clicked');

		var returned_url = currentNode.url;
		var pathI = returned_url.indexOf('path');	
	
		if (pathI != -1) {
			var packageI = returned_url.indexOf('package');
			var searchI = returned_url.indexOf('search');
			var minIndex = Math.min(packageI, searchI);
			var maxIndex = Math.max(packageI, searchI);

			// no package or search, or they are both before path 
			if ((minIndex == -1 && maxIndex == -1) || (minIndex < pathI && maxIndex < pathI)) {				
				returned_url = currentNode.url.substring(pathI+5);
			// path is between package and search	
			} else if (pathI > minIndex && pathI < maxIndex) {	
				returned_url = currentNode.url.substring(pathI+5,maxIndex);
			// path before max Index, but the other parameter isn't there
			} else if (minIndex == -1 && maxIndex != -1) { 
				returned_url = currentNode.url.substring(pathI+5,maxIndex);
			} else {
				//both search and package are ahead of path
				returned_url = currentNode.url.substring(pathI+5,minIndex);
			}

			var nodelink = "?link="+returned_url;
				//nodelink = currentNode.url.substring(pathIndex+5);
			if ($location.path().indexOf(nodelink) == -1) {

				if ($location.path().indexOf("/link") > -1) {
					$location.url($location.path() + nodelink);
					//$location.search("link",nodelink);
				}else {
					//var link = ($location.path()[$location.path().length -1] == "/")?"link":"/link";
					//$location.search("link",nodelink);
					$location.url($location.path()+nodelink);
					//$location.url($location.path()+link+nodelink);	
				}
			} 
		}
        nodeSelectionChanged($scope.selectedTreeNode);
	};	
	
    $scope.nodeChanged = function(newNode) {
        // do something when node changed
        //console.log(newNode.id);
        if (newNode.subid) {
            //console.log('  %s', newNode.subid);
        }	
    };    
	
	//angular.element('#here').select2('focus');
});


/**
 * Filter to un-escape html content in json
 */
bpApp.filter('to_trusted', ['$sce', function($sce){
    return function(text) {
		return $sce.trustAsHtml(text);
    };
}]);

bpApp.filter('to_trusted_resource', ['$sce', function($sce){
    return function(url) {
		return $sce.trustAsResourceUrl(url);
    };
}]);

/**
 * Filter to convert to uppercase
 */
bpApp.filter('dataFilter', function() {
    return function(input, uppercase) {
      input = input || '';
      var out = "";
      for (var i = 0; i < input.length; i++) {
        out = input.charAt(i) + out;
      }
      // conditional based on optional argument
      if (uppercase) {
        out = out.toUpperCase();
      }
      return out;
    };
  });
  
bpApp.directive('initializeSlider', function($timeout) {
	return{
		link: function(scope, element, attrs) {
			element.bind('load', function() {
				var packageImg = new Image();
				packageImg.src = attrs.src;
				var th = element.parent().height();
				var targetleft = Math.floor((element.parent().width() - Math.floor((packageImg.width * th) / packageImg.height)) / 2);
				var clonedSlide = document.getElementsByClassName(scope.package.image);
				$(clonedSlide).css({
					'height' : th,
					'left' : targetleft
				});
				scope.selectedTreeNode.showSlider = false;
				$timeout(function() {
					$('#package-images').slick({
						centerMode: true,
						arrows: true,
						slidesToScroll: 1,
						autoplay: true,
						autoplaySpeed: 2000,
						variableWidth: true,
						nextArrow: '.btn-prev',
						prevArrow: '.btn-next',
					}).on('init', function (){
						scope.selectedTreeNode.showSlider = true;
					});
				});
			});
		}
	}
});

/**
 * Directive to wrap fancybox jquery 
 */
bpApp.directive('fancybox',function($compile, $timeout){
    return {
        link: function($scope, element, attrs) {
            element.fancybox({
                hideOnOverlayClick:false,
                hideOnContentClick:false,
                enableEscapeButton:false,
                showNavArrows:false,
                onComplete: function(){
                    $timeout(function(){
                        $compile($("#fancybox-content"))($scope);
                        $scope.$apply();
                        $.fancybox.resize();
                    })
                }
            });
        }
    }
});



/**
 * jstree controller/directive (in progress)
 */
bpApp.directive('jstree', function($sce, $http, $location, $cookieStore, $timeout, $parse) {   
    return {
        restrict: 'A',
        require: '?ngModel',
        scope: {
        	selectedNode: '=?',
            selectedNodeChanged: '=',
			selectedPath: '=?'
        },
        link: function(scope, element, attrs) {
        	scope.selectedNode = scope.selectedNode || {};
            var treeElement = $(element);            
            var rootNodes = [];
            var selectedPath = scope.selectedPath;
            var tree = treeElement.jstree({
                'core' : {
					"animation" : 0,
					"worker" : false,
					'check_callback' : true,
                    'data' : {
                        'url' : function (node) {
							//selectedPath = $cookieStore.get('pegah'); 
							//console.log(selectedPath.search);
							var selectedPath = scope.selectedPath;
							//if (selectedPath == null) {
							//	selectedPath = $cookieStore.get('pegah');
							//}
							//console.log(selectedPath)
							var url = 'api/resources';
                            if ($location.path() === '/All' || $location.path() === '/') {
								rootNodes.push(node);
                                scope.selectedNode.showWelcome = true;
                            } else {
                                scope.selectedNode.showWelcome = false;
                            }
                            if (node.id === '#') {
								if (typeof(selectedPath.device) != "undefined") {
									url = 'api/resources?device='+selectedPath.device;
									if (typeof(selectedPath.packageId) != "undefined") {
										url += "&package=" + selectedPath.packageId;
									}									
									if (typeof(selectedPath.search) != "undefined") {
										url += "&search=" + selectedPath.search;
									}
								}
								else if (typeof(selectedPath.devtool) != "undefined") {
									url = 'api/resources?devtool='+selectedPath.devtool;								
									if (typeof(selectedPath.packageId) != "undefined") {
										url += "&package=" + selectedPath.packageId;
									}																		
									if (typeof(selectedPath.search) != "undefined") {
										url += "&search=" + selectedPath.search;
									}
								}
								else if (typeof(selectedPath.packageId) != "undefined") {
									url = 'api/resources?package='+selectedPath.packageId;								
									if (typeof(selectedPath.search) != "undefined") {
										url += "&search=" + selectedPath.search;
									}
								}								
								else if (typeof(selectedPath.search) != "undefined") {
									url = "api/resources?search=" + selectedPath.search;
								}
								
                            } else {
                                var path = ''; 
                                for (var i = node.parents.length - 2; i >= 0 ; i--) { 
                                    var nodInfo = $("#" + node.parents[i]);
                                    var node_name = nodInfo.children("a").text();
                                    var span = node_name.indexOf(' - (');
                                    if (span > 0) {
                                        node_name = node_name.substring(0,span);
                                    }
                                    if (node_name != "")
										path = path + node_name + '/';
                                }
                                var text = node.text;
                                var span = text.indexOf(' - (');
                                if (span > 0) {
                                    text = text.substring(0,span);
                                }
                                path = path + text;
								url = 'api/resources?path=' + path;
								if (typeof(selectedPath.device) != "undefined") {
									url = 'api/resources?device='+selectedPath.device;
									if (typeof(selectedPath.packageId) != "undefined") {
										url += "&package=" + selectedPath.packageId;
									}																		
									if (typeof(selectedPath.search) != "undefined") {
										url += "&search=" + selectedPath.search;
									}
									url += '&path=' + path;
								}
								else if (typeof(selectedPath.devtool) != "undefined") {
									url = 'api/resources?devtool='+selectedPath.devtool;
									if (typeof(selectedPath.packageId) != "undefined") {
										url += "&package=" + selectedPath.packageId;
									}																		
									if (typeof(selectedPath.search) != "undefined") {
										url += "&search=" + selectedPath.search;
									}
									url += '&path=' + path;
								}
								else if (typeof(selectedPath.packageId) != "undefined") {
									url = 'api/resources?package='+selectedPath.packageId;
									if (typeof(selectedPath.search) != "undefined") {
										url += "&search=" + selectedPath.search;
									}
									url += '&path=' + path;
								}								
								else {
									if (typeof(selectedPath.search) != "undefined") {
										url = "api/resources?search=" + selectedPath.search+'&path=' + path;
									}
									else {
										url = "api/resources?path=" + path;
									}
								}
                                
                            }
							//show the selected node content on right hand side
							if (typeof(scrollingContent) != "undefined" && scrollingContent != null) {
								scrollingContent.jScrollPane( { autoReinitialise: true })
								.parent(".jScrollPaneContainer").css({
									width:	'100%'
								,	height:	'100%'
								});
							}							
							if (typeof(scrollingContent2) != "undefined" && scrollingContent2 != null) {
								scrollingContent2.jScrollPane( { autoReinitialise: true })
								.parent(".jScrollPaneContainer").css({
									width:	'100%'
								,	height:	'100%'
								});
							}
							if (typeof(node.state) != "undefined" && typeof(node.state.selected) != "undefined" && node.state.selected) {
								scope.selectedNode.waiting = true;
								var n =  node;				
								n.parentUrl = node.original.url;
								if (n.parentUrl.substring(0,1) === '/') {
									n.parentUrl = n.parentUrl.substring(1); 
								}
								
								var nodeName = node.text;
								var span = nodeName.indexOf(' - (');
								if (span > 0) {
									nodeName = nodeName.substring(0,span);
								}	
								var pathI = n.parentUrl.indexOf('path');								
								if (pathI != -1) {
									var searchI = n.parentUrl.indexOf('search');
									var packageI = n.parentUrl.indexOf('package');
									var maxIndex = Math.max(searchI, packageI);
									if (maxIndex == -1 || pathI > maxIndex)
										n.url = n.parentUrl + '/' + nodeName;
									else {
										n.url = n.parentUrl.substring(0,maxIndex-1) +  '/' + nodeName + '&' + n.parentUrl.substring(maxIndex);
									}
								}
					
								scope.selectedNode.id = node.id;
								scope.selectedNode.url = n.url;
								scope.selectedNode.path = n.a_attr.path;
								scope.selectedNode.text = n.text;
								if (typeof( n.url) != "undefined" && n.url != null) {                    	                 	
									$http({
										url:  n.url, // /" + $scope.ware,
										method: "GET"
									}).success(function(data, status, headers, config) {
										scope.selectedNode.content = angular.fromJson(data);
										scope.selectedNode.show = true;	
										scope.selectedNode.waiting = false;
									}).error(function(data, status, headers, config) {
									});
								}
								else {
									scope.selectedNode.content = null;
									scope.selectedNode.show = false;
								}				
								if (typeof( n.parentUrl) != "undefined" && n.parentUrl != null && n.parentUrl !== 'api') {  
									scope.selectedNode.waiting = true;
									scope.selectedNode.showAce = false;
									scope.selectedNode.showFrame = false;						
									
									var span = n.parentUrl.indexOf(' - (');
									if (span >0) {
										n.parentUrl = n.parentUrl.substring(0,span);
									}                    	
									$http({
										url:  n.parentUrl, // /" + $scope.ware,
										method: "GET"
									}).success(function(data, status, headers, config) {
										var parentContent = angular.fromJson(data);
										scope.selectedNode.show = true;	
										var span = n.text.indexOf(' - (');
										var nodeName = n.text;
										if (span >0) {
											nodeName = nodeName.substring(0,span);
										}
										
										for(var i = 0; i < parentContent.length; i++) {
											if ((parentContent[i].text) === nodeName) {
												scope.selectedNode.parentContent = parentContent[i];
											}
										}
										if (scope.selectedNode.parentContent.overviewDescription != null && scope.selectedNode.parentContent.overviewDescription != 'undefined')  {
											scope.title = scope.selectedNode.parentContent.text + " | " + " Blueprint";
											scope.keywords = scope.selectedNode.parentContent.text+ ", blueprint, requirements";
											scope.description = scope.selectedNode.parentContent.text + " - " +  String(scope.selectedNode.parentContent.overviewDescription).replace(/<[^>]+>/gm, '') ;
											scope.$root.metaservice.set(scope.title,scope.description,scope.keywords);	
										}
	
										if (scope.selectedNode.parentContent != null) { // && scope.selectedNode.parentContent.resourceType=='file') {
                                            if (scope.selectedNode.parentContent.resourceType ==='file' ||
												scope.selectedNode.parentContent.resourceType == 'project.energia') {
                                                if (scope.selectedNode.parentContent.link.substr(-2) === '.c'
													|| scope.selectedNode.parentContent.link.substr(-4) === '.cpp'
													|| scope.selectedNode.parentContent.link.substr(-4) === '.asm'
													|| scope.selectedNode.parentContent.link.substr(-4) === '.cmd'
													|| scope.selectedNode.parentContent.link.substr(-4) === '.ino'
													|| scope.selectedNode.parentContent.link.substr(-2) === '.h') {
													scope.selectedNode.showAce = true;
                                                    scope.selectedNode.showFrame = false;
                                                    var link =  scope.selectedNode.parentContent.link;
                                                    $http({
                                                        url : link,
                                                        method : "GET"// ,
                                                    }).success(function(data, status, headers, config) {
                                                        scope.selectedNode.aceContent = data;
                                                        scope.selectedNode.waiting = false;
													}).error(function(data, status, headers, config) {
                                                        scope.selectedNode.waiting = false;
													});
                                                }
												else {
													scope.selectedNode.showAce = false;
													scope.selectedNode.showFrame = true;
													scope.selectedNode.show = false;									
													if (scope.selectedNode.parentContent.link.substr(-4) === '.txt' 
														|| scope.selectedNode.parentContent.link.substr(-4) === '.pdf'
														|| scope.selectedNode.parentContent.link.substr(-4) === '.htm'
														|| scope.selectedNode.parentContent.link.substr(-5) === '.html' ) {
														var iframeSrc = scope.selectedNode.parentContent.link;
														scope.selectedNode.weblink = $sce.trustAsResourceUrl(iframeSrc);
													}
													else {
														scope.selectedNode.showFrame = false;
													}
													scope.selectedNode.show = true;
													scope.selectedNode.waiting = false;
												}												
                                            }
                                            else if (scope.selectedNode.parentContent.resourceType ==='web.app' || scope.selectedNode.parentContent.resourceType ==='folder') {
                                                scope.selectedNode.showAce = false;
                                                scope.selectedNode.showFrame = true;
                                                var iframeSrc = scope.selectedNode.parentContent.link;
                                                scope.selectedNode.weblink = $sce.trustAsResourceUrl(iframeSrc);
                                                scope.selectedNode.waiting = false;
                                            }
											else if (scope.selectedNode.parentContent.type ==='weblink') {
												scope.selectedNode.showAce = false;
												scope.selectedNode.showFrame = true;
												var iframeSrc = scope.selectedNode.parentContent.link;
												scope.selectedNode.weblink = $sce.trustAsResourceUrl(iframeSrc);
												scope.selectedNode.waiting = false;
												
											}
											else if (scope.selectedNode.parentContent.overviewLink != null) {
												scope.selectedNode.showAce = false;
												scope.selectedNode.showFrame = true;
												var iframeSrc = scope.selectedNode.parentContent.overviewLink;
												scope.selectedNode.weblink = $sce.trustAsResourceUrl(iframeSrc);
												scope.selectedNode.waiting = false;
											}
											else {
												scope.selectedNode.showAce = false;
												scope.selectedNode.showFrame = false;
												scope.selectedNode.waiting = false;
											}
										}
									}).error(function(data, status, headers, config) {
									});
								}
								else {
									scope.selectedNode.parentContent = null;
									scope.selectedNode.waiting = false;
							}
							}

							return url;
                        },
						/*
						"ajax" : {
							"url" : "_search_data.json",
							"data" : function (n) {
								return { id : n.attr ? n.attr("id") : 0 };
							}
						}, */
						'dataFilter' : function (data) {
							var j = angular.fromJson(data);
							scope.selectedNode.emptyTree = false;
							if (j.length == 0) {
								scope.selectedNode.emptyTree = true;
							}
							var uripath = $location.search();
							if ("link" in uripath) {
								var paths = uripath['link'].split("/");
							} else {
								var paths = [];
							}
							//get rid of "" entry
							if (paths[paths.length -1] == "") {
								paths.pop();
							}

							var resourceTitle = paths.pop();
							var parentTitle = paths[paths.length-1];
							var count = 0;
							for (var i=0; i<j.length; i++) {

								j[i].a_attr =  { 'title' : j[i].text } ;
								//preceding directories...
								if (typeof j[i].state !== "undefined") {
									if ($.inArray(j[i].a_attr.title,paths) > -1) {
										if (j[i].state.opened) {

											//already open	
 										} else {
 											//in array, so open
											j[i].state.opened = true;
										}
									} 
								}	


								
								if (j[i].icon != null) {
									j[i].icon = 'content/'+(j[i].icon).replace(/\\/g,"/");
								}

								if (j[i].text === 'Devices') {
									j[i].type = 'devices';
								}
								else if (j[i].text === 'Libraries') {
									j[i].type = 'libraries';
								}	
								else if (j[i].text === 'Energia') {
									j[i].type = 'ino';
								}								
								else if (j[i].text === 'Development Tools') {
									j[i].type = 'kits';
								}								
								else if (j[i].type === 'weblink') {
									j[i].type = 'link';
									if ((j[i].link).lastIndexOf('.pdf') > 0) {
										j[i].type = 'pdf';
									}
								}
								else if (j[i].resourceType === 'file' || j[i].resourceType === 'project.energia') {
									j[i].type = 'file';
									if ((j[i].link).lastIndexOf('.pdf') > 0) {
										j[i].type = 'pdf';
									}
									else if ((j[i].link).lastIndexOf('.cmd') > 0) {
										j[i].type = 'cmd';
									}
									else if ((j[i].link).lastIndexOf('.zip') > 0) {
										j[i].type = 'zip';
									}
									else if ((j[i].link).lastIndexOf('.ino') > 0) {
										j[i].type = 'c';
									}
									else if (j[i].link.substr(-2) === '.c' || j[i].link.substr(-4) === '.cpp') {
										j[i].type = 'c';
										//j[i].a_attr = { 'title' : 'A C file that can be imported to Code Composer Studio cloud' };
									}
									else if ((j[i].link).lastIndexOf('.asm') > 0) {
										j[i].type = 'asm';
									}
									else if (j[i].link.substr(-2) === '.h') {
										j[i].type = 'h';
									}
									else if (j[i].link.substr(-4) === '.htm' || j[i].link.substr(-5) === '.html') {
										j[i].type = 'link';
									}
								}
								else if (j[i].resourceType === 'folder') {
									j[i].type = 'folder';
								}
                                else if (j[i].resourceType === 'file.executable') {
                                    j[i].type = 'exec';
									j[i].a_attr = { 'title' : j[i].text+' : A desktop application example that can be downloaded to your PC to run' } ;
                                }
                                else if (j[i].resourceType === 'web.app') {
                                    j[i].type = 'app';
									j[i].a_attr = { 'title' : j[i].text+' : A web based application that you can run directly in Resource Explorer' };
                                }
                                else if (j[i].resourceType === 'projectSpec' || j[i].resourceType === 'project.ccs' || j[i].resourceType === 'folder.importable') {
									j[i].type = 'ccs';
									j[i].a_attr = { 'title' : j[i].text+' : A C/C++ Project for Code Composer Studio' };
								}
								else if (j[i].resourceType === 'project.energia') {
									j[i].type = 'ino';
									j[i].a_attr = { 'title' : j[i].text+' : Energia Sketch' };
								}

								/*
								else if (j[i].numChildren > 0) {
									j[i].type = 'group';
								}
								*/

								if (j[i].numChildren != null && j[i].numChildren !== 0) {
									if (parentTitle === j[i].text) {
											//show the number for the children, however we don't change that they are returned from the server
											//they will be hidden by the filter below (removed from the server response)
											j[i].text = j[i].text+' - ('+j[i].numChildren+')';
									} else {
										if (j[i].text === 'C' || j[i].text === 'Assembly') { // || j[i].text === 'Energia') {
											//don't show these files
											j[i].children = false;
											j[i].text = j[i].text+' - ('+j[i].numChildren+')';
											j[i].numChildren = 0;
										}								
										else {
											//its okay to show these files
											j[i].text = j[i].text+' - ('+j[i].numChildren+')';
										}
				 								
									}
								}

								// some magic to automatically open links, more in select_node event binding
								// remove files from the response except the one that we need
								// check for package parameter in response, need to account for this to filter them out properly
								// hacky.....

								var returned_url = j[i].url;
								var pathI = j[i].url.indexOf('path');	
							
								if (pathI != -1) {
									var packageI = j[i].url.indexOf('package');
									var searchI = j[i].url.indexOf('search');
									var minIndex = Math.min(packageI, searchI);
									var maxIndex = Math.max(packageI, searchI);

									// no package or search, or they are both before path 
									if ((minIndex == -1 && maxIndex == -1) || (minIndex < pathI && maxIndex < pathI)) {				
										returned_url = j[i].url.substring(pathI+5);
									// path is between package and search	
									} else if (pathI > minIndex && pathI < maxIndex) {	
										returned_url = j[i].url.substring(pathI+5,maxIndex-1);
									// path before max Index, but the other parameter isn't there
									} else if (minIndex == -1 && maxIndex != -1) { 
										returned_url = j[i].url.substring(pathI+5,maxIndex-1);
									} else {
										//both search and package are ahead of path
										returned_url = j[i].url.substring(pathI+5,minIndex-1);
									}								}
								// if (j[i].url.indexOf("&") > -1) {
								// 	 = j[i].url.substring(0,j[i].url.indexOf("&"));
								// } else {
								// 	var returned_url = j[i].url
								// }

								returned_url = returned_url.split("/");
							
								if (returned_url[returned_url.length-1] == "C" || returned_url[returned_url.length -1] == "Assembly") {
									var res_text = j[i].text
									if (res_text != resourceTitle) {
										j.splice(i,1);
										i--;
										if (j.length == 1) break;
									}
								}

							}
							return angular.toJson(j);
						}
                    }
                }, 			
        		"search" : { 
					'search_callback': function(str, nodes) {
						var f = new $.vakata.search(str, true, { 
									caseSensitive : false, 
									fuzzy : false 
								}
							);
						if (f.search(nodes.text).isMatch) return true;

						//console.log(nodes);
						
						if (!nodes.original) return false;
						
						/* search description field */
						if (typeof(nodes.original.description) != "undefined") {
							if (f.search(nodes.original.description).isMatch) return true;
						}
						
						/* search tags array*/
						if (typeof(nodes.original.tags) != "undefined" && nodes.original.tags.length > 0) {
							for (i=0; i< nodes.original.tags.length; i++) {
								if (f.search(nodes.original.tags[i]).isMatch) return true;
							}
						}
						return false;
					},
					'fuzzy' : false /*,
					'show_only_matches' : true*/
				},     
        		"types" : {
        			"folder" : {
        				"icon" : "icns/folder.gif"
        			},
					"file" : {
						"icon" : "icns/file.gif"
					},
					"resource" : {
        				"icon" : "icns/file.gif"
        			},
					"zip" : {
						"icon" : "icns/zip.png"
					},
					"devices" : {
						"icon" : "icns/devices.png"
					},
					"libraries" : {
						"icon" : "icns/libraries.png"
					},
					"kits" : {
						"icon" : "icns/kits.png"
					},					
					"link" : {
						"icon" : "icns/link.png"
					},
					"group" : {
						"icon" : "icns/group.png"
					},
					"cmd": {
						"icon" : "icns/linker_command_file.gif"
					},
					"ino": {
						"icon" : "icns/new_sketch.gif"
					},
					"c" : {
						"icon" : "icns/c_file_obj.gif"
					},
					"h" : {
						"icon" : "icns/h_file_obj.gif"
					},
					"ccs" : {
						"icon" : "icns/ccs_proj.gif"
					},					
					"asm" : {
						"icon" : "icns/s_file_obj.gif"
					},					
        			"pdf" : {
        				"icon" : "icns/pdf.png"
        			},
                    "exec" : {
                        "icon" : "icns/exec.gif"
                    },
                    "app" : {
                        "icon" : "icns/demo.png"
                    }
                },
        		"plugins" : ["types","search", "wholerow"]
        	});            
            tree.bind('open_node.jstree', function(event, data) {
				if (typeof(scrollingContent) != "undefined" && scrollingContent != null) {
					scrollingContent.jScrollPane( { autoReinitialise: true })
					.parent(".jScrollPaneContainer").css({
						width:	'100%'
					,	height:	'100%'
					});
				}					
				if (typeof(scrollingContent2) != "undefined" && scrollingContent2 != null) {
					scrollingContent2.jScrollPane( { autoReinitialise: true })
					.parent(".jScrollPaneContainer").css({
						width:	'100%'
					,	height:	'100%'
					});
				}			
			});
            tree.bind('close_node.jstree', function(event, data) {
				if (typeof(scrollingContent) != "undefined" && scrollingContent != null) {
					scrollingContent.jScrollPane( { autoReinitialise: true })
					.parent(".jScrollPaneContainer").css({
						width:	'100%'
					,	height:	'100%'
					});
				}					
				if (typeof(scrollingContent2) != "undefined" && scrollingContent2 != null) {
					scrollingContent2.jScrollPane( { autoReinitialise: true })
					.parent(".jScrollPaneContainer").css({
						width:	'100%'
					,	height:	'100%'
					});
				}			
			});			
  
        	tree.bind('after_open.jstree', function(event,data) {
    //     		console.log("here");
    			var uripath = $location.search();
				if ("link" in uripath) {
					var paths = uripath['link'].split("/");
					//get rid of "" entry at end of array
					//caused by split with "/" at end of URI
					if (paths[paths.length -1] == "") {
						paths.pop();
					}
					var resourceTitle = paths.pop();
					var parentNode = paths.pop();

					var node_text_index = data.node.text.indexOf("- (");
					var node_title = "";
					if (node_text_index > -1) {
						node_title = data.node.text.substring(0,node_text_index).trim();
					} else {
						node_title = data.node.text;
					}


					if (node_title == parentNode) {
						var parentDOM = $('#'+data.node.id);
						// The title of an element and the text can be different
						// while we should pass the title that the element is given to the URL, it becomes problematic
						// because all example projects will have the title "A C/C++ Project for Code Composer Studio"
						// creating much longer url in most cases, so instead we pass the text returned from the server
						// However, selecting the node based on the text for "C" folders, is very impossible
						// because its one letter 
						if (resourceTitle == "C") { 
							var wantedResource = $("a[title='"+resourceTitle+"']");
						} else {
							var wantedResource = $("a:contains('"+resourceTitle+"')");
						}
						if (wantedResource.length > 1) {
							//wantedResource = wantedResource[wantedResource.length-1];
							for (i in wantedResource) {
							 	if ($.contains(parentDOM,wantedResource)) {
							 		wantedResource = wantedResource[i];
							 		break;
							 	}
							 }
						}

						var tree_id = $(wantedResource).attr("id");

						if (typeof tree_id !== "undefined") {
							var actual_id = tree_id.replace("_anchor", "");
							if ($.inArray(actual_id,data.node.children) > -1) {
								data.instance.select_node("#"+tree_id);	
							}
						}
					}
				}
				

        	});  
			tree.bind('select_node.jstree',function(event,data) {
				var pathIndex = data.node.original.url.indexOf("path=");
				var node_title = (typeof(data.node.original.name) !== "undefined") ? data.node.original.name:data.node.a_attr.title;
				var nodelink = "";
				if (pathIndex > -1) {
					nodelink = data.node.original.url.substring(pathIndex+5);
					var packageIndex = nodelink.indexOf("&");
					if ( packageIndex > -1) {
						nodelink = "?link="+nodelink.substring(0,packageIndex) + "/" + node_title
					} else {
						nodelink = "?link="+nodelink + "/"+ node_title; 
					}
				} else {
					nodelink = "?link="+node_title; 
				}

				//this is a super hacky way to do this, not entirely sure how to not do it this way however
				if (data.node.type == "asm" || data.node.type == "c" ) {
					//better yet hide, do not remove nodes unless their parent is C or Assembly
					// e.g. Energia files, or "Empty" Projects with a single C file in them
					
					if (data.instance.get_node(data.node.parent).text.indexOf("Assembly - (") > -1
							|| data.instance.get_node(data.node.parent).text.indexOf("C - (") > -1 ) {
							if ($location.url().indexOf(node_title) != -1) {
								data.instance.delete_node(data.node.id);
							}	
					}

				}				

				if ($location.path().indexOf(nodelink) == -1) {

					if ($location.path().indexOf("/link") > -1) {
						$location.url($location.path() + nodelink);
					}else {
						//var link = ($location.path()[$location.path().length -1] == "/")?"link":"/link";
						//$location.url($location.path()+link+nodelink);
						$location.url($location.path()+nodelink);
					}
				}

			});
            tree.bind('select_node.jstree', function(event, data) {

					$timeout(function() {
				if (typeof(scrollingContent) != "undefined" && scrollingContent != null) {
					scrollingContent.jScrollPane( { autoReinitialise: true })
					.parent(".jScrollPaneContainer").css({
						width:	'100%'
					,	height:	'100%'
					});
				}			
				if (typeof(scrollingContent2) != "undefined" && scrollingContent2 != null) {
					scrollingContent2.jScrollPane( { autoReinitialise: true })
					.parent(".jScrollPaneContainer").css({
						width:	'100%'
					,	height:	'100%'
					});
				}			
					},500);

				scope.selectedNode.waiting = true;
				var id = data.node.id;
				/* 
				if (id != undefined) {
					if ($("li[id=" + id + "]").hasClass("jstree-open"))
						treeElement.jstree("close_node", "#" + id);
					else
						treeElement.jstree("open_node", "#" + id);
				}
				*/
							
                var n =  data.node;				
				n.parentUrl = data.node.original.url;
				if (n.parentUrl.substring(0,1) === '/') {
					n.parentUrl = n.parentUrl.substring(1); 
				}
				
				var nodeName = data.node.text;
				var span = nodeName.indexOf(' - (');
				if (span > 0) {
					nodeName = nodeName.substring(0,span);
				}
				var pathI = n.parentUrl.indexOf('path');	
							
				if (pathI != -1) {
					var packageI = n.parentUrl.indexOf('package');
					var searchI = n.parentUrl.indexOf('search');
					var minIndex = Math.min(packageI, searchI);
					var maxIndex = Math.max(packageI, searchI);

					// no package or search, or they are both before path 
					if ((minIndex == -1 && maxIndex == -1) || (minIndex < pathI && maxIndex < pathI)) {				
						n.url = n.parentUrl + '/' + nodeName;
					// path is between package and search	
					} else if (pathI > minIndex && pathI < maxIndex) {	
						n.url = n.parentUrl.substring(0,maxIndex-1) + '/' + nodeName + '&' + n.parentUrl.substring(maxIndex);
					// path before max Index, but the other parameter isn't there
					} else if (minIndex == -1 && maxIndex != -1) { 
						n.url = n.parentUrl.substring(0,maxIndex-1) +  '/' + nodeName + '&' + n.parentUrl.substring(maxIndex);
					} else {
						//both search and package are ahead of path
						n.url = n.parentUrl.substring(0,minIndex-1) +  '/' + nodeName + '&' + n.parentUrl.substring(minIndex);
					}

				}
				else {
					if (n.parentUrl.indexOf('?') == -1)
						n.url = n.parentUrl + '?path=' + nodeName;
					else
						n.url = n.parentUrl + '&path=' + nodeName;
				}


				scope.selectedNode.id = n.id;
				scope.selectedNode.url = n.url;
				scope.selectedNode.path = n.a_attr.path;
				scope.selectedNode.text = n.text;

				
				if (typeof( n.url) != "undefined" && n.url != null) {             	                 	
					$http({
						url:  n.url, // /" + $scope.ware,
						method: "GET"
					}).success(function(data, status, headers, config) {
						scope.selectedNode.content = angular.fromJson(data);
						scope.selectedNode.show = true;	
						scope.selectedNode.waiting = false;
						nodeSelectionChanged(scope.selectedNode);
					}).error(function(data, status, headers, config) {
					});
				}
				else {
					scope.selectedNode.content = null;
					scope.selectedNode.show = false;
				}			
				if (typeof( n.parentUrl) != "undefined" && n.parentUrl != null && n.parentUrl !== 'api') {   
					scope.selectedNode.waiting = true;
					scope.selectedNode.showAce = false;
					scope.selectedNode.showFrame = false;	
                    scope.selectedNode.weblink = $sce.trustAsResourceUrl('about:blank');
					var span = n.parentUrl.indexOf(' - (');
					if (span >0) {
						n.parentUrl = n.parentUrl.substring(0,span);
					}                    	
					$http({
						url:  n.parentUrl, // /" + $scope.ware,
						method: "GET"
					}).success(function(data, status, headers, config) {
						var parentContent = angular.fromJson(data);
						scope.selectedNode.show = true;	
						var span = n.text.indexOf(' - (');
						var nodeName = n.text;
						if (span >0) {
							nodeName = nodeName.substring(0,span);
						}
						
						for(var i = 0; i < parentContent.length; i++) {
							if ((parentContent[i].text) === nodeName) {
								scope.selectedNode.parentContent = parentContent[i];
							}
						}
						if (scope.selectedNode.parentContent != null) { // && scope.selectedNode.parentContent.resourceType=='file') {
							scope.title = "Blueprint";
							scope.keywords = "Blueprint, requirements";
							scope.description = "Our requirements management software helps to de-risk and accelerate enterprise projects so that they are completed on time, and on budget.";
							if (scope.selectedNode.parentContent.text != undefined) {
								scope.title = scope.selectedNode.parentContent.text + " | " + "Blueprint";
							}
							if (scope.selectedNode.parentContent.description != undefined) {
								scope.description = scope.selectedNode.parentContent.description;
							}
							if (scope.selectedNode.parentContent.tags != undefined) {
								scope.keywords = scope.selectedNode.parentContent.tags + ", blueprint, requirements";
							}
							scope.$root.metaservice.set(scope.title,scope.description,scope.keywords);	
							if (scope.selectedNode.parentContent.resourceType ==='file' ||
								scope.selectedNode.parentContent.resourceType == 'project.energia') {
								if (scope.selectedNode.parentContent.link.substr(-2) === '.c'
									|| scope.selectedNode.parentContent.link.substr(-4) === '.cpp'
									|| scope.selectedNode.parentContent.link.substr(-4) === '.asm'
									|| scope.selectedNode.parentContent.link.substr(-4) === '.cmd'
									|| scope.selectedNode.parentContent.link.substr(-4) === '.ino'
									|| scope.selectedNode.parentContent.link.substr(-2) === '.h') {
									scope.selectedNode.showAce = true;
									scope.selectedNode.showFrame = false;								
									var link =  scope.selectedNode.parentContent.link;
									$http({
										url : link,
										method : "GET"// ,
									}).success(function(data, status, headers, config) {
										scope.selectedNode.aceContent = data;
										scope.selectedNode.waiting = false;
									}).error(function(data, status, headers, config) {
                                        scope.selectedNode.waiting = false;
									});									
								}
								else {
									scope.selectedNode.showAce = false;
									scope.selectedNode.showFrame = true;
									scope.selectedNode.show = false;									
									if (scope.selectedNode.parentContent.link.substr(-4) === '.txt'
										|| scope.selectedNode.parentContent.link.substr(-4) === '.pdf'
										|| scope.selectedNode.parentContent.link.substr(-4) === '.htm'
										|| scope.selectedNode.parentContent.link.substr(-5) === '.html' ) {
										var iframeSrc = scope.selectedNode.parentContent.link;
										scope.selectedNode.weblink = $sce.trustAsResourceUrl(iframeSrc);
									}
									else {
										scope.selectedNode.showFrame = false;
									}
									scope.selectedNode.show = true;
									scope.selectedNode.waiting = false;
								}
							}
                            else if (scope.selectedNode.parentContent.resourceType ==='web.app' || scope.selectedNode.parentContent.resourceType ==='folder') {
                                scope.selectedNode.showAce = false;
                                scope.selectedNode.showFrame = true;
                                var iframeSrc = scope.selectedNode.parentContent.link;
                                scope.selectedNode.weblink = $sce.trustAsResourceUrl(iframeSrc);
                                scope.selectedNode.waiting = false;
                            }
							else if (scope.selectedNode.parentContent.type ==='weblink') {
                                // <<< open weblinks in new tab/window until we solve the https issue, OPS, 10/9/2014
								/*
                                scope.selectedNode.showAce = false;
                                scope.selectedNode.showFrame = false;
                                scope.selectedNode.waiting = false;
                                openLinkInTab(scope.selectedNode.parentContent.link);
								*/
                                
                                scope.selectedNode.showAce = false;
								scope.selectedNode.showFrame = true;							
								var iframeSrc = scope.selectedNode.parentContent.link;
								//if (iframeSrc.indexOf('www-s') > 0)
								//	iframeSrc = 'api/resolve?source='+iframeSrc;
								//else if ($location.absUrl().indexOf('https') > -1)
								//	iframeSrc = iframeSrc.replace('http', 'https');
								scope.selectedNode.weblink = $sce.trustAsResourceUrl(iframeSrc);
								scope.selectedNode.waiting = false;
								
                                // >>>
							}
							else if (scope.selectedNode.parentContent.overviewLink != null) {
                                scope.selectedNode.showAce = false;
								scope.selectedNode.showFrame = true;							
								var iframeSrc = scope.selectedNode.parentContent.overviewLink;
								//if (iframeSrc.indexOf('www-s') > 0)
								//	iframeSrc = 'api/resolve?source='+iframeSrc;
								//else if ($location.absUrl().indexOf('https') > -1)
								//	iframeSrc = iframeSrc.replace('http', 'https');

								iframeSrc = 'content/' + iframeSrc;

								scope.selectedNode.weblink = $sce.trustAsResourceUrl(iframeSrc);
								scope.selectedNode.waiting = false;								
							}
                            else {
                                scope.selectedNode.showAce = false;
                                scope.selectedNode.showFrame = false;
                                scope.selectedNode.waiting = false;
                            }
						}
						nodeSelectionChanged(scope.selectedNode);
					}).error(function(data, status, headers, config) {
					});
				}
				else {
					scope.selectedNode.parentContent = null;
					scope.selectedNode.waiting = false;
				}
				if(scope.selectionChanged) 
				  $timeout(function() {
					scope.selectionChanged(scope.selectedNode);
				  });
				  				  
				if (typeof(scrollingContent) != "undefined" && scrollingContent != null) {
					scrollingContent.jScrollPane( { autoReinitialise: true })
					.parent(".jScrollPaneContainer").css({
						width:	'100%'
					,	height:	'100%'
					});
				}			
				if (typeof(scrollingContent2) != "undefined" && scrollingContent2 != null) {
					scrollingContent2.jScrollPane( { autoReinitialise: true })
					.parent(".jScrollPaneContainer").css({
						width:	'100%'
					,	height:	'100%'
					});
				}			

				nodeSelectionChanged(scope.selectedNode);
              });
              function expandAndSelect(ids) {
                ids = ids.slice()
                var expandIds = function() {
                  if(ids.length == 1) {
                    treeElement.jstree('deselect_node', treeElement.jstree('get_selected'));
                    treeElement.jstree('select_node', ids[0]);
                  }
                  else
                    treeElement.jstree('open_node', ids[0], function() {
                      ids.splice(0, 1);
                      expandIds();
                    });
                };
                expandIds();
              }      
              scope.$watch('selectedNode.id', function() {
                var selectedIds = treeElement.jstree('get_selected');
                if((selectedIds.length == 0 && scope.selectedNode.id) 
                 || selectedIds.length != 1 || selectedIds[0] != scope.selectedNode.id) {
                  if(selectedIds.length != 0)
                    treeElement.jstree('deselect_node', treeElement.jstree('get_selected'));
                  if(scope.selectedNode.id){
                    if(scope.selectedNode.showWelcome){
                      for (var i = 0; i < rootNodes.length; i++) {
                        if (rootNodes[i].text != undefined && rootNodes[i].text.indexOf(scope.selectedNode.id) != -1){
                          scope.selectedNode.id = rootNodes[i].id;
                          break;
                        }
                      }
                    }
                    treeElement.jstree('select_node', scope.selectedNode.id);
                  }
                }
                //nodeSelectionChanged(scope.selectedNode);
              });
              scope.$watch('selectedNode.path', function() {
                if(scope.pathToIdsUrl) {         
                  var selected = treeElement.jstree('get_selected', true);
                  var prevPath = selected.length ? selected[0].a_attr.path : null;
                  var newPath = scope.selectedNode.path
                  if(selected.length != 1 || prevPath != newPath) {
                    if(newPath)
                      $http.get(scope.pathToIdsUrl, { params: { path: newPath }}).then(function(data) {
                        expandAndSelect(data.data);

                      });
                    else
                      scope.selectedNode.id = null
                  }
                }
              });      
            //nodeSelectionChanged(scope.selectedTreeNode);
        }
    };
});

bpApp.value('ui.config', {
    uiLayout: {
        applyDemoStyles: true,
		west__size: 430,
		east__size: 500,
		north__size: 0,
		west__resizable: true,
		west__slidable: true,
		west__spacing_open: 3,
		south__resizable: true,
		south__slidable: true,
		south__closed: true,
		south__spacing_open: 3,
		east__spacing_open: 3,		
		stateManagement__enabled: true,
		stateManagement__autoLoad:	true,
		stateManagement__autoSave:  true,
		center__onresize:	function() {
			if (scrollingContent != null) {
				scrollingContent.jScrollPane( { autoReinitialise: true })
				.parent(".jScrollPaneContainer").css({
					width:	'100%'
				,	height:	'100%'
				});
			}
			if (scrollingContent2 != null) {
				scrollingContent2.jScrollPane( { autoReinitialise: true })
				.parent(".jScrollPaneContainer").css({
					width:	'100%'
				,	height:	'100%'
				});
			}
		}

    }
});

bpApp.directive('uiLayout', ['ui.config', function (uiConfig) {
  var options = uiConfig.uiLayout || {};
  return {

    priority:0,
    restrict: 'EA',
    compile: function (tElm, tAttrs) {
      if (angular.isUndefined(window.jQuery)) {
        throw new Error('ui-jq: Need jQuery, maybe...');
      }
      if (!angular.isFunction($(tElm).layout)) {
        throw new Error('ui-jq: Need jquery.layout, maybe...');
      } 
      return function(scope, iElement, iAttr){
        options = angular.extend({}, options, scope.$eval(tAttrs.uiLayout));
        var mylayout = $(tElm).layout(options);		
		//playout = angular.element(mylayout);
		scrollingContent = angular.element(mylayout.panes.center).find("div.scrolling-content:first");
		if (scrollingContent != null) {
			scrollingContent.jScrollPane( { autoReinitialise: true })
			.parent(".jScrollPaneContainer").css({
				width:	'100%'
			,	height:	'100%'
			});
		}
		scrollingContent2 = angular.element(mylayout.panes.west).find("div#jstree:first");				
		if (scrollingContent2 != null) {
			scrollingContent2.jScrollPane( { autoReinitialise: true })
			.parent(".jScrollPaneContainer").css({
				width:	'100%'
			,	height:	'100%'
			});
		}
      };
      
    }
  };
}]);
          
bpApp.directive('uiLayoutCenter', ['ui.config', function (uiConfig) {
  return {
    priority:1,
    restrict: 'EA',
    transclude:true,
    replace:true,
    template:'<div class="ui-layout-center"><div ng-transclude></div></div>'
  };
}]);
          
bpApp.directive('uiLayoutNorth', ['ui.config', function (uiConfig) {
  return {
    priority:1,
    restrict: 'EA',
    transclude:true,
    replace:true,
    template:'<div class="ui-layout-north"><div ng-transclude></div></div>'
  };
}]);
          
bpApp.directive('uiLayoutSouth', ['ui.config', function (uiConfig) {
  return {
    priority:1,
    restrict: 'EA',
    transclude:true,
    replace:true,
    template:'<div class="ui-layout-south"><div ng-transclude></div></div>'
  };
}]);
          
bpApp.directive('uiLayoutEast', ['ui.config', function (uiConfig) {
  return {
    priority:1,
    restrict: 'EA',
    transclude:true,
    replace:true,
    template:'<div class="ui-layout-east"><div ng-transclude></div></div>'
  };
}]);
          
bpApp.directive('uiLayoutWest', ['ui.config', function (uiConfig) {
  return {
    priority:1,
    restrict: 'EA',
    transclude:true,
    replace:true,
    template:'<div class="ui-layout-west"><div ng-transclude></div></div>'
  };
}]);

bpApp.controller('LayoutCtrl', function($scope) {

});

bpApp.directive('focusOn', function ($parse) {
     return function(scope, element, attr) {
       var val = $parse(attr['focusOn']);
      scope.$watch(val, function (val) {
         if (val) {
           element.focus();
         }
       });
     }
  });
  
bpApp.directive('select2FocusOn', function ($parse, $cookieStore, $timeout) {
    return function(scope, element, attr) {
      var val = $parse(attr['select2FocusOn']);
      scope.$watch(val, function (val) {
        if (val) {
          $timeout(function () {
            element.select2('focus', true);
			if (typeof(scope.devtoolId) != "undefined" && scope.devtoolId != null) {
				var p = '#/DevTool/'+scope.devtoolId;
				//if (typeof($scope.packageId) != "undefined" && $scope.packageId !== "") {
				//	p += "/Package/"+$scope.packageId;							
				//}				
				element.select2('data', { id: p , text: scope.devtoolId, image: $cookieStore.get('select2Image') });
				element.select2('val', scope.devtoolId );
			}
			else if (typeof(scope.deviceId) != "undefined" && scope.deviceId != null) {
				var p = '#/Device/'+scope.deviceId;
				//if (typeof($scope.packageId) != "undefined" && $scope.packageId !== "") {
				//	p += "/Package/"+$scope.packageId;							
				//}								
				element.select2('data', { id: p, text: scope.deviceId });
				element.select2('val', scope.deviceId );
			}
			else {
				element.tooltip({'trigger':'focus', 'title': 'Password tooltip'});
			}
			//element.select2('open')
          }, 0);
        }
      });
    }
  });

  
bpApp.config(['$tooltipProvider', function($tooltipProvider){
  $tooltipProvider.setTriggers({
	'placement': 'bottom',
	'animation': true,
	'popupDelay': 0,	
    'never': 'mouseleave' // <- This ensures the tooltip will go away on mouseleave
  });
}]);


bpApp.controller('MyController', function ($scope, $rootScope, $cookieStore, $location, $timeout, $route /*, ipCookie*/) {
	$scope.areCookiesEnabled = false;

    //ipCookie("TestCookie", "TestCookieText");
    //$scope.cookieValue = ipCookie("TestCookie");

    //if ($scope.cookieValue) {
    //    $scope.areCookiesEnabled = true;
    //}
	
	
// TI Tools menu logic
	$scope.showTIToolsMenu = false; // initial value set to false to not show the menu
	$scope.hideMenuTimeout;	// interval timeout event variable, can be use to set and clear the event
	
	$scope.showAppMenu = function() {
		// show the apps menu (and clear any interval event we might have set up to close the menu)
		if ( $(window).width() > 720 ) {
			clearInterval($scope.hideMenuTimeout);
			$scope.showTIToolsMenu=true;
		}
	}
	
	$scope.hideAppMenu = function(timeout) {
		// set up an interval event to close the menu after the given timeout value (in ms)
		if ( $(window).width() > 720 ) {
			$scope.hideMenuTimeout=setInterval(function(){
				$scope.showTIToolsMenu=false;
				clearInterval($scope.hideMenuTimeout);
				$scope.$digest();
			},timeout);
		}
	}
	
	/*
	$scope.expires = 365 * 2 ;
    $scope.expirationUnit = 'days';
		
	if ($scope.areCookiesEnabled && (ipCookie('showIntro') === undefined || ipCookie('showIntro'))) {
		$rootScope.showIntro = true;
		ipCookie('showIntro', 'false', { expires: $scope.expires, expirationUnit: $scope.expirationUnit });
	}
	else {
		$rootScope.showIntro = false;
	}		
	*/
	$rootScope.showIntro = false;
	$scope.startTour = function() {		
	/*
		if ($scope.areCookiesEnabled) {
			ipCookie.remove('showIntro');
			$route.reload();
		}
		else {
	*/		
		$rootScope.showIntro = true;
		$scope.GuidedTour();
		$('#guided-tour-package-images').slick({
			centerMode: true,
			arrows: true,
			slidesToScroll: 1,
			autoplay: true,
			autoplaySpeed: 2000,
			variableWidth: true,
			nextArrow: '.btn-prev',
			prevArrow: '.btn-next',
		});
	}

    $scope.CompletedEvent = function (scope) {	
		$timeout(function() {			
			if (scrollingContent != null) {
				scrollingContent.jScrollPane( { autoReinitialise: true })
				.parent(".jScrollPaneContainer").css({
					width:	'100%'
				,	height:	'100%'
				});
			}
			if (scrollingContent2 != null) {
				scrollingContent2.jScrollPane( { autoReinitialise: true })
				.parent(".jScrollPaneContainer").css({
					width:	'100%'
				,	height:	'100%'
				});
			}
		}, 500);
		$route.reload();	

    };

    $scope.ExitEvent = function (scope) {
		$timeout(function() {
			if (scrollingContent != null) {
				scrollingContent.jScrollPane( { autoReinitialise: true })
				.parent(".jScrollPaneContainer").css({
					width:	'100%'
				,	height:	'100%'
				});
			}
			if (scrollingContent2 != null) {
				scrollingContent2.jScrollPane( { autoReinitialise: true })
				.parent(".jScrollPaneContainer").css({
					width:	'100%'
				,	height:	'100%'
				});
			}			
		}, 500);		
		$route.reload();
    };

    $scope.ChangeEvent = function (targetElement, scope) {
    };

    $scope.BeforeChangeEvent = function (targetElement, scope) {
        /*console.log("Before Change Event called");
        console.log(targetElement);
		console.log(this._currentStep);
		if (this._currentStep == 0) {
			this.setOption("nextLabel","<strong>Yes</strong>");
		}
		*/
		//console.log(this);
    };

    $scope.AfterChangeEvent = function (targetElement, scope) {
    };


    $scope.IntroOptions = {
        steps:[
        /*{
            element: document.querySelector('#step0'),
            intro: "Do you want a guided tour?"
        }, */
        {
            element: document.querySelector('#step1'),
            intro: "<strong>Guided Tour</strong><br/>Start by selecting your board or device here. This will show resources related to your selection in tree below. You can filter the list of available boards/devices by typing any part of the name in this box.",
			position: "right"
        },
        {
            element: document.querySelectorAll('#step2')[0],
            intro: "Search resources in tree containing specified keyword(s).",
            position: 'left'
        },
        {
            element: '#step3',
            intro: 'Specify filter to use for reducing list of device/development tools in dropdown and resources shown in tree.',
            position: 'left'
        },		
        {
            element: '#step4',
            intro: 'Use this tree to navigate and view the list of resources resulting from your selection and search.',
            position: 'right'
        },
        {
            element: '#step5',
            intro: "As you navigate the tree, selected resources will be shown in this pane. You can use buttons available in this pane: <br/><ul><li> <a class='btn btn-info btn-mini icon-size'><img src='icns/download7.svg' height='14'></a> To download resources to desktop</li><li> <a  class='btn btn-info btn-mini icon-size'><img src='icns/cloudCube.svg' height='14'></a> To import examples into CCS cloud</li></ul>",
            position: 'left'
        }
        ],
        showStepNumbers: false,
        exitOnOverlayClick: true,
        exitOnEsc:true,
        nextLabel: '<strong>Next</strong>',
        prevLabel: '<span>Previous</span>',
        skipLabel: '<strong>Exit</strong>',
        doneLabel: 'Done'
    };

});

// [ Bruce - patch for #12945
//   A better approach is to centralize more node related property updates and munipulate the CSS style directly.
//   Since I am not familiar with the original author's code, I put a patch here for easier maintenance.
function nodeSelectionChanged(node) {
    if (node === undefined || !node) return false;

    // default
    node.headerBgColor = "#ebebeb";
    node.isLeaf = false;
    
    if(!node.content || node.content.length==0) {
        // leaf specific settings
        node.isLeaf = true;
        node.headerBgColor = "#ffffff";
    }
    return;
}
// ]
