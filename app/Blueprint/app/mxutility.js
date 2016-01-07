var taskid = 1;

var TASK_WIDTH = 126;
var TASK_HEIGHT = 150;
var CONDITION_WIDTH = 120;
var CONDITION_HEIGHT = 120;
var BRANCH_HEIGHT = 180;


function createPopupMenu(graph, menu, cell, evt) {
	try {

		var model = graph.getModel();

		if (!cell && graph.rmbEdge) {
			cell = graph.rmbEdge;
		}

		if (cell == null || cell.id == 'start' || cell.id == 'stop') return;
			
		if (model.isVertex(cell)) {

			console.log(cell.style);
			if (cell.id.indexOf("Condition") > -1) {
				menu.addItem('Add Condition Branch', 'images/tree.gif', function() {
					addConditionBranch1(graph, cell);
				});

			}

			menu.addItem('Edit Label', 'images/text.gif', function() {
				graph.startEditingAtCell(cell);
			});

			if (cell.id.indexOf("Condition") > -1) {
				menu.addItem('Configure Task', 'images/open.gif', function() {
					openDialog(graph, cell);
				});
			}

			if (cell.id != 'treeRoot' ) {
				menu.addItem('Delete', 'images/delete.gif', function() {
					deleteTask(graph, cell);
				});
			}
		} else if (cell == graph.rmbEdge) { //edge overlay
			menu.addItem('Add Task', 'images/rectangle.gif', function() {
				addTask1(graph, cell);
			});


			menu.addItem('Add Condition', 'images/rhombus.gif', function() {
				addCondition1(graph, cell);
			});

			menu.addItem('Edit Label', 'images/text.gif', function() {
				graph.startEditingAtCell(cell);
			});
		}
	} finally {
		graph.rmbEdge = null;
	}


	//menu.addSeparator();

	/*
	menu.addItem('Fit', 'images/zoom.gif', function()
	{
		graph.fit();
	});

	menu.addItem('Actual', 'images/zoomactual.gif', function()
	{
		graph.zoomActual();
	});

	menu.addSeparator();

	menu.addItem('Print', 'images/print.gif', function()
	{
		var preview = new mxPrintPreview(graph, 1);
		preview.open();
	});

	menu.addItem('Poster Print', 'images/print.gif', function()
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
}

function addOverlays(graph, cell, addDeleteIcon)
{
	/*
	var overlay = new mxCellOverlay(new mxImage('images/add.png', 24, 24), 'Add Task');
	overlay.cursor = 'hand';
	overlay.align = mxConstants.ALIGN_RIGHT;

	overlay.addListener(mxEvent.CLICK, mxUtils.bind(this, function(sender, evt)
	{
		addTask(graph, cell);
	}));
	
	graph.addCellOverlay(cell, overlay);

	if (addDeleteIcon)
	{
		overlay = new mxCellOverlay(new mxImage('images/close.png', 30, 30), 'Delete');
		overlay.cursor = 'hand';
		//overlay.offset = new mxPoint(-4, 8);
		overlay.align = mxConstants.ALIGN_RIGHT;
		overlay.verticalAlign = mxConstants.ALIGN_TOP;
		overlay.addListener(mxEvent.CLICK, mxUtils.bind(this, function(sender, evt)
		{
			deleteTask(graph, cell);
		}));
	
		graph.addCellOverlay(cell, overlay);
	}

	overlay = new mxCellOverlay(new mxImage('images/icons48/gear.png', 30, 30), 'Open Dialog');
	overlay.cursor = 'hand';
	overlay.align = mxConstants.ALIGN_LEFT;
	overlay.addListener(mxEvent.CLICK, mxUtils.bind(this, function(sender, evt)
	{
		openDialog(graph, cell);
	}));
	graph.addCellOverlay(cell, overlay);	

	*/


	var overlays = graph.getCellOverlays(cell);
					
	if (overlays != null) {
		graph.removeCellOverlays(cell);
	}

	var overlayColors = new mxCellOverlay(new mxImage('images/colors-on.png', 25, 25), 'Colors');
	overlayColors.align = mxConstants.ALIGN_RIGHT;
	overlayColors.verticalAlign = mxConstants.ALIGN_TOP;
	overlayColors.offset = new mxPoint(-12, 12);	
	graph.addCellOverlay(cell, overlayColors);	

	var overlayPersona = new mxCellOverlay(new mxImage('images/defaultuser.svg', 25, 25), 'Persona');
	overlayPersona.align = mxConstants.ALIGN_LEFT;
	overlayPersona.verticalAlign = mxConstants.ALIGN_TOP;
	overlayPersona.offset = new mxPoint(13, 13);	
	graph.addCellOverlay(cell, overlayPersona);		

	var overlayDelete = new mxCellOverlay(new mxImage('images/delete-hover.svg', 20, 20), 'Trash');
	if (taskid%2 == 0)
		overlayDelete = new mxCellOverlay(new mxImage('images/delete-neutral.svg', 20, 20), 'Trash');
	overlayDelete.align = mxConstants.ALIGN_LEFT;
	overlayDelete.verticalAlign = mxConstants.ALIGN_BOTTOM;
	overlayDelete.offset = new mxPoint(15, -15);	
	graph.addCellOverlay(cell, overlayDelete);
	overlayDelete.addListener(mxEvent.CLICK, mxUtils.bind(this, function(sender, evt)
	{
		deleteTask(graph, cell);
	}));

	var overlayMockup = new mxCellOverlay(new mxImage('images/mockup-neutral.svg', 20, 20), 'Mockup');
	overlayMockup.align = mxConstants.ALIGN_LEFT;
	overlayMockup.verticalAlign = mxConstants.ALIGN_BOTTOM;
	overlayMockup.offset = new mxPoint(39, -15);	
	graph.addCellOverlay(cell, overlayMockup);	

	var overlayLink = new mxCellOverlay(new mxImage('images/include-neutral.svg', 20, 20), 'Link');
	overlayLink.align = mxConstants.ALIGN_LEFT;
	overlayLink.verticalAlign = mxConstants.ALIGN_BOTTOM;
	overlayLink.offset = new mxPoint(63, -15);	
	graph.addCellOverlay(cell, overlayLink);	

	var overlayUserStories = new mxCellOverlay(new mxImage('images/userstories-neutral.svg', 20, 20), 'Link');
	overlayUserStories.align = mxConstants.ALIGN_LEFT;
	overlayUserStories.verticalAlign = mxConstants.ALIGN_BOTTOM;
	overlayUserStories.offset = new mxPoint(87, -15);	
	graph.addCellOverlay(cell, overlayUserStories);	

	var overlayDetails = new mxCellOverlay(new mxImage('images/adddetails-neutral.svg', 20, 20), 'Link');
	overlayDetails.align = mxConstants.ALIGN_LEFT;
	overlayDetails.verticalAlign = mxConstants.ALIGN_BOTTOM;
	overlayDetails.offset = new mxPoint(111, -15);	
	graph.addCellOverlay(cell, overlayDetails);
	overlayDetails.addListener(mxEvent.CLICK, mxUtils.bind(this, function(sender, evt) {
		openDialog(graph, cell);
	}))	

};


