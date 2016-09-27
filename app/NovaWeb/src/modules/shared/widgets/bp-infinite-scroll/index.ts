import * as angular from "angular";
import { BPInfiniteScroll } from "./bp-infinite-scroll";

angular.module("bp.widjets.infinitescroll", [])
    .directive("bpInfiniteScroll", BPInfiniteScroll.factory());

