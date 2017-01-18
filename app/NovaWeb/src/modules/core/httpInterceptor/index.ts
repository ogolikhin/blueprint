import {HttpErrorInterceptor} from "./httpErrorInterceptor.service";

export const HttpInterceptorModule = angular.module("httpInterceptor", [])
    .service("httpErrorInterceptor", HttpErrorInterceptor)
    .name;

export {IHttpInterceptorConfig} from "./httpErrorInterceptor.service";