function addTask(graph, cell) {

	var model = graph.getModel();
	var parent = graph.getDefaultParent();
	var vertex;
	var nextCell;

	var style = graph.getStylesheet().getDefaultVertexStyle();
	style[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_LABEL;

	model.beginUpdate();
	try {

		model.getConnections(cell).forEach(function(connectedCell) {
			if (connectedCell.edge) {
				if (connectedCell.target != null && connectedCell.target.id != cell.id) {
					nextCell = connectedCell.target;
					cell.removeEdge(connectedCell);
				}
			}
		});

		vertex = graph.insertVertex(parent, "Task_"+taskid, 'Task');
		taskid++;
		var geometry = model.getGeometry(vertex);
		var size = graph.getPreferredSizeForCell(vertex);
		geometry.width = 126;
		geometry.height = 150;
		var edge = addEdge(graph,parent, null, '', cell, vertex);  
		// edge.geometry.x = 1;
		// edge.geometry.y = 0;
		// edge.geometry.offset = new mxPoint(0, -20);

		var insertedEdge = addEdge(graph,parent, null, '', vertex, nextCell); 
		// insertedEdge.geometry.x = 1;
		// insertedEdge.geometry.y = 0;
		// insertedEdge.geometry.offset = new mxPoint(0, -20);

		addOverlays(graph, vertex, true);
	}
	finally {
		model.endUpdate();
	}
	
	return vertex;
}


function addCondition(graph, cell) {

	var model = graph.getModel();
	var parent = graph.getDefaultParent();
	var vertex;
	var nextCell;

	var style = graph.getStylesheet().getDefaultVertexStyle();
	style[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_RHOMBUS;


	model.beginUpdate();
	try {

		model.getConnections(cell).forEach(function(connectedCell) {
			if (connectedCell.edge) {
				if (connectedCell.target != null && connectedCell.target.id != cell.id) {
					nextCell = connectedCell.target;
					cell.removeEdge(connectedCell);
				}
			}
		});


		vertex = graph.insertVertex(parent, null, 'Condition');
		var geometry = model.getGeometry(vertex);
		var size = graph.getPreferredSizeForCell(vertex);
		geometry.width = 100;
		geometry.height = 100;
		var edge =  addEdge(graph,parent, null, '', cell, vertex);  
		edge.geometry.x = 1;
		edge.geometry.y = 0;
		edge.geometry.offset = new mxPoint(0, -20);
		vertex.setStyle( style );

		var insertedEdge = addEdge(graph,parent, null, '', vertex, nextCell); 
		insertedEdge.geometry.x = 1;
		insertedEdge.geometry.y = 0;
		insertedEdge.geometry.offset = new mxPoint(0, -20);

		//var endEdge = graph.insertEdge(parent, null, '', vertex, graph.bpStop);
		//endEdge.geometry = insertedEdge.swap();

	}
	finally {
		model.endUpdate();
	}
	
	return vertex;
}



function addConditionBranch(graph, cell) {

	var model = graph.getModel();
	var parent = graph.getDefaultParent();
	var vertex;
	var nextCell;

	var style = graph.getStylesheet().getDefaultVertexStyle();
	style[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_RHOMBUS;


	model.beginUpdate();
	try {



		var endEdge = graph.insertEdge(parent, null, '', cell, graph.bpStop, 'edgeStyle=orthogonalEdgeStyle;');
		endEdge.geometry.x = 1;
		endEdge.geometry.y = 0;
		endEdge.geometry.offset = new mxPoint(0, -20);



	}
	finally {
		model.endUpdate();
	}
	
	return vertex;
}


function addStop(graph, cell) {

	var model = graph.getModel();
	var parent = graph.getDefaultParent();
	var vertex;
	var nextCell;

	var style = graph.getStylesheet().getDefaultVertexStyle();
	style[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_DOUBLE_ELLIPSE;


	model.beginUpdate();
	try {

		model.getConnections(cell).forEach(function(connectedCell) {
			if (connectedCell.edge) {
				if (connectedCell.target != null && connectedCell.target.id != cell.id) {
					nextCell = connectedCell.target;
					cell.removeEdge(connectedCell);
				}
			}
		});


		vertex = graph.insertVertex(parent, 'stop', '');
		var geometry = model.getGeometry(vertex);
		var size = graph.getPreferredSizeForCell(vertex);
		geometry.width = 20;
		geometry.height = 20;
		var edge = addEdge(graph,parent, null, '', cell, vertex); //graph.insertEdge(parent, null, '', cell, vertex);
		// edge.geometry.x = 1;
		// edge.geometry.y = 0;
		// edge.geometry.offset = new mxPoint(0, -20);
		vertex.setStyle( style );

		var insertedEdge = addEdge(graph,parent, null, '', vertex, nextCell); 
		// insertedEdge.geometry.x = 1;
		// insertedEdge.geometry.y = 0;
		// insertedEdge.geometry.offset = new mxPoint(0, -20);

	}
	finally {
		model.endUpdate();
	}
	
	return vertex;

}


function addStart(graph, cell) {

	var model = graph.getModel();
	var parent = graph.getDefaultParent();
	var vertex;
	var nextCell;

	var style = graph.getStylesheet().getDefaultVertexStyle();
	style[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_ELLIPSE;


	model.beginUpdate();
	try {


		vertex = graph.insertVertex(parent, 'start', '');
		var geometry = model.getGeometry(vertex);
		var size = graph.getPreferredSizeForCell(vertex);
		geometry.width = 20;
		geometry.height = 20;
		var edge = addEdge(graph,parent, null, '', vertex, cell);
		// edge.geometry.x = 1;
		// edge.geometry.y = 0;
		// edge.geometry.offset = new mxPoint(0, -20);
		vertex.setStyle( style );

		var insertedEdge = addEdge(graph,parent, null, '', vertex, nextCell); 
		// insertedEdge.geometry.x = 1;
		// insertedEdge.geometry.y = 0;
		// insertedEdge.geometry.offset = new mxPoint(0, -20);
	}
	finally {
		model.endUpdate();
	}
	
	return vertex;

}



function deleteTask(graph, cell) {

	var model = graph.getModel();
	var parent = graph.getDefaultParent();
	var nextCell;
	var prevCell;

taskid--;
	model.beginUpdate();
	try {

		model.getConnections(cell).forEach(function(connectedCell) {
			if (connectedCell.edge) {
				if (connectedCell.target != null && connectedCell.target.id != cell.id) {
					nextCell = connectedCell.target;
					cell.removeEdge(connectedCell);
				}
				if (connectedCell.source != null && connectedCell.source.id != cell.id) {
					prevCell = connectedCell.source;
					cell.removeEdge(connectedCell);
				}			
			}
		});

		var insertedEdge = addEdge(graph,parent, null, '', prevCell, nextCell);


		//graph.insertEdge(parent, null, '', prevCell, nextCell);
			insertedEdge.geometry.x = 1;
			insertedEdge.geometry.y = 0;
			insertedEdge.geometry.offset = new mxPoint(0, -20);	


		graph.removeCells([cell]);

	}
	finally {
		model.endUpdate();
	}
	
	/*
	// Gets the subtree from cell downwards
	var cells = [];
	graph.traverse(cell, true, function(vertex)
	{
		cells.push(vertex);
		
		return true;
	});

	graph.removeCells(cells); 
	*/
}


function addEdge(graph, parent, id, label, prevCell, nextCell, style) {
	var model = graph.getModel();
	var edge = graph.insertEdge(parent, id, label, prevCell, nextCell, style);
	edge.geometry.offset = new mxPoint(0, 30);
	var overlay = new mxCellOverlay(new mxImage('images/add128.png', 16, 16), 'Add Task/Condition');
	overlay.cursor = 'hand';
	//overlay.align = mxConstants.ALIGN_RIGHT;
	//overlay.offset.x = - 20;
	//overlay.id = "overlay_" + edge.id;
	var offset = 0;
	var edgeLength = Math.abs(prevCell.geometry.getCenterY() - nextCell.geometry.getCenterY()) +
					 Math.abs(prevCell.geometry.getCenterX() - nextCell.geometry.getCenterX());

	if (style == 'edgeStyle=DownRight')	{
		offset = 1 - 120 / edgeLength;
	} else if (style == 'edgeStyle=RightUp' && prevCell.geometry.getCenterY() - nextCell.geometry.getCenterY() >= BRANCH_HEIGHT) {
		offset = - (1 - 100 / edgeLength);
	}

	overlay.getBounds = function(state)
	{
	  var bounds = mxCellOverlay.prototype.getBounds.apply(this, arguments);

	  if (state.view.graph.getModel().isEdge(state.cell))
	  {
	    var pt = state.view.getPoint(state, {x: offset, y: 0, relative: true});

	    bounds.x = pt.x - bounds.width / 2;
	    bounds.y = pt.y - bounds.height / 2;
	  }

	  return bounds;
	};

	graph.addCellOverlay(edge, overlay);
	return edge;
}





function openDialog(graph, cell)
{

	$('#myModal').modal('show'); 

	// Gets the subtree from cell downwards
	/*
	var cells = [];
	graph.traverse(cell, true, function(vertex)
	{
		cells.push(vertex);
		
		return true;
	});

	graph.removeCells(cells); */
};



function createPopupMenu(graph, menu, cell, evt) {
	try {

		var model = graph.getModel();

		if (!cell && graph.rmbEdge) {
			cell = graph.rmbEdge;
		}

		if (cell == null || cell.id == 'start' || cell.id == 'stop') return;
			
		if (model.isVertex(cell)) {

			console.log(cell.style);
			if (cell.id.indexOf("Condition") > -1) {
				menu.addItem('Add Condition Branch', 'images/tree.gif', function() {
					addConditionBranch1(graph, cell);
				});

			}

			menu.addItem('Edit Label', 'images/text.gif', function() {
				graph.startEditingAtCell(cell);
			});

			if (cell.id.indexOf("Condition") > -1) {
				menu.addItem('Configure Task', 'images/open.gif', function() {
					openDialog(graph, cell);
				});
			}

			if (cell.id != 'treeRoot' ) {
				menu.addItem('Delete', 'images/delete.gif', function() {
					deleteTask(graph, cell);
				});
			}
		} else if (cell == graph.rmbEdge) { //edge overlay
			menu.addItem('Add Task', 'images/rectangle.gif', function() {
				addTask1(graph, cell);
			});


			menu.addItem('Add Condition', 'images/rhombus.gif', function() {
				addCondition1(graph, cell);
			});

			menu.addItem('Edit Label', 'images/text.gif', function() {
				graph.startEditingAtCell(cell);
			});
		}
	} finally {
		graph.rmbEdge = null;
	}


	//menu.addSeparator();

	/*
	menu.addItem('Fit', 'images/zoom.gif', function()
	{
		graph.fit();
	});

	menu.addItem('Actual', 'images/zoomactual.gif', function()
	{
		graph.zoomActual();
	});

	menu.addSeparator();

	menu.addItem('Print', 'images/print.gif', function()
	{
		var preview = new mxPrintPreview(graph, 1);
		preview.open();
	});

	menu.addItem('Poster Print', 'images/print.gif', function()
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
}




function addStart1(graph) {

	var model = graph.getModel();
	var parent = graph.getDefaultParent();
	var vertex;

	var style = graph.getStylesheet().getDefaultVertexStyle();
	style[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_ELLIPSE;

	model.beginUpdate();
	try {

		vertex = graph.insertVertex(parent, 'start', '');
		var geometry = model.getGeometry(vertex);
		geometry.width = 20;
		geometry.height = 20;
		geometry.x = 20;
		geometry.y = 80;
		vertex.setStyle( style );

	}
	finally {
		model.endUpdate();
	}
	
	return vertex;
}

function addStop1(graph) {

	var model = graph.getModel();
	var parent = graph.getDefaultParent();
	var vertex;

	var style = graph.getStylesheet().getDefaultVertexStyle();
	style[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_DOUBLE_ELLIPSE;

	model.beginUpdate();
	try {

		vertex = graph.insertVertex(parent, 'stop', '');
		var geometry = model.getGeometry(vertex);
		geometry.width = 20;
		geometry.height = 20;
		geometry.x = 240;
		geometry.y = 80;
		vertex.setStyle( style );

	}
	finally {
		model.endUpdate();
	}
	
	return vertex;

}

function addTask1(graph, edge) {
	return addVertex(graph, edge, true);
}

function addVertex(graph, edge, isTask) {

	var vertex;
	var prevVertex;
	var nextVertex;
	var y;

	if (edge == null) {
		prevVertex = graph.bpStart;
		nextVertex = graph.bpStop;
		y = prevVertex.geometry.getCenterY();
	} else {
		if (!edge.edge) return null;
		prevVertex = edge.source;
		nextVertex = edge.target;
		if (edge.geometry.points) {
			y = edge.geometry.points[0].y;
		} else {
			y = Math.max(prevVertex.geometry.getCenterY(), nextVertex.geometry.getCenterY());
		}
	}

	y = y - ((isTask) ? TASK_HEIGHT : CONDITION_HEIGHT) / 2;

	var model = graph.getModel();
	var parent = graph.getDefaultParent();

	var style = graph.getStylesheet().getDefaultVertexStyle();
	style[mxConstants.STYLE_SHAPE] = (isTask) ? mxConstants.SHAPE_LABEL : mxConstants.SHAPE_RHOMBUS;
	style[mxConstants.STYLE_FOLDABLE] = 0;

	model.beginUpdate();
	try {

		if (edge) graph.removeCells([edge]);

		var idBase = (isTask) ? "Task" : "Condition";
		vertex = graph.insertVertex(parent, idBase + "_" + taskid, idBase + "_" + taskid);
		vertex.setStyle( style );

		taskid++;

		var geometry = model.getGeometry(vertex);
		geometry.width = (isTask) ? TASK_WIDTH : CONDITION_WIDTH;
		geometry.height = (isTask) ? TASK_HEIGHT : CONDITION_HEIGHT;
		var vPort = null;
		if (prevVertex.geometry.x < 1) { // Port
			vPort = prevVertex;
			prevVertex = prevVertex.parent;
		} else if (prevVertex.children) {
			vPort = prevVertex.children[0];
		}

		geometry.x = prevVertex.geometry.x + 50;
		geometry.y = y;

		if (!vPort) {
			addEdge(graph,parent, null, '', prevVertex, vertex);  
			//addEdge(graph,parent, null, '', vertex, nextVertex); 
		} else {
			var edge = addEdge(graph,parent, null, '', prevVertex, vertex, 'edgeStyle=DownRight');  
			model.setTerminals(edge, vPort, vertex);
			var constraint = new mxConnectionConstraint(new mxPoint(0.5, 1), true);
			graph.setConnectionConstraint(edge, vPort, true, constraint);
		}

		if (nextVertex == graph.bpStop) {
			addEdge(graph,parent, null, '', vertex, nextVertex, 'edgeStyle=RightUp'); 
		} else {
			addEdge(graph,parent, null, '', vertex, nextVertex);
		}
	
		if (isTask) {
			addOverlays(graph, vertex, true);
		}

		if (nextVertex.geometry.x - prevVertex.geometry.x - prevVertex.geometry.width - 120 < vertex.geometry.width) {
			var rapper = new mxPoint(prevVertex.geometry.getCenterX(), prevVertex.geometry.getCenterY());
			moveCellsUnsafe(graph, rapper, vertex, 120 + prevVertex.geometry.width, 0);
			graph.moveCells([vertex], -50, 0);
		} else {
			geometry.x =  prevVertex.geometry.x + prevVertex.geometry.width  + 120;
		}
	}
	finally {
		model.endUpdate();
	}

	if (!isTask) {
		var style1 = graph.getStylesheet().getDefaultVertexStyle();
		style1[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_RECTANGLE;

		model.beginUpdate();
		try {

			var vPort = graph.insertVertex(vertex, null, '', 0.5, 1, 10, 10);
			vPort.setConnectable(true);
			vPort.setStyle( style1 );
			vPort.geometry.offset = new mxPoint(-5, -5);
			vPort.geometry.relative = true;		
			vPort.id = 'Port_' + taskid;
		}
		finally {
			model.endUpdate();
		}
	}

	return vertex;
}

function addCondition1(graph, edge) {
	return addVertex(graph, edge, false);

	// var vertex;
	// var prevVertex;
	// var nextVertex;

	// if (edge == null) {
	// 	prevVertex = graph.bpStart;
	// 	nextVertex = graph.bpStop;
	// } else {
	// 	if (!edge.edge) return null;
	// 	prevVertex = edge.source;
	// 	nextVertex = edge.target;
	// }

	// var model = graph.getModel();
	// var parent = graph.getDefaultParent();

	// var style = graph.getStylesheet().getDefaultVertexStyle();
	// style[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_RHOMBUS;
 //    style[mxConstants.STYLE_FOLDABLE] = 0;

	// model.beginUpdate();
	// try {

	// 	if (edge) graph.removeCells([edge]);

	// 	vertex = graph.insertVertex(parent, "Condition_"+taskid, 'Condition');
	// 	vertex.setStyle( style );
	// 	vertex.setConnectable(false);
	// 	taskid++;

	// 	var geometry = model.getGeometry(vertex);
	// 	geometry.width = CONDITION_WIDTH;
	// 	geometry.height = CONDITION_HEIGHT;

	// 	var vPort = null;
	// 	if (prevVertex.geometry.x < 1) { // Port
	// 		vPort = prevVertex;
	// 		prevVertex = prevVertex.parent;
	// 	}
	// 	geometry.x = prevVertex.geometry.x + 50;
	// 	geometry.y = prevVertex.geometry.getCenterY() - geometry.height / 2;

	// 	if (!vPort) {
		
	// 		addEdge(graph,parent, null, '', prevVertex, vertex);  
		
	// 	} else {

	// 		var edge = addEdge(graph,parent, null, '', null, null, 'edgeStyle=DownRight');  
	// 		model.setTerminals(edge, vPort, vertex);
	// 		var constraint = new mxConnectionConstraint(new mxPoint(0.5, 1), true);
	// 		graph.setConnectionConstraint(edge, vPort, true, constraint);
	// 	}
	// 	addEdge(graph,parent, null, '', vertex, nextVertex, 'edgeStyle=RightUp'); 


	// 	var rapper = new mxPoint(prevVertex.geometry.getCenterX(), prevVertex.geometry.getCenterY());
	// 	moveCellsUnsafe(graph, rapper, vertex, 120 + prevVertex.geometry.width, 0);
	// 	graph.moveCells([vertex], -50, 0);
	// }
	// finally {
	// 	model.endUpdate();
	// }
	
	// var style1 = graph.getStylesheet().getDefaultVertexStyle();
	// style1[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_RECTANGLE;

	// model.beginUpdate();
	// try {

	// 	var vPort = graph.insertVertex(vertex, null, '', 0.5, 1, 10, 10);
	// 	vPort.setConnectable(true);
	// 	vPort.setStyle( style1 );
	// 	vPort.geometry.offset = new mxPoint(-5, -5);
	// 	vPort.geometry.relative = true;		
	// 	vPort.id = 'Port_' + taskid;
	// }
	// finally {
	// 	model.endUpdate();
	// }
	//return vertex;
}

function addConditionBranch1(graph, cell) {

	var model = graph.getModel();
	var parent = graph.getDefaultParent();

	model.beginUpdate();
	try {
		var vPort = cell.children[0];
		//var edge = addEdge(graph,parent, null, '', vPort, graph.bpStop, "portConstraint=south");  
		var edge = addEdge(graph,parent, null, '', cell, graph.bpStop, 'edgeStyle=DownRightUp');  
		
		cell.removeEdge(edge);
		vPort.insertEdge(edge, true);
		model.setTerminals(edge, vPort, graph.bpStop);
		
		var constraint = new mxConnectionConstraint(new mxPoint(0.5, 1.04), true);
		graph.setConnectionConstraint(edge, vPort, true, constraint);
		//graph.bpStop.insertEdge(edge, false);

		//edge.geometry.x = cell.geometry.getCenterX() - 10;
		//edge.geometry.offset = new mxPoint(-5, -5);
		//edge.geometry.relative = true;		
		//edge.geometry.y = cell.geometry.y - cell.geometry.height / 2;
		var rapper = new mxPoint(cell.geometry.getCenterX(), graph.bpStop.geometry.getCenterY() + ((vPort.edges) ? vPort.edges.length * BRANCH_HEIGHT : BRANCH_HEIGHT));
		edge.geometry.points = new Array (rapper);

		moveCellsUnsafe(graph, rapper, null, 0, BRANCH_HEIGHT);

	}
	finally {
		model.endUpdate();
	}

	return edge;
}



function moveCellsUnsafe(graph, rapper, cell, deltaX, deltaY) {
	var model = graph.getModel();
	//var geometry = model.getGeometry(cell);
	//var target = model.getTerminal(cell, false);
	var cellsToMove = new Array();

	// if (deltaX > 0 && cell != null) {
	//  	cellsToMove.push(cell);
	// }

	graph.getChildCells(null, true, true).forEach(function(target) {
		if (model.isVertex(target) && (!cell || cell && (cell.parent == graph.getDefaultParent())) && 
			((target.geometry.getCenterX() > rapper.x + 5) && deltaX > 0 || 
			 (target.geometry.getCenterY() >= rapper.y) && deltaY > 0 && target != cell)) {
			cellsToMove.push(target);
		}
	});

	graph.moveCells(cellsToMove, deltaX, deltaY);
}

function moveCells(graph, rapper, cell, deltaX, deltaY) {
	var model = graph.getModel();
	model.beginUpdate();
	try {
		moveCellsUnsafe(graph, rapper, cell, deltaX, deltaY)	
	}
	finally {
		model.endUpdate();
	}
}