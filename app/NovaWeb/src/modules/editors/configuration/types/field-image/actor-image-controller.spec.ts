import * as angular from "angular";
import "angular-mocks";
import "angular-messages";
import "angular-sanitize";
import "angular-ui-bootstrap";
import "angular-ui-tinymce";
import "ui-select";
import "angular-formly";
import "tinymce";

import {LocalizationServiceMock} from "../../../../core/localization/localization.mock";
import {MessageServiceMock} from "../../../../core/messages/message.mock";
import {IDialogService} from "../../../../shared/widgets/bp-dialog/bp-dialog";
import {BPFieldImageController} from "./field-image";
import {ActorImagePickerDialogServiceMock} from "./actor-image-choose-window.mock";
import {SettingsService, ISettingsService} from "../../../../core/configuration/settings";
import {ILocalizationService} from "../../../../core/localization/localizationService";
import {IMessageService} from "../../../../core/messages/message.svc";

describe("Actor image controller tests", () => {

    let controller: BPFieldImageController,
        scope,
        rootScope,
        compile,
        $controller: ng.IControllerService,
        imageModel;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("messageService", MessageServiceMock);
        $provide.service("dialogService", ActorImagePickerDialogServiceMock);
        $provide.service("settingsService", SettingsService);
    }));

    beforeEach(
        inject(
            ($compile: ng.ICompileService, $rootScope: ng.IRootScopeService, $controller) => {
                rootScope = $rootScope;
                compile = $compile;
                scope = rootScope.$new();

                imageModel = {
                    imageSource: ""
                };

                scope.model = {
                    image: imageModel,
                };

                scope["to"] = {
                    onChange($value: any, $field: AngularFormly.IFieldConfigurationObject, $scope: ng.IScope) {
                    }
                };

                scope.options = {
                    key: "image"
                };

                controller = $controller(BPFieldImageController, {$scope: scope});
            }
        )
    );

    beforeEach(angular.mock.inject(function (_$controller_) {
        $controller = _$controller_;
    }));

    describe("On file select function test ", () => {

        it("On file select function test", inject(($timeout: ng.ITimeoutService, localization: ILocalizationService,
                                                   $window: ng.IWindowService,
                                                   messageService: IMessageService, dialogService: IDialogService,
                                                   settingsService: ISettingsService) => {

            var base64 = "";

            var files = [{
                file: new Blob([base64], {type: "image/png"}),
                guid: "2788d782-4d7f-e611-82cc-a0999b0c8c40",
                name: imageModel,
                url: "svc/bpfilestore/file/2788d782-4d7f-e611"
            }];

            var readerSpy = spyOn(FileReader.prototype, "readAsDataURL");

            scope.onFileSelect(files, function () {
            });
            $timeout.flush();
            expect(readerSpy).toHaveBeenCalled();
        }));
    });

    describe("delete image for actor", () => {

        it("delete image for actor without readonly mode", inject(($timeout: ng.ITimeoutService, localization: ILocalizationService, $window: ng.IWindowService,
                                                                   messageService: IMessageService, dialogService: IDialogService, settingsService: ISettingsService) => {

            scope.onActorImageDelete();
            expect(scope.model.image === null).toBeTruthy();
        }));

        it("delete image for actor with read only mode", inject(($timeout: ng.ITimeoutService, localization: ILocalizationService, $window: ng.IWindowService,
                                                                 messageService: IMessageService, dialogService: IDialogService, settingsService: ISettingsService) => {

            scope.onActorImageDelete(true);
            expect(scope.model.image === imageModel).toBeTruthy();

        }));
    });
});

