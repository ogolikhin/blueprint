import * as angular from "angular";
import "angular-mocks";
import {OpenProjectController} from "./open-project";
import {IDialogSettings} from "../../../../shared";
import {AdminStoreModels, TreeModels} from "../../../models";

describe("OpenProjectController", () => {
    let $sce: ng.ISCEService;
    let controller: OpenProjectController;
    let $scope: ng.IScope;

    beforeEach(inject(($rootScope: ng.IRootScopeService, _$sce_: ng.ISCEService) => {
        $scope = $rootScope.$new();
        const $uibModalInstance = {} as ng.ui.bootstrap.IModalServiceInstance;
        const dialogSettings = {} as IDialogSettings;
        $sce = _$sce_;
        controller = new OpenProjectController($scope, $uibModalInstance, dialogSettings, $sce);
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
        expect(controller.selectedName).toEqual("name");
        expect($sce.getTrustedHtml(controller.selectedDescription)).toEqual("abc");
        expect(controller.returnValue).toEqual(3);
    }));

    it("onSelect, when selected folder, sets selection", inject(($browser) => {
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
        $browser.defer.flush(); // wait for $applyAsync()
        expect(controller.isProjectSelected).toEqual(false);
        expect(controller.selectedName).toEqual("def");
        expect(controller.selectedDescription).toBeUndefined();
        expect(controller.returnValue).toBeUndefined();
    }));

    it("onDoubleClick, sets selection and calls ok", inject(($browser) => {
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
        expect(controller.returnValue).toEqual(8);
        expect(controller.ok).toHaveBeenCalled();
    }));
});
