import "../../../";
import * as angular from "angular";
import "angular-mocks";
import "angular-sanitize";
import { Relationships } from "../../../../main";
import { LocalizationServiceMock } from "../../../../core/localization/localization.mock";
import {BPManageTracesItem, BPManageTracesItemController} from "./bp-manage-traces-item";
import { ComponentTest } from "../../../../util/component.test";
//import { BpArtifactInfo } from "./../../bp-artifact-info";
import { DialogServiceMock } from "../../../../shared/widgets/bp-dialog/bp-dialog";

describe("Component BPManageTracesItem", () => {
    //angular.module("bp.components.artifactinfo", [])
        //.component("bpManageTracesItem", new BPManageTracesItem());

    beforeEach(angular.mock.module("app.main"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("dialogService", DialogServiceMock);
    }));

    let directiveTest: ComponentTest<BPManageTracesItemController>;
    let vm: BPManageTracesItemController;

    beforeEach(inject(() => {
        let template = `<bp-manage-traces-item item="item" delete-trace="$ctrl.deleteTrace(item)"
                      selected-traces="$ctrl.selectedTraces[$ctrl.data.artifactId]"
                      class="clearfix"></bp-manage-traces-item>`;
        directiveTest = new ComponentTest<BPManageTracesItemController>(template, "bp-manage-traces-item");
        vm = directiveTest.createComponent({});

        vm.item = <Relationships.IRelationship>{
            "artifactId": 1,
            "itemId": 1,
            "suspect": false,
            "hasAccess": false,
            "traceDirection": 1,
            "artifactTypePrefix": "DOC",
            "artifactName": "test",
            "itemTypePrefix": "DOC",
            "itemName": "test",
            "itemLabel":"test",
            "projectId": 1,
            "projectName": "1",
            "primitiveItemTypePredefined": 4099,
            "traceType": 2,
            "isSelected": false,
            "readOnly": false
        };
    }));

    afterEach(() => {
        vm = null;
    });

    it("Check if trace was loaded", inject(($compile: ng.ICompileService, $rootScope: ng.IRootScopeService) => {
        expect(directiveTest.element.find(".trace-main").length).toBeGreaterThan(0);
    }));

    it("check toggleFlag if item.hasAccess", () => {
        //Arrange
        vm.item.hasAccess = true;

        //Act
        vm.toggleFlag();

        //Assert
        expect(vm.item.suspect).toBe(true);
    });

    it("check toggleFlag if !item.hasAccess", () => {
        //Arrange
        vm.item.hasAccess = false;

        //Act
        vm.toggleFlag();

        //Assert
        expect(vm.item.suspect).toBe(false);
    });

    it("check setDirection if !item.hasAccess", () => {
        //Arrange
        vm.item.hasAccess = false;

        //Act
        vm.setDirection(0);

        //Assert
        expect(vm.item.traceDirection).toBe(1);
    });

    it("check setDirection if item.hasAccess", () => {
        //Arrange
        vm.item.hasAccess = true;

        //Act
        vm.setDirection(2);

        //Assert
        expect(vm.item.traceDirection).toBe(2);
    });

    it("check if unselected trace will be added to selectedTraces and selected", () => {
        //Arrange
        vm.selectedTraces = [];
        vm.item.isSelected = false;

        //Act
        vm.selectTrace();

        //Assert
        expect(vm.item.isSelected).toBe(true);
        expect(vm.selectedTraces.length).toBe(1);
    });

    it("check if selected trace will be deleted from selectedTraces and unselected", () => {
        //Arrange
        vm.selectedTraces = [vm.item];
        vm.item.isSelected = true;

        //Act
        vm.selectTrace();

        //Assert
        expect(vm.item.isSelected).toBe(false);
        expect(vm.selectedTraces.length).toBe(0);
    });
});