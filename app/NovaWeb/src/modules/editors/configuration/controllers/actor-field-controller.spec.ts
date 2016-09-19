import "angular";
import "Rx";
import "angular-mocks";
import "angular-messages";
import "angular-sanitize";
import "angular-ui-bootstrap";
import "angular-ui-tinymce";
import "ui-select";
import "angular-formly";
import "angular-formly-templates-bootstrap";
import "tinymce";
import {PrimitiveType} from "../../../main/models/enums";
import { ILocalizationService } from "../../../core/localization";
import { LocalizationServiceMock } from "../../../core/localization/localization.mock";
import { MessageServiceMock } from "../../../core/messages/message.mock";
import { IMessageService } from "../../../core/messages";
import { DialogServiceMock, IDialogService } from "../../../shared/widgets/bp-dialog/bp-dialog";
import {formlyConfig} from "../formly-config";
import { SettingsService } from "../../../core";
import { ISelectionManager, SelectionManager } from "../../../managers/selection-manager/selection-manager";
import { actorController } from "./actor-field-controller";
import { ArtifactPickerDialogServiceMock } from "./artifact-picker-dialog-mock";
import { IStatefulArtifact } from "../../../managers/models";



describe("Actor Inheritance controller", () => {
    
    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("localization", LocalizationServiceMock)
        $provide.service("messageService", MessageServiceMock)
        $provide.service("dialogService", ArtifactPickerDialogServiceMock);     
        $provide.service("selectionManager", SelectionManager); 
    }));

    let compile, scope, rootScope, module;

    beforeEach(
        inject(
            ($compile: ng.ICompileService, $rootScope: ng.IRootScopeService) => {
                rootScope = $rootScope;
                compile = $compile;
                scope = rootScope.$new();
                scope.model = {};
                scope.options = {
                    key: "Key"
                }         

                scope.model[scope.options.key] = {
                    pathToProject: [],
                    actorName: "Name",
                    actorPrefix: "Prefix",
                    actorId: 5,
                    hasAccess: true,
                    isProjectPathVisible: true
                };
            }
        )
    );    

    it("delete base actor", inject((localization: ILocalizationService, $window: ng.IWindowService, messageService: IMessageService, dialogService: IDialogService, selectionManager: ISelectionManager) => {
        
        let ac = actorController(scope, localization, $window, messageService, dialogService, selectionManager);

        // Act
        scope.deleteBaseActor();     

        expect(scope.model[scope.options.key] === null).toBeTruthy();
    }));

    it("select base actor", inject(($timeout: ng.ITimeoutService, localization: ILocalizationService, $window: ng.IWindowService, messageService: IMessageService, dialogService: IDialogService, selectionManager: ISelectionManager) => {                

        let ac = actorController(scope, localization, $window, messageService, dialogService, selectionManager);

        // Act
        scope.selectBaseActor();
        $timeout.flush();

        expect(scope.model[scope.options.key] !== null).toBeTruthy();
        expect(scope.model[scope.options.key].actorId === 10).toBeTruthy();
        expect(scope.model[scope.options.key].actorName === "actor name").toBeTruthy();
        expect(scope.model[scope.options.key].pathToProject[0] === "parent").toBeTruthy();
    }));

    it("current artifact as base actor", inject(($timeout: ng.ITimeoutService, localization: ILocalizationService, $window: ng.IWindowService, messageService: IMessageService, dialogService: IDialogService, selectionManager: ISelectionManager) => {
        
        let artifact: any = {
            id: 10                       
        };
        selectionManager.setArtifact(artifact);
        let ac = actorController(scope, localization, $window, messageService, dialogService, selectionManager);
        var addErrorSpy = spyOn(messageService, "addError");

        // Act
        scope.selectBaseActor();
        $timeout.flush();

        expect(addErrorSpy).toHaveBeenCalledWith("App_Properties_Actor_SameBaseActor_ErrorMessage");
    }));
});