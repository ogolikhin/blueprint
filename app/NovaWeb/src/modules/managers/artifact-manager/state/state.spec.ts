import * as angular from "angular";
import "angular-mocks";
import "rx/dist/rx.lite";
import { Models, Enums } from "../../../main/models";
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
    let artifactModel: Models.IArtifact;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        // inject any services that are required
        $provide.service("artifactService", ArtifactServiceMock);
    }));

    beforeEach(inject((
        _$q_: ng.IQService,
        artifactService: IArtifactService) => {

        $q = _$q_;

        session = new SessionSvcMock($q);
 
        const services = new StatefulArtifactServices(
                $q, session, null, null, null, artifactService, null, null, null);

        artifactModel = {
            id: 22,
            name: "Artifact",
            prefix: "My",
            predefinedType: Models.ItemTypePredefined.Process,
            lockedByUser: {
                id: 1,
                displayName: "Default Instance Admin"
            }
        };
       
        artifact = new StatefulArtifact(artifactModel, services);
    }));

    it("correctly initializes", () => {

        artifact.artifactState.initialize(artifactModel);

        expect(artifact.artifactState.lockedBy).toBe(Enums.LockedByEnum.CurrentUser);
        expect(artifact.artifactState.lockOwner).toBe("Default Instance Admin");
    });

    it("correctly sets locking state", (done) => {

        const lock = {
            result: Enums.LockResultEnum.Success,
            info: {
                utcLockedDateTime: new Date(),
                lockOwnerId: 1,
                lockOwnerDisplayName: "Default Instance Admin"
            }
        };
        const stateObserver = artifact.artifactState.onStateChange.subscribe(
            (state) => {
                // assert
                if (state && state.lockedBy != Enums.LockedByEnum.None) {
                    expect(state.lockOwner).toBe("Default Instance Admin");
                    stateObserver.dispose();
                    done();
                }
            },
            (err) => {
                fail("state change error: " + err);
            });
  
        artifact.artifactState.lock(lock);
    });
     
    it("correctly sets dirty state", (done) => {
             
        const stateObserver = artifact.artifactState.onStateChange.subscribe(
            (state) => {
                // assert
                if (state && state.lockedBy != Enums.LockedByEnum.None) {
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
            lockedBy: Enums.LockedByEnum.CurrentUser,
            lockOwner: "Default Instance Admin"
        }

        artifact.artifactState.setState(newState, false);
        artifact.artifactState.dirty = true;

    });

    it("correctly sets deleted state", (done) => {

        const stateObserver = artifact.artifactState.onStateChange.subscribe(
            (state) => {
                // assert
                if (state && state.lockedBy != Enums.LockedByEnum.None) {
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
            lockedBy: Enums.LockedByEnum.CurrentUser,
            lockOwner: "Default Instance Admin"
        }

        artifact.artifactState.setState(newState, false);
        artifact.artifactState.deleted = true;

    });

    it("correctly sets published state", (done) => {

        const stateObserver = artifact.artifactState.onStateChange.subscribe(
            (state) => {
                // assert
                if (state && state.lockedBy != Enums.LockedByEnum.None) {
                    expect(state.published).toBe(true);
                    stateObserver.dispose();
                    done();
                }
            },
            (err) => {
                fail("state change error: " + err);
            });

        let newState: IState = {
            lockDateTime: new Date(),
            lockedBy: Enums.LockedByEnum.CurrentUser,
            lockOwner: "Default Instance Admin"
        }

        artifact.artifactState.setState(newState, false);
        artifact.artifactState.published = true;

    });

    it("correctly returns readonly when locked by another user", () => {
 
        let newState: IState = {
            lockDateTime: new Date(),
            lockedBy: Enums.LockedByEnum.OtherUser,
            lockOwner: "Default Instance Admin"
        }

        artifact.artifactState.setState(newState, false);
        expect(artifact.artifactState.readonly).toBe(true);

    });
});
