import "angular-mocks";
import "rx/dist/rx.lite";
import {LockedByEnum, LockResultEnum, RolePermissions} from "../../../main/models/enums";
import {ItemTypePredefined} from "../../../main/models/itemTypePredefined.enum";
import {IArtifact} from "../../../main/models/models";
import {ISession} from "../../../shell/login/session.svc";
import {SessionSvcMock} from "../../../shell/login/session.svc.mock";
import {IStatefulArtifact, StatefulArtifact} from "../artifact/artifact";
import {IArtifactService} from "../artifact/artifact.svc";
import {ArtifactServiceMock} from "../artifact/artifact.svc.mock";
import {IStatefulArtifactServices, StatefulArtifactServices} from "../services";
import {IArtifactState} from "./state";
import * as angular from "angular";

describe("ArtifactState", () => {
    let $q: ng.IQService;
    let $log: ng.ILogService;
    let artifact: IStatefulArtifact = null;
    let session: ISession = null;
    let artifactModel: IArtifact;
    const currentUserId: number = 1;
    const currentUserName: string = "Default Instance Admin";

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        // inject any services that are required
        $provide.service("artifactService", ArtifactServiceMock);
    }));

    beforeEach(inject((_$q_: ng.IQService, _$log_: ng.ILogService, artifactService: IArtifactService) => {
        $q = _$q_;
        $log = _$log_;
        session = new SessionSvcMock($q);
        const services = new StatefulArtifactServices(
            $q,
            $log,
            session,
            null,
            null,
            null,
            artifactService,
            null, null, null, null, null, null, null);
        artifact = createArtifact(services);
    }));

    describe("initialize", () => {
        it("throws error for falsy artifact", () => {
            // arrange
            let error: Error;

            // act
            try {
                artifact.artifactState.initialize(null);
            } catch (err) {
                error = err;
            }

            // assert
            expect(error).toBeDefined();
            expect(error.message).toEqual("artifact is invalid");
        });

        it("sets readonly to true for artifact locked by another user", () => {
            // arrange
            const otherUserId: number = 15;
            const model = <IArtifact>{
                id: 22,
                lockedByUser: {
                    id: otherUserId
                }
            };

            // act
            artifact.artifactState.initialize(model);

            // assert
            expect(artifact.artifactState.readonly).toEqual(true);
        });

        it("sets readonly to true for current user has no edit permissions to the artifact", () => {
            // arrange
            const model = <IArtifact>{
                id: 22,
                permissions: RolePermissions.None
            };

            // act
            artifact.artifactState.initialize(model);

            // assert
            expect(artifact.artifactState.readonly).toEqual(true);
        });

        it("sets readonly to false for artifact if user has edit permissions to a live, unlocked artifact", () => {
            // arrange
            const model = <IArtifact>{
                id: 22,
                permissions: RolePermissions.Edit
            };
            const artifact = createArtifact(null);

            // act
            artifact.artifactState.initialize(model);

            // assert
            expect(artifact.artifactState.readonly).toEqual(false);
        });

        it("sets lockedBy to CurrentUser for artifact locked by current user", () => {
            // arrange
            const model = <IArtifact>{
                id: 22,
                lockedByUser: {
                    id: currentUserId
                }
            };

            // act
            artifact.artifactState.initialize(model);

            // assert
            expect(artifact.artifactState.lockedBy).toBe(LockedByEnum.CurrentUser);
        });

        it("sets lockedBy to OtherUser for artifact locked by other user", () => {
            // arrange
            const otherUserId: number = 15;
            const model = <IArtifact>{
                id: 22,
                lockedByUser: {
                    id: otherUserId
                }
            };

            // act
            artifact.artifactState.initialize(model);

            // assert
            expect(artifact.artifactState.lockedBy).toBe(LockedByEnum.OtherUser);
        });

        it("sets lockDateTime to date and time of artifact lock", () => {
            // arrange
            const expectedLockDateTime: Date = new Date();
            const model = <IArtifact>{
                id: 22,
                lockedByUser: {
                    id: currentUserId,
                    displayName: currentUserName
                },
                lockedDateTime: expectedLockDateTime
            };

            // act
            artifact.artifactState.initialize(model);

            // assert
            expect(artifact.artifactState.lockDateTime).toBe(expectedLockDateTime);
        });

        it("sets lockOwner to name of artifact's lock owner", () => {
            // arrange
            const model = <IArtifact>{
                id: 22,
                lockedByUser: {
                    id: currentUserId,
                    displayName: currentUserName
                }
            };

            // act
            artifact.artifactState.initialize(model);

            // assert
            expect(artifact.artifactState.lockOwner).toBe(currentUserName);
        });
    });

    describe("setState", () => {
        it("throws error for falsy newState", () => {
            // arrange
            let error: Error;

            // act
            try {
                artifact.artifactState.setState(null);
            } catch (err) {
                error = err;
            }

            // assert
            expect(error).toBeDefined();
            expect(error.message).toEqual("newState is invalid");
        });
    });

    describe("lock", () => {
        it("correctly sets locking state for artifact locked by current user", (done: DoneFn) => {
            // arrange
            const lockTimestamp: Date = new Date();
            const lock = {
                result: LockResultEnum.Success,
                info: {
                    utcLockedDateTime: lockTimestamp,
                    lockOwnerId: currentUserId,
                    lockOwnerDisplayName: currentUserName
                }
            };
            const stateObserver = artifact.artifactState.onStateChange.subscribeOnNext((state: IArtifactState) => {
                // assert
                if (state) {
                    expect(state.lockedBy).toEqual(LockedByEnum.CurrentUser);
                    expect(state.lockOwner).toBe(currentUserName);
                    expect(state.lockDateTime).toEqual(lockTimestamp);
                    stateObserver.dispose();
                    done();
                }
            });

            // act
            artifact.artifactState.lock(lock);
        });

        it("correctly sets locking state for artifact locked by current user", (done: DoneFn) => {
            // arrange
            const userId: number = 5;
            const userName: string = "Tom Hanks";
            const lockTimestamp: Date = new Date();
            const lock = {
                result: LockResultEnum.AlreadyLocked,
                info: {
                    utcLockedDateTime: lockTimestamp,
                    lockOwnerId: userId,
                    lockOwnerDisplayName: userName
                }
            };
            const subscriber = artifact.artifactState.onStateChange.subscribeOnNext((state: IArtifactState) => {
                // assert
                if (state) {
                    expect(state.lockedBy).toEqual(LockedByEnum.OtherUser);
                    expect(state.lockOwner).toBe(userName);
                    expect(state.lockDateTime).toEqual(lockTimestamp);
                    subscriber.dispose();
                    done();
                }
            });

            // act
            artifact.artifactState.lock(lock);
        });
    });

    describe("dirty", () => {
        it("correctly sets dirty state", (done: DoneFn) => {
            // arrange
            const stateObserver = artifact.artifactState.onStateChange.subscribeOnNext((state: IArtifactState) => {
                // assert
                if (state) {
                    expect(state.lockedBy).toEqual(LockedByEnum.CurrentUser);
                    expect(state.dirty).toBe(true);
                    stateObserver.dispose();
                    done();
                }
            });

            let newStateValues = {
                lockDateTime: new Date(),
                lockedBy: LockedByEnum.CurrentUser,
                lockOwner: currentUserName
            };

            // act
            artifact.artifactState.setState(newStateValues, false);
            artifact.artifactState.dirty = true;
        });
    });

    describe("historical", () => {
        it("changes readonly to true when readonly is false and historical is true", () => {
            // arrange
            artifact.artifactState.readonly = false;

            // act
            artifact.artifactState.historical = true;

            // assert
            expect(artifact.artifactState.readonly).toEqual(true);
        });

        it("doesn't change readonly when readonly is true and historical is true", () => {
            // arrange
            artifact.artifactState.readonly = true;

            // act
            artifact.artifactState.historical = true;

            // assert
            expect(artifact.artifactState.readonly).toEqual(true);
        });

        it("doesn't change readonly when readonly is false and historical is false", () => {
            // arrange
            artifact.artifactState.readonly = false;

            // act
            artifact.artifactState.historical = false;

            // assert
            expect(artifact.artifactState.readonly).toEqual(false);
        });

        it("doesn't change readonly when readonly is true and historical is false", () => {
            // arrange
            artifact.artifactState.readonly = true;

            // act
            artifact.artifactState.historical = false;

            // assert
            expect(artifact.artifactState.readonly).toEqual(true);
        });
    });

    describe("deleted", () => {
        it("correctly sets deleted state", (done: DoneFn) => {
            // arrange
            const stateObserver = artifact.artifactState.onStateChange.subscribeOnNext((state: IArtifactState) => {
                // assert
                if (state && state.lockedBy !== LockedByEnum.None) {
                    expect(state.deleted).toBe(true);
                    stateObserver.dispose();
                    done();
                }
            });

            let newStateValues = {
                lockDateTime: new Date(),
                lockedBy: LockedByEnum.CurrentUser,
                lockOwner: currentUserName
            };

            // act
            artifact.artifactState.setState(newStateValues, false);
            artifact.artifactState.deleted = true;
        });

        it("changes readonly to true when readonly is false and deleted is true", () => {
            // arrange
            artifact.artifactState.readonly = false;

            // act
            artifact.artifactState.deleted = true;

            // assert
            expect(artifact.artifactState.readonly).toEqual(true);
        });

        it("doesn't change readonly when readonly is true and deleted is true", () => {
            // arrange
            artifact.artifactState.readonly = true;

            // act
            artifact.artifactState.deleted = true;

            // assert
            expect(artifact.artifactState.readonly).toEqual(true);
        });

        it("doesn't change readonly when readonly is false and deleted is false", () => {
            // arrange
            artifact.artifactState.readonly = false;

            // act
            artifact.artifactState.deleted = false;

            // assert
            expect(artifact.artifactState.readonly).toEqual(false);
        });

        it("doesn't change readonly when readonly is true and deleted is false", () => {
            // arrange
            artifact.artifactState.readonly = true;

            // act
            artifact.artifactState.deleted = false;

            // assert
            expect(artifact.artifactState.readonly).toEqual(true);
        });
    });

    describe("readonly", () => {
        it("correctly returns readonly when locked by another user", () => {
            // arrange
            let newStateValues = {
                lockDateTime: new Date(),
                lockedBy: LockedByEnum.OtherUser,
                lockOwner: currentUserName
            };

            // act
            artifact.artifactState.setState(newStateValues, false);

            // assert
            expect(artifact.artifactState.readonly).toBe(true);
        });
    });

    describe("published", () => {
        it("is false for artifact with draft version", () => {
            // arrange
            const draftVersion: number = -1;

            // act
            const artifact = createArtifact(null, draftVersion);

            // assert
            expect(artifact.artifactState.published).toBe(false);
        });

        it("is false for artifact with a published version, but a lock (potential edit)", () => {
            // arrange
            const draftVersion: number = 1;
            const artifact = createArtifact(null, draftVersion);
            const newStateValues = {
                lockDateTime: new Date(),
                lockedBy: LockedByEnum.CurrentUser,
                lockOwner: currentUserName
            };

            // act
            artifact.artifactState.setState(newStateValues, false);

            // assert
            expect(artifact.artifactState.published).toBe(false);
        });

        it("is true for artifact with a version and no lock", () => {
            // arrange
            const draftVersion: number = 1;

            // act
            const artifact = createArtifact(null, draftVersion);

            // assert
            expect(artifact.artifactState.published).toBe(true);
        });
    });

    describe("everPublished", () => {
        it("is false for non-published draft artifact", () => {
            // arrange
            const draftVersion: number = -1;

            // act
            const artifact = createArtifact(null, draftVersion);

            // assert
            expect(artifact.artifactState.everPublished).toBe(false);
        });

        it("is true for published artifact", () => {
            // arrange
            const publishedVersion: number = 12;

            // act
            const artifact = createArtifact(null, publishedVersion);

            // assert
            expect(artifact.artifactState.everPublished).toBe(true);
        });
    });

    describe("invalid", () => {
        it("doesn't notify if value not changed", (done: DoneFn) => {
            // arrange
            const newState = {invalid: true};
            artifact.artifactState.setState(newState, false);
            const subscriber = artifact.artifactState.onStateChange.subscribeOnNext((state: IArtifactState) => {
                if (state) {
                    // assert
                    fail("Unexpected notification");
                    subscriber.dispose();
                    done();
                }
            });

            // act
            artifact.artifactState.invalid = true;
            done();
        });

        it("sets new value and notifies if value changed", (done: DoneFn) => {
            // arrange
            const newState = {invalid: false};
            artifact.artifactState.setState(newState, false);
            const subscriber = artifact.artifactState.onStateChange.subscribeOnNext((state: IArtifactState) => {
                if (state) {
                    // assert
                    expect(artifact.artifactState.invalid).toEqual(true);
                    subscriber.dispose();
                    done();
                }
            });

            // act
            artifact.artifactState.invalid = true;
        });
    });

    describe("misplaced", () => {
        it("doesn't notify if value not changed", (done: DoneFn) => {
            // arrange
            const newState = {misplaced: true};
            artifact.artifactState.setState(newState, false);
            const subscriber = artifact.artifactState.onStateChange.subscribeOnNext((state: IArtifactState) => {
                if (state) {
                    // assert
                    subscriber.dispose();
                    fail("Unexpected notification");
                    done();
                }
            });

            // act
            artifact.artifactState.misplaced = true;
            done();
        });

        it("sets new value and notifies if value changed", (done: DoneFn) => {
            // arrange
            const newState = {misplaced: false};
            artifact.artifactState.setState(newState, false);
            const subscriber = artifact.artifactState.onStateChange.subscribeOnNext((state: IArtifactState) => {
                if (state) {
                    // assert
                    expect(artifact.artifactState.misplaced).toEqual(true);
                    subscriber.dispose();
                    done();
                }
            });

            // act
            artifact.artifactState.misplaced = true;
        });
    });

    function createArtifact(services: IStatefulArtifactServices, version?: number): IStatefulArtifact {
        artifactModel = {
            id: 22,
            name: "Artifact",
            prefix: "My",
            predefinedType: ItemTypePredefined.Process,
            lockedByUser: {
                id: currentUserId,
                displayName: currentUserName
            },
            version: version
        };

        return new StatefulArtifact(artifactModel, services);
    }
});
