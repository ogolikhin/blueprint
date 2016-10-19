import * as angular from "angular";
import {IArtifactService} from "../../managers/artifact-manager/";
import {ArtifactServiceMock} from "../../managers/artifact-manager/artifact/artifact.svc.mock";
import {IProcessService} from "./services/process.svc";
import {ProcessServiceMock} from "./services/process.svc.mock";

import {
    IStatefulProcessArtifactServices,
    StatefulArtifactServices,
    StatefulProcessArtifactServices
} from "../../managers/artifact-manager/services";
import {StatefulProcessArtifact} from "./process-artifact";
import {StatefulProcessSubArtifact} from "./process-subartifact";
import {IStatefulSubArtifact} from "../../managers/artifact-manager/sub-artifact/sub-artifact";


import {Models} from "../../main/models";

import * as TestModels from "./models/test-model-factory";
import {IProcess} from "./models/process-models";

describe("StatefulProcessArtifact", () => {

    let services: IStatefulProcessArtifactServices;
    let $q: ng.IQService;
    let $rootScope: ng.IRootScopeService;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("artifactService", ArtifactServiceMock);
        $provide.service("session", null);
        $provide.service("processService", ProcessServiceMock);
        $provide.service("messageService", null);
        $provide.service("localizationService", null);
        $provide.service("dialogService", null);
        $provide.service("attachmentService", null);
        $provide.service("relationshipsService", null);
        $provide.service("projectManager", null);
        $provide.service("metadataService", null);
    }));
    beforeEach(inject((_$rootScope_: ng.IRootScopeService,
                       _$q_: ng.IQService,
                       artifactService: IArtifactService,
                       processService: IProcessService) => {
        $rootScope = _$rootScope_;
        $q = _$q_;
        let artitfactServices = new StatefulArtifactServices(_$q_, null, null, null, null, artifactService, null, null, null, null);
        services = new StatefulProcessArtifactServices(artitfactServices, _$q_, processService);
    }));


    it("Load - calls both the artifact service and process service to retrieve information", () => {
        //Arrange

        const artifact = {
            id: 1,
            name: "",
            projectId: 1,
            predefinedType: Models.ItemTypePredefined.TextualRequirement
        } as Models.IArtifact;


        let processArtifact = new StatefulProcessArtifact(artifact, services);

        let loadSpy = spyOn(services.processService, "load").and.callThrough();
        let artifactSpy = spyOn(services.artifactService, "getArtifact").and.callThrough();

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
            predefinedType: Models.ItemTypePredefined.TextualRequirement
        } as Models.IArtifact;


        let processArtifact = new StatefulProcessArtifact(artifact, services);

        let loadSpy = spyOn(services.processService, "load").and.callThrough();
        let artifactSpy = spyOn(services.artifactService, "getArtifact").and.callThrough();

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
            predefinedType: Models.ItemTypePredefined.TextualRequirement
        } as Models.IArtifact;


        let processArtifact = new StatefulProcessArtifact(artifact, services);
        let isLoaded: boolean = false;
        let loaded = () => {
            isLoaded = true;
        };
        //Act
        processArtifact.getObservable().subscribe(loaded, () => {
        });
        $rootScope.$digest();

        //Assert
        expect(isLoaded).toBeTruthy();
    });

    describe("Load - process service updates are reflected on model", ()=> {

        let processArtifact: StatefulProcessArtifact,
            model: IProcess;
        beforeEach(()=> {
            const artifact = {
                id: 1,
                name: "",
                projectId: 1,
                predefinedType: Models.ItemTypePredefined.TextualRequirement
            } as Models.IArtifact;


            processArtifact = new StatefulProcessArtifact(artifact, services);

            model = TestModels.createDefaultProcessModel();

            let loadSpy = spyOn(services.processService, "load");
            loadSpy.and.returnValue($q.when(model));
        })

        it("IProcess is populated", () => {
            //Act
            processArtifact.getObservable();
            $rootScope.$digest();

            //Assert
            let process: IProcess = processArtifact;
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
            let statefulSubArtifact: IStatefulSubArtifact = processArtifact.subArtifactCollection.list()[0];
            expect(statefulSubArtifact.artifactState).not.toBeUndefined();
        });


    });
});
