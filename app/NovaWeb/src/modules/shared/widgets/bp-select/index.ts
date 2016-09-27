import * as angular from "angular";
import { BPSelect } from "./bp-select";

angular.module("bp.widjets.select", [])
    .component("bpSelect", new BPSelect());

