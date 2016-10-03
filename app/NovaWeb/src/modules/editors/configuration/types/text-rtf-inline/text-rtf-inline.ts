import * as angular from "angular";
import "angular-formly";
import { Helper } from "../../../../shared";

export class BPFieldTextRTFInline implements AngularFormly.ITypeOptions {
    public name: string = "bpFieldTextRTFInline";
    public template: string = require("./text-rtf-inline.template.html");
    public wrapper: string[] = ["bpFieldLabel", "bootstrapHasError"];
    public controller: ng.Injectable<ng.IControllerConstructor> = BpFieldTextRTFInlineController;

    constructor() {
    }
}

export class BpFieldTextRTFInlineController {
    static $inject: [string] = ["$scope"];

    constructor(private $scope: AngularFormly.ITemplateScope) {
        let to: AngularFormly.ITemplateOptions = {
            tinymceOption: { // this will go to ui-tinymce directive
                autoresize_bottom_margin: 0,
                inline: true,
                fixed_toolbar_container: ".tinymce-toolbar-" + $scope.options["key"],
                menubar: false,
                toolbar: "fontsize | bold italic underline | forecolor format | link table",
                statusbar: false,
                plugins: "paste textcolor table noneditable autolink link autoresize, contextmenu",
                contextmenu: "link image inserttable | cell row column deletetable",
                init_instance_callback: function (editor) {
                    editor.formatter.register("font8px", {
                        inline: "span",
                        styles: { "font-size": "8px" }
                    });
                    editor.formatter.register("font10px", {
                        inline: "span",
                        styles: { "font-size": "10px" }
                    });
                    editor.formatter.register("font12px", {
                        inline: "span",
                        styles: { "font-size": "12px" }
                    });
                    editor.formatter.register("font15px", {
                        inline: "span",
                        styles: { "font-size": "15px" }
                    });
                    editor.formatter.register("font18px", {
                        inline: "span",
                        styles: { "font-size": "18px" }
                    });

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
                setup: function (editor) {
                    editor.addButton("format", {
                        title: "Format",
                        type: "menubutton",
                        text: "",
                        icon: "format",
                        menu: [
                            { icon: "strikethrough", text: " Strikethrough", onclick: function () { tinymce.execCommand("strikethrough"); } },
                            { icon: "bullist", text: " Bulleted list", onclick: function () { tinymce.execCommand("InsertUnorderedList"); } },
                            { icon: "numlist", text: " Numeric list", onclick: function () { tinymce.execCommand("InsertOrderedList"); } },
                            { icon: "outdent", text: " Outdent", onclick: function () { tinymce.execCommand("Outdent"); } },
                            { icon: "indent", text: " Indent", onclick: function () { tinymce.execCommand("Indent"); } }
                        ]
                    });
                    editor.addButton("fontsize", {
                        type: "menubutton", // https://www.tinymce.com/docs/demo/custom-toolbar-menu-button/
                        text: "",
                        icon: "font-size",
                        menu: [
                            {
                                text: "8px",
                                onclick: function () {
                                    editor.formatter.apply("font8px");
                                }
                            },
                            {
                                text: "10px",
                                onclick: function () {
                                    editor.formatter.apply("font10px");
                                }
                            }, {
                                text: "12px",
                                onclick: function () {
                                    editor.formatter.apply("font12px");
                                }
                            }, {
                                text: "15px",
                                onclick: function () {
                                    editor.formatter.apply("font15px");
                                }
                            }, {
                                text: "18px",
                                onclick: function () {
                                    editor.formatter.apply("font18px");
                                }
                            }]
                    });
                }
            }
        };
        angular.merge($scope.to, to);
    }
}