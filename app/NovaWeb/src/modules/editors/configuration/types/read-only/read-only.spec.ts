import * as angular from "angular";
import "angular-mocks";
import "angular-messages";
import "angular-sanitize";
import "angular-ui-bootstrap";
import "angular-formly";
import "angular-formly-templates-bootstrap";
import { createFormlyModule } from "../../formly-config.mock";
import { PrimitiveType } from "../../../../main/models/enums";

describe("Formly ReadOnly", () => {
    let fieldsDefinition = [
        {
            type: "bpFieldReadOnly",
            key: "field",
            data: {
                primitiveType: PrimitiveType.Number
            }
        },
        {
            type: "bpFieldReadOnly",
            key: "readonlyDate",
            data: {
                primitiveType: PrimitiveType.Date
            }
        },
        {
            type: "bpFieldReadOnly",
            key: "readonlySelectMulti",
            templateOptions: {
                options: [
                    { value: 1, name: "Option 1" },
                    { value: 2, name: "Option 2" },
                    { value: 3, name: "Option 3" },
                    { value: 4, name: "Option 4" },
                    { value: 5, name: "Option 5" }
                ],
                optionsAttr: "bs-options"
            },
            data: {
                primitiveType: PrimitiveType.Choice,
                isMultipleAllowed: true
            }
        },
        {
            type: "bpFieldReadOnly",
            key: "readonlySelect",
            data: {
                primitiveType: PrimitiveType.Choice,
                validValues: [
                    { id: 1, value: "Option 1" },
                    { id: 2, value: "Option 2" },
                    { id: 3, value: "Option 3" },
                    { id: 4, value: "Option 4" },
                    { id: 5, value: "Option 5" }
                ]
            }
        },
        {
            type: "bpFieldReadOnly",
            key: "readonlyText",
            data: {
                primitiveType: PrimitiveType.Text
            }
        },
        {
            type: "bpFieldReadOnly",
            key: "readonlyTextMulti",
            data: {
                primitiveType: PrimitiveType.Text,
                isMultipleAllowed: true
            }
        },
        {
            type: "bpFieldReadOnly",
            key: "readonlyRichText",
            data: {
                primitiveType: PrimitiveType.Text,
                isRichText: true
            }
        },
        {
            type: "bpFieldReadOnly",
            key: "readonlyUser",
            data: {
                primitiveType: PrimitiveType.User
            }
        },
        {
            type: "bpFieldReadOnly",
            key: "readonlyInvalid",
            data: {
                primitiveType: -100000 // invalid type
            }
        }
    ];

    let moduleName = createFormlyModule("formlyModuleReadOnly", [
        "ngSanitize",
        "formly",
        "formlyBootstrap"
    ], fieldsDefinition);

    beforeEach(angular.mock.module(moduleName));

    afterEach(() => {
        angular.element("body").empty();
    });

    let template = `<formly-dir model="model"></formly-dir>`;
    let compile, scope, rootScope, element, node, isolateScope, vm;

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
        compileAndSetupStuff();

        let fieldNode = node.querySelector(".formly-field-bpFieldReadOnly");
        let fieldScope = angular.element(fieldNode).isolateScope();

        expect(fieldNode).toBeDefined();
        expect(fieldScope).toBeDefined();
    });

    it("should display read only number", function () {
        compileAndSetupStuff({model: {field: 10}});

        let fieldInput = node.querySelectorAll(".formly-field-bpFieldReadOnly div.read-only-input")[0];

        expect(fieldInput.innerHTML).toBe("10");
    });

    it("should display read only date", function () {
        compileAndSetupStuff({model: {readonlyDate: new Date("2016-08-08")}});

        let fieldInput = node.querySelectorAll(".formly-field-bpFieldReadOnly div.read-only-input")[1];

        expect(fieldInput.innerHTML).toContain("2016");
    });

    it("should display read only multichoice", function () {
        compileAndSetupStuff({model: {readonlySelectMulti: [1, 2]}});

        let fieldInput = node.querySelectorAll(".formly-field-bpFieldReadOnly div.read-only-input")[2];

        expect(fieldInput.children.length).toBe(2);
    });

    it("should display read only select", function () {
        compileAndSetupStuff({model: {readonlySelect: 5}});

        let fieldInput = node.querySelectorAll(".formly-field-bpFieldReadOnly div.read-only-input")[3];

        expect(fieldInput.innerHTML).toContain("Option 5");
    });

    it("should display read only select with custom values", function () {
        compileAndSetupStuff({model: {readonlySelect: {customValue: "Custom value"}}});

        let fieldInput = node.querySelectorAll(".formly-field-bpFieldReadOnly div.read-only-input")[3];

        expect(fieldInput.innerHTML).toContain("Custom value");
    });

    it("should display read only text", function () {
        compileAndSetupStuff({model: {readonlyText: "Lorem ipsum"}});

        let fieldInput = node.querySelectorAll(".formly-field-bpFieldReadOnly div.read-only-input")[4];

        expect(fieldInput.innerHTML).toBe("Lorem ipsum");
        expect(fieldInput.classList.contains("simple")).toBeTruthy();
        expect(fieldInput.classList.contains("multiple")).toBeFalsy();
        expect(fieldInput.classList.contains("richtext")).toBeFalsy();
    });

    it("should display read only multiline text", function () {
        compileAndSetupStuff({model: {readonlyTextMulti: "Lorem ipsum"}});

        let fieldInput = node.querySelectorAll(".formly-field-bpFieldReadOnly div.read-only-input")[5];

        expect(fieldInput.firstChild.innerHTML).toBe("Lorem ipsum");
        expect(fieldInput.classList.contains("simple")).toBeFalsy();
        expect(fieldInput.classList.contains("multiple")).toBeTruthy();
        expect(fieldInput.classList.contains("richtext")).toBeFalsy();
    });

    it("should display read only rich text", function () {
        compileAndSetupStuff({model: {readonlyRichText: "Lorem ipsum"}});

        let fieldInput = node.querySelectorAll(".formly-field-bpFieldReadOnly div.read-only-input")[6];

        expect(fieldInput.firstChild.innerHTML).toBe("Lorem ipsum");
        expect(fieldInput.classList.contains("simple")).toBeFalsy();
        expect(fieldInput.classList.contains("multiple")).toBeFalsy();
        expect(fieldInput.classList.contains("richtext")).toBeTruthy();
    });

    it("should display read only users", function () {
        compileAndSetupStuff({model: {readonlyUser: [
            {
                id: 1,
                displayName: "User"
            },
            {
                id: 1,
                displayName: "Group",
                isGroup: true
            }
        ]}});

        let fieldInput = node.querySelectorAll(".formly-field-bpFieldReadOnly div.read-only-input")[7];
        let content = fieldInput.innerHTML.split(", ");

        expect(content.length).toBe(2);
        expect(content[0]).toContain("User");
        expect(content[1]).toContain("Group");
    });

    it("should not display read only invalid type", function () {
        compileAndSetupStuff({model: {readonlyInvalid: "Invalid"}});

        let fields = document.querySelectorAll(".formly-field-bpFieldReadOnly div.read-only-input");

        expect(fields.length).toBe(fieldsDefinition.length - 1);
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