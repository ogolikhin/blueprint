import * as angular from "angular";
import {SystemTask} from "../diagram/presentation/graph/shapes/system-task";
import {ShapesFactory} from "../diagram/presentation/graph/shapes/shapes-factory";
import {ITaskFlags} from "../../models/process-models";
import {ItemTypePredefined} from "../../../../main/models/enums";
import {ISystemTaskShape} from "../diagram/presentation/graph/models/";
import {UploadImageDirective} from "./upload-image";
import {IStatefulArtifactFactory} from "../../../../managers/artifact-manager";
import {StatefulArtifactFactoryMock} from "../../../../managers/artifact-manager/artifact/artifact.factory.mock";
import {MessageServiceMock} from "../../../../core/messages/message.mock";
import {IMessageService} from "../../../../core/messages/message.svc";
import {IFileUploadService, FileUploadService} from "../../../../core/file-upload/fileUploadService";

describe("UploadImage Directive", () => {
    let element: ng.IAugmentedJQuery;
    let scope: ng.IScope;
    let imageUpload: any;
    let isolatedScope: ng.IScope;

    let messageService: IMessageService;

    const directiveTemplate: string = "<button id=\"upload-image-btn\" class=\"btn btn-block button-white\" ng-click=\"uploadImage()\">Upload</button>";

    const sampleSystemTask: ISystemTaskShape = {
        id: 1, name: "", projectId: 1, typePrefix: "", parentId: 2,
        baseItemTypePredefined: ItemTypePredefined.PROShape,
        propertyValues: {}, associatedArtifact: null, personaReference: null, flags: <ITaskFlags>{}
    };
    const fakeRootScope = {
        config: {
            labels: {
                ["ST_Settings_Label"]: ""
            }
        }
    };
    const fakeUrl = "api to call get image";

    let shapesFactory: ShapesFactory;
    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService, $compileProvider: ng.ICompileProvider) => {
        $compileProvider.directive("uploadImage", UploadImageDirective.factory());
        $provide.service("fileUploadService", FileUploadService);
        $provide.service("statefulArtifactFactory", StatefulArtifactFactoryMock);
        $provide.service("messageService", MessageServiceMock);
    }));

    beforeEach(
        inject(($compile: ng.ICompileService,
                $rootScope: ng.IRootScopeService,
                $templateCache: ng.ITemplateCacheService,
                $injector: ng.auto.IInjectorService,
                statefulArtifactFactory: IStatefulArtifactFactory,
                _messageService_: IMessageService) => {
            shapesFactory = new ShapesFactory($rootScope, statefulArtifactFactory);
            $templateCache.put("/Areas/Web/App/Components/Storyteller/Directives/UploadImageTemplate.html", directiveTemplate);
            scope = $rootScope.$new();
            scope.$parent["$ctrl"] = {
                isReadonly: false
            };
            sampleSystemTask.propertyValues["associatedImageUrl"] = shapesFactory.createAssociatedImageUrlValue(fakeUrl);

            scope["systemTaskModel"] = new SystemTask(sampleSystemTask, fakeRootScope, shapesFactory.NEW_SYSTEM_TASK_LABEL, null, shapesFactory);

            /* tslint:disable:max-line-length */
            element = $compile("<upload-image data-image-container-class=\"file-upload_preview\" data-system-task-model=\"systemTaskModel\"  data-image-uploaded=\"imageUploaded\"></upload-image>")(scope);
            /* tslint:enable:max-line-length */

            scope.$digest();
            imageUpload = $injector.get("uploadImageDirective")[0].__proto__;

            isolatedScope = element.isolateScope();
            messageService = _messageService_;
        })
    );

    it("can show a directive", () => {
        expect(element.find("button#upload-image-btn")).toBeDefined();
    });

    it("downloadImage", inject(($injector: ng.auto.IInjectorService, $window: ng.IWindowService) => {
        //Arange
        spyOn($window, "open").and.callFake(function () {
            return;
        });
        //Act
        isolatedScope["downloadImage"]();
        // Assert
        expect($window.open).toHaveBeenCalledWith(fakeUrl, "_blank");
    }));
    it("clearImage", inject(($injector: ng.auto.IInjectorService) => {
        //Arange
        spyOn(imageUpload, "clearImageContainer");
        //Act
        isolatedScope["clearImage"]();
        // Assert
        expect(imageUpload.clearImageContainer).toHaveBeenCalled();
    }));

    xit("createImage ", function (done) {
        inject(
            ($injector: ng.auto.IInjectorService, $compile: ng.ICompileService) => {
                //Arange
                const image = new Image();
                const imageElement = $compile(image)(scope);

                spyOn(imageUpload, "toggleButtons").and.callThrough();
                const attr = {};
                attr["imageContainerClass"] = "file-upload_preview";

                //Act
                imageUpload.createImage(scope, element, attr, image);
                imageElement.trigger("onload");

                // Assert
                setTimeout(function () {
                    expect(imageUpload.toggleButtons).toHaveBeenCalled();
                    done();
                }, 0);
            });
    });


    xit("toggleButtons", inject(($injector: ng.auto.IInjectorService) => {
        //Act
        imageUpload.toggleButtons(scope, element, true);
        // Assert
        expect(element.find("#upload-image-btn").text()).toEqual("Change");
    }));

    it("uploadImage", inject(($injector: ng.auto.IInjectorService) => {
        //Arange
        const imageBtn = element.find("input")[0];
        const spy = spyOn(imageBtn, "click");
        //Act
        isolatedScope["uploadImage"]();
        // Assert
        expect(spy).toHaveBeenCalled();
    }));

    it("fileChanged", inject(($injector: ng.auto.IInjectorService, fileUploadService: IFileUploadService) => {
        //Arrange
        const fileMock = {
            files: {
                [0]: {
                    type: "image/jpeg",
                    name: "test.jpg",
                    size: 10000
                }
            }
        };
        spyOn(fileUploadService, "uploadToFileStore").and.callThrough();
        //Act
        isolatedScope["fileChanged"](fileMock);
        //Assert
        expect(fileUploadService.uploadToFileStore).toHaveBeenCalled();
    }));

    it("fileChanged, size over 2mb", inject(($injector: ng.auto.IInjectorService, fileUploadService: IFileUploadService) => {
        //Arrange
        const fileMock = {
            files: {
                [0]: {
                    type: "image/jpeg",
                    name: "test.jpg",
                    size: 2097153
                }
            }
        };
        spyOn(fileUploadService, "uploadToFileStore").and.callThrough();
        //Act
        isolatedScope["fileChanged"](fileMock);
        //Assert
        expect(isolatedScope["sizeError"]).toBeTruthy();
    }));

    it("fileChanged, file type not image", inject(($injector: ng.auto.IInjectorService, fileUploadService: IFileUploadService) => {
        //Arrange
        const fileMock = {
            files: {
                [0]: {
                    type: "application/pdf",
                    name: "test.pdf",
                    size: 1000
                }
            }
        };
        spyOn(fileUploadService, "uploadToFileStore").and.callThrough();
        //Act
        isolatedScope["fileChanged"](fileMock);
        //Assert
        expect(isolatedScope["typeError"]).toBeTruthy();
    }));

});
