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
            console.log(xhr.responseText);
            //add the supported browser from a JSON obj and render the page

            var divUnsupportedBrowser = document.createElement("div");
            divUnsupportedBrowser.id = "unsupported-browser-container";
            divUnsupportedBrowser.innerHTML = xhr.responseText;
            document.body.insertBefore(divUnsupportedBrowser, document.body.firstChild);
        }
    };
    xhr.open('GET', '/novaweb/static/unsupported-browser.html');
    xhr.send();

    var text: string = "<div style='display: inline-block; width:150px; vertical-align:top;'></div>";
    //if (window["config"] != null && typeof (window["config"]) != "undefined") {
    //    text += "<div style='display: inline-block; width:600px'>" + window["config"].labels["Unsupported_Message"] + "<br/>" +
    //        window["config"].labels["Supported_Browsers_Message"] + ":";
    //}
    text +=
        `<ul class='browser-list'>
            <li>Windows 7 &amp; 8.x
               <ul>\
                <li>Internet Explorer 9 and newer</li>
                <li>Google Chrome 25 and newer</li>
                <li>Mozilla Firefox 19 and newer</li>
              </ul>
            </li>
            <li>Mac OS X (10.9 and newer)
              <ul>
                <li>Safari 7 and newer</li>
               </ul>
            </li>\
            <li>iOS (6.0 and newer) - Rapid Review Only
               <ul>
                 <li>Safari 7 and newer</li>
                 <li>Mobile Chrome 35 and newer</li>
               </ul>
            </li>
            <li>Android (4.0 and newer) - Rapid Review Only
               <ul>
                 <li>Mobile Chrome 35 and newer</li>
               </ul>
            </li>
          </ul>
          <br/><br/><input id='proceed' type='submit' value='Proceed Anyway'><br/>
          </div></div>`;

    var div = document.createElement("div");
    div.id = "unsupported";
    div.className = "unsupported";
    div.innerHTML = "<div>" + text;

    var sheet = document.createElement("style");
    var style = `.unsupported {position:absolute;position:fixed;z-index:111111;
            width:100%; height:100%; top:0px; left:0px;
            border-bottom:1px solid #A29330;
            background:#FDF2AB;
            text-align:left;
            font-family: Arial,Helvetica,sans-serif; color:#000; font-size: 12px;}
            .unsupported div { padding:5px 36px 5px 40px; }
            .unsupported a,.unsupported a:visited  {color:#E25600; text-decoration: underline;}
            #unsupportedclose { position: absolute; right: 6px; top:-2px; height: 20px; width: 12px; font-weight: bold;font-size:18px; padding:0; }`;
    document.body.insertBefore(div, document.body.firstChild);
    document.getElementsByTagName("head")[0].appendChild(sheet);
    try {
        sheet.innerText = style;
        sheet.innerHTML = style;
    }
    catch (e) {
        try {
            sheet["styleSheet"].cssText = style;
        }
        catch (e) {
            return;
        }
    }

    document.getElementById("proceed").onclick = () => {
        document.getElementById("unsupported").style.display = "none";
        document.getElementById("unsupported-browser-container").style.display = "none";
        // TODO: change that to
        // var element = document.getElementById("unsupported");
        // element.parentNode.removeChild(element);

        initApp();
        return false;
    };
    try {
        div.getElementsByTagName("a")[0].onclick = mouseEvent => {
            var e = mouseEvent || window.event;
            if (e.stopPropagation) e.stopPropagation();
            else e.cancelBubble = true;
            return true;
        };
    }
    catch (e) { }
};