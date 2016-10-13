import "angular";
import {AuthenticationRequired} from "../authentication";

export class MainState extends AuthenticationRequired implements ng.ui.IState {
    public url = "/main";
    public template = "<bp-main-view></bp-main-view>";
}
