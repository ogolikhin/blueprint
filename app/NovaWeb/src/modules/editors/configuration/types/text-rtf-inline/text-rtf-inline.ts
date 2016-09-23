import "angular";
import "angular-formly";
import { Helper } from "../../../../shared";

export class BPFieldTextRTFInline implements AngularFormly.ITypeOptions {
    public name: string = "bpFieldTextRTFInline";
    public template: string = require("./text-rtf-inline.template.html");
    public wrapper: string = "bpFieldLabel";
    public controller: ng.Injectable<ng.IControllerConstructor> = BpFieldTextRTFInlineController;

    constructor() {
    }
}

export class BpFieldTextRTFInlineController {
    static $inject: [string] = ["$scope"];

    constructor(private $scope: AngularFormly.ITemplateScope) {
        let to: AngularFormly.ITemplateOptions = {
            tinymceOption: { // this will go to ui-tinymce directive
                inline: true,
                plugins: "advlist autolink link image paste lists charmap print noneditable mention",
                init_instance_callback: function (editor) {
                    Helper.autoLinkURLText(editor.getBody());
                    editor.dom.setAttrib(editor.dom.select("a"), "data-mce-contenteditable", "false");
                    editor.dom.bind(editor.dom.select("a"), "click", function (e) {
                        let element = e.target as HTMLElement;
                        while (element && element.tagName.toUpperCase() !== "A") {
                            element = element.parentElement;
                        }
                        if (element && element.getAttribute("href")) {
                            window.open(element.getAttribute("href"), "_blank");
                        }
                    });
                },
                mentions: {} // an empty mentions is needed when including the mention plugin and not using it
            }
        };
        angular.merge($scope.to, to);
    }
}