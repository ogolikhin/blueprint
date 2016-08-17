import "angular";
import "angular-mocks";
import { SettingsService } from "../../../../core";
import { MessageService } from "../../../../shell/";
import { ProjectManager, Models } from "../../../";
import { ArtifactPickerController } from "./bp-artifact-picker";
import { SelectionManager } from "../../../services/selection-manager";

import { BPTreeControllerMock, ITreeNode } from "../../../../shared/widgets/bp-tree/bp-tree.mock";
import { LocalizationServiceMock } from "../../../../core/localization/localization.mock";
import { ProjectRepositoryMock } from "../../../services/project-repository.mock";
import { ModalServiceInstanceMock } from "../open-project.spec.ts";
import { DialogServiceMock, DialogTypeEnum } from "../../../../shared/widgets/bp-dialog/bp-dialog";

describe("Artifact Picker", () => {
    let isReloadCalled: number = 0;
    let $scope;
    let elem;
    var controller: ArtifactPickerController;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("settings", SettingsService);
        $provide.service("messageService", MessageService);
        $provide.service("projectRepository", ProjectRepositoryMock);
        $provide.service("projectManager", ProjectManager);
        $provide.service("selectionManager", SelectionManager);
        $provide.service("explorer", ArtifactPickerController);
        $provide.service("dialogService", DialogServiceMock);
    }));

    beforeEach(inject((
        $rootScope: ng.IRootScopeService,
        localization: LocalizationServiceMock,
        projectRepository: ProjectRepositoryMock,
        projectManager: ProjectManager,
        $compile: ng.ICompileService,
        selectionManager: SelectionManager,
        dialogService: DialogServiceMock
    ) => {
        $rootScope["config"] = {
            "settings": {
                "StorytellerMessageTimeout": `{ "Warning": 0, "Info": 3000, "Error": 0 }`
            }
        };
        projectManager.initialize();
        projectManager.loadProject(new Models.Project({ id: 1, name: "Project 1" }));
        $rootScope.$digest();

        $scope = $rootScope.$new();

        elem = angular.element(`<div ag-grid="ctrl.gridOptions" class="ag-grid"></div>`);

        controller = new ArtifactPickerController(
            $scope,
            localization,
            new ModalServiceInstanceMock(),
            projectManager,
            selectionManager,
            projectRepository,
            dialogService,
            {
                type: DialogTypeEnum.Base,
                header: "test" ,
                message: "test" ,
                cancelButton: "test",
                okButton: "test",
                template: "test",
                controller: null,
                css: null,
                backdrop: false
            });

        controller["tree"] = new BPTreeControllerMock();      

        $compile(elem)($scope);
        $scope.$digest();
    }));


    it("check property map", inject(() => {
        // Assert
        expect(controller.propertyMap).toBeDefined();
        expect(controller.propertyMap["id"]).toEqual("id");
        expect(controller.propertyMap["type"]).toEqual("type");
        expect(controller.propertyMap["name"]).toEqual("name");
        expect(controller.propertyMap["hasChildren"]).toEqual("hasChildren");
    }));

    it("check columns", inject(() => {
        // Assert
        var column = controller.columns[0];
        expect(column).toBeDefined();
        expect(column.headerName).toEqual("");
        expect(column.field).toEqual("name");
        expect(column.cellRenderer).toEqual("group");
        expect(column.suppressMenu).toEqual(true);
        expect(column.suppressFiltering).toEqual(true);
        expect(column.suppressFiltering).toEqual(true);
    }));


    it("doLoad", inject(($rootScope: ng.IRootScopeService) => {
        // Arrange
        isReloadCalled = 0;
        controller.tree.reload = function (data: any[], id?: number) {
            isReloadCalled += 1;
        };
        // Act
        controller.doLoad(new Models.Project({ id: 1, name: "Project 1" }));
        $rootScope.$digest();
        // Assert
        expect(isReloadCalled).toEqual(1);
    }));

    it("doLoad (project view)", inject(($rootScope: ng.IRootScopeService) => {
        // Arrange
        isReloadCalled = 0;
        controller.tree.reload = function (data: any[], id?: number) {
            isReloadCalled += 1;
        };
        // Act
        controller.projectView = true;
        controller.doLoad(null);
        $rootScope.$digest();
        // Assert
        expect(isReloadCalled).toEqual(1);

    }));

    it("doSelect (not project view)", inject(($rootScope: ng.IRootScopeService, projectManager: ProjectManager) => {
        // Arrange
        isReloadCalled = 0;
        controller.tree.reload = function (data: any[], id?: number) {
            isReloadCalled += 1;
        };
        projectManager.projectCollection.onNext([new Models.Project({ id: 1, name: "Project 1" })]);
        $rootScope.$digest();
        // Act
        controller.projectView = false;
        controller.doSelect(({ id: 1, name: "Project 1", type: 1 } as ITreeNode));
        $rootScope.$digest();
        // Assert
        expect(isReloadCalled).toEqual(0);

    }));

    it("doSelect (project view)", inject(($rootScope: ng.IRootScopeService, projectManager: ProjectManager) => {
        // Arrange
        isReloadCalled = 0;
        controller.tree.reload = function (data: any[], id?: number) {
            isReloadCalled += 1;
        };
        projectManager.projectCollection.onNext([new Models.Project({ id: 1, name: "Project 1" })]);
        $rootScope.$digest();
        // Act
        controller.projectView = true;
        controller.doSelect(({ id: 1, name: "Project 1", type: 1 } as ITreeNode));
        $rootScope.$digest();

        // Assert
        expect(isReloadCalled).toEqual(1);
    }));

    it("doSync (not project view)", inject(($rootScope: ng.IRootScopeService, projectManager: ProjectManager) => {
        // Arrange
        controller.projectView = false;
        // Act
        var result = controller.doSync(({ id: 1, name: "Project 1", type: 1 } as ITreeNode));
        $rootScope.$digest();

        // Assert
        expect(result.id).toEqual(1);
    }));

});