import * as angular from "angular";
import "angular-formly";
import "angular-formly-templates-bootstrap";
import {GlossaryEditor} from "./glossary";
import {ArtifactEditors} from "./artifact";
import {DiagramEditor} from "./diagram";
import "./bp-process";
import {CollectionEditors} from "./collection";
import {UnpublishedEditor} from "./unpublished";
import {JobsEditor} from "./jobs";
import {ArtifactRoutes} from "./editors.router";
import {
    IPropertyDescriptor,
    IPropertyDescriptorBuilder,
    EditorServices
} from "./services";
import {ItemState} from "./itemState";

angular.module("bp.editors", [
        "formly",
        "formlyBootstrap",
        GlossaryEditor,
        ArtifactEditors,
        EditorServices,
        ItemState,
        DiagramEditor,
        "bp.editors.process",
        CollectionEditors,
        JobsEditor,
        UnpublishedEditor
    ])
    .config(ArtifactRoutes);

export {IPropertyDescriptor, IPropertyDescriptorBuilder}
export {formlyConfig} from "./configuration/formly-config";
