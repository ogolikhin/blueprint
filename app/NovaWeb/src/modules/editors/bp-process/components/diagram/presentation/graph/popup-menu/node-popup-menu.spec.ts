import * as angular from "angular";
import {IProcess, ProcessModel} from "../../../../../models/process-models";
import {IProcessViewModel, ProcessViewModel} from "../../../viewmodel/process-viewmodel";
import {ILocalizationService, LocalizationService} from "../../../../../../../core/localization/";
import {NodeType} from "../models/";
import {NodePopupMenu} from "./node-popup-menu";
import {BpMxGraphModel} from "../bp-mxgraph-model";
import {ShapesFactory} from "./../shapes/shapes-factory";
import {ILayout} from "./../models/";
import {IStatefulArtifactFactory} from "../../../../../../../managers/artifact-manager/";
import {StatefulArtifactFactoryMock} from "../../../../../../../managers/artifact-manager/artifact/artifact.factory.mock";

describe("Popup Menu", () => {
    let mxgraph: MxGraph;
    let rootScope: ng.IRootScopeService;
    let htmlElement: HTMLElement;
    let processModel: IProcess;
    let viewModel: IProcessViewModel;
    let shapesFactory: ShapesFactory;
    let layout: ILayout;
    let localization: ILocalizationService;
    let popupMenu: NodePopupMenu;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        // inject any services that are required
        $provide.service("localization", LocalizationService);
        $provide.service("statefulArtifactFactory", StatefulArtifactFactoryMock);
    }));

    beforeEach(inject((_$window_: ng.IWindowService,
                       $rootScope: ng.IRootScopeService,
                       _localization_: ILocalizationService,
                       statefulArtifactFactory: IStatefulArtifactFactory) => {

        localization = _localization_;
        rootScope = $rootScope;

        rootScope["config"] = {};
        rootScope["config"].labels = {
            "ST_Decision_Modal_Add_Condition_Button_Label": "Add Condition",
            "ST_Popup_Menu_Add_User_Task_Label": "Add User Task",
            "ST_Popup_Menu_Add_System_Decision_Label": "Add System Decision Point",
            "ST_Popup_Menu_Add_User_Decision_Label": "Add User Decision Point"
        };

        const wrapper = document.createElement("DIV");
        htmlElement = document.createElement("DIV");
        wrapper.appendChild(htmlElement);
        document.body.appendChild(wrapper);

        processModel = new ProcessModel();
        viewModel = new ProcessViewModel(processModel, null);
        viewModel.isReadonly = false;
        viewModel.isSpa = true;

        mxgraph = new mxGraph(htmlElement, new BpMxGraphModel());
        shapesFactory = new ShapesFactory(rootScope, statefulArtifactFactory);

        popupMenu = new NodePopupMenu(
              layout, shapesFactory, localization, htmlElement, mxgraph, null, null, null, null, null);
    }));
     
    it("should have options to 'Add User Task' and 'Add Decision Point' when edge is not connected to a a user decision node ", () => {
        
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
        expect(args[0]).toContain("Add User Decision Point");

    });

    it("should have the option to 'Add User Task' when edge is connected to a user decision node ", () => {
 
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

    it("should have the option to 'Add Branch' when edge is false and node type is 'UserDecision' ", () => {
 
        popupMenu.insertionPoint = new mxCell("test", null, null);
        popupMenu.insertionPoint["__proto__"]["edge"] = false;
        popupMenu.insertionPoint["__proto__"]["vertex"] = true;
        popupMenu.insertionPoint["getNodeType"] = () => {
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
        expect(args[0]).toContain("Add Condition");

    });
});
