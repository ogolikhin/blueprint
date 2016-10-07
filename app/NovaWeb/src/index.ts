import "angular";
import "./modules/main/";
// load our default (non specific) css
import "./styles/screen.scss";


declare var appBootstrap: any;

(function() {
    if (appBootstrap.isSupportedVersion()) {
        appBootstrap.initApp();
    }
})();
