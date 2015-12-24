var taskid = 1;

// Function to create the entries in the popupmenu
function createPopupMenu(graph, menu, cell, evt) {
	
	var model = graph.getModel();

	if (cell == null || cell.id == 'start' || cell.id == 'stop') return;
		
	if (model.isVertex(cell)) {
		menu.addItem('Add Task', 'images/rectangle.gif', function() {
			addTask(graph, cell);
		});


		menu.addItem('Add Condition', 'images/rhombus.gif', function() {
			addCondition(graph, cell);
		});

		console.log(cell.style);
		if (cell.style &&  cell.style.shape == mxConstants.SHAPE_RHOMBUS) {
			menu.addItem('Add Condition Branch', 'images/tree.gif', function() {
				addConditionBranch(graph, cell);
			});

		}

		menu.addItem('Edit Label', 'images/text.gif', function() {
			graph.startEditingAtCell(cell);
		});

		if (!cell.style ||  cell.style.shape != mxConstants.SHAPE_RHOMBUS) {
			menu.addItem('Configure Task', 'images/open.gif', function() {
				openDialog(graph, cell);
			});
		}

		if (cell.id != 'treeRoot' ) {
			menu.addItem('Delete', 'images/delete.gif', function() {
				deleteTask(graph, cell);
			});
		}
	}
	else { //edge
		menu.addItem('Edit Label', 'images/text.gif', function() {
			graph.startEditingAtCell(cell);
		});
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

	var overlay = new mxCellOverlay(new mxImage('images/colors-on.png', 25, 25), 'Colors');
	overlay.align = mxConstants.ALIGN_RIGHT;
	overlay.verticalAlign = mxConstants.ALIGN_TOP;
	overlay.offset = new mxPoint(-12, 12);	
	graph.addCellOverlay(cell, overlay);	

	overlay = new mxCellOverlay(new mxImage('images/user-persona-default-icon.png', 25, 25), 'Persona');
	overlay.align = mxConstants.ALIGN_LEFT;
	overlay.verticalAlign = mxConstants.ALIGN_TOP;
	overlay.offset = new mxPoint(13, 13);	
	graph.addCellOverlay(cell, overlay);		



	overlay = new mxCellOverlay(new mxImage('images/trash-on.png', 20, 20), 'Trash');
	if (taskid%2 == 0)
		overlay = new mxCellOverlay(new mxImage('images/trash-off.png', 20, 20), 'Trash');
	overlay.align = mxConstants.ALIGN_LEFT;
	overlay.verticalAlign = mxConstants.ALIGN_BOTTOM;
	overlay.offset = new mxPoint(15, -15);	
	graph.addCellOverlay(cell, overlay);
	overlay.addListener(mxEvent.CLICK, mxUtils.bind(this, function(sender, evt)
	{
		deleteTask(graph, cell);
	}));


	overlay = new mxCellOverlay(new mxImage('images/mockup-on.png', 20, 20), 'Mockup');
	overlay.align = mxConstants.ALIGN_LEFT;
	overlay.verticalAlign = mxConstants.ALIGN_BOTTOM;
	overlay.offset = new mxPoint(45, -15);	
	graph.addCellOverlay(cell, overlay);	


	overlay = new mxCellOverlay(new mxImage('images/link-include-on.png', 20, 20), 'Link');
	overlay.align = mxConstants.ALIGN_LEFT;
	overlay.verticalAlign = mxConstants.ALIGN_BOTTOM;
	overlay.offset = new mxPoint(75, -15);	
	graph.addCellOverlay(cell, overlay);	


	overlay = new mxCellOverlay(new mxImage('images/more-on.png', 20, 20), 'Link');
	overlay.align = mxConstants.ALIGN_LEFT;
	overlay.verticalAlign = mxConstants.ALIGN_BOTTOM;
	overlay.offset = new mxPoint(105, -15);	
	graph.addCellOverlay(cell, overlay);	

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
		edge.geometry.x = 1;
		edge.geometry.y = 0;
		edge.geometry.offset = new mxPoint(0, -20);

		var insertedEdge = addEdge(graph,parent, null, '', vertex, nextCell); 
		insertedEdge.geometry.x = 1;
		insertedEdge.geometry.y = 0;
		insertedEdge.geometry.offset = new mxPoint(0, -20);

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
		edge.geometry.x = 1;
		edge.geometry.y = 0;
		edge.geometry.offset = new mxPoint(0, -20);
		vertex.setStyle( style );

		var insertedEdge = addEdge(graph,parent, null, '', vertex, nextCell); 
		insertedEdge.geometry.x = 1;
		insertedEdge.geometry.y = 0;
		insertedEdge.geometry.offset = new mxPoint(0, -20);

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
		edge.geometry.x = 1;
		edge.geometry.y = 0;
		edge.geometry.offset = new mxPoint(0, -20);
		vertex.setStyle( style );

		var insertedEdge = addEdge(graph,parent, null, '', vertex, nextCell); 
		insertedEdge.geometry.x = 1;
		insertedEdge.geometry.y = 0;
		insertedEdge.geometry.offset = new mxPoint(0, -20);
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
	var edge = graph.insertEdge(parent, id, label, prevCell, nextCell, style);
	var overlay = new mxCellOverlay(new mxImage('images/add128.png', 16, 16), 'Add Task/Condition');
	overlay.cursor = 'hand';
	overlay.align = mxConstants.ALIGN_LEFT;
	overlay.addListener(mxEvent.CLICK, mxUtils.bind(this, function(sender, evt)	{
		// TODO - show a menu with add task/add condition
		addTask(graph, prevCell);
		//mxUtils.popup("testing", true);
		// var cell = evt.getProperty('cell');
		// console.log(cell);
		// graph.popupMenuHandler.factoryMethod = mxUtils.bind(this, function(menu, cell, evt)	{
		// 	return createPopupMenu(menu, cell, evt);
		// });
	}));
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