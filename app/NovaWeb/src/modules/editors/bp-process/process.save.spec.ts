import * as angular from "angular";
import "rx/dist/rx.lite";
import { Models, Enums } from "../../main/models";
import { IProcess } from "./models/process-models";
import { IState } from "../../managers/artifact-manager/state/state";
import { IArtifactService } from "../../managers/artifact-manager/";
import { ArtifactServiceMock } from "../../managers/artifact-manager/artifact/artifact.svc.mock";
import { ISession } from "../../shell/login/session.svc";
import { SessionSvcMock } from "../../shell/login//mocks.spec";
import { IProcessService, ProcessService } from "./services/process.svc";
import { IProcessUpdateResult } from "./services/process.svc";
import { IStatefulProcessArtifactServices } from "../../managers/artifact-manager/services";
import { StatefulArtifactServices } from "../../managers/artifact-manager/services";
import { StatefulProcessArtifactServices } from "../../managers/artifact-manager/services";
import { StatefulProcessArtifact } from "./process-artifact";
import { StatefulProcessSubArtifact } from "./process-subartifact";
import { IStatefulSubArtifact } from "../../managers/artifact-manager/sub-artifact/sub-artifact";
import * as TestModels from "./models/test-model-factory";


describe("When process is saved", () => {

    let services: IStatefulProcessArtifactServices;
    let $q: ng.IQService;
    let $httpBackend: ng.IHttpBackendService;
    let $rootScope: ng.IRootScopeService;
    let session: ISession = null;
    let artifactModel: Models.IArtifact;
    let processModel: IProcess;
    let result: IProcessUpdateResult;
    let processArtifact: StatefulProcessArtifact;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("artifactService", ArtifactServiceMock);
        $provide.service("session", null);
        $provide.service("processService", ProcessService);
        $provide.service("messageService", null);
        $provide.service("localizationService", null);
        $provide.service("dialogService", null);
        $provide.service("attachmentService", null);
        $provide.service("relationshipsService", null);
        $provide.service("projectManager", null);
        $provide.service("metadataService", null);
        $provide.service("publishService", null);
    }));
    beforeEach(inject((
        _$rootScope_: ng.IRootScopeService,
		_$q_: ng.IQService,
		_$httpBackend_: ng.IHttpBackendService,
		artifactService: IArtifactService,
		processService: IProcessService) => {

        $rootScope = _$rootScope_;
        $q = _$q_;
        $httpBackend = _$httpBackend_;

        session = new SessionSvcMock($q);

        processModel = JSON.parse('{ "id":4831, "name":"Process 4", "typePrefix":"PRO", "projectId":1, "baseItemTypePredefined":4114, "shapes":[{ "associatedArtifact": null, "baseItemTypePredefined": 8228, "id": 4900, "name": "UT1", "parentId": 4831, "projectId": 1, "typePrefix": "PROS", "propertyValues": { "Persona": { "propertyName": "Persona", "typePredefined": 4130, "typeId": null, "value": "User" }, "StoryLinks": { "propertyName": "StoryLinks", "typePredefined": 4131, "typeId": null, "value": null }, "Description": { "propertyName": "Description", "typePredefined": 4099, "typeId": 47, "value": "" }, "Label": { "propertyName": "Label", "typePredefined": 4115, "typeId": 63, "value": "" }, "Width": { "propertyName": "Width", "typePredefined": 8195, "typeId": 66, "value": -1 }, "Height": { "propertyName": "Height", "typePredefined": 8196, "typeId": 67, "value": -1 }, "X": { "propertyName": "X", "typePredefined": 8193, "typeId": 64, "value": 2 }, "Y": { "propertyName": "Y", "typePredefined": 8194, "typeId": 65, "value": 0 }, "ClientType": { "propertyName": "ClientType", "typePredefined": 4114, "typeId": 62, "value": 2 }, "ItemLabel": { "propertyName": "ItemLabel", "typePredefined": 4102, "typeId": 51, "value": "" }, "ImageId": { "propertyName": "ImageId", "typePredefined": 4132, "typeId": null, "value": null } } }, { "associatedArtifact": null, "baseItemTypePredefined": 8228, "id": 4903, "name": "ST2", "parentId": 4831, "projectId": 1, "typePrefix": "PROS", "propertyValues": { "Persona": { "propertyName": "Persona", "typePredefined": 4130, "typeId": null, "value": "System" }, "AssociatedImageUrl": { "propertyName": "AssociatedImageUrl", "typePredefined": 0, "typeId": null, "value": null }, "ImageId": { "propertyName": "ImageId", "typePredefined": 4132, "typeId": null, "value": null }, "StoryLinks": { "propertyName": "StoryLinks", "typePredefined": 4131, "typeId": null, "value": null }, "Description": { "propertyName": "Description", "typePredefined": 4099, "typeId": 47, "value": "" }, "Label": { "propertyName": "Label", "typePredefined": 4115, "typeId": 63, "value": "" }, "Width": { "propertyName": "Width", "typePredefined": 8195, "typeId": 66, "value": -1 }, "Height": { "propertyName": "Height", "typePredefined": 8196, "typeId": 67, "value": -1 }, "X": { "propertyName": "X", "typePredefined": 8193, "typeId": 64, "value": 3 }, "Y": { "propertyName": "Y", "typePredefined": 8194, "typeId": 65, "value": 0 }, "ClientType": { "propertyName": "ClientType", "typePredefined": 4114, "typeId": 62, "value": 4 }, "ItemLabel": { "propertyName": "ItemLabel", "typePredefined": 4102, "typeId": 51, "value": "" } } }, { "associatedArtifact": null, "baseItemTypePredefined": 8228, "id": 4833, "name": "Start", "parentId": 4831, "projectId": 1, "typePrefix": "PROS", "propertyValues": { "Description": { "propertyName": "Description", "typePredefined": 4099, "typeId": 47, "value": "" }, "Label": { "propertyName": "Label", "typePredefined": 4115, "typeId": 63, "value": "" }, "Width": { "propertyName": "Width", "typePredefined": 8195, "typeId": 66, "value": 126 }, "Height": { "propertyName": "Height", "typePredefined": 8196, "typeId": 67, "value": 150 }, "X": { "propertyName": "X", "typePredefined": 8193, "typeId": 64, "value": 0 }, "Y": { "propertyName": "Y", "typePredefined": 8194, "typeId": 65, "value": 0 }, "ClientType": { "propertyName": "ClientType", "typePredefined": 4114, "typeId": 62, "value": 1 }, "ItemLabel": { "propertyName": "ItemLabel", "typePredefined": 4102, "typeId": 51, "value": "" } } }, { "associatedArtifact": null, "baseItemTypePredefined": 8228, "id": 4834, "name": "Precondition", "parentId": 4831, "projectId": 1, "typePrefix": "PROS", "propertyValues": { "Persona": { "propertyName": "Persona", "typePredefined": 4130, "typeId": null, "value": "System" }, "AssociatedImageUrl": { "propertyName": "AssociatedImageUrl", "typePredefined": 0, "typeId": null, "value": null }, "ImageId": { "propertyName": "ImageId", "typePredefined": 4132, "typeId": null, "value": null }, "StoryLinks": { "propertyName": "StoryLinks", "typePredefined": 4131, "typeId": null, "value": null }, "Description": { "propertyName": "Description", "typePredefined": 4099, "typeId": 47, "value": "" }, "Label": { "propertyName": "Label", "typePredefined": 4115, "typeId": 63, "value": "" }, "Width": { "propertyName": "Width", "typePredefined": 8195, "typeId": 66, "value": 126 }, "Height": { "propertyName": "Height", "typePredefined": 8196, "typeId": 67, "value": 150 }, "X": { "propertyName": "X", "typePredefined": 8193, "typeId": 64, "value": 1 }, "Y": { "propertyName": "Y", "typePredefined": 8194, "typeId": 65, "value": 0 }, "ClientType": { "propertyName": "ClientType", "typePredefined": 4114, "typeId": 62, "value": 5 }, "ItemLabel": { "propertyName": "ItemLabel", "typePredefined": 4102, "typeId": 51, "value": "" } } }, { "associatedArtifact": null, "baseItemTypePredefined": 8228, "id": 4843, "name": "End", "parentId": 4831, "projectId": 1, "typePrefix": "PROS", "propertyValues": { "Description": { "propertyName": "Description", "typePredefined": 4099, "typeId": 47, "value": "" }, "Label": { "propertyName": "Label", "typePredefined": 4115, "typeId": 63, "value": "" }, "Width": { "propertyName": "Width", "typePredefined": 8195, "typeId": 66, "value": 126 }, "Height": { "propertyName": "Height", "typePredefined": 8196, "typeId": 67, "value": 150 }, "X": { "propertyName": "X", "typePredefined": 8193, "typeId": 64, "value": 6 }, "Y": { "propertyName": "Y", "typePredefined": 8194, "typeId": 65, "value": 0 }, "ClientType": { "propertyName": "ClientType", "typePredefined": 4114, "typeId": 62, "value": 3 }, "ItemLabel": { "propertyName": "ItemLabel", "typePredefined": 4102, "typeId": 51, "value": "" } } }, { "associatedArtifact": null, "baseItemTypePredefined": 8228, "id": -1, "name": "New Task", "parentId": 4831, "projectId": 1, "typePrefix": "PROS", "propertyValues": { "Persona": { "propertyName": "Persona", "typePredefined": 4130, "typeId": -1, "value": "User" }, "Label": { "propertyName": "Label", "typePredefined": 4115, "typeId": -1, "value": "" }, "Description": { "propertyName": "Description", "typePredefined": 4099, "typeId": -1, "value": "" }, "X": { "propertyName": "X", "typePredefined": 8193, "typeId": -1, "value": 4 }, "Y": { "propertyName": "Y", "typePredefined": 8194, "typeId": -1, "value": 0 }, "Width": { "propertyName": "Width", "typePredefined": 8195, "typeId": -1, "value": -1 }, "Height": { "propertyName": "Height", "typePredefined": 8196, "typeId": -1, "value": -1 }, "ClientType": { "propertyName": "ClientType", "typePredefined": 4114, "typeId": -1, "value": 2 }, "ItemLabel": { "propertyName": "ItemLabel", "typePredefined": 4102, "typeId": -1, "value": "" } } }, { "associatedArtifact": null, "baseItemTypePredefined": 8228, "id": -2, "name": "New System", "parentId": 4831, "projectId": 1, "typePrefix": "PROS", "propertyValues": { "Persona": { "propertyName": "Persona", "typePredefined": 4130, "typeId": -1, "value": "System" }, "AssociatedImageUrl": { "propertyName": "AssociatedImageUrl", "typePredefined": 0, "typeId": -1, "value": "" }, "ImageId": { "propertyName": "ImageId", "typePredefined": 4132, "typeId": -1, "value": "" }, "Label": { "propertyName": "Label", "typePredefined": 4115, "typeId": -1, "value": "" }, "Description": { "propertyName": "Description", "typePredefined": 4099, "typeId": -1, "value": "" }, "X": { "propertyName": "X", "typePredefined": 8193, "typeId": -1, "value": 5 }, "Y": { "propertyName": "Y", "typePredefined": 8194, "typeId": -1, "value": 0 }, "Width": { "propertyName": "Width", "typePredefined": 8195, "typeId": -1, "value": -1 }, "Height": { "propertyName": "Height", "typePredefined": 8196, "typeId": -1, "value": -1 }, "ClientType": { "propertyName": "ClientType", "typePredefined": 4114, "typeId": -1, "value": 4 }, "ItemLabel": { "propertyName": "ItemLabel", "typePredefined": 4102, "typeId": -1, "value": "" } } }], "links":[{ "destinationId": 4903, "label": null, "orderindex": 0, "sourceId": 4900 }, { "destinationId": -1, "label": null, "orderindex": 0, "sourceId": 4903 }, { "destinationId": 4834, "label": null, "orderindex": 0, "sourceId": 4833 }, { "destinationId": 4900, "label": null, "orderindex": 0, "sourceId": 4834 }, { "destinationId": -2, "label": "", "orderindex": 0, "sourceId": -1 }, { "destinationId": 4843, "label": "", "orderindex": 0, "sourceId": -2 }], "propertyValues":{ "Description":{ "propertyName":"Description", "typePredefined":4099, "typeId":47, "value":"" },"ClientType":{ "propertyName": "ClientType", "typePredefined": 4114, "typeId": 62, "value": 2 }}, "decisionBranchDestinationLinks":[], "itemTypeId":0, "requestedVersionInfo":null}');
        
        let artifactServices = new StatefulArtifactServices(
            _$q_, session, null, null, null, artifactService, null, null, null, null, null);

        services = new StatefulProcessArtifactServices(artifactServices, _$q_, processService);
		
        artifactModel = {
            id: 22,
            name: "Process Artifact",
            prefix: "My",
            predefinedType: Models.ItemTypePredefined.Process,
            permissions: Enums.RolePermissions.Edit
        };
		
        processArtifact = new StatefulProcessArtifact(artifactModel, services);
        processArtifact["onLoad"](processModel);

        let newState: IState = {
            lockDateTime: new Date(),
            lockedBy: Enums.LockedByEnum.CurrentUser,
            lockOwner: "Default Instance Admin",
            readonly: false,
            dirty: true
        };

        processArtifact.artifactState.setState(newState, false);

        // Setup the data we wish to return for the http call  
        result = JSON.parse('{"messages":[],"result":{"id":4831,"name":"Process 4","typePrefix":"PRO","projectId":1,"itemTypeId":281,"baseItemTypePredefined":4114,"shapes":[{"flags":{"hasComments":false,"hasTraces":false},"id":4900,"name":"UT1","projectId":1,"typePrefix":"PROS","parentId":4831,"baseItemTypePredefined":8228,"propertyValues":{"persona":{"propertyName":"Persona","typePredefined":4130,"typeId":null,"value":"User"},"storyLinks":{"propertyName":"StoryLinks","typePredefined":4131,"typeId":null,"value":null},"description":{"propertyName":"Description","typePredefined":4099,"typeId":47,"value":""},"label":{"propertyName":"Label","typePredefined":4115,"typeId":63,"value":""},"width":{"propertyName":"Width","typePredefined":8195,"typeId":66,"value":-1.0},"height":{"propertyName":"Height","typePredefined":8196,"typeId":67,"value":-1.0},"x":{"propertyName":"X","typePredefined":8193,"typeId":64,"value":2.0},"y":{"propertyName":"Y","typePredefined":8194,"typeId":65,"value":0.0},"clientType":{"propertyName":"ClientType","typePredefined":4114,"typeId":62,"value":2},"itemLabel":{"propertyName":"ItemLabel","typePredefined":4102,"typeId":51,"value":""},"imageId":{"propertyName":"ImageId","typePredefined":4132,"typeId":null,"value":null}},"associatedArtifact":null},{"flags":{"hasComments":false,"hasTraces":false},"id":4903,"name":"ST2","projectId":1,"typePrefix":"PROS","parentId":4831,"baseItemTypePredefined":8228,"propertyValues":{"persona":{"propertyName":"Persona","typePredefined":4130,"typeId":null,"value":"System"},"associatedImageUrl":{"propertyName":"AssociatedImageUrl","typePredefined":0,"typeId":null,"value":null},"imageId":{"propertyName":"ImageId","typePredefined":4132,"typeId":null,"value":null},"storyLinks":{"propertyName":"StoryLinks","typePredefined":4131,"typeId":null,"value":null},"description":{"propertyName":"Description","typePredefined":4099,"typeId":47,"value":""},"label":{"propertyName":"Label","typePredefined":4115,"typeId":63,"value":""},"width":{"propertyName":"Width","typePredefined":8195,"typeId":66,"value":-1.0},"height":{"propertyName":"Height","typePredefined":8196,"typeId":67,"value":-1.0},"x":{"propertyName":"X","typePredefined":8193,"typeId":64,"value":3.0},"y":{"propertyName":"Y","typePredefined":8194,"typeId":65,"value":0.0},"clientType":{"propertyName":"ClientType","typePredefined":4114,"typeId":62,"value":4},"itemLabel":{"propertyName":"ItemLabel","typePredefined":4102,"typeId":51,"value":""}},"associatedArtifact":null},{"flags":{"hasComments":false,"hasTraces":false},"id":4888,"name":"New Task","projectId":1,"typePrefix":"PROS","parentId":4831,"baseItemTypePredefined":8228,"propertyValues":{"persona":{"propertyName":"Persona","typePredefined":4130,"typeId":null,"value":"User"},"storyLinks":{"propertyName":"StoryLinks","typePredefined":4131,"typeId":null,"value":null},"description":{"propertyName":"Description","typePredefined":4099,"typeId":47,"value":""},"label":{"propertyName":"Label","typePredefined":4115,"typeId":63,"value":""},"width":{"propertyName":"Width","typePredefined":8195,"typeId":66,"value":-1.0},"height":{"propertyName":"Height","typePredefined":8196,"typeId":67,"value":-1.0},"x":{"propertyName":"X","typePredefined":8193,"typeId":64,"value":4.0},"y":{"propertyName":"Y","typePredefined":8194,"typeId":65,"value":0.0},"clientType":{"propertyName":"ClientType","typePredefined":4114,"typeId":62,"value":2},"itemLabel":{"propertyName":"ItemLabel","typePredefined":4102,"typeId":51,"value":""},"imageId":{"propertyName":"ImageId","typePredefined":4132,"typeId":null,"value":null}},"associatedArtifact":null},{"flags":{"hasComments":false,"hasTraces":false},"id":4889,"name":"New System","projectId":1,"typePrefix":"PROS","parentId":4831,"baseItemTypePredefined":8228,"propertyValues":{"persona":{"propertyName":"Persona","typePredefined":4130,"typeId":null,"value":"System"},"associatedImageUrl":{"propertyName":"AssociatedImageUrl","typePredefined":0,"typeId":null,"value":null},"imageId":{"propertyName":"ImageId","typePredefined":4132,"typeId":null,"value":null},"storyLinks":{"propertyName":"StoryLinks","typePredefined":4131,"typeId":null,"value":null},"description":{"propertyName":"Description","typePredefined":4099,"typeId":47,"value":""},"label":{"propertyName":"Label","typePredefined":4115,"typeId":63,"value":""},"width":{"propertyName":"Width","typePredefined":8195,"typeId":66,"value":-1.0},"height":{"propertyName":"Height","typePredefined":8196,"typeId":67,"value":-1.0},"x":{"propertyName":"X","typePredefined":8193,"typeId":64,"value":5.0},"y":{"propertyName":"Y","typePredefined":8194,"typeId":65,"value":0.0},"clientType":{"propertyName":"ClientType","typePredefined":4114,"typeId":62,"value":4},"itemLabel":{"propertyName":"ItemLabel","typePredefined":4102,"typeId":51,"value":""}},"associatedArtifact":null},{"id":4833,"name":"Start","projectId":1,"typePrefix":"PROS","parentId":4831,"baseItemTypePredefined":8228,"propertyValues":{"description":{"propertyName":"Description","typePredefined":4099,"typeId":47,"value":""},"label":{"propertyName":"Label","typePredefined":4115,"typeId":63,"value":""},"width":{"propertyName":"Width","typePredefined":8195,"typeId":66,"value":126.0},"height":{"propertyName":"Height","typePredefined":8196,"typeId":67,"value":150.0},"x":{"propertyName":"X","typePredefined":8193,"typeId":64,"value":0.0},"y":{"propertyName":"Y","typePredefined":8194,"typeId":65,"value":0.0},"clientType":{"propertyName":"ClientType","typePredefined":4114,"typeId":62,"value":1},"itemLabel":{"propertyName":"ItemLabel","typePredefined":4102,"typeId":51,"value":""}},"associatedArtifact":null},{"flags":{"hasComments":false,"hasTraces":false},"id":4834,"name":"Precondition","projectId":1,"typePrefix":"PROS","parentId":4831,"baseItemTypePredefined":8228,"propertyValues":{"persona":{"propertyName":"Persona","typePredefined":4130,"typeId":null,"value":"System"},"associatedImageUrl":{"propertyName":"AssociatedImageUrl","typePredefined":0,"typeId":null,"value":null},"imageId":{"propertyName":"ImageId","typePredefined":4132,"typeId":null,"value":null},"storyLinks":{"propertyName":"StoryLinks","typePredefined":4131,"typeId":null,"value":null},"description":{"propertyName":"Description","typePredefined":4099,"typeId":47,"value":""},"label":{"propertyName":"Label","typePredefined":4115,"typeId":63,"value":""},"width":{"propertyName":"Width","typePredefined":8195,"typeId":66,"value":126.0},"height":{"propertyName":"Height","typePredefined":8196,"typeId":67,"value":150.0},"x":{"propertyName":"X","typePredefined":8193,"typeId":64,"value":1.0},"y":{"propertyName":"Y","typePredefined":8194,"typeId":65,"value":0.0},"clientType":{"propertyName":"ClientType","typePredefined":4114,"typeId":62,"value":5},"itemLabel":{"propertyName":"ItemLabel","typePredefined":4102,"typeId":51,"value":""}},"associatedArtifact":null},{"id":4843,"name":"End","projectId":1,"typePrefix":"PROS","parentId":4831,"baseItemTypePredefined":8228,"propertyValues":{"description":{"propertyName":"Description","typePredefined":4099,"typeId":47,"value":""},"label":{"propertyName":"Label","typePredefined":4115,"typeId":63,"value":""},"width":{"propertyName":"Width","typePredefined":8195,"typeId":66,"value":126.0},"height":{"propertyName":"Height","typePredefined":8196,"typeId":67,"value":150.0},"x":{"propertyName":"X","typePredefined":8193,"typeId":64,"value":6.0},"y":{"propertyName":"Y","typePredefined":8194,"typeId":65,"value":0.0},"clientType":{"propertyName":"ClientType","typePredefined":4114,"typeId":62,"value":3},"itemLabel":{"propertyName":"ItemLabel","typePredefined":4102,"typeId":51,"value":""}},"associatedArtifact":null}],"links":[{"sourceId":4900,"destinationId":4903,"orderindex":0.0,"label":null},{"sourceId":4903,"destinationId":4888,"orderindex":0.0,"label":null},{"sourceId":4888,"destinationId":4889,"orderindex":0.0,"label":null},{"sourceId":4889,"destinationId":4843,"orderindex":0.0,"label":null},{"sourceId":4833,"destinationId":4834,"orderindex":0.0,"label":null},{"sourceId":4834,"destinationId":4900,"orderindex":0.0,"label":null}],"decisionBranchDestinationLinks":null,"propertyValues":{"description":{"propertyName":"Description","typePredefined":4099,"typeId":47,"value":""},"clientType":{"propertyName":"ClientType","typePredefined":4114,"typeId":62,"value":2}},"status":{"userId":1,"lockOwnerId":1,"revisionId":2147483647,"isDeleted":false,"isLocked":true,"isLockedByMe":true,"isUnpublished":true,"hasEverBeenPublished":true,"hasReadOnlyReuse":false,"hasReuse":false,"isReadOnly":false,"versionId":2},"requestedVersionInfo":{"artifactId":4831,"utcLockedDateTime":"2016-10-26T22:45:07.007Z","lockOwnerId":null,"lockOwnerLogin":null,"lockOwnerDisplayName":null,"projectId":1,"parentId":null,"orderIndex":null,"versionId":null,"revisionId":null,"baselineId":null,"isVersionInformationProvided":false,"isHeadOrSavedDraftVersion":true}},"tempIdMap":[{"key":-1,"value":4888},{"key":-2,"value":4889}]}');

        $httpBackend.when("PATCH", `/svc/components/storyteller/processes/${processArtifact.id}`)
            .respond(result);

    }));
	
    it("calls both saveProcess() and saveArtifact() methods ", (done) => {

        spyOn(processArtifact, "saveProcess").and.callThrough();

        spyOn(processArtifact, "saveArtifact").and.callFake(() => {
            let deferred = $q.defer();
            deferred.resolve();
            return deferred.promise;
        });
		
        processArtifact.save()
            .then((processArtifact) => {
                // assert 

				// two distinct calls are made to save the process model and 
				// the artifact collections, properties etc.
                expect(processArtifact["saveProcess"]).toHaveBeenCalled();
                expect(processArtifact["saveArtifact"]).toHaveBeenCalled();
            })
            .catch((error) => {
                fail("save process error: " + error);
            })
            .finally(() => {
                done();
            });
		
        $httpBackend.flush();

    });

    it("returns temporary id map after saving ", (done) => {

        spyOn(processArtifact, "saveArtifact").and.callFake(() => {
            let deferred = $q.defer();
            deferred.resolve();
            return deferred.promise;
        });

        spyOn(processArtifact, "mapTempIdsAfterSave").and.callFake((tempIdMap) => {
			// assert
            expect(tempIdMap).toBeDefined();
        });

        processArtifact.save()
            .then((processArtifact) => {
				// a call should be made to map temporary ids to actual ids 
                expect(processArtifact["mapTempIdsAfterSave"]).toHaveBeenCalled();
            })
            .catch((error) => {
                fail("save process error: " + error);
            })
            .finally(() => {
                done();
            });

        $httpBackend.flush();

    });

    it("replaces temporary ids with actual ids after saving ", (done) => {

        spyOn(processArtifact, "mapTempIdsAfterSave").and.callThrough();

        spyOn(processArtifact, "saveArtifact").and.callFake(() => {
            let deferred = $q.defer();
            deferred.resolve();
            return deferred.promise;
        });
        // before save there should be temporary ids (negative integers) 
        // assigned to new shapes 

        let countTempIdsBefore: number = 0;
        processArtifact.subArtifactCollection.list().forEach(item => {
            if (item.id <= 0) {
                countTempIdsBefore++;
            }
        });
        processArtifact.save()
            .then((processArtifact) => {
                  // there should be no temporary ids after saving is done
                let countTempIdsAfter: number = 0;
                let dbLinks = processArtifact["decisionBranchDestinationLinks"];
                if (dbLinks) {
                    dbLinks.forEach((link) => {
                        if (link.destinationId <= 0) {
                            countTempIdsAfter++;
                        }
                        if (link.sourceId <= 0) {
                            countTempIdsAfter++;
                        }
                    });
                }
                let shapes = processArtifact["shapes"];
                for (let sCounter = 0; sCounter < shapes.length; sCounter++) {
                    const shape = shapes[sCounter];
                    if (shape.id <= 0) {
                        countTempIdsAfter++;
                    }
                }
                let links = processArtifact["links"];
                if (links) {
                    links.forEach((link) => {
                        if (link.destinationId <= 0) {
                            countTempIdsAfter++;
                        }
                        if (link.sourceId <= 0) {
                            countTempIdsAfter++;
                        }
                    });
                }
                processArtifact.subArtifactCollection.list().forEach(item => {
                    if (item.id <= 0) {
                        countTempIdsAfter++;
                    }
                });
				// assert 

				// a call should be made to map temporary ids to actual ids 
                expect(processArtifact["mapTempIdsAfterSave"]).toHaveBeenCalled();

				// no temporary ids should remain 
                expect(countTempIdsBefore).toBeGreaterThan(0);
                expect(countTempIdsAfter).toBe(0);
            })
            .catch((error) => {
                fail("save process error: " + error);
            })
            .finally(() => {
                done();
            });

        $httpBackend.flush();

    });

    it("recovers if saveProcess() succeeds and saveArtifact() fails  ", (done) => {

        spyOn(processArtifact, "notifySubscribers").and.callThrough();

        spyOn(processArtifact, "saveArtifact").and.callFake(() => {
            let deferred = $q.defer();
            deferred.reject("save artifact failed");
            return deferred.promise;
        });
        // before save there should be temporary ids (negative integers) 
        // assigned to new shapes
		 
        let countTempIdsBefore: number = 0;
        processArtifact.subArtifactCollection.list().forEach(item => {
            if (item.id <= 0) {
                countTempIdsBefore++;
            }
        });
        processArtifact.save()
            .then((processArtifact) => {
                ; // no-op 
            })
            .catch((error) => {
				// if the process model is saved but the artifact cannot 
                // be saved the process model should be patched with actual
                // ids and no temporary ids should remain 
				 
                let countTempIdsAfter: number = 0;
                processArtifact.subArtifactCollection.list().forEach(item => {
                    if (item.id <= 0) {
                        countTempIdsAfter++;
                    }
                });
				// assert

				// a call should be made to redraw the process diagram 
                expect(processArtifact["notifySubscribers"]).toHaveBeenCalled();

				// the dirty flag should remain true after save fails
                expect(processArtifact.artifactState.dirty).toBe(true);

                // no temporary ids should remain 
                expect(countTempIdsBefore).toBeGreaterThan(0);
                expect(countTempIdsAfter).toBe(0);
            })
            .finally(() => {
                done();
            });

        $httpBackend.flush();

    });
});
