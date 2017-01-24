//3rd party(external) library dependencies used for this module
import "angular";
//internal dependencies used for this module
import {HttpErrorInterceptor} from "./httpErrorInterceptor.service";

export const HttpInterceptorModule = angular.module("httpInterceptor", [])
    .service("httpErrorInterceptor", HttpErrorInterceptor)
    .name;

//export 'API' interfaces from this module so that we can access them elsewhere in the project
export {IHttpInterceptorConfig} from "./httpErrorInterceptor.service";

