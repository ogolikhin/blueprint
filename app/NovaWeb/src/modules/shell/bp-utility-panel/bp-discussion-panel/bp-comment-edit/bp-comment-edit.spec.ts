﻿import "../../../";
import "angular";
import "angular-mocks";
import "angular-sanitize";
import { ComponentTest } from "../../../../util/component.test";
import { BPCommentEditController } from "./bp-comment-edit";
import { LocalizationServiceMock } from "../../../../core/localization/localization.mock";
import "angular-ui-tinymce";
import "tinymce";

describe("Component BPCommentEdit", () => {

    let vm: BPCommentEditController;

    beforeEach(angular.mock.module("app.shell"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
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

    it("callPostComment should call postCommentInternal ",
        inject(($rootScope: ng.IRootScopeService, $timeout: ng.ITimeoutService, $q: ng.IQService) => {
            //Arrange
            vm.postComment = () => { };
            vm.isWaiting = false;
            spyOn(vm, "postCommentInternal").and.callThrough();
            let formatter = {};
            let editor = {
                formatter: formatter
            };
            formatter["register"] = (a, b) => { };
            editor["focus"] = () => { };
            vm.tinymceOptions.init_instance_callback(editor);

            //Act
            vm.callPostComment();
            $timeout.flush();

            //Assert
            expect(vm.postCommentInternal).toHaveBeenCalled();
        }));

    it("tinymce init callback should call register",
        inject(($rootScope: ng.IRootScopeService, $timeout: ng.ITimeoutService, $q: ng.IQService) => {
            //Arrange
            let formatter = {};
            let editor = {
                formatter: formatter
            };
            formatter["register"] = (a, b) => { };
            editor["focus"] = () => { };
            spyOn(formatter, "register").and.callThrough();

            //Act
            vm.tinymceOptions.init_instance_callback(editor);

            //Assert
            expect(formatter["register"]).toHaveBeenCalled();
        }));

    it("tinymce setup should call addButton",
        inject(($rootScope: ng.IRootScopeService, $timeout: ng.ITimeoutService, $q: ng.IQService) => {
            //Arrange
            let addButton = (a, b) => { };
            let editor = {
                addButton: addButton
            };
            spyOn(editor, "addButton").and.callThrough();

            //Act
            vm.tinymceOptions.setup(editor);

            //Assert
            expect(editor.addButton).toHaveBeenCalled();
        }));

    it("tinymce setup should call apply after onclick call",
        inject(($rootScope: ng.IRootScopeService, $timeout: ng.ITimeoutService, $q: ng.IQService) => {
            //Arrange 
            interface IMenuItem {
                icon: string;
                text: string;
                onclick();
            };
            interface IMenuToolbar {
                type: string;
                text: string;
                icon: string;
                menu: IMenuItem[];
            };
            let menuItems: IMenuItem[];
            let addButton = (a: string, menuToolbar: IMenuToolbar) => {
                if (!menuItems) {
                    menuItems = menuToolbar.menu;
                } else {
                    menuItems.push.apply(menuItems, menuToolbar.menu);
                }
            };
            let apply = {};
            let formatter = {
                apply: apply
            };
            let editor = {
                addButton: addButton,
                formatter: formatter
            };
            editor.formatter.apply = () => { };
            spyOn(editor.formatter, "apply").and.callThrough();

            //Act
            vm.tinymceOptions.setup(editor);
            for (let i = 0; i < menuItems.length; i++) {
                menuItems[i].onclick();
            }

            //Assert
            expect(editor.formatter.apply).toHaveBeenCalled();
        }));
            
});