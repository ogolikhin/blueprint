import * as angular from "angular";
import {SystemTask} from "../diagram/presentation/graph/shapes/system-task";
import {ShapesFactory} from "../diagram/presentation/graph/shapes/shapes-factory";
import {ITaskFlags} from "../../models/process-models";
import {ItemTypePredefined} from "../../../../main/models/enums";
import {ISystemTaskShape} from "../diagram/presentation/graph/models/";
import {IFileUploadService, FileUploadService} from "../../../../core/file-upload/";
import {UploadImageDirective} from "./upload-image";

describe("UploadImage Directive", () => {
    var element: ng.IAugmentedJQuery;
    var scope: ng.IScope;
    var imageUpload: any;
    var isolatedScope: ng.IScope;

    var directiveTemplate: string = "<button id=\"upload-image-btn\" class=\"btn btn-block button-white\" ng-click=\"uploadImage()\">Upload</button>";

    var sampleSystemTask: ISystemTaskShape = {
        id: 1, name: "", projectId: 1, typePrefix: "", parentId: 2,
        baseItemTypePredefined: ItemTypePredefined.PROShape,
        propertyValues: {}, associatedArtifact: null, flags: <ITaskFlags>{}
    };
    var fakeRootScope = {
        config: {
            labels: {
                ["ST_Settings_Label"]: ""
            }
        }
    };
    var fakeUrl = "api to call get image";

    var shapesFactory: ShapesFactory;
    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService, $compileProvider: ng.ICompileProvider) => {
        $compileProvider.directive("uploadImage", UploadImageDirective.factory());
        $provide.service("fileUploadService", FileUploadService);
        $provide.service("shapesFactoryService", ShapesFactory);
    }));

    beforeEach(
        inject(($compile: ng.ICompileService, $rootScope: ng.IRootScopeService,
            $templateCache: ng.ITemplateCacheService, $injector: ng.auto.IInjectorService, shapesFactoryService: ShapesFactory) => {
            shapesFactory = shapesFactoryService;
            $templateCache.put("/Areas/Web/App/Components/Storyteller/Directives/UploadImageTemplate.html", directiveTemplate);
            scope = $rootScope.$new();
            scope.$parent["vm"] = {
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
                var image = new Image();
                var imageElement = $compile(image)(scope);

                spyOn(imageUpload, "toggleButtons").and.callThrough();
                var attr = {};
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
    xit("uploadImage", inject(($injector: ng.auto.IInjectorService) => {
        //Arange
        spyOn($.fn, "click");
        //Act
        isolatedScope["uploadImage"]();
        // Assert           
        expect($.fn.click).toHaveBeenCalled();
    }));

    xit("fileChanged", inject(($injector: ng.auto.IInjectorService, fileUploadService: IFileUploadService) => {
        //Arrange
        var fileMock = {
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

    xit("fileChanged, size over 2mb", inject(($injector: ng.auto.IInjectorService, fileUploadService: IFileUploadService) => {
        //Arrange
        var fileMock = {
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

    xit("fileChanged, file type not image", inject(($injector: ng.auto.IInjectorService, fileUploadService: IFileUploadService) => {
        //Arrange
        var fileMock = {
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