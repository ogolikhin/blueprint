import * as angular from "angular";

import { ManageTracesDialogController } from "./bp-manage-traces";
export { ManageTracesDialogController }
import { BPManageTracesItem } from "./bp-manage-traces-item";

angular.module("bp.components", [])
    .component("bpManageTracesItem", new BPManageTracesItem());
