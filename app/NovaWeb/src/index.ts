import "angular";
import "./modules/main/main.module";
import {ExecutionEnvironmentDetector} from "./modules/shell/execution-environment/execution-environment-detector";

// load our default (non specific) css
import "./styles/screen.scss";

function initApp() {
    angular.module("app", ["app.main"]);

    angular.bootstrap(document, ["app"], {
        strictDi: true
    });
}

window.onload = () => {
    var executionEnvironmentDetector = new ExecutionEnvironmentDetector();

    if (executionEnvironmentDetector.isSupportedVersion()) {
        initApp();
        return;
    }

    var xhr = new XMLHttpRequest();
    xhr.onreadystatechange = function () {
        if (xhr.readyState === 4) {

            var divUnsupportedBrowser = document.createElement("div");
            divUnsupportedBrowser.id = "unsupported-browser-container";
            divUnsupportedBrowser.innerHTML = xhr.responseText;
            document.body.insertBefore(divUnsupportedBrowser, document.body.firstChild);

            var innerScript = divUnsupportedBrowser.getElementsByTagName("script");
            for(var i = 0; i < innerScript.length;i++)
            {
                eval(innerScript[i].text);
            }
        }
    };
    xhr.open('GET', '/novaweb/static/unsupported-browser.html');
    xhr.send();
};