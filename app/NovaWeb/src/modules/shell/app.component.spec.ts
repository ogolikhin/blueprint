import "angular";
import "angular-mocks";
import "angular-sanitize";
import "./index";
import {ComponentTest} from "../util/component.test";
import {AppComponent, AppController} from "./app.component";

describe("Component AppComponent", () => {

    beforeEach(angular.mock.module("app.shell"));

    var componentTest: ComponentTest<AppController>;

    beforeEach(() => {
        componentTest = new ComponentTest<AppController>("<app></app>", "AppComponent");
    });

    //TODO: Mock session and configvaluehelper
    //TODO: Mock window and check values

    describe("the component is created", () => {
        it("should be visible by default", () => {

            //Arrange
            var vm: AppController = componentTest.createComponent({});

            //Assert
            expect(vm.currentUser).toBe("me");
        });
    });
});