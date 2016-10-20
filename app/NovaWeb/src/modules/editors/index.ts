import * as angular from "angular";
import "angular-formly";
import "angular-formly-templates-bootstrap";
import "./bp-glossary";
import "./bp-artifact";
import "./bp-diagram";
import "./bp-process";
import "./bp-collection";
import {ArtifactRoutes} from "./editors.router";

angular.module("bp.editors", [
        "formly",
        "formlyBootstrap",
        "bp.editors.glossary",
        "bp.editors.details",
        "bp.editors.diagram",
        "bp.editors.process",
        "bp.editors.collection"
    ])
    .config(ArtifactRoutes);

export {formlyConfig} from "./configuration/formly-config"
