import {IDiagramNode, INotifyModelChanged} from "../models/";
import {IDiagramLink, IDiagramNodeElement} from "../models/";
import {NodeType, ILayout} from "../models/";
import {ShapesFactory} from "./../shapes/shapes-factory";

export class NodePopupMenu {

    private menu: MxPopupMenu = null;
    private initialScrollPosition: number[] = null;
    private initialPopupPosition: number[] = null;
    private diagramAbsoluteCoordinates: number[] = null;
    private scrollTicking: boolean = false;
    
    public insertionPoint: MxCell = null;

    constructor(
        private layout: ILayout,
        private shapesFactoryService: ShapesFactory,
        private rootScope,
        private htmlElement: HTMLElement,
        private mxgraph: MxGraph, 
        private insertTaskFn,
        private insertUserDecisionFn,
        private insertUserDecisionBranchFn,
        private insertSystemDecisionFn,
        private insertSystemDecisionBranchFn,
        private postDeleteFunction: INotifyModelChanged = null) {

        this.init();
    }

    private init() {
    
        this.installPopupMenuHandlers();

        this.mxgraph.popupMenuHandler["setEnabled"](true);
        this.mxgraph.popupMenuHandler["useLeftButtonForPopup"] = true;
    }

    // hook the mxPopupMenu popup() function 
   
    private installPopupMenuHandlers() {

        this.mxgraph.popupMenuHandler["isPopupTrigger"] = this.isPopupTrigger;
        this.mxgraph.popupMenuHandler["popup"] = function (x, y, cell, evt) {

            return window["mxPopupMenu"].prototype.popup.apply(this, arguments);
        };

        this.mxgraph.popupMenuHandler.factoryMethod = (menu, cell, evt) => {
            // Do not open menu for non image elements
            if (evt.srcElement && evt.srcElement.nodeName !== "image") {
                return;
            }
            this.menu = menu;
            this.createPopupMenu(this.mxgraph, menu, cell, evt);
        };
    }

    private updatePositionOfPopupMenu = (x: number, y: number) => {
        // calculates the movement relative to the last know measurement 
        let compensatedX = this.initialScrollPosition["x"] - x;
        let compensatedY = this.initialScrollPosition["y"] - y;

        if (this.menu && this.menu["div"]) {
            let menuDiv = <HTMLElement>this.menu["div"];
            menuDiv.style.left = (this.initialPopupPosition["x"] + compensatedX) + "px";
            menuDiv.style.top = (this.initialPopupPosition["y"] + compensatedY) + "px";

            // hide the popup and remove the listener if near the edges of the diagram, 
            // as otherwise it will go over other elements (e.g.the toolbar)
            if (
                (compensatedY < 0 && parseInt(menuDiv.style.top, 10) < this.diagramAbsoluteCoordinates["top"]) ||
                (compensatedX > 0 && (parseInt(menuDiv.style.left, 10) + menuDiv.offsetWidth) > this.diagramAbsoluteCoordinates["right"]) ||
                (compensatedX < 0 && parseInt(menuDiv.style.left, 10) < this.diagramAbsoluteCoordinates["left"])
                // no need to check for the bottom edge
            ) {
                this.removeScrollHandler();
            }
        }
    };

    private initScrollingPositionOfPopupMenu = () => {
        // set the properties to be used for calculating the popup position when scrolling
         
        this.initialScrollPosition = [];
        this.initialScrollPosition["x"] = this.htmlElement.scrollLeft;
        this.initialScrollPosition["y"] = this.htmlElement.scrollTop;

        if (this.menu && this.menu["div"]) {
            let menuDiv = <HTMLElement>this.menu["div"];
            this.initialPopupPosition = [];
            this.initialPopupPosition["x"] = parseInt(menuDiv.style.left, 10);
            this.initialPopupPosition["y"] = parseInt(menuDiv.style.top, 10);
        }

        let containerBoundingClientRect = this.htmlElement.getBoundingClientRect();
        this.diagramAbsoluteCoordinates = [];
        // StoryTeller main viewport doesn't have scrollbars, otherwise we may need to compensate the following values
        // ref: https://developer.mozilla.org/en-US/docs/Web/API/Element/getBoundingClientRect
        this.diagramAbsoluteCoordinates["top"] = containerBoundingClientRect.top;
        this.diagramAbsoluteCoordinates["right"] = containerBoundingClientRect.right;
        this.diagramAbsoluteCoordinates["bottom"] = containerBoundingClientRect.bottom;
        this.diagramAbsoluteCoordinates["left"] = containerBoundingClientRect.left;
    };

