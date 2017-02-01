import {IDownloadService} from "../../../../commonModule/download";
import {ISystemTask} from "../diagram/presentation/graph/models/";
import {IFileUploadService, IFileResult} from "../../../../commonModule/fileUpload/fileUpload.service";
import {IMessageService} from "../../../../main/components/messages/message.svc";

export interface IUploadImageScope extends ng.IScope {
    uploadImage: () => void;
    downloadImage: () => void;
    clearImage: () => void;
    fileChanged: (element: any) => void;
    systemTaskModel: ISystemTask;
    imageUploaded: boolean;
    typeError: boolean;
    sizeError: boolean;
    buttonsContainerEnabled: boolean;
    isReadonly: boolean;
    imageUrl: string;
    imageAlt: string;
}
export class UploadImageDirective implements ng.IDirective {
    public scope = {
        systemTaskModel: "=",
        imageUploaded: "=",
        typeError: "=",
        sizeError: "=",
        buttonsContainerEnabled: "="
    };
    public restrict = "E";

    constructor(private fileUploadService: IFileUploadService,
                private $timeout: ng.ITimeoutService,
                private $compile: ng.ICompileService,
                private messageService: IMessageService,
                private downloadService: IDownloadService) {
    }

    public static factory(): ng.IDirectiveFactory {
        const directive: ng.IDirectiveFactory = (fileUploadService: IFileUploadService,
                                                 $timeout: ng.ITimeoutService,
                                                 $compile: ng.ICompileService,
                                                 messageService: IMessageService,
                                                 downloadService: IDownloadService) =>
            new UploadImageDirective(fileUploadService, $timeout, $compile, messageService, downloadService);
        directive.$inject = ["fileUploadService",
            "$timeout",
            "$compile",
            "messageService",
            "downloadService"];
        return directive;
    }

    public link: ng.IDirectiveLinkFn = ($scope: IUploadImageScope, $element: ng.IAugmentedJQuery, attr: ng.IAttributes) => {
        $scope.imageUploaded = false;

        $scope.isReadonly = $scope.$parent["$ctrl"].isReadonly;

        if (!!$scope.systemTaskModel && !!$scope.systemTaskModel.associatedImageUrl) {

            this.createImage($scope, $element, attr);
        }

        const uploadImageCntr = $element.find("input");
        $scope.uploadImage = () => {
            if (!$scope.isReadonly) {
                const fileInput = uploadImageCntr[0];
                fileInput.click();
            }
        };

        $scope.downloadImage = () => {
            if ($scope.systemTaskModel.associatedImageUrl) {
                this.downloadService.downloadFile($scope.systemTaskModel.associatedImageUrl);
            }
        };

        $scope.clearImage = () => {
            if (!$scope.isReadonly) {
                const fileInput = uploadImageCntr;
                fileInput.val("");
                this.clearImageContainer($scope, $element, attr);
                this.toggleButtons($scope, $element, false);
                if (!!$scope.systemTaskModel.associatedImageUrl) {
                    $scope.systemTaskModel.associatedImageUrl = null;
                }
                if (!!$scope.systemTaskModel.imageId) {
                    $scope.systemTaskModel.imageId = null;
                }
            }
        };

        $scope.fileChanged = (files: File[], callback?: Function) => {
            $scope.typeError = false;
            $scope.sizeError = false;
            const dataFile = files[0];
            if (dataFile) { //datafile is defined only if the user selects a file, on delete it is null
                let type = (dataFile.type || "").toLowerCase();
                if (type.indexOf("/") > -1) {
                    type = type.split("/")[1];
                }
                if (type !== "jpeg" && type !== "jpg" && type !== "png") {
                    $scope.typeError = true;
                    $scope.$digest();
                    return;
                }
                if (dataFile.size > 2 * 1024 * 1024) {//2 MegaBytes
                    $scope.sizeError = true;
                    $scope.$digest();
                    return;
                }
                // Create new file in filestore as a temporary file with expirary 1 day.
                const expirationDate = new Date();
                expirationDate.setDate(expirationDate.getDate() + 1);
                this.fileUploadService.uploadToFileStore(dataFile, expirationDate).then((result: IFileResult) => {
                        $scope.systemTaskModel.associatedImageUrl = result.uriToFile;
                        $scope.systemTaskModel.imageId = result.guid;
                        this.createImage($scope, $element, attr);
                    },
                    (error: any) => {
                        this.messageService.addError(error.message);
                    }).finally(() => {
                        if (callback) {
                            callback();
                        }
                    });
            }
        };
    };

    private createImage($scope: IUploadImageScope, $element: ng.IAugmentedJQuery, attr: ng.IAttributes) {

        this.clearImageContainer($scope, $element, attr);
        // forcing reload by adding query parameter http://stackoverflow.com/questions/18845298/forcing-a-ng-src-reload
        let imageUrl = $scope.systemTaskModel.associatedImageUrl;
        let decacheValue = "decache=" + Math.random();
        //if query parameter already exists in the image then append the decache value to query parameter
        //else create query parameter
        if (imageUrl.indexOf("?") > 0) {
            imageUrl += "&" + decacheValue;
        } else {
            //add request for latest version
            imageUrl += "?revisionId=2147483647&" + decacheValue;
        }

        const imageAlt = $scope.systemTaskModel.action ? $scope.systemTaskModel.action.replace(/"/g, "'") : "";

        const zoomableImage = "<zoomable-image id=\"uploadedImage\" class=\"img-responsive preview-image-placeholder\"" +
            "enable-zoom=\"" + !!$scope.systemTaskModel.associatedImageUrl + "\"" +
            "image-src=\"" + imageUrl + "\"" +
            "image-alt=\"" + imageAlt + "\" ></zoomable-image>";

        const el = this.$compile(zoomableImage)($scope);
        const result = document.getElementsByClassName("file-upload_preview");
        const wrappedResult = angular.element(result);
        wrappedResult.append(el);

        this.toggleButtons($scope, $element, true);
    }

    private clearImageContainer($scope: IUploadImageScope, $element: ng.IAugmentedJQuery, attr: ng.IAttributes) {
        const result = document.getElementsByClassName("file-upload_preview");
        const wrappedResult = angular.element(result);
        wrappedResult.empty();
    }

    private toggleButtons($scope: IUploadImageScope, $element: ng.IAugmentedJQuery, imageUploaded: boolean) {
        $scope.imageUploaded = imageUploaded;
        $element.find("#upload-image-btn").text(imageUploaded ? "Change" : "Upload");
    }

    public template: string = require("./upload-image-template.html");
}
