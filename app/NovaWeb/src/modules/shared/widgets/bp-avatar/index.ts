import * as angular from "angular";
import { BPAvatar } from "./bp-avatar";

angular.module("bp.widgets.avatar", [])
    .component("bpAvatar", new BPAvatar());