    private removeScrollHandler = () => {
        // unregister of the scroll handler
        this.htmlElement.removeEventListener("scroll", this.scrollHandler);
        // reset the properties used for calculating the popup position when scrolling
        this.initialScrollPosition = null;
        this.initialPopupPosition = null;
        this.diagramAbsoluteCoordinates = null;
        this.scrollTicking = false;
        // we make sure the popup menu is closed
        this.hidePopupMenu();
    };

    private scrollHandler = (evt) => {
        if (!this.menu["isMenuShowing"]()) { // the popup menu is hidden/destroyed, we self-unregister
            this.removeScrollHandler();
        } else {
            if (!this.scrollTicking) {
                // scroll events can fire at a high rate. We throttle the event using requestAnimationFrame
                // ref: https://developer.mozilla.org/en-US/docs/Web/API/window/requestAnimationFrame
                window.requestAnimationFrame(() => {
                    this.updatePositionOfPopupMenu(evt.target.scrollLeft, evt.target.scrollTop);
                    this.scrollTicking = false;
                });
            }
            this.scrollTicking = true;
        }
    };

    private isPopupTrigger = (me) => {
        
        // this handler determines whether to show the Insert Node popup menu in 
        // response to a left mouse button click
        
        // Note:  me param is the mxMouseEvent 
       
        var isPopupTrigger = false;
        this.insertionPoint = null;

        if (mxEvent.isRightMouseButton(me.evt)) {
            isPopupTrigger = false;
        } else if (mxEvent.isLeftMouseButton(me.evt)) {
            if (me.sourceState && me.sourceState.cell &&
                me.evt["InsertNodeIcon"] === true) {
                
                 // if the source of the trigger is an Add Task/Decision icon then 
                 // show the popup menu
                 
                this.insertionPoint = me.sourceState.cell;
                isPopupTrigger = true;

                // clear the flag on the event object
                me.evt["InsertNodeIcon"] = false;
            } else {
                isPopupTrigger = false;
            }
        }
        return isPopupTrigger;

    };

    public createPopupMenu = (graph, menu, cell, evt) => {
        if ((<any>this.insertionPoint).edge) {
            
            // check if the edge is connected to a a user decision node 
            // if it is then do not show 'Add Decision Point' menu option 
            
            if (this.isSourceNodeOfType(this.insertionPoint, NodeType.UserDecision) ||
                this.isDestNodeOfType(this.insertionPoint, NodeType.UserDecision)) {
                menu.addItem(this.rootScope.config.labels["ST_Popup_Menu_Add_User_Task_Label"], null, () => {
                    if (this.insertTaskFn && this.insertionPoint) {
                        this.insertTaskFn(this.insertionPoint, this.layout, this.shapesFactoryService, this.postDeleteFunction);
                        this.insertionPoint = null;
                    }
                });
            } else if (this.canAddSystemDecision(this.insertionPoint)) {

                menu.addItem(this.rootScope.config.labels["ST_Popup_Menu_Add_System_Decision_Label"], null, () => {
                    if (this.insertSystemDecisionFn && this.insertionPoint) {
                        this.insertSystemDecisionFn(this.insertionPoint, this.layout, this.shapesFactoryService);
                        this.insertionPoint = null;
                    }
                });
            } else {
                menu.addItem(this.rootScope.config.labels["ST_Popup_Menu_Add_User_Task_Label"], null, () => {
                    if (this.insertTaskFn && this.insertionPoint) {
                        this.insertTaskFn(this.insertionPoint, this.layout, this.shapesFactoryService,this.postDeleteFunction);
                        this.insertionPoint = null;
                    }
                });

                menu.addItem(this.rootScope.config.labels["ST_Popup_Menu_Add_User_Decision_Label"], null, () => {
                    if (this.insertUserDecisionFn && this.insertionPoint) {
                        this.insertUserDecisionFn(this.insertionPoint, this.layout, this.shapesFactoryService);
                        this.insertionPoint = null;
                    }
                });
            }
        } else if ((<IDiagramNode>this.insertionPoint).getNodeType && (<IDiagramNode>this.insertionPoint).getNodeType() === NodeType.UserDecision) {
            menu.addItem(this.rootScope.config.labels["ST_Decision_Modal_Add_Condition_Button_Label"], null, () => {
                if (this.insertUserDecisionBranchFn && this.insertionPoint, this.layout, this.shapesFactoryService) {
                    this.insertUserDecisionBranchFn((<IDiagramNode>this.insertionPoint).model.id, this.layout, this.shapesFactoryService);
                    this.insertionPoint = null;
                }
            });
        } else if ((<IDiagramNode>this.insertionPoint).getNodeType && (<IDiagramNode>this.insertionPoint).getNodeType() === NodeType.SystemDecision) {
            menu.addItem(this.rootScope.config.labels["ST_Decision_Modal_Add_Condition_Button_Label"], null, () => {
                if (this.insertSystemDecisionBranchFn && this.insertionPoint) {
                    this.insertSystemDecisionBranchFn((<IDiagramNode>this.insertionPoint).model.id, this.layout, this.shapesFactoryService);
                    this.insertionPoint = null;
                }
            });
        }

        // adjust the offsets of the popup menu to position it above
        // the insertion point
        this.calcMenuOffsets(menu);

        // This is an edge case where the user opens a popup menu by clicking on (+), starts scrolling the diagram,
        // opens a different popup menu while the first one is still visible
        // It is safe to be called even on first run as "Calling removeEventListener() with arguments that do not
        // identify any currently registered EventListener on the EventTarget has no effect."
        // ref: https://developer.mozilla.org/en-US/docs/Web/API/EventTarget/removeEventListener
        this.removeScrollHandler();

        this.initScrollingPositionOfPopupMenu();
        // register the scroll handler
        this.htmlElement.addEventListener("scroll", this.scrollHandler);
    };

