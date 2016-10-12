import * as angular from "angular";
import "angular-mocks";
import "../../../";
import {OpenImpactAnalysisAction} from "./open-impact-analysis-action";
import {IStatefulArtifact, IStatefulArtifactFactory} from "../../../../managers/artifact-manager";
import {StatefulArtifactFactoryMock} from "../../../../managers/artifact-manager/artifact/artifact.factory.mock";
import {ILocalizationService} from "../../../../core";
import {LocalizationServiceMock} from "../../../../core/localization/localization.mock";
import {ItemTypePredefined, RolePermissions} from "../../../../main/models/enums";

describe("OpenImpactAnalysisAction", () => {
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
            new OpenImpactAnalysisAction(artifact, localization);
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
        const openImpactAnalysisAction = new OpenImpactAnalysisAction(artifact, localization);

        // assert
        expect(openImpactAnalysisAction.disabled).toBe(true);
    }));

    it("is disabled for Project artifact", inject((
        statefulArtifactFactory: IStatefulArtifactFactory,
        localization: ILocalizationService) => {
        // arrange
        const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact({ 
            id: 1, 
            predefinedType: ItemTypePredefined.Project
        });

        // act
        const openImpactAnalysisAction = new OpenImpactAnalysisAction(artifact, localization);

        // assert
        expect(openImpactAnalysisAction.disabled).toBe(true);
    }));

    it("is disabled for new artifact", inject((
        statefulArtifactFactory: IStatefulArtifactFactory,
        localization: ILocalizationService) => {
        // arrange
        const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact(
            { 
                id: 1, 
                predefinedType: ItemTypePredefined.Actor, 
                lockedByUser: null,
                lockedDateTime: null,
                permissions: RolePermissions.Edit,
                version: -1
            });

        // act
        const openImpactAnalysisAction = new OpenImpactAnalysisAction(artifact, localization);

        // assert
        expect(openImpactAnalysisAction.disabled).toBe(true);
    }));

    it("is enabled for published artifact", inject((
        statefulArtifactFactory: IStatefulArtifactFactory,
        localization: ILocalizationService) => {
        // arrange
        const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact(
            { 
                id: 1, 
                predefinedType: ItemTypePredefined.Actor, 
                lockedByUser: null,
                lockedDateTime: null,
                permissions: RolePermissions.Edit,
                version: 1
            });

        // act
        const openImpactAnalysisAction = new OpenImpactAnalysisAction(artifact, localization);

        // assert
        expect(openImpactAnalysisAction.disabled).toBe(false);
    }));

    it("opens new window when executed", inject((statefulArtifactFactory: IStatefulArtifactFactory, localization: ILocalizationService) => {
        // arrange
        const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact({ id: 1 });
        const openImpactAnalysisAction = new OpenImpactAnalysisAction(artifact, localization);
        const openSpy = spyOn(window, "open");

        // act
        openImpactAnalysisAction.execute();

        // assert
        expect(openSpy).toHaveBeenCalledWith(`Web/#/ImpactAnalysis/${artifact.id}`);
    }));
});