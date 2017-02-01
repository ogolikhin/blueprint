import * as angular from "angular";
require("script!mxClient");
import {BPDiagram} from "./diagram.component";
import {StencilService} from "./impl/stencil.service";

export const DiagramEditor = angular.module("diagramEditor", [])
    .service("stencilService", StencilService)
    .value("mxUtils", mxUtils)
    .component("bpDiagram", new BPDiagram())
    .name;
