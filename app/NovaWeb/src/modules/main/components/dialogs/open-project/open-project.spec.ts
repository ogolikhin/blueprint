import "angular";
import "angular-mocks";
import {OpenProjectController} from "./open-project";
import {IDialogSettings} from "../../../../shared";
import {AdminStoreModels, TreeModels} from "../../../models";

describe("OpenProjectController", () => {
    let $sce: ng.ISCEService;
    let controller: OpenProjectController;
    let $scope: ng.IScope;
    let $window: ng.IWindowService;
    let $timeout: ng.ITimeoutService;

    beforeEach(inject(($rootScope: ng.IRootScopeService, _$sce_: ng.ISCEService, _$window_: ng.IWindowService, _$timeout_: ng.ITimeoutService) => {
        $scope = $rootScope.$new();
        const $uibModalInstance = {} as ng.ui.bootstrap.IModalServiceInstance;
        const dialogSettings = {} as IDialogSettings;
        $sce = _$sce_;
        $window = _$window_;
        $timeout = _$timeout_;
        controller = new OpenProjectController($window, $scope, $uibModalInstance, dialogSettings, $sce, $timeout);
    }));

    it("onSelect, when selected project, sets selection", inject(() => {
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
        expect(controller.isProjectSelected).toEqual(true);
        expect(controller.selectedName).toEqual(model.name);
        expect(controller.selectedDescription).toEqual(model.description);
        expect(controller.returnValue).toEqual(model);
    }));

    it("onSelect, when selected folder, sets selection", inject(() => {
        // Arrange
        const model = {
            id: 2,
            name: "def",
            type: AdminStoreModels.InstanceItemType.Folder
        } as AdminStoreModels.IInstanceItem;
        const vm = new TreeModels.InstanceItemNodeVM(undefined, model);

        // Act
        controller.onSelectionChanged([vm]);

        // Assert
        expect(controller.isProjectSelected).toEqual(false);
        expect(controller.selectedName).toEqual(model.name);
        expect(controller.selectedDescription).toEqual(model.description);
        expect(controller.returnValue).toEqual(undefined);
    }));

    it("onDoubleClick, when a project, sets selection and return value and calls ok", inject(($browser) => {
        // Arrange
        const model = {
            id: 8,
            name: "ghi",
            type: AdminStoreModels.InstanceItemType.Project
        } as AdminStoreModels.IInstanceItem;
        const vm = new TreeModels.InstanceItemNodeVM(undefined, model);
        spyOn(controller, "ok");

        // Act
        controller.onDoubleClick(vm);

        // Assert
        $browser.defer.flush(); // wait for $applyAsync()
        expect(controller.selectedName).toEqual("ghi");
        expect(controller.selectedDescription).toBeUndefined();
        expect(controller.returnValue).toEqual(model);
        expect(controller.ok).toHaveBeenCalled();
    }));

    it("onDoubleClick, when a folder, sets selection, clears return value and does not call ok", inject(($browser) => {
        // Arrange
        const model = {
            id: 9,
            name: "jkl",
            type: AdminStoreModels.InstanceItemType.Folder
        } as AdminStoreModels.IInstanceItem;
        const vm = new TreeModels.InstanceItemNodeVM(undefined, model);
        spyOn(controller, "ok");

        // Act
        controller.onDoubleClick(vm);

        // Assert
        $browser.defer.flush(); // wait for $applyAsync()
        expect(controller.selectedName).toEqual("jkl");
        expect(controller.selectedDescription).toBeUndefined();
        expect(controller.returnValue).toBeUndefined();
        expect(controller.ok).not.toHaveBeenCalled();
    }));
});
