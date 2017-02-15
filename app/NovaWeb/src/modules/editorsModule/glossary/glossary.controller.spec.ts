import "./";
import "angular-mocks";
import "rx/dist/rx.lite";
import {LocalizationServiceMock} from "../../commonModule/localization/localization.service.mock";
import {MessageServiceMock} from "../../main/components/messages/message.mock";
import {ItemTypePredefined} from "../../main/models/itemTypePredefined.enum";
import {StatefulArtifact} from "../../managers/artifact-manager/artifact/artifact";
import {StatefulArtifactFactoryMock} from "../../managers/artifact-manager/artifact/artifact.factory.mock";
import {ArtifactServiceMock} from "../../managers/artifact-manager/artifact/artifact.svc.mock";
import {StatefulArtifactServices} from "../../managers/artifact-manager/services";
import {SelectionManagerMock} from "../../managers/selection-manager/selection-manager.mock";
import {ComponentTest} from "../../util/component.test";
import {BpGlossaryController} from "./glossary.controller";
import {GlossaryServiceMock} from "./glossary.service.mock";

describe("Component BP Glossary", () => {

    let componentTest: ComponentTest<BpGlossaryController>;
    let template = `<bp-glossary context="context"></bp-glossary>`;
    let vm: BpGlossaryController;
    let bindings = {};

    beforeEach(angular.mock.module("glossaryEditor"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("selectionManager", SelectionManagerMock);
        $provide.service("messageService", MessageServiceMock);
        $provide.service("artifactService", ArtifactServiceMock);
        $provide.service("statefulArtifactFactory", StatefulArtifactFactoryMock);
    }));

    beforeEach(inject((statefulArtifactFactory: StatefulArtifactFactoryMock,
        selectionManager: SelectionManagerMock,
        artifactService: ArtifactServiceMock,
        $q: ng.IQService) => {
        const services = new StatefulArtifactServices($q, null, null, null, null, null, artifactService, null, null, null, null, null, null, null);
        const artifact = new StatefulArtifact({id: 263, name: "Artifact 263", predefinedType: ItemTypePredefined.Process, version: 1}, services);
        spyOn(artifact, "lock").and.callFake(() => { return; });
        const terms = GlossaryServiceMock.getTerms();
        _.each(terms, value => {
            artifact.subArtifactCollection.add(statefulArtifactFactory.createStatefulSubArtifact(artifact, value));
        });
        selectionManager.setArtifact(artifact);
        componentTest = new ComponentTest<BpGlossaryController>(template, "bp-glossary");
        vm = componentTest.createComponent(bindings);
    }));

    afterEach(() => {
        vm = null;
    });

    it("should be visible by default", () => {
        //Assert
        expect(componentTest.element.find("table").length).toBe(1);
    });

    it("should display data for a provided artifact id", inject(() => {
        //Assert
        expect(vm.artifact.id).toBe(263);
        expect(vm.terms).toBeDefined();
        expect(vm.terms.length).toBe(4);
    }));

    it("should select a specified term", inject(($rootScope: ng.IRootScopeService, selectionManager: SelectionManagerMock) => {
        // pre-req
        expect(componentTest.element.find(".selected-term").length).toBe(0);


        // Act
        selectionManager.clearAll();
        vm.selectTerm(vm.artifact.subArtifactCollection.get(386));
        $rootScope.$digest();

        //Assert
        expect(componentTest.element.find(".selected-term").length).toBe(1);
    }));
});
