import "../../../";
import * as angular from "angular";
import "angular-mocks";
import "angular-sanitize";
import {Relationships} from "../../../../main";
import {LocalizationServiceMock} from "../../../../core/localization/localization.mock";
import {BPManageTracesItem, BPManageTracesItemController} from "./bp-manage-traces-item";
import {ComponentTest} from "../../../../util/component.test";
import {DialogServiceMock} from "../../../../shared/widgets/bp-dialog/bp-dialog";

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
                      selected-traces="$ctrl.selectedTraces[$ctrl.data.artifactId]"></bp-manage-traces-item>`;
        directiveTest = new ComponentTest<BPManageTracesItemController>(template, "bp-manage-traces-item");
        vm = directiveTest.createComponent({});

        vm.item = <Relationships.IRelationshipView>{
            "artifactId": 1,
            "itemId": 1,
            "suspect": false,
            "hasAccess": false,
            "traceDirection": 1,
            "artifactTypePrefix": "DOC",
            "artifactName": "test",
            "itemTypePrefix": "DOC",
            "itemName": "test",
            "itemLabel": "test",
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
});
