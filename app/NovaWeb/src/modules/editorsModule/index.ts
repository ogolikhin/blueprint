import "angular";

import "angular-formly";
import "angular-formly-templates-bootstrap";
import {GlossaryEditor} from "./glossary";
import {ArtifactEditors} from "./artifactEditor";
import {DiagramEditor} from "./diagram";
import {CollectionEditors} from "./collection";
import {BaselineEditors} from "./baseline";
import {UnpublishedEditor} from "./unpublished";
import {JobsEditor} from "./jobs";

import {ArtifactRoutes} from "./editors.router";
import {
    IPropertyDescriptor,
    IPropertyDescriptorBuilder,
    EditorServices
} from "./services";
import {ItemState} from "./itemState";

import {ProcessEditor} from "./bp-process";

angular.module("bp.editors", [
    "formly",
    "formlyBootstrap",
    GlossaryEditor,
    ArtifactEditors,
    EditorServices,
    ItemState,
    DiagramEditor,
    ProcessEditor,
    CollectionEditors,
    BaselineEditors,
    JobsEditor,
    UnpublishedEditor
])
    .config(ArtifactRoutes);

export {IPropertyDescriptor, IPropertyDescriptorBuilder}
export {formlyConfig} from "./configuration/formly-config";
