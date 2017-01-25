import {DownloadService} from "./download.service";


export const Download = angular.module("download", [])
    .service("downloadService", DownloadService)
    .name;

//export 'API' interfaces from this module so that we can access them elsewhere in the project
export {
    IDownloadService
} from "./download.service"
