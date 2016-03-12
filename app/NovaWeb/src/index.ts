import "angular";
import "./modules/shell";
import "./modules/main/main.module";

// load our default (non specific) css
import "./styles/screen.scss";

angular.module("app", ["app.shell","app.main"]);

angular.bootstrap(document, ["app"], {
    strictDi: true
});