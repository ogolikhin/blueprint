import * as angular from "angular";
import "angular-mocks";
import "../../shell";
import {IProcessUpdateResult} from "./services/process.svc";
import {ProcessServiceMock} from "./services/process.svc.mock";
import {StatefulProcessArtifact} from "./process-artifact";
import {IStatefulSubArtifact} from "../../managers/artifact-manager/sub-artifact/sub-artifact";
import {Models} from "../../main/models";
import * as TestModels from "./models/test-model-factory";
import {IProcess} from "./models/process-models";
import {ShapeModelMock} from "./components/diagram/presentation/graph/shapes/shape-model.mock";
import {
    ArtifactManager,
    IStatefulArtifactFactory,
    StatefulArtifactFactory,
    MetaDataService
} from "../../managers/artifact-manager";
import {ArtifactServiceMock} from "../../managers/artifact-manager/artifact/artifact.svc.mock";
import {MessageServiceMock} from "../../core/messages/message.mock";
import {ValidationServiceMock} from "../../managers/artifact-manager/validation/validation.mock";
import {LocalizationServiceMock} from "../../core/localization/localization.mock";
import {ArtifactAttachmentsMock} from "../../managers/artifact-manager/attachments/attachments.svc.mock";
import {ArtifactRelationshipsMock} from "../../managers/artifact-manager/relationships/relationships.svc.mock";
import {DialogServiceMock} from "../../shared/widgets/bp-dialog/bp-dialog";
import {SelectionManager} from "../../managers/selection-manager/selection-manager";
import {PropertyDescriptorBuilderMock} from "../configuration/property-descriptor-builder.mock";
import {UnpublishedArtifactsServiceMock} from "../unpublished/unpublished.svc.mock";

