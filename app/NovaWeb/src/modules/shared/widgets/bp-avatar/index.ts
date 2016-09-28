import * as angular from "angular";
import { BPAvatar } from "./bp-avatar";

angular.module("bp.widjets.avatar", [])
    .component("bpAvatar", new BPAvatar());

