import * as angular from "angular";
import {ArtifactStateController} from "./artifact.state";
import {DetailsStateController} from "./editor-states/details.state";
import {DiagramStateController} from "./editor-states/diagram.state";
import {GeneralStateController} from "./editor-states/general.state";
import {GlossaryStateController} from "./editor-states/glossary.state";
import {ProcessStateController} from "./editor-states/process.state";

// import {Routes} from "./router.config";

angular.module("bp.router", [])
    .controller("artifactStateController", ArtifactStateController)
    .controller("generalStateController", GeneralStateController)
    .controller("detailsStateController", DetailsStateController)
    .controller("diagramStateController", DiagramStateController)
    .controller("glossaryStateController", GlossaryStateController)
    .controller("processStateController", ProcessStateController);
    // .config(Routes);    