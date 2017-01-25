import * as angular from "angular";
import "angular-ui-bootstrap";
import {uibModalWindowConfig} from "./uibModalWindow/uibModalWindow.decorator";

angular.module("bp.decorators", ["ui.bootstrap"])
    .config(uibModalWindowConfig);
