import * as angular from "angular";
import { BPInfiniteScroll } from "./bp-infinite-scroll";

angular.module("bp.widgets.infinitescroll", [])
    .directive("bpInfiniteScroll", BPInfiniteScroll.factory());

