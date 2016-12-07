import "./";
import * as angular from "angular";
import "angular-mocks";
import "rx/dist/rx.lite";
import {ComponentTest} from "../../util/component.test";
import {LocalizationServiceMock} from "../../core/localization/localization.mock";
import {BpGlossaryController} from "./bp-glossary";
import {GlossaryServiceMock} from "./glossary.svc.mock";
import {MessageServiceMock} from "../../core/messages/message.mock";
import {SelectionManagerMock} from "../../managers/selection-manager/selection-manager.mock";
import {ArtifactManagerMock} from "../../managers/artifact-manager/artifact-manager.mock";
import {StatefulArtifactFactoryMock} from "../../managers/artifact-manager/artifact/artifact.factory.mock";
import {ArtifactServiceMock} from "../../managers/artifact-manager/artifact/artifact.svc.mock";
import {StatefulArtifactServices} from "../../managers/artifact-manager/services";
import {StatefulArtifact} from "../../managers/artifact-manager/artifact/artifact";
import {ItemTypePredefined} from "../../main/models/enums";
import {ISubArtifact} from "../../main/models/models";

describe("Component BP Glossary", () => {

    let componentTest: ComponentTest<BpGlossaryController>;
    let template = `<bp-glossary context="context"></bp-glossary>`;
    let vm: BpGlossaryController;
    let bindings = {};

    beforeEach(angular.mock.module("bp.editors.glossary"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("selectionManager", SelectionManagerMock);
        $provide.service("messageService", MessageServiceMock);
        $provide.service("artifactService", ArtifactServiceMock);
        $provide.service("artifactManager", ArtifactManagerMock);
        $provide.service("statefulArtifactFactory", StatefulArtifactFactoryMock);
    }));

    beforeEach(inject((artifactManager: ArtifactManagerMock,
        statefulArtifactFactory: StatefulArtifactFactoryMock,
        selectionManager: SelectionManagerMock,
        artifactService: ArtifactServiceMock,
        $q: ng.IQService) => {
        artifactManager.selection = selectionManager;
        const services = new StatefulArtifactServices($q, null, null, null, null, null, artifactService, null, null, null, null, null, null, null);
        const artifact = new StatefulArtifact({id: 263, name: "Artifact 263", predefinedType: ItemTypePredefined.Process, version: 1}, services);
        spyOn(artifact, "lock").and.callFake(() => { return; });
        const terms = GlossaryServiceMock.getTerms();
        _.each(terms, value => {
            artifact.subArtifactCollection.add(statefulArtifactFactory.createStatefulSubArtifact(artifact, value));
        });
        artifactManager.selection.setArtifact(artifact);
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

    it("should select a specified term", inject(($rootScope: ng.IRootScopeService, artifactManager: ArtifactManagerMock) => {
        // pre-req
        expect(componentTest.element.find(".selected-term").length).toBe(0);


        // Act
        artifactManager.selection.clearAll();
        vm.selectTerm(vm.artifact.subArtifactCollection.get(386));
        $rootScope.$digest();

        //Assert
        expect(componentTest.element.find(".selected-term").length).toBe(1);
    }));
});
