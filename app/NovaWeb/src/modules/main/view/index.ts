import * as angular from "angular";
import { MainView } from "./view";

angular.module("bp.main.view", [])
    .component("bpMainView", new MainView());
