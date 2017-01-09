import * as angular from "angular";
import "angular-formly";
import "angular-formly-templates-bootstrap";
import "./bp-glossary";
import "./bp-artifact";
import "./bp-diagram";
import "./bp-process";
import "./bp-collection";
import "./unpublished";
import "./jobs";
import {ArtifactRoutes} from "./editors.router";
import {
    IPropertyDescriptor,
    IPropertyDescriptorBuilder,
    PropertyDescriptorBuilder
} from "./configuration/property-descriptor-builder";
import {ItemStateService} from "./item-state/item-state.svc";

angular.module("bp.editors", [
        "formly",
        "formlyBootstrap",
        "bp.editors.glossary",
        "bp.editors.details",
        "bp.editors.diagram",
        "bp.editors.process",
        "bp.editors.collection",
        "bp.editors.unpublished",
        "bp.editors.jobs"
    ])
    .service("propertyDescriptorBuilder", PropertyDescriptorBuilder)
    .service("itemStateService", ItemStateService)
    .config(ArtifactRoutes);

export {IPropertyDescriptor, IPropertyDescriptorBuilder}
export {formlyConfig} from "./configuration/formly-config";
