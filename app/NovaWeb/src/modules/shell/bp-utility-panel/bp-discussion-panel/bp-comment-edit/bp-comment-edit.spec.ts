﻿import "../../../";
import * as angular from "angular";
import "angular-mocks";
import "angular-sanitize";
import {ComponentTest} from "../../../../util/component.test";
import {BPCommentEditController} from "./bp-comment-edit";
import {LocalizationServiceMock} from "../../../../commonModule/localization/localization.service.mock";
import "angular-ui-tinymce";
import "tinymce";
import {UsersAndGroupsService} from "../../../../commonModule/services/usersAndGroups.service";

describe("Component BPCommentEdit", () => {

    let vm: BPCommentEditController;

    beforeEach(angular.mock.module("app.shell"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("usersAndGroupsService", UsersAndGroupsService);
        $provide.service("localization", LocalizationServiceMock);
    }));

    let directiveTest: ComponentTest<BPCommentEditController>;
    let template = `<bp-comment-edit
                        cancel-comment="null"
                        add-button-text=''
                        cancel-button-text=''
                        comment-place-holder-text=''
                        comment-text='test comment'>
                    </bp-comment-edit>`;

    beforeEach(() => {
        directiveTest = new ComponentTest<BPCommentEditController>(template, "bp-comment-edit");
        vm = directiveTest.createComponent({});
    });

    it("should be visible by default", () => {

        //Arrange
        directiveTest.createComponent({});

        //Assert
        expect(directiveTest.element.find(".comment-edit-button-bar").length).toBe(1);
        expect(directiveTest.element.find("textarea").length).toBe(1);
    });

    it("callPostComment should finally make isWaiting false",
        inject(($timeout: ng.ITimeoutService, $q: ng.IQService) => {
            //Arrange
            vm.postComment = () => undefined;
            vm.isWaiting = false;
            const editor = {
                getContent: () => {
                    return "";
                },
                focus: () => {
                    return;
                },
                formatter: {
                    register: (a, b) => {
                        return;
                    }
                }
            };
            vm.tinymceOptions.init_instance_callback(editor);
            vm.postComment = (): ng.IPromise<any> => {
                const defer = $q.defer();
                let result = true;
                defer.resolve(result);
                return defer.promise;
            };

            //Act
            vm.callPostComment();
            $timeout.flush();

            //Assert
            expect(vm.isWaiting).toBe(false);
        }));

    it("tinymce init callback should call register",
        () => {
            //Arrange
            let formatter = {};
            let editor = {
                formatter: formatter
            };
            formatter["register"] = (a, b) => undefined;
            editor["focus"] = () => undefined;
            spyOn(formatter, "register").and.callThrough();

            //Act
            vm.tinymceOptions.init_instance_callback(editor);

            //Assert
            expect(formatter["register"]).toHaveBeenCalled();
        });

    it("tinymce setup should call addButton",
        () => {
            //Arrange
            let addButton = (a, b) => undefined;
            let editor = {
                addButton: addButton
            };
            spyOn(editor, "addButton").and.callThrough();

            //Act
            vm.tinymceOptions.setup(editor);

            //Assert
            expect(editor.addButton).toHaveBeenCalled();
        });

    it("tinymce setup should call apply after onclick call",
        () => {
            //Arrange
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
            let addButton = (a: string, menuToolbar: IMenuToolbar) => {
                if (!menuItems) {
                    menuItems = menuToolbar.menu;
                } else {
                    menuItems.push.apply(menuItems, menuToolbar.menu);
                }
            };
            let apply = {};
            let execCommand = {};
            let formatter = {
                apply: apply
            };
            let editorCommands = {
                execCommand: execCommand
            };
            let editor = {
                addButton: addButton,
                formatter: formatter,
                editorCommands: editorCommands
            };
            editor.formatter.apply = () => undefined;
            editor.editorCommands.execCommand = (command: string) => undefined;
            spyOn(editor.formatter, "apply").and.callThrough();

            //Act
            vm.tinymceOptions.setup(editor);
            for (let i = 0; i < menuItems.length; i++) {
                if (menuItems[i].text && menuItems[i].text !== "-") { // skip the menu separator
                    menuItems[i].onclick();
                }
            }

            //Assert
            expect(editor.formatter.apply).toHaveBeenCalled();
        });

});
