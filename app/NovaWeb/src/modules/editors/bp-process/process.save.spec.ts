import * as angular from "angular";
require("script!mxClient");
import "rx/dist/rx.lite";
import { Models, Enums } from "../../main/models";
import { IProcess } from "./models/process-models";
import { IArtifactService } from "../../managers/artifact-manager/";
import { ArtifactServiceMock } from "../../managers/artifact-manager/artifact/artifact.svc.mock";
import { ValidationServiceMock } from  "../../managers/artifact-manager/validation/validation.mock";
import { IValidationService } from  "../../managers/artifact-manager/validation/validation.svc";
import { ISession } from "../../shell/login/session.svc";
import { SessionSvcMock } from "../../shell/login//mocks.spec";
import { IMessageService } from "../../core/messages/message.svc";
import { MessageServiceMock } from "../../core/messages/message.mock";
import { LocalizationServiceMock } from "../../core/localization/localization.mock";
import { IProcessService, ProcessService } from "./services/process.svc";
import { IProcessUpdateResult } from "./services/process.svc";
import { IStatefulProcessArtifactServices } from "../../managers/artifact-manager/services";
import { StatefulArtifactServices } from "../../managers/artifact-manager/services";
import { StatefulProcessArtifactServices } from "../../managers/artifact-manager/services";
import { StatefulProcessArtifact } from "./process-artifact";
import { StatefulProcessSubArtifact } from "./process-subartifact";
import { IStatefulSubArtifact } from "../../managers/artifact-manager/sub-artifact/sub-artifact";
import * as TestModels from "./models/test-model-factory";
import { MetaDataService } from "../../managers/artifact-manager";

class ExecutionEnvironmentDetectorMock {
    private browserInfo: any;

    constructor() {
        this.browserInfo = { msie: false, firefox: false, version: 0 };
    }

    public getBrowserInfo(): any {
        return this.browserInfo;
    }
}

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

    let _window: any = window;
    _window.executionEnvironmentDetector = ExecutionEnvironmentDetectorMock;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("artifactService", ArtifactServiceMock);
        $provide.service("session", null);
        $provide.service("processService", ProcessService);
        $provide.service("messageService", MessageServiceMock);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("dialogService", null);
        $provide.service("attachmentService", null);
        $provide.service("relationshipsService", null);
        $provide.service("projectManager", null);
        $provide.service("metadataService", MetaDataService);
        $provide.service("publishService", null);
        $provide.service("validationService", ValidationServiceMock);
    }));
    beforeEach(inject((
        _$rootScope_: ng.IRootScopeService,
        _$q_: ng.IQService,
        _$httpBackend_: ng.IHttpBackendService,
        messageService: IMessageService, 
        artifactService: IArtifactService,
        processService: IProcessService,
        validationService: IValidationService,
        metadataService: MetaDataService,
        localization: LocalizationServiceMock) => {

        $rootScope = _$rootScope_;
        $q = _$q_;
        $httpBackend = _$httpBackend_;

        session = new SessionSvcMock($q);

        processModel = JSON.parse(require("./mocks/process-model-1.mock.json")); 
        
        const artifactServices = new StatefulArtifactServices(
            _$q_, session, messageService, null, localization, artifactService, null, null, metadataService, null, null, validationService);

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

        let newStateValues = {
            lockDateTime: new Date(),
            lockedBy: Enums.LockedByEnum.CurrentUser,
            lockOwner: "Default Instance Admin",
            readonly: false,
            dirty: true
        };

        processArtifact.artifactState.setState(newStateValues, false);

        // Setup the data we wish to return for the http call  
        result = JSON.parse(require("./mocks/process-model-2.mock.json"));

        $httpBackend.when("PATCH", `/svc/components/storyteller/processes/${processArtifact.id}`)
            .respond(result);

    }));

    it("calls both saveProcess() and saveArtifact() methods ", (done) => {
        spyOn(services.metaDataService, "getArtifactPropertyTypes").and.callFake(() => {
            const deferred = $q.defer();
            deferred.resolve();
            return deferred.promise;
        });
        spyOn(processArtifact, "saveProcess").and.callThrough();

        spyOn(processArtifact, "saveArtifact").and.callFake(() => {
            const deferred = $q.defer();
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
        spyOn(services.metaDataService, "getArtifactPropertyTypes").and.callFake(() => {
            const deferred = $q.defer();
            deferred.resolve();
            return deferred.promise;
        });

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
        spyOn(services.metaDataService, "getArtifactPropertyTypes").and.callFake(() => {
            const deferred = $q.defer();
            deferred.resolve();
            return deferred.promise;
        });

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
        spyOn(services.metaDataService, "getArtifactPropertyTypes").and.callFake(() => {
            const deferred = $q.defer();
            deferred.resolve();
            return deferred.promise;
        });
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
