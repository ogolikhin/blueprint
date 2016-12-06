import * as angular from "angular";
import "angular-mocks";
import {OpenProjectController} from "./open-project";
import {IDialogSettings} from "../../../../shared";
import {AdminStoreModels, TreeModels} from "../../../models";
import {IColumnRendererParams} from "../../../../shared/widgets/bp-tree-view/";
import {ILocalizationService} from "../../../../core/localization/localizationService";

describe("OpenProjectController", () => {
    let localization: ILocalizationService;
    let $sce: ng.ISCEService;
    let controller: OpenProjectController;
    let $scope: ng.IScope;

    beforeEach(inject(($rootScope: ng.IRootScopeService, _$sce_: ng.ISCEService) => {
        $scope = $rootScope.$new();
        localization = jasmine.createSpyObj("localization", ["get"]) as ILocalizationService;
        (localization.get as jasmine.Spy).and.callFake(name => name === "App_Header_Name" ? "Blueprint" : undefined);
        const $uibModalInstance = {} as ng.ui.bootstrap.IModalServiceInstance;
        const dialogSettings = {} as IDialogSettings;
        $sce = _$sce_;
        controller = new OpenProjectController($scope, localization, $uibModalInstance, dialogSettings, $sce);
    }));

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
        const vm = new TreeModels.InstanceItemNodeVM(undefined, model);

        // Act
        controller.onSelectionChanged([vm]);

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
        const vm = new TreeModels.InstanceItemNodeVM(undefined, model);

        // Act
        controller.onSelectionChanged([vm]);

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
        const vm = new TreeModels.InstanceItemNodeVM(undefined, model);
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
