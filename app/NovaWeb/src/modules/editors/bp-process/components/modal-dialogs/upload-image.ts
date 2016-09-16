/// <reference path="clear-text.ts" />
import {IFileResult, IFileUploadService} from "../../../../core/file-upload/"
import {ISystemTask} from "../diagram/presentation/graph/models/"

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
        buttonsContainerEnabled: "=",
        imageUrl: "@",
        imageAlt: "@"
    }
    public restrict = "E";
    public defaultName = "default";

    constructor(private fileUploadService: IFileUploadService, private $window: ng.IWindowService, private $timeout: ng.ITimeoutService, private $compile: ng.ICompileService) {
    }
    public static factory(): ng.IDirectiveFactory {
        var directive: ng.IDirectiveFactory = (fileUploadService: IFileUploadService, $window: ng.IWindowService, $timeout: ng.ITimeoutService, $compile: ng.ICompileService) =>
            new UploadImageDirective(fileUploadService, $window, $timeout, $compile);
        directive.$inject = ["fileUploadService", "$window", "$timeout", "$compile"];
        return directive;
    }
    public link: ng.IDirectiveLinkFn = ($scope: IUploadImageScope, $element: ng.IAugmentedJQuery, attr: ng.IAttributes) => {
        $scope.imageUploaded = false;

        $scope.isReadonly = $scope.$parent["vm"].isReadonly;

        if (!!$scope.systemTaskModel &&
            !!$scope.systemTaskModel.associatedImageUrl) {

            this.createImage($scope, $element, attr);
        }

        var uploadImageCntr = $element.find("#upload-image");

        $scope.uploadImage = () => {
            var fileInput = uploadImageCntr;
            fileInput.click();
        }

        $scope.downloadImage = () => {
            if ($scope.systemTaskModel.associatedImageUrl) {
                this.$window.open($scope.systemTaskModel.associatedImageUrl, "_blank");
            }
        }

        $scope.clearImage = () => {
            var fileInput = uploadImageCntr;
            fileInput.val('');
            this.clearImageContainer($scope, $element, attr);
            this.toggleButtons($scope, $element, false);
            if (!!$scope.systemTaskModel.associatedImageUrl) {
                $scope.systemTaskModel.associatedImageUrl = null;
            }
            if (!!$scope.systemTaskModel.imageId) {
                $scope.systemTaskModel.imageId = null;
            }
        }

        $scope.fileChanged = (element: any) => {
            $scope.typeError = false;
            $scope.sizeError = false;
            var dataFile = element.files[0];
            if (dataFile) { //datafile is defined only if the user selects a file, on delete it is null
                var type = (dataFile.type || "").toLowerCase();
                if (type.indexOf("/") > -1) type = type.split('/')[1];
                if (type !== "jpeg" && type !== "jpg" && type !== 'png') {
                    $scope.typeError = true;
                    return;
                }
                if (dataFile.size > 2 * 1024 * 1024) {//2 MegaBytes
                    $scope.sizeError = true;
                    return;
                }
                // Create new file in filestore as a temporary file with expirary 1 day.
                var expirationDate = new Date();
                expirationDate.setDate(expirationDate.getDate() + 1);
                this.fileUploadService.uploadToFileStore(dataFile, expirationDate).then((result: IFileResult) => {
                    $scope.systemTaskModel.associatedImageUrl = result.uriToFile;
                    $scope.systemTaskModel.imageId = result.guid;
                    $scope.systemTaskModel.model.propertyValues["imageId"].value = result.guid;
                    this.createImage($scope, $element, attr);
                });
            }

        }
    };
    private createImage($scope: IUploadImageScope, $element: ng.IAugmentedJQuery, attr: ng.IAttributes) {

        // forcing reload by adding query parameter http://stackoverflow.com/questions/18845298/forcing-a-ng-src-reload
        $scope.imageUrl = $scope.systemTaskModel.associatedImageUrl;
        let decacheValue = "decache=" + Math.random();
        //if query parameter already exists in the image then append the decache value to query parameter
        //else create query parameter 
        if ($scope.imageUrl.indexOf("?") > 0) {
            $scope.imageUrl += "&" + decacheValue;
        } else {
            //add request for latest version
            $scope.imageUrl += "?revisionId=2147483647&" + decacheValue;
        }

        $scope.imageAlt = $scope.systemTaskModel.action ? $scope.systemTaskModel.action : "";
        $scope.imageAlt = $scope.imageAlt.replace(/"/g, "'");

        this.toggleButtons($scope, $element, true);
    }

    private clearImageContainer($scope: IUploadImageScope, $element: ng.IAugmentedJQuery, attr: ng.IAttributes) {
        //$("." + attr["imageContainerClass"]).empty();
        var result = document.getElementsByClassName("file-upload_preview");
        var wrappedResult = angular.element(result);
        wrappedResult.empty();
    }

    private toggleButtons($scope: IUploadImageScope, $element: ng.IAugmentedJQuery, imageUploaded: boolean) {
        $scope.imageUploaded = imageUploaded;
        $element.find("#upload-image-btn").text(imageUploaded ? "Change" : "Upload");
    }

    public template: string = require("./upload-image-template.html");
}
