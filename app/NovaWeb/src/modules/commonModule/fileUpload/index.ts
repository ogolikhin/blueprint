//3rd party(external) library dependencies used for this module
import "angular";
//internal dependencies used for this module
import {FileUploadService} from "./fileUpload.service";

export const FileUpload = angular.module("fileUpload", [])
    .service("fileUploadService", FileUploadService)
    .name;

//export 'API' interfaces from this module so that we can access them elsewhere in the project
export {
    ICopyImageResult,
    IFileUploadService,
    IFileResult
} from "./fileUpload.service"
