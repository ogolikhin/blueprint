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
//import { ArtifactRelationshipsService } from "./relationships.svc";
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
      let template = `<bp-relationships-panel></bp-relationships-panel>`;
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
        inject(($rootScope: ng.IRootScopeService, statefulArtifactFactory: IStatefulArtifactFactory, artifactManager: IArtifactManager,
                $timeout:  ng.ITimeoutService) => {


            vm.item = statefulArtifactFactory.createStatefulArtifact({id: 22, name: "Artifact", prefix: "AC"});

            vm.selectedTraces = {};

            vm.selectedTraces[22] = [traces[0]];

            vm.setSelectedDirection(2);

            $timeout.flush();

            expect(vm.selectedTraces[22][0].traceDirection).toBe(2);
        }));

    it("should delete trace from artifact",
        inject(($httpBackend: ng.IHttpBackendService, $rootScope: ng.IRootScopeService,
                statefulArtifactFactory: IStatefulArtifactFactory, artifactManager: IArtifactManager,
                artifactRelationships: IArtifactRelationshipsService, $timeout:  ng.ITimeoutService) => {

            let artifact = statefulArtifactFactory.createStatefulArtifact({id: 22, name: "Artifact", prefix: "AC"});

            vm.item = artifact;

            let readerSpy = spyOn(vm.item.relationships, 'remove');
            vm.deleteTrace(traces[0]);
            $timeout.flush();
            expect(readerSpy).toHaveBeenCalled();

        }));

    it("should flag traces",
        inject(($httpBackend: ng.IHttpBackendService, $rootScope: ng.IRootScopeService,
                statefulArtifactFactory: IStatefulArtifactFactory, artifactManager: IArtifactManager,
                artifactRelationships: IArtifactRelationshipsService, $timeout:  ng.ITimeoutService) => {

            vm.item = statefulArtifactFactory.createStatefulArtifact({id: 22, name: "Artifact", prefix: "AC"});
            vm.selectedTraces = {};
            vm.selectedTraces[22] = traces;

            vm.toggleFlag();
            $timeout.flush();
            expect(vm.selectedTraces[22][0].suspect).toBe(true);
            expect(vm.selectedTraces[22][1].suspect).toBe(true);

        }));

    it("should delete selected traces from artifact",
        inject(($httpBackend: ng.IHttpBackendService, $rootScope: ng.IRootScopeService,
                statefulArtifactFactory: IStatefulArtifactFactory, artifactManager: IArtifactManager,
                artifactRelationships: IArtifactRelationshipsService, $timeout:  ng.ITimeoutService) => {



            let artifact = statefulArtifactFactory.createStatefulArtifact({id: 22, name: "Artifact", prefix: "AC"});

            vm.item = artifact;

            let readerSpy = spyOn(vm.item.relationships, 'remove');
            vm.deleteTraces(traces);
            $timeout.flush();
            expect(readerSpy).toHaveBeenCalled();

        }));
    // it("should not load data for artifact without Prefix",
    //     inject(($rootScope: ng.IRootScopeService, artifactManager: IArtifactManager, statefulArtifactFactory: IStatefulArtifactFactory) => {
    //
    //         //Arrange
    //         const artifact = statefulArtifactFactory.createStatefulArtifact({id: 22, name: "Artifact"});
    //
    //         //Act
    //         artifactManager.selection.setArtifact(artifact);
    //         $rootScope.$digest();
    //         const selectedArtifact = artifactManager.selection.getArtifact();
    //
    //         //Assert
    //         expect(selectedArtifact).toBeDefined();
    //         expect(vm.manualTraces).toBe(null);
    //         expect(vm.otherTraces).toBe(null);
    //     }));
});
