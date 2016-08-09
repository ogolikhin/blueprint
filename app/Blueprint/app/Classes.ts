Classes:

+ directive StoryTellerEditor 
properties:
	public graph: mxGraph;
methods:
	constructor(public container: HTMLElement);
	init();
	loadProcess(); //<- use srvice
	saveProcess(); //<- use srvice
	// controller
	serialize();
	deserialize();
	validate();

+ class Layout //<- singleton
	// rendering area
	var left, top, width, height: number;
	// grid 
	var columnWidth, rowHeight: number
methods:
	getInstance();
	init();
	addNode(connector: Iconnector, node: IProcessNode);
	deleteNode(node: IProcessNode);
	addBranch(node: IProcessNode, target?: IProcessNode);
	deleteBranch(node: IProcessNode);
	changeTarget(node: IProcessNode);
	moveNodes(nodes: IProcessNode[], deltaX: number, deltaY: number);
	updateNodePositions();

export class Renderer {

    private static _instance:Renderer = new Renderer();

    private left: number;
    private top: number;
    private width: number;
    private height: number;
	private columnWidth: number;
	private rowHeight: number;

    constructor() {
        if(Renderer._instance){
            throw new Error("Error: Instantiation failed: Use Renderer.getInstance() instead of new.");
        }
        Renderer._instance = this;
    }

    public static getInstance():Renderer
    {
        return Renderer._instance;
    }

    public init(left: number, 
    			top: number, 
    			width: number, 
    			height: number,
    			columnWidth: number,
    			rowHeight: number) {
	    this.left = left;
	    this.top = top;
	    this.width = width;
	    this.height = height;
	    this.columnWidth = columnWidth;
	    this.rowHeight = rowHeight;
    }
}

+ IProcessNode interface
properties:
	enum Direction {LeftToRight = 0, RightToLeft};
	var cell: mxCell;
	var direction: Direction;
methods:
	init();
	getAttribute(name: string): any;
	setAttribute(name: string, value: any);
	getHeight(): number;
	getWidth(): number;
	getCenter(): mxPoint;
	getId(): string;
	setId(value	: string);
	getName(): string; 
	setName(value: string);
	getSources(): IProcessNode[] - returns array of connected sourceses
	getTargets(): IProcessNode[] - return array of connected targets
	preRender();
	render();
	postRender();

// Nodes:
+ class Task implements IProcessNode
+ class SystemResponce implements IProcessNode
+ class Condition implements IProcessNode
+ class SystemCondition implements IProcessNode
+ class Merger implements IProcessNode

+ interface Iconnector
methods:
	getAttribute(name: string): any;
	setAttribute(name: string, value: any);
	GetSource - returns source node
	GetTarget - returns target node
	GetPoints - returns child points
	getAttribute(name: string): any;
	setAttribute(name: string, value: any);
	getId(): string;
	setId(value	: string);

+ class Connector implements Iconnector

===================================================

+ Interface IBehaviour - for animation???
+ Class BasicBehaviour implements IBehaviour
