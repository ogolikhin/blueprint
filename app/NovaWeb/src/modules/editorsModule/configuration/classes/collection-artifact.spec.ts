import "angular-mocks";
import "rx/dist/rx.lite";
import {ILoadingOverlayService, LoadingOverlayService} from "../../../commonModule/loadingOverlay/loadingOverlay.service";
import {Models} from "../../../main/models";
import {ItemTypePredefined} from "../../../main/models/item-type-predefined";
import {IArtifactService} from "../../../managers/artifact-manager/";
import {ArtifactServiceMock} from "../../../managers/artifact-manager/artifact/artifact.svc.mock";
import {StatefulArtifactServices} from "../../../managers/artifact-manager/services";
import {ICollection, StatefulCollectionArtifact} from "./collection-artifact";
import * as angular from "angular";

describe("StatefulCollectionArtifact", () => {

    let artifactServices: StatefulArtifactServices;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
       $provide.service("artifactService", ArtifactServiceMock);
       $provide.service("loadingOverlayService", LoadingOverlayService);
    }));

    beforeEach(inject((_$rootScope_: ng.IRootScopeService,
                       _$q_: ng.IQService,
                       _$log_: ng.ILogService,
                       artifactService: IArtifactService,
                       loadingOverlayService: ILoadingOverlayService) => {
                            artifactServices = new StatefulArtifactServices(
                            _$q_,
                            _$log_,
                            null,
                            null,
                            null,
                            null,
                            artifactService,
                            null,
                            null,
                            null,
                            loadingOverlayService,
                            null,
                            null,
                            null);
    }));


    it("Add artifacts to collection", () => {

        const collection = <ICollection> {
            reviewName: "Review",
            isCreated: false,
            artifacts: []
        };

        const artifact = {
            id: 1,
            name: "1",
            projectId: 1,
            predefinedType: ItemTypePredefined.Actor
        } as Models.IArtifact;

        let collectionArtifact = new StatefulCollectionArtifact(collection, artifactServices);
        collectionArtifact["initialize"](collection);

        // Act
        collectionArtifact.addArtifactsToCollection([artifact]);

        //Assert
        expect(collectionArtifact.artifacts.length === 1).toBeTruthy();
        expect(collectionArtifact.specialProperties.changes().length === 1).toBeTruthy();
    });

    it("Remove artifacts from collection", () => {

        const artifact = {
            id: 1,
            name: "1",
            projectId: 1,
            predefinedType: ItemTypePredefined.Actor
        } as Models.IArtifact;

        const collection = <ICollection> {
            reviewName: "Review",
            isCreated: false,
            artifacts: [artifact]
        };

        let collectionArtifact = new StatefulCollectionArtifact(collection, artifactServices);
        collectionArtifact["initialize"](collection);

        // Act
        collectionArtifact.removeArtifacts([artifact]);

        //Assert
        expect(collectionArtifact.artifacts.length === 0).toBeTruthy();
        expect(collectionArtifact.specialProperties.changes().length === 1).toBeTruthy();
    });

  });

