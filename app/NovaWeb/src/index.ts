import "angular";
import "./modules/main/main.module";
import {AppComponent} from "./modules/application/app.component";

// load our default (non specific) css
import "./styles/screen.scss";

angular.module("app", ["app.main"])
    .component("app", new AppComponent());

angular.bootstrap(document, ["app"], {
    strictDi: true
});