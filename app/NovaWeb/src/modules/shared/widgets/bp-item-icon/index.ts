import * as angular from "angular";
import {BPItemTypeIconComponent} from "./bp-item-icon";

angular.module("bp.widgets.itemicon", [])
    .component("bpItemTypeIcon", new BPItemTypeIconComponent());

