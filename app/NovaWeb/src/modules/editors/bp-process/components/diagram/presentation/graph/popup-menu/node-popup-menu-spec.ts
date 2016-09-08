import {IProcess, ProcessModel} from "../../../../../models/processModels";
import {IProcessViewModel, ProcessViewModel} from "../../../viewmodel/process-viewmodel";
import {NodeType} from "../models/";
import {NodePopupMenu} from "./node-popup-menu";
import {BpMxGraphModel} from "../bp-mxgraph-model";
import {ShapesFactory} from "./../shapes/shapes-factory";
import {ILayout} from "./../models/";

describe("Popup Menu test", () => {
    let mxgraph: MxGraph;
    let localScope, rootScope;
    let htmlElement: HTMLElement;
    let processModel: IProcess;
    let viewModel: IProcessViewModel;
    let shapesFactory: ShapesFactory;
    let layout: ILayout;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        // inject any services that are required
    }));

    beforeEach(inject((_$window_: ng.IWindowService, $rootScope: ng.IRootScopeService) => {

        rootScope = $rootScope;

        rootScope["config"] = {};
        rootScope["config"].labels = {
            "ST_Decision_Modal_Add_Condition_Button_Label": "Add Condition",
            "ST_Popup_Menu_Add_User_Task_Label": "Add User Task",
            "ST_Popup_Menu_Add_System_Decision_Label": "Add System Decision Point",
            "ST_Popup_Menu_Add_User_Decision_Label": "Add User Decision Point"
        };

        localScope = {};

        var wrapper = document.createElement('DIV');
        htmlElement = document.createElement('DIV');
        wrapper.appendChild(htmlElement);
        document.body.appendChild(wrapper);
  
        processModel = new ProcessModel();
        viewModel = new ProcessViewModel(processModel);
        viewModel.isReadonly = false;
        viewModel.isSpa = true;

        mxgraph = new mxGraph(htmlElement, new BpMxGraphModel());  
        shapesFactory = new ShapesFactory(rootScope);
    }));

    function insertTask(edge: MxCell) {

    };

    function insertUserDecision(edge: MxCell) {

    };

    function insertUserDecisionBranch(edge: MxCell) {

    };

    function insertSystemDecision(edge: MxCell) {

    };

    function insertSystemDecisionBranch(edge: MxCell) {

    };


    it("The menu should have options to 'Add User Task' and 'Add Decision Point' when edge is not connected to a a user decision node ", () => {
        // Arrange

        // Act
     
        var popupMenu = new NodePopupMenu(
            layout,
            shapesFactory,
            rootScope,
            htmlElement,
            mxgraph,
            insertTask,
            insertUserDecision,
            insertUserDecisionBranch,
            insertSystemDecision,
            insertSystemDecisionBranch
        );
        popupMenu.insertionPoint = new mxCell("test", null, null);
        popupMenu.insertionPoint["__proto__"]["edge"] = true;

        var menu = new mxPopupMenu();
        menu["div"] = document.createElement("div");
        menu["div"].className = "mxPopupMenu";
        menu["div"].style.left = "100px";
        menu["div"].style.top = "100px";

        spyOn(menu, "addItem");

        // Act

        popupMenu.createPopupMenu(mxgraph, menu, null, null);

        //Assert

        expect(menu.addItem["calls"].count()).toEqual(2);
        var args = menu.addItem["calls"].argsFor(0);
        expect(args[0]).toContain("Add User Task");
        args = menu.addItem["calls"].argsFor(1);
        expect(args[0]).toContain("Add User Decision Point");

    });

    it("The menu should have the option to 'Add User Task' when edge is connected to a user decision node ", () => {
        // Arrange

        // Act
        var popupMenu = new NodePopupMenu(
            layout,
            shapesFactory,
            rootScope,
            htmlElement,
            mxgraph,
            insertTask,
            insertUserDecision,
            insertUserDecisionBranch,
            insertSystemDecision,
            insertSystemDecisionBranch
        );
        popupMenu.insertionPoint = new mxCell("test", null, null);
        popupMenu.insertionPoint["__proto__"]["edge"] = true;

        var menu = new mxPopupMenu();
        menu["div"] = document.createElement("div");
        menu["div"].className = "mxPopupMenu";
        menu["div"].style.left = "100px";
        menu["div"].style.top = "100px";

        spyOn(menu, "addItem");
        spyOn(popupMenu, "isSourceNodeOfType");
        spyOn(popupMenu, "isDestNodeOfType").and.callFake(function () {
            return true;
        });

        // Act

        popupMenu.createPopupMenu(mxgraph, menu, null, null);

        //Assert

        expect(menu.addItem["calls"].count()).toEqual(1);
        var args = menu.addItem["calls"].argsFor(0);
        expect(args[0]).toContain("Add User Task");


    });

    it("The menu should have the option to 'Add Branch' when edge is false and node type is 'UserDecision' ", () => {
        // Arrange
        
        // Act
      
        var popupMenu = new NodePopupMenu(
            layout,
            shapesFactory,
            rootScope,
            htmlElement,
            mxgraph,
            insertTask,
            insertUserDecision,
            insertUserDecisionBranch,
            insertSystemDecision,
            insertSystemDecisionBranch
        );
        popupMenu.insertionPoint = new mxCell("test", null, null);
        popupMenu.insertionPoint["__proto__"]["edge"] = false;
        popupMenu.insertionPoint["__proto__"]["vertex"] = true;
        popupMenu.insertionPoint["getNodeType"] = () => { };

        var menu = new mxPopupMenu();
        menu["div"] = document.createElement("div");
        menu["div"].className = "mxPopupMenu";
        menu["div"].style.left = "100px";
        menu["div"].style.top = "100px";

        spyOn(menu, "addItem");

        spyOn(popupMenu.insertionPoint, "getNodeType").and.callFake(() => NodeType.UserDecision);

        // Act

        popupMenu.createPopupMenu(mxgraph, menu, null, null);

        //Assert

        expect(menu.addItem["calls"].count()).toEqual(1);
        var args = menu.addItem["calls"].argsFor(0);
        expect(args[0]).toContain("Add Condition");

    });
});
