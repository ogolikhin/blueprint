import "angular";
import "./modules/main/main.module";

// load our default (non specific) css
import "./styles/screen.scss";

angular.module("app", ["app.main"]);

angular.bootstrap(document, ["app"], {
    strictDi: true
});