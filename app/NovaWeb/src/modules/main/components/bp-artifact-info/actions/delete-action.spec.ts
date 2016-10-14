import * as angular from "angular";
import "angular-mocks";
import "../../../";
import {DeleteAction} from "./delete-action";
import {IStatefulArtifact, IStatefulArtifactFactory} from "../../../../managers/artifact-manager";
import {StatefulArtifactFactoryMock} from "../../../../managers/artifact-manager/artifact/artifact.factory.mock";
import {ILocalizationService} from "../../../../core";
import {LocalizationServiceMock} from "../../../../core/localization/localization.mock";
import {IDialogService, IDialogSettings} from "../../../../shared";
import {DialogServiceMock} from "../../../../shared/widgets/bp-dialog/bp-dialog";
import {ItemTypePredefined, RolePermissions} from "../../../../main/models/enums";

describe("DeleteAction", () => {
    let $scope: ng.IScope;

    beforeEach(angular.mock.module("app.main"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("statefulArtifactFactory", StatefulArtifactFactoryMock);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("dialogService", DialogServiceMock);
    }));

    beforeEach(inject(($rootScope: ng.IRootScopeService) => {
        $scope = $rootScope.$new();
    }));

    it("throws exception when localization is null", inject((statefulArtifactFactory: IStatefulArtifactFactory, dialogService: IDialogService) => {
        // arrange
        const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact({id: 1});
        const localization: ILocalizationService = null;
        let error: Error = null;

        // act
        try {
            new DeleteAction(artifact, localization, dialogService, {});
        } catch (exception) {
            error = exception;
        }

        // assert
        expect(error).not.toBeNull();
        expect(error).toEqual(new Error("Localization service not provided or is null"));
    }));

    it("throws exception when dialogService is null", inject((statefulArtifactFactory: IStatefulArtifactFactory, localization: ILocalizationService) => {
        // arrange
        const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact({id: 1});
        const dialogService: IDialogService = null;
        let error: Error = null;

        // act
        try {
            new DeleteAction(artifact, localization, dialogService, {});
        } catch (exception) {
            error = exception;
        }

        // assert
        expect(error).not.toBeNull();
        expect(error).toEqual(new Error("Dialog service not provided or is null"));
    }));

    it("throws exception when dialogServiceSettings is null",
        inject((statefulArtifactFactory: IStatefulArtifactFactory,
                localization: ILocalizationService,
                dialogService: IDialogService) => {
            // arrange
            const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact({id: 1});
            const deleteDialogSettings: IDialogSettings = null;
            let error: Error = null;

            // act
            try {
                new DeleteAction(artifact, localization, dialogService, deleteDialogSettings);
            } catch (exception) {
                error = exception;
            }

            // assert
            expect(error).not.toBeNull();
            expect(error).toEqual(new Error("Delete dialog settings not provided or is null"));
        }));

    it("is disabled when artifact is null",
        inject((localization: ILocalizationService,
                dialogService: IDialogService) => {
            // arrange
            const artifact: IStatefulArtifact = null;

            // act
            const deleteAction = new DeleteAction(artifact, localization, dialogService, {});

            // assert
            expect(deleteAction.disabled).toBe(true);
        }));

    it("is disabled when artifact is read-only",
        inject((statefulArtifactFactory: IStatefulArtifactFactory,
                localization: ILocalizationService,
                dialogService: IDialogService) => {
            // arrange
            const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact({id: 1});
            artifact.artifactState.readonly = true;

            // act
            const deleteAction = new DeleteAction(artifact, localization, dialogService, {});

            // assert
            expect(deleteAction.disabled).toBe(true);
        }));

    it("is disabled when artifact is Project",
        inject((statefulArtifactFactory: IStatefulArtifactFactory,
                localization: ILocalizationService,
                dialogService: IDialogService) => {
            // arrange
            const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact(
                {
                    id: 1,
                    predefinedType: ItemTypePredefined.Project
                });

            // act
            const deleteAction = new DeleteAction(artifact, localization, dialogService, {});

            // assert
            expect(deleteAction.disabled).toBe(true);
        }));

    it("is disabled when artifact is Collections",
        inject((statefulArtifactFactory: IStatefulArtifactFactory,
                localization: ILocalizationService,
                dialogService: IDialogService) => {
            // arrange
            const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact(
                {
                    id: 1,
                    predefinedType: ItemTypePredefined.Collections
                });

            // act
            const deleteAction = new DeleteAction(artifact, localization, dialogService, {});

            // assert
            expect(deleteAction.disabled).toBe(true);
        }));

    it("is enabled when artifact is valid",
        inject((statefulArtifactFactory: IStatefulArtifactFactory,
                localization: ILocalizationService,
                dialogService: IDialogService) => {
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
            const deleteAction = new DeleteAction(artifact, localization, dialogService, {});

            // assert
            expect(deleteAction.disabled).toBe(false);
        }));

    it("opens dialog when executed",
        inject((statefulArtifactFactory: IStatefulArtifactFactory,
                localization: ILocalizationService,
                dialogService: IDialogService) => {
            // arrange
            const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact(
                {
                    id: 1,
                    predefinedType: ItemTypePredefined.TextualRequirement,
                    lockedByUser: null,
                    lockedDateTime: null,
                    permissions: RolePermissions.Edit
                });
            const deleteAction = new DeleteAction(artifact, localization, dialogService, {});
            const dialogOpenSpy = spyOn(dialogService, "open").and.callThrough();
            const deleteArtifactSpy = spyOn(deleteAction, "deleteArtifact").and.callThrough();

            // act
            deleteAction.execute();
            $scope.$digest();

            // assert
            expect(dialogOpenSpy).toHaveBeenCalled();
            expect(deleteArtifactSpy).toHaveBeenCalled();
        }));

    it("calls deleteArtifact when dialog is confirmed",
        inject(($q: ng.IQService,
                statefulArtifactFactory: IStatefulArtifactFactory,
                localization: ILocalizationService,
                dialogService: IDialogService) => {
            // arrange
            const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact(
                {
                    id: 1,
                    predefinedType: ItemTypePredefined.TextualRequirement,
                    lockedByUser: null,
                    lockedDateTime: null,
                    permissions: RolePermissions.Edit
                });
            const deleteAction = new DeleteAction(artifact, localization, dialogService, {});
            spyOn(dialogService, "open").and.callFake(() => {
                const deferred = $q.defer();
                deferred.resolve(true);
                return deferred.promise;
            });
            const deleteArtifactSpy = spyOn(deleteAction, "deleteArtifact").and.callThrough();

            // act
            deleteAction.execute();
            $scope.$digest();

            // assert
            expect(deleteArtifactSpy).toHaveBeenCalled();
        }));

    it("doesn't call deleteArtifact when dialog is not confirmed",
        inject(($q: ng.IQService,
                statefulArtifactFactory: IStatefulArtifactFactory,
                localization: ILocalizationService,
                dialogService: IDialogService) => {
            // arrange
            const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact(
                {
                    id: 1,
                    predefinedType: ItemTypePredefined.TextualRequirement,
                    lockedByUser: null,
                    lockedDateTime: null,
                    permissions: RolePermissions.Edit
                });
            const deleteAction = new DeleteAction(artifact, localization, dialogService, {});
            spyOn(dialogService, "open").and.callFake(() => {
                const deferred = $q.defer();
                deferred.resolve(false);
                return deferred.promise;
            });
            const deleteArtifactSpy = spyOn(deleteAction, "deleteArtifact").and.callThrough();

            // act
            deleteAction.execute();
            $scope.$digest();

            // assert
            expect(deleteArtifactSpy).not.toHaveBeenCalled();
        }));
});
