import * as angular from "angular";
import "angular-mocks";
import "../../../shell";
import {LocalizationServiceMock} from "../../../core/localization/localization.mock";
import {Models, Enums} from "../../../main/models";
import {IPublishService} from "./../publish.svc/publish.svc";
import {PublishServiceMock} from "./../publish.svc/publish.svc.mock";
import {IStatefulArtifact} from "./artifact";
import {ArtifactRelationshipsMock} from "./../../../managers/artifact-manager/relationships/relationships.svc.mock";
import {ArtifactAttachmentsMock} from "./../../../managers/artifact-manager/attachments/attachments.svc.mock";
import {ArtifactServiceMock} from "./../../../managers/artifact-manager/artifact/artifact.svc.mock";
import {DialogServiceMock} from "../../../shared/widgets/bp-dialog/bp-dialog";
import {ProcessServiceMock} from "../../../editors/bp-process/services/process.svc.mock";
import {SelectionManager} from "./../../../managers/selection-manager/selection-manager";
import {MessageServiceMock} from "../../../core/messages/message.mock";
import {PropertyDescriptorBuilderMock} from "../../../editors/configuration/property-descriptor-builder.mock";
import {
    ArtifactManager,
    IStatefulArtifactFactory,
    StatefulArtifactFactory,
    MetaDataService
} from "../../../managers/artifact-manager";
import {HttpStatusCode} from "../../../core/http/http-status-code";
import {IMessageService} from "../../../core/messages/message.svc";
import {MessageType} from "../../../core/messages/message";
import {ApplicationError} from "../../../core/error/applicationError";
import {ValidationServiceMock} from "../../../managers/artifact-manager/validation/validation.mock";

describe("Artifact", () => {
    let artifact: IStatefulArtifact;
    let $q: ng.IQService;
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
        $provide.service("publishService", PublishServiceMock);
        $provide.service("validationService", ValidationServiceMock);
        $provide.service("propertyDescriptorBuilder", PropertyDescriptorBuilderMock);
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
        spyOn(artifact, "validate").and.callFake(() => {
                return $q.resolve();
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
            expect(error.message).toEqual("App_Save_Artifact_Error_409_123");
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

        it("publish dependents success", inject((publishService: IPublishService, $rootScope: ng.IRootScopeService,
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

        it("publish dependents error", inject((publishService: IPublishService, $rootScope: ng.IRootScopeService,
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

        it("discard dependents success", inject((publishService: IPublishService, $rootScope: ng.IRootScopeService,
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

        it("discard dependents error", inject((publishService: IPublishService, $rootScope: ng.IRootScopeService,
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
            let result;
           artifact.delete().then((it) => {
               result = it;
           });
           $rootScope.$digest();
          // assert
           expect(result).toBeDefined();
           expect(result).toEqual(jasmine.any(Array));
           expect(error).toBeUndefined();
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


    describe("refresh", () => {
        it("invokes custom refresh if allowed", inject(() => {
            // arrange
            const customRefreshSpy = spyOn(artifact, "getCustomArtifactPromisesForRefresh");

            // act
            artifact.refresh();

            // assert
            expect(customRefreshSpy).toHaveBeenCalled();
        }));

        it("doesn't invoke custom refresh if not allowed", inject(() => {
            // arrange
            const customRefreshSpy = spyOn(artifact, "getCustomArtifactPromisesForRefresh");

            // act
            artifact.refresh(false);

            // assert
            expect(customRefreshSpy).not.toHaveBeenCalled();
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
