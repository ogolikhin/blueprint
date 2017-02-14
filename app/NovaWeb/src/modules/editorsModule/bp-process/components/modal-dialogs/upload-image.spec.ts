import {IDownloadService} from "../../../../commonModule/download/download.service";
import {DownloadServiceMock} from "../../../../commonModule/download/download.service.mock";
import {FileUploadService, IFileUploadService} from "../../../../commonModule/fileUpload/fileUpload.service";
import {MessageServiceMock} from "../../../../main/components/messages/message.mock";
import {IMessageService} from "../../../../main/components/messages/message.svc";
import {ItemTypePredefined} from "../../../../main/models/itemTypePredefined.enum";
import {IStatefulArtifactFactory} from "../../../../managers/artifact-manager";
import {StatefulArtifactFactoryMock} from "../../../../managers/artifact-manager/artifact/artifact.factory.mock";
import {ISystemTaskShape} from "../../models/process-models";
import {ITaskFlags} from "../../models/process-models";
import {ShapesFactory} from "../diagram/presentation/graph/shapes/shapes-factory";
import {SystemTask} from "../diagram/presentation/graph/shapes/system-task";
import {UploadImageDirective} from "./upload-image";
import * as angular from "angular";

describe("UploadImage Directive", () => {
    let element: ng.IAugmentedJQuery;
    let scope: ng.IScope;
    let imageUpload: any;
    let isolatedScope: ng.IScope;

    let messageService: IMessageService;
    let downloadService: IDownloadService;
    let $q: ng.IQService;

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
        $provide.service("downloadService", DownloadServiceMock);
    }));

    beforeEach(
        inject(($compile: ng.ICompileService,
                $rootScope: ng.IRootScopeService,
                $templateCache: ng.ITemplateCacheService,
                $injector: ng.auto.IInjectorService,
                statefulArtifactFactory: IStatefulArtifactFactory,
                _messageService_: IMessageService,
                _downloadService_: IDownloadService,
                _$q_: ng.IQService) => {
            shapesFactory = new ShapesFactory($rootScope, statefulArtifactFactory);
            $templateCache.put("/Areas/Web/App/Components/Storyteller/Directives/UploadImageTemplate.html", directiveTemplate);
            scope = $rootScope.$new();
            scope.$parent["$ctrl"] = {
                isReadonly: false
            };
            sampleSystemTask.propertyValues["associatedImageUrl"] = shapesFactory.createAssociatedImageUrlValue(fakeUrl);

            scope["systemTaskModel"] = new SystemTask(sampleSystemTask, fakeRootScope, shapesFactory.NEW_SYSTEM_TASK_PERSONAREFERENCE, null, shapesFactory);

            // tslint:disable-next-line: max-line-length
            element = $compile("<upload-image data-image-container-class=\"file-upload_preview\" data-system-task-model=\"systemTaskModel\"  data-image-uploaded=\"imageUploaded\"></upload-image>")(scope);

            scope.$digest();
            imageUpload = $injector.get("uploadImageDirective")[0].__proto__;

            isolatedScope = element.isolateScope();
            messageService = _messageService_;
            downloadService = _downloadService_;
            $q = _$q_;
        })
    );

    it("can show a directive", () => {
        expect(element.find("button#upload-image-btn")).toBeDefined();
    });

    it("downloadImage - expects download service to be called with fake url", inject(($injector: ng.auto.IInjectorService) => {
        //Arange
        const spyDownload = spyOn(downloadService, "downloadFile");
        //Act
        isolatedScope["downloadImage"]();
        // Assert
        expect(spyDownload).toHaveBeenCalledWith(fakeUrl);
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
        const files = {
            [0]: {
                type: "image/jpeg",
                name: "test.jpg",
                size: 10000
            }
        };
        spyOn(fileUploadService, "uploadToFileStore").and.callThrough();
        //Act
        isolatedScope["fileChanged"](files);
        //Assert
        expect(fileUploadService.uploadToFileStore).toHaveBeenCalled();
    }));

    it("fileChanged, size over 2mb", inject(($injector: ng.auto.IInjectorService, fileUploadService: IFileUploadService) => {
        //Arrange
        const files = {
            [0]: {
                type: "image/jpeg",
                name: "test.jpg",
                size: 2097153
            }
        };
        spyOn(fileUploadService, "uploadToFileStore").and.callThrough();
        //Act
        isolatedScope["fileChanged"](files);
        //Assert
        expect(isolatedScope["sizeError"]).toBeTruthy();
    }));

    it("fileChanged, file type not image", inject(($injector: ng.auto.IInjectorService, fileUploadService: IFileUploadService) => {
        //Arrange
        const files = {
            [0]: {
                type: "application/pdf",
                name: "test.pdf",
                size: 1000
            }
        };
        spyOn(fileUploadService, "uploadToFileStore").and.callThrough();
        //Act
        isolatedScope["fileChanged"](files);
        //Assert
        expect(isolatedScope["typeError"]).toBeTruthy();
    }));

});
