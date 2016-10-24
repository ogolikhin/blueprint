import * as angular from "angular";
import "angular-mocks";
import "angular-messages";
import "angular-sanitize";
import "angular-ui-bootstrap";
import "angular-ui-tinymce";
import "angular-formly";
import "angular-formly-templates-bootstrap";
import "tinymce";
import {Helper} from "../../../../shared";
import {BpFieldTextRTFInlineController} from "./text-rtf-inline";
import {createFormlyModule} from "../../formly-config.mock";
import {NavigationServiceMock} from "../../../../core/navigation/navigation.svc.mock";

describe("Formly Text RTF Inline", () => {
    let fieldsDefinition = [
        {
            type: "bpFieldTextRTFInline",
            key: "textRtf",
            templateOptions: {
                required: true
            }
        },
        {
            type: "bpFieldTextRTFInline",
            key: "textRtfNotVal"
        }
    ];

    let moduleName = createFormlyModule("formlyModuleTextRTFInline", [
        "ui.tinymce",
        "formly",
        "formlyBootstrap"
    ], fieldsDefinition);

    beforeEach(angular.mock.module(moduleName));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("navigationService", NavigationServiceMock);
    }));

    afterEach(() => {
        angular.element("body").empty();
    });

    let template = `<formly-dir model="model"></formly-dir>`;
    let compile, scope, rootScope, element, node, isolateScope, vm;

    let controller: BpFieldTextRTFInlineController;

    let tinymceBody = document.createElement("div");
    tinymceBody.innerHTML = `<p style="font-family: 'Courier New', monospace">This is a normal text</p><p>This is BOLD</p><p>This is ITALIC</p><p></p>` +
        `<p style="font-family: 'Open Sans', sans-serif;">This is a link created as normal text: http://www.google.com</p>` +
        `<p style="font-family: 'Times New Roman', serif;">This is a <a href="http://www.yahoo.com">link created inside Silverlight</a></p>` +
        `<p>This is an inline trace <a linkassemblyqualifiedname="BluePrintSys.RC.Client.SL.RichText.RichTextArtifactLink,` +
        ` BluePrintSys.RC.Client.SL.RichText, Version=7.4.0.0, Culture=neutral, PublicKeyToken=null" canclick="True" isvalid="True"` +
        ` href="http://localhost:9801/?ArtifactId=365" target="_blank" artifactid="365">AC365: New Actor #365</a></p>`;

    interface IMenuItem {
        icon: string;
        text: string;
        onclick();
    }
    interface IMenuToolbar {
        type: string;
        text: string;
        icon: string;
        menu: IMenuItem[];
    }
    let menuItems: IMenuItem[];
    let editor = {
        editorCommands: {
            execCommand: (command: string) => {
                return;
            }
        },
        formatter: {
            apply: () => {
                return;
            },
            register: (a, b) => {
                return;
            }
        },
        addButton: (a: string, menuToolbar: IMenuToolbar) => {
            if (!menuItems) {
                menuItems = menuToolbar.menu;
            } else {
                menuItems.push.apply(menuItems, menuToolbar.menu);
            }
        },
        getBody: () => {
            return tinymceBody;
        },
        on: (eventName: string, callBack: Function) => {
            callBack.call(null);
        }
    };

    beforeEach(
        inject(
            ($compile: ng.ICompileService, $rootScope: ng.IRootScopeService, $controller: ng.IControllerService, navigationService: NavigationServiceMock) => {
                rootScope = $rootScope;
                compile = $compile;
                scope = rootScope.$new();
                scope.model = {};
                scope.options = {};
                scope.to = {};
                scope.tinymceBody = tinymceBody;
                controller = $controller(BpFieldTextRTFInlineController, {$scope: scope, navigationService: navigationService});
            }
        )
    );

    it("should be initialized properly", function () {
        compileAndSetupStuff({model: {textRtf: ""}});

        let fieldNode = node.querySelectorAll(".formly-field-bpFieldTextRTFInline");
        let fieldScope = angular.element(fieldNode[0]).isolateScope();

        expect(fieldNode.length).toBe(2);
        expect(fieldNode[0]).toBeDefined();
        expect(fieldScope).toBeDefined();
    });

    it("should fail if empty", function () {
        compileAndSetupStuff({model: {textRtf: ""}});

        let fieldNode = node.querySelectorAll(".formly-field-bpFieldTextRTFInline")[0];
        let fieldScope = angular.element(fieldNode).isolateScope();

        expect((<any>fieldScope).fc.$valid).toBeFalsy();
        expect((<any>fieldScope).fc.$invalid).toBeTruthy();
        expect((<any>fieldScope).fc.$error.required).toBeTruthy();
    });

    it("should succeed if empty, as not required", function () {
        compileAndSetupStuff({model: {textRtfNotVal: ""}});

        let fieldNode = node.querySelectorAll(".formly-field-bpFieldTextRTFInline")[1];
        let fieldScope = angular.element(fieldNode).isolateScope();

        expect((<any>fieldScope).fc.$valid).toBeTruthy();
        expect((<any>fieldScope).fc.$invalid).toBeFalsy();
        expect((<any>fieldScope).fc.$error.required).toBeUndefined();
        expect((<any>fieldScope).fc.$error.requiredCustom).toBeUndefined();
    });

    it("should fail if empty-like (empty HTML tags)", function () {
        compileAndSetupStuff({model: {textRtf: "<div><br> </div>"}});

        let fieldNode = node.querySelectorAll(".formly-field-bpFieldTextRTFInline")[0];
        let fieldScope = angular.element(fieldNode).isolateScope();

        expect((<any>fieldScope).fc.$valid).toBeFalsy();
        expect((<any>fieldScope).fc.$invalid).toBeTruthy();
        expect((<any>fieldScope).fc.$error.required).toBeUndefined();
        expect((<any>fieldScope).fc.$error.requiredCustom).toBeTruthy();
    });

    it("tinyMCE init_instance_callback should call register, getBody, on", function () {
        compileAndSetupStuff({model: {textRtf: ""}});

        let fieldNode = node.querySelectorAll(".formly-field-bpFieldTextRTFInline")[0];
        let fieldScope = angular.element(fieldNode).isolateScope();
        let to = fieldScope["to"];

        spyOn(editor.formatter, "register").and.callThrough();
        spyOn(editor, "getBody").and.callThrough();
        spyOn(editor, "on").and.callThrough();

        //Act
        to.tinymceOptions.init_instance_callback(editor);

        //Assert
        expect(editor.formatter.register).toHaveBeenCalled();
        expect(editor.getBody).toHaveBeenCalled();
        expect(editor.on).toHaveBeenCalled();
    });

    it("tinyMCE setup should call addButton", function () {
        compileAndSetupStuff({model: {textRtf: ""}});

        let fieldNode = node.querySelectorAll(".formly-field-bpFieldTextRTFInline")[0];
        let fieldScope = angular.element(fieldNode).isolateScope();
        let to = fieldScope["to"];

        spyOn(editor, "addButton").and.callThrough();

        //Act
        to.tinymceOptions.setup(editor);

        //Assert
        expect(editor.addButton).toHaveBeenCalled();
    });

    it("tinyMCE setup should call apply after onclick call", function () {
        compileAndSetupStuff({model: {textRtf: ""}});

        let fieldNode = node.querySelectorAll(".formly-field-bpFieldTextRTFInline")[0];
        let fieldScope = angular.element(fieldNode).isolateScope();
        let to = fieldScope["to"];

        spyOn(editor.formatter, "apply").and.callThrough();

        //Act
        to.tinymceOptions.setup(editor);
        for (let i = 0; i < menuItems.length; i++) {
            if (menuItems[i].text && menuItems[i].text !== "-") {
                menuItems[i].onclick();
            }
        }

        //Assert
        expect(editor.formatter.apply).toHaveBeenCalled();
    });

    it("tinyMCE paste_preprocess should remove generic font families", function () {
        compileAndSetupStuff({model: {textRtf: ""}});

        let fieldNode = node.querySelectorAll(".formly-field-bpFieldTextRTFInline")[0];
        let fieldScope = angular.element(fieldNode).isolateScope();
        let to = fieldScope["to"];
        let args = {
            content: tinymceBody.innerHTML
        };

        //Act
        to.tinymceOptions.paste_preprocess(null, args);
        let testDiv = document.createElement("div");
        testDiv.innerHTML = args.content;
        let p = testDiv.querySelectorAll("p");

        //Assert
        expect(p[0].style.fontFamily).not.toContain("monospace");
        expect(p[1].style.fontFamily).not.toContain("sans-serif");
        expect(p[1].style.fontFamily).not.toContain("serif");
    });

    it("tinyMCE paste_postprocess should run tags cleanup functions", function () {
        compileAndSetupStuff({model: {textRtf: ""}});

        let fieldNode = node.querySelectorAll(".formly-field-bpFieldTextRTFInline")[0];
        let fieldScope = angular.element(fieldNode).isolateScope();
        let to = fieldScope["to"];
        let args = {
            node: tinymceBody
        };
        spyOn(Helper, "autoLinkURLText").and.callThrough();
        spyOn(Helper, "setFontFamilyOrOpenSans").and.callThrough();

        //Act
        to.tinymceOptions.paste_postprocess(null, args);

        //Assert
        expect(Helper.autoLinkURLText).toHaveBeenCalled();
        expect(Helper.setFontFamilyOrOpenSans).toHaveBeenCalled();
    });

    function compileAndSetupStuff(extraScopeProps?) {
        angular.merge(scope, extraScopeProps);
        element = compile(template)(scope);
        angular.element("body").append(element);
        scope.$digest();
        rootScope.$apply();
        node = element[0];
        isolateScope = element.isolateScope();
        vm = isolateScope.vm;
    }
});
