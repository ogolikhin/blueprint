import "../../";
import * as angular from "angular";
import "angular-mocks";
import "angular-sanitize";
import "rx/dist/rx.lite";
import "../../";
import { IProcessService } from "../../../editors/bp-process/services/process.svc";
import { ProcessServiceMock } from "../../../editors/bp-process/services/process.svc.mock";
import { ComponentTest } from "../../../util/component.test";
import { BPRelationshipsPanelController } from "./bp-relationships-panel";
import { LocalizationServiceMock } from "../../../core/localization/localization.mock";
import { ArtifactRelationshipsMock } from "./../../../managers/artifact-manager/relationships/relationships.svc.mock";
import { MessageServiceMock } from "../../../core/messages/message.mock";
import { SelectionManager } from "./../../../managers/selection-manager/selection-manager";
import { DialogServiceMock } from "../../../shared/widgets/bp-dialog/bp-dialog";
import {
    IArtifactManager,
    ArtifactManager,
    IArtifactRelationshipsService,
    IStatefulArtifactFactory,
    StatefulArtifactFactory,
    MetaDataService,
    ArtifactService,
    ArtifactAttachmentsService
} from "../../../managers/artifact-manager";

describe("Component BPRelationshipsPanel", () => {

      let directiveTest: ComponentTest<BPRelationshipsPanelController>;
      let vm: BPRelationshipsPanelController;
      let bpAccordionPanelController = {
          isActiveObservable: new Rx.BehaviorSubject<boolean>(true).asObservable()
      };
      let traces;

    beforeEach(angular.mock.module("app.shell"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("artifactRelationships", ArtifactRelationshipsMock);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("dialogService", DialogServiceMock);
        $provide.service("selectionManager", SelectionManager);
        $provide.service("messageService", MessageServiceMock);
        $provide.service("artifactService", ArtifactService);
        $provide.service("artifactManager", ArtifactManager);
        $provide.service("artifactAttachments", ArtifactAttachmentsService);
        $provide.service("metadataService", MetaDataService);
        $provide.service("statefulArtifactFactory", StatefulArtifactFactory);
        $provide.service("processService", ProcessServiceMock);
    }));

    beforeEach(inject(() => {
        let template = `<bp-relationships-panel></bp-relationships-panel>`;
        directiveTest = new ComponentTest<BPRelationshipsPanelController>(template, "bp-relationships-panel");
        vm = directiveTest.createComponentWithMockParent({}, "bpAccordionPanel", bpAccordionPanelController);

        traces = [{
            artifactId: 8,
            artifactName: "New Document 1",
            artifactTypePrefix: "DOC",
            hasAccess: true,
            isSelected: false,
            itemId: 8,
            itemLabel: null,
            itemName: "New Document 1",
            itemTypePrefix: "DOC",
            primitiveItemTypePredefined: 4110,
            projectId: 1,
            projectName: "1",
            suspect: false,
            traceDirection: 0,
            traceType: 2
        },
        {
            artifactId: 9,
            artifactName: "New Document 1",
            artifactTypePrefix: "DOC",
            hasAccess: true,
            isSelected: false,
            itemId: 8,
            itemLabel: null,
            itemName: "New Document 1",
            itemTypePrefix: "DOC",
            primitiveItemTypePredefined: 4110,
            projectId: 1,
            projectName: "1",
            suspect: false,
            traceDirection: 0,
            traceType: 2
        }];
    }));

    afterEach(() => {
        vm = null;
    });

    it("should load data for a selected artifact",
        inject(($rootScope: ng.IRootScopeService) => {
            expect(directiveTest.element.find(".filter-bar").length).toBe(0);
            expect(directiveTest.element.find(".empty-state").length).toBe(1);
        }));

    it("should load data for a selected artifact",
        inject(($rootScope: ng.IRootScopeService, processService: IProcessService, artifactManager: IArtifactManager,
                statefulArtifactFactory: IStatefulArtifactFactory) => {

        //Arrange
        const artifact = statefulArtifactFactory.createStatefulArtifact({id: 22, name: "Artifact", prefix: "AC"});

        //Act
        artifactManager.selection.setArtifact(artifact);
        $rootScope.$digest();
        const selectedArtifact = artifactManager.selection.getArtifact();

        //Assert
        expect(selectedArtifact).toBeDefined();
        expect(vm.manualTraces.length).toBe(2);
        expect(vm.otherTraces.length).toBe(3);
    }));

    it("should change direction of the selected artifact",
        inject(($rootScope: ng.IRootScopeService, statefulArtifactFactory: IStatefulArtifactFactory, artifactManager: IArtifactManager) => {

            //Arrange
            const artifact = statefulArtifactFactory.createStatefulArtifact({id: 22, name: "Artifact", prefix: "AC"});

            //Act
            artifactManager.selection.setArtifact(artifact);
            $rootScope.$digest();
            const selectedArtifact = artifactManager.selection.getArtifact();

            //Assert
            expect(selectedArtifact).toBeDefined();
            expect(vm.manualTraces.length).toBe(2);
            expect(vm.otherTraces.length).toBe(3);
        }));

    it("should load data for a selected artifacts",
        inject(($rootScope: ng.IRootScopeService, statefulArtifactFactory: IStatefulArtifactFactory, artifactManager: IArtifactManager) => {

            //Arrange
            vm.item = statefulArtifactFactory.createStatefulArtifact({id: 22, name: "Artifact", prefix: "AC"});
            vm.selectedTraces = {};
            vm.selectedTraces[22] = [traces[0]];

            //Act
            vm.setSelectedDirection(2);
            $rootScope.$digest();

            //Assert
            expect(vm.selectedTraces[22][0].traceDirection).toBe(2);
        }));

    it("should flag traces",
        inject(($httpBackend: ng.IHttpBackendService, $rootScope: ng.IRootScopeService,
                statefulArtifactFactory: IStatefulArtifactFactory, artifactManager: IArtifactManager,
                artifactRelationships: IArtifactRelationshipsService) => {

            //Arrange
            vm.item = statefulArtifactFactory.createStatefulArtifact({id: 22, name: "Artifact", prefix: "AC"});
            vm.selectedTraces = {};
            vm.selectedTraces[22] = traces;

            //Act
            vm.toggleFlag();
            $rootScope.$digest();

            //Assert
            expect(vm.selectedTraces[22][0].suspect).toBe(true);
            expect(vm.selectedTraces[22][1].suspect).toBe(true);

        }));

    it("should delete trace from artifact",
        inject(($httpBackend: ng.IHttpBackendService, $rootScope: ng.IRootScopeService,
                statefulArtifactFactory: IStatefulArtifactFactory, artifactManager: IArtifactManager,
                artifactRelationships: IArtifactRelationshipsService) => {

            //Arrange
            vm.item = statefulArtifactFactory.createStatefulArtifact({id: 22, name: "Artifact", prefix: "AC"});
            let readerSpy = spyOn(vm.item.relationships, "remove");

            //Act
            vm.deleteTrace(traces[0]);
            $rootScope.$digest();

            //Assert
            expect(readerSpy).toHaveBeenCalled();

        }));

    it("should delete selected traces from artifact",
        inject(($httpBackend: ng.IHttpBackendService, $rootScope: ng.IRootScopeService,
                statefulArtifactFactory: IStatefulArtifactFactory, artifactManager: IArtifactManager,
                artifactRelationships: IArtifactRelationshipsService) => {

            //Arrange
            vm.item = statefulArtifactFactory.createStatefulArtifact({id: 22, name: "Artifact", prefix: "AC"});
            vm.selectedTraces = {};
            vm.selectedTraces[22] = traces;
            let readerSpy = spyOn(vm.item.relationships, "remove");

            //Act
            vm.deleteTraces(traces);
            $rootScope.$digest();

            //Assert
            expect(readerSpy).toHaveBeenCalled();
        }));
});
