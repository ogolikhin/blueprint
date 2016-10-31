import * as angular from "angular";
import "angular-mocks";
import "rx/dist/rx.lite";
import { LockedByEnum, LockResultEnum } from "../../../main/models/enums";
import { IArtifact, ItemTypePredefined } from "../../../main/models/models";
import { ISession } from "../../../shell/login/session.svc";
import { SessionSvcMock } from "../../../shell/login//mocks.spec";
import { IState } from "./state";
import { StatefulItem, IStatefulItem, IIStatefulItem } from "../item";
import { StatefulArtifact, IStatefulArtifact, IIStatefulArtifact } from "../artifact/artifact";
import { IArtifactService } from "../artifact/artifact.svc";
import { ArtifactServiceMock } from "../artifact/artifact.svc.mock";
import { StatefulArtifactServices, IStatefulArtifactServices } from "../services";

describe("ArtifactState", () => {
    let $q: ng.IQService;
    let artifact: IStatefulArtifact = null;
    let session: ISession = null;
    let artifactModel: IArtifact;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        // inject any services that are required
        $provide.service("artifactService", ArtifactServiceMock);
    }));

    beforeEach(inject((
        _$q_: ng.IQService,
        artifactService: IArtifactService) => {

        $q = _$q_;

        session = new SessionSvcMock($q);
        const services = new StatefulArtifactServices($q, session, null, null, null, artifactService, null, null, null, null, null);
        artifact = createArtifact(services); 
    }));

    it("correctly initializes", () => {
        // act
        artifact.artifactState.initialize(artifactModel);

        // assert
        expect(artifact.artifactState.lockedBy).toBe(LockedByEnum.CurrentUser);
        expect(artifact.artifactState.lockOwner).toBe("Default Instance Admin");
    });

    it("correctly sets locking state", (done) => {
        // arrange
        const lock = {
            result: LockResultEnum.Success,
            info: {
                utcLockedDateTime: new Date(),
                lockOwnerId: 1,
                lockOwnerDisplayName: "Default Instance Admin"
            }
        };
        const stateObserver = artifact.artifactState.onStateChange.subscribe(
            (state) => {
                // assert
                if (state && state.lockedBy !== LockedByEnum.None) {
                    expect(state.lockOwner).toBe("Default Instance Admin");
                    stateObserver.dispose();
                    done();
                }
            },
            (err) => {
                fail("state change error: " + err);
            });

        // act
        artifact.artifactState.lock(lock);
    });

    it("correctly sets dirty state", (done) => {
        // arrange
        const stateObserver = artifact.artifactState.onStateChange.subscribe(
            (state) => {
                // assert
                if (state && state.lockedBy !== LockedByEnum.None) {
                    expect(state.dirty).toBe(true);
                    stateObserver.dispose();
                    done();
                }
            },
            (err) => {
                fail("state change error: " + err);
            });

        let newState: IState = {
            lockDateTime: new Date(),
            lockedBy: LockedByEnum.CurrentUser,
            lockOwner: "Default Instance Admin"
        };

        // act
        artifact.artifactState.setState(newState, false);
        artifact.artifactState.dirty = true;
    });

    it("correctly sets deleted state", (done) => {
        // arrange
        const stateObserver = artifact.artifactState.onStateChange.subscribe(
            (state) => {
                // assert
                if (state && state.lockedBy !== LockedByEnum.None) {
                    expect(state.deleted).toBe(true);
                    stateObserver.dispose();
                    done();
                }
            },
            (err) => {
                fail("state change error: " + err);
            });

        let newState: IState = {
            lockDateTime: new Date(),
            lockedBy: LockedByEnum.CurrentUser,
            lockOwner: "Default Instance Admin"
        };

        // act
        artifact.artifactState.setState(newState, false);
        artifact.artifactState.deleted = true;
    });

    it("correctly returns readonly when locked by another user", () => {
        // arrange
        let newState: IState = {
            lockDateTime: new Date(),
            lockedBy: LockedByEnum.OtherUser,
            lockOwner: "Default Instance Admin"
        };

        // act
        artifact.artifactState.setState(newState, false);

        // assert
        expect(artifact.artifactState.readonly).toBe(true);
    });

    describe("published", () => {
        it("returns false for artifact with draft version", () => {
            // arrange
            const draftVersion: number = -1;

            // act
            const artifact = createArtifact(null, draftVersion);

            // assert
            expect(artifact.artifactState.published).toBe(false);
        });

        it("returns false for artifact with a published version, but a lock (potential edit)", () => {
            // arrange
            const draftVersion: number = 1;
            const artifact = createArtifact(null, draftVersion);
            const newState: IState = {
                lockDateTime: new Date(),
                lockedBy: LockedByEnum.CurrentUser,
                lockOwner: "Default Instance Admin"
            };

            // act
            artifact.artifactState.setState(newState, false);

            // assert
            expect(artifact.artifactState.published).toBe(false);
        });

        it("returns true for artifact with a version and no lock", () => {
            // arrange
            const draftVersion: number = 1;

            // act
            const artifact = createArtifact(null, draftVersion);

            // assert
            expect(artifact.artifactState.published).toBe(true);
        });
    });

    function createArtifact(services: IStatefulArtifactServices, version?: number): IStatefulArtifact {
        artifactModel = {
            id: 22,
            name: "Artifact",
            prefix: "My",
            predefinedType: ItemTypePredefined.Process,
            lockedByUser: {
                id: 1,
                displayName: "Default Instance Admin"
            },
            version: version ? version : undefined
        };
        
        return new StatefulArtifact(artifactModel, services);
    }
});
