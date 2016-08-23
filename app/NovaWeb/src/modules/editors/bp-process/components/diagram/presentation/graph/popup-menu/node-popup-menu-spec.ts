import {IProcess, ProcessModel} from "../../../../../models/processModels";
import {IProcessGraph} from "../models/";
import {NodeType} from "../models/";
import {ProcessGraph} from "../process-graph";
import {ShapesFactory} from "../shapes/shapes-factory";
import {IProcessViewModel, ProcessViewModel} from "../../../viewModel/process-viewmodel";
import {NodePopupMenu} from "./node-popup-menu";

describe("Popup Menu test", () => {
    var graph: IProcessGraph;
    var shapesFactory: ShapesFactory;
    var localScope, rootScope;
    var htmlElement: HTMLElement;
    var processModel: IProcess;
    var viewModel: IProcessViewModel;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        // inject any services that are required
    }));

    beforeEach(inject((_$window_: ng.IWindowService, $rootScope: ng.IRootScopeService) => {
        rootScope = $rootScope;
        shapesFactory = new ShapesFactory(rootScope);

        var wrapper = document.createElement('DIV');
        htmlElement = document.createElement('DIV');
        wrapper.appendChild(htmlElement);
        document.body.appendChild(wrapper);

        $rootScope["config"] = {};
        $rootScope["config"].labels = {};
        
        localScope = {};

        processModel = new ProcessModel();
        viewModel = new ProcessViewModel(processModel);
        viewModel.isReadonly = false;
        viewModel.isSpa = true;

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
        var graph = new ProcessGraph(rootScope, localScope, htmlElement, null, viewModel);

        var popupMenu = new NodePopupMenu(graph, insertTask, insertUserDecision, insertUserDecisionBranch, insertSystemDecision, insertSystemDecisionBranch, null);
        popupMenu.insertionPoint = new mxCell("test", null, null);
        popupMenu.insertionPoint["__proto__"]["edge"] = true;

        var menu = new mxPopupMenu();
        menu["div"] = document.createElement("div");
        menu["div"].className = "mxPopupMenu";
        menu["div"].style.left = "100px";
        menu["div"].style.top = "100px";

        spyOn(menu, "addItem");

        // Act

        popupMenu.createPopupMenu(graph, menu, null, null);

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
        var graph = new ProcessGraph(rootScope, localScope, htmlElement, null, viewModel);

        var popupMenu = new NodePopupMenu(graph, insertTask, insertUserDecision, insertUserDecisionBranch, insertSystemDecision, insertSystemDecisionBranch, null);
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

        popupMenu.createPopupMenu(graph, menu, null, null);

        //Assert

        expect(menu.addItem["calls"].count()).toEqual(1);
        var args = menu.addItem["calls"].argsFor(0);
        expect(args[0]).toContain("Add User Task");


    });

    it("The menu should have the option to 'Add Branch' when edge is false and node type is 'UserDecision' ", () => {
        // Arrange
        let rootScope = {
            config: {
                labels: {
                    ST_Decision_Modal_Add_Condition_Button_Label: "Add Condition"
                }
            }
        };
        // Act
        var graph = new ProcessGraph(rootScope, localScope, htmlElement, null, viewModel);

        var popupMenu = new NodePopupMenu(graph, insertTask, insertUserDecision, insertUserDecisionBranch, insertSystemDecision, insertSystemDecisionBranch, rootScope);
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

        popupMenu.createPopupMenu(graph, menu, null, null);

        //Assert

        expect(menu.addItem["calls"].count()).toEqual(1);
        var args = menu.addItem["calls"].argsFor(0);
        expect(args[0]).toContain("Add Condition");


    });
});
