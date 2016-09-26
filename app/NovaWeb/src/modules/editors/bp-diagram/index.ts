import * as angular from "angular";
require("script!mxClient");
import { BPDiagram } from "./bp-diagram.component";
import { DiagramService } from "./diagram.svc";
import { StencilService } from "./impl/stencil.svc";

angular.module("bp.editors.diagram", [])
    .service("diagramService", DiagramService)
    .service("stencilService", StencilService)
    .value("mxUtils", mxUtils)
    .component("bpDiagram", new BPDiagram());
