import * as angular from "angular";
import {IProcess, ProcessModel, ProcessClipboardData, IProcessShape} from "../../../../../models/process-models";
import {IProcessViewModel, ProcessViewModel} from "../../../viewmodel/process-viewmodel";
import {NodeType} from "../models/";
import {NodePopupMenu} from "./node-popup-menu";
import {BpMxGraphModel} from "../bp-mxgraph-model";
import {ShapesFactory} from "./../shapes/shapes-factory";
import {ILayout} from "./../models/";
import {IStatefulArtifactFactory} from "../../../../../../../managers/artifact-manager/";
import {StatefulArtifactFactoryMock} from "../../../../../../../managers/artifact-manager/artifact/artifact.factory.mock";
import {ILocalizationService, LocalizationService} from "../../../../../../../commonModule/localization/localization.service";
import {IClipboardService, ClipboardService} from "../../../../../services/clipboard.svc";

describe("Popup Menu", () => {
    let mxgraph: MxGraph;
    let rootScope: ng.IRootScopeService;
    let htmlElement: HTMLElement;
    let processModel: IProcess;
    let viewModel: IProcessViewModel;
    let shapesFactory: ShapesFactory;
    let layout: ILayout;
    let localization: ILocalizationService;
    let clipboard: IClipboardService;
    let popupMenu: NodePopupMenu;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        // inject any services that are required
        $provide.service("localization", LocalizationService);
        $provide.service("clipboardService", ClipboardService);
        $provide.service("statefulArtifactFactory", StatefulArtifactFactoryMock);
    }));

    beforeEach(inject((_$window_: ng.IWindowService,
                       $rootScope: ng.IRootScopeService,
                       _localization_: ILocalizationService,
                       _clipboardService_: IClipboardService,
                       statefulArtifactFactory: IStatefulArtifactFactory) => {

        localization = _localization_;
        clipboard = _clipboardService_;
        rootScope = $rootScope;

        rootScope["config"] = {};
        rootScope["config"].labels = {
            "ST_Popup_Menu_Add_User_Task_Label": "Add User Task",
            "ST_Popup_Menu_Add_System_Decision_Label": "Add Condition",
            "ST_Popup_Menu_Add_User_Decision_Label": "Add Choice",
            "ST_Popup_Menu_Insert_Shapes_Label": "Insert Selected Shapes"
        };

        const wrapper = document.createElement("DIV");
        htmlElement = document.createElement("DIV");
        wrapper.appendChild(htmlElement);
        document.body.appendChild(wrapper);

        processModel = new ProcessModel();
        viewModel = new ProcessViewModel(processModel, null);
        viewModel.isSpa = true;

        mxgraph = new mxGraph(htmlElement, new BpMxGraphModel());
        shapesFactory = new ShapesFactory(rootScope, statefulArtifactFactory);

        popupMenu = new NodePopupMenu(
              layout, shapesFactory, localization, clipboard, htmlElement, mxgraph, null, null, null, null, null, null);
    }));

    it("should show 'Insert' when clipboard has process shapes to insert", () => {

        popupMenu.insertionPoint = new mxCell("test", null, null);
        popupMenu.insertionPoint["__proto__"]["edge"] = true;

        let menu = new mxPopupMenu();
        menu["div"] = document.createElement("div");
        menu["div"].className = "mxPopupMenu";
        menu["div"].style.left = "100px";
        menu["div"].style.top = "100px";

        let clipboardData: IProcess = new ProcessModel();
        let shapes: IProcessShape[] = [];
        let userTaskShape = shapesFactory.createModelUserTaskShape(-1, -1, -1, -1, -1);
        userTaskShape.name = "UT1";
        shapes.push(userTaskShape);
        let systemTaskShape = shapesFactory.createModelSystemTaskShape(-1, -1, -2, -1, -1);
        systemTaskShape.name = "ST1";
        shapes.push(systemTaskShape);
        clipboardData.shapes = shapes;

        clipboard.setData(new ProcessClipboardData(clipboardData));

        spyOn(menu, "addItem");

        popupMenu.createPopupMenu(mxgraph, menu, null, null);

        expect(menu.addItem["calls"].count()).toEqual(3);
        let args = menu.addItem["calls"].argsFor(0);
        expect(args[0]).toContain("Add User Task");
        args = menu.addItem["calls"].argsFor(1);
        expect(args[0]).toContain("Add Choice");
        args = menu.addItem["calls"].argsFor(2);
        expect(args[0]).toContain("Insert");

        clipboard.clearData();
    });

    it("should not show 'Insert' when clipboard has some other kind of data", () => {

        popupMenu.insertionPoint = new mxCell("test", null, null);
        popupMenu.insertionPoint["__proto__"]["edge"] = true;

        let menu = new mxPopupMenu();
        menu["div"] = document.createElement("div");
        menu["div"].className = "mxPopupMenu";
        menu["div"].style.left = "100px";
        menu["div"].style.top = "100px";

        clipboard.setData(new ProcessClipboardData(null));
        clipboard["_data"].type = 999; // not a process data type

        spyOn(menu, "addItem");

        popupMenu.createPopupMenu(mxgraph, menu, null, null);

        expect(menu.addItem["calls"].count()).not.toEqual(3);
        let args = menu.addItem["calls"].argsFor(0);
        expect(args[0]).toContain("Add User Task");
        args = menu.addItem["calls"].argsFor(1);
        expect(args[0]).toContain("Add Choice");

        clipboard.clearData();
    });

    it("should show 'Add User Task' and 'Add Choice' when edge is not connected to a a user decision node ", () => {

        popupMenu.insertionPoint = new mxCell("test", null, null);
        popupMenu.insertionPoint["__proto__"]["edge"] = true;

        let menu = new mxPopupMenu();
        menu["div"] = document.createElement("div");
        menu["div"].className = "mxPopupMenu";
        menu["div"].style.left = "100px";
        menu["div"].style.top = "100px";

        spyOn(menu, "addItem");

        popupMenu.createPopupMenu(mxgraph, menu, null, null);

        // assert
        expect(menu.addItem["calls"].count()).toEqual(2);
        let args = menu.addItem["calls"].argsFor(0);
        expect(args[0]).toContain("Add User Task");
        args = menu.addItem["calls"].argsFor(1);
        expect(args[0]).toContain("Add Choice");

    });

    it("should show 'Add User Task' when edge is connected to a user decision node ", () => {

        popupMenu.insertionPoint = new mxCell("test", null, null);
        popupMenu.insertionPoint["__proto__"]["edge"] = true;

        let menu = new mxPopupMenu();
        menu["div"] = document.createElement("div");
        menu["div"].className = "mxPopupMenu";
        menu["div"].style.left = "100px";
        menu["div"].style.top = "100px";

        spyOn(menu, "addItem");
        spyOn(popupMenu, "isSourceNodeOfType");
        spyOn(popupMenu, "isDestNodeOfType").and.callFake(function () {
            return true;
        });

        popupMenu.createPopupMenu(mxgraph, menu, null, null);

        // assert
        expect(menu.addItem["calls"].count()).toEqual(1);
        let args = menu.addItem["calls"].argsFor(0);
        expect(args[0]).toContain("Add User Task");


    });

    it("should show 'Add Choice' when edge is false and node type is 'UserDecision' ", () => {

        popupMenu.insertionPoint = new mxCell("test", null, null);
        popupMenu.insertionPoint["__proto__"]["edge"] = false;
        popupMenu.insertionPoint["__proto__"]["vertex"] = true;
        popupMenu.insertionPoint["getNodeType"] = () => {
            return;
        };

        let menu = new mxPopupMenu();
        menu["div"] = document.createElement("div");
        menu["div"].className = "mxPopupMenu";
        menu["div"].style.left = "100px";
        menu["div"].style.top = "100px";

        spyOn(menu, "addItem");

        spyOn(popupMenu.insertionPoint, "getNodeType").and.callFake(() => NodeType.UserDecision);

        popupMenu.createPopupMenu(mxgraph, menu, null, null);

        // assert
        expect(menu.addItem["calls"].count()).toEqual(1);
        let args = menu.addItem["calls"].argsFor(0);
        expect(args[0]).toContain("Add Choice");

    });
});
