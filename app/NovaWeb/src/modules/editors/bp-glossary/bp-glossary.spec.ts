import "./";
import * as angular from "angular";
import "angular-mocks";
import "rx/dist/rx.lite";
import {ComponentTest} from "../../util/component.test";
import {LocalizationServiceMock} from "../../core/localization/localization.mock";
import {BpGlossaryController} from "./bp-glossary";
import {GlossaryServiceMock} from "./glossary.svc.mock";
import {SelectionManager} from "./../../managers/selection-manager/selection-manager";
import {MessageServiceMock} from "../../core/messages/message.mock";
import {SessionSvcMock} from "../../shell/login/mocks.spec";
import {
    IArtifactManager,
    ArtifactManager,
    StatefulArtifactFactory,
    MetaDataService,
    ArtifactService,
    ArtifactAttachmentsService,
    ArtifactRelationshipsService
}
    from "../../managers/artifact-manager";

xdescribe("Component BP Glossary", () => {

    let componentTest: ComponentTest<BpGlossaryController>;
    let template = `<bp-glossary context="context"></bp-glossary>`;
    let vm: BpGlossaryController;
    let bindings = {};

    beforeEach(angular.mock.module("bp.editors.glossary"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("glossaryService", GlossaryServiceMock);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("selectionManager", SelectionManager);
        $provide.service("messageService", MessageServiceMock);
        $provide.service("session", SessionSvcMock);
        $provide.service("selectionManager", SelectionManager);
        $provide.service("artifactService", ArtifactService);
        $provide.service("artifactManager", ArtifactManager);
        $provide.service("artifactAttachments", ArtifactAttachmentsService);
        $provide.service("metadataService", MetaDataService);
        $provide.service("artifactRelationships", ArtifactRelationshipsService);
        $provide.service("statefulArtifactFactory", StatefulArtifactFactory);
    }));

    beforeEach(inject((artifactManager: IArtifactManager, statefulArtifactFactory: StatefulArtifactFactory) => {
        const artifact = statefulArtifactFactory.createStatefulArtifact({id: 263});
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

    it("should select a specified term", inject(($rootScope: ng.IRootScopeService, artifactManager: IArtifactManager) => {
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
