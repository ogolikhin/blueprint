/**
 * Copyright (c) 2006-2012, JGraph Ltd
 */
/**
 * Constructs a new graph instance. Note that the constructor does not take a
 * container because the graph instance is needed for creating the UI, which
 * in turn will create the container for the graph. Hence, the container is
 * assigned later in EditorUi.
 */
/**
 * Defines graph class.
 */
Graph = function(container, model, renderHint, stylesheet)
{
	mxGraph.call(this, container, model, renderHint, stylesheet);

    // Adds support for HTML labels via style. Note: Currently, only the Java
    // backend supports HTML labels but CSS support is limited to the following:
    // http://docs.oracle.com/javase/6/docs/api/index.html?javax/swing/text/html/CSS.html
	// TODO: Wrap should not affect isHtmlLabel output (should be handled later)
	this.isHtmlLabel = function(cell)
	{
		var state = this.view.getState(cell);
		var style = (state != null) ? state.style : this.getCellStyle(cell);
		
		return style['html'] == '1' || style[mxConstants.STYLE_WHITE_SPACE] == 'wrap';
	};
	
	// Implements a listener for hover and click handling on edges
	if (this.edgeMode)
	{
		var start = {
			point: null,
			event: null,
			state: null,
			handle: null
		};
		
		// Uses this event to process mouseDown to check the selection state before it is changed
		this.addListener(mxEvent.FIRE_MOUSE_EVENT, mxUtils.bind(this, function(sender, evt)
		{
			if (evt.getProperty('eventName') == 'mouseDown')
			{
				var me = evt.getProperty('event');
				
				if (!mxEvent.isControlDown(me.getEvent()) && !mxEvent.isShiftDown(me.getEvent()))
		    	{
			    	var state = me.getState();
		
			    	if (state != null)
			    	{
			    		// Checks if state was removed in call to stopEditing above
			    		if (this.model.isEdge(state.cell))
			    		{
			    			start.point = new mxPoint(me.getGraphX(), me.getGraphY());
			    			start.state = state;
			    			start.event = me;
			    			
	    					if (state.text != null && state.text.boundingBox != null &&
	    						mxUtils.contains(state.text.boundingBox, me.getGraphX(), me.getGraphY()))
	    					{
	    						start.handle = mxEvent.LABEL_HANDLE;
	    					}
	    					else
	    					{
				    			var handler = this.selectionCellsHandler.getHandler(state.cell);
	
				    			if (handler != null && handler.bends != null && handler.bends.length > 0)
				    			{
				    				start.handle = handler.getHandleForEvent(me);
				    			}
	    					}
			    		}
			    	}
		    	}
			}
		}));
		
		this.addMouseListener(
		{
			mouseDown: function() {},
		    mouseMove: mxUtils.bind(this, function(sender, me)
		    {
		    	if (!mxEvent.isControlDown(me.getEvent()) && !mxEvent.isShiftDown(me.getEvent()))
		    	{
		    		var tol = this.tolerance;
	
			    	if (start.point != null && start.state != null && start.event != null)
			    	{
			    		var state = start.state;
			    		
			    		if (Math.abs(start.point.x - me.getGraphX()) > tol ||
			    			Math.abs(start.point.y - me.getGraphY()) > tol)
			    		{
			    			var handler = this.selectionCellsHandler.getHandler(state.cell);
			    			
			    			if (handler != null && handler.bends != null && handler.bends.length > 0)
			    			{
			    				var handle = handler.getHandleForEvent(start.event);
			    				var edgeStyle = this.view.getEdgeStyle(state);
			    				var entity = edgeStyle == mxEdgeStyle.EntityRelation;
			    				
	    						if (!entity || handle == 0 || handle == handler.bends.length - 1 || handle == mxEvent.LABEL_HANDLE)
	    						{
				    				// Source or target handle or connected for direct handle access or orthogonal line
				    				// with just two points where the central handle is moved regardless of mouse position
				    				if (handle == mxEvent.LABEL_HANDLE || handle == 0 || state.visibleSourceState != null ||
				    					handle == handler.bends.length - 1 || state.visibleTargetState != null)
				    				{
				    					if (!entity && handle != mxEvent.LABEL_HANDLE)
				    					{
					    					var pts = state.absolutePoints;
				    						
					    					// Handles special case of orthogonal edge styles
					    					if (edgeStyle == mxEdgeStyle.OrthConnector && pts != null)
					    					{
					    						// Does not use handles if they were not initially visible
					    						handle = start.handle;

					    						if (handle == null)
					    						{
							    					var box = new mxRectangle(start.point.x, start.point.y);
							    					box.grow(mxEdgeHandler.prototype.handleImage.width / 2);
							    					
					    							if (mxUtils.contains(box, pts[0].x, pts[0].y))
					    							{
						    							// Moves source terminal handle
					    								handle = 0;
					    							}
					    							else if (mxUtils.contains(box, pts[pts.length - 1].x, pts[pts.length - 1].y))
					    							{
					    								// Moves target terminal handle
					    								handle = handler.bends.length - 1;
					    							}
					    							else
					    							{
							    						// Checks if edge has no bends
							    						var nobends = pts.length == 2 || (pts.length == 3 &&
							    								((Math.round(pts[0].x - pts[1].x) == 0 && Math.round(pts[1].x - pts[2].x) == 0) ||
							    								(Math.round(pts[0].y - pts[1].y) == 0 && Math.round(pts[1].y - pts[2].y) == 0)));
							    						
						    							if (nobends)
								    					{
									    					// Moves central handle for straight orthogonal edges
								    						handle = 2;
								    					}
								    					else
									    				{
										    				// Finds and moves vertical or horizontal segment
									    					handle = mxUtils.findNearestSegment(state, start.point.x, start.point.y) + 1;
									    				}
					    							}
					    						}
					    					}
							    			
						    				// Creates a new waypoint and starts moving it
						    				if (handle == null)
						    				{
						    					handle = mxEvent.VIRTUAL_HANDLE;
						    				}
				    					}
					    				
				    					handler.start(me.getGraphX(), me.getGraphX(), handle);
				    					start.state = null;
				    					start.event = null;
				    					start.point = null;
				    					start.handle = null;
				    					me.consume();
	
				    					// Removes preview rectangle in graph handler
				    					this.graphHandler.reset();
				    				}
	    						}
	    						else if (entity && (state.visibleSourceState != null || state.visibleTargetState != null))
	    						{
	    							// Disables moves on entity to make it consistent
			    					this.graphHandler.reset();
	    							me.consume();
	    						}
			    			}
			    		}
			    	}
			    	else
			    	{
			    		// Updates cursor for unselected edges under the mouse
				    	var state = me.getState();
				    	
				    	if (state != null)
				    	{
				    		// Checks if state was removed in call to stopEditing above
				    		if (this.model.isEdge(state.cell))
				    		{
				    			var cursor = null;
			    				var pts = state.absolutePoints;
			    				
			    				if (pts != null)
			    				{
			    					var box = new mxRectangle(me.getGraphX(), me.getGraphY());
			    					box.grow(mxEdgeHandler.prototype.handleImage.width / 2);
			    					
			    					if (state.text != null && state.text.boundingBox != null &&
			    						mxUtils.contains(state.text.boundingBox, me.getGraphX(), me.getGraphY()))
			    					{
			    						cursor = 'move';
			    					}
			    					else if (mxUtils.contains(box, pts[0].x, pts[0].y) ||
			    						mxUtils.contains(box, pts[pts.length - 1].x, pts[pts.length - 1].y))
			    					{
			    						cursor = 'pointer';
			    					}
			    					else if (state.visibleSourceState != null || state.visibleTargetState != null)
			    					{
		    							// Moving is not allowed for entity relation but still indicate hover state
			    						var tmp = this.view.getEdgeStyle(state);
			    						cursor = 'crosshair';
			    						
			    						if (tmp != mxEdgeStyle.EntityRelation && this.isOrthogonal(state))
						    			{
						    				var idx = mxUtils.findNearestSegment(state, me.getGraphX(), me.getGraphY());
						    				
						    				if (idx < pts.length - 1 && idx >= 0)
						    				{
					    						cursor = (Math.round(pts[idx].x - pts[idx + 1].x) == 0) ?
					    							'col-resize' : 'row-resize';
						    				}
						    			}
			    					}
			    				}
			    				
			    				if (cursor != null)
			    				{
			    					state.setCursor(cursor);
			    				}
				    		}
				    	}
			    	}
		    	}
		    }),
		    mouseUp: mxUtils.bind(this, function(sender, me)
		    {
				start.state = null;
				start.event = null;
				start.point = null;
				start.handle = null;
		    })
		});
	}
	
	// HTML entities are displayed as plain text in wrapped plain text labels
	this.cellRenderer.getLabelValue = function(state)
	{
		var result = mxCellRenderer.prototype.getLabelValue.apply(this, arguments);
		
		if (state.view.graph.isHtmlLabel(state.cell))
		{
			if (state.style['html'] != 1)
			{
				result = mxUtils.htmlEntities(result, false);
			}
			else
			{
				result = state.view.graph.sanitizeHtml(result);
			}
		}
		
		return result;
	};

	// All code below not available and not needed in embed mode
	if (typeof mxVertexHandler !== 'undefined')
	{
		this.setConnectable(true);
		this.setDropEnabled(true);
		this.setPanning(true);
		this.setTooltips(true);
		this.setAllowLoops(true);
		this.allowAutoPanning = true;
		this.resetEdgesOnConnect = false;
		this.constrainChildren = false;
		
		// Do not scroll after moving cells
		this.graphHandler.scrollOnMove = false;
		this.graphHandler.scaleGrid = true;

		// Disables cloning of connection sources by default
		this.connectionHandler.setCreateTarget(false);
		this.connectionHandler.insertBeforeSource = true;
		
		// Disables built-in connection starts
		this.connectionHandler.isValidSource = function(cell, me)
		{
			return false;
		};

		// Sets the style to be used when an elbow edge is double clicked
		this.alternateEdgeStyle = 'vertical';

		if (stylesheet == null)
		{
			this.loadStylesheet();
		}
		
		// Adds page centers to the guides for moving cells
		var graphHandlerGetGuideStates = this.graphHandler.getGuideStates;
		this.graphHandler.getGuideStates = function()
		{
			var result = graphHandlerGetGuideStates.apply(this, arguments);
			
			// Create virtual cell state for page centers
			if (this.graph.pageVisible)
			{
				var guides = [];
				
				var pf = this.graph.pageFormat;
				var ps = this.graph.pageScale;
				var pw = pf.width * ps;
				var ph = pf.height * ps;
				var t = this.graph.view.translate;
				var s = this.graph.view.scale;

				var layout = this.graph.getPageLayout();
				
				for (var i = 0; i < layout.width; i++)
				{
					guides.push(new mxRectangle(((layout.x + i) * pw + t.x) * s,
						(layout.y * ph + t.y) * s, pw * s, ph * s));
				}
				
				for (var j = 0; j < layout.height; j++)
				{
					guides.push(new mxRectangle((layout.x * pw + t.x) * s,
						((layout.y + j) * ph + t.y) * s, pw * s, ph * s));
				}
				
				// Page center guides have predence over normal guides
				result = guides.concat(result);
			}
			
			return result;
		};

		// Overrdes color for virtual guides for page centers
		mxGuide.prototype.getGuideColor = function(state, horizontal)
		{
			return (state.cell == null) ? '#ffa500' /* orange */ : mxConstants.GUIDE_COLOR;
		};

		// Changes color of move preview for black backgrounds
		this.graphHandler.createPreviewShape = function(bounds)
		{
			this.previewColor = (this.graph.background == '#000000') ? 'white' : mxGraphHandler.prototype.previewColor;
			
			return mxGraphHandler.prototype.createPreviewShape.apply(this, arguments);
		};
		
		// Handles parts of cells by checking if part=1 is in the style and returning the parent
		// if the parent is not already in the list of cells. container style is used to disable
		// step into swimlanes and dropTarget style is used to disable acting as a drop target.
		// LATER: Handle recursive parts
		this.graphHandler.getCells = function(initialCell)
		{
		    var cells = mxGraphHandler.prototype.getCells.apply(this, arguments);
		    var newCells = [];

		    for (var i = 0; i < cells.length; i++)
		    {
				var state = this.graph.view.getState(cells[i]);
				var style = (state != null) ? state.style : this.graph.getCellStyle(cells[i]);
		    	
				if (mxUtils.getValue(style, 'part', '0') == '1')
				{
			        var parent = this.graph.model.getParent(cells[i]);
		
			        if (this.graph.model.isVertex(parent) && mxUtils.indexOf(cells, parent) < 0)
			        {
			            newCells.push(parent);
			        }
				}
				else
				{
					newCells.push(cells[i]);
				}
		    }

		    return newCells;
		};

		// Handles parts of cells when cloning the source for new connections
		this.connectionHandler.createTargetVertex = function(evt, source)
		{
			var state = this.graph.view.getState(source);
			var style = (state != null) ? state.style : this.graph.getCellStyle(source);
	    	
			if (mxUtils.getValue(style, 'part', false))
			{
		        var parent = this.graph.model.getParent(source);

		        if (this.graph.model.isVertex(parent))
		        {
		        	source = parent;
		        }
			}
			
			return mxConnectionHandler.prototype.createTargetVertex.apply(this, arguments);
		};
		
	    var rubberband = new mxRubberband(this);
	    
	    this.getRubberband = function()
	    {
	    	return rubberband;
	    };
	    
	    // Timer-based activation of outline connect in connection handler
	    var startTime = new Date().getTime();
	    var timeOnTarget = 0;
	    
	    var connectionHandlerMouseMove = this.connectionHandler.mouseMove;
	    
	    this.connectionHandler.mouseMove = function()
	    {
	    	var prev = this.currentState;
	    	connectionHandlerMouseMove.apply(this, arguments);
	    	
	    	if (prev != this.currentState)
	    	{
	    		startTime = new Date().getTime();
	    		timeOnTarget = 0;
	    	}
	    	else
	    	{
		    	timeOnTarget = new Date().getTime() - startTime;
	    	}
	    };

	    // Activates outline connect after 500ms or if alt is pressed
	    var connectionHandleIsOutlineConnectEvent = this.connectionHandler.isOutlineConnectEvent;
	    
	    this.connectionHandler.isOutlineConnectEvent = function(me)
	    {
	    	return timeOnTarget > 1500 || ((mxEvent.isAltDown(me.getEvent()) || timeOnTarget > 500) &&
	    		connectionHandleIsOutlineConnectEvent.apply(this, arguments));
	    };
	    
	    // Adds shift+click to toggle selection state
	    var isToggleEvent = this.isToggleEvent;
	    this.isToggleEvent = function(evt)
	    {
	    	return isToggleEvent.apply(this, arguments) || mxEvent.isShiftDown(evt);
	    };
	    
	    // Workaround for Firefox where first mouse down is received
	    // after tap and hold if scrollbars are visible, which means
	    // start rubberband immediately if no cell is under mouse.
	    var isForceRubberBandEvent = rubberband.isForceRubberbandEvent;
	    rubberband.isForceRubberbandEvent = function(me)
	    {
	    	return isForceRubberBandEvent.apply(this, arguments) ||
	    		(mxUtils.hasScrollbars(this.graph.container) && mxClient.IS_FF &&
	    		mxClient.IS_WIN && me.getState() == null && mxEvent.isTouchEvent(me.getEvent()));
	    };
	    
	    // Shows hand cursor while panning
	    var prevCursor = null;
	    
		this.panningHandler.addListener(mxEvent.PAN_START, mxUtils.bind(this, function()
		{
			prevCursor = this.container.style.cursor;
			this.container.style.cursor = 'move';
		}));
			
		this.panningHandler.addListener(mxEvent.PAN_END, mxUtils.bind(this, function()
		{
			this.container.style.cursor = prevCursor;
		}));

		this.popupMenuHandler.autoExpand = true;
		
		this.popupMenuHandler.isSelectOnPopup = function(me)
		{
			return mxEvent.isMouseEvent(me.getEvent());
		};
	
		// Enables links if graph is "disabled" (ie. read-only)
		var click = this.click;
		this.click = function(me)
		{
			if (!this.isEnabled() && !me.isConsumed())
			{
				var cell = me.getCell();
				
				if (cell != null)
				{
					var link = this.getLinkForCell(cell);
					
					if (link != null)
					{
						window.open(link);
					}
				}
			}
			else
			{
				return click.apply(this, arguments);
			}
		};
		
		// Shows pointer cursor for clickable cells with links
		// ie. if the graph is disabled and cells cannot be selected
		var getCursorForCell = this.getCursorForCell;
		this.getCursorForCell = function(cell)
		{
			if (!this.isEnabled())
			{
				var link = this.getLinkForCell(cell);
				
				if (link != null)
				{
					return 'pointer';
				}
			}
			else
			{
				return getCursorForCell.apply(this, arguments);
			}
		};
		
		// Changes rubberband selection to be recursive
		this.selectRegion = function(rect, evt)
		{
			var cells = this.getAllCells(rect.x, rect.y, rect.width, rect.height);
			this.selectCellsForEvent(cells, evt);
			
			return cells;
		};
		
		// Recursive implementation for rubberband selection
		this.getAllCells = function(x, y, width, height, parent, result)
		{
			result = (result != null) ? result : [];
			
			if (width > 0 || height > 0)
			{
				var model = this.getModel();
				var right = x + width;
				var bottom = y + height;
	
				if (parent == null)
				{
					parent = this.getCurrentRoot();
					
					if (parent == null)
					{
						parent = model.getRoot();
					}
				}
				
				if (parent != null)
				{
					var childCount = model.getChildCount(parent);
					
					for (var i = 0; i < childCount; i++)
					{
						var cell = model.getChildAt(parent, i);
						var state = this.view.getState(cell);
						
						if (state != null && this.isCellVisible(cell) && mxUtils.getValue(state.style, 'locked', '0') != '1')
						{
							var deg = mxUtils.getValue(state.style, mxConstants.STYLE_ROTATION) || 0;
							var box = state;
							
							if (deg != 0)
							{
								box = mxUtils.getBoundingBox(box, deg);
							}
							
							if ((model.isEdge(cell) || model.isVertex(cell)) &&
								box.x >= x && box.y + box.height <= bottom &&
								box.y >= y && box.x + box.width <= right)
							{
								result.push(cell);
							}
	
							this.getAllCells(x, y, width, height, cell, result);
						}
					}
				}
			}
			
			return result;
		};

		// Never removes cells from parents that are being moved
		var graphHandlerShouldRemoveCellsFromParent = this.graphHandler.shouldRemoveCellsFromParent;
		this.graphHandler.shouldRemoveCellsFromParent = function(parent, cells, evt)
		{
			if (this.graph.isCellSelected(parent))
			{
				return false;
			}
			
			return graphHandlerShouldRemoveCellsFromParent.apply(this, arguments);
		};

		// Unlocks all cells
		this.isCellLocked = function(cell)
		{
			var pState = this.view.getState(cell);
			
			while (pState != null)
			{
				if (mxUtils.getValue(pState.style, 'locked', '0') == '1')
				{
					return true;
				}
				
				pState = this.view.getState(this.model.getParent(pState.cell));
			}
			
			return false;
		};
		
		// Tap and hold on background starts rubberband for multiple selected
		// cells the cell associated with the event is deselected
		this.addListener(mxEvent.TAP_AND_HOLD, mxUtils.bind(this, function(sender, evt)
		{
			if (!mxEvent.isMultiTouchEvent(evt))
			{
				var me = evt.getProperty('event');
				var cell = evt.getProperty('cell');
				
				if (cell == null)
				{
					var pt = mxUtils.convertPoint(this.container,
							mxEvent.getClientX(me), mxEvent.getClientY(me));
					rubberband.start(pt.x, pt.y);
				}
				else if (this.getSelectionCount() > 1 && this.isCellSelected(cell))
				{
					this.removeSelectionCell(cell);
				}
				
				// Blocks further processing of the event
				evt.consume();
			}
		}));
	
		// On connect the target is selected and we clone the cell of the preview edge for insert
		this.connectionHandler.selectCells = function(edge, target)
		{
			this.graph.setSelectionCell(target || edge);
		};
		
		// Shows connection points only if cell not selected
		this.connectionHandler.constraintHandler.isStateIgnored = function(state, source)
		{
			return source && state.view.graph.isCellSelected(state.cell);
		};
		
		// Updates constraint handler if the selection changes
		this.selectionModel.addListener(mxEvent.CHANGE, mxUtils.bind(this, function()
		{
			var ch = this.connectionHandler.constraintHandler;
			
			if (ch.currentFocus != null && ch.isStateIgnored(ch.currentFocus, true))
			{
				ch.currentFocus = null;
				ch.constraints = null;
				ch.destroyIcons();
			}
			
			ch.destroyFocusHighlight();
		}));
		
		// Initializes touch interface
		if (touchStyle)
		{
			this.initTouch();
		}
		
		/**
		 * Adds locking
		 */
		var graphUpdateMouseEvent = this.updateMouseEvent;
		this.updateMouseEvent = function(me)
		{
			me = graphUpdateMouseEvent.apply(this, arguments);
			
			if (this.isCellLocked(me.getCell()))
			{
				me.state = null;
			}
			
			return me;
		};
	}
};

// Graph inherits from mxGraph
mxUtils.extend(Graph, mxGraph);

/**
 * Allows all values in fit.
 */
Graph.prototype.minFitScale = null;

/**
 * Allows all values in fit.
 */
Graph.prototype.maxFitScale = null;

/**
 * Sets the default target for all links in cells.
 */
Graph.prototype.linkTarget = '_blank';

/**
 * Sets the default target for all links in cells.
 */
Graph.prototype.defaultEdgeLength = 100;

/**
 * Allows all values in fit.
 */
Graph.prototype.edgeMode = false;

/**
 * Allows all values in fit.
 */
Graph.prototype.connectionArrowsEnabled = true;

/**
 * Specifies the regular expression for matching placeholders.
 */
Graph.prototype.placeholderPattern = new RegExp('%(date\{.*\}|[^%^\{^\}]+)%', 'g');

/**
 * Installs child layout styles.
 */
Graph.prototype.init = function(container)
{
	mxGraph.prototype.init.apply(this, arguments);

	this.initLayoutManager();
};

/**
 * Installs automatic layout via styles
 */
Graph.prototype.initLayoutManager = function()
{
	this.layoutManager = new mxLayoutManager(this);

	this.layoutManager.getLayout = function(cell)
	{
		var state = this.graph.view.getState(cell);
		var style = (state != null) ? state.style : this.graph.getCellStyle(cell);
		
		if (style['childLayout'] == 'stackLayout')
		{
			var stackLayout = new mxStackLayout(this.graph, true);
			stackLayout.resizeParentMax = true;
			stackLayout.horizontal = mxUtils.getValue(style, 'horizontalStack', '1') == '1';
			stackLayout.resizeParent = mxUtils.getValue(style, 'resizeParent', '1') == '1';
			stackLayout.resizeLast = mxUtils.getValue(style, 'resizeLast', '0') == '1';
			stackLayout.marginLeft = style['marginLeft'] || 0;
			stackLayout.marginRight = style['marginRight'] || 0;
			stackLayout.marginTop = style['marginTop'] || 0;
			stackLayout.marginBottom = style['marginBottom'] || 0;
			stackLayout.fill = true;
			
			return stackLayout;
		}
		else if (style['childLayout'] == 'treeLayout')
		{
			var treeLayout = new mxCompactTreeLayout(this.graph);
			treeLayout.horizontal = mxUtils.getValue(style, 'horizontalTree', '1') == '1';
			treeLayout.resizeParent = mxUtils.getValue(style, 'resizeParent', '1') == '1';
			treeLayout.groupPadding = mxUtils.getValue(style, 'parentPadding', 20);
			treeLayout.levelDistance = mxUtils.getValue(style, 'treeLevelDistance', 30);
			treeLayout.maintainParentLocation = true;
			treeLayout.edgeRouting = false;
			treeLayout.resetEdges = false;
			
			return treeLayout;
		}
		else if (style['childLayout'] == 'flowLayout')
		{
			var flowLayout = new mxHierarchicalLayout(this.graph, mxUtils.getValue(style,
					'flowOrientation', mxConstants.DIRECTION_EAST));
			flowLayout.resizeParent = mxUtils.getValue(style, 'resizeParent', '1') == '1';
			flowLayout.parentBorder = mxUtils.getValue(style, 'parentPadding', 20);
			flowLayout.maintainParentLocation = true;
			
			// Special undocumented styles for changing the hierarchical
			flowLayout.intraCellSpacing = mxUtils.getValue(style, 'intraCellSpacing', mxHierarchicalLayout.prototype.intraCellSpacing);
			flowLayout.interRankCellSpacing = mxUtils.getValue(style, 'interRankCellSpacing', mxHierarchicalLayout.prototype.interRankCellSpacing);
			flowLayout.interHierarchySpacing = mxUtils.getValue(style, 'interHierarchySpacing', mxHierarchicalLayout.prototype.interHierarchySpacing);
			flowLayout.parallelEdgeSpacing = mxUtils.getValue(style, 'parallelEdgeSpacing', mxHierarchicalLayout.prototype.parallelEdgeSpacing);
			
			return flowLayout;
		}
		
		return null;
	};
};

