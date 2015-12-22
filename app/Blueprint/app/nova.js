 var nova = angular.module("nova", ["jqwidgets"]);

 nova.controller("novaController", ['$scope', '$window', function($scope, $window) {
     // the 'layout' JSON array defines the internal structure of the layout
     var layout = [{
         type: 'layoutGroup',
         orientation: 'horizontal',
         items: [{
                 type: 'tabbedGroup',
                 width: 220,
                 minWidth: 200,
                 items: [{
                     type: 'layoutPanel',
                     title: 'Explorer',
                     contentContainer: 'SolutionExplorerPanel',
                     initContent: function() {
                         // initialize a jqxTree inside the Solution Explorer Panel
                         var source = [{
                             icon: '/images/earth.png',
                             label: 'Project',
                             expanded: true,
                             items: [{
                                 icon: '/images/Folder.png',
                                 label: 'css',
                                 expanded: true,
                                 items: [{
                                     icon: '/images/Text.png',
                                     label: 'jqx.base.css'
                                 }, {
                                     icon: '/images/Text.png',
                                     label: 'jqx.energyblue.css'
                                 }, {
                                     icon: '/images/Text.png',
                                     label: 'jqx.orange.css'
                                 }]
                             }, {
                                 icon: '/images/Folder.png',
                                 label: 'scripts',
                                 items: [{
                                     icon: '/images/Text.png',
                                     label: 'jqxcore.js'
                                 }, {
                                     icon: '/images/Text.png',
                                     label: 'jqxdata.js'
                                 }, {
                                     icon: '/images/Text.png',
                                     label: 'jqxgrid.js'
                                 }]
                             }, {
                                 icon: '/images/Text.png',
                                 label: 'index.htm'
                             }]
                         }];

                         $('#solutionExplorerTree').jqxTree({
                             source: source,
                             width: 190
                         });

                         $('#jstree').jstree({
                             "core": {
                                 "animation": 0,
                                 "check_callback": true,
                                 "themes": {
                                     "stripes": false
                                 },
                                 'data': {
                                     'url': function(node) {
                                         //return node.id === '#' ? 'app/tree_root.json' : 'app/tree_children.json';

                                         if (node.id === '#')
                                             return 'app/tree_root.json';
                                         else if (node.id === 'scenarios')
                                             return 'app/scenarios.json';
                                         else return 'app/tree_children.json';

                                     },
                                     'data': function(node) {
                                         return {
                                             'id': node.id
                                         };
                                     }
                                 }
                             },
                             "types": {
                                 "#": {
                                     "max_children": 1,
                                     "max_depth": 4,
                                     "valid_children": ["root"]
                                 },
                                 "root": {
                                     "icon": "/static/3.2.1/assets/images/tree_icon.png",
                                     "valid_children": ["default"]
                                 },
                                 "default": {
                                     "valid_children": ["default", "file"]
                                 },
                                 "file": {
                                     "icon": "glyphicon glyphicon-file",
                                     "valid_children": []
                                 }
                             },
                             "plugins": [
                                 "contextmenu", "dnd", "search", "state", "types", "wholerow"
                             ]
                         });
                         $('#jstree').bind('select_node.jstree', function(event, data) {
                             if (data.node.id.indexOf('scenario') === -1 || data.node.text === 'Scenarios') return;
                             console.log(data.node.id);
                             console.log(data.node);
                             console.log(layout);

                             var floatingPanel = $('#jqxLayout').jqxDockingLayout('addFloatGroup', 500, 400, {
                                     x: 500,
                                     y: 200
                                 }, 'documentPanel', 'Scenario: ' + data.node.text,
                                 '<div id="mxgraph-' + data.node.id + '"><div id="outlineContainer-' + data.node.id + '" style="z-index:1;position:absolute;overflow:hidden;bottom:0px;right:0px;width:160px;height:120px;background:transparent;border-style:solid;border-color:lightgray;"></div></div>',
                                 function() {
                                     if (!mxClient.isBrowserSupported()) {
                                         mxUtils.error('Browser is not supported!', 200, false);
                                         return;
                                     }
                                     var container = angular.element('<div/>')[0];
                                     $("#mxgraph-"+data.node.id).append(container);

                                     container.style.position = 'absolute';
                                     container.style.overflow = 'hidden';
                                     container.style.left = '10px';
                                     container.style.top = '40px';
                                     container.style.right = '10px';
                                     container.style.bottom = '10px';

                                     var outline = angular.element('#outlineContainer-'+ data.node.id)[0]; //document.getElementById('outlineContainer');
                                     mxEvent.disableContextMenu(container);

                                     if (mxClient.IS_QUIRKS) {
                                         document.body.style.overflow = 'hidden';
                                         new mxDivResizer(container);
                                         new mxDivResizer(outline);
                                     }
                                     var graph = new mxGraph(container);
                                     graph.setCellsMovable(true);
                                     graph.setAutoSizeCells(false);
                                     graph.setPanning(true);
                                     graph.centerZoom = true;
                                     graph.panningHandler.useLeftButtonForPanning = true;
                                     graph.panningHandler.popupMenuHandler = true;

                                     var outln = new mxOutline(graph, outline);
                                     graph.setTooltips(!mxClient.IS_TOUCH);
                                     var style = graph.getStylesheet().getDefaultVertexStyle();
                                     style[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_LABEL;

                                     style[mxConstants.STYLE_VERTICAL_ALIGN] = mxConstants.ALIGN_MIDDLE;
                                     style[mxConstants.STYLE_ALIGN] = mxConstants.ALIGN_LEFT;
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


                                     var edgeHandleConnect = mxEdgeHandler.prototype.connect;
                                     mxEdgeHandler.prototype.connect = function(edge, terminal, isSource, isClone, me) {
                                         edgeHandleConnect.apply(this, arguments);
                                         executeLayout();
                                     };


                                     layout.useBoundingBox = false;
                                     layout.edgeRouting = false;
                                     layout.levelDistance = 60;
                                     layout.nodeDistance = 16;

                                     // Allows the layout to move cells even though cells
                                     // aren't movable in the graph
                                     layout.isVertexMovable = function(cell) {
                                         return true;
                                     };

                                     var layoutMgr = new mxLayoutManager(graph);

                                     layoutMgr.getLayout = function(cell) {
                                         if (cell.getChildCount() > 0) {
                                             return layout;
                                         }
                                     };

                                     // Installs a popupmenu handler using local function (see below).
                                     graph.popupMenuHandler.factoryMethod = function(menu, cell, evt) {
                                         return createPopupMenu(graph, menu, cell, evt);
                                     };

                                     // Fix for wrong preferred size
                                     // var oldGetPreferredSizeForCell = graph.getPreferredSizeForCell;
                                     // graph.getPreferredSizeForCell = function(cell) {
                                     //     var result = oldGetPreferredSizeForCell.apply(this, arguments);

                                     //     if (result != null) {
                                     //         result.width = Math.max(120, result.width - 40);
                                     //     }

                                     //     return result;
                                     // };

                                     // Sets the maximum text scale to 1
                                     graph.cellRenderer.getTextScale = function(state) {
                                         return Math.min(1, state.view.scale);
                                     };

                                     // Dynamically adds text to the label as we zoom in
                                     // (without affecting the preferred size for new cells)
                                     graph.cellRenderer.getLabelValue = function(state) {
                                         var result = state.cell.value;

                                         if (state.view.graph.getModel().isVertex(state.cell)) {
                                             if (state.view.scale > 1) {
                                                 result += '\nDetails 1';
                                             }

                                             if (state.view.scale > 1.3) {
                                                 result += '\nDetails 2';
                                             }
                                         }

                                         return result;
                                     };

                                     // Gets the default parent for inserting new cells. This
                                     // is normally the first child of the root (ie. layer 0).
                                     var parent = graph.getDefaultParent();
                                     var v1;


                                    graph.getModel().beginUpdate();         
                                    try
                                    {
                                        v1 = addTask(graph,null)

                                    }
                                    finally
                                    {
                                        // Updates the display
                                        graph.getModel().endUpdate();
                                    }

                                    graph.bpStop = addStop(graph, v1);
                                    addStart(graph, v1);

                                 }
                             );


                         });

                        //console.log(floatingPanel);



                     }
                 }, {
                     type: 'layoutPanel',
                     title: 'Properties',
                     contentContainer: 'PropertiesPanel'
                 }]
             },

             {
                 type: 'layoutGroup',
                 orientation: 'vertical',
                 width: $window.innerWidth - 220 - 100,
                 items: [{
                     type: 'documentGroup',
                     height: $window.innerHeight - 120 - 60,
                     minHeight: 200,
                     items: [{
                         type: 'documentPanel',
                         title: 'New Scenario',
                         contentContainer: 'Document2Panel',
                         initContent: function() {



                             if (!mxClient.isBrowserSupported()) {
                                 mxUtils.error('Browser is not supported!', 200, false);
                                 return;
                             }
                             // Workaround for Internet Explorer ignoring certain styles
                             //var container = document.createElement('div');
                             var container = angular.element('<div/>')[0];
                             $("#mxgraph").append(container);

                             container.style.position = 'absolute';
                             container.style.overflow = 'hidden';
                             container.style.left = '10px';
                             container.style.top = '40px';
                             container.style.right = '10px';
                             container.style.bottom = '10px';

                             var outline = angular.element('#outlineContainer')[0]; //document.getElementById('outlineContainer');
                             //element.append(outline);

                             mxEvent.disableContextMenu(container);

                             if (mxClient.IS_QUIRKS) {
                                 document.body.style.overflow = 'hidden';
                                 new mxDivResizer(container);
                                 new mxDivResizer(outline);
                             }

                             // Sets a gradient background
                             // if (mxClient.IS_GC || mxClient.IS_SF)
                             // {
                             //  container.style.background = '-webkit-gradient(linear, 0% 0%, 0% 100%, from(#FFFFFF), to(#E7E7E7))';
                             // }
                             // else if (mxClient.IS_NS)
                             // {
                             //  container.style.background = '-moz-linear-gradient(top, #FFFFFF, #E7E7E7)';  
                             // }
                             // else if (mxClient.IS_IE)
                             // {
                             //  container.style.filter = 'progid:DXImageTransform.Microsoft.Gradient('+
                             //             'StartColorStr=\'#FFFFFF\', EndColorStr=\'#E7E7E7\', GradientType=0)';
                             // }

                             //document.body.appendChild(container);

                             // Creates the graph inside the given container
                             var graph = new mxGraph(container);

                             // Enables automatic sizing for vertices after editing and
                             // panning by using the left mouse button.
                             graph.setCellsMovable(true);
                             graph.setAutoSizeCells(false);
                             graph.setPanning(true);
                             graph.centerZoom = true;
                             graph.panningHandler.useLeftButtonForPanning = true;
                             //graph.htmlLabels = true;



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

                             style[mxConstants.STYLE_VERTICAL_ALIGN] = mxConstants.ALIGN_MIDDLE;
                             style[mxConstants.STYLE_ALIGN] = mxConstants.ALIGN_LEFT;
                             // style[mxConstants.STYLE_SPACING_LEFT] = 54;

                             style[mxConstants.STYLE_GRADIENTCOLOR] = '#dddddd';
                             //style[mxConstants.STYLE_STROKECOLOR] = '#999999';
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



                             //var layout = new mxHierarchicalLayout(graph, mxConstants.DIRECTION_WEST);

                             var executeLayout = function(change, post) {
                                 graph.getModel().beginUpdate();
                                 try {
                                     if (change != null) {
                                         change();
                                     }

                                     layout.execute(graph.getDefaultParent(), v1);
                                 } catch (e) {
                                     throw e;
                                 } finally {
                                     // New API for animating graph layout results asynchronously
                                     var morph = new mxMorphing(graph);
                                     morph.addListener(mxEvent.DONE, mxUtils.bind(this, function() {
                                         graph.getModel().endUpdate();

                                         if (post != null) {
                                             post();
                                         }
                                     }));

                                     morph.startAnimation();
                                 }
                             };

                             var edgeHandleConnect = mxEdgeHandler.prototype.connect;
                             mxEdgeHandler.prototype.connect = function(edge, terminal, isSource, isClone, me) {
                                 edgeHandleConnect.apply(this, arguments);
                                 executeLayout();
                             };



                             //

                             //

                             layout.useBoundingBox = false;
                             layout.edgeRouting = false;
                             layout.levelDistance = 60;
                             layout.nodeDistance = 16;

                             // Allows the layout to move cells even though cells
                             // aren't movable in the graph
                             layout.isVertexMovable = function(cell) {
                                 return true;
                             };

                             var layoutMgr = new mxLayoutManager(graph);

                             layoutMgr.getLayout = function(cell) {
                                 if (cell.getChildCount() > 0) {
                                     return layout;
                                 }
                             };

                             // Installs a popupmenu handler using local function (see below).
                             graph.popupMenuHandler.factoryMethod = function(menu, cell, evt) {
                                 return createPopupMenu(graph, menu, cell, evt);
                             };

                             // Fix for wrong preferred size
                             // var oldGetPreferredSizeForCell = graph.getPreferredSizeForCell;
                             // graph.getPreferredSizeForCell = function(cell) {
                             //     var result = oldGetPreferredSizeForCell.apply(this, arguments);

                             //     if (result != null) {
                             //         result.width = Math.max(120, result.width - 40);
                             //     }

                             //     return result;
                             // };

                             // Sets the maximum text scale to 1
                             graph.cellRenderer.getTextScale = function(state) {
                                 return Math.min(1, state.view.scale);
                             };

                             // Dynamically adds text to the label as we zoom in
                             // (without affecting the preferred size for new cells)
                             graph.cellRenderer.getLabelValue = function(state) {
                                 var result = state.cell.value;

                                 if (state.view.graph.getModel().isVertex(state.cell)) {
                                     if (state.view.scale > 1) {
                                         result += '\nDetails 1';
                                     }

                                     if (state.view.scale > 1.3) {
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
                            graph.getModel().beginUpdate();         
                            try
                            {
                                v1 = addTask(graph,null)

                            }
                            finally
                            {
                                // Updates the display
                                graph.getModel().endUpdate();
                            }

                            graph.bpStop = addStop(graph, v1);
                            addStart(graph, v1);


                             /*

                                                          var content = document.createElement('div');
                                                          content.style.padding = '4px';

                                                          var tb = new mxToolbar(content);

                                                          tb.addItem('Zoom In', 'images/zoom_in32.png', function(evt) {
                                                              graph.zoomIn();
                                                          });

                                                          tb.addItem('Zoom Out', 'images/zoom_out32.png', function(evt) {
                                                              graph.zoomOut();
                                                          });

                                                          tb.addItem('Actual Size', 'images/view_1_132.png', function(evt) {
                                                              graph.zoomActual();
                                                          });
                             */
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

 */
                             /*

                             wnd = new mxWindow('Tools', content, 100, 500, 130, 66, false);
                             wnd.setMaximizable(false);
                             wnd.setScrollable(false);
                             wnd.setResizable(false);
                             wnd.setVisible(true);
*/
                         }
                     }, {
                         type: 'documentPanel',
                         title: 'Document 1',
                         contentContainer: 'Document1Panel',
                         initContent: function() {
                             var data = new Array();
                             var rowscount = 100;
                             var firstNames =
                                 [
                                     "Andrew", "Nancy", "Shelley", "Regina", "Yoshi", "Antoni", "Mayumi", "Ian", "Peter", "Lars", "Petra", "Martin", "Sven", "Elio", "Beate", "Cheryl", "Michael", "Guylene"
                                 ];

                             var lastNames =
                                 [
                                     "Fuller", "Davolio", "Burke", "Murphy", "Nagase", "Saavedra", "Ohno", "Devling", "Wilson", "Peterson", "Winkler", "Bein", "Petersen", "Rossi", "Vileid", "Saylor", "Bjorn", "Nodier"
                                 ];

                             var productNames =
                                 [
                                     "Black Tea", "Green Tea", "Caffe Espresso", "Doubleshot Espresso", "Caffe Latte", "White Chocolate Mocha", "Caramel Latte", "Caffe Americano", "Cappuccino", "Espresso Truffle", "Espresso con Panna", "Peppermint Mocha Twist"
                                 ];

                             var priceValues =
                                 [
                                     "2.25", "1.5", "3.0", "3.3", "4.5", "3.6", "3.8", "2.5", "5.0", "1.75", "3.25", "4.0"
                                 ];

                             for (var i = 0; i < rowscount; i++) {
                                 var row = {};
                                 var productindex = Math.floor(Math.random() * productNames.length);
                                 var price = parseFloat(priceValues[productindex]);
                                 var quantity = 1 + Math.round(Math.random() * 10);

                                 row["id"] = i;
                                 row["available"] = productindex % 2 == 0;
                                 if (productindex % 2 != 0) {
                                     var random = Math.floor(Math.random() * rowscount);
                                     row["available"] = i % random == 0 ? null : false;
                                 }
                                 row["firstname"] = firstNames[Math.floor(Math.random() * firstNames.length)];
                                 row["lastname"] = lastNames[Math.floor(Math.random() * lastNames.length)];
                                 row["name"] = row["firstname"] + " " + row["lastname"];
                                 row["productname"] = productNames[productindex];
                                 row["price"] = price;
                                 row["quantity"] = quantity;
                                 row["total"] = price * quantity;

                                 var date = new Date();
                                 date.setFullYear(2015, Math.floor(Math.random() * 12), Math.floor(Math.random() * 27));
                                 date.setHours(0, 0, 0, 0);
                                 row["date"] = date;

                                 data[i] = row;
                             }

                             var source = {
                                 localdata: data,
                                 datafields: [{
                                     name: 'name',
                                     type: 'string'
                                 }, {
                                     name: 'productname',
                                     type: 'string'
                                 }, {
                                     name: 'available',
                                     type: 'bool'
                                 }, {
                                     name: 'date',
                                     type: 'date'
                                 }, {
                                     name: 'quantity',
                                     type: 'number'
                                 }],
                                 datatype: "array"
                             };
                             var dataAdapter = new $.jqx.dataAdapter(source);
                             $("#jqxgrid").jqxGrid({
                                 width: '100%',
                                 height: '100%',
                                 source: dataAdapter,
                                 showfilterrow: true,
                                 filterable: true,
                                 theme: 'blueprint',
                                 selectionmode: 'multiplecellsextended',
                                 columns: [{
                                     text: 'Name',
                                     columntype: 'textbox',
                                     datafield: 'name',
                                     width: '20%'
                                 }, {
                                     text: 'Product',
                                     datafield: 'productname',
                                     width: '35%'
                                 }, {
                                     text: 'Ship Date',
                                     datafield: 'date',
                                     filtertype: 'date',
                                     width: '30%',
                                     cellsalign: 'right',
                                     cellsformat: 'd'
                                 }, {
                                     text: 'Qty.',
                                     datafield: 'quantity',
                                     width: '15%',
                                     cellsalign: 'right'
                                 }]
                             });
                         }
                     }]
                 }, {
                     type: 'tabbedGroup',
                     height: 120,
                     pinnedHeight: 30,
                     items: [{
                         type: 'layoutPanel',
                         title: 'Error List',
                         contentContainer: 'ErrorListPanel'
                     }, {
                         type: 'layoutPanel',
                         title: 'Output',
                         contentContainer: 'OutputPanel',
                         selected: true
                     }]
                 }]
             },

             {
                 type: 'autoHideGroup',
                 alignment: 'right',
                 width: 100,
                 unpinnedWidth: 300,

                 items: [{
                     type: 'layoutPanel',
                     title: 'Properties',
                     contentContainer: 'Utility-PropertiesPanel',
                     initContent: function() {
                        $('#props').jqxExpander({width: '100%', theme: 'blueprint'});
                        $('#author').jqxExpander({width: '100%', theme: 'blueprint'});
                        $('#details').jqxExpander({width: '100%', theme: 'blueprint'});
                     }
                 }, {
                     type: 'layoutPanel',
                     title: 'Discussions',
                     contentContainer: 'DiscussionsPanel'
                 }, {
                     type: 'layoutPanel',
                     title: 'Files',
                     contentContainer: 'FilesPanel'
                 }, {
                     type: 'layoutPanel',
                     title: 'Relationships',
                     contentContainer: 'RelationshipsPanel'
                 }, {
                     type: 'layoutPanel',
                     title: 'Outline',
                     contentContainer: 'OutlinePanel'
                 }, {
                     type: 'layoutPanel',
                     title: 'Browse',
                     contentContainer: 'BrowsePanel'
                 }, {
                     type: 'layoutPanel',
                     title: 'History',
                     contentContainer: 'HistoryPanel'
                 }, {
                     type: 'layoutPanel',
                     title: 'Review',
                     contentContainer: 'ReviewsPanel'
                 }]


             }

         ]
     }];
     $scope.settings = {
         width: '100%', //$window.innerWidth,
         height: $window.innerHeight - 60,
         layout: layout,
         contextMenu: true,
         theme: 'blueprint'
     };
 }]);


 nova.controller("demoController", function($scope) {
     // Grid data.
     var data = new Array();
     var firstNames = ["Nancy", "Andrew", "Janet", "Margaret", "Steven", "Michael", "Robert", "Laura", "Anne"];
     var lastNames = ["Davolio", "Fuller", "Leverling", "Peacock", "Buchanan", "Suyama", "King", "Callahan", "Dodsworth"];
     var titles = ["Sales Representative", "Vice President, Sales", "Sales Representative", "Sales Representative", "Sales Manager", "Sales Representative", "Sales Representative", "Inside Sales Coordinator", "Sales Representative"];
     var city = ["Seattle", "Tacoma", "Kirkland", "Redmond", "London", "London", "London", "Seattle", "London"];
     var country = ["USA", "USA", "USA", "USA", "UK", "UK", "UK", "USA", "UK"];

     for (var i = 0; i < firstNames.length; i++) {
         var row = {};
         row["firstname"] = firstNames[i];
         row["lastname"] = lastNames[i];
         row["title"] = titles[i];
         row["city"] = city[i];
         row["country"] = country[i];
         data[i] = row;
     }

     $scope.people = data;

     $scope.bindingCompleted = "";
     $scope.settings = {
         altrows: true,
         width: 800,
         height: 200,
         ready: function() {
             $scope.settings.apply('selectrow', 1);
         },
         sortable: true,
         source: $scope.people,
         columns: [{
             text: 'First Name',
             datafield: 'firstname',
             width: 150
         }, {
             text: 'Last Name',
             datafield: 'lastname',
             width: 150
         }, {
             text: 'Title',
             datafield: 'title',
             width: 150
         }, {
             text: 'City',
             datafield: 'city',
             width: 150
         }, {
             text: 'Country',
             datafield: 'country'
         }],
         bindingcomplete: function(event) {
             $scope.bindingCompleted = "binding is completed";
         }
     }
 });