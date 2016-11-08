import {BpArtifactInfo} from "./bp-artifact-info";
import {BPManageTracesItem} from "../dialogs/bp-manage-traces/bp-manage-traces-item";

angular.module("bp.components.artifactinfo", [])
    .component("bpArtifactInfo", new BpArtifactInfo())
    .component("bpManageTracesItem", new BPManageTracesItem());