/**
 * Sanitizes the given HTML markup.
 */
Graph.prototype.sanitizeHtml = function(value)
{
	// Uses https://code.google.com/p/google-caja/wiki/JsHtmlSanitizer
	// TODO: Add MathML to whitelisted tags, add data URIs for images
	function urlX(url) { if(/^https?:\/\//.test(url)) { return url }}
    function idX(id) { return id }
	
	return html_sanitize(value, urlX, idX);
};

/**
 * Adds support for placeholders in labels.
 */
Graph.prototype.isReplacePlaceholders = function(cell)
{
	return cell.value != null && typeof(cell.value) == 'object' &&
		cell.value.getAttribute('placeholders') == '1';
};

/**
 * Adds support for placeholders in labels.
 */
Graph.prototype.isSplitTarget = function(target, cells, evt)
{
	return !mxEvent.isShiftDown(evt) && mxGraph.prototype.isSplitTarget.apply(this, arguments);
};

/**
 * Adds support for placeholders in labels.
 */
Graph.prototype.getLabel = function(cell)
{
	var result = mxGraph.prototype.getLabel.apply(this, arguments);
	
	if (result != null && this.isReplacePlaceholders(cell))
	{
		result = this.replacePlaceholders(cell, result);
	}
	
	return result;
};

/**
 * Private helper method.
 */
Graph.prototype.getGlobalVariable = function(name)
{
	var val = null;
	
	if (name == 'date')
	{
		val = new Date().toLocaleDateString();
	}
	else if (name == 'time')
	{
		val = new Date().toLocaleTimeString();
	}
	else if (name == 'timestamp')
	{
		val = new Date().toLocaleString();
	}
	else if (name.substring(0, 5) == 'date{')
	{
		var fmt = name.substring(5, name.length - 1);
		val = this.formatDate(new Date(), fmt);
	}

	return val;
};

/**
 * Formats a date, see http://blog.stevenlevithan.com/archives/date-time-format
 */
Graph.prototype.formatDate = function(date, mask, utc)
{
	// LATER: Cache regexs
	if (this.dateFormatCache == null)
	{
		this.dateFormatCache = {
			i18n: {
			    dayNames: [
			        "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat",
			        "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"
			    ],
			    monthNames: [
			        "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec",
			        "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"
			    ]
			},
			
			masks: {
			    "default":      "ddd mmm dd yyyy HH:MM:ss",
			    shortDate:      "m/d/yy",
			    mediumDate:     "mmm d, yyyy",
			    longDate:       "mmmm d, yyyy",
			    fullDate:       "dddd, mmmm d, yyyy",
			    shortTime:      "h:MM TT",
			    mediumTime:     "h:MM:ss TT",
			    longTime:       "h:MM:ss TT Z",
			    isoDate:        "yyyy-mm-dd",
			    isoTime:        "HH:MM:ss",
			    isoDateTime:    "yyyy-mm-dd'T'HH:MM:ss",
			    isoUtcDateTime: "UTC:yyyy-mm-dd'T'HH:MM:ss'Z'"
			}
		};
	}
    
    var dF = this.dateFormatCache;
	var token = /d{1,4}|m{1,4}|yy(?:yy)?|([HhMsTt])\1?|[LloSZ]|"[^"]*"|'[^']*'/g,
    	timezone = /\b(?:[PMCEA][SDP]T|(?:Pacific|Mountain|Central|Eastern|Atlantic) (?:Standard|Daylight|Prevailing) Time|(?:GMT|UTC)(?:[-+]\d{4})?)\b/g,
    	timezoneClip = /[^-+\dA-Z]/g,
    	pad = function (val, len) {
			val = String(val);
			len = len || 2;
			while (val.length < len) val = "0" + val;
			return val;
		};

    // You can't provide utc if you skip other args (use the "UTC:" mask prefix)
    if (arguments.length == 1 && Object.prototype.toString.call(date) == "[object String]" && !/\d/.test(date)) {
        mask = date;
        date = undefined;
    }

    // Passing date through Date applies Date.parse, if necessary
    date = date ? new Date(date) : new Date;
    if (isNaN(date)) throw SyntaxError("invalid date");

    mask = String(dF.masks[mask] || mask || dF.masks["default"]);

    // Allow setting the utc argument via the mask
    if (mask.slice(0, 4) == "UTC:") {
        mask = mask.slice(4);
        utc = true;
    }

    var _ = utc ? "getUTC" : "get",
        d = date[_ + "Date"](),
        D = date[_ + "Day"](),
        m = date[_ + "Month"](),
        y = date[_ + "FullYear"](),
        H = date[_ + "Hours"](),
        M = date[_ + "Minutes"](),
        s = date[_ + "Seconds"](),
        L = date[_ + "Milliseconds"](),
        o = utc ? 0 : date.getTimezoneOffset(),
        flags = {
            d:    d,
            dd:   pad(d),
            ddd:  dF.i18n.dayNames[D],
            dddd: dF.i18n.dayNames[D + 7],
            m:    m + 1,
            mm:   pad(m + 1),
            mmm:  dF.i18n.monthNames[m],
            mmmm: dF.i18n.monthNames[m + 12],
            yy:   String(y).slice(2),
            yyyy: y,
            h:    H % 12 || 12,
            hh:   pad(H % 12 || 12),
            H:    H,
            HH:   pad(H),
            M:    M,
            MM:   pad(M),
            s:    s,
            ss:   pad(s),
            l:    pad(L, 3),
            L:    pad(L > 99 ? Math.round(L / 10) : L),
            t:    H < 12 ? "a"  : "p",
            tt:   H < 12 ? "am" : "pm",
            T:    H < 12 ? "A"  : "P",
            TT:   H < 12 ? "AM" : "PM",
            Z:    utc ? "UTC" : (String(date).match(timezone) || [""]).pop().replace(timezoneClip, ""),
            o:    (o > 0 ? "-" : "+") + pad(Math.floor(Math.abs(o) / 60) * 100 + Math.abs(o) % 60, 4),
            S:    ["th", "st", "nd", "rd"][d % 10 > 3 ? 0 : (d % 100 - d % 10 != 10) * d % 10]
        };

    return mask.replace(token, function ($0)
    {
        return $0 in flags ? flags[$0] : $0.slice(1, $0.length - 1);
    });
};

/**
 * Private helper method.
 */
Graph.prototype.replacePlaceholders = function(cell, str)
{
	var result = [];
	var last = 0;
	
	while (match = this.placeholderPattern.exec(str))
	{
		var val = match[0];
		
		if (val.length > 2 && val != '%label%' && val != '%tooltip%')
		{
			var tmp = null;

			if (match.index > last && str.charAt(match.index - 1) == '%')
			{
				tmp = val.substring(1);
			}
			else
			{
				var name = val.substring(1, val.length - 1);
				var current = cell;
				
				while (tmp == null && current != null)
				{
					if (current.value != null && typeof(current.value) == 'object')
					{
						tmp = current.value.getAttribute(name);
					}
					
					current = this.model.getParent(current);
				}
				
				if (tmp == null)
				{
					tmp = this.getGlobalVariable(name);
				}
			}

			result.push(str.substring(last, match.index) + (tmp || val));
			last = match.index + val.length;
		}
	}
	
	result.push(str.substring(last));

	return result.join('');
};

/**
 * Selects cells for connect vertex return value.
 */
Graph.prototype.selectCellsForConnectVertex = function(cells, evt, hoverIcons)
{
	// Selects only target vertex if one exists
	if (cells.length == 2 && this.model.isVertex(cells[1]))
	{
		this.setSelectionCell(cells[1]);
		
		if (hoverIcons != null)
		{
			// Adds hover icons to new target vertex for touch devices
			if (mxEvent.isTouchEvent(evt))
			{
				hoverIcons.update(hoverIcons.getState(this.view.getState(cells[1])));
			}
			else
			{
				// Hides hover icons after click with mouse
				hoverIcons.reset();
			}
		}
		
		this.scrollCellToVisible(cells[1]);
	}
	else
	{
		this.setSelectionCells(cells);
	}
};

/**
 * Adds a connection to the given vertex.
 */
Graph.prototype.connectVertex = function(source, direction, length, evt)
{
	var pt = (source.geometry.relative) ? new mxPoint(source.parent.geometry.width * source.geometry.x,
			source.parent.geometry.height * source.geometry.y) : new mxPoint(source.geometry.x, source.geometry.y);
		
	if (direction == mxConstants.DIRECTION_NORTH)
	{
		pt.x += source.geometry.width / 2;
		pt.y -= length ;
	}
	else if (direction == mxConstants.DIRECTION_SOUTH)
	{
		pt.x += source.geometry.width / 2;
		pt.y += source.geometry.height + length;
	}
	else if (direction == mxConstants.DIRECTION_WEST)
	{
		pt.x -= length;
		pt.y += source.geometry.height / 2;
	}
	else
	{
		pt.x += source.geometry.width + length;
		pt.y += source.geometry.height / 2;
	}

	var parentState = this.view.getState(this.model.getParent(source));
	var s = this.view.scale;
	var t = this.view.translate;
	var dx = t.x * s;
	var dy = t.y * s;
	
	if (this.model.isVertex(parentState.cell))
	{
		dx = parentState.x;
		dy = parentState.y;
	}

	// Workaround for relative child cells
	if (this.model.isVertex(source.parent) && source.geometry.relative)
	{
		pt.x += source.parent.geometry.x;
		pt.y += source.parent.geometry.y;
	}
	
	// Checks actual end point of edge for target cell
	var target = (mxEvent.isControlDown(evt)) ? null : this.getCellAt(dx + pt.x * s, dy + pt.y * s);
	
	if (this.model.isAncestor(target, source))
	{
		target = null;
	}
	
	// Checks if target or ancestor is locked
	var temp = target;
	
	while (temp != null)
	{
		if (this.isCellLocked(temp))
		{
			target = null;
			break;
		}
		
		temp = this.model.getParent(temp);
	}
	
	// Checks if source and target intersect
	if (target != null)
	{
		var sourceState = this.view.getState(source);
		var targetState = this.view.getState(target);
		
		if (sourceState != null && targetState != null && mxUtils.intersects(sourceState, targetState))
		{
			target = null;
		}
	}
	
	var duplicate = !mxEvent.isShiftDown(evt);
	
	if (duplicate)
	{
		if (direction == mxConstants.DIRECTION_NORTH)
		{
			pt.y -= source.geometry.height / 2;
		}
		else if (direction == mxConstants.DIRECTION_SOUTH)
		{
			pt.y += source.geometry.height / 2;
		}
		else if (direction == mxConstants.DIRECTION_WEST)
		{
			pt.x -= source.geometry.width / 2;
		}
		else
		{
			pt.x += source.geometry.width / 2;
		}
	}

	// Uses connectable parent vertex if one exists
	if (target != null && !this.isCellConnectable(target))
	{
		var parent = this.getModel().getParent(target);
		
		if (this.getModel().isVertex(parent) && this.isCellConnectable(parent))
		{
			target = parent;
		}
	}
	
	if (target == source || this.model.isEdge(target) || !this.isCellConnectable(target))
	{
		target = null;
	}
	
	var result = [];
	
	this.model.beginUpdate();
	try
	{
		var realTarget = target;
		
		if (realTarget == null && duplicate)
		{
			// Handles relative children
			var cellToClone = source;
			var geo = this.getCellGeometry(source);
			
			while (geo != null && geo.relative)
			{
				cellToClone = this.getModel().getParent(cellToClone);
				geo = this.getCellGeometry(cellToClone);
			}
			
			// Handle consistuents for cloning
			var state = this.view.getState(cellToClone);
			var style = (state != null) ? state.style : this.getCellStyle(cellToClone);
	    	
			if (mxUtils.getValue(style, 'part', false))
			{
		        var tmpParent = this.model.getParent(cellToClone);

		        if (this.model.isVertex(tmpParent))
		        {
		        	cellToClone = tmpParent;
		        }
			}
			
			realTarget = this.duplicateCells([cellToClone], false)[0];
			
			var geo = this.getCellGeometry(realTarget);
			geo.x = pt.x - geo.width / 2;
			geo.y = pt.y - geo.height / 2;
		}
		
		// Never connects children in stack layouts
		var layout = null;

		if (this.layoutManager != null)
		{
			layout = this.layoutManager.getLayout(this.model.getParent(source));
		}
		
		var edge = ((mxEvent.isControlDown(evt) && duplicate) || (target == null && layout != null && layout.constructor == mxStackLayout)) ? null :
			this.insertEdge(this.model.getParent(source), null, '', source, realTarget, this.createCurrentEdgeStyle());

		// Inserts edge before source
		if (edge != null && this.connectionHandler.insertBeforeSource)
		{
			var index = null;
			var tmp = source;
			
			while (tmp.parent != null && tmp.geometry != null &&
				tmp.geometry.relative && tmp.parent != edge.parent)
			{
				tmp = this.model.getParent(tmp);
			}
		
			if (tmp != null && tmp.parent != null && tmp.parent == edge.parent)
			{
				var index = tmp.parent.getIndex(tmp);
				tmp.parent.insert(edge, index);
			}
		}
		
		// Special case: Click on west icon puts clone before cell
		if (target == null && realTarget != null && layout != null && source.parent != null &&
			layout.constructor == mxStackLayout && direction == mxConstants.DIRECTION_WEST)
		{
			var index = source.parent.getIndex(source);
			source.parent.insert(realTarget, index);
		}
		
		if (edge != null)
		{
			// Uses elbow edges with vertical or horizontal direction
//			var elbowValue = (direction == mxConstants.DIRECTION_NORTH || direction == mxConstants.DIRECTION_SOUTH) ? 'vertical' : 'horizontal';
//			edge.style = mxUtils.setStyle(edge.style, 'edgeStyle', 'elbowEdgeStyle');
//			edge.style = mxUtils.setStyle(edge.style, 'elbow', elbowValue);
			result.push(edge);
		}
		
		if (target == null && realTarget != null)
		{
			result.push(realTarget);
		}
		
		if (realTarget == null && edge != null)
		{
			edge.geometry.setTerminalPoint(pt, false);
		}
		
		if (edge != null)
		{
			this.fireEvent(new mxEventObject('cellsInserted', 'cells', [edge]));
		}
	}
	finally
	{
		this.model.endUpdate();
	}
	
	return result;
};

/**
 * Returns the label for the given cell.
 */
Graph.prototype.convertValueToString = function(cell)
{
	if (cell.value != null && typeof(cell.value) == 'object')
	{
		return cell.value.getAttribute('label');
	}
	
	return mxGraph.prototype.convertValueToString.apply(this, arguments);
};

/**
 * Returns the link for the given cell.
 */
Graph.prototype.getLinkForCell = function(cell)
{
	if (cell.value != null && typeof(cell.value) == 'object')
	{
		return cell.value.getAttribute('link');
	}
	
	return null;
};

/**
 * Overrides label orientation for collapsed swimlanes inside stack.
 */
Graph.prototype.getCellStyle = function(cell)
{
	var style = mxGraph.prototype.getCellStyle.apply(this, arguments);
	
	if (cell != null && this.layoutManager != null)
	{
		var parent = this.model.getParent(cell);
		
		if (this.model.isVertex(parent) && this.isCellCollapsed(cell))
		{
			var layout = this.layoutManager.getLayout(parent);
			
			if (layout != null && layout.constructor == mxStackLayout)
			{
				style[mxConstants.STYLE_HORIZONTAL] = !layout.horizontal;
			}
		}
	}
	
	return style;
};

/**
 * Disables alternate width persistence for stack layout parents
 */
Graph.prototype.updateAlternateBounds = function(cell, geo, willCollapse)
{
	if (cell != null && geo != null && this.layoutManager != null && geo.alternateBounds != null)
	{
		var layout = this.layoutManager.getLayout(this.model.getParent(cell));
		
		if (layout != null && layout.constructor == mxStackLayout)
		{
			if (layout.horizontal)
			{
				geo.alternateBounds.height = 0;
			}
			else
			{
				geo.alternateBounds.width = 0;
			}
		}
	}
	
	mxGraph.prototype.updateAlternateBounds.apply(this, arguments);
};

/**
 * Adds Shift+collapse/expand and size management for folding inside stack
 */
Graph.prototype.foldCells = function(collapse, recurse, cells, checkFoldable, evt)
{
	recurse = (recurse != null) ? recurse : false;
	
	if (cells == null)
	{
		cells = this.getFoldableCells(this.getSelectionCells(), collapse);
	}
	
	if (cells != null)
	{
		this.model.beginUpdate();
		
		try
		{
			mxGraph.prototype.foldCells.apply(this, arguments);
			
			// Resizes all parent stacks if alt is not pressed
			if (this.layoutManager != null)
			{
				for (var i = 0; i < cells.length; i++)
				{
					var state = this.view.getState(cells[i]);
					var geo = this.getCellGeometry(cells[i]);
					
					if (state != null && geo != null)
					{
						var dx = Math.round(geo.width - state.width / this.view.scale);
						var dy = Math.round(geo.height - state.height / this.view.scale);
						
						if (dy != 0 || dx != 0)
						{
							var parent = this.model.getParent(cells[i]);
							var layout = this.layoutManager.getLayout(parent);
							
							if (layout == null)
							{
								// Moves cells to the right and down after collapse/expand
								if (evt != null && mxEvent.isShiftDown(evt))
								{
									this.moveSiblings(state, parent, dx, dy);
								} 
							}
							else if ((evt == null || !mxEvent.isAltDown(evt)) && layout.constructor == mxStackLayout && !layout.resizeLast)
							{
								this.resizeParentStacks(parent, layout, dx, dy);
							}
						}
					}
				}
			}
		}
		finally
		{
			this.model.endUpdate();
		}
		
		// Selects cells after folding
		if (this.isEnabled())
		{
			this.setSelectionCells(cells);
		}
	}
};

/**
 * Overrides label orientation for collapsed swimlanes inside stack.
 */
Graph.prototype.moveSiblings = function(state, parent, dx, dy)
{
	this.model.beginUpdate();
	try
	{
		var cells = this.getCellsBeyond(state.x, state.y, parent, true, true);
		
		for (var i = 0; i < cells.length; i++)
		{
			if (cells[i] != state.cell)
			{
				var tmp = this.view.getState(cells[i]);
				var geo = this.getCellGeometry(cells[i]);
				
				if (tmp != null && geo != null)
				{
					geo = geo.clone();
					geo.translate(Math.round(dx * Math.max(0, Math.min(1, (tmp.x - state.x) / state.width))),
						Math.round(dy * Math.max(0, Math.min(1, (tmp.y - state.y) / state.height))));
					this.model.setGeometry(cells[i], geo);
				}
			}
		}
	}
	finally
	{
		this.model.endUpdate();
	}
};

/**
 * Overrides label orientation for collapsed swimlanes inside stack.
 */
Graph.prototype.resizeParentStacks = function(parent, layout, dx, dy)
{
	if (this.layoutManager != null && layout != null && layout.constructor == mxStackLayout && !layout.resizeLast)
	{
		this.model.beginUpdate();
		try
		{
			var dir = layout.horizontal;
			
			// Bubble resize up for all parent stack layouts with same orientation
			while (parent != null && layout != null && layout.constructor == mxStackLayout &&
				layout.horizontal == dir && !layout.resizeLast)
			{
				var pgeo = this.getCellGeometry(parent);
				var pstate = this.view.getState(parent);
				
				if (pstate != null && pgeo != null)
				{
					pgeo = pgeo.clone();
					
					if (layout.horizontal)
					{
						pgeo.width += dx + Math.min(0, pstate.width / this.view.scale - pgeo.width);									
					}
					else
					{
						pgeo.height += dy + Math.min(0, pstate.height / this.view.scale - pgeo.height);
					}
		
					this.model.setGeometry(parent, pgeo);
				}
				
				parent = this.model.getParent(parent);
				layout = this.layoutManager.getLayout(parent);
			}
		}
		finally
		{
			this.model.endUpdate();
		}
	}
};

/**
 * Disables drill-down for non-swimlanes.
 */
Graph.prototype.isContainer = function(cell)
{
	var state = this.view.getState(cell);
	var style = (state != null) ? state.style : this.getCellStyle(cell);
	
	if (this.isSwimlane(cell))
	{
		return style['container'] != '0';
	}
	else
	{
		return style['container'] == '1';
	}
};

/**
 * Adds a connectable style.
 */
Graph.prototype.isCellConnectable = function(cell)
{
	var state = this.view.getState(cell);
	var style = (state != null) ? state.style : this.getCellStyle(cell);
	
	return (style['connectable'] != null) ? style['connectable']  != '0' :
		mxGraph.prototype.isCellConnectable.apply(this, arguments);
};

/**
 * Function: selectAll
 * 
 * Selects all children of the given parent cell or the children of the
 * default parent if no parent is specified. To select leaf vertices and/or
 * edges use <selectCells>.
 * 
 * Parameters:
 * 
 * parent - Optional <mxCell> whose children should be selected.
 * Default is <defaultParent>.
 */
Graph.prototype.selectAll = function(parent)
{
	parent = parent || this.getDefaultParent();

	if (!this.isCellLocked(parent))
	{
		mxGraph.prototype.selectAll.apply(this, arguments);
	}
};

/**
 * Function: selectCells
 * 
 * Selects all vertices and/or edges depending on the given boolean
 * arguments recursively, starting at the given parent or the default
 * parent if no parent is specified. Use <selectAll> to select all cells.
 * For vertices, only cells with no children are selected.
 * 
 * Parameters:
 * 
 * vertices - Boolean indicating if vertices should be selected.
 * edges - Boolean indicating if edges should be selected.
 * parent - Optional <mxCell> that acts as the root of the recursion.
 * Default is <defaultParent>.
 */
Graph.prototype.selectCells = function(vertices, edges, parent)
{
	parent = parent || this.getDefaultParent();

	if (!this.isCellLocked(parent))
	{
		mxGraph.prototype.selectCells.apply(this, arguments);
	}
};

/**
 * Function: getSwimlaneAt
 * 
 * Returns the bottom-most swimlane that intersects the given point (x, y)
 * in the cell hierarchy that starts at the given parent.
 * 
 * Parameters:
 * 
 * x - X-coordinate of the location to be checked.
 * y - Y-coordinate of the location to be checked.
 * parent - <mxCell> that should be used as the root of the recursion.
 * Default is <defaultParent>.
 */
Graph.prototype.getSwimlaneAt = function (x, y, parent)
{
	parent = parent || this.getDefaultParent();

	if (!this.isCellLocked(parent))
	{
		return mxGraph.prototype.getSwimlaneAt.apply(this, arguments);
	}
	
	return null;
};

/**
 * Disables folding for non-swimlanes.
 */
Graph.prototype.isCellFoldable = function(cell)
{
	var state = this.view.getState(cell);
	var style = (state != null) ? state.style : this.getCellStyle(cell);
	
	return this.foldingEnabled && !this.isCellLocked(cell) &&
		((this.isContainer(cell) && style['collapsible'] != '0') ||
		(!this.isContainer(cell) && style['collapsible'] == '1'));
};

/**
 * Stops all interactions and clears the selection.
 */
Graph.prototype.reset = function()
{
	if (this.isEditing())
	{
		this.stopEditing(true);
	}
	
	this.escape();
					
	if (!this.isSelectionEmpty())
	{
		this.clearSelection();
	}
};

/**
 * Overridden to limit zoom to 160x.
 */
Graph.prototype.zoom = function(factor, center)
{
	factor = Math.min(this.view.scale * factor, 160) / this.view.scale;
	
	mxGraph.prototype.zoom.apply(this, arguments);
};

/**
 * Hover icons are used for hover, vertex handler and drag from sidebar.
 */
HoverIcons = function(graph)
{
	this.graph = graph;
	this.init();
};

/**
 * Up arrow.
 */
HoverIcons.prototype.arrowSpacing = 6;

/**
 * Delay to switch to another state of overlapping bbox. Default is 500ms.
 */
HoverIcons.prototype.updateDelay = 500;

/**
 * Up arrow.
 */
HoverIcons.prototype.currentState = null;

/**
 * Up arrow.
 */
HoverIcons.prototype.activeArrow = null;

/**
 * Up arrow.
 */
HoverIcons.prototype.triangleUp = new mxImage((mxClient.IS_SVG) ? 'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABoAAAAaCAYAAACpSkzOAAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAAyJpVFh0WE1MOmNvbS5hZG9iZS54bXAAAAAAADw/eHBhY2tldCBiZWdpbj0i77u/IiBpZD0iVzVNME1wQ2VoaUh6cmVTek5UY3prYzlkIj8+IDx4OnhtcG1ldGEgeG1sbnM6eD0iYWRvYmU6bnM6bWV0YS8iIHg6eG1wdGs9IkFkb2JlIFhNUCBDb3JlIDUuMC1jMDYwIDYxLjEzNDc3NywgMjAxMC8wMi8xMi0xNzozMjowMCAgICAgICAgIj4gPHJkZjpSREYgeG1sbnM6cmRmPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5LzAyLzIyLXJkZi1zeW50YXgtbnMjIj4gPHJkZjpEZXNjcmlwdGlvbiByZGY6YWJvdXQ9IiIgeG1sbnM6eG1wPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvIiB4bWxuczp4bXBNTT0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wL21tLyIgeG1sbnM6c3RSZWY9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9zVHlwZS9SZXNvdXJjZVJlZiMiIHhtcDpDcmVhdG9yVG9vbD0iQWRvYmUgUGhvdG9zaG9wIENTNSBNYWNpbnRvc2giIHhtcE1NOkluc3RhbmNlSUQ9InhtcC5paWQ6NDA3RjREMDM0NTVCMTFFNEIxOTZFRjE3NzRENjQ0RjIiIHhtcE1NOkRvY3VtZW50SUQ9InhtcC5kaWQ6NDA3RjREMDQ0NTVCMTFFNEIxOTZFRjE3NzRENjQ0RjIiPiA8eG1wTU06RGVyaXZlZEZyb20gc3RSZWY6aW5zdGFuY2VJRD0ieG1wLmlpZDo0MDdGNEQwMTQ1NUIxMUU0QjE5NkVGMTc3NEQ2NDRGMiIgc3RSZWY6ZG9jdW1lbnRJRD0ieG1wLmRpZDo0MDdGNEQwMjQ1NUIxMUU0QjE5NkVGMTc3NEQ2NDRGMiIvPiA8L3JkZjpEZXNjcmlwdGlvbj4gPC9yZGY6UkRGPiA8L3g6eG1wbWV0YT4gPD94cGFja2V0IGVuZD0iciI/PpFdYkUAAAEsSURBVHjaYvz//z8DPQATA50AC+P6D6Tq0QViHiA+TpJFZDhuMhDzA7EhLYMuEIjtgdgAiJNJ0cjIsO49sWo5gfgqECtC+S+AWBWIv1DbR0VIloCABBA3UttHIENvQxMBMvgFxNpAfIdaPurFYgkIsAFxF7WCzgyIowgkEA9qWDSZSB8zU2JRMtRHhIAWEKeRmxh4oAlAgsh4fAdN7u9I9VEFCZaAgBAQt5DqIxVo5mQjseT4C8R6QHyNWB91kWEJAzRBTCA26JygSZZc4IpNPxMWF01moBxghAi6RWnQpEopAMVxPq7EIARNzkJUqlS/QJP7C3QftVDRElg+bEH3ESi4LhEqRsgEoJr4AhNSeUYLS0BgJqzNACrLRIH4Mo0sAtXM9ozDrl0HEGAAOt00sQRg5yAAAAAASUVORK5CYII=' :
	IMAGE_PATH + '/triangle-up.png', 26, 26);

/**
 * Right arrow.
 */
HoverIcons.prototype.triangleRight = new mxImage((mxClient.IS_SVG) ? 'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABoAAAAaCAYAAACpSkzOAAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAAyJpVFh0WE1MOmNvbS5hZG9iZS54bXAAAAAAADw/eHBhY2tldCBiZWdpbj0i77u/IiBpZD0iVzVNME1wQ2VoaUh6cmVTek5UY3prYzlkIj8+IDx4OnhtcG1ldGEgeG1sbnM6eD0iYWRvYmU6bnM6bWV0YS8iIHg6eG1wdGs9IkFkb2JlIFhNUCBDb3JlIDUuMC1jMDYwIDYxLjEzNDc3NywgMjAxMC8wMi8xMi0xNzozMjowMCAgICAgICAgIj4gPHJkZjpSREYgeG1sbnM6cmRmPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5LzAyLzIyLXJkZi1zeW50YXgtbnMjIj4gPHJkZjpEZXNjcmlwdGlvbiByZGY6YWJvdXQ9IiIgeG1sbnM6eG1wPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvIiB4bWxuczp4bXBNTT0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wL21tLyIgeG1sbnM6c3RSZWY9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9zVHlwZS9SZXNvdXJjZVJlZiMiIHhtcDpDcmVhdG9yVG9vbD0iQWRvYmUgUGhvdG9zaG9wIENTNSBNYWNpbnRvc2giIHhtcE1NOkluc3RhbmNlSUQ9InhtcC5paWQ6RkQ3OEM2Nzg0NTVBMTFFNEIxOTZFRjE3NzRENjQ0RjIiIHhtcE1NOkRvY3VtZW50SUQ9InhtcC5kaWQ6RkQ3OEM2Nzk0NTVBMTFFNEIxOTZFRjE3NzRENjQ0RjIiPiA8eG1wTU06RGVyaXZlZEZyb20gc3RSZWY6aW5zdGFuY2VJRD0ieG1wLmlpZDpGRDc4QzY3NjQ1NUExMUU0QjE5NkVGMTc3NEQ2NDRGMiIgc3RSZWY6ZG9jdW1lbnRJRD0ieG1wLmRpZDpGRDc4QzY3NzQ1NUExMUU0QjE5NkVGMTc3NEQ2NDRGMiIvPiA8L3JkZjpEZXNjcmlwdGlvbj4gPC9yZGY6UkRGPiA8L3g6eG1wbWV0YT4gPD94cGFja2V0IGVuZD0iciI/PmADGesAAAE2SURBVHjatJaxSgNBFEU3iQhCQEllK1hFItjsB1jYr6WtgigIAcFGwSafIBaipa1pBRG0Eq0koI1+QCqxSCXIekbug2UhGJN5Fw7Dzi4c5s283a3keZ6EVLqfKcNTEjl5Nvc7Vgtz53ALzcQh1dL1KvTgFBqeopAa7MCbxpqXyNLQynpaqZvI0tTeXcGip8iSwQt0oO4pCpmGQ+3fpqfIMq92eITUU2RJJbuU3E1k2VA5Q1lnPEWJDkhHBybzFFkW1Ap30PIUWWaLbeAh6sMWrMCDTU5FFHzBCRzDoHwzlqgLB/A+7IFJRa+wD9deDfsBu7A8imScFX3DGRxJNnL+I7qBtsrl8pkIG7wOa+NK/hINdJKWdKomyrDSXWgf+rGarCwK/3Xb8Bz7dVEU7cG914vvR4ABAGCSNhcpqHjLAAAAAElFTkSuQmCC':
	IMAGE_PATH + '/triangle-right.png', 26, 26);

/**
 * Down arrow.
 */
HoverIcons.prototype.triangleDown = new mxImage((mxClient.IS_SVG) ? 'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABoAAAAaCAYAAACpSkzOAAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAAyJpVFh0WE1MOmNvbS5hZG9iZS54bXAAAAAAADw/eHBhY2tldCBiZWdpbj0i77u/IiBpZD0iVzVNME1wQ2VoaUh6cmVTek5UY3prYzlkIj8+IDx4OnhtcG1ldGEgeG1sbnM6eD0iYWRvYmU6bnM6bWV0YS8iIHg6eG1wdGs9IkFkb2JlIFhNUCBDb3JlIDUuMC1jMDYwIDYxLjEzNDc3NywgMjAxMC8wMi8xMi0xNzozMjowMCAgICAgICAgIj4gPHJkZjpSREYgeG1sbnM6cmRmPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5LzAyLzIyLXJkZi1zeW50YXgtbnMjIj4gPHJkZjpEZXNjcmlwdGlvbiByZGY6YWJvdXQ9IiIgeG1sbnM6eG1wPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvIiB4bWxuczp4bXBNTT0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wL21tLyIgeG1sbnM6c3RSZWY9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9zVHlwZS9SZXNvdXJjZVJlZiMiIHhtcDpDcmVhdG9yVG9vbD0iQWRvYmUgUGhvdG9zaG9wIENTNSBNYWNpbnRvc2giIHhtcE1NOkluc3RhbmNlSUQ9InhtcC5paWQ6RkQ3OEM2N0M0NTVBMTFFNEIxOTZFRjE3NzRENjQ0RjIiIHhtcE1NOkRvY3VtZW50SUQ9InhtcC5kaWQ6RkQ3OEM2N0Q0NTVBMTFFNEIxOTZFRjE3NzRENjQ0RjIiPiA8eG1wTU06RGVyaXZlZEZyb20gc3RSZWY6aW5zdGFuY2VJRD0ieG1wLmlpZDpGRDc4QzY3QTQ1NUExMUU0QjE5NkVGMTc3NEQ2NDRGMiIgc3RSZWY6ZG9jdW1lbnRJRD0ieG1wLmRpZDpGRDc4QzY3QjQ1NUExMUU0QjE5NkVGMTc3NEQ2NDRGMiIvPiA8L3JkZjpEZXNjcmlwdGlvbj4gPC9yZGY6UkRGPiA8L3g6eG1wbWV0YT4gPD94cGFja2V0IGVuZD0iciI/PuktDIwAAAE0SURBVHjaYvz//z8DPQATA50AC+P6D/ZAejKN7UlhARIHgfg7EJvRyJJ9QHwKFnTpNLLkLxDnIsfRBSCeSwOLZgHxNRCDkWHde5igBBDfBmIeKlnyDohVoTRKqnsBxE1U9E0NzBJ0H4EAGxBfBWIVCi0BBZceNI6w5qNfQFxGBd/kIluCK8OuB+LdFFiyHpqkiSoZCtBdRCTAGSJMeMJ4FhkWdQPxHWwS6IkBGQhBk7sQkZa8gCbnL6QWqu+gSZSU5PyF3NIbnrMJgFOEShYmIsqqYiKTM8X10Q5oksUFlkF9RJWKrwyadNHBFyJ9TLRFoCQ7BYt4BzS1EQT4kjc64IEmdwko/z4Qa0MrTaq2Gb6gJfdiYi0h1UcwcB6IPwKxA0mNEzKKmSx8GROnj4Zduw4gwAC3tEnG5i1iXgAAAABJRU5ErkJggg==' :
	IMAGE_PATH + '/triangle-down.png', 26, 26);

/**
 * Left arrow.
 */
HoverIcons.prototype.triangleLeft = new mxImage((mxClient.IS_SVG) ? 'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABoAAAAaCAYAAACpSkzOAAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAAyJpVFh0WE1MOmNvbS5hZG9iZS54bXAAAAAAADw/eHBhY2tldCBiZWdpbj0i77u/IiBpZD0iVzVNME1wQ2VoaUh6cmVTek5UY3prYzlkIj8+IDx4OnhtcG1ldGEgeG1sbnM6eD0iYWRvYmU6bnM6bWV0YS8iIHg6eG1wdGs9IkFkb2JlIFhNUCBDb3JlIDUuMC1jMDYwIDYxLjEzNDc3NywgMjAxMC8wMi8xMi0xNzozMjowMCAgICAgICAgIj4gPHJkZjpSREYgeG1sbnM6cmRmPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5LzAyLzIyLXJkZi1zeW50YXgtbnMjIj4gPHJkZjpEZXNjcmlwdGlvbiByZGY6YWJvdXQ9IiIgeG1sbnM6eG1wPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvIiB4bWxuczp4bXBNTT0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wL21tLyIgeG1sbnM6c3RSZWY9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9zVHlwZS9SZXNvdXJjZVJlZiMiIHhtcDpDcmVhdG9yVG9vbD0iQWRvYmUgUGhvdG9zaG9wIENTNSBNYWNpbnRvc2giIHhtcE1NOkluc3RhbmNlSUQ9InhtcC5paWQ6NDA3RjRDRkY0NTVCMTFFNEIxOTZFRjE3NzRENjQ0RjIiIHhtcE1NOkRvY3VtZW50SUQ9InhtcC5kaWQ6NDA3RjREMDA0NTVCMTFFNEIxOTZFRjE3NzRENjQ0RjIiPiA8eG1wTU06RGVyaXZlZEZyb20gc3RSZWY6aW5zdGFuY2VJRD0ieG1wLmlpZDpGRDc4QzY3RTQ1NUExMUU0QjE5NkVGMTc3NEQ2NDRGMiIgc3RSZWY6ZG9jdW1lbnRJRD0ieG1wLmRpZDo0MDdGNENGRTQ1NUIxMUU0QjE5NkVGMTc3NEQ2NDRGMiIvPiA8L3JkZjpEZXNjcmlwdGlvbj4gPC9yZGY6UkRGPiA8L3g6eG1wbWV0YT4gPD94cGFja2V0IGVuZD0iciI/PquyPQ0AAAE+SURBVHjatJaxSgNBFEU3KoKQQqzyAdoohFSKlQERbGNrqVik8QeCVdIIgpVBMK2lW9oF0qYRFFMJtlZBQRAEWc/AHVklxE123oUDm+zA4b03s0whSZLIpRC/RgbZSmqLPfcwF9mkApewAGX3x0xgQQmu4A7W0y9CVTQPx3ACxVELQohqcArL4xblEa3COexkWTzNjJbgAu6zSiataBaOoCnZRMkq2oUztWuq/Nc6N+AbuM0jGScqqoJH7arcGdW6A82hFPIkp0Wb2k0Vi29SunXv8Gb07fsleoAq7MGzpcgnhjVoqEozkcsHtGAFri1FPi+wDxvQtxT59CU7lNxM5NNRO11bPy1F/hg0tGFiS5HPk47CNgwsRT5dXUDqMLQUuXxBW/Nr67eJyGeoysqqNPgt6G8Gmt3PletbgAEAmkYzZ9MOuCsAAAAASUVORK5CYII=' :
	IMAGE_PATH + '/triangle-left.png', 26, 26);

/**
 * Round target.
 */
HoverIcons.prototype.roundDrop = new mxImage((mxClient.IS_SVG) ? 'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABoAAAAaCAYAAACpSkzOAAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAAyJpVFh0WE1MOmNvbS5hZG9iZS54bXAAAAAAADw/eHBhY2tldCBiZWdpbj0i77u/IiBpZD0iVzVNME1wQ2VoaUh6cmVTek5UY3prYzlkIj8+IDx4OnhtcG1ldGEgeG1sbnM6eD0iYWRvYmU6bnM6bWV0YS8iIHg6eG1wdGs9IkFkb2JlIFhNUCBDb3JlIDUuMC1jMDYwIDYxLjEzNDc3NywgMjAxMC8wMi8xMi0xNzozMjowMCAgICAgICAgIj4gPHJkZjpSREYgeG1sbnM6cmRmPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5LzAyLzIyLXJkZi1zeW50YXgtbnMjIj4gPHJkZjpEZXNjcmlwdGlvbiByZGY6YWJvdXQ9IiIgeG1sbnM6eG1wPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvIiB4bWxuczp4bXBNTT0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wL21tLyIgeG1sbnM6c3RSZWY9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9zVHlwZS9SZXNvdXJjZVJlZiMiIHhtcDpDcmVhdG9yVG9vbD0iQWRvYmUgUGhvdG9zaG9wIENTNSBNYWNpbnRvc2giIHhtcE1NOkluc3RhbmNlSUQ9InhtcC5paWQ6RTgxRjYzRTU1MDRFMTFFNEExQ0VFNDQwNDhGNzg2RDkiIHhtcE1NOkRvY3VtZW50SUQ9InhtcC5kaWQ6RTgxRjYzRTY1MDRFMTFFNEExQ0VFNDQwNDhGNzg2RDkiPiA8eG1wTU06RGVyaXZlZEZyb20gc3RSZWY6aW5zdGFuY2VJRD0ieG1wLmlpZDpFODFGNjNFMzUwNEUxMUU0QTFDRUU0NDA0OEY3ODZEOSIgc3RSZWY6ZG9jdW1lbnRJRD0ieG1wLmRpZDpFODFGNjNFNDUwNEUxMUU0QTFDRUU0NDA0OEY3ODZEOSIvPiA8L3JkZjpEZXNjcmlwdGlvbj4gPC9yZGY6UkRGPiA8L3g6eG1wbWV0YT4gPD94cGFja2V0IGVuZD0iciI/PuJ657wAAAE0SURBVHjaYvz//z8DPQATA50AC4zBuP4DLjXaQOwMxJZArAfE8lDxh0B8CYiPA/FeIL6KTfP/QAFUi7AABSBOAOJoIFbBIq8FxRFAfAeIlwLxAiB+gNdHaMABiIuB2IfIkAE5pB6IjYG4F4gPEBNHIEtaSLAEGfhA9ToQskgB6hNrCuLdGmqGAj6LEsj0CTafJeCySBsa8dQC0VAzMSxyxpG6yAUqUDMxLLKkQT61xGaRHg0s0sNmkTwNLJKne1mHbNFDGpj/EJtFl2hg0SVsFh2ngUXHsVm0F1oKUwvcgZqJYdFVaFFPLbAUuY5CT3Wg+mQLFSzZAjULZ6H6AFqfHKXAkqNQMx4Qqo9AlVYNmT7bAtV7gNga9gDURWfxVOXoEY+3KmeENbdo3ThhHHbtOoAAAwDmEETshQ0fBAAAAABJRU5ErkJggg==' :
	IMAGE_PATH + '/round-drop.png', 26, 26);

/**
 * Refresh target.
 */
HoverIcons.prototype.refreshTarget = new mxImage((mxClient.IS_SVG) ? 'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAACYAAAAmCAYAAACoPemuAAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAAyJpVFh0WE1MOmNvbS5hZG9iZS54bXAAAAAAADw/eHBhY2tldCBiZWdpbj0i77u/IiBpZD0iVzVNME1wQ2VoaUh6cmVTek5UY3prYzlkIj8+IDx4OnhtcG1ldGEgeG1sbnM6eD0iYWRvYmU6bnM6bWV0YS8iIHg6eG1wdGs9IkFkb2JlIFhNUCBDb3JlIDUuMC1jMDYwIDYxLjEzNDc3NywgMjAxMC8wMi8xMi0xNzozMjowMCAgICAgICAgIj4gPHJkZjpSREYgeG1sbnM6cmRmPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5LzAyLzIyLXJkZi1zeW50YXgtbnMjIj4gPHJkZjpEZXNjcmlwdGlvbiByZGY6YWJvdXQ9IiIgeG1sbnM6eG1wPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvIiB4bWxuczp4bXBNTT0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wL21tLyIgeG1sbnM6c3RSZWY9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9zVHlwZS9SZXNvdXJjZVJlZiMiIHhtcDpDcmVhdG9yVG9vbD0iQWRvYmUgUGhvdG9zaG9wIENTNSBNYWNpbnRvc2giIHhtcE1NOkluc3RhbmNlSUQ9InhtcC5paWQ6NDQxNERDRTU1QjY1MTFFNDkzNTRFQTVEMTdGMTdBQjciIHhtcE1NOkRvY3VtZW50SUQ9InhtcC5kaWQ6NDQxNERDRTY1QjY1MTFFNDkzNTRFQTVEMTdGMTdBQjciPiA8eG1wTU06RGVyaXZlZEZyb20gc3RSZWY6aW5zdGFuY2VJRD0ieG1wLmlpZDo0NDE0RENFMzVCNjUxMUU0OTM1NEVBNUQxN0YxN0FCNyIgc3RSZWY6ZG9jdW1lbnRJRD0ieG1wLmRpZDo0NDE0RENFNDVCNjUxMUU0OTM1NEVBNUQxN0YxN0FCNyIvPiA8L3JkZjpEZXNjcmlwdGlvbj4gPC9yZGY6UkRGPiA8L3g6eG1wbWV0YT4gPD94cGFja2V0IGVuZD0iciI/PsvuX50AAANaSURBVHja7FjRZ1tRGD9ZJ1NCyIQSwrivI4Q8hCpjlFDyFEoYfSp9Ko1QWnmo0If+BSXkIfo0QirTMUpeGo2EPfWllFYjZMLKLDJn53d3biU337m5J223bPbxk5t7v+/c3/2+73znO8fDOWezKM/YjMpz68Lj8ejY+QTeCCwLxOS9qPxtyN+6wAeBTwJ31CCO0cJDjXBGBN4LfIepSwykTUT1bgpuib0SONIgo8KRHOtRiCFcvUcgZeGrHPNBxLIyFPyRgTGz0xLbegJCdmzpElue5KlAIMDX19d5uVzm5+fnfDAYmMA17uEZdOx2Yvb/sHlu2S0xwymn5ufneTab5b1ej08S6EAXNrDd2dnhiUTim21MvMtwQ6yiIrWwsMDPzs64rsBmf3/fvM7n89TYlUnEllSkQqEQv7q64g+Vk5MTVXosORErU0Zer5f0FEIlw2N6MxwO82QyaXql2+2SxDqdjopYWUUsqEp45IldqtWq6UWVh/1+P7+8vCTJ4QMUJSRIEXuneoH96w8PDyeWAnhSJfCqwm6NIlaklFdXV0cGhRcQ2mlJQXK5nMq2YPEZbnteU1U2lUqN/D84OGD9fl+5fgnSrFarsUwmw0qlEru4uBjTicViTk3Cr27HSnxR+Doyz0ZE1CAWiUTusbu7y9rttlZv5fP5WDQavYfIMba4uEipfhF8XtqJoZXx/uH+sC/4vPg7OljZZQbsCmLtYzc3N6zRaJhotVrmfx0xDINtbm6athYUeXpHdbBNaqZUKpWxWXV7e2vex+xaWVnhc3NzjrPUXgexyCt0m67LBV7uJMITjqRE4o8tZeg8FPpFitgapYxiOC0poFgsji1jKNo6BZZckrAGUtJsNk1vqAihCBcKhTE7hNWhqw2qFnGy5UFOUYJVIJ1OjzSE+BCEilon0URavRmBqnbbQ00AXbm+vnZc9O1tj72OnQoc2+cwygRkb2+P1et17ZoEm3g87lRmjgWZ00kbXkNuse6/Bu2wlegIxfb2tuvWGroO4bO2c4bbzUh60mxDXm1sbJhhxkQYnhS4h2fUZoRAWnf7lv8N27f8P7Xhnekjgpk+VKGOoQbsiY+hhhtF3YO7twIJ+ULvUGv+GQ2fQEvWxI/THNx5/p/BaspPAQYAqStgiSQwCDoAAAAASUVORK5CYII=' :
	IMAGE_PATH + '/refresh.png', 38, 38);

/**
 * 
 */
HoverIcons.prototype.init = function()
{
	this.arrowUp = this.createArrow(this.triangleUp, mxResources.get('plusTooltip'));
	this.arrowRight = this.createArrow(this.triangleRight, mxResources.get('plusTooltip'));
	this.arrowDown = this.createArrow(this.triangleDown, mxResources.get('plusTooltip'));
	this.arrowLeft = this.createArrow(this.triangleLeft, mxResources.get('plusTooltip'));

	this.elts = [this.arrowUp, this.arrowRight, this.arrowDown, this.arrowLeft];

	this.repaintHandler = mxUtils.bind(this, function()
	{
		this.repaint();
	});

	this.graph.selectionModel.addListener(mxEvent.CHANGE, this.repaintHandler);
	this.graph.model.addListener(mxEvent.CHANGE, this.repaintHandler);
	this.graph.view.addListener(mxEvent.SCALE_AND_TRANSLATE, this.repaintHandler);
	this.graph.view.addListener(mxEvent.TRANSLATE, this.repaintHandler);
	this.graph.view.addListener(mxEvent.SCALE, this.repaintHandler);
	this.graph.view.addListener(mxEvent.DOWN, this.repaintHandler);
	this.graph.view.addListener(mxEvent.UP, this.repaintHandler);
	
	// Resets the mouse point on escape
	this.graph.addListener(mxEvent.ESCAPE, mxUtils.bind(this, function()
	{
		this.mouseDownPoint = null;
	}));

	// Removes hover icons if mouse leaves the container
	mxEvent.addListener(this.graph.container, 'mouseleave',  mxUtils.bind(this, function(evt)
	{
		// Workaround for IE11 firing mouseleave for touch in diagram
		if (evt.relatedTarget != null && mxEvent.getSource(evt) == this.graph.container)
		{
			this.setDisplay('none');
		}
	}));
	
	// Resets current state after update of selection state for touch events
	var graphClick = this.graph.click;
	this.graph.click = mxUtils.bind(this, function(me)
	{
		graphClick.apply(this.graph, arguments);
		
		if (this.currentState != null && !this.graph.isCellSelected(this.currentState.cell) &&
			mxEvent.isTouchEvent(me.getEvent()) && !this.graph.model.isVertex(me.getCell()))
		{
			this.reset();
		}
	});
	
	// Checks if connection handler was active in mouse move
	// as workaround for possible double connection inserted
	var connectionHandlerActive = false;
	
	// Implements a listener for hover and click handling
	this.graph.addMouseListener(
	{
	    mouseDown: mxUtils.bind(this, function(sender, me)
	    {
	    	connectionHandlerActive = false;
	    	var evt = me.getEvent();
	    	
	    	if (this.isResetEvent(evt))
	    	{
	    		this.reset();
	    	}
	    	else if (!this.isActive())
	    	{
	    		var state = this.getState(me.getState());
	    		
	    		if (state != null || !mxEvent.isTouchEvent(evt))
	    		{
	    			this.update(state);
	    		}
	    	}
	    	
	    	this.setDisplay('none');
	    }),
	    mouseMove: mxUtils.bind(this, function(sender, me)
	    {
	    	var evt = me.getEvent();
	    	
	    	if (this.isResetEvent(evt))
	    	{
	    		this.reset();
	    	}
	    	else if (!this.graph.isMouseDown && !mxEvent.isTouchEvent(evt))
	    	{
	    		this.update(this.getState(me.getState()), me.getGraphX(), me.getGraphY());
	    	}
	    	
	    	if (this.graph.connectionHandler != null && this.graph.connectionHandler.shape != null)
	    	{
	    		connectionHandlerActive = true;
	    	}
	    }),
	    mouseUp: mxUtils.bind(this, function(sender, me)
	    {
	    	var evt = me.getEvent();
	    	
	    	if (this.isResetEvent(evt))
	    	{
	    		this.reset();
	    	}
	    	else if (this.isActive() && this.mouseDownPoint != null &&
	    		Math.abs(me.getGraphX() - this.mouseDownPoint.x) < this.graph.tolerance &&
		    	Math.abs(me.getGraphY() - this.mouseDownPoint.y) < this.graph.tolerance)
	    	{
	    		// Executes click event on highlighted arrow
	    		if (!connectionHandlerActive)
	    		{
	    			this.click(this.currentState, this.getDirection(), me);
	    		}
	    	}
	    	else if (this.isActive())
	    	{
	    		// Selects target vertex after drag and clone if not only new edge was inserted
	    		if (this.graph.getSelectionCount() != 1 || !this.graph.model.isEdge(this.graph.getSelectionCell()))
	    		{
	    			this.update(this.getState(this.graph.view.getState(this.graph.getCellAt(me.getGraphX(), me.getGraphY()))));
	    		}
	    		else
	    		{
	    			this.reset();
	    		}
	    	}
	    	else if (mxEvent.isTouchEvent(evt) || (this.bbox != null && mxUtils.contains(this.bbox, me.getGraphX(), me.getGraphY())))
	    	{
	    		// Shows existing hover icons if inside bounding box
	    		this.setDisplay('');
	    		this.repaint();
	    	}
	    	else if (!mxEvent.isTouchEvent(evt))
	    	{
	    		this.reset();
	    	}
	    	
	    	connectionHandlerActive = false;
	    	this.resetActiveArrow();
	    })
	});
};

/**
 * 
 */
HoverIcons.prototype.isResetEvent = function(evt, allowShift)
{
	return mxEvent.isAltDown(evt) || (this.activeArrow == null && mxEvent.isShiftDown(evt)) ||
		mxEvent.isMetaDown(evt) || (mxEvent.isPopupTrigger(evt) && !mxEvent.isControlDown(evt));
};

/**
 * 
 */
HoverIcons.prototype.createArrow = function(img, tooltip)
{
	var arrow = null;
	
	if (mxClient.IS_IE && !mxClient.IS_SVG)
	{
		// Workaround for PNG images in IE6
		if (mxClient.IS_IE6 && document.compatMode != 'CSS1Compat')
		{
			arrow = document.createElement(mxClient.VML_PREFIX + ':image');
			arrow.setAttribute('src', img.src);
			arrow.style.borderStyle = 'none';
		}
		else
		{
			arrow = document.createElement('div');
			arrow.style.backgroundImage = 'url(' + img.src + ')';
			arrow.style.backgroundPosition = 'center';
			arrow.style.backgroundRepeat = 'no-repeat';
		}
		
		arrow.style.width = (img.width + 4) + 'px';
		arrow.style.height = (img.height + 4) + 'px';
		arrow.style.display = (mxClient.IS_QUIRKS) ? 'inline' : 'inline-block';
	}
	else
	{
		arrow = mxUtils.createImage(img.src);
		arrow.style.width = img.width + 'px';
		arrow.style.height = img.height + 'px';
	}
	
	if (tooltip != null)
	{
		arrow.setAttribute('title', tooltip);
	}
	
	arrow.style.position = 'absolute';
	arrow.style.cursor = 'crosshair';

	mxEvent.addGestureListeners(arrow, mxUtils.bind(this, function(evt)
	{
		if (this.currentState != null && !this.isResetEvent(evt))
		{
			this.mouseDownPoint = mxUtils.convertPoint(this.graph.container,
					mxEvent.getClientX(evt), mxEvent.getClientY(evt));
			this.drag(evt, this.mouseDownPoint.x, this.mouseDownPoint.y);
			this.activeArrow = arrow;
			this.setDisplay('none');
			mxEvent.consume(evt);
		}
	}));
	
	// Captures mouse events as events on graph
	mxEvent.redirectMouseEvents(arrow, this.graph, this.currentState);
	
	mxEvent.addListener(arrow, 'mouseenter', mxUtils.bind(this, function(evt)
	{
		// Workaround for Firefox firing mouseenter on touchend
		if (mxEvent.isMouseEvent(evt))
		{
	    	if (this.activeArrow != null && this.activeArrow != arrow)
	    	{
	    		mxUtils.setOpacity(this.activeArrow, 20);
	    	}

			this.graph.connectionHandler.constraintHandler.reset();
			mxUtils.setOpacity(arrow, 100);
			this.activeArrow = arrow;
		}
	}));
	
	mxEvent.addListener(arrow, 'mouseleave', mxUtils.bind(this, function(evt)
	{
		// Workaround for IE11 firing this event on touch
		if (!this.graph.isMouseDown)
		{
			this.resetActiveArrow();
		}
	}));
	
	return arrow;
};

/**
 * 
 */
HoverIcons.prototype.resetActiveArrow = function()
{
	if (this.activeArrow != null)
	{
		mxUtils.setOpacity(this.activeArrow, 20);
		this.activeArrow = null;
	}
};

/**
 * 
 */
HoverIcons.prototype.getDirection = function()
{
	var dir = mxConstants.DIRECTION_EAST;

	if (this.activeArrow == this.arrowUp)
	{
		dir = mxConstants.DIRECTION_NORTH;
	}
	else if (this.activeArrow == this.arrowDown)
	{
		dir = mxConstants.DIRECTION_SOUTH;
	}
	else if (this.activeArrow == this.arrowLeft)
	{
		dir = mxConstants.DIRECTION_WEST;
	}
		
	return dir;
};

/**
 * 
 */
HoverIcons.prototype.visitNodes = function(visitor)
{
	for (var i = 0; i < this.elts.length; i++)
	{
		if (this.elts[i] != null)
		{
			visitor(this.elts[i]);
		}
	}
};

/**
 * 
 */
HoverIcons.prototype.removeNodes = function()
{
	this.visitNodes(function(elt)
	{
		if (elt.parentNode != null)
		{
			elt.parentNode.removeChild(elt);
		}
	});
};

/**
 *
 */
HoverIcons.prototype.setDisplay = function(display)
{
	this.visitNodes(function(elt)
	{
		elt.style.display = display;
	});
};

/**
 *
 */
HoverIcons.prototype.isActive = function()
{
	return this.activeArrow != null && this.currentState != null;
};

/**
 *
 */
HoverIcons.prototype.drag = function(evt, x, y)
{
	this.graph.popupMenuHandler.hideMenu();
	this.graph.stopEditing(false);

	// Checks if state was removed in call to stopEditing above
	if (this.currentState != null)
	{
		this.graph.connectionHandler.start(this.currentState, x, y);
		this.graph.isMouseTrigger = mxEvent.isMouseEvent(evt);
		this.graph.isMouseDown = true;
		
		// Hides handles for selection cell
		var handler = this.graph.selectionCellsHandler.getHandler(this.currentState.cell);
		
		if (handler != null)
		{
			handler.setHandlesVisible(false);
		}
		
		// Uses elbow edges with vertical or horizontal direction
//		var direction = this.getDirection();
//		var elbowValue = (direction == mxConstants.DIRECTION_NORTH || direction == mxConstants.DIRECTION_SOUTH) ? 'vertical' : 'horizontal';
//		
//		var es = this.graph.connectionHandler.edgeState;
//		es.style['edgeStyle'] = 'elbowEdgeStyle';
//		es.style['elbow'] = elbowValue;
//		es.cell.style = mxUtils.setStyle(es.cell.style, 'edgeStyle', es.style['edgeStyle']);
//		es.cell.style = mxUtils.setStyle(es.cell.style, 'elbow', es.style['elbow']);
	}
};

/**
 *
 */
HoverIcons.prototype.click = function(state, dir, me)
{
	var evt = me.getEvent();
	var x = me.getGraphX();
	var y = me.getGraphY();
	
	var tmp = this.graph.view.getState(this.graph.getCellAt(x, y));
	
	if (tmp != null && this.graph.model.isEdge(tmp.cell) && !mxEvent.isControlDown(evt) &&
		(tmp.getVisibleTerminalState(true) == state || tmp.getVisibleTerminalState(false) == state))
	{
		this.graph.setSelectionCell(tmp.cell);
		this.reset();
	}
	else if (state != null)
	{
		var cells = this.graph.connectVertex(state.cell, dir, this.graph.defaultEdgeLength, evt);
		this.graph.selectCellsForConnectVertex(cells, evt, this);
		
		// Selects only target vertex if one exists
		if (cells.length == 2 && this.graph.model.isVertex(cells[1]))
		{
			this.graph.setSelectionCell(cells[1]);
			
			// Adds hover icons to new target vertex for touch devices
			if (mxEvent.isTouchEvent(evt))
			{
				this.update(this.getState(this.graph.view.getState(cells[1])));
			}
			else
			{
				// Hides hover icons after click with mouse
				this.reset();
			}
			
			this.graph.scrollCellToVisible(cells[1]);
		}
		else
		{
			this.graph.setSelectionCells(cells);
		}
	}
	
	me.consume();
};

/**
 * 
 */
HoverIcons.prototype.reset = function()
{
	if (this.updateThread != null)
	{
		window.clearTimeout(this.updateThread);
	}

	this.mouseDownPoint = null;
	this.currentState = null;
	this.activeArrow = null;
	this.removeNodes();
	this.bbox = null;
};

/**
 * 
 */
HoverIcons.prototype.repaint = function()
{
	this.bbox = null;
	
	if (this.currentState != null)
	{
		// Checks if cell was deleted
		this.currentState = this.getState(this.currentState);
		
		// Cell was deleted	
		if (this.currentState != null &&
			this.graph.model.isVertex(this.currentState.cell) &&
			this.graph.isCellConnectable(this.currentState.cell))
		{
			var bds = mxRectangle.fromRectangle(this.currentState);
			
			// Uses outer bounding box to take rotation into account
			if (this.currentState.shape != null && this.currentState.shape.boundingBox != null)
			{
				bds = mxRectangle.fromRectangle(this.currentState.shape.boundingBox);
			}

			bds.grow(this.graph.tolerance);
			bds.grow(this.arrowSpacing);
			
			var handler = this.graph.selectionCellsHandler.getHandler(this.currentState.cell);
			
			if (handler != null)
			{
				bds.x -= handler.horizontalOffset / 2;
				bds.y -= handler.verticalOffset / 2;
				bds.width += handler.horizontalOffset;
				bds.height += handler.verticalOffset;
				
				// Adds bounding box of rotation handle to avoid overlap
				if (handler.rotationShape != null && handler.rotationShape.node != null &&
					handler.rotationShape.node.style.visibility != 'hidden' &&
					handler.rotationShape.node.style.display != 'none' &&
					handler.rotationShape.boundingBox != null)
				{
					bds.add(handler.rotationShape.boundingBox);
				}
			}
			
			this.arrowUp.style.left = Math.round(this.currentState.getCenterX() - this.triangleUp.width / 2) + 'px';
			this.arrowUp.style.top = Math.round(bds.y - this.triangleUp.height) + 'px';
			mxUtils.setOpacity(this.arrowUp, 20);
			
			this.arrowRight.style.left = Math.round(bds.x + bds.width) + 'px';
			this.arrowRight.style.top = Math.round(this.currentState.getCenterY() - this.triangleRight.height / 2) + 'px';
			mxUtils.setOpacity(this.arrowRight, 20);
			
			this.arrowDown.style.left = this.arrowUp.style.left
			this.arrowDown.style.top = Math.round(bds.y + bds.height) + 'px';
			mxUtils.setOpacity(this.arrowDown, 20);
			
			this.arrowLeft.style.left = Math.round(bds.x - this.triangleLeft.width) + 'px';
			this.arrowLeft.style.top = this.arrowRight.style.top;
			mxUtils.setOpacity(this.arrowLeft, 20);
		}
		else
		{
			this.reset();
		}
		
		// Updates bounding box
		if (this.currentState != null)
		{
			this.bbox = this.computeBoundingBox();
			
			// Adds tolerance for hover
			if (this.bbox != null)
			{
				this.bbox.grow(10);
			}
		}
	}
};

/**
 * 
 */
HoverIcons.prototype.computeBoundingBox = function()
{
	var bbox = (!this.graph.model.isEdge(this.currentState.cell)) ? mxRectangle.fromRectangle(this.currentState) : null;
	
	this.visitNodes(function(elt)
	{
		if (elt.parentNode != null)
		{
			var tmp = new mxRectangle(elt.offsetLeft, elt.offsetTop, elt.offsetWidth, elt.offsetHeight);
			
			if (bbox == null)
			{
				bbox = tmp;
			}
			else
			{
				bbox.add(tmp);
			}
		}
	});
	
	return bbox;
};

/**
 * 
 */
HoverIcons.prototype.getState = function(state)
{
	if (state != null)
	{
		var cell = state.cell;

		// Uses connectable parent vertex if child is not connectable
		if (this.graph.getModel().isVertex(cell) && !this.graph.isCellConnectable(cell))
		{
			var parent = this.graph.getModel().getParent(cell);
			
			if (this.graph.getModel().isVertex(parent) && this.graph.isCellConnectable(parent))
			{
				cell = parent;
			}
		}
		
		// Ignores locked cells and edges
		if (this.graph.isCellLocked(cell) || this.graph.model.isEdge(cell))
		{
			cell = null;
		}
		
		state = this.graph.view.getState(cell);
	}
	
	return state;
};

/**
 * 
 */
HoverIcons.prototype.update = function(state, x, y)
{
	if (!this.graph.connectionArrowsEnabled)
	{
		this.reset();
	}
	else
	{
		var timeOnTarget = null;
		
		// Time on target
		if (this.prev != state || this.isActive())
		{
			this.startTime = new Date().getTime();
			this.prev = state;
			timeOnTarget = 0;
	
			if (this.updateThread != null)
			{
				window.clearTimeout(this.updateThread);
			}
			
			if (state != null)
			{
				// Starts timer to update current state with no mouse events
				this.updateThread = window.setTimeout(mxUtils.bind(this, function()
				{
					if (!this.isActive() && !this.graph.isMouseDown)
					{
						this.prev = state;
						this.update(state, x, y);
					}
				}), this.updateDelay + 10);
			}
		}
		else if (this.startTime != null)
		{
			timeOnTarget = new Date().getTime() - this.startTime;
		}
		
		this.setDisplay('');
		
		if (this.currentState != state && ((timeOnTarget > this.updateDelay && state != null) ||
			this.bbox == null || x == null || y == null || !mxUtils.contains(this.bbox, x, y)))
		{
			if (state != null && this.graph.isEnabled())
			{
				this.removeNodes();
				this.setCurrentState(state);
				this.repaint();
				
				// Resets connection points on other focused cells
				if (this.graph.connectionHandler.constraintHandler.currentFocus != state)
				{
					this.graph.connectionHandler.constraintHandler.reset();
				}
			}
			else
			{
				this.reset();
			}
		}
	}
};

/**
 * 
 */
HoverIcons.prototype.setCurrentState = function(state)
{
	if (state.style['portConstraint'] != 'eastwest')
	{
		this.graph.container.appendChild(this.arrowUp);
		this.graph.container.appendChild(this.arrowDown);
	}

	this.graph.container.appendChild(this.arrowRight);
	this.graph.container.appendChild(this.arrowLeft);
	this.currentState = state;
};

/**
 * These overrides are only added if mxVertexHandler is defined (ie. not in embedded graph)
 */
if (typeof mxVertexHandler != 'undefined')
{
	(function()
	{
		// Sets colors for handles
		mxConstants.HANDLE_FILLCOLOR = '#99ccff';
		mxConstants.HANDLE_STROKECOLOR = '#0088cf';
		mxConstants.VERTEX_SELECTION_COLOR = '#00a8ff';
		mxConstants.OUTLINE_COLOR = '#00a8ff';
		mxConstants.OUTLINE_HANDLE_FILLCOLOR = '#99ccff';
		mxConstants.OUTLINE_HANDLE_STROKECOLOR = '#00a8ff';
		mxConstants.CONNECT_HANDLE_FILLCOLOR = '#cee7ff';
		mxConstants.EDGE_SELECTION_COLOR = '#00a8ff';
		mxConstants.DEFAULT_VALID_COLOR = '#00a8ff';
		mxConstants.LABEL_HANDLE_FILLCOLOR = '#cee7ff';
		mxConstants.GUIDE_COLOR = '#0088cf';
		mxConstants.DEFAULT_HOTSPOT = 0.3;
		
		//Enables snapping to off-grid terminals for edge waypoints
		mxEdgeHandler.prototype.snapToTerminals = true;
	
		//Enables guides
		mxGraphHandler.prototype.guidesEnabled = true;
	
		//Alt-move disables guides
		mxGuide.prototype.isEnabledForEvent = function(evt)
		{
			return !mxEvent.isAltDown(evt);
		};
		
		// Extends connection handler to enable ctrl+drag for cloning source cell
		// since copyOnConnect is now disabled by default
		var mxConnectionHandlerCreateTarget = mxConnectionHandler.prototype.isCreateTarget;
		mxConnectionHandler.prototype.isCreateTarget = function(evt)
		{
			return mxEvent.isControlDown(evt) || mxConnectionHandlerCreateTarget.apply(this, arguments);
		};
		
		/**
		 * Contains the default style for edges.
		 */
		Graph.prototype.defaultEdgeStyle = {'edgeStyle': 'orthogonalEdgeStyle', 'rounded': '0', 'html': '1'};
		
		/**
		 * Contains the current style for edges.
		 */
		Graph.prototype.currentEdgeStyle = Graph.prototype.defaultEdgeStyle;
		
		/**
		 * Contains the current style for vertices.
		 */
		Graph.prototype.currentVertexStyle = {};

		/**
		 * Returns the current edge style as a string.
		 */
		Graph.prototype.createCurrentEdgeStyle = function()
		{
			var style = 'edgeStyle=' + (this.currentEdgeStyle['edgeStyle'] || 'none') + ';';
			
			if (this.currentEdgeStyle['shape'] != null)
			{
				style += 'shape=' + this.currentEdgeStyle['shape'] + ';';
			}
			
			if (this.currentEdgeStyle['curved'] != null)
			{
				style += 'curved=' + this.currentEdgeStyle['curved'] + ';';
			}
			
			if (this.currentEdgeStyle['rounded'] != null)
			{
				style += 'rounded=' + this.currentEdgeStyle['rounded'] + ';';
			}
			
			// Special logic for custom property of elbowEdgeStyle
			if (this.currentEdgeStyle['edgeStyle'] == 'elbowEdgeStyle' && this.currentEdgeStyle['elbow'] != null)
			{
				style += 'elbow=' + this.currentEdgeStyle['elbow'] + ';';
			}
			
			if (this.currentEdgeStyle['html'] != null)
			{
				style += 'html=' + this.currentEdgeStyle['html'] + ';';
			}
			else
			{
				style += 'html=1;';
			}
			
			return style;
		};
	
		/**
		 * Hook for subclassers.
		 */
		Graph.prototype.getPagePadding = function()
		{
			return new mxPoint(0, 0);
		};
		
		/**
		 * Loads the stylesheet for this graph.
		 */
		Graph.prototype.loadStylesheet = function()
		{
		    var node = mxUtils.load(STYLE_PATH + '/default.xml').getDocumentElement();
			var dec = new mxCodec(node.ownerDocument);
			dec.decode(node, this.getStylesheet());
		};
		
		/**
		 * Overrides method to provide connection constraints for shapes.
		 */
		Graph.prototype.getAllConnectionConstraints = function(terminal, source)
		{
			if (terminal != null)
			{
				var constraints = mxUtils.getValue(terminal.style, 'points', null);
				
				if (constraints != null)
				{
					// Requires an array of arrays with x, y (0..1) and an optional
					// perimeter (0 or 1), eg. points=[[0,0,1],[0,1,0],[1,1]]
					var result = [];
					
					try
					{
						var c = JSON.parse(constraints);
						
						for (var i = 0; i < c.length; i++)
						{
							var tmp = c[i];
							result.push(new mxConnectionConstraint(new mxPoint(tmp[0], tmp[1]), (tmp.length > 2) ? tmp[2] != '0' : true));
						}
					}
					catch (e)
					{
						// ignore
					}
					
					return result;
				}
				else
				{
					if (terminal.shape != null)
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
				}
			}
		
			return null;
		};
		
		/**
		 * Inverts the elbow edge style without removing existing styles.
		 */
		Graph.prototype.flipEdge = function(edge)
		{
			if (edge != null)
			{
				var state = this.view.getState(edge);
				var style = (state != null) ? state.style : this.getCellStyle(edge);
				
				if (style != null)
				{
					var elbow = mxUtils.getValue(style, mxConstants.STYLE_ELBOW,
						mxConstants.ELBOW_HORIZONTAL);
					var value = (elbow == mxConstants.ELBOW_HORIZONTAL) ?
						mxConstants.ELBOW_VERTICAL : mxConstants.ELBOW_HORIZONTAL;
					this.setCellStyles(mxConstants.STYLE_ELBOW, value, [edge]);
				}
			}
		};
	
		/**
		 * Disables drill-down for non-swimlanes.
		 */
		Graph.prototype.isValidRoot = function(cell)
		{
			return this.isContainer(cell);
		};
		
		/**
		 * Disables drill-down for non-swimlanes.
		 */
		Graph.prototype.isValidDropTarget = function(cell)
		{
			var state = this.view.getState(cell);
			var style = (state != null) ? state.style : this.getCellStyle(cell);
		
			return mxUtils.getValue(style, 'part', '0') != '1' && (this.isContainer(cell) ||
				(mxGraph.prototype.isValidDropTarget.apply(this, arguments) &&
				mxUtils.getValue(style, 'dropTarget', '1') != '0'));
		};
	
		/**
		 * Overrides createGroupCell to set the group style for new groups to 'group'.
		 */
		Graph.prototype.createGroupCell = function()
		{
			var group = mxGraph.prototype.createGroupCell.apply(this, arguments);
			group.setStyle('group');
			
			return group;
		};
		
		/**
		 * Disables extending parents with stack layouts on add
		 */
		Graph.prototype.isExtendParentsOnAdd = function(cell)
		{
			var result = mxGraph.prototype.isExtendParentsOnAdd.apply(this, arguments);
			
			if (result && cell != null && this.layoutManager != null)
			{
				var parent = this.model.getParent(cell);
				
				if (parent != null)
				{
					var layout = this.layoutManager.getLayout(parent);
					
					if (layout != null && layout.constructor == mxStackLayout)
					{
						result = false;
					}
				}
			}
			
			return result;
		};
	
		/**
		 * Overrides tooltips to show custom tooltip or metadata.
		 */
		Graph.prototype.getTooltipForCell = function(cell)
		{
			var tip = '';
			
			if (mxUtils.isNode(cell.value))
			{
				var tmp = cell.value.getAttribute('tooltip');
				
				if (tmp != null)
				{
					if (tmp != null && this.isReplacePlaceholders(cell))
					{
						tmp = this.replacePlaceholders(cell, tmp);
					}
					
					tip = this.sanitizeHtml(tmp);
				}
				else
				{
					var ignored = ['label', 'tooltip', 'placeholders'];
					var attrs = cell.value.attributes;
					
					// Hides links in edit mode
					if (this.isEnabled())
					{
						ignored.push('link');
					}
					
					for (var i = 0; i < attrs.length; i++)
					{
						if (mxUtils.indexOf(ignored, attrs[i].nodeName) < 0 && attrs[i].nodeValue.length > 0)
						{
							var key = attrs[i].nodeName.substring(0, 1).toUpperCase() + attrs[i].nodeName.substring(1);
							tip += key + ': ' + mxUtils.htmlEntities(attrs[i].nodeValue) + '\n';
						}
					}
					
					if (tip.length > 0)
					{
						tip = tip.substring(0, tip.length - 1);
					}
				}
			}
			
			return tip;
		};
		
		/**
		 * Overrides autosize to add a border.
		 */
		Graph.prototype.getPreferredSizeForCell = function(cell)
		{
			var result = mxGraph.prototype.getPreferredSizeForCell.apply(this, arguments);
			
			// Adds buffer
			if (result != null)
			{
				result.width += 10;
				result.height += 4;
				
				if (this.gridEnabled)
				{
					result.width = this.snap(result.width);
					result.height = this.snap(result.height);
				}
			}
			
			return result;
		}
		
		/**
		 * Removes all illegal control characters with ASCII code <32 except TAB, LF
		 * and CR.
		 */
		Graph.prototype.zapGremlins = function(text)
		{
			var checked = [];
			
			for (var i = 0; i < text.length; i++)
			{
				var code = text.charCodeAt(i);
				
				// Removes all control chars except TAB, LF and CR
				if (code >= 32 || code == 9 || code == 10 || code == 13)
				{
					checked.push(text.charAt(i));
				}
			}
			
			return checked.join('');
		};
		
		/**
		 * Turns the given cells and returns the changed cells.
		 */
		Graph.prototype.turnShapes = function(cells)
		{
			var model = this.getModel();
			var select = [];
			
			model.beginUpdate();
			try
			{
				for (var i = 0; i < cells.length; i++)
				{
					var cell = cells[i];
					
					if (model.isEdge(cell))
					{
						var src = model.getTerminal(cell, true);
						var trg = model.getTerminal(cell, false);
						
						model.setTerminal(cell, trg, true);
						model.setTerminal(cell, src, false);
						
						var geo = model.getGeometry(cell);
						
						if (geo != null)
						{
							geo = geo.clone();
							
							if (geo.points != null)
							{
								geo.points.reverse();
							}
							
							var sp = geo.getTerminalPoint(true);
							var tp = geo.getTerminalPoint(false)
							
							geo.setTerminalPoint(sp, false);
							geo.setTerminalPoint(tp, true);
							model.setGeometry(cell, geo);
							
							// Inverts constraints
							var edgeState = this.view.getState(cell);
							var sourceState = this.view.getState(src);
							var targetState = this.view.getState(trg);
							
							if (edgeState != null)
							{
								var sc = (sourceState != null) ? this.getConnectionConstraint(edgeState, sourceState, true) : null;
								var tc = (targetState != null) ? this.getConnectionConstraint(edgeState, targetState, false) : null;
								
								this.setConnectionConstraint(cell, src, true, tc);
								this.setConnectionConstraint(cell, trg, false, sc);
							}
		
							select.push(cell);
						}
					}
					else if (model.isVertex(cell))
					{
						var geo = this.getCellGeometry(cell);
			
						if (geo != null)
						{
							// Rotates the size and position in the geometry
							geo = geo.clone();
							geo.x += geo.width / 2 - geo.height / 2;
							geo.y += geo.height / 2 - geo.width / 2;
							var tmp = geo.width;
							geo.width = geo.height;
							geo.height = tmp;
							model.setGeometry(cell, geo);
							
							// Reads the current direction and advances by 90 degrees
							var state = this.view.getState(cell);
							
							if (state != null)
							{
								var dir = state.style[mxConstants.STYLE_DIRECTION] || 'east'/*default*/;
								
								if (dir == 'east')
								{
									dir = 'south';
								}
								else if (dir == 'south')
								{
									dir = 'west';
								}
								else if (dir == 'west')
								{
									dir = 'north';
								}
								else if (dir == 'north')
								{
									dir = 'east';
								}
								
								this.setCellStyles(mxConstants.STYLE_DIRECTION, dir, [cell]);
							}
		
							select.push(cell);
						}
					}
				}
			}
			finally
			{
				model.endUpdate();
			}
			
			return select;
		};
		
		/**
		 * Updates the child cells with placeholders if metadata of a cell has changed.
		 */
		Graph.prototype.processChange = function(change)
		{
			mxGraph.prototype.processChange.apply(this, arguments);
			
			if (change instanceof mxValueChange && change.cell.value != null &&
				typeof(change.cell.value) == 'object')
			{
				// Invalidates all descendants with placeholders
				var desc = this.model.getDescendants(change.cell);
				
				// LATER: Check if only label or tooltip have changed
				if (desc.length > 0)
				{
					for (var i = 0; i < desc.length; i++)
					{
						if (this.isReplacePlaceholders(desc[i]))
						{
							this.view.invalidate(desc[i], false, false);
						}
					}
				}
			}
		};
		
		/**
		 * Handles label changes for XML user objects.
		 */
		Graph.prototype.cellLabelChanged = function(cell, value, autoSize)
		{
			// Removes all illegal control characters in user input
			value = this.zapGremlins(value);
			
			if (cell.value != null && typeof(cell.value) == 'object')
			{
				var tmp = cell.value.cloneNode(true);
				tmp.setAttribute('label', value);
				value = tmp;
			}
			
			mxGraph.prototype.cellLabelChanged.apply(this, arguments);
		};
		
		/**
		 * Sets the link for the given cell.
		 */
		Graph.prototype.setLinkForCell = function(cell, link)
		{
			this.setAttributeForCell(cell, 'link', link);
		};
		
		/**
		 * Sets the link for the given cell.
		 */
		Graph.prototype.setTooltipForCell = function(cell, link)
		{
			this.setAttributeForCell(cell, 'tooltip', link);
		};
		
		/**
		 * Sets the link for the given cell.
		 */
		Graph.prototype.setAttributeForCell = function(cell, attributeName, attributeValue)
		{
			var value = null;
			
			if (cell.value != null && typeof(cell.value) == 'object')
			{
				value = cell.value.cloneNode(true);
			}
			else
			{
				var doc = mxUtils.createXmlDocument();
				
				value = doc.createElement('UserObject');
				value.setAttribute('label', cell.value || '');
			}
			
			if (attributeValue != null && attributeValue.length > 0)
			{
				value.setAttribute(attributeName, attributeValue);
			}
			else
			{
				value.removeAttribute(attributeName);
			}
			
			this.model.setValue(cell, value);
		};
		
		/**
		 * Overridden to stop moving edge labels between cells.
		 */
		Graph.prototype.getDropTarget = function(cells, evt, cell, clone)
		{
			var model = this.getModel();
			
			// Disables drop into group if alt is pressed
			if (mxEvent.isAltDown(evt))
			{
				return null;
			}
			
			// Disables dragging edge labels out of edges
			for (var i = 0; i < cells.length; i++)
			{
				if (this.model.isEdge(this.model.getParent(cells[i])))
				{
					return null;
				}
			}
			
			return mxGraph.prototype.getDropTarget.apply(this, arguments);
		};
	
		/**
		 * Overrides double click handling to avoid accidental inserts of new labels in dblClick below.
		 */
		Graph.prototype.click = function(me)
		{
			mxGraph.prototype.click.call(this, me);
			
			// Stores state and source for checking in dblClick
			this.firstClickState = me.getState();
			this.firstClickSource = me.getSource();
		};
		
		/**
		 * Overrides double click handling to add the tolerance and inserting text.
		 */
		Graph.prototype.dblClick = function(evt, cell)
		{
			if (this.isEnabled())
			{
				var pt = mxUtils.convertPoint(this.container, mxEvent.getClientX(evt), mxEvent.getClientY(evt));
		
				// Automatically adds new child cells to edges on double click
				if (evt != null && !this.model.isVertex(cell))
				{
					var state = (this.model.isEdge(cell)) ? this.view.getState(cell) : null;
					var src = mxEvent.getSource(evt);
					
					if (this.firstClickState == state && this.firstClickSource == src)
					{
						if (state == null || (state.text == null || state.text.node == null ||
							(!mxUtils.contains(state.text.boundingBox, pt.x, pt.y) &&
							!mxUtils.isAncestorNode(state.text.node, mxEvent.getSource(evt)))))
						{
							if ((state == null && !this.isCellLocked(this.getDefaultParent())) ||
								(state != null && !this.isCellLocked(state.cell)))
							{
								// Avoids accidental inserts on background
								if (state != null || (mxClient.IS_VML && src == this.view.getCanvas()) ||
									(mxClient.IS_SVG && src == this.view.getCanvas().ownerSVGElement))
								{
									cell = this.addText(pt.x, pt.y, state);
								}
							}
						}
					}
				}
			
				mxGraph.prototype.dblClick.call(this, evt, cell);
			}
		};
		
		/**
		 * Returns a point that specifies the location for inserting cells.
		 */
		Graph.prototype.getInsertPoint = function()
		{
			var gs = this.getGridSize();
			var dx = this.container.scrollLeft / this.view.scale - this.view.translate.x;
			var dy = this.container.scrollTop / this.view.scale - this.view.translate.y;
			
			if (this.pageVisible)
			{
				var layout = this.getPageLayout();
				var page = this.getPageSize();
				dx = Math.max(dx, layout.x * page.width);
				dy = Math.max(dy, layout.y * page.height);
			}
			
			return new mxPoint(this.snap(dx + gs), this.snap(dy + gs));
		};
		
		/**
		 * Adds a new label at the given position and returns the new cell. State is
		 * an optional edge state to be used as the parent for the label. Vertices
		 * are not allowed currently as states.
		 */
		Graph.prototype.addText = function(x, y, state)
		{
			// Creates a new edge label with a predefined text
			var label = new mxCell();
			label.value = 'Text';
			label.style = 'text;html=1;resizable=0;points=[];'
			label.geometry = new mxGeometry(0, 0, 0, 0);
			label.vertex = true;
			
			if (state != null)
			{
				label.style += ';align=center;verticalAlign=middle;labelBackgroundColor=#ffffff;'
				label.geometry.relative = true;
				label.connectable = false;
				
				// Resets the relative location stored inside the geometry
				var pt2 = this.view.getRelativePoint(state, x, y);
				label.geometry.x = Math.round(pt2.x * 10000) / 10000;
				label.geometry.y = Math.round(pt2.y);
				
				// Resets the offset inside the geometry to find the offset from the resulting point
				label.geometry.offset = new mxPoint(0, 0);
				pt2 = this.view.getPoint(state, label.geometry);
			
				var scale = this.view.scale;
				label.geometry.offset = new mxPoint(Math.round((x - pt2.x) / scale), Math.round((y - pt2.y) / scale));
			}
			else
			{
				label.style += 'autosize=1;align=left;verticalAlign=top;spacingTop=-4;'
		
				var tr = this.view.translate;
				label.geometry.width = 40;
				label.geometry.height = 20;
				label.geometry.x = Math.round(x / this.view.scale) - tr.x;
				label.geometry.y = Math.round(y / this.view.scale) - tr.y;
			}
				
			this.getModel().beginUpdate();
			try
			{
				this.addCells([label], (state != null) ? state.cell : null);
				this.fireEvent(new mxEventObject('textInserted', 'cells', [label]));
				// Updates size of text after possible change of style via event
				this.autoSizeCell(label);
			}
			finally
			{
				this.getModel().endUpdate();
			}
			
			return label;
		};
		
		/**
		 * Duplicates the given cells and returns the duplicates.
		 */
		Graph.prototype.duplicateCells = function(cells, append)
		{
			cells = (cells != null) ? cells : this.getSelectionCells();
			append = (append != null) ? append : true;
			
			cells = this.model.getTopmostCells(cells);
			
			var model = this.getModel();
			var s = this.gridSize;
			var select = [];
			
			model.beginUpdate();
			try
			{
				var clones = this.cloneCells(cells, false);
				
				for (var i = 0; i < cells.length; i++)
				{
					var parent = model.getParent(cells[i]);
					var child = this.moveCells([clones[i]], s, s, false, parent)[0]; 
					select.push(child);
					
					if (append)
					{
						model.add(parent, clones[i]);
					}
					else
					{
						// Maintains child index by inserting after cloned in parent
						var index = parent.getIndex(cells[i]);
						model.add(parent, clones[i], index + 1);
					}
				}
			}
			finally
			{
				model.endUpdate();
			}
			
			return select;
		};
		
		/**
		 * Inserts the given image at the cursor in a content editable text box using
		 * the insertimage command on the document instance.
		 */
		Graph.prototype.insertImage = function(newValue, w, h)
		{
			// To find the new image, we create a list of all existing links first
			if (newValue != null)
			{
				var tmp = this.cellEditor.textarea.getElementsByTagName('img');
				var oldImages = [];
				
				for (var i = 0; i < tmp.length; i++)
				{
					oldImages.push(tmp[i]);
				}
				
				// LATER: Fix inserting link/image in IE8/quirks after focus lost
				document.execCommand('insertimage', false, newValue);
				
				// Sets size of new image
				var newImages = this.cellEditor.textarea.getElementsByTagName('img');
				
				if (newImages.length == oldImages.length + 1)
				{
					// Inverse order in favor of appended images
					for (var i = newImages.length - 1; i >= 0; i--)
					{
						if (i == 0 || newImages[i] != oldImages[i - 1])
						{
							// Workaround for lost styles during undo and redo is using attributes
							newImages[i].setAttribute('width', w);
							newImages[i].setAttribute('height', h);
							
							break;
						}
					}
				}
			}
		};
		
		/**
		 * 
		 * @param cell
		 * @returns {Boolean}
		 */
		Graph.prototype.isCellResizable = function(cell)
		{
			var result = mxGraph.prototype.isCellResizable.apply(this, arguments);
		
			var state = this.view.getState(cell);
			var style = (state != null) ? state.style : this.getCellStyle(cell);
				
			return result || (mxUtils.getValue(style, mxConstants.STYLE_RESIZABLE, '1') != '0' &&
				style[mxConstants.STYLE_WHITE_SPACE] == 'wrap');
		};
		
		/**
		 * Function: alignCells
		 * 
		 * Aligns the given cells vertically or horizontally according to the given
		 * alignment using the optional parameter as the coordinate.
		 * 
		 * Parameters:
		 * 
		 * horizontal - Boolean that specifies the direction of the distribution.
		 * cells - Optional array of <mxCells> to be distributed. Edges are ignored.
		 */
		Graph.prototype.distributeCells = function(horizontal, cells)
		{
			if (cells == null)
			{
				cells = this.getSelectionCells();
			}
			
			if (cells != null && cells.length > 1)
			{
				var vertices = [];
				var max = null;
				var min = null;
				
				for (var i = 0; i < cells.length; i++)
				{
					if (this.getModel().isVertex(cells[i]))
					{
						var state = this.view.getState(cells[i]);
						
						if (state != null)
						{
							var tmp = (horizontal) ? state.getCenterX() : state.getCenterY();
							max = (max != null) ? Math.max(max, tmp) : tmp;
							min = (min != null) ? Math.min(min, tmp) : tmp;
							
							vertices.push(state);
						}
					}
				}
				
				if (vertices.length > 2)
				{
					vertices.sort(function(a, b)
					{
						return (horizontal) ? a.x - b.x : a.y - b.y;
					});
		
					var t = this.view.translate;
					var s = this.view.scale;
					
					min = min / s - ((horizontal) ? t.x : t.y);
					max = max / s - ((horizontal) ? t.x : t.y);
					
					this.getModel().beginUpdate();
					try
					{
						var dt = (max - min) / (vertices.length - 1);
						var t0 = min;
						
						for (var i = 1; i < vertices.length - 1; i++)
						{
							var geo = this.getCellGeometry(vertices[i].cell);
							t0 += dt;
							
							if (geo != null)
							{
								geo = geo.clone();
								
								if (horizontal)
								{
									geo.x = Math.round(t0 - geo.width / 2);
								}
								else
								{
									geo.y = Math.round(t0 - geo.height / 2);
								}
								
								this.getModel().setGeometry(vertices[i].cell, geo);
							}
						}
					}
					finally
					{
						this.getModel().endUpdate();
					}
				}
			}
			
			return cells;
		};
		
		/**
		 * Adds meta-drag an Mac.
		 * @param evt
		 * @returns
		 */
		Graph.prototype.isCloneEvent = function(evt)
		{
			return (mxClient.IS_MAC && mxEvent.isMetaDown(evt)) || mxEvent.isControlDown(evt);
		};
		
		/**
		 * Translates this point by the given vector.
		 * 
		 * @param {number} dx X-coordinate of the translation.
		 * @param {number} dy Y-coordinate of the translation.
		 */
		Graph.prototype.encodeCells = function(cells)
		{
			var clones = this.cloneCells(cells);
			
			// Checks for orphaned relative children and makes absolute
			for (var i = 0; i < clones.length; i++)
			{
				var state = this.view.getState(cells[i]);
				
				if (state != null)
				{
					var geo = this.getCellGeometry(clones[i]);
					
					if (geo != null && geo.relative)
					{
						geo.relative = false;
						geo.x = state.x / state.view.scale - state.view.translate.x;
						geo.y = state.y / state.view.scale - state.view.translate.y;
					}
				}
			}
			
			var codec = new mxCodec();
			var model = new mxGraphModel();
			var parent = model.getChildAt(model.getRoot(), 0);
			
			for (var i = 0; i < cells.length; i++)
			{
				model.add(parent, clones[i]);
			}

			return codec.encode(model);
		};
		
		/**
		 * Translates this point by the given vector.
		 * 
		 * @param {number} dx X-coordinate of the translation.
		 * @param {number} dy Y-coordinate of the translation.
		 */
		Graph.prototype.getSvg = function(background, scale, border, nocrop, crisp, ignoreSelection, showText)
		{
			scale = (scale != null) ? scale : 1;
			border = (border != null) ? border : 1;
			crisp = (crisp != null) ? crisp : true;
			ignoreSelection = (ignoreSelection != null) ? ignoreSelection : true;
			showText = (showText != null) ? showText : true;

			var bounds = (nocrop) ? this.view.getBackgroundPageBounds() : (ignoreSelection) ?
					this.getGraphBounds() : this.getBoundingBox(this.getSelectionCells());

			if (bounds == null || bounds.width == 0 || bounds.height == 0)
			{
				throw Error(mxResources.get('drawingEmpty'));	
			}
			
			var imgExport = new mxImageExport();
			var vs = this.view.scale;
			
			// Prepares SVG document that holds the output
			var svgDoc = mxUtils.createXmlDocument();
			var root = (svgDoc.createElementNS != null) ?
		    		svgDoc.createElementNS(mxConstants.NS_SVG, 'svg') : svgDoc.createElement('svg');
		    
			if (background != null)
			{
				if (root.style != null)
				{
					root.style.backgroundColor = background;
				}
				else
				{
					root.setAttribute('style', 'background-color:' + background);
				}
			}
		    
			if (svgDoc.createElementNS == null)
			{
		    	root.setAttribute('xmlns', mxConstants.NS_SVG);
		    	root.setAttribute('xmlns:xlink', mxConstants.NS_XLINK);
			}
			else
			{
				// KNOWN: Ignored in IE9-11, adds namespace for each image element instead. No workaround.
				root.setAttributeNS('http://www.w3.org/2000/xmlns/', 'xmlns:xlink', mxConstants.NS_XLINK);
			}
			
			var s = scale / vs;
			root.setAttribute('width', (Math.ceil(bounds.width * s) + 2 * border) + 'px');
			root.setAttribute('height', (Math.ceil(bounds.height * s) + 2 * border) + 'px');
			root.setAttribute('version', '1.1');
			
		    // Adds group for anti-aliasing via transform
			var node = root;
			
			if (crisp)
			{
				var group = (svgDoc.createElementNS != null) ?
						svgDoc.createElementNS(mxConstants.NS_SVG, 'g') : svgDoc.createElement('g');
				group.setAttribute('transform', 'translate(0.5,0.5)');
				root.appendChild(group);
				svgDoc.appendChild(root);
				node = group;
			}
			else
			{
				svgDoc.appendChild(root);
			}
		
		    // Renders graph. Offset will be multiplied with state's scale when painting state.
			var svgCanvas = new mxSvgCanvas2D(node);
			svgCanvas.translate(Math.floor((border / scale - bounds.x) / vs), Math.floor((border / scale - bounds.y) / vs));
			
			// Paints background image
			var bgImg = this.backgroundImage;
			
			if (bgImg != null)
			{
				var s2 = vs / scale;
				var tr = this.view.translate;
				var tmp = new mxRectangle(tr.x * s2, tr.y * s2, bgImg.width * s2, bgImg.height * s2);
				
				// Checks if visible
				if (mxUtils.intersects(bounds, tmp))
				{
					svgCanvas.image(tr.x, tr.y, bgImg.width, bgImg.height, bgImg.src, true);
				}
			}
			
			svgCanvas.scale(s);
			svgCanvas.textEnabled = showText;
			
			// Adds hyperlinks (experimental)
			imgExport.getLinkForCellState = mxUtils.bind(this, function(state, canvas)
			{
				return this.getLinkForCell(state.cell);
			});
			
			// Implements ignoreSelection flag
			imgExport.drawCellState = function(state, canvas)
			{
				if (ignoreSelection || state.view.graph.isCellSelected(state.cell))
				{
					mxImageExport.prototype.drawCellState.apply(this, arguments);
				}
			};

			imgExport.drawState(this.getView().getState(this.model.root), svgCanvas);
		
			return root;
		};
		
		/**
		 * Returns the first ancestor of the current selection with the given name.
		 */
		Graph.prototype.getSelectedElement = function()
		{
			var node = null;
			
			if (window.getSelection)
			{
				var sel = window.getSelection();
				
			    if (sel.getRangeAt && sel.rangeCount)
			    {
			        var range = sel.getRangeAt(0);
			        node = range.commonAncestorContainer;
			    }
			}
			else if (document.selection)
			{
				node = document.selection.createRange().parentElement();
			}
			
			return node;
		};
		
		/**
		 * Returns the first ancestor of the current selection with the given name.
		 */
		Graph.prototype.getParentByName = function(node, name, stopAt)
		{
			while (node != null)
			{
				if (node.nodeName == name)
				{
					return node;
				}
		
				if (node == stopAt)
				{
					return null;
				}
				
				node = node.parentNode;
			}
			
			return node;
		};
		
		/**
		 * Selects the given node.
		 */
		Graph.prototype.selectNode = function(node)
		{
			var sel = null;
			
		    // IE9 and non-IE
			if (window.getSelection)
		    {
		    	sel = window.getSelection();
		    	
		        if (sel.getRangeAt && sel.rangeCount)
		        {
		        	var range = document.createRange();
		            range.selectNode(node);
		            sel.removeAllRanges();
		            sel.addRange(range);
		        }
		    }
		    // IE < 9
			else if ((sel = document.selection) && sel.type != 'Control')
		    {
		        var originalRange = sel.createRange();
		        originalRange.collapse(true);
		        var range = sel.createRange();
		        range.setEndPoint('StartToStart', originalRange);
		        range.select();
		    }
		};
		
		/**
		 * Inserts a new row into the given table.
		 */
		Graph.prototype.insertRow = function(table, index)
		{
			var bd = table.tBodies[0];
			var cols = (bd.rows.length > 0) ? bd.rows[0].cells.length : 1;
			var row = bd.insertRow(index);
			
			for (var i = 0; i < cols; i++)
			{
				mxUtils.br(row.insertCell(-1));
			}
			
			return row.cells[0];
		};
		
		/**
		 * Deletes the given column.
		 */
		Graph.prototype.deleteRow = function(table, index)
		{
			table.tBodies[0].deleteRow(index);
		};
		
		/**
		 * Deletes the given column.
		 */
		Graph.prototype.insertColumn = function(table, index)
		{
			var hd = table.tHead;
			
			if (hd != null)
			{
				// TODO: use colIndex
				for (var h = 0; h < hd.rows.length; h++)
				{
					var th = document.createElement('th');
					hd.rows[h].appendChild(th);
					mxUtils.br(th);
				}
			}
		
			var bd = table.tBodies[0];
			
			for (var i = 0; i < bd.rows.length; i++)
			{
				var cell = bd.rows[i].insertCell(index);
				mxUtils.br(cell);
			}
			
			return bd.rows[0].cells[(index >= 0) ? index : bd.rows[0].cells.length - 1];
		};
		
		/**
		 * Deletes the given column.
		 */
		Graph.prototype.deleteColumn = function(table, index)
		{
			if (index >= 0)
			{
				var bd = table.tBodies[0];
				var rows = bd.rows;
				
				for (var i = 0; i < rows.length; i++)
				{
					if (rows[i].cells.length > index)
					{
						rows[i].deleteCell(index);
					}
				}
			}
		};
		
		/**
		 * Inserts the given HTML at the caret position (no undo).
		 */
		Graph.prototype.pasteHtmlAtCaret = function(html)
		{
		    var sel, range;
		
			// IE9 and non-IE
		    if (window.getSelection)
		    {
		        sel = window.getSelection();
		        
		        if (sel.getRangeAt && sel.rangeCount)
		        {
		            range = sel.getRangeAt(0);
		            range.deleteContents();
		
		            // Range.createContextualFragment() would be useful here but is
		            // only relatively recently standardized and is not supported in
		            // some browsers (IE9, for one)
		            var el = document.createElement("div");
		            el.innerHTML = html;
		            var frag = document.createDocumentFragment(), node;
		            
		            while ((node = el.firstChild))
		            {
		                lastNode = frag.appendChild(node);
		            }
		            
		            range.insertNode(frag);
		        }
		    }
		    // IE < 9
		    else if ((sel = document.selection) && sel.type != "Control")
		    {
		    	// FIXME: Does not work if selection is empty
		        sel.createRange().pasteHTML(html);
		    }
		};
	
		/**
		 * Customized graph for touch devices.
		 */
		Graph.prototype.initTouch = function()
		{
			// Disables new connections via "hotspot"
			this.connectionHandler.marker.isEnabled = function()
			{
				return this.graph.connectionHandler.first != null;
			};
		
			// Hides menu when editing starts
			this.addListener(mxEvent.START_EDITING, function(sender, evt)
			{
				this.popupMenuHandler.hideMenu();
			});
		
			// Adds custom hit detection if native hit detection found no cell
			var graphUpdateMouseEvent = this.updateMouseEvent;
			this.updateMouseEvent = function(me)
			{
				me = graphUpdateMouseEvent.apply(this, arguments);
	
				if (mxEvent.isTouchEvent(me.getEvent()) && me.getState() == null)
				{
					var cell = this.getCellAt(me.graphX, me.graphY);
		
					if (cell != null && this.isSwimlane(cell) && this.hitsSwimlaneContent(cell, me.graphX, me.graphY))
					{
						cell = null;
					}
					else
					{
						me.state = this.view.getState(cell);
						
						if (me.state != null && me.state.shape != null)
						{
							this.container.style.cursor = me.state.shape.node.style.cursor;
						}
					}
				}
				
				if (me.getState() == null)
				{
					this.container.style.cursor = 'default';
				}
				
				return me;
			};
		
			// Context menu trigger implementation depending on current selection state
			// combined with support for normal popup trigger.
			var cellSelected = false;
			var selectionEmpty = false;
			var menuShowing = false;
			
			var oldFireMouseEvent = this.fireMouseEvent;
			
			this.fireMouseEvent = function(evtName, me, sender)
			{
				if (evtName == mxEvent.MOUSE_DOWN)
				{
					// For hit detection on edges
					me = this.updateMouseEvent(me);
					
					cellSelected = this.isCellSelected(me.getCell());
					selectionEmpty = this.isSelectionEmpty();
					menuShowing = this.popupMenuHandler.isMenuShowing();
				}
				
				oldFireMouseEvent.apply(this, arguments);
			};
			
			// Shows popup menu if cell was selected or selection was empty and background was clicked
			// FIXME: Conflicts with mxPopupMenuHandler.prototype.getCellForPopupEvent in Editor.js by
			// selecting parent for selected children in groups before this check can be made.
			this.popupMenuHandler.mouseUp = mxUtils.bind(this, function(sender, me)
			{
				this.popupMenuHandler.popupTrigger = !this.isEditing() && (this.popupMenuHandler.popupTrigger ||
					(!menuShowing && !mxEvent.isMouseEvent(me.getEvent()) &&
					((selectionEmpty && me.getCell() == null && this.isSelectionEmpty()) ||
					(cellSelected && this.isCellSelected(me.getCell())))));
				mxPopupMenuHandler.prototype.mouseUp.apply(this.popupMenuHandler, arguments);
			});
		};
		
		/**
		 * HTML in-place editor
		 */
		mxCellEditor.prototype.isContentEditing = function()
		{
			var state = this.graph.view.getState(this.editingCell);
			
			return state != null && state.style['html'] == 1;
		};
	
		/**
		 * Creates the keyboard event handler for the current graph and history.
		 */
		mxCellEditor.prototype.saveSelection = function()
		{
		    if (window.getSelection)
		    {
		        sel = window.getSelection();
		        
		        if (sel.getRangeAt && sel.rangeCount)
		        {
		            var ranges = [];
		            
		            for (var i = 0, len = sel.rangeCount; i < len; ++i)
		            {
		                ranges.push(sel.getRangeAt(i));
		            }
		            
		            return ranges;
		        }
		    }
		    else if (document.selection && document.selection.createRange)
		    {
		        return document.selection.createRange();
		    }
		    
		    return null;
		};
	
		/**
		 * Creates the keyboard event handler for the current graph and history.
		 */
		mxCellEditor.prototype.restoreSelection = function(savedSel)
		{
			if (savedSel)
			{
				if (window.getSelection)
				{
					sel = window.getSelection();
					sel.removeAllRanges();
	
					for (var i = 0, len = savedSel.length; i < len; ++i)
					{
						sel.addRange(savedSel[i]);
					}
				}
				else if (document.selection && savedSel.select)
				{
					savedSel.select();
				}
			}
		};
	
		/**
		 * Handling of special nl2Br style for not converting newlines to breaks in HTML labels.
		 * NOTE: Since it's easier to set this when the label is created we assume that it does
		 * not change during the lifetime of the mxText instance.
		 */
		var mxCellRendererInitializeLabel = mxCellRenderer.prototype.initializeLabel;
		mxCellRenderer.prototype.initializeLabel = function(state)
		{
			if (state.text != null)
			{
				state.text.replaceLinefeeds = mxUtils.getValue(state.style, 'nl2Br', '1') != '0';
			}
			
			mxCellRendererInitializeLabel.apply(this, arguments);
		};
	
		var mxConstraintHandlerUpdate = mxConstraintHandler.prototype.update;
		mxConstraintHandler.prototype.update = function(me, source)
		{
			if (this.isKeepFocusEvent(me) || !mxEvent.isAltDown(me.getEvent()))
			{
				mxConstraintHandlerUpdate.apply(this, arguments);
			}
			else
			{
				this.reset();
			}
		};
	
		/**
		 * No dashed shapes.
		 */
		mxGuide.prototype.createGuideShape = function(horizontal)
		{
			var guide = new mxPolyline([], mxConstants.GUIDE_COLOR, mxConstants.GUIDE_STROKEWIDTH);
			
			return guide;
		};
	
		/**
		 * HTML in-place editor
		 */
		var mxCellEditorStartEditing = mxCellEditor.prototype.startEditing;
		mxCellEditor.prototype.startEditing = function(cell, trigger)
		{
			mxCellEditorStartEditing.apply(this, arguments);
			
			// Overrides class in case of HTML content to add
			// dashed borders for divs and table cells
			var state = this.graph.view.getState(cell);
	
			if (state != null && state.style['html'] == 1)
			{
				this.textarea.className = 'mxCellEditor geContentEditable';
			}
			else
			{
				this.textarea.className = 'mxCellEditor mxPlainTextEditor';
			}
			
			// Toggles markup vs wysiwyg mode
			this.codeViewMode = false;
			
			// Stores current selection range when switching between markup and code
			this.switchSelectionState = null;
			
			// Selects editing cell
			this.graph.setSelectionCell(cell);
	
			// First run cannot set display before supercall because textarea is lazy created
			// Lazy instantiates textarea to save memory in IE
			if (this.textarea == null)
			{
				this.init();
			}
	
			// Enables focus outline for edges and edge labels
			var parent = this.graph.getModel().getParent(cell);
			var geo = this.graph.getCellGeometry(cell);
			
			if ((this.graph.getModel().isEdge(parent) && geo != null && geo.relative) ||
				this.graph.getModel().isEdge(cell))
			{
				// Quirks does not support outline at all so use border instead
				if (mxClient.IS_QUIRKS)
				{
					this.textarea.style.border = 'gray dotted 1px';
				}
				// IE>8 and FF on Windows uses outline default of none
				else if (mxClient.IS_IE || mxClient.IS_IE11 || (mxClient.IS_FF && mxClient.IS_WIN))
				{
					this.textarea.style.outline = 'gray dotted 1px';
				}
				else
				{
					this.textarea.style.outline = '';
				}
			}
			else if (mxClient.IS_QUIRKS)
			{
				this.textarea.style.outline = 'none';
				this.textarea.style.border = '';
			}
		}

		/**
		 * HTML in-place editor
		 */
		var cellEditorInstallListeners = mxCellEditor.prototype.installListeners;
		mxCellEditor.prototype.installListeners = function(elt)
		{
			cellEditorInstallListeners.apply(this, arguments);

			// Adds a reference from the clone to the original node, recursively
			function reference(node, clone)
			{
				clone.originalNode = node;
				
				node = node.firstChild;
				var child = clone.firstChild;
				
				while (node != null && child != null)
				{
					reference(node, child);
					node = node.nextSibling;
					child = child.nextSibling;
				}
				
				return clone;
			};
			
			// Checks the given node for new nodes, recursively
			function checkNode(node, clone)
			{
				if (clone.originalNode != node)
				{
					cleanNode(node);
				}
				else
				{
					node = node.firstChild;
					clone = clone.firstChild;
					
					while (node != null)
					{
						var nextNode = node.nextSibling;
						
						if (clone == null)
						{
							cleanNode(node);
						}
						else
						{
							checkNode(node, clone);
							clone = clone.nextSibling;
						}

						node = nextNode;
					}
				}
			};

			// Removes unused DOM nodes and attributes, recursively
			function cleanNode(node)
			{
				var child = node.firstChild;
				
				while (child != null)
				{
					var next = child.nextSibling;
					cleanNode(child);
					child = next;
				}
				
				if ((node.nodeType != 1 || (node.nodeName !== 'BR' && node.firstChild == null)) &&
					(node.nodeType != 3 || mxUtils.trim(mxUtils.getTextContent(node)).length == 0))
				{
					node.parentNode.removeChild(node);
				}
				else
				{
					// Removes linefeeds
					if (node.nodeType == 3)
					{
						mxUtils.setTextContent(node, mxUtils.getTextContent(node).replace(/\n|\r/g, ''));
					}

					// Removes CSS classes and styles (for Word and Excel)
					if (node.nodeType == 1)
					{
						node.removeAttribute('style');
						node.removeAttribute('class');
						node.removeAttribute('width');
						node.removeAttribute('cellpadding');
						node.removeAttribute('cellspacing');
						node.removeAttribute('border');
					}
				}
			};
			
			// Handles paste from Word, Excel etc by removing styles, classnames and unused nodes
			// LATER: Fix undo/redo for paste
			if (!mxClient.IS_QUIRKS && document.documentMode !== 7 && document.documentMode !== 8)
			{
				mxEvent.addListener(this.textarea, 'paste', mxUtils.bind(this, function(evt)
				{
					var clone = reference(this.textarea, this.textarea.cloneNode(true));
	
					window.setTimeout(mxUtils.bind(this, function()
					{
						checkNode(this.textarea, clone);
					}), 0);
				}));
			}
		};
		
		mxCellEditor.prototype.toggleViewMode = function()
		{
			var state = this.graph.view.getState(this.editingCell);
			var nl2Br = state != null && mxUtils.getValue(state.style, 'nl2Br', '1') != '0';
			var tmp = this.saveSelection();
			
			if (!this.codeViewMode)
			{
				// Clears the initial empty label on the first keystroke
				if (this.clearOnChange && this.textarea.innerHTML == this.getEmptyLabelText())
				{
					this.clearOnChange = false;
					this.textarea.innerHTML = '';
				}
				
				// Removes newlines from HTML and converts breaks to newlines
				// to match the HTML output in plain text
				var content = mxUtils.htmlEntities(this.textarea.innerHTML);
	
			    // Workaround for trailing line breaks being ignored in the editor
				if (!mxClient.IS_QUIRKS && document.documentMode != 8)
				{
					content = mxUtils.replaceTrailingNewlines(content, '<div><br></div>');
				}
				
			    content = this.graph.sanitizeHtml((nl2Br) ? content.replace(/\n/g, '').replace(/&lt;br\s*.?&gt;/g, '<br>') : content);
				this.textarea.className = 'mxCellEditor mxPlainTextEditor';
				
				var size = mxConstants.DEFAULT_FONTSIZE;
				
				this.textarea.style.lineHeight = (mxConstants.ABSOLUTE_LINE_HEIGHT) ? Math.round(size * mxConstants.LINE_HEIGHT) + 'px' : mxConstants.LINE_HEIGHT;
				this.textarea.style.fontSize = Math.round(size) + 'px';
				this.textarea.style.textDecoration = '';
				this.textarea.style.fontWeight = 'normal';
				this.textarea.style.fontStyle = '';
				this.textarea.style.fontFamily = mxConstants.DEFAULT_FONTFAMILY;
				this.textarea.style.textAlign = 'left';
				
				// Adds padding to make cursor visible with borders
				this.textarea.style.padding = '2px';
				
				if (this.textarea.innerHTML != content)
				{
					this.textarea.innerHTML = content;
				}
	
				this.codeViewMode = true;
			}
			else
			{
				var content = mxUtils.extractTextWithWhitespace(this.textarea.childNodes);
			    
				// Strips trailing line break
			    if (content.length > 0 && content.charAt(content.length - 1) == '\n')
			    {
			    	content = content.substring(0, content.length - 1);
			    }
			    
				content = this.graph.sanitizeHtml((nl2Br) ? content.replace(/\n/g, '<br/>') : content)
				this.textarea.className = 'mxCellEditor geContentEditable';
				
				var size = mxUtils.getValue(state.style, mxConstants.STYLE_FONTSIZE, mxConstants.DEFAULT_FONTSIZE);
				var family = mxUtils.getValue(state.style, mxConstants.STYLE_FONTFAMILY, mxConstants.DEFAULT_FONTFAMILY);
				var align = mxUtils.getValue(state.style, mxConstants.STYLE_ALIGN, mxConstants.ALIGN_LEFT);
				var bold = (mxUtils.getValue(state.style, mxConstants.STYLE_FONTSTYLE, 0) &
						mxConstants.FONT_BOLD) == mxConstants.FONT_BOLD;
				var italic = (mxUtils.getValue(state.style, mxConstants.STYLE_FONTSTYLE, 0) &
						mxConstants.FONT_ITALIC) == mxConstants.FONT_ITALIC;
				var uline = (mxUtils.getValue(state.style, mxConstants.STYLE_FONTSTYLE, 0) &
						mxConstants.FONT_UNDERLINE) == mxConstants.FONT_UNDERLINE;
				
				this.textarea.style.lineHeight = (mxConstants.ABSOLUTE_LINE_HEIGHT) ? Math.round(size * mxConstants.LINE_HEIGHT) + 'px' : mxConstants.LINE_HEIGHT;
				this.textarea.style.fontSize = Math.round(size) + 'px';
				this.textarea.style.textDecoration = (uline) ? 'underline' : '';
				this.textarea.style.fontWeight = (bold) ? 'bold' : 'normal';
				this.textarea.style.fontStyle = (italic) ? 'italic' : '';
				this.textarea.style.fontFamily = family;
				this.textarea.style.textAlign = align;
				this.textarea.style.padding = '0px';
				
				if (this.textarea.innerHTML != content)
				{
					this.textarea.innerHTML = content;
					
					if (this.textarea.innerHTML.length == 0)
					{
						this.textarea.innerHTML = this.getEmptyLabelText();
						this.clearOnChange = this.textarea.innerHTML.length > 0;
					}
				}
	
				this.codeViewMode = false;
			}
			
			this.textarea.focus();
		
			if (this.switchSelectionState != null)
			{
				this.restoreSelection(this.switchSelectionState);
			}
			
			this.switchSelectionState = tmp;
			this.resize();
		};
		
		var mxCellEditorResize = mxCellEditor.prototype.resize;
		mxCellEditor.prototype.resize = function(state, trigger)
		{
			var state = this.graph.getView().getState(this.editingCell);
			
			if (this.codeViewMode && state != null)
			{
				var scale = state.view.scale;
				this.bounds = mxRectangle.fromRectangle(state);
				
				// General placement of code editor if cell has no size
				// LATER: Fix HTML editor bounds for edge labels
				if (this.bounds.width == 0 && this.bounds.height == 0)
				{
					this.bounds.width = 160 * scale;
					this.bounds.height = 60 * scale;
					
					var m = (state.text != null) ? state.text.margin : null;
					
					if (m == null)
					{
						m = mxUtils.getAlignmentAsPoint(mxUtils.getValue(state.style, mxConstants.STYLE_ALIGN, mxConstants.ALIGN_CENTER),
								mxUtils.getValue(state.style, mxConstants.STYLE_VERTICAL_ALIGN, mxConstants.ALIGN_MIDDLE));
					}
					
					this.bounds.x += m.x * this.bounds.width;
					this.bounds.y += m.y * this.bounds.height;
				}
	
				this.textarea.style.width = Math.round((this.bounds.width - 4) / scale) + 'px';
				this.textarea.style.height = Math.round((this.bounds.height - 4) / scale) + 'px';
				this.textarea.style.overflow = 'auto';
	
				// Adds scrollbar offset if visible
				if (this.textarea.clientHeight < this.textarea.offsetHeight)
				{
					this.textarea.style.height = Math.round((this.bounds.height / scale)) + (this.textarea.offsetHeight - this.textarea.clientHeight) + 'px';
					this.bounds.height = parseInt(this.textarea.style.height) * scale;
				}
				
				if (this.textarea.clientWidth < this.textarea.offsetWidth)
				{
					this.textarea.style.width = Math.round((this.bounds.width / scale)) + (this.textarea.offsetWidth - this.textarea.clientWidth) + 'px';
					this.bounds.width = parseInt(this.textarea.style.width) * scale;
				}
								
				// FIXME: Offset when scaled
				if (document.documentMode == 8)
				{
					this.textarea.style.left = Math.round(this.bounds.x) + 'px';
					this.textarea.style.top = Math.round(this.bounds.y) + 'px';
				}
				else if (mxClient.IS_QUIRKS)
				{
					this.textarea.style.left = Math.round(this.bounds.x) + 'px';
					this.textarea.style.top = Math.round(this.bounds.y) + 'px';
				}
				else
				{
					this.textarea.style.left = Math.round(this.bounds.x - 0.5 * this.bounds.width * (1 - scale) / scale) + 'px';
					this.textarea.style.top = Math.round(this.bounds.y - 0.5 * this.bounds.height * (1 - scale) / scale) + 'px';
				}
	
				if (mxClient.IS_VML)
				{
					this.textarea.style.zoom = scale;
				}
				else
				{
					mxUtils.setPrefixedStyle(this.textarea.style, 'transform', 'scale(' + scale + ',' + scale + ')');	
				}
			}
			else
			{
				this.textarea.style.height = '';
				this.textarea.style.overflow = '';
				mxCellEditorResize.apply(this, arguments);
			}
		};
		
		mxCellEditorGetInitialValue = mxCellEditor.prototype.getInitialValue;
		mxCellEditor.prototype.getInitialValue = function(state, trigger)
		{
			if (mxUtils.getValue(state.style, 'html', '0') == '0')
			{
				return mxCellEditorGetInitialValue.apply(this, arguments);
			}
			else
			{
				var result = this.graph.getEditingValue(state.cell, trigger)
			
				if (mxUtils.getValue(state.style, 'nl2Br', '1') == '1')
				{
					result = result.replace(/\n/g, '<br/>');
				}
				
				result = this.graph.sanitizeHtml(result);
				
				return result;
			}
		};
		
		mxCellEditorGetCurrentValue = mxCellEditor.prototype.getCurrentValue;
		mxCellEditor.prototype.getCurrentValue = function(state)
		{
			if (mxUtils.getValue(state.style, 'html', '0') == '0')
			{
				return mxCellEditorGetCurrentValue.apply(this, arguments);
			}
			else
			{
				var result = this.graph.sanitizeHtml(this.textarea.innerHTML);
	
				if (mxUtils.getValue(state.style, 'nl2Br', '1') == '1')
				{
					result = result.replace(/\r\n/g, '<br/>').replace(/\n/g, '<br/>');
				}
				else
				{
					result = result.replace(/\r\n/g, '').replace(/\n/g, '');
				}
				
				return result;
			}
		};
	
		var mxCellEditorStopEditing = mxCellEditor.prototype.stopEditing;
		mxCellEditor.prototype.stopEditing = function(cancel)
		{
			// Restores default view mode before applying value
			if (this.codeViewMode)
			{
				this.toggleViewMode();
			}
			
			mxCellEditorStopEditing.apply(this, arguments);
			
			// Tries to move focus back to container after editing if possible
			try
			{
				this.graph.container.focus();
			}
			catch (e)
			{
				// ignore
			}
		};
	
		var mxCellEditorApplyValue = mxCellEditor.prototype.applyValue;
		mxCellEditor.prototype.applyValue = function(state, value)
		{
			// Removes empty relative child labels in edges
			this.graph.getModel().beginUpdate();
			
			try
			{
				mxCellEditorApplyValue.apply(this, arguments);
				
				var stroke = mxUtils.getValue(state.style, mxConstants.STYLE_STROKECOLOR, mxConstants.NONE);
				var fill = mxUtils.getValue(state.style, mxConstants.STYLE_FILLCOLOR, mxConstants.NONE);
				
				if (mxUtils.trim(value || '') == '' && stroke == mxConstants.NONE && fill == mxConstants.NONE)
				{
					this.graph.removeCells([state.cell], false);
				}
			}
			finally
			{
				this.graph.getModel().endUpdate();
			}
		};

		/**
		 * Returns the background color to be used for the editing box. This returns
		 * the label background for edge labels and null for all other cases.
		 */
		mxCellEditor.prototype.getBackgroundColor = function(state)
		{
			var color = null;
			
			if (this.graph.getModel().isEdge(state.cell) || this.graph.getModel().isEdge(this.graph.getModel().getParent(state.cell)))
			{
				var color = mxUtils.getValue(state.style, mxConstants.STYLE_LABEL_BACKGROUNDCOLOR, null);
				
				if (color == mxConstants.NONE)
				{
					color = null;
				}
			}
			
			return color;
		};
		
		mxCellEditor.prototype.getMinimumSize = function(state)
		{
			var scale = this.graph.getView().scale;
			
			return new mxRectangle(0, 0, (state.text == null) ? 30 :  state.text.size * scale + 20, 30);
		};
		
		/**
		 * Hints on handlers
		 */
		function createHint()
		{
			var hint = document.createElement('div');
			hint.className = 'geHint';
			hint.style.whiteSpace = 'nowrap';
			hint.style.position = 'absolute';
			
			return hint;
		};
		
		/**
		 * Updates the hint for the current operation.
		 */
		mxGraphHandler.prototype.updateHint = function(me)
		{
			if (this.shape != null)
			{
				if (this.hint == null)
				{
					this.hint = createHint();
					this.graph.container.appendChild(this.hint);
				}
	
				var t = this.graph.view.translate;
				var s = this.graph.view.scale;
				var x = this.roundLength((this.bounds.x + this.currentDx) / s - t.x);
				var y = this.roundLength((this.bounds.y + this.currentDy) / s - t.y);
				
				this.hint.innerHTML = x + ', ' + y;
	
				this.hint.style.left = (this.shape.bounds.x + Math.round((this.shape.bounds.width - this.hint.clientWidth) / 2)) + 'px';
				this.hint.style.top = (this.shape.bounds.y + this.shape.bounds.height + 12) + 'px';
			}
		};
	
		/**
		 * Updates the hint for the current operation.
		 */
		mxGraphHandler.prototype.removeHint = function()
		{
			if (this.hint != null)
			{
				this.hint.parentNode.removeChild(this.hint);
				this.hint = null;
			}
		};
	
		/**
		 * Enables recursive resize for groups.
		 */
		mxVertexHandler.prototype.isRecursiveResize = function(state, me)
		{
			return !this.graph.isSwimlane(state.cell) && this.graph.model.getChildCount(state.cell) > 0 &&
				!mxEvent.isControlDown(me.getEvent()) && !this.graph.isCellCollapsed(state.cell) &&
				mxUtils.getValue(state.style, 'recursiveResize', '1') == '1' &&
				mxUtils.getValue(state.style, 'childLayout', null) == null;
		};
	
		/**
		 * Updates the hint for the current operation.
		 */
		mxVertexHandler.prototype.updateHint = function(me)
		{
			if (this.index != mxEvent.LABEL_HANDLE)
			{
				if (this.hint == null)
				{
					this.hint = createHint();
					this.state.view.graph.container.appendChild(this.hint);
				}
	
				if (this.index == mxEvent.ROTATION_HANDLE)
				{
					this.hint.innerHTML = this.currentAlpha + '&deg;';
				}
				else
				{
					var s = this.state.view.scale;
					this.hint.innerHTML = this.roundLength(this.bounds.width / s) + ' x ' + this.roundLength(this.bounds.height / s);
				}
				
				var rot = (this.currentAlpha != null) ? this.currentAlpha : this.state.style[mxConstants.STYLE_ROTATION] || '0';
				var bb = mxUtils.getBoundingBox(this.bounds, rot);
				
				if (bb == null)
				{
					bb = this.bounds;
				}
				
				this.hint.style.left = bb.x + Math.round((bb.width - this.hint.clientWidth) / 2) + 'px';
				this.hint.style.top = (bb.y + bb.height + 12) + 'px';
			}
		};
	
		/**
		 * Updates the hint for the current operation.
		 */
		mxVertexHandler.prototype.removeHint = mxGraphHandler.prototype.removeHint;
	
		/**
		 * Updates the hint for the current operation.
		 */
		mxEdgeHandler.prototype.updateHint = function(me, point)
		{
			if (this.hint == null)
			{
				this.hint = createHint();
				this.state.view.graph.container.appendChild(this.hint);
			}
	
			var t = this.graph.view.translate;
			var s = this.graph.view.scale;
			var x = this.roundLength(point.x / s - t.x);
			var y = this.roundLength(point.y / s - t.y);
			
			this.hint.innerHTML = x + ', ' + y;
			this.hint.style.visibility = 'visible';
			
			if (this.isSource || this.isTarget)
			{
				if (this.constraintHandler.currentConstraint != null &&
					this.constraintHandler.currentFocus != null)
				{
					var pt = this.constraintHandler.currentConstraint.point;
					this.hint.innerHTML = '[' + Math.round(pt.x * 100) + '%, '+ Math.round(pt.y * 100) + '%]';
				}
				else if (this.marker.hasValidState())
				{
					this.hint.style.visibility = 'hidden';
				}
			}
			
			this.hint.style.left = Math.round(me.getGraphX() - this.hint.clientWidth / 2) + 'px';
			this.hint.style.top = (me.getGraphY() + 12) + 'px';
			
			if (this.hideEdgeHintThread != null)
			{
				window.clearTimeout(this.hideEdgeHintThread);
			}
			
			this.hideEdgeHintThread = window.setTimeout(mxUtils.bind(this, function()
			{
				if (this.hint != null)
				{
					this.hint.style.visibility = 'hidden';
				}
			}), 500);
		};
	
		/**
		 * Updates the hint for the current operation.
		 */
		mxEdgeHandler.prototype.removeHint = mxGraphHandler.prototype.removeHint;
	
		/**
		 * Defines the handles for the UI. Uses data-URIs to speed-up loading time where supported.
		 */
		var mainHandle = new mxImage((mxClient.IS_SVG) ? 'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABEAAAARCAYAAAA7bUf6AAAACXBIWXMAAAsTAAALEwEAmpwYAAABLUlEQVQ4y61US4rCQBBNeojiRrLSnbMOWWU3V1FPouARcgc9hyLOCSSbYZw5gRCIkM9KbevJaycS4zCOBY+iq6pf1y+xrNtiE6oEY/tVzMUXgSNoCJrUDu3qHpldutwSuIKOoEvt0m7I7DoCvNj2fb8XRdEojuN5lmVraJxhh59xFSLFF9phGL7lef6hRb63R73aHM8aAjv8JHJ47yqLlud5r0VRbHa51sPZQVuT/QU4ww4/4ljaJRubrC5SxouD6TWBQV/sEIkbs0eOIVGssSO1L5D6LQID+BHHZjdMSYpj7KZpun7/uk8CP5rNqTXLJP/OpNyTMWruP9CTP08nCILKdCp7gkCzJ8vPnz2BvW5PKhuLjJBykiQLaWIEjTP3o3Zjn/LtPO0rfvh/cgKu7z6wtPPltQAAAABJRU5ErkJggg==' :
			IMAGE_PATH + '/handle-main.png', 17, 17);
		var fixedHandle = new mxImage((mxClient.IS_SVG) ? 'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABEAAAARCAYAAAA7bUf6AAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAAyJpVFh0WE1MOmNvbS5hZG9iZS54bXAAAAAAADw/eHBhY2tldCBiZWdpbj0i77u/IiBpZD0iVzVNME1wQ2VoaUh6cmVTek5UY3prYzlkIj8+IDx4OnhtcG1ldGEgeG1sbnM6eD0iYWRvYmU6bnM6bWV0YS8iIHg6eG1wdGs9IkFkb2JlIFhNUCBDb3JlIDUuMC1jMDYwIDYxLjEzNDc3NywgMjAxMC8wMi8xMi0xNzozMjowMCAgICAgICAgIj4gPHJkZjpSREYgeG1sbnM6cmRmPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5LzAyLzIyLXJkZi1zeW50YXgtbnMjIj4gPHJkZjpEZXNjcmlwdGlvbiByZGY6YWJvdXQ9IiIgeG1sbnM6eG1wPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvIiB4bWxuczp4bXBNTT0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wL21tLyIgeG1sbnM6c3RSZWY9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9zVHlwZS9SZXNvdXJjZVJlZiMiIHhtcDpDcmVhdG9yVG9vbD0iQWRvYmUgUGhvdG9zaG9wIENTNSBNYWNpbnRvc2giIHhtcE1NOkluc3RhbmNlSUQ9InhtcC5paWQ6NkE1NkU4Njk2QjI1MTFFNEFDMjFGQTcyODkzNTc3NkYiIHhtcE1NOkRvY3VtZW50SUQ9InhtcC5kaWQ6NkE1NkU4NkE2QjI1MTFFNEFDMjFGQTcyODkzNTc3NkYiPiA8eG1wTU06RGVyaXZlZEZyb20gc3RSZWY6aW5zdGFuY2VJRD0ieG1wLmlpZDo2QTU2RTg2NzZCMjUxMUU0QUMyMUZBNzI4OTM1Nzc2RiIgc3RSZWY6ZG9jdW1lbnRJRD0ieG1wLmRpZDo2QTU2RTg2ODZCMjUxMUU0QUMyMUZBNzI4OTM1Nzc2RiIvPiA8L3JkZjpEZXNjcmlwdGlvbj4gPC9yZGY6UkRGPiA8L3g6eG1wbWV0YT4gPD94cGFja2V0IGVuZD0iciI/Pmuk6K8AAAGBSURBVHjarFRBSsNQEM3/atNs6qLowixcKELoqjuXoqfQeoF6BMEj9BCC1YIXcCGlV8hGLNZlBKWlCk1JSs13Xvw/nca6UDrwmMzMy8tk/iTCWmwi52Eq53+QeWwg2bXSSNi1WiRibgRWCTahwEQmhJgw1WJGML2BC6wQnEqlsuH7fr3f7zdHo9EdPGLkUdc8mX8TJNYIpUajsR+G4YMie3pNVKebpB6GPOrgab7kr5F24Hne9ng87r6HStUuP5V1Mc2AGHnUwWMdCck6sVut1onjOHtnt4nV7M0fAuI65VEnXk3PTFq5Eyi4rnvUe1PW9fO3QOdUzvkbyqNOvEM2dMEHK2zbLr98zJ5+cJWkAvDGUC8Wi2X28Gww6bnHcTzYWp+JGAHTCQz1KIoGfFckCyZBELR3N4V1vCOyTrhHHnXw9N5kQn8+nWq1Onc6C/cERLMn7cfZniD/257wbjDxEjqiDT0fDof3tLE+PGK9HyXNy7pYyrez9K/43/+TLwEGAMb7AY6w980DAAAAAElFTkSuQmCC' :
			IMAGE_PATH + '/handle-fixed.png', 17, 17);
		var terminalHandle = new mxImage((mxClient.IS_SVG) ? 'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABEAAAARCAYAAAA7bUf6AAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAAyJpVFh0WE1MOmNvbS5hZG9iZS54bXAAAAAAADw/eHBhY2tldCBiZWdpbj0i77u/IiBpZD0iVzVNME1wQ2VoaUh6cmVTek5UY3prYzlkIj8+IDx4OnhtcG1ldGEgeG1sbnM6eD0iYWRvYmU6bnM6bWV0YS8iIHg6eG1wdGs9IkFkb2JlIFhNUCBDb3JlIDUuMC1jMDYwIDYxLjEzNDc3NywgMjAxMC8wMi8xMi0xNzozMjowMCAgICAgICAgIj4gPHJkZjpSREYgeG1sbnM6cmRmPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5LzAyLzIyLXJkZi1zeW50YXgtbnMjIj4gPHJkZjpEZXNjcmlwdGlvbiByZGY6YWJvdXQ9IiIgeG1sbnM6eG1wPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvIiB4bWxuczp4bXBNTT0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wL21tLyIgeG1sbnM6c3RSZWY9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9zVHlwZS9SZXNvdXJjZVJlZiMiIHhtcDpDcmVhdG9yVG9vbD0iQWRvYmUgUGhvdG9zaG9wIENTNSBNYWNpbnRvc2giIHhtcE1NOkluc3RhbmNlSUQ9InhtcC5paWQ6MEMzRUVERTk2NzU1MTFFNTg5NjNEMjREQ0FFNENFQzgiIHhtcE1NOkRvY3VtZW50SUQ9InhtcC5kaWQ6MEMzRUVERUE2NzU1MTFFNTg5NjNEMjREQ0FFNENFQzgiPiA8eG1wTU06RGVyaXZlZEZyb20gc3RSZWY6aW5zdGFuY2VJRD0ieG1wLmlpZDowQzNFRURFNzY3NTUxMUU1ODk2M0QyNERDQUU0Q0VDOCIgc3RSZWY6ZG9jdW1lbnRJRD0ieG1wLmRpZDowQzNFRURFODY3NTUxMUU1ODk2M0QyNERDQUU0Q0VDOCIvPiA8L3JkZjpEZXNjcmlwdGlvbj4gPC9yZGY6UkRGPiA8L3g6eG1wbWV0YT4gPD94cGFja2V0IGVuZD0iciI/Poj8AGUAAAF6SURBVHjarFTBSsNAEM2u2jSXeCh6sAcPilB6ys2j6Fdo/YH6CYKf0I8QrBb8AQ9S+gu5iMV6jKC0VCEJTalZ54VNnMR4ULrwmJ2Zt5PZmdkIo3yJgsRSBfmDzPUUku2VRsz2qixIehBYJZiECgsyJ0SEhQ6WBkwO8AArBKvZbG64rtsej8dd3/fvIKHDDr/myeJNYFgj2J1OZz8IggdF6+k1VoNhnEgs2OEHT/Mlv0aSQaPR2A7DcPgeKNW6/FTGxSIDdNjhB49lJCTLxOz1eieWZe2d3cZGd5RvAvQ22eEnXkvXTBqFDlTq9frR6E0Z18+qtO83ZIefeIes6IIXVpimWXv5yB8cnMqcDn+1Wq2xj2eFSfoeRdFkaz0f5OAqzunwz2azCZ8VyZS553n93U1hHO+I0uvADj94em6yQH/ujuM4ue6UzgmI6Zz0H7/nBPbf5oRng4rbyIgm9Hw6nd7TxLqQ0PV82JqXZbGUt7P0V/zv/8mXAAMASSz1f9Cd7ycAAAAASUVORK5CYII=' :
			IMAGE_PATH + '/handle-terminal.png', 17, 17);
		var secondaryHandle = new mxImage((mxClient.IS_SVG) ? 'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABEAAAARCAYAAAA7bUf6AAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAAyJpVFh0WE1MOmNvbS5hZG9iZS54bXAAAAAAADw/eHBhY2tldCBiZWdpbj0i77u/IiBpZD0iVzVNME1wQ2VoaUh6cmVTek5UY3prYzlkIj8+IDx4OnhtcG1ldGEgeG1sbnM6eD0iYWRvYmU6bnM6bWV0YS8iIHg6eG1wdGs9IkFkb2JlIFhNUCBDb3JlIDUuMC1jMDYwIDYxLjEzNDc3NywgMjAxMC8wMi8xMi0xNzozMjowMCAgICAgICAgIj4gPHJkZjpSREYgeG1sbnM6cmRmPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5LzAyLzIyLXJkZi1zeW50YXgtbnMjIj4gPHJkZjpEZXNjcmlwdGlvbiByZGY6YWJvdXQ9IiIgeG1sbnM6eG1wPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvIiB4bWxuczp4bXBNTT0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wL21tLyIgeG1sbnM6c3RSZWY9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9zVHlwZS9SZXNvdXJjZVJlZiMiIHhtcDpDcmVhdG9yVG9vbD0iQWRvYmUgUGhvdG9zaG9wIENTNSBNYWNpbnRvc2giIHhtcE1NOkluc3RhbmNlSUQ9InhtcC5paWQ6MEJBMUVERjNEMkZDMTFFM0I0Qzc5RkE1RTc2NjI0OUIiIHhtcE1NOkRvY3VtZW50SUQ9InhtcC5kaWQ6MEJBMUVERjREMkZDMTFFM0I0Qzc5RkE1RTc2NjI0OUIiPiA8eG1wTU06RGVyaXZlZEZyb20gc3RSZWY6aW5zdGFuY2VJRD0ieG1wLmlpZDowQkExRURGMUQyRkMxMUUzQjRDNzlGQTVFNzY2MjQ5QiIgc3RSZWY6ZG9jdW1lbnRJRD0ieG1wLmRpZDowQkExRURGMkQyRkMxMUUzQjRDNzlGQTVFNzY2MjQ5QiIvPiA8L3JkZjpEZXNjcmlwdGlvbj4gPC9yZGY6UkRGPiA8L3g6eG1wbWV0YT4gPD94cGFja2V0IGVuZD0iciI/PvXDOj4AAAFqSURBVHjarFTNToNAEN5FLeiBmDRe7MGLF4IXbp71KapP4CPoO/QdvKiv4ME0PkAvJI2J0SueIHgAAk3b7XxkwSlgE38mmSwz8+3HsPMtUnSbbKww1VhbYB5XbrBnpX3JnlUXSbURvk1ukvcYyYy8IJ9rsoqw3MAJtsh3Xdc98H3/KgzDuyRJHrEiRh51jTOaX4LEDrk9Go1O0zR9UWTL9E0to+dyhSGPOnAab/DPKDtwHOcoy7LXz1SpxeRSzW9F7YiRRx041pGsSMC6Ty1f442LycUawRfRsOyIcDfA632ST6A3GAzOVfYu1PS+c+5q+iBQJ9wZO3TJD1aaptkX+YfYaFS3LKvPXl4fTDn3oigiYR1uJqF6nucR14rBglkQBGO5dyzkybBbxpRHHTitm5rox9PxPK81nZZOAKx1Eo5rnSD/nU54NzhxGx1hjHEcP5FifayItT5sjVvTyJ/vzr/f4l//T1YCDAC4VAdLL1OIRAAAAABJRU5ErkJggg==' :
			IMAGE_PATH + '/handle-secondary.png', 17, 17);
		var rotationHandle = new mxImage((mxClient.IS_SVG) ? 'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABMAAAAVCAYAAACkCdXRAAAACXBIWXMAAAsTAAALEwEAmpwYAAAKT2lDQ1BQaG90b3Nob3AgSUNDIHByb2ZpbGUAAHjanVNnVFPpFj333vRCS4iAlEtvUhUIIFJCi4AUkSYqIQkQSoghodkVUcERRUUEG8igiAOOjoCMFVEsDIoK2AfkIaKOg6OIisr74Xuja9a89+bN/rXXPues852zzwfACAyWSDNRNYAMqUIeEeCDx8TG4eQuQIEKJHAAEAizZCFz/SMBAPh+PDwrIsAHvgABeNMLCADATZvAMByH/w/qQplcAYCEAcB0kThLCIAUAEB6jkKmAEBGAYCdmCZTAKAEAGDLY2LjAFAtAGAnf+bTAICd+Jl7AQBblCEVAaCRACATZYhEAGg7AKzPVopFAFgwABRmS8Q5ANgtADBJV2ZIALC3AMDOEAuyAAgMADBRiIUpAAR7AGDIIyN4AISZABRG8lc88SuuEOcqAAB4mbI8uSQ5RYFbCC1xB1dXLh4ozkkXKxQ2YQJhmkAuwnmZGTKBNA/g88wAAKCRFRHgg/P9eM4Ors7ONo62Dl8t6r8G/yJiYuP+5c+rcEAAAOF0ftH+LC+zGoA7BoBt/qIl7gRoXgugdfeLZrIPQLUAoOnaV/Nw+H48PEWhkLnZ2eXk5NhKxEJbYcpXff5nwl/AV/1s+X48/Pf14L7iJIEyXYFHBPjgwsz0TKUcz5IJhGLc5o9H/LcL//wd0yLESWK5WCoU41EScY5EmozzMqUiiUKSKcUl0v9k4t8s+wM+3zUAsGo+AXuRLahdYwP2SycQWHTA4vcAAPK7b8HUKAgDgGiD4c93/+8//UegJQCAZkmScQAAXkQkLlTKsz/HCAAARKCBKrBBG/TBGCzABhzBBdzBC/xgNoRCJMTCQhBCCmSAHHJgKayCQiiGzbAdKmAv1EAdNMBRaIaTcA4uwlW4Dj1wD/phCJ7BKLyBCQRByAgTYSHaiAFiilgjjggXmYX4IcFIBBKLJCDJiBRRIkuRNUgxUopUIFVIHfI9cgI5h1xGupE7yAAygvyGvEcxlIGyUT3UDLVDuag3GoRGogvQZHQxmo8WoJvQcrQaPYw2oefQq2gP2o8+Q8cwwOgYBzPEbDAuxsNCsTgsCZNjy7EirAyrxhqwVqwDu4n1Y8+xdwQSgUXACTYEd0IgYR5BSFhMWE7YSKggHCQ0EdoJNwkDhFHCJyKTqEu0JroR+cQYYjIxh1hILCPWEo8TLxB7iEPENyQSiUMyJ7mQAkmxpFTSEtJG0m5SI+ksqZs0SBojk8naZGuyBzmULCAryIXkneTD5DPkG+Qh8lsKnWJAcaT4U+IoUspqShnlEOU05QZlmDJBVaOaUt2ooVQRNY9aQq2htlKvUYeoEzR1mjnNgxZJS6WtopXTGmgXaPdpr+h0uhHdlR5Ol9BX0svpR+iX6AP0dwwNhhWDx4hnKBmbGAcYZxl3GK+YTKYZ04sZx1QwNzHrmOeZD5lvVVgqtip8FZHKCpVKlSaVGyovVKmqpqreqgtV81XLVI+pXlN9rkZVM1PjqQnUlqtVqp1Q61MbU2epO6iHqmeob1Q/pH5Z/YkGWcNMw09DpFGgsV/jvMYgC2MZs3gsIWsNq4Z1gTXEJrHN2Xx2KruY/R27iz2qqaE5QzNKM1ezUvOUZj8H45hx+Jx0TgnnKKeX836K3hTvKeIpG6Y0TLkxZVxrqpaXllirSKtRq0frvTau7aedpr1Fu1n7gQ5Bx0onXCdHZ4/OBZ3nU9lT3acKpxZNPTr1ri6qa6UbobtEd79up+6Ynr5egJ5Mb6feeb3n+hx9L/1U/W36p/VHDFgGswwkBtsMzhg8xTVxbzwdL8fb8VFDXcNAQ6VhlWGX4YSRudE8o9VGjUYPjGnGXOMk423GbcajJgYmISZLTepN7ppSTbmmKaY7TDtMx83MzaLN1pk1mz0x1zLnm+eb15vft2BaeFostqi2uGVJsuRaplnutrxuhVo5WaVYVVpds0atna0l1rutu6cRp7lOk06rntZnw7Dxtsm2qbcZsOXYBtuutm22fWFnYhdnt8Wuw+6TvZN9un2N/T0HDYfZDqsdWh1+c7RyFDpWOt6azpzuP33F9JbpL2dYzxDP2DPjthPLKcRpnVOb00dnF2e5c4PziIuJS4LLLpc+Lpsbxt3IveRKdPVxXeF60vWdm7Obwu2o26/uNu5p7ofcn8w0nymeWTNz0MPIQ+BR5dE/C5+VMGvfrH5PQ0+BZ7XnIy9jL5FXrdewt6V3qvdh7xc+9j5yn+M+4zw33jLeWV/MN8C3yLfLT8Nvnl+F30N/I/9k/3r/0QCngCUBZwOJgUGBWwL7+Hp8Ib+OPzrbZfay2e1BjKC5QRVBj4KtguXBrSFoyOyQrSH355jOkc5pDoVQfujW0Adh5mGLw34MJ4WHhVeGP45wiFga0TGXNXfR3ENz30T6RJZE3ptnMU85ry1KNSo+qi5qPNo3ujS6P8YuZlnM1VidWElsSxw5LiquNm5svt/87fOH4p3iC+N7F5gvyF1weaHOwvSFpxapLhIsOpZATIhOOJTwQRAqqBaMJfITdyWOCnnCHcJnIi/RNtGI2ENcKh5O8kgqTXqS7JG8NXkkxTOlLOW5hCepkLxMDUzdmzqeFpp2IG0yPTq9MYOSkZBxQqohTZO2Z+pn5mZ2y6xlhbL+xW6Lty8elQfJa7OQrAVZLQq2QqboVFoo1yoHsmdlV2a/zYnKOZarnivN7cyzytuQN5zvn//tEsIS4ZK2pYZLVy0dWOa9rGo5sjxxedsK4xUFK4ZWBqw8uIq2Km3VT6vtV5eufr0mek1rgV7ByoLBtQFr6wtVCuWFfevc1+1dT1gvWd+1YfqGnRs+FYmKrhTbF5cVf9go3HjlG4dvyr+Z3JS0qavEuWTPZtJm6ebeLZ5bDpaql+aXDm4N2dq0Dd9WtO319kXbL5fNKNu7g7ZDuaO/PLi8ZafJzs07P1SkVPRU+lQ27tLdtWHX+G7R7ht7vPY07NXbW7z3/T7JvttVAVVN1WbVZftJ+7P3P66Jqun4lvttXa1ObXHtxwPSA/0HIw6217nU1R3SPVRSj9Yr60cOxx++/p3vdy0NNg1VjZzG4iNwRHnk6fcJ3/ceDTradox7rOEH0x92HWcdL2pCmvKaRptTmvtbYlu6T8w+0dbq3nr8R9sfD5w0PFl5SvNUyWna6YLTk2fyz4ydlZ19fi753GDborZ752PO32oPb++6EHTh0kX/i+c7vDvOXPK4dPKy2+UTV7hXmq86X23qdOo8/pPTT8e7nLuarrlca7nuer21e2b36RueN87d9L158Rb/1tWeOT3dvfN6b/fF9/XfFt1+cif9zsu72Xcn7q28T7xf9EDtQdlD3YfVP1v+3Njv3H9qwHeg89HcR/cGhYPP/pH1jw9DBY+Zj8uGDYbrnjg+OTniP3L96fynQ89kzyaeF/6i/suuFxYvfvjV69fO0ZjRoZfyl5O/bXyl/erA6xmv28bCxh6+yXgzMV70VvvtwXfcdx3vo98PT+R8IH8o/2j5sfVT0Kf7kxmTk/8EA5jz/GMzLdsAAAAgY0hSTQAAeiUAAICDAAD5/wAAgOkAAHUwAADqYAAAOpgAABdvkl/FRgAAA6ZJREFUeNqM001IY1cUB/D/fYmm2sbR2lC1zYlgoRG6MpEyBlpxM9iFIGKFIm3s0lCKjOByhCLZCFqLBF1YFVJdSRbdFHRhBbULtRuFVBTzYRpJgo2mY5OX5N9Fo2TG+eiFA/dd3vvd8+65ByTxshARTdf1JySp6/oTEdFe9T5eg5lIcnBwkCSZyWS+exX40oyur68/KxaLf5Okw+H4X+A9JBaLfUySZ2dnnJqaosPhIAACeC34DJRKpb7IZrMcHx+nwWCgUopGo/EOKwf9fn/1CzERUevr6+9ls1mOjIwQAH0+H4PBIKPR6D2ofAQCgToRUeVYJUkuLy8TANfW1kiS8/PzCy84Mw4MDBAAZ2dnmc/nub+/X0MSEBF1cHDwMJVKsaGhgV6vl+l0mqOjo1+KyKfl1dze3l4NBoM/PZ+diFSLiIKIGBOJxA9bW1sEwNXVVSaTyQMRaRaRxrOzs+9J8ujoaE5EPhQRq67rcZ/PRwD0+/3Udf03EdEgIqZisZibnJykwWDg4eEhd3Z2xkXELCJvPpdBrYjUiEhL+Xo4HH4sIhUaAKNSqiIcDsNkMqG+vh6RSOQQQM7tdhsAQCkFAHC73UUATxcWFqypVApmsxnDw8OwWq2TADQNgAYAFosF+XweyWQSdru9BUBxcXFRB/4rEgDcPouIIx6P4+bmBi0tLSCpAzBqAIqnp6c/dnZ2IpfLYXNzE62traMADACKNputpr+/v8lms9UAKAAwiMjXe3t7KBQKqKurQy6Xi6K0i2l6evpROp1mbW0t29vbGY/Hb8/IVIqq2zlJXl1dsaOjg2azmefn5wwEAl+JSBVExCgi75PkzMwMlVJsbGxkIpFgPp8PX15ePopEIs3JZPITXdf/iEajbGpqolKKExMT1HWdHo/nIxGpgIgoEXnQ3d39kCTHxsYIgC6Xi3NzcwyHw8xkMozFYlxaWmJbWxuVUuzt7WUul6PX6/1cRN4WEe2uA0SkaWVl5XGpRVhdXU0A1DSNlZWVdz3qdDrZ09PDWCzG4+Pjn0XEWvp9KJKw2WwKwBsA3gHQHAqFfr24uMDGxgZ2d3cRiUQAAHa7HU6nE319fTg5Ofmlq6vrGwB/AngaCoWK6rbsNptNA1AJoA7Aux6Pp3NoaMhjsVg+QNmIRqO/u1yubwFEASRKUAEA7rASqABUAKgC8KAUb5XWCOAfAFcA/gJwDSB7C93DylCtdM8qABhLc5TumV6KQigUeubjfwcAHkQJ94ndWeYAAAAASUVORK5CYII=' :
			IMAGE_PATH + '/handle-rotate.png', 19, 21);

		mxVertexHandler.prototype.handleImage = mainHandle;
		mxVertexHandler.prototype.secondaryHandleImage = secondaryHandle;
		mxEdgeHandler.prototype.handleImage = mainHandle;
		mxEdgeHandler.prototype.terminalHandleImage = terminalHandle;
		mxEdgeHandler.prototype.fixedHandleImage = fixedHandle;
		mxEdgeHandler.prototype.labelHandleImage = secondaryHandle;
		mxOutline.prototype.sizerImage = mainHandle;
		Sidebar.prototype.triangleUp = HoverIcons.prototype.triangleUp;
		Sidebar.prototype.triangleRight = HoverIcons.prototype.triangleRight;
		Sidebar.prototype.triangleDown = HoverIcons.prototype.triangleDown;
		Sidebar.prototype.triangleLeft = HoverIcons.prototype.triangleLeft;
		Sidebar.prototype.refreshTarget = HoverIcons.prototype.refreshTarget;
		Sidebar.prototype.roundDrop = HoverIcons.prototype.roundDrop;

		// Pre-fetches images (only needed for non data-uris)
		if (!mxClient.IS_SVG)
		{
			new Image().src = mainHandle.src;
			new Image().src = fixedHandle.src;
			new Image().src = terminalHandle.src;
			new Image().src = secondaryHandle.src;
			new Image().src = rotationHandle.src;
			
			new Image().src = HoverIcons.prototype.triangleUp.src;
			new Image().src = HoverIcons.prototype.triangleRight.src;
			new Image().src = HoverIcons.prototype.triangleDown.src;
			new Image().src = HoverIcons.prototype.triangleLeft.src;
			new Image().src = HoverIcons.prototype.refreshTarget.src;
			new Image().src = HoverIcons.prototype.roundDrop.src;
		}
		
		// Adds rotation handle and live preview
		mxVertexHandler.prototype.rotationEnabled = true;
		mxVertexHandler.prototype.manageSizers = true;
		mxVertexHandler.prototype.livePreview = true;
	
		// Increases default rubberband opacity (default is 20)
		mxRubberband.prototype.defaultOpacity = 30;
		
		// Enables connections along the outline, virtual waypoints, parent highlight etc
		mxConnectionHandler.prototype.outlineConnect = true;
		mxCellHighlight.prototype.keepOnTop = true;
		mxVertexHandler.prototype.parentHighlightEnabled = true;
		mxVertexHandler.prototype.rotationHandleVSpacing = -20;
		
		mxEdgeHandler.prototype.parentHighlightEnabled = true;
		mxEdgeHandler.prototype.dblClickRemoveEnabled = true;
		mxEdgeHandler.prototype.straightRemoveEnabled = true;
		mxEdgeHandler.prototype.virtualBendsEnabled = true;
		mxEdgeHandler.prototype.mergeRemoveEnabled = true;
		mxEdgeHandler.prototype.manageLabelHandle = true;
		mxEdgeHandler.prototype.outlineConnect = true;
		
		// Disables adding waypoints if shift is pressed
		mxEdgeHandler.prototype.isAddVirtualBendEvent = function(me)
		{
			return !mxEvent.isShiftDown(me.getEvent());
		};
	
		// Disables custom handles if shift is pressed
		mxEdgeHandler.prototype.isCustomHandleEvent = function(me)
		{
			return !mxEvent.isShiftDown(me.getEvent());
		};
		
		/**
		 * Implements touch style
		 */
		if (touchStyle)
		{
			// Larger tolerance for real touch devices
			if (mxClient.IS_TOUCH || navigator.maxTouchPoints > 0 || navigator.msMaxTouchPoints > 0)
			{
				mxShape.prototype.svgStrokeTolerance = 18;
				mxVertexHandler.prototype.tolerance = 12;
				mxEdgeHandler.prototype.tolerance = 12;
				Graph.prototype.tolerance = 12;
				
				mxVertexHandler.prototype.rotationHandleVSpacing = -24;
				
				// Implements a smaller tolerance for mouse events and a larger tolerance for touch
				// events on touch devices. The default tolerance (4px) is used for mouse events.
				mxConstraintHandler.prototype.getTolerance = function(me)
				{
					return (mxEvent.isMouseEvent(me.getEvent())) ? 4 : this.graph.getTolerance();
				};
			}
				
			// One finger pans (no rubberband selection) must start regardless of mouse button
			mxPanningHandler.prototype.isPanningTrigger = function(me)
			{
				var evt = me.getEvent();
				
			 	return (me.getState() == null && !mxEvent.isMouseEvent(evt)) ||
			 		(mxEvent.isPopupTrigger(evt) && (me.getState() == null ||
			 		mxEvent.isControlDown(evt) || mxEvent.isShiftDown(evt)));
			};
			
			// Don't clear selection if multiple cells selected
			var graphHandlerMouseDown = mxGraphHandler.prototype.mouseDown;
			mxGraphHandler.prototype.mouseDown = function(sender, me)
			{
				graphHandlerMouseDown.apply(this, arguments);
	
				if (mxEvent.isTouchEvent(me.getEvent()) && this.graph.isCellSelected(me.getCell()) &&
					this.graph.getSelectionCount() > 1)
				{
					this.delayedSelection = false;
				}
			};
		}
		
	    // Timer-based activation of outline connect in connection handler
	    var startTime = new Date().getTime();
	    var timeOnTarget = 0;
	    
		var mxEdgeHandlerUpdatePreviewState = mxEdgeHandler.prototype.updatePreviewState;
		
		mxEdgeHandler.prototype.updatePreviewState = function(edge, point, terminalState, me)
		{
			mxEdgeHandlerUpdatePreviewState.apply(this, arguments);
			
	    	if (terminalState != this.currentTerminalState)
	    	{
	    		startTime = new Date().getTime();
	    		timeOnTarget = 0;
	    	}
	    	else
	    	{
		    	timeOnTarget = new Date().getTime() - startTime;
	    	}
			
			this.currentTerminalState = terminalState;
		};
	
		// Timer-based outline connect
		var mxEdgeHandlerIsOutlineConnectEvent = mxEdgeHandler.prototype.isOutlineConnectEvent;
		
		mxEdgeHandler.prototype.isOutlineConnectEvent = function(me)
		{
			return timeOnTarget > 1500 || ((mxEvent.isAltDown(me.getEvent()) || timeOnTarget > 500) &&
				mxEdgeHandlerIsOutlineConnectEvent.apply(this, arguments));
		};
		
		// Disables custom handles if shift is pressed
		mxVertexHandler.prototype.isCustomHandleEvent = function(me)
		{
			return !mxEvent.isShiftDown(me.getEvent());
		};
	
		// Shows secondary handle for fixed connection points
		mxEdgeHandler.prototype.createHandleShape = function(index, virtual)
		{
			var source = index != null && index == 0;
			var terminalState = this.state.getVisibleTerminalState(source);
			var c = (index != null && (index == 0 || index >= this.state.absolutePoints.length - 1)) ?
				this.graph.getConnectionConstraint(this.state, terminalState, source) : null;
			var pt = (c != null) ? this.graph.getConnectionPoint(this.state.getVisibleTerminalState(source), c) : null;
			var img = (pt != null) ? this.fixedHandleImage : ((c != null && terminalState != null) ?
				this.terminalHandleImage : this.handleImage);
			
			if (img != null)
			{
				var shape = new mxImageShape(new mxRectangle(0, 0, img.width, img.height), img.src);
				
				// Allows HTML rendering of the images
				shape.preserveImageAspect = false;
	
				return shape;
			}
			else
			{
				var s = mxConstants.HANDLE_SIZE;
				
				if (this.preferHtml)
				{
					s -= 1;
				}
				
				return new mxRectangleShape(new mxRectangle(0, 0, s, s), mxConstants.HANDLE_FILLCOLOR, mxConstants.HANDLE_STROKECOLOR);
			}
		};
	
		var vertexHandlerCreateSizerShape = mxVertexHandler.prototype.createSizerShape;
		mxVertexHandler.prototype.createSizerShape = function(bounds, index, fillColor)
		{
			this.handleImage = (index == mxEvent.ROTATION_HANDLE) ? rotationHandle : (index == mxEvent.LABEL_HANDLE) ? this.secondaryHandleImage : this.handleImage;
			return vertexHandlerCreateSizerShape.apply(this, arguments);
		};
		
		// Special case for single edge label handle moving in which case the text bounding box is used
		var mxGraphHandlerGetBoundingBox = mxGraphHandler.prototype.getBoundingBox;
		mxGraphHandler.prototype.getBoundingBox = function(cells)
		{
			if (cells != null && cells.length == 1)
			{
				var model = this.graph.getModel();
				var parent = model.getParent(cells[0]);
				var geo = this.graph.getCellGeometry(cells[0]);
				
				if (model.isEdge(parent) && geo != null && geo.relative)
				{
					var state = this.graph.view.getState(cells[0]);
					
					if (state != null && state.width < 2 && state.height < 2 && state.text != null && state.text.boundingBox != null)
					{
						return mxRectangle.fromRectangle(state.text.boundingBox);
					}
				}
			}
			
			return mxGraphHandlerGetBoundingBox.apply(this, arguments);
		};
		
		// Uses text bounding box for edge labels
		var mxVertexHandlerGetSelectionBounds = mxVertexHandler.prototype.getSelectionBounds;
		mxVertexHandler.prototype.getSelectionBounds = function(state)
		{
			var model = this.graph.getModel();
			var parent = model.getParent(state.cell);
			var geo = this.graph.getCellGeometry(state.cell);
			
			if (model.isEdge(parent) && geo != null && geo.relative && state.width < 2 && state.height < 2 && state.text != null && state.text.boundingBox != null)
			{
				var bbox = state.text.unrotatedBoundingBox || state.text.boundingBox;
				
				return new mxRectangle(Math.round(bbox.x), Math.round(bbox.y), Math.round(bbox.width), Math.round(bbox.height));
			}
			else
			{
				return mxVertexHandlerGetSelectionBounds.apply(this, arguments);
			}
		};
	
		// Redirects moving of edge labels to mxGraphHandler by not starting here.
		// This will use the move preview of mxGraphHandler (see above).
		var mxVertexHandlerMouseDown = mxVertexHandler.prototype.mouseDown;
		mxVertexHandler.prototype.mouseDown = function(sender, me)
		{
			var model = this.graph.getModel();
			var parent = model.getParent(this.state.cell);
			var geo = this.graph.getCellGeometry(this.state.cell);
			
			// Lets rotation events through
			var handle = this.getHandleForEvent(me);
			
			if (handle == mxEvent.ROTATION_HANDLE || !model.isEdge(parent) || geo == null || !geo.relative ||
				this.state == null || this.state.width >= 2 || this.state.height >= 2)
			{
				mxVertexHandlerMouseDown.apply(this, arguments);
			}
		};

		// Shows rotation handle for edge labels.
		mxVertexHandler.prototype.isRotationHandleVisible = function()
		{
			return this.graph.isEnabled() && this.rotationEnabled && this.graph.isCellRotatable(this.state.cell) &&
				(mxGraphHandler.prototype.maxCells <= 0 || this.graph.getSelectionCount() < mxGraphHandler.prototype.maxCells);
		};
	
		// Invokes turn on single click on rotation handle
		mxVertexHandler.prototype.rotateClick = function()
		{
			this.state.view.graph.turnShapes([this.state.cell]);
		};
		
		var vertexHandlerMouseMove = mxVertexHandler.prototype.mouseMove;
		mxVertexHandler.prototype.mouseMove = function()
		{
			vertexHandlerMouseMove.apply(this, arguments);
			
			if (this.graph.graphHandler.first != null)
			{
				if (this.rotationShape != null && this.rotationShape.node != null)
				{
					this.rotationShape.node.style.display = 'none';
				}
			}
		};
		
		var vertexHandlerMouseUp = mxVertexHandler.prototype.mouseUp;
		mxVertexHandler.prototype.mouseUp = function(sender, me)
		{
			vertexHandlerMouseUp.apply(this, arguments);
			
			// Shows rotation handle only if one vertex is selected
			if (this.rotationShape != null && this.rotationShape.node != null)
			{
				this.rotationShape.node.style.display = (this.graph.getSelectionCount() == 1) ? '' : 'none';
			}
		};
	
		var vertexHandlerInit = mxVertexHandler.prototype.init;
		mxVertexHandler.prototype.init = function()
		{
			vertexHandlerInit.apply(this, arguments);
			var redraw = false;
			
			if (this.rotationShape != null)
			{
				this.rotationShape.node.setAttribute('title', mxResources.get('rotateTooltip'));
			}
			
			var update = mxUtils.bind(this, function()
			{
				// Shows rotation handle only if one vertex is selected
				if (this.rotationShape != null && this.rotationShape.node != null)
				{
					this.rotationShape.node.style.display = (this.graph.getSelectionCount() == 1) ? '' : 'none';
				}
				
				if (this.specialHandle != null)
				{
					this.specialHandle.node.style.display = (this.graph.isEnabled() && this.graph.getSelectionCount() < this.graph.graphHandler.maxCells) ? '' : 'none';
				}
				
				this.redrawHandles();
			});
			
			this.selectionHandler = mxUtils.bind(this, function(sender, evt)
			{
				update();
			});
			
			this.graph.getSelectionModel().addListener(mxEvent.CHANGE, this.selectionHandler);
			
			this.changeHandler = mxUtils.bind(this, function(sender, evt)
			{
				this.updateLinkHint(this.graph.getLinkForCell(this.state.cell));
				update();
			});
			
			this.graph.getModel().addListener(mxEvent.CHANGE, this.changeHandler);
			var link = this.graph.getLinkForCell(this.state.cell);
			this.updateLinkHint(link);
			
			if (link != null)
			{
				redraw = true;
			}
			
			if (redraw)
			{
				this.redrawHandles();
			}
		};
	
		mxVertexHandler.prototype.updateLinkHint = function(link)
		{
			if (link == null || this.graph.getSelectionCount() > 1)
			{
				if (this.linkHint != null)
				{
					this.linkHint.parentNode.removeChild(this.linkHint);
					this.linkHint = null;
				}
			}
			else if (link != null)
			{
				if (this.linkHint == null)
				{
					this.linkHint = createHint();
					this.linkHint.style.padding = '4px 10px 6px 10px';
					this.linkHint.style.fontSize = '90%';
					this.linkHint.style.opacity = '1';
					this.linkHint.style.filter = '';
					this.updateLinkHint(link);
					
					this.graph.container.appendChild(this.linkHint);
				}
				
				var label = link;
				var max = 60;
				var head = 36;
				var tail = 20;
				
				if (label.length > max)
				{
					label = label.substring(0, head) + '...' + label.substring(label.length - tail);
				}
				
				var a = document.createElement('a');
				a.setAttribute('href', link);
				a.setAttribute('title', link);
				
				if (this.graph.linkTarget != null)
				{
					a.setAttribute('target', this.graph.linkTarget);
				}
				
				mxUtils.write(a, label);
				
				this.linkHint.innerHTML = '';
				this.linkHint.appendChild(a);
	
				if (this.graph.isEnabled() && typeof this.graph.editLink === 'function')
				{
					var changeLink = document.createElement('img');
					changeLink.setAttribute('src', IMAGE_PATH + '/edit.gif');
					changeLink.setAttribute('title', mxResources.get('editLink'));
					changeLink.setAttribute('width', '11');
					changeLink.setAttribute('height', '11');
					changeLink.style.marginLeft = '10px';
					changeLink.style.marginBottom = '-1px';
					changeLink.style.cursor = 'pointer';
					this.linkHint.appendChild(changeLink);
					
					mxEvent.addListener(changeLink, 'click', mxUtils.bind(this, function(evt)
					{
						this.graph.setSelectionCell(this.state.cell);
						this.graph.editLink();
						mxEvent.consume(evt);
					}));
				}
			}
		};
		
		mxEdgeHandler.prototype.updateLinkHint = mxVertexHandler.prototype.updateLinkHint;
		
		var edgeHandlerInit = mxEdgeHandler.prototype.init;
		mxEdgeHandler.prototype.init = function()
		{
			edgeHandlerInit.apply(this, arguments);
			
			// Disables connection points
			this.constraintHandler.isEnabled = mxUtils.bind(this, function()
			{
				return this.state.view.graph.connectionHandler.isEnabled();
			});
			
			var update = mxUtils.bind(this, function()
			{
				if (this.linkHint != null)
				{
					this.linkHint.style.display = (this.graph.getSelectionCount() == 1) ? '' : 'none';
				}
				
				if (this.labelShape != null)
				{
					this.labelShape.node.style.display = (this.graph.isEnabled() && this.graph.getSelectionCount() < this.graph.graphHandler.maxCells) ? '' : 'none';
				}
			});
	
			this.selectionHandler = mxUtils.bind(this, function(sender, evt)
			{
				update();
			});
			
			this.graph.getSelectionModel().addListener(mxEvent.CHANGE, this.selectionHandler);
			
			this.changeHandler = mxUtils.bind(this, function(sender, evt)
			{
				this.updateLinkHint(this.graph.getLinkForCell(this.state.cell));
				update();
				this.redrawHandles();
			});
			
			this.graph.getModel().addListener(mxEvent.CHANGE, this.changeHandler);
	
			var link = this.graph.getLinkForCell(this.state.cell);
									
			if (link != null)
			{
				this.updateLinkHint(link);
				this.redrawHandles();
			}
		};
	
		// Disables connection points
		var connectionHandlerInit = mxConnectionHandler.prototype.init;
		
		mxConnectionHandler.prototype.init = function()
		{
			connectionHandlerInit.apply(this, arguments);
			
			this.constraintHandler.isEnabled = mxUtils.bind(this, function()
			{
				return this.graph.connectionHandler.isEnabled();
			});
		};
	
		var vertexHandlerRedrawHandles = mxVertexHandler.prototype.redrawHandles;
		mxVertexHandler.prototype.redrawHandles = function()
		{
			vertexHandlerRedrawHandles.apply(this);

			if (this.state != null && this.linkHint != null)
			{
				var c = new mxPoint(this.state.getCenterX(), this.state.getCenterY());
				var tmp = new mxRectangle(this.state.x, this.state.y - 22, this.state.width + 24, this.state.height + 22);
				var bb = mxUtils.getBoundingBox(tmp, this.state.style[mxConstants.STYLE_ROTATION] || '0', c);
				var rs = (bb != null) ? mxUtils.getBoundingBox(this.state, this.state.style[mxConstants.STYLE_ROTATION] || '0') : this.state;
				
				if (bb == null)
				{
					bb = this.state;
				}
				
				this.linkHint.style.left = Math.round(rs.x + (rs.width - this.linkHint.clientWidth) / 2) + 'px';
				this.linkHint.style.top = Math.round(bb.y + bb.height + this.verticalOffset / 2 +
						6 + this.state.view.graph.tolerance) + 'px';
			}
		};

		
		var vertexHandlerReset = mxVertexHandler.prototype.reset;
		mxVertexHandler.prototype.reset = function()
		{
			vertexHandlerReset.apply(this, arguments);
			
			// Shows rotation handle only if one vertex is selected
			if (this.rotationShape != null && this.rotationShape.node != null)
			{
				this.rotationShape.node.style.display = (this.graph.getSelectionCount() == 1) ? '' : 'none';
			}
		};
	
		var vertexHandlerDestroy = mxVertexHandler.prototype.destroy;
		mxVertexHandler.prototype.destroy = function(sender, me)
		{
			vertexHandlerDestroy.apply(this, arguments);
			
			if (this.linkHint != null)
			{
				this.linkHint.parentNode.removeChild(this.linkHint);
				this.linkHint = null;
			}

			if (this.selectionHandler != null)
			{
				this.graph.getSelectionModel().removeListener(this.selectionHandler);
				this.selectionHandler = null;
			}
			
			if  (this.changeHandler != null)
			{
				this.graph.getModel().removeListener(this.changeHandler);
				this.changeHandler = null;
			}
		};
		
		var edgeHandlerRedrawHandles = mxEdgeHandler.prototype.redrawHandles;
		mxEdgeHandler.prototype.redrawHandles = function()
		{
			// Workaround for special case where handler
			// is reset before this which leads to a NPE
			if (this.marker != null)
			{
				edgeHandlerRedrawHandles.apply(this);
		
				if (this.state != null && this.linkHint != null)
				{
					var b = this.state;
					
					if (this.state.text != null && this.state.text.bounds != null)
					{
						b = new mxRectangle(b.x, b.y, b.width, b.height);
						b.add(this.state.text.bounds);
					}
					
					this.linkHint.style.left = Math.round(b.x + (b.width - this.linkHint.clientWidth) / 2) + 'px';
					this.linkHint.style.top = Math.round(b.y + b.height + 6 + this.state.view.graph.tolerance) + 'px';
				}
			}
		};
	
		var edgeHandlerReset = 	mxEdgeHandler.prototype.reset;
		mxEdgeHandler.prototype.reset = function()
		{
			edgeHandlerReset.apply(this, arguments);
			
			if (this.linkHint != null)
			{
				this.linkHint.style.visibility = '';
			}
		};
		
		var edgeHandlerDestroy = 	mxEdgeHandler.prototype.destroy;
		mxEdgeHandler.prototype.destroy = function(sender, me)
		{
			edgeHandlerDestroy.apply(this, arguments);
			
			if (this.linkHint != null)
			{
				this.linkHint.parentNode.removeChild(this.linkHint);
				this.linkHint = null;
			}
	
			if (this.selectionHandler != null)
			{
				this.graph.getSelectionModel().removeListener(this.selectionHandler);
				this.selectionHandler = null;
			}
	
			if  (this.changeHandler != null)
			{
				this.graph.getModel().removeListener(this.changeHandler);
				this.changeHandler = null;
			}
		};
	})();
}