    public hidePopupMenu = () => {
        if (this.menu == null) {
            return;
        }

        this.menu.hideMenu();
    };

    private calcMenuOffsets(menu) {
        /*
         * adjust the x,y offset of the popup menu so that the menu appears
         * above the insertion point 
         */
        var regex = new RegExp("[0-9/.]+"); // strip off the 'px'
        var res = regex.exec(menu.div.style.left);
        var x: number = parseInt(res[0], 10);
        res = regex.exec(menu.div.style.top);
        var y: number = parseInt(res[0], 10);

        menu.div.style.left = (x - 62) + "px";
        if (menu.itemCount === 1) {
            menu.div.style.top = (y - 55) + "px";
        } else if (menu.itemCount === 2) {
            menu.div.style.top = (y - 90) + "px";
        }
    }
    private canAddSystemDecision(edge: MxCell): boolean {

        if (this.isSourceNodeOfType(this.insertionPoint, NodeType.UserTask) ||
            this.isDestNodeOfType(this.insertionPoint, NodeType.SystemTask)) {
            return true;
        }
        if (this.isSourceNodeOfType(this.insertionPoint, NodeType.SystemDecision) ||
            this.isDestNodeOfType(this.insertionPoint, NodeType.SystemTask)) {
            return true;
        }
        if (this.isSourceNodeOfType(this.insertionPoint, NodeType.SystemDecision) ||
            this.isDestNodeOfType(this.insertionPoint, NodeType.SystemDecision)) {
            return true;
        }

        return false;
    }
    private isSourceNodeOfType(edge: MxCell, nodeType: NodeType) {
        let result: boolean = false;
   
        if (edge && edge.source) {
            var node = (<IDiagramNodeElement>edge.source).getNode();
            if (node.getNodeType() === NodeType.MergingPoint) {
                let incomingLinks: IDiagramLink[] = node.getIncomingLinks(this.mxgraph.getModel());
                for (let link of incomingLinks) {
                    if (this.isSourceNodeOfType(link, nodeType)) {
                        return true;
                    }
                }
            } else {
                result = (node.getNodeType() === nodeType);
            }
        }

        return result;
    }

    private isDestNodeOfType(edge: MxCell, nodeType: NodeType) {
        var result: boolean = false;

        if (edge && edge.target) {
            var node = (<IDiagramNodeElement>edge.target).getNode();
            result = (node.getNodeType() === nodeType);
        }
        return result;
    }

    public dispose() {
        this.menu = null;
    }
}