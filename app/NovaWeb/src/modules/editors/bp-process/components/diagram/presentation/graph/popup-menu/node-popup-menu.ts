import {IDiagramNode} from "../models/";
import {IDiagramLink, IDiagramNodeElement} from "../models/";
import {NodeType, ILayout} from "../models/";
import {ShapesFactory} from "./../shapes/shapes-factory";
import {ILocalizationService} from "../../../../../../../core/localization/localizationService";

export class NodePopupMenu {

    private menu: MxPopupMenu = null;
    private eventSubscriber: Rx.IDisposable = null;
    public insertionPoint: MxCell;

    constructor(private layout: ILayout,
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
        this.mxgraph.popupMenuHandler["isPopupTrigger"] = mxUtils.bind(this, this.isPopupTrigger);
        this.mxgraph.popupMenuHandler["popup"] = this.handlePopup;
        this.mxgraph.popupMenuHandler.factoryMethod = mxUtils.bind(this, this.popupFactoryMethod);
    }

    private handlePopup(x, y, cell, evt) {
        // 'this' is mxPopupMenuHandler

        window["mxPopupMenu"].prototype.popup.apply(this, arguments);

        // ==> calls the factoryMethod and returns here
    }

    private popupFactoryMethod(menu, cell, evt) {
        // 'this' is NodePopupMenu

        this.unsubscribeHidePopupEvents();

        // Do not open menu for non image elements
        if (evt.srcElement && evt.srcElement.nodeName !== "image") {
            return;
        }

        this.menu = menu;
        this.createPopupMenu(this.mxgraph, menu, cell, evt);

        // hide the popup if events are detected elsewhere in the UI
        this.subscribeHidePopupEvents();
    }

    private isPopupTrigger(me) {

        // this handler determines whether to show the popup menu in
        // response to a left mouse button click

        // 'me' param is the mxMouseEvent
        // 'this' is NodePopupMenu

        let isPopupTrigger = false;

        if (mxEvent.isRightMouseButton(me.evt)) {
            isPopupTrigger = false;
        } else if (mxEvent.isLeftMouseButton(me.evt)) {
            if (me.sourceState && me.sourceState.cell &&
                me.evt["InsertNodeIcon"] === true) {

                // if the source of the trigger has been marked as an
                // insertion point in ProcessCellRenderer then show
                // the popup menu
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

    public createPopupMenu(graph, menu, cell, evt) {

        // apply business rules for showing the popup menu options

        if ((<any>this.insertionPoint).edge) {

            if (this.isSourceNodeOfType(this.insertionPoint, NodeType.UserDecision) ||
                this.isDestNodeOfType(this.insertionPoint, NodeType.UserDecision)) {
                menu.addItem(this.localization.get("ST_Popup_Menu_Add_User_Task_Label"), null, () => {
                    if (this.insertTaskFn && this.insertionPoint) {
                        this.insertTaskFn(this.insertionPoint, this.layout, this.shapesFactoryService);
                        this.insertionPoint = null;
                    }
                });
            } else if (this.canAddSystemDecision(this.insertionPoint)) {
                menu.addItem(this.localization.get("ST_Popup_Menu_Add_System_Decision_Label"), null, () => {

                    if (this.insertSystemDecisionFn && this.insertionPoint) {
                        this.insertSystemDecisionFn(this.insertionPoint, this.layout, this.shapesFactoryService);
                        this.insertionPoint = null;
                    }
                });
            } else {
                menu.addItem(this.localization.get("ST_Popup_Menu_Add_User_Task_Label"), null, () => {

                    if (this.insertTaskFn && this.insertionPoint) {
                        this.insertTaskFn(this.insertionPoint, this.layout, this.shapesFactoryService);
                        this.insertionPoint = null;
                    }
                });

                menu.addItem(this.localization.get("ST_Popup_Menu_Add_User_Decision_Label"), null, () => {

                    if (this.insertUserDecisionFn && this.insertionPoint) {
                        this.insertUserDecisionFn(this.insertionPoint, this.layout, this.shapesFactoryService);
                        this.insertionPoint = null;
                    }
                });
            }

        } else if ((<IDiagramNode>this.insertionPoint).getNodeType && (<IDiagramNode>this.insertionPoint).getNodeType() === NodeType.UserDecision) {
            menu.addItem(this.localization.get("ST_Decision_Modal_Add_Condition_Button_Label"), null, () => {
                if (this.insertUserDecisionBranchFn && this.insertionPoint) {
                    this.insertUserDecisionBranchFn((<IDiagramNode>this.insertionPoint).model.id, this.layout, this.shapesFactoryService);
                    this.insertionPoint = null;
                }
            });
        } else if ((<IDiagramNode>this.insertionPoint).getNodeType && (<IDiagramNode>this.insertionPoint).getNodeType() === NodeType.SystemDecision) {
            menu.addItem(this.localization.get("ST_Decision_Modal_Add_Condition_Button_Label"), null, () => {
                if (this.insertSystemDecisionBranchFn && this.insertionPoint) {
                    this.insertSystemDecisionBranchFn((<IDiagramNode>this.insertionPoint).model.id, this.layout, this.shapesFactoryService);
                    this.insertionPoint = null;
                }
            });
        }

        // adjust the offsets of the popup menu to position it above
        // the insertion point
        this.calcMenuOffsets(menu);

    };

    private subscribeHidePopupEvents() {
        // listen for a mousedown, resize or scroll event
        // and hide the popup menu if it is still showing

        const containerScroll$ = Rx.Observable.fromEvent<any>(this.htmlElement, "scroll");
        const mouseDown$ = Rx.Observable.fromEvent<MouseEvent>(document, "mousedown");
        const windowResize$ = Rx.Observable.fromEvent<any>(window, "resize");

        this.eventSubscriber = mouseDown$.merge(windowResize$).merge(containerScroll$).subscribe(event => {
            this.hidePopupMenu();
            this.unsubscribeHidePopupEvents();
        });
    }

    private unsubscribeHidePopupEvents() {
        if (this.eventSubscriber) {
            this.eventSubscriber.dispose();
            this.eventSubscriber = null;
        }
    }

    public hidePopupMenu = () => {
        if (this.menu == null) {
            return;
        }
        this.menu.hideMenu();
        this.menu = null;
    };

    private calcMenuOffsets(menu) {
        /*
         * adjust the x,y offset of the popup menu so that the menu appears
         * above the insertion point
         */
        const regex = new RegExp("[0-9/.]+"); // strip off the 'px'
        let res = regex.exec(menu.div.style.left);
        const x: number = parseInt(res[0], 10);
        res = regex.exec(menu.div.style.top);
        const y: number = parseInt(res[0], 10);

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
            const node = (<IDiagramNodeElement>edge.source).getNode();
            if (node.getNodeType() === NodeType.MergingPoint) {
                const incomingLinks: IDiagramLink[] = node.getIncomingLinks(this.mxgraph.getModel());
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
        let result: boolean = false;

        if (edge && edge.target) {
            const node = (<IDiagramNodeElement>edge.target).getNode();
            result = (node.getNodeType() === nodeType);
        }
        return result;
    }

    public dispose() {
        this.menu = null;
    }
}
