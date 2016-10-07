import * as angular from "angular";
import "angular-mocks";
import "angular-messages";
import "angular-sanitize";
import "angular-ui-bootstrap";
import "angular-ui-tinymce";
import "angular-formly";
import "angular-formly-templates-bootstrap";
import "tinymce";
import { createFormlyModule } from "../../formly-config.mock";

describe("Formly Text RTF", () => {
    let fieldsDefinition = [
        {
            type: "bpFieldTextRTF",
            key: "textRtf",
            templateOptions: {
                required: true
            }
        },
        {
            type: "bpFieldTextRTF",
            key: "textRtfNotVal"
        }
    ];

    let moduleName = createFormlyModule("formlyModuleTextRTF", [
        "ui.tinymce",
        "formly",
        "formlyBootstrap"
    ], fieldsDefinition);

    beforeEach(angular.mock.module(moduleName));

    afterEach(() => {
        angular.element("body").empty();
    });

    let template = `<formly-dir model="model"></formly-dir>`;
    let compile, scope, rootScope, element, node, isolateScope, vm;

    let tinymceBody = document.createElement("div");
    tinymceBody.innerHTML = `<p>This is a normal text</p><p>This is BOLD</p><p>This is ITALIC</p><p></p>` +
        `<p>This is a link created as normal text: http://www.google.com</p>` +
        `<p>This is a <a href="http://www.yahoo.com">link created inside Silverlight</a></p>` +
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
            execCommand: (command: string) => { }
        },
        formatter: {
            apply: () => { },
            register: (a, b) => { }
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
        on: () => { }
    };

    beforeEach(
        inject(
            ($compile: ng.ICompileService, $rootScope: ng.IRootScopeService) => {
                rootScope = $rootScope;
                compile = $compile;
                scope = rootScope.$new();
                scope.model = {};
            }
        )
    );

    it("should be initialized properly", function () {
        compileAndSetupStuff({model: {textRtf: ""}});

        let fieldNode = node.querySelectorAll(".formly-field-bpFieldTextRTF");
        let fieldScope = angular.element(fieldNode[0]).isolateScope();

        expect(fieldNode.length).toBe(2);
        expect(fieldNode[0]).toBeDefined();
        expect(fieldScope).toBeDefined();
    });

    it("should fail if empty", function () {
        compileAndSetupStuff({model: {textRtf: ""}});

        let fieldNode = node.querySelectorAll(".formly-field-bpFieldTextRTF")[0];
        let fieldScope = angular.element(fieldNode).isolateScope();

        expect((<any>fieldScope).fc.$valid).toBeFalsy();
        expect((<any>fieldScope).fc.$invalid).toBeTruthy();
        expect((<any>fieldScope).fc.$error.required).toBeTruthy();
    });

    it("should succeed if empty, as not required", function () {
        compileAndSetupStuff({model: {textRtfNotVal: ""}});

        let fieldNode = node.querySelectorAll(".formly-field-bpFieldTextRTF")[1];
        let fieldScope = angular.element(fieldNode).isolateScope();

        expect((<any>fieldScope).fc.$valid).toBeTruthy();
        expect((<any>fieldScope).fc.$invalid).toBeFalsy();
        expect((<any>fieldScope).fc.$error.required).toBeUndefined();
        expect((<any>fieldScope).fc.$error.requiredCustom).toBeUndefined();
    });

    it("should fail if empty-like (empty HTML tags)", function () {
        compileAndSetupStuff({model: {textRtf: "<div><br> </div>"}});

        let fieldNode = node.querySelectorAll(".formly-field-bpFieldTextRTF")[0];
        let fieldScope = angular.element(fieldNode).isolateScope();

        expect((<any>fieldScope).fc.$valid).toBeFalsy();
        expect((<any>fieldScope).fc.$invalid).toBeTruthy();
        expect((<any>fieldScope).fc.$error.required).toBeUndefined();
        expect((<any>fieldScope).fc.$error.requiredCustom).toBeTruthy();
    });

    it("tinyMCE init_instance_callback should call register and getBody", function () {
        compileAndSetupStuff({model: {textRtf: ""}});

        let fieldNode = node.querySelectorAll(".formly-field-bpFieldTextRTF")[0];
        let fieldScope = angular.element(fieldNode).isolateScope();
        let to = fieldScope["to"];

        spyOn(editor.formatter, "register").and.callThrough();
        spyOn(editor, "getBody").and.callThrough();

        //Act
        to.tinymceOptions.init_instance_callback(editor);

        //Assert
        expect(editor.formatter.register).toHaveBeenCalled();
        expect(editor.getBody).toHaveBeenCalled();
    });

    it("tinyMCE setup should call addButton", function () {
        compileAndSetupStuff({model: {textRtf: ""}});

        let fieldNode = node.querySelectorAll(".formly-field-bpFieldTextRTF")[0];
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

        let fieldNode = node.querySelectorAll(".formly-field-bpFieldTextRTF")[0];
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

    // function changeBody() {
    //     tinymceBody.innerHTML = "<p>body has changed</p>";
    // }

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