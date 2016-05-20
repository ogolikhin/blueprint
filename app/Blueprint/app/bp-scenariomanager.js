var bpApp = angular.module('bpApp', ['ngRoute', 'ui.bootstrap.modal']);

bpApp.config(function($routeProvider, $locationProvider) {

	$routeProvider
		.when('/Scenario/:scenarioName/', {
			templateUrl: 'scenario.html'
		})	
		.when('/', {
			templateUrl: 'scenarios.html',
			controller: 'ScenarioPickerController'
		})
;

});


bpApp.controller('Scenario', function($scope, $routeParams) {

	$scope.scenarioName = $routeParams['scenarioName'].split('+').join(' ');

});


bpApp.controller('ScenarioPickerController', function($scope) {

 $scope.showModal = false;
    $scope.toggleModal = function(){
        $scope.showModal = !$scope.showModal;
    };	

	$scope.open = function() {
	  $scope.showModal = true;
	};

	$scope.ok = function() {
	  $scope.showModal = false;
	};

	$scope.cancel = function() {
	  $scope.showModal = false;
	};	
});



bpApp.directive('modal', function () {
    return {
      template: '<div class="modal fade">' + 
          '<div class="modal-dialog">' + 
            '<div class="modal-content">' + 
              '<div class="modal-header">' + 
                '<button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>' + 
                '<h4 class="modal-title">{{ title }}</h4>' + 
              '</div>' + 
              '<div class="modal-body" ng-transclude></div>' + 
            '</div>' + 
          '</div>' + 
        '</div>',
      restrict: 'E',
      transclude: true,
      replace:true,
      scope:true,
      link: function postLink(scope, element, attrs) {
        scope.title = attrs.title;

        scope.$watch(attrs.visible, function(value){
          if(value == true)
            $(element).modal('show');
          else
            $(element).modal('hide');
        });

        $(element).on('shown.bs.modal', function(){
          scope.$apply(function(){
            scope.$parent[attrs.visible] = true;
          });
        });

        $(element).on('hidden.bs.modal', function(){
          scope.$apply(function(){
            scope.$parent[attrs.visible] = false;
          });
        });
      }
    };
  });


