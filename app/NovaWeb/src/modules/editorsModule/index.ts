import * as angular from "angular";
import "angular-formly";
import "angular-formly-templates-bootstrap";
import "./bp-glossary";
import {ArtifactEditors} from "./artifact";
import "./bp-diagram";
import "./bp-process";
import "./bp-collection";
import "./unpublished";
import "./jobs";
import {ArtifactRoutes} from "./editors.router";
import {
    IPropertyDescriptor,
    IPropertyDescriptorBuilder,
    EditorServices
} from "./services";
import {ItemStateService} from "./item-state/item-state.svc";

angular.module("bp.editors", [
        "formly",
        "formlyBootstrap",
        "bp.editors.glossary",
        ArtifactEditors,
        EditorServices,
        "bp.editors.diagram",
        "bp.editors.process",
        "bp.editors.collection",
        "bp.editors.unpublished",
        "bp.editors.jobs"
    ])
    .service("itemStateService", ItemStateService)
    .config(ArtifactRoutes);

export {IPropertyDescriptor, IPropertyDescriptorBuilder}
export {formlyConfig} from "./configuration/formly-config";
