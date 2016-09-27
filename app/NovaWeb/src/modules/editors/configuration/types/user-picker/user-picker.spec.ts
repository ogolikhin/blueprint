import * as angular from "angular";
import "angular-mocks";
import "angular-messages";
import "angular-sanitize";
import "angular-ui-bootstrap";
import "ui-select";
import "angular-formly";
import "angular-formly-templates-bootstrap";
import { UsersAndGroupsServiceMock, UserOrGroupInfo } from "../../../../core/services/users-and-groups.svc.mock";
import { createFormlyModule } from "../../formly-config.mock";

describe("Formly User Picker", () => {
    let fieldsDefinition = [
        {
            type: "bpFieldUserPicker",
            key: "userPicker",
            templateOptions: {
                options: [
                    { value: {
                        id: 1,
                        displayName: "User 1"
                    }, name: "User 1" },
                    { value: {
                        id: 2,
                        displayName: "User 2"
                    }, name: "User 2" },
                    { value: {
                        id: 1,
                        isGroup: true,
                        displayName: "Group 1"
                    }, name: "Group 1" },
                    { value: {
                        id: 2,
                        isGroup: true,
                        displayName: "Group 2"
                    }, name: "Group 2" }
                ],
                optionsAttr: "bs-options",
                required: true
            }
        },
        {
            type: "bpFieldUserPicker",
            key: "userPickerNotVal",
            templateOptions: {
                options: [
                    { value: {
                        id: 10,
                        displayName: "User 10"
                    }, name: "User 10" },
                    { value: {
                        id: 20,
                        displayName: "User 20"
                    }, name: "User 20" },
                    { value: {
                        id: 10,
                        isGroup: true,
                        displayName: "Group 10"
                    }, name: "Group 10" },
                    { value: {
                        id: 20,
                        isGroup: true,
                        displayName: "Group 20"
                    }, name: "Group 20" }
                ],
                optionsAttr: "bs-options"
            }
        }
    ];

    let moduleName = createFormlyModule("formlyModuleUserPicker", [
        "ngSanitize",
        "ui.select",
        "formly",
        "formlyBootstrap"
    ], fieldsDefinition);

    beforeEach(angular.mock.module(moduleName));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("usersAndGroupsService", UsersAndGroupsServiceMock);
    }));

    afterEach(() => {
        angular.element("body").empty();
    });

    let template = `<formly-dir model="model"></formly-dir>`;
    let compile, scope, rootScope, element, node, isolateScope, vm;

    beforeEach(
        inject((
            $compile: ng.ICompileService,
            $rootScope: ng.IRootScopeService
            ) => {
                rootScope = $rootScope;
                compile = $compile;
                scope = rootScope.$new();
                scope.model = {};
            }
        )
    );

    it("should be initialized properly", function () {
        compileAndSetupStuff({model: {userPicker: ""}});

        let fieldNode = node.querySelectorAll(".formly-field-bpFieldUserPicker");
        let fieldScope = angular.element(fieldNode[0]).isolateScope();
        fieldNode[0].querySelector(".ui-select-container > div").dispatchEvent(new Event("click", { "bubbles": true }));

        expect(fieldNode.length).toBe(2);
        expect(fieldNode[0]).toBeDefined();
        expect(fieldScope).toBeDefined();
        expect(document.activeElement.tagName.toUpperCase()).toBe("INPUT");
        expect(document.activeElement.classList.contains("ui-select-search")).toBeTruthy();
    });

    it("should fail if empty", function () {
        compileAndSetupStuff({model: {userPicker: []}});

        let fieldNode = node.querySelectorAll(".formly-field-bpFieldUserPicker")[0];
        let fieldScope = angular.element(fieldNode).isolateScope();

        expect((<any>fieldScope).fc.$valid).toBeFalsy();
        expect((<any>fieldScope).fc.$invalid).toBeTruthy();
        expect((<any>fieldScope).fc.$error.requiredCustom).toBeTruthy();
    });

    it("should succeed with values", function () {
        compileAndSetupStuff({model: {userPicker: [{
            id: 1,
            displayName: "User 1"
        }, {
            id: 2,
            isGroup: true,
            displayName: "Group 2"
        }]}});

        let fieldNode = node.querySelectorAll(".formly-field-bpFieldUserPicker")[0];
        let fieldScope = angular.element(fieldNode).isolateScope();
        let items = fieldNode.querySelectorAll(".ui-select-match-item-chosen");

        expect(items.length).toBe(2);
        expect(items[0].innerHTML).toContain("User 1");
        expect(items[1].innerHTML).toContain("Group 2");
        expect((<any>fieldScope).fc.$valid).toBeTruthy();
        expect((<any>fieldScope).fc.$invalid).toBeFalsy();
        expect((<any>fieldScope).fc.$error.requiredCustom).toBeUndefined();
    });

    it("should succeed if empty, as not required", function () {
        compileAndSetupStuff({model: {userPickerNotVal: []}});

        let fieldNode = node.querySelectorAll(".formly-field-bpFieldUserPicker")[1];
        let fieldScope = angular.element(fieldNode).isolateScope();

        expect((<any>fieldScope).fc.$valid).toBeTruthy();
        expect((<any>fieldScope).fc.$invalid).toBeFalsy();
        expect((<any>fieldScope).fc.$error.requiredCustom).toBeUndefined();
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