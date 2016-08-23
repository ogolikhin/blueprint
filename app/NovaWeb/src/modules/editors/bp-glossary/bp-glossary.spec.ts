import "./";
import "angular";
import "angular-mocks";
import "Rx";
import { ComponentTest } from "../../util/component.test";
import { LocalizationServiceMock } from "../../core/localization/localization.mock";
import { BpGlossaryController } from "./bp-glossary";
import { GlossaryServiceMock } from "./glossary.svc.mock";
import { ISelectionManager, SelectionManager } from "./../../main/services/selection-manager";

describe("Component BP Glossary", () => {

    let componentTest: ComponentTest<BpGlossaryController>;
    let template = `<bp-glossary context="context"></bp-glossary>`;
    let vm: BpGlossaryController;
    let bindings = {
        context: {
            artifact: {
                id: 263
            }
        }
    };

    beforeEach(angular.mock.module("bp.editors.glossary"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("glossaryService", GlossaryServiceMock);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("selectionManager", SelectionManager);
    }));

    beforeEach(inject(() => {
        componentTest = new ComponentTest<BpGlossaryController>(template, "bp-glossary");
        vm = componentTest.createComponent(bindings);
    }));
    
    afterEach( () => {
        vm = null;
    });

    it("should be visible by default", () => {
        //Assert
        expect(componentTest.element.find("table").length).toBe(1);
    });

    it("should display data for a provided artifact id", inject(() => {
       //Assert
       expect(vm.glossary.id).toBe(263);
       expect(vm.glossary.terms.length).toBe(4);
    }));

    it("should select a specified term", inject(($rootScope: ng.IRootScopeService, selectionManager: ISelectionManager) => {
       // pre-req
       expect(componentTest.element.find(".selected-term").length).toBe(0);
       

       // Act
       selectionManager.clearSelection();
       vm.selectTerm(vm.glossary.terms[2]);
       $rootScope.$digest();

       //Assert
       expect(componentTest.element.find(".selected-term").length).toBe(1);
    }));
});
