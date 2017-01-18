import {FileUploadService} from "./fileUpload.service";

export const FileUpload = angular.module("fileUpload", [])
    .service("fileUploadService", FileUploadService)
    .name;

export {
    ICopyImageResult,
    IFileUploadService,
    IFileResult
} from "./fileUpload.service"
