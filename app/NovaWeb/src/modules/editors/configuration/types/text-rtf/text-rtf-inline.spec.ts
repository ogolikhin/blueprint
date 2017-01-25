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
import {NavigationServiceMock} from "../../../../commonModule/navigation/navigation.service.mock";
import {INavigationService} from "../../../../commonModule/navigation/navigation.service";
import {ValidationServiceMock} from "../../../../managers/artifact-manager/validation/validation.mock";
import {DialogServiceMock} from "../../../../shared/widgets/bp-dialog/bp-dialog.mock";
import {IValidationService} from "../../../../managers/artifact-manager/validation/validation.svc";
import {ArtifactRelationshipsMock} from "../../../../managers/artifact-manager/relationships/relationships.svc.mock";
import {ArtifactServiceMock} from "../../../../managers/artifact-manager/artifact/artifact.svc.mock";
import {MessageServiceMock} from "../../../../main/components/messages/message.mock";

describe("Formly Text RTF Inline", () => {
    const fieldsDefinition = [
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

    const moduleName = createFormlyModule("formlyModuleTextRTFInline", [
        "ui.tinymce",
        "formly",
        "formlyBootstrap"
    ], fieldsDefinition);

    beforeEach(angular.mock.module(moduleName));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("navigationService", NavigationServiceMock);
        $provide.service("validationService", ValidationServiceMock);
        $provide.service("messageService", MessageServiceMock);
        $provide.service("dialogService", DialogServiceMock);
        $provide.service("selectionManager", () => ({
            getArtifact: () => undefined,
            getSubArtifact: () => undefined
        }));
        $provide.service("artifactService", ArtifactServiceMock);
        $provide.service("artifactRelationships", ArtifactRelationshipsMock);
    }));

    afterEach(() => {
        angular.element("body").empty();
    });

    const template = `<formly-dir model="model"></formly-dir>`;
    let compile, scope, rootScope, element, node, isolateScope, vm;

    let controller: BpFieldTextRTFInlineController;

    const tinymceBody = document.createElement("div");
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
    const editor = {
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
        undoManager: {
            clear: () => {
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
        getContent: () => {
            return tinymceBody.innerHTML;
        },
        setContent: (content?: string) => {
            return;
        },
        on: (eventName: string, callBack: Function) => {
            callBack.call(null);
        },
        destroy: (automatic?: boolean) => {
            return;
        }
    };

    beforeEach(
        inject(
            ($compile: ng.ICompileService,
             $rootScope: ng.IRootScopeService,
             $controller: ng.IControllerService,
             navigationService: INavigationService,
             validationService: IValidationService) => {
                rootScope = $rootScope;
                compile = $compile;
                scope = rootScope.$new();
                scope.model = {};
                scope.options = {};
                scope.to = {};
                scope.tinymceBody = tinymceBody;
                controller = $controller(BpFieldTextRTFInlineController, {
                    $scope: scope,
                    navigationService: navigationService,
                    validationService: validationService
                });
            }
        )
    );

    it("should be initialized properly", function () {
        // Arrange
        compileAndSetupStuff({model: {textRtf: ""}});

        const fieldNode = node.querySelectorAll(".formly-field-bpFieldTextRTFInline");
        const fieldScope = angular.element(fieldNode[0]).isolateScope();

        //Assert
        expect(fieldNode.length).toBe(2);
        expect(fieldNode[0]).toBeDefined();
        expect(fieldScope).toBeDefined();
    });

    it("tinyMCE init_instance_callback should call register, getBody, on", function () {
        // Arrange
        compileAndSetupStuff({model: {textRtf: ""}});

        const fieldNode = node.querySelectorAll(".formly-field-bpFieldTextRTFInline")[0];
        const fieldScope = angular.element(fieldNode).isolateScope();
        const to = fieldScope["to"];

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
        // Arrange
        compileAndSetupStuff({model: {textRtf: ""}});

        const fieldNode = node.querySelectorAll(".formly-field-bpFieldTextRTFInline")[0];
        const fieldScope = angular.element(fieldNode).isolateScope();
        const to = fieldScope["to"];

        spyOn(editor, "addButton").and.callThrough();

        //Act
        to.tinymceOptions.setup(editor);

        //Assert
        expect(editor.addButton).toHaveBeenCalled();
    });

    it("tinyMCE setup should call apply after onclick call", function () {
        // Arrange
        compileAndSetupStuff({model: {textRtf: ""}});

        const fieldNode = node.querySelectorAll(".formly-field-bpFieldTextRTFInline")[0];
        const fieldScope = angular.element(fieldNode).isolateScope();
        const to = fieldScope["to"];

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
        // Arrange
        compileAndSetupStuff({model: {textRtf: ""}});

        const fieldNode = node.querySelectorAll(".formly-field-bpFieldTextRTFInline")[0];
        const fieldScope = angular.element(fieldNode).isolateScope();
        const to = fieldScope["to"];
        const args = {
            content: tinymceBody.innerHTML
        };

        //Act
        to.tinymceOptions.paste_preprocess(null, args);
        const testDiv = document.createElement("div");
        testDiv.innerHTML = args.content;
        const p = testDiv.querySelectorAll("p");

        //Assert
        expect(p[0].style.fontFamily).not.toContain("monospace");
        expect(p[1].style.fontFamily).not.toContain("sans-serif");
        expect(p[1].style.fontFamily).not.toContain("serif");
    });

    it("tinyMCE paste_postprocess should run tags cleanup functions", function () {
        // Arrange
        compileAndSetupStuff({model: {textRtf: ""}});

        const fieldNode = node.querySelectorAll(".formly-field-bpFieldTextRTFInline")[0];
        const fieldScope = angular.element(fieldNode).isolateScope();
        const to = fieldScope["to"];
        const args = {
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
