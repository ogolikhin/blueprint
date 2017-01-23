import "angular-mocks";
import "angular-sanitize";
import "rx/dist/rx.lite";
import {LocalizationServiceMock} from "../../../core/localization/localization.service.mock";
import {NavigationServiceMock} from "../../../core/navigation/navigation.service.mock";
import {IProcessService} from "../../../editors/bp-process/services/process.svc";
import {ProcessServiceMock} from "../../../editors/bp-process/services/process.svc.mock";
import {PropertyDescriptorBuilderMock} from "../../../editors/configuration/property-descriptor-builder.mock";
import {UnpublishedArtifactsServiceMock} from "../../../editors/unpublished/unpublished.svc.mock";
import {
    ArtifactAttachmentsService,
    ArtifactService,
    IArtifactRelationshipsService,
    IStatefulArtifactFactory,
    MetaDataService,
    StatefulArtifactFactory
} from "../../../managers/artifact-manager";
import {ArtifactRelationshipsMock} from "../../../managers/artifact-manager/relationships/relationships.svc.mock";
import {ValidationServiceMock} from "../../../managers/artifact-manager/validation/validation.mock";
import {SelectionManager} from "../../../managers/selection-manager/selection-manager";
import {DialogServiceMock} from "../../../shared/widgets/bp-dialog/bp-dialog.mock";
import {ComponentTest} from "../../../util/component.test";
import {MessageServiceMock} from "../../../main/components/messages/message.mock";
import {IOnPanelChangesObject, PanelType} from "../utility-panel.svc";
import {BPRelationshipsPanelController} from "./bp-relationships-panel";
import * as angular from "angular";

