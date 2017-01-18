import {LocalStorageService} from "./localStorage.service";

export const LocalStorage = angular.module("localStorage", [])
    .service("localStorageService", LocalStorageService)
    .name;

export {ILocalStorageService} from "./localStorage.service";
