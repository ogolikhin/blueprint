import * as angular from "angular";
import { BPFileUpload } from "./bp-file-upload";
// import { BpFileUploadStatusController } from "./bp-file-upload-status/bp-file-upload-status";

angular.module("bp.widjets.fileupload", [])
    // .controller("artifactStateController", BpFileUploadStatusController)
    .directive("bpFileUpload", BPFileUpload.factory());

export {
    BPFileUpload
};