describe("Component BPRelationshipsPanel", () => {

    let directiveTest: ComponentTest<BPRelationshipsPanelController>;
    let vm: BPRelationshipsPanelController;
    let bpAccordionPanelController = {
        isActiveObservable: new Rx.BehaviorSubject<boolean>(true).asObservable()
    };
    let traces;
    let onChangesObj: IOnPanelChangesObject;

    beforeEach(angular.mock.module("ui.router"));
    beforeEach(angular.mock.module("app.shell"));
    beforeEach(angular.mock.module("app.main"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("artifactRelationships", ArtifactRelationshipsMock);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("dialogService", DialogServiceMock);
        $provide.service("selectionManager", SelectionManager);
        $provide.service("messageService", MessageServiceMock);
        $provide.service("artifactService", ArtifactService);
        $provide.service("selectionManager", SelectionManager);
        $provide.service("artifactAttachments", ArtifactAttachmentsService);
        $provide.service("metadataService", MetaDataService);
        $provide.service("statefulArtifactFactory", StatefulArtifactFactory);
        $provide.service("processService", ProcessServiceMock);
        $provide.service("navigationService", NavigationServiceMock);
        $provide.service("publishService", UnpublishedArtifactsServiceMock);
        $provide.service("validationService", ValidationServiceMock);
        $provide.service("propertyDescriptorBuilder", PropertyDescriptorBuilderMock);
    }));

    beforeEach(inject(() => {
        let template = `<bp-relationships-panel></bp-relationships-panel>`;
        directiveTest = new ComponentTest<BPRelationshipsPanelController>(template, "bp-relationships-panel");
        vm = directiveTest.createComponentWithMockParent({}, "bpAccordionPanel", bpAccordionPanelController);
        onChangesObj = {
            context: {
                currentValue: {
                    panelType: PanelType.Relationships
                },
                previousValue: undefined,
                isFirstChange: () => { return true; }
            }
        };

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
        vm = undefined;
        onChangesObj = undefined;
    });

    it("should load data for a selected artifact",
        inject(($rootScope: ng.IRootScopeService) => {
            expect(directiveTest.element.find(".filter-bar").length).toBe(0);
            expect(directiveTest.element.find(".empty-state").length).toBe(1);
        }));

    it("should have empty traces for newly created sub-artifacts",
        inject(($rootScope: ng.IRootScopeService, statefulArtifactFactory: IStatefulArtifactFactory) => {
            //Arrange
            const artifact = statefulArtifactFactory.createStatefulArtifact({
                id: 2,
                name: "Artifact 2",
                version: 1
            });
            let processShape = {
                projectId: 1,
                parentId: 2,
                id: -2,
                name: "SubArtifact 2",
                baseItemTypePredefined: null,
                typePrefix: "PROS",
                propertyValues: {},
                associatedArtifact: null,
                personaReference: null,
                flags: null

            };
            const subArtifact = statefulArtifactFactory.createStatefulProcessSubArtifact(artifact, processShape);
            onChangesObj.context.currentValue.artifact = artifact;
            onChangesObj.context.currentValue.subArtifact = subArtifact;

            //Act
            vm.$onChanges(onChangesObj);
            $rootScope.$digest();

            //Assert
            expect(vm.allTraces.length).toBe(0);
            expect(vm.associations.length).toBe(0);
            expect(vm.manualTraces.length).toBe(0);
            expect(vm.otherTraces.length).toBe(0);
            expect(vm.actorInherits.length).toBe(0);
            expect(vm.documentReferences.length).toBe(0);
        }));

    it("should load data for a selected artifact",
        inject(($rootScope: ng.IRootScopeService, processService: IProcessService,
                statefulArtifactFactory: IStatefulArtifactFactory) => {
            //Arrange
            const artifact = statefulArtifactFactory.createStatefulArtifact({id: 22, name: "Artifact", prefix: "AC"});
            onChangesObj.context.currentValue.artifact = artifact;

            //Act
            vm.$onChanges(onChangesObj);
            $rootScope.$digest();

            //Assert
            expect(vm.manualTraces.length).toBe(2);
            expect(vm.otherTraces.length).toBe(3);
        }));

    it("should change direction of the selected artifact",
        inject(($rootScope: ng.IRootScopeService, statefulArtifactFactory: IStatefulArtifactFactory) => {

            //Arrange
            const artifact = statefulArtifactFactory.createStatefulArtifact({id: 22, name: "Artifact", prefix: "AC"});
            onChangesObj.context.currentValue.artifact = artifact;
            //Act
            vm.$onChanges(onChangesObj);
            $rootScope.$digest();

            //Assert
            expect(vm.manualTraces.length).toBe(2);
            expect(vm.otherTraces.length).toBe(3);
        }));

    it("should load data for a selected artifacts",
        inject(($rootScope: ng.IRootScopeService, statefulArtifactFactory: IStatefulArtifactFactory,
                $httpBackend: ng.IHttpBackendService) => {

            //Arrange
            vm.item = statefulArtifactFactory.createStatefulArtifact({id: 22, name: "Artifact", prefix: "AC"});
            vm.selectedTraces = {};
            vm.selectedTraces[22] = [traces[0]];

            $httpBackend.whenPOST(`/svc/shared/artifacts/lock`, [22]).respond("");

            //Act
            vm.setSelectedDirection(2);
            $rootScope.$digest();

            //Assert
            expect(vm.selectedTraces[22][0].traceDirection).toBe(2);
        }));

    it("should flag traces",
        inject(($httpBackend: ng.IHttpBackendService, $rootScope: ng.IRootScopeService,
                statefulArtifactFactory: IStatefulArtifactFactory,
                artifactRelationships: IArtifactRelationshipsService) => {

            //Arrange
            vm.item = statefulArtifactFactory.createStatefulArtifact({id: 22, name: "Artifact", prefix: "AC"});
            vm.selectedTraces = {};
            vm.selectedTraces[22] = traces;

            $httpBackend.whenPOST(`/svc/shared/artifacts/lock`, [22]).respond("");

            //Act
            vm.toggleFlag();
            $rootScope.$digest();

            //Assert
            expect(vm.selectedTraces[22][0].suspect).toBe(true);
            expect(vm.selectedTraces[22][1].suspect).toBe(true);

        }));

    it("should delete trace from artifact",
        inject(($httpBackend: ng.IHttpBackendService, $rootScope: ng.IRootScopeService,
                statefulArtifactFactory: IStatefulArtifactFactory,
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
                statefulArtifactFactory: IStatefulArtifactFactory,
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