bpApp.directive('mxgraph', function() {
	return {
        restrict: 'E',
        link: function(scope, element, attrs) {
        	if (!mxClient.isBrowserSupported())	{
				mxUtils.error('Browser is not supported!', 200, false);
				return;
			}
			// Workaround for Internet Explorer ignoring certain styles
			//var container = document.createElement('div');
			var container = angular.element('<div/>')[0];
			element.append(container);
			
			container.style.position = 'absolute';
			container.style.overflow = 'hidden';
			container.style.left = '10px';
			container.style.top = '220px';
			container.style.right = '0px';
			container.style.bottom = '0px';

			var outline = angular.element('#outlineContainer')[0]; //document.getElementById('outlineContainer');
			//element.append(outline);

			mxEvent.disableContextMenu(container);

			if (mxClient.IS_QUIRKS)
			{
				document.body.style.overflow = 'hidden';
				new mxDivResizer(container);
				new mxDivResizer(outline);
			}

			// Sets a gradient background
		    // if (mxClient.IS_GC || mxClient.IS_SF)
		    // {
		    // 	container.style.background = '-webkit-gradient(linear, 0% 0%, 0% 100%, from(#FFFFFF), to(#E7E7E7))';
		    // }
		    // else if (mxClient.IS_NS)
		    // {
		    // 	container.style.background = '-moz-linear-gradient(top, #FFFFFF, #E7E7E7)';  
		    // }
		    // else if (mxClient.IS_IE)
		    // {
		    // 	container.style.filter = 'progid:DXImageTransform.Microsoft.Gradient('+
		    //             'StartColorStr=\'#FFFFFF\', EndColorStr=\'#E7E7E7\', GradientType=0)';
		    // }

			//document.body.appendChild(container);

			// Creates the graph inside the given container
			var graph = new mxGraph(container);
			
			// Enables automatic sizing for vertices after editing and
			// panning by using the left mouse button.
			graph.setCellsMovable(false);
			graph.setAutoSizeCells(false);
 			graph.setCellsSelectable(false);
 			graph.setPanning(true);
			graph.centerZoom = false;
			graph.panningHandler.useLeftButtonForPanning = true;
			//graph.htmlLabels = true;

			graph.labelChanged = function(cell, newValue, trigger)
			{
				var name = (trigger != null) ? trigger.fieldname : null;
				console.log(cell);
				console.log(newValue);
				console.log(trigger);

				mxGraph.prototype.labelChanged.apply(this, arguments);
				//
			};



			graph.getLabel = function(cell)
			{
				var label = (this.labelsVisible) ? this.convertValueToString(cell) : '';
				var geometry = this.model.getGeometry(cell);
				
				if (!this.model.isCollapsed(cell) && geometry != null && (geometry.offset == null ||
					(geometry.offset.x == 0 && geometry.offset.y == 0)) && this.model.isVertex(cell) &&
					geometry.width >= 2)
				{
					var style = this.getCellStyle(cell);
					var fontSize = style[mxConstants.STYLE_FONTSIZE] || mxConstants.DEFAULT_FONTSIZE;
					var max = geometry.width / (fontSize * 0.625);
					
					if (max < label.length)
					{
						return label.substring(0, max) + '...';
					}
				}
				
				return label;
			};
			
			// Enables wrapping for vertex labels
			graph.isWrapping = function(cell)
			{
				return true;
			};
			
			// Enables clipping of vertex labels if no offset is defined
			graph.isLabelClipped = function(cell)
			{
				var geometry = this.model.getGeometry(cell);
				
				return geometry != null && !geometry.relative && (geometry.offset == null ||
					(geometry.offset.x == 0 && geometry.offset.y == 0));
			};
			
			// Changes fill color to red on mouseover
			// graph.addMouseListener(
			// {
			//     mouseDown: function(sender, me)
			//     {
			//     	console.log("mousedown");
			//     	console.log(me.getCell());
			//     	console.log(me);
			//     },
			//     mouseMove: function(sender, me)
			//     {
			//     	// console.log("mouseMove");
			//     	// console.log(me.getCell());
			//     },
			//     mouseUp: function(sender, me) 
			//     { 
			//     	console.log("mouseUp");
			//     	console.log(me.getCell());
			//     },
			//     dragEnter: function(evt, state)
			//     {
			//     	console.log("dragEnter");
			//     	console.log(me.getCell());
			//     },
			//     dragLeave: function(evt, state)
			//     {
			//     	console.log("dragLeave");
			//     	console.log(me.getCell());
			//     }
			// });



			// Displays a popupmenu when the user clicks
			// on a cell (using the left mouse button) but
			// do not select the cell when the popup menu
			// is displayed
			graph.panningHandler.popupMenuHandler = true;

			// Creates the outline (navigator, overview) for moving
			// around the graph in the top, right corner of the window.
			var outln = new mxOutline(graph, outline);
			
			// Disables tooltips on touch devices
			graph.setTooltips(!mxClient.IS_TOUCH);

			// Set some stylesheet options for the visual appearance of vertices
			var style = graph.getStylesheet().getDefaultVertexStyle();
			style[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_LABEL;
			
			//style[mxConstants.STYLE_VERTICAL_ALIGN] = mxConstants.ALIGN_MIDDLE;
			//style[mxConstants.STYLE_ALIGN] = mxConstants.ALIGN_MIDDLE;
			// style[mxConstants.STYLE_SPACING_LEFT] = 54;
			
			style[mxConstants.STYLE_GRADIENTCOLOR] = '#dddddd';
			style[mxConstants.STYLE_STROKECOLOR] = '#999999';
			style[mxConstants.STYLE_FILLCOLOR] = '#ffffff';
			
			style[mxConstants.STYLE_FONTCOLOR] = '#333333';
			style[mxConstants.STYLE_FONTFAMILY] = '"Helvetica Neue",Helvetica,Arial,sans-serif';
			style[mxConstants.STYLE_FONTSIZE] = '12';
			style[mxConstants.STYLE_FONTSTYLE] = '0';
			
			// style[mxConstants.STYLE_SHADOW] = '1';
			// style[mxConstants.STYLE_ROUNDED] = '1';
			// style[mxConstants.STYLE_GLASS] = '1';
			
			//style[mxConstants.STYLE_IMAGE] = 'editors/images/scene.png';
			//style[mxConstants.STYLE_IMAGE_WIDTH] = '48';
			//style[mxConstants.STYLE_IMAGE_HEIGHT] = '48';
			style[mxConstants.STYLE_SPACING] = 8;

			// Sets the default style for edges
			style = graph.getStylesheet().getDefaultEdgeStyle();
			style[mxConstants.STYLE_ROUNDED] = true;
			style[mxConstants.STYLE_STROKEWIDTH] = 1;
			style[mxConstants.STYLE_EXIT_X] = 1; // right
			style[mxConstants.STYLE_EXIT_Y] = 0.5; // center
			style[mxConstants.STYLE_EXIT_PERIMETER] = 0; // disabled
			style[mxConstants.STYLE_ENTRY_X] = 0; // left
			style[mxConstants.STYLE_ENTRY_Y] = 0.5; // center
			style[mxConstants.STYLE_ENTRY_PERIMETER] = 0; // disabled
			style[mxConstants.STYLE_STROKECOLOR] = '#999999';

			
			// Disable the following for straight lines
			style[mxConstants.STYLE_EDGE] = mxEdgeStyle.ElbowConnector;

			// Stops editing on enter or escape keypress
			var keyHandler = new mxKeyHandler(graph);

			// Enables automatic layout on the graph and installs
			// a tree layout for all groups who's children are
			// being changed, added or removed.
			
			//pegah var layout = new mxCompactTreeLayout(graph, false);
			
			var layout = new mxCompactTreeLayout(graph, true);




		// Overridden to define per-shape connection points
		mxGraph.prototype.getAllConnectionConstraints = function(terminal, source)
		{
			if (terminal != null && terminal.shape != null)
			{
				if (terminal.shape.stencil != null)
				{
					if (terminal.shape.stencil != null)
					{
						return terminal.shape.stencil.constraints;
					}
				}
				else if (terminal.shape.constraints != null)
				{
					return terminal.shape.constraints;
				}
			}
	
			return null;
		};
	
		// Defines the default constraints for all shapes
		mxShape.prototype.constraints = [new mxConnectionConstraint(new mxPoint(0.25, 0), true),
										 new mxConnectionConstraint(new mxPoint(0.5, 0), true),
										 new mxConnectionConstraint(new mxPoint(0.75, 0), true),
		        	              		 new mxConnectionConstraint(new mxPoint(0, 0.25), true),
		        	              		 new mxConnectionConstraint(new mxPoint(0, 0.5), true),
		        	              		 new mxConnectionConstraint(new mxPoint(0, 0.75), true),
		        	            		 new mxConnectionConstraint(new mxPoint(1, 0.25), true),
		        	            		 new mxConnectionConstraint(new mxPoint(1, 0.5), true),
		        	            		 new mxConnectionConstraint(new mxPoint(1, 0.75), true),
		        	            		 new mxConnectionConstraint(new mxPoint(0.25, 1), true),
		        	            		 new mxConnectionConstraint(new mxPoint(0.5, 1), true),
		        	            		 new mxConnectionConstraint(new mxPoint(0.75, 1), true)];
		
		// Edges have no connection points
		mxPolyline.prototype.constraints = null;







//var layout = new mxHierarchicalLayout(graph, mxConstants.DIRECTION_WEST);
				
				var executeLayout = function(change, post)
				{
					graph.getModel().beginUpdate();
					try
					{
						if (change != null)
						{
							change();
						}
						
		    			layout.execute(graph.getDefaultParent(), v1);
					}
					catch (e)
					{
						throw e;
					}
					finally
					{
						// New API for animating graph layout results asynchronously
						var morph = new mxMorphing(graph);
						morph.addListener(mxEvent.DONE, mxUtils.bind(this, function()
						{
							graph.getModel().endUpdate();
							
							if (post != null)
							{
								post();
							}
						}));
						
						morph.startAnimation();
					}
				};
				
				var edgeHandleConnect = mxEdgeHandler.prototype.connect;
				mxEdgeHandler.prototype.connect = function(edge, terminal, isSource, isClone, me)
				{
					edgeHandleConnect.apply(this, arguments);
					executeLayout();
				};
				

                // Open popup menu from overlay
                var mxCellRendererInstallCellOverlayListeners = mxCellRenderer.prototype.installCellOverlayListeners;
                mxCellRenderer.prototype.installCellOverlayListeners = function(state, overlay, shape)
                {
                    mxCellRendererInstallCellOverlayListeners.apply(this, arguments);

                    mxEvent.addGestureListeners(shape.node, function (evt) {
                            graph.fireMouseEvent(mxEvent.MOUSE_DOWN, new mxMouseEvent(evt, state));
                            graph.rmbEdge = state.cell;
                    });
                };

                // Create edge styles
                mxEdgeStyle.DownRight = function(state, source, target, points, result)
                {
                    if (source != null && target != null)
                    {
                        var pt = new mxPoint(source.getCenterX(), target.getCenterY());

                        if (mxUtils.contains(source, pt.x, pt.y))
                        {
                            pt.x = source.x + source.width;
                        }

                        result.push(pt);
                    }
                };


                mxEdgeStyle.RightUp = function(state, source, target, points, result)
                {
                    if (source != null && target != null)
                    {
                        var pt = new mxPoint(target.getCenterX() - 10, source.getCenterY());

                        if (mxUtils.contains(source, pt.x, pt.y))
                        {
                            pt.y = source.y + source.height;
                        }

                        result.push(pt);
                    }
                };

                mxEdgeStyle.DownRightUp = function(state, source, target, points, result)
                {
                    if (source != null && target != null)
                    {
                        var yShift =  (source.cell.edges) ? source.cell.edges.length * BRANCH_HEIGHT : BRANCH_HEIGHT;
                        var pt = new mxPoint(source.getCenterX(), target.getCenterY() + yShift);
                        result.push(pt);
                        
                        pt = new mxPoint(target.getCenterX() - 10, target.getCenterY() + yShift);
                        result.push(pt);
                    }
                };

                // Register edge styles
                mxStyleRegistry.putValue('DownRight', mxEdgeStyle.DownRight);
                mxStyleRegistry.putValue('RightUp', mxEdgeStyle.RightUp);
                mxStyleRegistry.putValue('DownRightUp', mxEdgeStyle.DownRightUp);



//

//

			layout.useBoundingBox = false;
			layout.edgeRouting = false;
			layout.levelDistance = 60;
			layout.nodeDistance = 16;

			// Allows the layout to move cells even though cells
			// aren't movable in the graph
			layout.isVertexMovable = function(cell)
			{
				return true;
			};

			var layoutMgr = new mxLayoutManager(graph);

			layoutMgr.getLayout = function(cell)
			{
				if (cell.getChildCount() > 0)
				{
					return layout;
				}
			};

			// Installs a popupmenu handler using local function (see below).
			graph.popupMenuHandler.factoryMethod = function(menu, cell, evt)
			{
				return createPopupMenu(graph, menu, cell, evt);
			};

			// Fix for wrong preferred size
			// var oldGetPreferredSizeForCell = graph.getPreferredSizeForCell;
			// graph.getPreferredSizeForCell = function(cell)
			// {
			// 	var result = oldGetPreferredSizeForCell.apply(this, arguments);

			// 	if (result != null)
			// 	{
			// 		result.width = 120; //Math.max(120, result.width - 40);
			// 	}

			// 	return result;
			// };
			
			// Sets the maximum text scale to 1
			graph.cellRenderer.getTextScale = function(state)
			{
				return Math.min(1, state.view.scale);
			};

			// Dynamically adds text to the label as we zoom in
			// (without affecting the preferred size for new cells)
			graph.cellRenderer.getLabelValue = function(state)
			{
				var result = state.cell.value;
				
				if (state.view.graph.getModel().isVertex(state.cell))
				{
					if (state.view.scale > 1)
					{
						result += '\nDetails 1';
					}
					
					if (state.view.scale > 1.3)
					{
						result += '\nDetails 2';
					}
				}
				
				return result;
			};

			// Gets the default parent for inserting new cells. This
			// is normally the first child of the root (ie. layer 0).
			var parent = graph.getDefaultParent();
			var v1;


			// Adds the root vertex of the tree
			//graph.getModel().beginUpdate();			
			try
			{
                graph.bpStart = addStart1(graph);
                graph.bpStop = addStop1(graph);
                v1 = addTask1(graph, null);

			}
			finally
			{
				// Updates the display
				//graph.getModel().endUpdate();
			}



/*

			var content = document.createElement('div');
			content.style.padding = '4px';

			var tb = new mxToolbar(content);

			tb.addItem('Zoom In', 'images/zoom_in32.png',function(evt)
			{
				graph.zoomIn();
			});

			tb.addItem('Zoom Out', 'images/zoom_out32.png',function(evt)
			{
				graph.zoomOut();
			});
			
			tb.addItem('Actual Size', 'images/view_1_132.png',function(evt)
			{
				graph.zoomActual();
			});

			/*

			tb.addItem('Print', 'images/print32.png',function(evt)
			{
				var preview = new mxPrintPreview(graph, 1);
				preview.open();
			});

			tb.addItem('Poster Print', 'images/press32.png',function(evt)
			{
				var pageCount = mxUtils.prompt('Enter maximum page count', '1');

				if (pageCount != null)
				{
					var scale = mxUtils.getScaleForPageCount(pageCount, graph);
					var preview = new mxPrintPreview(graph, scale);
					preview.open();
				}
			});



			wnd = new mxWindow('Tools', content, 0, 500, 130, 66, false);
			wnd.setMaximizable(false);
			wnd.setScrollable(false);
			wnd.setResizable(false);
			wnd.setVisible(true);
			
			*/


        }
    }
});