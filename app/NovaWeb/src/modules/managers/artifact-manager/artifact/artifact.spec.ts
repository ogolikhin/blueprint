import "angular-mocks";
import "rx/dist/rx.lite";
import {LocalizationServiceMock} from "../../../commonModule/localization/localization.service.mock";
import {ProcessServiceMock} from "../../../editorsModule/bp-process/services/process.svc.mock";
import {PropertyDescriptorBuilderMock} from "../../../editorsModule/services";
import {IUnpublishedArtifactsService} from "../../../editorsModule/unpublished/unpublished.service";
import {IMessageService} from "../../../main/components/messages/message.svc";
import {MessageServiceMock} from "../../../main/components/messages/message.mock";
import {UnpublishedArtifactsServiceMock} from "../../../editorsModule/unpublished/unpublished.service.mock";
import {Enums, Models} from "../../../main/models";
import {IStatefulArtifactFactory, MetaDataService, StatefulArtifactFactory} from "../../../managers/artifact-manager";
import {DialogServiceMock} from "../../../shared/widgets/bp-dialog/bp-dialog.mock";
import {SelectionManager} from "../../selection-manager/selection-manager";
import {ArtifactAttachmentsMock} from "../attachments/attachments.svc.mock";
import {ArtifactRelationshipsMock} from "../relationships/relationships.svc.mock";
import {ValidationServiceMock} from "../validation/validation.mock";
import {IStatefulArtifact} from "./artifact";
import {ArtifactServiceMock} from "./artifact.svc.mock";
import * as angular from "angular";
import {HttpStatusCode} from "../../../commonModule/httpInterceptor/http-status-code";
import {ApplicationError} from "../../../shell/error/applicationError";
import {MessageType} from "../../../main/components/messages/message";
import {ErrorCode} from "../../../shell/error/error-code";
import {LoadingOverlayServiceMock} from "../../../commonModule/loadingOverlay/loadingOverlay.service.mock";
import {ItemInfoServiceMock} from "../../../commonModule/itemInfo/itemInfo.service.mock";
import {SessionSvcMock} from "../../../shell/login/session.svc.mock";

