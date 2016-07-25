import "angular";
import { IMessageService } from "../../shell/";
import { Helper } from "../../core";
import { Models, IArtifactService } from "../../main";
import { BpGeneralEditor } from "./bp-general-editor";
import { BpArtifactEditor } from "./bp-artifact-editor";


angular.module("bp.editors.details", []) 
    .component("bpGeneralEditor", new BpGeneralEditor())
    .component("bpArtifactEditor", new BpArtifactEditor());


export {IArtifactService, IMessageService, Models, Helper}
