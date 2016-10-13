import "../../../";
import * as angular from "angular";
import "angular-mocks";
import "angular-sanitize";
import {Relationships} from "../../../../main";
import {LocalizationServiceMock} from "../../../../core/localization/localization.mock";
import {ManageTracesDialogController} from "./bp-manage-traces";
import {DialogServiceMock, IDialogSettings} from "../../../../shared/widgets/bp-dialog/bp-dialog";
import {DataMock, DialogSettingsMock} from "./bp-manage-traces.mock";
import {ModalServiceInstanceMock, ModalServiceMock} from "../../../../shell/login/mocks.spec";


describe("ManageTracesController", () => {

    beforeEach(angular.mock.module("app.main"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        let dialogSettings: IDialogSettings = {};

        $provide.service("ctrl", ManageTracesDialogController);
        $provide.service("$uibModalInstance", ModalServiceInstanceMock);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("dialogService", DialogServiceMock);
        $provide.service("dialogData", DataMock);
        $provide.service("dialogSettings", DialogSettingsMock);
    }));

    beforeEach(inject(($rootScope: ng.IRootScopeService, ctrl: ManageTracesDialogController) => {

        ctrl.data.artifactId = 15;

        ctrl.data.manualTraces = <Relationships.IRelationship[]>[{
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
            "isSelected": true,
            "readOnly": false
        }];
    }));

    it("should clear selectedTraces array",
        inject(($rootScope: ng.IRootScopeService, ctrl: ManageTracesDialogController) => {

            //Arrange
            ctrl.selectedTraces[ctrl.data.artifactId] = ctrl.data.manualTraces;

            //Act
            ctrl.clearSelected();

            //Assert
            expect(ctrl.selectedTraces[ctrl.data.artifactId].length).toBe(0);
        }));

    it("should delete trace from selectedTraces[] and manualTraces[]",
        inject(($rootScope: ng.IRootScopeService, ctrl: ManageTracesDialogController) => {

            //Arrange
            ctrl.selectedTraces[ctrl.data.artifactId] = ctrl.data.manualTraces;

            //Act
            ctrl.deleteTrace(ctrl.data.manualTraces[0]);
            $rootScope.$digest();

            //Assert
            expect(ctrl.selectedTraces[ctrl.data.artifactId].length).toBe(0);
            expect(ctrl.data.manualTraces.length).toBe(0);
        }));

    it("should set direction for all selected traces",
        inject(($rootScope: ng.IRootScopeService, ctrl: ManageTracesDialogController) => {

            //Arrange
            ctrl.selectedTraces[ctrl.data.artifactId] = ctrl.data.manualTraces;

            //Act
            ctrl.setSelectedDirection(2);

            //Assert
            expect(ctrl.selectedTraces[ctrl.data.artifactId][0].traceDirection).toBe(2);
        }));

    it("should set direction for traces that will be added from artifact picker",
        inject(($rootScope: ng.IRootScopeService, ctrl: ManageTracesDialogController) => {

            //Arrange
            ctrl.direction = 0;

            //Act
            ctrl.setDirection(2);

            //Assert
            expect(ctrl.direction).toBe(2);
        }));

    it("should delete traces from selectedTraces[] and manualTraces[]",
        inject(($rootScope: ng.IRootScopeService, ctrl: ManageTracesDialogController) => {

            //Arrange
            ctrl.selectedTraces[ctrl.data.artifactId] = ctrl.data.manualTraces;

            //Act
            ctrl.deleteTraces();
            $rootScope.$digest();

            //Assert
            expect(ctrl.data.manualTraces.length).toBe(0);
            expect(ctrl.selectedTraces[ctrl.data.artifactId].length).toBe(0);
        }));


    it("should toggle flag for traces from false to true",
        inject(($rootScope: ng.IRootScopeService, ctrl: ManageTracesDialogController) => {

            //Arrange
            ctrl.selectedTraces[ctrl.data.artifactId] = ctrl.data.manualTraces;
            ctrl.selectedTraces[ctrl.data.artifactId][0].hasAccess = true;

            //Act
            ctrl.toggleTraces();

            //Assert
            expect(ctrl.selectedTraces[ctrl.data.artifactId][0].suspect).toBe(true);
        }));

    it("should toggle flag for traces from true to false",
        inject(($rootScope: ng.IRootScopeService, ctrl: ManageTracesDialogController) => {

            //Arrange
            ctrl.selectedTraces[ctrl.data.artifactId] = ctrl.data.manualTraces;
            ctrl.selectedTraces[ctrl.data.artifactId][0].suspect = true;
            ctrl.selectedTraces[ctrl.data.artifactId][0].hasAccess = true;

            //Act
            ctrl.toggleTraces();

            //Assert
            expect(ctrl.selectedTraces[ctrl.data.artifactId][0].suspect).toBe(false);
        }));

    it("should get manual traces",
        inject(($rootScope: ng.IRootScopeService, ctrl: ManageTracesDialogController) => {



            //Act
            ctrl.getManualTraces();

            //Assert
            expect(ctrl.artifactId).toBe(15);
        }));

    it("should toggle flag for traces from true to false",
        inject(($rootScope: ng.IRootScopeService, ctrl: ManageTracesDialogController) => {

            //Arrange
            ctrl.data.manualTraces[0]["hasAccess"] = true;

            //Act
            ctrl.getManualTraces();

            //Assert
            expect(ctrl.artifactId).toBe(15);
            expect(ctrl.data.manualTraces[0]["cssClass"]).toBe("icon-glossary");
        }));

    it("add artifact from artifact picker to manual traces",
        inject(($rootScope: ng.IRootScopeService, ctrl: ManageTracesDialogController) => {

            //Arrange
            let selectedVM = {
                name: "published",
                key: "19",
                isExpandable: true,
                isExpanded: false,
                children: [],
                isSelectable: () => {
                    return true;
                },
                getCellClass: () => {
                    return ["test"];
                },
                getIcon: () => {
                    return "<i></i>";
                },
                model: {
                    "id": 19,
                    "name": "published",
                    "projectId": 1,
                    "parentId": 1,
                    "itemTypeId": 67,
                    "prefix": "AC",
                    "predefinedType": 4104,
                    "version": 15,
                    "orderIndex": 75,
                    "hasChildren": false,
                    "permissions": 4623,
                    "lockedByUser": {"id": 1005},
                    "lockedDateTime": "2016-10-11T13:48:55.09",
                    "parent": {
                        "id": 1,
                        "type": 1,
                        "name": "1",
                        "hasChildren": true
                    }
                }
            };

            //Act
            ctrl.onSelectionChanged([selectedVM]);
            ctrl.trace();

            //Assert
            expect(ctrl.data.manualTraces.length).toBe(2);
        }));
});
