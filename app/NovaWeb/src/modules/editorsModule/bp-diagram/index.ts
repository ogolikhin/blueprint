import * as angular from "angular";
require("script!mxClient");
import {BPDiagram} from "./bp-diagram.component";
import {StencilService} from "./impl/stencil.svc";

angular.module("bp.editors.diagram", [])
    .service("stencilService", StencilService)
    .value("mxUtils", mxUtils)
    .component("bpDiagram", new BPDiagram());
