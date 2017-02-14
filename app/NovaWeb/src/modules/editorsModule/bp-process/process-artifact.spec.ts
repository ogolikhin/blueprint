import "angular-mocks";
import {ItemInfoService} from "../../commonModule/itemInfo/itemInfo.service";
import {LoadingOverlayService} from "../../commonModule/loadingOverlay/loadingOverlay.service";
import {LocalizationServiceMock} from "../../commonModule/localization/localization.service.mock";
import {MessageServiceMock} from "../../main/components/messages/message.mock";
import {Models} from "../../main/models";
import {ItemTypePredefined} from "../../main/models/itemTypePredefined.enum";
import {IStatefulArtifactFactory, MetaDataService, StatefulArtifactFactory} from "../../managers/artifact-manager";
import {IArtifactService} from "../../managers/artifact-manager/artifact/artifact.svc";
import {ArtifactServiceMock} from "../../managers/artifact-manager/artifact/artifact.svc.mock";
import {ArtifactAttachmentsMock} from "../../managers/artifact-manager/attachments/attachments.svc.mock";
import {ArtifactRelationshipsMock} from "../../managers/artifact-manager/relationships/relationships.svc.mock";
import {IStatefulSubArtifact} from "../../managers/artifact-manager/sub-artifact/sub-artifact";
import {ValidationServiceMock} from "../../managers/artifact-manager/validation/validation.mock";
import {SelectionManager} from "../../managers/selection-manager/selection-manager";
import {DialogServiceMock} from "../../shared/widgets/bp-dialog/bp-dialog.mock";
import {AuthSvc} from "../../shell/login/auth.svc";
import {SettingsMock} from "../../shell/login/mocks.spec";
import {SessionSvcMock} from "../../shell/login/session.svc.mock";
import {PropertyDescriptorBuilderMock} from "../services";
import {UnpublishedArtifactsServiceMock} from "../unpublished/unpublished.service.mock";
import {IProcess} from "./models/process-models";
import * as TestModels from "./models/test-model-factory";
import {INovaProcess, StatefulProcessArtifact} from "./process-artifact";
import * as angular from "angular";

describe("StatefulProcessArtifact", () => {

    beforeEach(angular.mock.module("ui.bootstrap"));



    let $q: ng.IQService;
    let $log: ng.ILogService;
    let $rootScope: ng.IRootScopeService;
    let statefulArtifactFactory: IStatefulArtifactFactory;
    let getArtifactModelSpy: jasmine.Spy;
    let artifactServiceMock: IArtifactService;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("artifactRelationships", ArtifactRelationshipsMock);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("dialogService", DialogServiceMock);
        $provide.service("selectionManager", SelectionManager);
        $provide.service("messageService", MessageServiceMock);
        $provide.service("artifactService", ArtifactServiceMock);
        $provide.service("selectionManager", SelectionManager);
        $provide.service("artifactAttachments", ArtifactAttachmentsMock);
        $provide.service("metadataService", MetaDataService);
        $provide.service("statefulArtifactFactory", StatefulArtifactFactory);
        $provide.service("publishService", UnpublishedArtifactsServiceMock);
        $provide.service("validationService", ValidationServiceMock);
        $provide.service("propertyDescriptorBuilder", PropertyDescriptorBuilderMock);
        $provide.service("session", SessionSvcMock);
        $provide.service("auth", AuthSvc);
        $provide.service("settings", SettingsMock);

        $provide.service("itemInfoService", ItemInfoService);
        $provide.service("loadingOverlayService", LoadingOverlayService);

    }));
    beforeEach(inject((_$rootScope_: ng.IRootScopeService,
                       _$q_: ng.IQService,
                       _$log_: ng.ILogService,
                       _statefulArtifactFactory_: IStatefulArtifactFactory,
                       artifactService: ArtifactServiceMock) => {

        statefulArtifactFactory = _statefulArtifactFactory_;

        $rootScope = _$rootScope_;
        $q = _$q_;
        $log = _$log_;
        artifactServiceMock = artifactService;

        const processArtifactReturn: INovaProcess = ArtifactServiceMock.createArtifact(1);
        processArtifactReturn.process = TestModels.createDefaultProcessModel();
        getArtifactModelSpy = spyOn(artifactService, "getArtifactModel").and.returnValue($q.when(processArtifactReturn));
    }));

    describe("Load -", () => {

        it("calls the artifact service to retrieve information", () => {
            //Arrange

            const artifact = {
                id: 1,
                name: "",
                projectId: 1,
                predefinedType: ItemTypePredefined.Process
            } as Models.IArtifact;


            const processArtifact = <StatefulProcessArtifact>statefulArtifactFactory.createStatefulArtifact(artifact);

            //Act
            processArtifact.getObservable();

            //Assert
            expect(getArtifactModelSpy).toHaveBeenCalled();
        });

        it("multiple loads will only execute once if initial load is not finished.", () => {
            //Arrange

            const artifact = {
                id: 1,
                name: "",
                projectId: 1,
                predefinedType: ItemTypePredefined.Process
            } as Models.IArtifact;


            const processArtifact = <StatefulProcessArtifact>statefulArtifactFactory.createStatefulArtifact(artifact);

            //Act
            processArtifact.getObservable();
            processArtifact.getObservable();
            processArtifact.getObservable();

            //Assert
            expect(getArtifactModelSpy).toHaveBeenCalledTimes(1);
        });

        it("getArtifactModel is called with process load url", () => {
            const artifact = {
                id: 1,
                name: "",
                projectId: 1,
                predefinedType: ItemTypePredefined.Process
            } as Models.IArtifact;


            const processArtifact = <StatefulProcessArtifact>statefulArtifactFactory.createStatefulArtifact(artifact);

            processArtifact.getObservable();
            $rootScope.$digest();

            expect(getArtifactModelSpy).toHaveBeenCalledWith("/svc/bpartifactstore/process/1", 1, undefined);
        });

        it("artifact service updates are reflected on model", () => {
            //Arrange

            const artifact = {
                id: 1,
                name: "",
                projectId: 1,
                predefinedType: ItemTypePredefined.Process
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

        describe("process service updates are reflected on model", () => {

            let processArtifact: StatefulProcessArtifact,
                model: IProcess;
            beforeEach(() => {
                const artifact = {
                    id: 1,
                    name: "",
                    projectId: 1,
                    predefinedType: ItemTypePredefined.Process
                } as Models.IArtifact;


                processArtifact = <StatefulProcessArtifact>statefulArtifactFactory.createStatefulArtifact(artifact);

                model = TestModels.createDefaultProcessModel();
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
    });

    describe("Save -", () => {
        it("updateArtifact is called with process save url and contains process model", () => {
            const artifact = {
                id: 1,
                name: "",
                projectId: 1,
                predefinedType: ItemTypePredefined.Process
            } as Models.IArtifact;


            const processArtifact = <StatefulProcessArtifact>statefulArtifactFactory.createStatefulArtifact(artifact);
            spyOn(processArtifact, "canBeSaved").and.returnValue(true);

            let url: string;
            let changes: INovaProcess;
            const updateSpy = spyOn(artifactServiceMock, "updateArtifact").and.callFake((_url, _changes) => {
                url = _url;
                changes = _changes;
                return $q.when(processArtifact);
            });

            processArtifact.save();
            $rootScope.$digest();

            expect(url).toBe("/svc/bpartifactstore/processupdate/1");
            expect(changes).toBeDefined();
            expect(changes.process).toBeDefined();
        });
    });
});
