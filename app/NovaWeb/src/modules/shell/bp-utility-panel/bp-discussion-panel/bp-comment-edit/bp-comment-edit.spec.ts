﻿import "../../../";
import "angular";
import "angular-mocks";
import "angular-sanitize";
import { ComponentTest } from "../../../../util/component.test";
import { BPCommentEditController } from "./bp-comment-edit";
import { LocalizationServiceMock } from "../../../../core/localization/localization.mock";

describe("Component BPCommentEdit", () => {

    beforeEach(angular.mock.module("app.shell"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("localization", LocalizationServiceMock);
    }));

    let directiveTest: ComponentTest<BPCommentEditController>;
    let template = `<bp-comment-edit 
            cancel-comment="null"
            add-button-text=''
            cancel-button-text=''
            comment-place-holder-text=''>
        </bp-comment-edit>`;

    beforeEach(() => {
        directiveTest = new ComponentTest<BPCommentEditController>(template, "bp-comment-edit");
    });

    it("should be visible by default", () => {

        //Arrange
        directiveTest.createComponent({});

        //Assert
        expect(directiveTest.element.find(".comment-edit-button-bar").length).toBe(1);
        expect(directiveTest.element.find("textarea").length).toBe(1);
    });
});