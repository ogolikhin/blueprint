import * as angular from "angular";
import "angular-mocks";
import "../../../";
import {PublishAction} from "./publish-action";
import {IStatefulArtifact, IStatefulArtifactFactory} from "../../../../managers/artifact-manager";
import {StatefulArtifactFactoryMock} from "../../../../managers/artifact-manager/artifact/artifact.factory.mock";
import {ILocalizationService} from "../../../../core";
import {LocalizationServiceMock} from "../../../../core/localization/localization.mock";
import {ItemTypePredefined, RolePermissions} from "../../../../main/models/enums";

describe("PublishAction", () => {
    beforeEach(angular.mock.module("app.main"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("statefulArtifactFactory", StatefulArtifactFactoryMock);
        $provide.service("localization", LocalizationServiceMock);
    }));

    it("throws exception when localization is null", inject((statefulArtifactFactory: IStatefulArtifactFactory) => {
        // arrange
        const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact({ id: 1 });
        const localization: ILocalizationService = null;
        let error: Error = null;

        // act
        try {
            new PublishAction(artifact, localization);
        } catch (exception) {
            error = exception;
        }

        // assert
        expect(error).not.toBeNull();
        expect(error).toEqual(new Error("Localization service not provided or is null"));
    }));

    it("is disabled when artifact is null", inject((localization: ILocalizationService) => {
        // arrange
        const artifact: IStatefulArtifact = null;

        // act
        const publishAction = new PublishAction(artifact, localization);

        // assert
        expect(publishAction.disabled).toBe(true);
    }));

    it("is disabled when artifact is read-only", 
        inject((
            statefulArtifactFactory: IStatefulArtifactFactory, 
            localization: ILocalizationService) => {
        // arrange
        const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact({ id: 1 });
        artifact.artifactState.readonly = true;

        // act
        const publishAction = new PublishAction(artifact, localization);

        // assert
        expect(publishAction.disabled).toBe(true);
    }));

    it("is disabled when artifact is Project", 
        inject((
            statefulArtifactFactory: IStatefulArtifactFactory, 
            localization: ILocalizationService) => {
        // arrange
        const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact(
            {
                 id: 1, 
                 predefinedType: ItemTypePredefined.Project 
            });

        // act
        const publishAction = new PublishAction(artifact, localization);

        // assert
        expect(publishAction.disabled).toBe(true);
    }));

    it("is disabled when artifact is Collections", 
        inject((
            statefulArtifactFactory: IStatefulArtifactFactory, 
            localization: ILocalizationService) => {
        // arrange
        const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact(
            { 
                id: 1, 
                predefinedType: ItemTypePredefined.Collections 
            });

        // act
        const publishAction = new PublishAction(artifact, localization);

        // assert
        expect(publishAction.disabled).toBe(true);
    }));

    it("is enabled when artifact is valid", 
        inject((
            statefulArtifactFactory: IStatefulArtifactFactory, 
            localization: ILocalizationService) => {
        // arrange
        const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact(
            { 
                id: 1, 
                predefinedType: ItemTypePredefined.TextualRequirement, 
                lockedByUser: null,
                lockedDateTime: null,
                permissions: RolePermissions.Edit
            });

        // act
        const publishAction = new PublishAction(artifact, localization);

        // assert
        expect(publishAction.disabled).toBe(false);
    }));

    it("calls artifact.discard when executed", 
        inject((
            statefulArtifactFactory: IStatefulArtifactFactory, 
            localization: ILocalizationService) => {
        // arrange
        const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact(
            { 
                id: 1, 
                predefinedType: ItemTypePredefined.TextualRequirement, 
                lockedByUser: null,
                lockedDateTime: null,
                permissions: RolePermissions.Edit
            });
        const publishSpy = spyOn(artifact, "publish");
        const publishAction = new PublishAction(artifact, localization);

        // act
        publishAction.execute();

        // assert
        expect(publishSpy).toHaveBeenCalled();
    }));
});