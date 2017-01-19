//3rd party(external) library dependencies used for this module
import "angular";
import "lodash";
//internal dependencies used for this module
import {LocalStorageService} from "./localStorage.service";

export const LocalStorage = angular.module("localStorage", [])
    .service("localStorageService", LocalStorageService)
    .name;
//export 'API' interfaces from this module so that we can access them elsewhere in the project
export {ILocalStorageService} from "./localStorage.service";
