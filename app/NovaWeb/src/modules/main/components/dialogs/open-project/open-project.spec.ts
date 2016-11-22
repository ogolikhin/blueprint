﻿import * as angular from "angular";
import "angular-mocks";
import {OpenProjectController} from "./open-project";
import {IDialogSettings} from "../../../../shared";
import {Enums, AdminStoreModels, TreeModels} from "../../../models";
import {IProjectService} from "../../../../managers/project-manager/project-service";
import {IArtifactManager, IStatefulArtifactFactory} from "../../../../managers/artifact-manager";
import {IColumnRendererParams} from "../../../../shared/widgets/bp-tree-view/";
import {ILocalizationService} from "../../../../core/localization/localizationService";

describe("OpenProjectController", () => {
    let localization: ILocalizationService;
    let projectService: IProjectService;
    let $sce: ng.ISCEService;
    let controller: OpenProjectController;
    let $scope: ng.IScope;

    beforeEach(inject(($rootScope: ng.IRootScopeService, _$sce_: ng.ISCEService) => {
        $scope = $rootScope.$new();
        localization = jasmine.createSpyObj("localization", ["get"]) as ILocalizationService;
        (localization.get as jasmine.Spy).and.callFake(name => name === "App_Header_Name" ? "Blueprint" : undefined);
        const $uibModalInstance = {} as ng.ui.bootstrap.IModalServiceInstance;
        projectService = {} as IProjectService;
        const artifactManager = {} as IArtifactManager;
        const statefulArtifactFactory = {} as IStatefulArtifactFactory;
        const dialogSettings = {} as IDialogSettings;
        $sce = _$sce_;
        controller = new OpenProjectController($scope, localization, $uibModalInstance,
            projectService, artifactManager, statefulArtifactFactory, dialogSettings, $sce);
    }));

    it("constructor sets root node", () => {
        // Arrange

        // Act

        // Assert
        expect(controller.rowData).toEqual([controller.factory.createInstanceItemNodeVM({
            id: 0,
            type: AdminStoreModels.InstanceItemType.Folder,
            name: "",
            hasChildren: true
        } as AdminStoreModels.IInstanceItem, true)]);
    });

    describe("columns", () => {
        it("column properties are correctly defined", () => {
            // Arrange

            // Act

            // Assert
            expect(controller.columns).toEqual([jasmine.objectContaining({
                headerName: "Blueprint",
                isGroup: true
            })]);
            expect(angular.isFunction(controller.columns[0].cellClass)).toEqual(true);
            expect(angular.isFunction(controller.columns[0].innerRenderer)).toEqual(true);
        });

        it("getCellClass returns correct result", () => {
            // Arrange
            const vm = {getCellClass: () => ["test"]} as TreeModels.ITreeNodeVM<any>;

            // Act
            const css = controller.columns[0].cellClass(vm);

            // Assert
            expect(css).toEqual(["test"]);
        });

        it("innerRenderer returns correct result", () => {
            // Arrange
            const vm = {
                getLabel() {
                    return "name";
                }
            } as TreeModels.ITreeNodeVM<any>;
            const cell = {} as HTMLElement;

            const params: IColumnRendererParams = {
                data: vm,
                $scope: $scope,
                eGridCell: cell
            };

            // Act
            const result = controller.columns[0].innerRenderer(params);

            // Assert
            expect(result).toEqual(`<span class="ag-group-value-wrapper"><i></i><span>name</span></span>`);
        });

        it("innerRenderer, when project, calls ok on enter", () => {
            // Arrange
            const model = {id: 3, type: AdminStoreModels.InstanceItemType.Project} as AdminStoreModels.IInstanceItem;
            const vm = controller.factory.createInstanceItemNodeVM(model);
            const cell = document.createElement("div");
            const params: IColumnRendererParams = {
                data: vm,
                $scope: $scope,
                eGridCell: cell
            };

            controller.columns[0].innerRenderer(params);
            spyOn(controller, "ok");

            // Act
            const event = new Event("keydown") as any;
            event.keyCode = 13;
            cell.dispatchEvent(event);

            // Assert
            expect(controller.ok).toHaveBeenCalled();
        });
    });

    it("onSelect, when selected project, sets selection", inject(($browser) => {
        // Arrange
        const model = {
            id: 3,
            parentFolderId: 1,
            name: "name",
            description: "abc",
            type: AdminStoreModels.InstanceItemType.Project,
            hasChildren: true
        } as AdminStoreModels.IInstanceItem;
        const vm = controller.factory.createInstanceItemNodeVM(model);

        // Act
        controller.onSelect(vm, true);

        // Assert
        $browser.defer.flush(); // wait for $applyAsync()
        expect(controller.isProjectSelected).toEqual(true);
        expect(controller.selectedItem).toEqual(vm);
        expect($sce.getTrustedHtml(controller.selectedDescription)).toEqual("abc");
        expect(controller.returnValue).toEqual(model);
    }));

    it("onSelect, when selected folder, sets selection", inject(($browser) => {
        // Arrange
        const model = {id: 3, type: AdminStoreModels.InstanceItemType.Folder} as AdminStoreModels.IInstanceItem;
        const vm = controller.factory.createInstanceItemNodeVM(model);

        // Act
        controller.onSelect(vm, true);

        // Assert
        $browser.defer.flush(); // wait for $applyAsync()
        expect(controller.isProjectSelected).toEqual(false);
        expect(controller.selectedItem).toEqual(vm);
        expect(controller.selectedDescription).toBeUndefined();
        expect(controller.returnValue).toBeUndefined();
    }));

    it("onDoubleClick, when project, sets selection and calls ok", inject(($browser) => {
        // Arrange
        const model = {id: 3, type: AdminStoreModels.InstanceItemType.Project} as AdminStoreModels.IInstanceItem;
        const vm = controller.factory.createInstanceItemNodeVM(model);
        spyOn(controller, "ok");

        // Act
        controller.onDoubleClick(vm);

        // Assert
        $browser.defer.flush(); // wait for $applyAsync()
        expect(controller.selectedItem).toEqual(vm);
        expect(controller.ok).toHaveBeenCalled();
    }));

    it("onError sets error message", () => {
        // Arrange
        (localization.get as jasmine.Spy).and.callFake(name => name === "Project_NoProjectsAvailable" ? "error" : undefined);

        // Act
        controller.onError("reason");

        // Assert
        expect(controller.errorMessage).toEqual("error");
    });
});
