import * as angular from "angular";
import {BpArtifactInfo} from "./bp-artifact-info";

angular.module("bp.components.artifactinfo", [])
    .component("bpArtifactInfo", new BpArtifactInfo());