describe("StatefulProcessArtifact", () => {

    let $q: ng.IQService;
    let $log: ng.ILogService;
    let $rootScope: ng.IRootScopeService;
    let statefulArtifactFactory: IStatefulArtifactFactory;

    beforeEach(angular.mock.module("app.shell"));
    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("artifactRelationships", ArtifactRelationshipsMock);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("dialogService", DialogServiceMock);
        $provide.service("selectionManager", SelectionManager);
        $provide.service("messageService", MessageServiceMock);
        $provide.service("artifactService", ArtifactServiceMock);
        $provide.service("artifactManager", ArtifactManager);
        $provide.service("artifactAttachments", ArtifactAttachmentsMock);
        $provide.service("metadataService", MetaDataService);
        $provide.service("statefulArtifactFactory", StatefulArtifactFactory);
        $provide.service("processService", ProcessServiceMock);
        $provide.service("publishService", UnpublishedArtifactsServiceMock);
        $provide.service("validationService", ValidationServiceMock);
        $provide.service("propertyDescriptorBuilder", PropertyDescriptorBuilderMock);
    }));
    beforeEach(inject((_$rootScope_: ng.IRootScopeService,
        _$q_: ng.IQService,
        _$log_: ng.ILogService,
        _statefulArtifactFactory_: IStatefulArtifactFactory
        ) => {

        statefulArtifactFactory = _statefulArtifactFactory_;
        $rootScope = _$rootScope_;
        $q = _$q_;
        $log = _$log_;
    }));


    it("Load - calls both the artifact service and process service to retrieve information", () => {
        //Arrange

        const artifact = {
            id: 1,
            name: "",
            projectId: 1,
            predefinedType: Models.ItemTypePredefined.Process
        } as Models.IArtifact;


        const processArtifact = <StatefulProcessArtifact>statefulArtifactFactory.createStatefulArtifact(artifact);

        const loadSpy = spyOn(processArtifact.getServices().processService, "load").and.callThrough();
        const artifactSpy = spyOn(processArtifact.getServices().artifactService, "getArtifact").and.callThrough();

        //Act
        processArtifact.getObservable();

        //Assert
        expect(loadSpy).toHaveBeenCalled();
        expect(artifactSpy).toHaveBeenCalled();
    });

    it("Load - multiple loads will only execute once if initial load is not finished.", () => {
        //Arrange

        const artifact = {
            id: 1,
            name: "",
            projectId: 1,
            predefinedType: Models.ItemTypePredefined.Process
        } as Models.IArtifact;


        const processArtifact = <StatefulProcessArtifact>statefulArtifactFactory.createStatefulArtifact(artifact);

        const loadSpy = spyOn(processArtifact.getServices().processService, "load").and.callThrough();
        const artifactSpy = spyOn(processArtifact.getServices().artifactService, "getArtifact").and.callThrough();

        //Act
        processArtifact.getObservable();
        processArtifact.getObservable();
        processArtifact.getObservable();

        //Assert
        expect(loadSpy).toHaveBeenCalledTimes(1);
        expect(artifactSpy).toHaveBeenCalledTimes(1);
    });

    it("Load - artifact service updates are reflected on model", () => {
        //Arrange

        const artifact = {
            id: 1,
            name: "",
            projectId: 1,
            predefinedType: Models.ItemTypePredefined.Process
        } as Models.IArtifact;


        const processArtifact = <StatefulProcessArtifact>statefulArtifactFactory.createStatefulArtifact(artifact);
        let isLoaded: boolean = false;
        const loaded = () => {
            isLoaded = true;
        };
        //Act
        processArtifact.getObservable().subscribe(loaded, () => {
            return;
        });
        $rootScope.$digest();

        //Assert
        expect(isLoaded).toBeTruthy();
    });

    describe("Load - process service updates are reflected on model", () => {

        let processArtifact: StatefulProcessArtifact,
            model: IProcess;
        beforeEach(() => {
            const artifact = {
                id: 1,
                name: "",
                projectId: 1,
                predefinedType: Models.ItemTypePredefined.Process
            } as Models.IArtifact;


            processArtifact = <StatefulProcessArtifact>statefulArtifactFactory.createStatefulArtifact(artifact);

            model = TestModels.createDefaultProcessModel();

            const loadSpy = spyOn(processArtifact.getServices().processService, "load");
            loadSpy.and.returnValue($q.when(model));
        });

        it("IProcess is populated", () => {
            //Act
            processArtifact.getObservable();
            $rootScope.$digest();

            //Assert
            const process: IProcess = processArtifact;
            expect(process.shapes.length).toBe(model.shapes.length);
            expect(process.links.length).toBe(model.links.length);
            expect(process.baseItemTypePredefined).toBe(processArtifact.predefinedType);
            expect(process.typePrefix).toBe(processArtifact.prefix);
        });

        it("subArtifactCollection is populated", () => {
            //Act
            processArtifact.getObservable();
            $rootScope.$digest();

            //Assert
            expect(processArtifact.subArtifactCollection.list().length).toBe(processArtifact.shapes.length);
        });

        it("IProcessShapes are subArtifactCollection with a valid state", () => {
            //Act
            processArtifact.getObservable();
            $rootScope.$digest();

            //Assert
            const statefulSubArtifact: IStatefulSubArtifact = processArtifact.subArtifactCollection.list()[0];
            expect(statefulSubArtifact.artifactState).not.toBeUndefined();
        });


    });

    describe("Changes - ", () => {
        it("updates new artifact ids and calls updateArtifact with correct updated subArtifacts", () => {
            // arrange
            const artifact = {
                id: 1,
                name: "",
                projectId: 1,
                predefinedType: Models.ItemTypePredefined.Process,
                version: 1
            } as Models.IArtifact;

            const processArtifact = <StatefulProcessArtifact>statefulArtifactFactory.createStatefulArtifact(artifact);
            processArtifact.artifactState.readonly = false;

            spyOn(processArtifact, "lock");
            spyOn(processArtifact, "canBeSaved").and.callFake(() => {
                return true;
            });

            const testUserTask = ShapeModelMock.instance().UserTaskMock();
            testUserTask.id = -1;
            const newShape = statefulArtifactFactory.createStatefulProcessSubArtifact(processArtifact, testUserTask);
            processArtifact.shapes = [newShape];
            processArtifact.subArtifactCollection.add(newShape);
            newShape.attachments.initialize([]);

            const newIdValue = 100;
            const keyValuePair: Models.IKeyValuePair = {key: -1, value: newIdValue};
            const updateModel: IProcessUpdateResult = {messages: [], result: processArtifact, tempIdMap: [keyValuePair]};
            const changesSpy = spyOn(processArtifact, "changes").and.callThrough();
            spyOn(processArtifact.getServices().processService, "save").and.returnValue($q.when(updateModel))();
            let changes;
            spyOn(processArtifact.getServices().artifactService, "updateArtifact").and.callFake((parameter) => {
                changes = parameter;
                return $q.when();
            })();


            //act
            newShape.attachments.add([{
                userId: 0,
                userName: "test",
                fileName: "test.txt",
                fileType: ".txt",
                attachmentId: 1,
                uploadedDate: "test",
                guid: "test",
                changeType: 0
            }]);
            processArtifact.save(true);
            $rootScope.$digest();

            //assert
            expect(changesSpy).toHaveBeenCalled();
            expect(changes.subArtifacts.length).toBe(1);
            expect(changes.subArtifacts[0].id).toBe(newIdValue);
        });
    });

});
