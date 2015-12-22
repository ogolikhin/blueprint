

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

	overlay = new mxCellOverlay(new mxImage('images/colors-on.png', 25, 25), 'Colors');
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


		vertex = graph.insertVertex(parent, null, 'Task');
		var geometry = model.getGeometry(vertex);
		var size = graph.getPreferredSizeForCell(vertex);
		geometry.width = 126;
		geometry.height = 150;
		var edge = graph.insertEdge(parent, null, '', cell, vertex);
		edge.geometry.x = 1;
		edge.geometry.y = 0;
		edge.geometry.offset = new mxPoint(0, -20);

		var insertedEdge = graph.insertEdge(parent, null, '', vertex, nextCell);
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
		var edge = graph.insertEdge(parent, null, '', cell, vertex);
		edge.geometry.x = 1;
		edge.geometry.y = 0;
		edge.geometry.offset = new mxPoint(0, -20);
		vertex.setStyle( style );

		var insertedEdge = graph.insertEdge(parent, null, '', vertex, nextCell);
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



		var e = graph.insertEdge(parent, null, '', cell, graph.bpStop);
		// e.geometry.x = 1;
		// e.geometry.y = 0;
		// e.geometry.offset = new mxPoint(0, -20);

		e.geometry.points = [
			new mxPoint(cell.geometry.x + cell.geometry.width / 2, cell.geometry.y + cell.geometry.height / 2 + 50) ,
			new mxPoint(graph.bpStop.geometry.x + graph.bpStop.geometry.width / 2, graph.bpStop.geometry.y + graph.bpStop.geometry.height / 2 + 50)
			];

		// e = graph.insertEdge(lane1a, null, 'Depending', step12, end1, 'verticalAlign=bottom');
		// e.geometry.points = [
		// 	new mxPoint(step12.geometry.x + step12.geometry.width / 2, step12.geometry.y + step12.geometry.height / 2 + 80) ,
		// 	new mxPoint(end1.geometry.x + end1.geometry.width / 2, end1.geometry.y + end1.geometry.height / 2 + 80)
		// 	];		



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
		var edge = graph.insertEdge(parent, null, '', cell, vertex);
		edge.geometry.x = 1;
		edge.geometry.y = 0;
		edge.geometry.offset = new mxPoint(0, -20);
		vertex.setStyle( style );

		var insertedEdge = graph.insertEdge(parent, null, '', vertex, nextCell);
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
		var edge = graph.insertEdge(parent, null, '', vertex, cell);
		edge.geometry.x = 1;
		edge.geometry.y = 0;
		edge.geometry.offset = new mxPoint(0, -20);
		vertex.setStyle( style );

		var insertedEdge = graph.insertEdge(parent, null, '', vertex, nextCell);
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

		var insertedEdge = graph.insertEdge(parent, null, '', prevCell, nextCell);
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