describe("Artifact", () => {
    let artifact: IStatefulArtifact;
    let $q: ng.IQService;
    let validateSpy: jasmine.Spy;
    //beforeEach(angular.mock.module("app.shell"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("artifactRelationships", ArtifactRelationshipsMock);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("dialogService", DialogServiceMock);
        $provide.service("selectionManager", SelectionManager);
        $provide.service("messageService", MessageServiceMock);
        $provide.service("artifactService", ArtifactServiceMock);
        $provide.service("artifactAttachments", ArtifactAttachmentsMock);
        $provide.service("metadataService", MetaDataService);
        $provide.service("statefulArtifactFactory", StatefulArtifactFactory);
        $provide.service("publishService", UnpublishedArtifactsServiceMock);
        $provide.service("validationService", ValidationServiceMock);
        $provide.service("propertyDescriptorBuilder", PropertyDescriptorBuilderMock);
        $provide.service("session", SessionSvcMock);
        $provide.service("itemInfoService", ItemInfoServiceMock);
        $provide.service("loadingOverlayService", LoadingOverlayServiceMock);
    }));

    beforeEach(inject((
        _$q_: ng.IQService,
        statefulArtifactFactory: IStatefulArtifactFactory) => {
        // arrange
        $q = _$q_;
        let artifactModel = {
            id: 22,
            name: "Artifact",
            prefix: "My",
            lockedByUser: Enums.LockedByEnum.None, //Enums.LockedByEnum.CurrentUser,
            predefinedType: Models.ItemTypePredefined.Actor,
            version: 0
        } as Models.IArtifact;
        artifact = statefulArtifactFactory.createStatefulArtifact(artifactModel);
        validateSpy = spyOn(artifact, "validate").and.callFake(() => {
            return $q.resolve();
        });
        spyOn(artifact, "canBeSaved").and.callFake(() => {
            return true;
        });

    }));

    describe("canBeSaved", () => {
        it("locked by current user", inject(() => {
            // arrange
            let newStateValues = {
                lockDateTime: new Date(),
                lockedBy: Enums.LockedByEnum.CurrentUser,
                lockOwner: "Default Instance Admin",
                dirty: true
            };
            artifact.artifactState.setState(newStateValues, false);

            // act
            let result: boolean;
            result = artifact.canBeSaved();

            // assert
            expect(result).toEqual(true);
        }));
    });

    describe("canBepublished", () => {
        it("locked by current user", inject(() => {
            // arrange
            let newStateValues = {
                lockDateTime: new Date(),
                lockedBy: Enums.LockedByEnum.CurrentUser,
                lockOwner: "Default Instance Admin"
            };
            artifact.artifactState.setState(newStateValues, false);

            // act
            let result: boolean;
            result = artifact.canBePublished();

            // assert
            expect(result).toEqual(true);
        }));
    });

    describe("Save", () => {

        it("success", inject(($rootScope: ng.IRootScopeService) => {
            // arrange

            // act
            let returnedArtifact: IStatefulArtifact;
            artifact.save().then((result) => {
                returnedArtifact = result;
            });
            $rootScope.$digest();

            // assert
            expect(returnedArtifact).toBeDefined();
        }));

        xit("success (skip validation)", inject(($rootScope: ng.IRootScopeService) => {
            // arrange

            // act
            let returnedArtifact: IStatefulArtifact;
            artifact.save(true).then((result) => {
                returnedArtifact = result;
            });
            const refreshSpy = spyOn(artifact, "refresh").and.callFake(() => {
                return $q.resolve();
            });
            $rootScope.$digest();

            // assert
            expect(returnedArtifact).toBeDefined();
            expect(validateSpy).toHaveBeenCalledTimes(0);
            expect(refreshSpy).toHaveBeenCalledTimes(0);
        }));

        it("failed (invalid)", inject(($rootScope: ng.IRootScopeService) => {
            // arrange
            validateSpy.and.callFake(() => {
                return $q.reject(new Error());
            });
            // act

            let returnedArtifact: IStatefulArtifact;
            let error: any;
            artifact.save().then((result) => {
                returnedArtifact = result;
            }).catch((err) => {
                error = err;
            });
            $rootScope.$digest();

            // assert
            expect(returnedArtifact).toBeUndefined();
            expect(error).toBeDefined();

        }));

        xit("error no changes", inject(($rootScope: ng.IRootScopeService) => {
            // arrange
            spyOn(artifact, "changes").and.callFake(() => {
                return null;
            });

            // act
            let error: Error;
            artifact.save().catch((err) => {
                error = err;
            });
            $rootScope.$digest();

            // assert
            expect(error.message).toEqual("App_Save_Artifact_Error_400_114");
        }));

        it("error 400 114", inject(($rootScope: ng.IRootScopeService, artifactService: ArtifactServiceMock, $q: ng.IQService) => {
            // arrange
            spyOn(artifactService, "updateArtifact").and.callFake(() => {
                const deferred = $q.defer<any>();
                deferred.reject({
                    statusCode: 400,
                    errorCode: 114
                });
                return deferred.promise;
            });

            // act
            let error: Error;
            artifact.save().catch((err) => {
                error = err;
            });
            $rootScope.$digest();
            // assert
            expect(error.message).toEqual("App_Save_Artifact_Error_400_114");
        }));

        it("error notfound", inject(($rootScope: ng.IRootScopeService, artifactService: ArtifactServiceMock, $q: ng.IQService) => {
            // arrange
            spyOn(artifactService, "updateArtifact").and.callFake(() => {
                const deferred = $q.defer<any>();
                deferred.reject({
                    statusCode: HttpStatusCode.NotFound
                });
                return deferred.promise;
            });

            // act
            let error: Error;
            artifact.save().catch((err) => {
                error = err;
            });
            $rootScope.$digest();

            // assert
            expect(error.message).toEqual("App_Save_Artifact_Error_404");
        }));

        it("error 409 116", inject(($rootScope: ng.IRootScopeService, artifactService: ArtifactServiceMock, $q: ng.IQService) => {
            // arrange
            spyOn(artifactService, "updateArtifact").and.callFake(() => {
                const deferred = $q.defer<any>();
                deferred.reject({
                    statusCode: HttpStatusCode.Conflict,
                    errorCode: 116
                });
                return deferred.promise;
            });

            // act
            let error: Error;
            artifact.save().catch((err) => {
                error = err;
            });
            $rootScope.$digest();

            // assert
            expect(error.message).toEqual("App_Save_Artifact_Error_409_116");
        }));

        it("error 409 117", inject(($rootScope: ng.IRootScopeService, artifactService: ArtifactServiceMock, $q: ng.IQService) => {
            // arrange
            spyOn(artifactService, "updateArtifact").and.callFake(() => {
                const deferred = $q.defer<any>();
                deferred.reject({
                    statusCode: HttpStatusCode.Conflict,
                    errorCode: 117
                });
                return deferred.promise;
            });

            // act
            let error: Error;
            artifact.save().catch((err) => {
                error = err;
            });
            $rootScope.$digest();

            // assert
            expect(error.message).toEqual("App_Save_Artifact_Error_409_117");
        }));

        it("error 409 115", inject(($rootScope: ng.IRootScopeService, artifactService: ArtifactServiceMock, $q: ng.IQService) => {
            // arrange
            spyOn(artifactService, "updateArtifact").and.callFake(() => {
                const deferred = $q.defer<any>();
                deferred.reject({
                    statusCode: HttpStatusCode.Conflict,
                    errorCode: 115
                });
                return deferred.promise;
            });

            // act
            let error: Error;
            artifact.save().catch((err) => {
                error = err;
            });
            $rootScope.$digest();

            // assert
            expect(error.message).toEqual("App_Save_Artifact_Error_409_115");
        }));

        it("error 409 123", inject(($rootScope: ng.IRootScopeService, artifactService: ArtifactServiceMock, $q: ng.IQService) => {
            // arrange
            spyOn(artifactService, "updateArtifact").and.callFake(() => {
                const deferred = $q.defer<any>();
                deferred.reject({
                    statusCode: HttpStatusCode.Conflict,
                    errorCode: 123
                });
                return deferred.promise;
            });

            // act
            let error: Error;
            artifact.save().catch((err) => {
                error = err;
            });
            $rootScope.$digest();

            // assert
            expect(error.message).toEqual("App_Save_Artifact_Error_409_123");
        }));

        it("error 409 124", inject(($rootScope: ng.IRootScopeService, artifactService: ArtifactServiceMock, $q: ng.IQService) => {
            // arrange
            spyOn(artifactService, "updateArtifact").and.callFake(() => {
                const deferred = $q.defer<any>();
                deferred.reject({
                    statusCode: HttpStatusCode.Conflict,
                    errorCode: 124
                });
                return deferred.promise;
            });

            // act
            let error: Error;
            artifact.save().catch((err) => {
                error = err;
            });
            $rootScope.$digest();

            // assert
            expect(error.message).toEqual("App_Save_Artifact_Error_409_124");
        }));

        it("error 409 other", inject(($rootScope: ng.IRootScopeService, artifactService: ArtifactServiceMock, $q: ng.IQService) => {
            // arrange
            spyOn(artifactService, "updateArtifact").and.callFake(() => {
                const deferred = $q.defer<any>();
                deferred.reject({
                    statusCode: HttpStatusCode.Conflict
                });
                return deferred.promise;
            });

            // act
            let error: Error;
            artifact.save().catch((err) => {
                error = err;
            });
            $rootScope.$digest();

            // assert
            expect(error.message).toEqual("App_Save_Artifact_Error_409");
        }));

        it("error other", inject(($rootScope: ng.IRootScopeService, artifactService: ArtifactServiceMock, $q: ng.IQService) => {
            // arrange
            spyOn(artifactService, "updateArtifact").and.callFake(() => {
                const deferred = $q.defer<any>();
                deferred.reject({
                    statusCode: HttpStatusCode.ServerError
                });
                return deferred.promise;
            });

            // act
            let error: Error;
            artifact.save().catch((err) => {
                error = err;
            });
            $rootScope.$digest();

            // assert
            expect(error.message).toEqual("App_Save_Artifact_Error_Other" + HttpStatusCode.ServerError);
        }));

        it("updateArtifact is called with process save url and contains process model",
            inject(($rootScope: ng.IRootScopeService, artifactService: ArtifactServiceMock, $q: ng.IQService) => {

            let url: string;
            let changes: Models.IArtifact;
            const updateSpy = spyOn(artifactService, "updateArtifact").and.callFake((_url, _changes) => {
                url = _url;
                changes = _changes;
                return $q.when(artifact);
            });

            artifact.save();
            $rootScope.$digest();

            expect(url).toBe("/svc/bpartifactstore/artifacts/22");
            expect(changes).toBeDefined();
        }));
    });

    //TODO: move to artifact-mamager.spec
    xdescribe("Autosave", () => {

        it("autosave calls save with flag to ignore validation", () => {
            // arrange
            const saveSpy = spyOn(artifact, "save").and.returnValue($q.when());
            const newStateValues = {
                lockDateTime: new Date(),
                lockedBy: Enums.LockedByEnum.CurrentUser,
                lockOwner: "Default Instance Admin",
                dirty: true
            };
            artifact.artifactState.setState(newStateValues, false);

            // act
            //artifact.autosave();

            // assert
            expect(saveSpy).toHaveBeenCalledWith(true);
            expect(validateSpy).not.toHaveBeenCalled();
        });

    });

    describe("Publish", () => {
        it("success", inject(($rootScope: ng.IRootScopeService, messageService: IMessageService) => {
            // arrange

            // act
            artifact.publish();
            $rootScope.$digest();

            // assert
            expect(artifact.artifactState.lockedBy).toEqual(Enums.LockedByEnum.None);
            expect(messageService.messages.length).toEqual(1);
            expect(messageService.messages[0].messageType).toEqual(MessageType.Info);
        }));

        it("with failed save", inject(($rootScope: ng.IRootScopeService, artifactService: ArtifactServiceMock, $q: ng.IQService) => {
            // arrange
            let newStateValues = {
                lockDateTime: new Date(),
                lockedBy: Enums.LockedByEnum.CurrentUser,
                lockOwner: "Default Instance Admin",
                dirty: true
            };
            artifact.artifactState.setState(newStateValues, false);
            spyOn(artifactService, "updateArtifact").and.callFake(() => {
                const deferred = $q.defer<any>();
                deferred.reject({
                    statusCode: HttpStatusCode.Conflict
                });
                return deferred.promise;
            });

            // act
            let error: ApplicationError;
            artifact.publish().catch((err) => {
                error = err;
            });
            $rootScope.$digest();

            // assert
            expect(error.message).toEqual("App_Save_Artifact_Error_409");
        }));

        it("publish dependents success", inject((publishService: IUnpublishedArtifactsService, $rootScope: ng.IRootScopeService,
                                                 messageService: IMessageService, $q: ng.IQService) => {
            // arrange
            spyOn(publishService, "publishArtifacts").and.callFake((artifactIds: number[]) => {
                let defer = $q.defer<Models.IPublishResultSet>();

                if (artifactIds[0] === 22) {
                    defer.reject({
                        errorContent: {
                            artifacts: [
                                {
                                    id: 2,
                                    projectId: 1
                                }
                            ],
                            projects: [
                                {
                                    id: 1
                                }
                            ]
                        },
                        statusCode: HttpStatusCode.Conflict

                    });
                } else if (artifactIds[0] === 2) {
                    defer.resolve();
                } else {
                    defer.reject();
                }
                return defer.promise;
            });

            // act
            artifact.publish();
            $rootScope.$digest();

            // assert
            expect(artifact.artifactState.lockedBy).toEqual(Enums.LockedByEnum.None);
            expect(messageService.messages.length).toEqual(1);
            expect(messageService.messages[0].messageType).toEqual(MessageType.Info);
        }));

        it("publish dependents error", inject((publishService: IUnpublishedArtifactsService, $rootScope: ng.IRootScopeService,
                                               messageService: IMessageService, $q: ng.IQService) => {
            // arrange
            spyOn(publishService, "publishArtifacts").and.callFake((artifactIds: number[]) => {
                let defer = $q.defer<Models.IPublishResultSet>();
                defer.reject({
                    errorContent: {
                        artifacts: [
                            {
                                id: 2,
                                projectId: 1
                            }
                        ],
                        projects: [
                            {
                                id: 1
                            }
                        ]
                    },
                    statusCode: HttpStatusCode.Conflict

                });
                return defer.promise;
            });

            // act
            artifact.publish();
            $rootScope.$digest();

            // assert
            expect(messageService.messages.length).toEqual(1);
            expect(messageService.messages[0].messageType).toEqual(MessageType.Error);
        }));
    });

    describe("Discard", () => {
        it("success", inject(($rootScope: ng.IRootScopeService, messageService: IMessageService) => {
            // arrange

            // act
            artifact.discardArtifact();
            $rootScope.$digest();

            // assert
            expect(artifact.artifactState.lockedBy).toEqual(Enums.LockedByEnum.None);
            expect(messageService.messages.length).toEqual(1);
            expect(messageService.messages[0].messageType).toEqual(MessageType.Info);
        }));

        it("discard dependents success", inject((publishService: IUnpublishedArtifactsService, $rootScope: ng.IRootScopeService,
                                                 messageService: IMessageService, $q: ng.IQService) => {
            // arrange
            spyOn(publishService, "discardArtifacts").and.callFake((artifactIds: number[]) => {
                let defer = $q.defer<Models.IPublishResultSet>();

                if (artifactIds[0] === 22) {
                    defer.reject({
                        errorContent: {
                            artifacts: [
                                {
                                    id: 2,
                                    projectId: 1
                                }
                            ],
                            projects: [
                                {
                                    id: 1
                                }
                            ]
                        },
                        statusCode: HttpStatusCode.Conflict

                    });
                } else if (artifactIds[0] === 2) {
                    defer.resolve();
                } else {
                    defer.reject();
                }
                return defer.promise;
            });

            // act
            artifact.discardArtifact();
            $rootScope.$digest();

            // assert
            expect(artifact.artifactState.lockedBy).toEqual(Enums.LockedByEnum.None);
            expect(messageService.messages.length).toEqual(1);
            expect(messageService.messages[0].messageType).toEqual(MessageType.Info);
        }));

        it("discard dependents error", inject((publishService: IUnpublishedArtifactsService, $rootScope: ng.IRootScopeService,
                                               messageService: IMessageService, $q: ng.IQService) => {
            // arrange
            spyOn(publishService, "discardArtifacts").and.callFake((artifactIds: number[]) => {
                let defer = $q.defer<Models.IPublishResultSet>();
                defer.reject({
                    errorContent: {
                        artifacts: [
                            {
                                id: 2,
                                projectId: 1
                            }
                        ],
                        projects: [
                            {
                                id: 1
                            }
                        ]
                    },
                    statusCode: HttpStatusCode.Conflict

                });
                return defer.promise;
            });

            // act
            artifact.discardArtifact();
            $rootScope.$digest();

            // assert
            expect(messageService.messages.length).toEqual(1);
            expect(messageService.messages[0].messageType).toEqual(MessageType.Error);
        }));

        it("discards changes and resolves promise when no changes error is thrown from server",
            inject((publishService: IUnpublishedArtifactsService, $rootScope: ng.IRootScopeService,
                                               messageService: IMessageService, $q: ng.IQService) => {
            // arrange
            spyOn(publishService, "discardArtifacts").and.callFake((artifactIds: number[]) => {
                let defer = $q.defer<Models.IPublishResultSet>();
                defer.reject({
                    errorContent: undefined,
                    statusCode: HttpStatusCode.Conflict,
                    errorCode: ErrorCode.NoChanges
                });
                return defer.promise;
            });
            const spyDiscard = spyOn(artifact, "discard").and.callThrough();

            // act
            let isResolved: boolean = false;
            artifact.discardArtifact().then(() => {
                isResolved = true;
            });
            $rootScope.$digest();

            // assert
            expect(messageService.messages.length).toEqual(1);
            expect(spyDiscard).toHaveBeenCalled();
            expect(isResolved).toBe(true);
        }));
    });

    describe("Lock", () => {
        it("success", inject(($rootScope: ng.IRootScopeService) => {
            // arrange
            // act
            artifact.lock();
            $rootScope.$digest();

            // assert
            expect(artifact.artifactState.lockedBy).toEqual(Enums.LockedByEnum.CurrentUser);
        }));

        it("success - does not call subscriber's onNext to notify artifact change", inject(($rootScope: ng.IRootScopeService) => {
            // arrange
            const spyOnNext = spyOn((<any>artifact).subject, "onNext");

            // act
            artifact.lock();
            $rootScope.$digest();

            // assert
            expect(spyOnNext).not.toHaveBeenCalled();
        }));

        it("wrong version failure", inject((statefulArtifactFactory: IStatefulArtifactFactory,
                                            $rootScope: ng.IRootScopeService, messageService: IMessageService) => {
            // arrange
            let artifactModel = {
                id: 22,
                name: "Artifact",
                prefix: "My",
                lockedByUser: Enums.LockedByEnum.None, //Enums.LockedByEnum.CurrentUser,
                predefinedType: Models.ItemTypePredefined.Actor,
                version: 1
            } as Models.IArtifact;
            artifact = statefulArtifactFactory.createStatefulArtifact(artifactModel);

            // act
            artifact.lock();
            $rootScope.$digest();

            // assert
            expect(messageService.messages.length).toEqual(1);
            expect(messageService.messages[0].messageType).toEqual(MessageType.Info);
            expect(messageService.messages[0].messageText).toEqual("Artifact_Lock_Refresh");
        }));

        it("wrong version failure - calls subscriber's onNext to notify artifact change", inject((statefulArtifactFactory: IStatefulArtifactFactory,
                                            $rootScope: ng.IRootScopeService, messageService: IMessageService) => {
            // arrange
            let artifactModel = {
                id: 22,
                name: "Artifact",
                prefix: "My",
                lockedByUser: Enums.LockedByEnum.None, //Enums.LockedByEnum.CurrentUser,
                predefinedType: Models.ItemTypePredefined.Actor,
                version: 1
            } as Models.IArtifact;

            artifact = statefulArtifactFactory.createStatefulArtifact(artifactModel);

            const spyOnNext = spyOn((<any>artifact).subject, "onNext");

            // act
            artifact.lock();
            $rootScope.$digest();

            // assert
            expect(spyOnNext).toHaveBeenCalled();
            expect(spyOnNext).toHaveBeenCalledTimes(1);
        }));

        it("service error already locked ignore failure", inject(($rootScope: ng.IRootScopeService,
                                                                  artifactService: ArtifactServiceMock, $q: ng.IQService) => {
            // arrange

            spyOn(artifactService, "lock").and.callFake(() => {
                const deferred = $q.defer<any>();
                let data = {
                    result: Enums.LockResultEnum.AlreadyLocked,
                    info: {
                        versionId: 0
                    }
                } as Models.ILockResult;
                deferred.resolve([data]);
                return deferred.promise;
            });

            // act
            let returnedArtifact: IStatefulArtifact;
            artifact.lock().then((item) => {
                returnedArtifact = item;
            });
            $rootScope.$digest();

            // assert
            expect(returnedArtifact).toBeDefined();
        }));

        it("service error not found failure", inject(($rootScope: ng.IRootScopeService,
                                                      artifactService: ArtifactServiceMock, $q: ng.IQService) => {
            // arrange

            spyOn(artifactService, "lock").and.callFake(() => {
                const deferred = $q.defer<any>();
                let data = {
                    result: Enums.LockResultEnum.DoesNotExist,
                    info: {
                        versionId: 0
                    }
                } as Models.ILockResult;
                deferred.resolve([data]);
                return deferred.promise;
            });

            // act
            let returnedArtifact: IStatefulArtifact;
            artifact.lock().then((item) => {
                returnedArtifact = item;
            });
            $rootScope.$digest();

            // assert
            expect(returnedArtifact).toBeDefined();
            expect(returnedArtifact.artifactState.deleted).toEqual(true);
        }));
    });

    describe("Delete", () => {
        it("success", inject(($rootScope: ng.IRootScopeService, artifactService: ArtifactServiceMock, $q: ng.IQService ) => {
            // arrange
            // act
                let error: ApplicationError;
                artifact.errorObservable().subscribeOnNext((err: ApplicationError) => {
                    error = err;
                });

            spyOn(artifactService, "deleteArtifact").and.callFake(() => {
                const deferred = $q.defer<any>();
                deferred.resolve([{
                    id: 1, name: "TEST"
                }]);
                return deferred.promise;
            });
            spyOn(artifact, "discard").and.callThrough();
            let result;
            artifact.delete().then((it) => {
                result = it;
            });
            $rootScope.$digest();
            // assert
            expect(result).toBeDefined();
            expect(result).toEqual(jasmine.any(Array));
            expect(error).toBeUndefined();
            expect(artifact.discard).toHaveBeenCalled();
            expect(artifact.artifactState.deleted).toBeTruthy();
        }));

        it("failed", inject(($rootScope: ng.IRootScopeService, artifactService: ArtifactServiceMock, $q: ng.IQService) => {
            // arrange
            let error: ApplicationError;
            artifact.errorObservable().subscribeOnNext((err: ApplicationError) => {
                error = err;
            });
            spyOn(artifactService, "deleteArtifact").and.callFake(() => {
                const deferred = $q.defer<any>();
                deferred.reject({
                    statusCode: HttpStatusCode.Conflict
                });
                return deferred.promise;
            });

            // act

            artifact.delete();
            $rootScope.$digest();

            // assert
            expect(error.statusCode).toEqual( HttpStatusCode.Conflict);
            expect(artifact.artifactState.deleted).toBeFalsy();
        }));

        it("failed, test error message", inject(($rootScope: ng.IRootScopeService, artifactService: ArtifactServiceMock, $q: ng.IQService) => {
            // arrange
            let error: ApplicationError;
            artifact.errorObservable().subscribeOnNext((err: ApplicationError) => {
                error = err;
            });
            spyOn(artifactService, "deleteArtifact").and.callFake(() => {
                const deferred = $q.defer<any>();
                deferred.reject({
                    statusCode: HttpStatusCode.Conflict,
                    errorContent : {
                        id: 222,
                        name: "TEST",
                        prefix: "PREFIX"
                    }
                });
                return deferred.promise;
            });
            const errormessage = "The artifact PREFIX222 is already locked by another user.";
            // act

            artifact.delete();
            $rootScope.$digest();

             // assert
             expect(error.statusCode).toEqual( HttpStatusCode.Conflict);
             expect(error.message).toEqual(errormessage);
             expect(artifact.artifactState.deleted).toBeFalsy();
         }));
    });
    describe("move", () => {
        it("success", inject(($rootScope: ng.IRootScopeService, artifactService: ArtifactServiceMock, $q: ng.IQService ) => {
            // arrange
            let error: ApplicationError;
            artifact.errorObservable().subscribeOnNext((err: ApplicationError) => {
                error = err;
            });
            const newParentId: number = 3;
            const newOrderIndex: number = 15;
            const expectedResult = [{
                    id: 1, name: "TEST", parentId: newParentId, orderIndex: newOrderIndex
                }];
            spyOn(artifactService, "moveArtifact").and.callFake(() => {
                const deferred = $q.defer<any>();
                deferred.resolve(expectedResult);
                return deferred.promise;
            });
            // act
            let result;
            artifact.move(newParentId, newOrderIndex).then((it) => {
                result = it;
            });
            $rootScope.$digest();
            // assert
            expect(result).toEqual(expectedResult);
            expect(error).toBeUndefined();
        }));

        it("failed", inject(($rootScope: ng.IRootScopeService, artifactService: ArtifactServiceMock, $q: ng.IQService) => {
            // arrange
            spyOn(artifactService, "moveArtifact").and.callFake(() => {
                const deferred = $q.defer<any>();
                deferred.reject({
                    statusCode: HttpStatusCode.Conflict
                });
                return deferred.promise;
            });
            const newParentId: number = 3;

            // act
            let error: ApplicationError;
            artifact.move(newParentId).catch((err) => { error = err; });
            $rootScope.$digest();
            // assert
            expect(error.statusCode).toEqual(HttpStatusCode.Conflict);
        }));
    });

    describe("copy", () => {
        it("success", inject(($rootScope: ng.IRootScopeService, artifactService: ArtifactServiceMock, $q: ng.IQService ) => {
            // arrange
            let error: ApplicationError;
            artifact.errorObservable().subscribeOnNext((err: ApplicationError) => {
                error = err;
            });
            const newParentId: number = 3;
            const newOrderIndex: number = 15;
            const expectedResult = [{
                    id: 1, name: "TEST", parentId: newParentId, orderIndex: newOrderIndex
                }];
            spyOn(artifactService, "copyArtifact").and.callFake(() => {
                const deferred = $q.defer<any>();
                deferred.resolve(expectedResult);
                return deferred.promise;
            });
            // act
            let result;
            artifact.copy(newParentId, newOrderIndex).then((it) => {
                result = it;
            });
            $rootScope.$digest();
            // assert
            expect(result).toEqual(expectedResult);
            expect(error).toBeUndefined();
        }));

        it("failed", inject(($rootScope: ng.IRootScopeService, artifactService: ArtifactServiceMock, $q: ng.IQService) => {
            // arrange
            spyOn(artifactService, "copyArtifact").and.callFake(() => {
                const deferred = $q.defer<any>();
                deferred.reject({
                    statusCode: HttpStatusCode.Conflict
                });
                return deferred.promise;
            });
            const newParentId: number = 3;

            // act
            let error: ApplicationError;
            artifact.copy(newParentId).catch((err) => { error = err; });
            $rootScope.$digest();
            // assert
            expect(error.statusCode).toEqual(HttpStatusCode.Conflict);
        }));
    });

    describe("Load", () => {
        it("error is deleted", inject(($rootScope: ng.IRootScopeService) => {
            // arrange
            spyOn(artifact, "isHeadVersionDeleted").and.callFake(() => {
                return true;
            });

            // act
            let error: ApplicationError;
            artifact.save().catch((err) => {
                error = err;
            });
            $rootScope.$digest();

            // assert
            expect(error.statusCode).toEqual(HttpStatusCode.NotFound);
        }));

        it("error not found", inject(($rootScope: ng.IRootScopeService, artifactService: ArtifactServiceMock, $q: ng.IQService) => {
            // arrange
            spyOn(artifactService, "getArtifact").and.callFake(() => {
                const deferred = $q.defer<any>();
                deferred.reject({
                    statusCode: HttpStatusCode.NotFound
                });
                return deferred.promise;
            });

            // act
            let error: ApplicationError;
            artifact.save().catch((err) => {
                error = err;
            });
            $rootScope.$digest();

            // assert
            expect(error.statusCode).toEqual(HttpStatusCode.NotFound);
            expect(artifact.artifactState.deleted).toEqual(true);
        }));
    });
});
