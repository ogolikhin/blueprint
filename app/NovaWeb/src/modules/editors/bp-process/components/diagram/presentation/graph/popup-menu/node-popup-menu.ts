import {ILocalizationService} from "../../../../../../../core/";
import {IDiagramNode} from "../models/";
import {IDiagramLink, IDiagramNodeElement} from "../models/";
import {NodeType, ILayout} from "../models/";
import {ShapesFactory} from "./../shapes/shapes-factory";

export class NodePopupMenu {

    private menu: MxPopupMenu = null;
    private eventSubscriber: Rx.IDisposable = null;

    public insertionPoint: MxCell = null;

    constructor(
        private layout: ILayout,
        private shapesFactoryService: ShapesFactory,
        private localization: ILocalizationService,
        private htmlElement: HTMLElement,
        private mxgraph: MxGraph, 
        private insertTaskFn,
        private insertUserDecisionFn,
        private insertUserDecisionBranchFn,
        private insertSystemDecisionFn,
        private insertSystemDecisionBranchFn) {

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
                menu.addItem(this.localization.get("ST_Popup_Menu_Add_User_Task_Label"), null, () => {
                    this.disposeEventSubscriptions();
                    if (this.insertTaskFn && this.insertionPoint) {
                        this.insertTaskFn(this.insertionPoint, this.layout, this.shapesFactoryService);
                        this.insertionPoint = null;
                    }
                });
            } else if (this.canAddSystemDecision(this.insertionPoint)) {

                menu.addItem(this.localization.get("ST_Popup_Menu_Add_System_Decision_Label"), null, () => {
                    this.disposeEventSubscriptions();
                    if (this.insertSystemDecisionFn && this.insertionPoint) {
                        this.insertSystemDecisionFn(this.insertionPoint, this.layout, this.shapesFactoryService);
                        this.insertionPoint = null;
                    }
                });
            } else {
                menu.addItem(this.localization.get("ST_Popup_Menu_Add_User_Task_Label"), null, () => {
                    this.disposeEventSubscriptions();
                    if (this.insertTaskFn && this.insertionPoint) {
                        this.insertTaskFn(this.insertionPoint, this.layout, this.shapesFactoryService);
                        this.insertionPoint = null;
                    }
                });

                menu.addItem(this.localization.get("ST_Popup_Menu_Add_User_Decision_Label"), null, () => {
                    this.disposeEventSubscriptions();
                    if (this.insertUserDecisionFn && this.insertionPoint) {
                        this.insertUserDecisionFn(this.insertionPoint, this.layout, this.shapesFactoryService);
                        this.insertionPoint = null;
                    }
                });
            }
        } else if ((<IDiagramNode>this.insertionPoint).getNodeType && (<IDiagramNode>this.insertionPoint).getNodeType() === NodeType.UserDecision) {
            menu.addItem(this.localization.get("ST_Decision_Modal_Add_Condition_Button_Label"), null, () => {
                this.disposeEventSubscriptions();
                if (this.insertUserDecisionBranchFn && this.insertionPoint) {
                    this.insertUserDecisionBranchFn((<IDiagramNode>this.insertionPoint).model.id, this.layout, this.shapesFactoryService);
                    this.insertionPoint = null;
                }
            });
        } else if ((<IDiagramNode>this.insertionPoint).getNodeType && (<IDiagramNode>this.insertionPoint).getNodeType() === NodeType.SystemDecision) {
            menu.addItem(this.localization.get("ST_Decision_Modal_Add_Condition_Button_Label"), null, () => {
                this.disposeEventSubscriptions();
                if (this.insertSystemDecisionBranchFn && this.insertionPoint) {
                    this.insertSystemDecisionBranchFn((<IDiagramNode>this.insertionPoint).model.id, this.layout, this.shapesFactoryService);
                    this.insertionPoint = null;
                }
            });
        }

        // adjust the offsets of the popup menu to position it above
        // the insertion point
        this.calcMenuOffsets(menu);

        // remove the popup if a mousedown or resize event is detected anywhere in the document 
        // this means that only one popup can be shown at a time
        
        this.removePopupOnEvent();
    };
    
    public hidePopupMenu = () => {
        if (this.menu == null) {
            return;
        }
        this.menu.hideMenu();
        this.menu = null;
    };
    
    private disposeEventSubscriptions() {
        if (this.eventSubscriber) {
            this.eventSubscriber.dispose();
            this.eventSubscriber = null;
        }
    }
  
    private removePopupOnEvent() {
        // listen for a mousedown, resize or scroll event 
        // and remove the popup menu if it is still showing

        var containerScroll$ = Rx.Observable.fromEvent<any>(this.htmlElement, "scroll");
        var mouseDown$ = Rx.Observable.fromEvent<MouseEvent>(document, "mousedown");
        var windowResize$ = Rx.Observable.fromEvent<any>(window, "resize");

        this.eventSubscriber = mouseDown$.merge(windowResize$).merge(containerScroll$).subscribe(event => {
            this.hidePopupMenu();
            this.disposeEventSubscriptions();
        });
    }

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