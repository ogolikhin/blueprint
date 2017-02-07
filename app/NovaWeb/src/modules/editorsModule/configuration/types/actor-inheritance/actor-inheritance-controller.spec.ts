import * as angular from "angular";
import "rx/dist/rx.lite";
import "angular-mocks";
import "angular-messages";
import "angular-sanitize";
import "angular-ui-bootstrap";
import "angular-ui-tinymce";
import "ui-select";
import "angular-formly";
import "angular-formly-templates-bootstrap";
import "tinymce";
import {LocalizationServiceMock} from "../../../../commonModule/localization/localization.service.mock";
import {IDialogService} from "../../../../shared/widgets/bp-dialog/bp-dialog";
import {ISelectionManager, SelectionManager} from "../../../../managers/selection-manager/selection-manager";
import {BPFieldInheritFromController} from "./actor-inheritance";
import {ArtifactPickerDialogServiceMock} from "./artifact-picker-dialog.mock";
import {NavigationServiceMock} from "../../../../commonModule/navigation/navigation.service.mock";
import {ILocalizationService} from "../../../../commonModule/localization/localization.service";
import {MessageServiceMock} from "../../../../main/components/messages/message.mock";
import {IMessageService} from "../../../../main/components/messages/message.svc";
import {LoadingOverlayServiceMock} from "../../../../commonModule/loadingOverlay/loadingOverlay.service.mock";

describe("Actor Inheritance controller", () => {

    let controller: BPFieldInheritFromController,
        scope,
        rootScope,
        compile,
        $controller: ng.IControllerService;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("messageService", MessageServiceMock);
        $provide.service("dialogService", ArtifactPickerDialogServiceMock);
        $provide.service("selectionManager", SelectionManager);
        $provide.service("navigationService", NavigationServiceMock);
        $provide.service("loadingOverlayService", LoadingOverlayServiceMock);
    }));

    beforeEach(
        inject(
            ($compile: ng.ICompileService, $rootScope: ng.IRootScopeService, $controller) => {
                rootScope = $rootScope;
                compile = $compile;
                scope = rootScope.$new();
                scope.model = {};
                scope.options = {
                    key: "Key"
                };

                scope.model[scope.options.key] = {
                    pathToProject: [],
                    actorName: "Name",
                    actorPrefix: "Prefix",
                    actorId: 5,
                    hasAccess: true,
                    isProjectPathVisible: true
                };

                scope["to"] = {
                    onChange($value: any, $field: AngularFormly.IFieldConfigurationObject, $scope: ng.IScope) {
                        return;
                    }
                };

                controller = $controller(BPFieldInheritFromController, {$scope: scope});
            }
        )
    );

    beforeEach(angular.mock.inject(function (_$controller_) {
        $controller = _$controller_;
    }));

    it("delete base actor", inject((localization: ILocalizationService, $window: ng.IWindowService, messageService: IMessageService,
                                    dialogService: IDialogService, selectionManager: ISelectionManager) => {

        // Act
        scope.deleteBaseActor();

        expect(scope.model[scope.options.key] === null).toBeTruthy();
    }));

    it("select base actor", inject(($timeout: ng.ITimeoutService, localization: ILocalizationService, $window: ng.IWindowService,
                                    messageService: IMessageService, dialogService: IDialogService, selectionManager: ISelectionManager) => {

        // Act
        scope.selectBaseActor();
        $timeout.flush();

        expect(scope.model[scope.options.key] !== null).toBeTruthy();
        expect(scope.model[scope.options.key].actorId === 10).toBeTruthy();
        expect(scope.model[scope.options.key].actorName === "actor name").toBeTruthy();
        expect(scope.model[scope.options.key].pathToProject[0] === "parent").toBeTruthy();
    }));

    it("current artifact as base actor", inject(($timeout: ng.ITimeoutService, localization: ILocalizationService, $window: ng.IWindowService,
                                                 messageService: IMessageService, dialogService: IDialogService, selectionManager: ISelectionManager) => {

        let artifact: any = {
            id: 10
        };
        selectionManager.setArtifact(artifact);

        const addErrorSpy = spyOn(messageService, "addError");

        // Act
        scope.selectBaseActor();
        $timeout.flush();

        expect(addErrorSpy).toHaveBeenCalledWith("App_Properties_Actor_SameBaseActor_ErrorMessage");
    }));
});