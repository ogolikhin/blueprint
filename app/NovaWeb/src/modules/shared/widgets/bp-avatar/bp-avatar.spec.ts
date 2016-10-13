import * as angular from "angular";
import "angular-mocks";
//import "angular-sanitize";
import "../../"; // app.shared
import {ComponentTest} from "../../../util/component.test";
import {BPAvatarController} from "./bp-avatar";

describe("Component BPAvatar", () => {
    beforeEach(angular.mock.module("app.shared"));

    let directiveTest: ComponentTest<BPAvatarController>;
    let bindings: any;
    let template = `
        <bp-avatar 
            icon="" 
            name="{{ displayName }}"
            color-base="{{ userId + displayName }}">
        </bp-avatar>
    `;

    beforeEach(() => {
        directiveTest = new ComponentTest<BPAvatarController>(template, "bpAvatar");
    });

    it("should be visible by default", () => {
        // Arrange
        bindings = {
            displayName: "admin",
            userId: 1
        };

        // Act
        // Act
        let ctrl: BPAvatarController = directiveTest.createComponent(bindings);

        //Assert
        expect(directiveTest.element.find(".avatar-placeholder").length).toBe(1);
        expect(ctrl.initials).toBe("A");
    });

    it("should display 2 initials for two names", () => {
        // Arrange
        bindings = {
            displayName: "Admin Person",
            userId: 1
        };

        // Act
        let ctrl: BPAvatarController = directiveTest.createComponent(bindings);

        //Assert
        expect(ctrl.initials).toBe("AP");
    });

    it("should display 2 initials for more than 2 names", () => {
        // Arrange
        bindings = {
            displayName: "Admin Middle Second Person",
            userId: 1
        };

        // Act
        let ctrl: BPAvatarController = directiveTest.createComponent(bindings);

        //Assert
        expect(ctrl.initials).toBe("AP");
    });
});
