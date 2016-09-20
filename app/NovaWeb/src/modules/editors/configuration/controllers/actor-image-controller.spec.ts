import "angular";
import "Rx";
import "angular-mocks";
import "angular-messages";
import "angular-sanitize";
import "angular-ui-bootstrap";
import "angular-ui-tinymce";
import "ui-select";
import "tinymce";

import { ILocalizationService } from "../../../core/localization";
import { LocalizationServiceMock } from "../../../core/localization/localization.mock";
import { MessageServiceMock } from "../../../core/messages/message.mock";
import { IMessageService } from "../../../core/messages";
import { DialogServiceMock, IDialogService } from "../../../shared/widgets/bp-dialog/bp-dialog";
import { SettingsService, ISettingsService } from "../../../core";
import { actorImageController } from "./actor-image-controller";
import { ActorImagePickerDialogServiceMock } from "./actor-image-choose-window-mock";

describe("Actor image controller tests", () => {

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("messageService", MessageServiceMock);
        $provide.service("dialogService", ActorImagePickerDialogServiceMock);
        $provide.service("settingsService", SettingsService);
    }));


    let compile, scope, rootScope, module, imageName, newImageName;

    beforeEach(
        inject(
            ($compile: ng.ICompileService, $rootScope: ng.IRootScopeService) => {
                rootScope = $rootScope;
                compile = $compile;
                scope = rootScope.$new();
                scope.model = {};
                scope.options = {};
                scope.options.key = 'image';

                imageName = 'test.png';
                newImageName = 'test2.png';

                scope.model.image = imageName;

            }
        )
    );

    describe("On file select function test ", () => {

        it("On file select function test", inject(($timeout: ng.ITimeoutService, localization: ILocalizationService,
                                                   $window: ng.IWindowService,
                                                                   messageService: IMessageService, dialogService: IDialogService,
                                                   settingsService: ISettingsService) => {

            actorImageController(scope, localization, $window, messageService, dialogService, settingsService);

            var base64 = "";

            var files = [{
                file: new Blob([base64], {type: 'image/png'}),
                guid: '2788d782-4d7f-e611-82cc-a0999b0c8c40',
                name: newImageName,
                url: 'svc/bpfilestore/file/2788d782-4d7f-e611'
            }];

            var readerSpy = spyOn(FileReader.prototype, 'readAsDataURL');

            scope.onFileSelect(files, function(){});

            $timeout.flush();

            expect(readerSpy).toHaveBeenCalled();

        }));

        it("If no file selected function test", inject(($timeout: ng.ITimeoutService, localization: ILocalizationService,
                                                   $window: ng.IWindowService,
                                                   messageService: IMessageService, dialogService: IDialogService,
                                                   settingsService: ISettingsService) => {

            actorImageController(scope, localization, $window, messageService, dialogService, settingsService);

            var files = [];

            var readerSpy = spyOn(FileReader.prototype, 'readAsDataURL');

            scope.onFileSelect(files, function(){});

            $timeout.flush();

            expect(readerSpy).not.toHaveBeenCalled();
        }));
    });

    describe("delete image for actor", () => {

        it("delete image for actor without readonly mode", inject(($timeout: ng.ITimeoutService, localization: ILocalizationService, $window: ng.IWindowService,
                                                                       messageService: IMessageService, dialogService: IDialogService, settingsService: ISettingsService) => {

            //actorImageController.$inject = ["localization", "$window", "messageService", "dialogService", "settingsService"];

            actorImageController(scope, localization, $window, messageService, dialogService, settingsService);

            scope.onActorImageDelete();

            expect(scope.model.image === null).toBeTruthy();

        }));

        it("delete image for actor with read only mode", inject(($timeout: ng.ITimeoutService, localization: ILocalizationService, $window: ng.IWindowService,
                                                                 messageService: IMessageService, dialogService: IDialogService, settingsService: ISettingsService) => {

            actorImageController.$inject = ["localization", "$window", "messageService", "dialogService", "settingsService"];

            actorImageController(scope, localization, $window, messageService, dialogService, settingsService);

            scope.onActorImageDelete(true);


            expect(scope.model.image === imageName).toBeTruthy()

        }));
    })

})

