import * as angular from "angular";
import "angular-mocks";
import "../../../";
import {DeleteAction} from "./delete-action";
import {IStatefulArtifact, IStatefulArtifactFactory} from "../../../../managers/artifact-manager";
import {StatefulArtifactFactoryMock} from "../../../../managers/artifact-manager/artifact/artifact.factory.mock";
import {ILocalizationService} from "../../../../core/localization/localizationService";
import {IMessageService} from "../../../../core/messages/message.svc";
import {LocalizationServiceMock} from "../../../../core/localization/localization.mock";
import {MessageServiceMock} from "../../../../core/messages/message.mock";
import {IDialogService, IDialogSettings} from "../../../../shared";
import {DialogServiceMock} from "../../../../shared/widgets/bp-dialog/bp-dialog";
import {ItemTypePredefined, RolePermissions} from "../../../../main/models/enums";
import {IProjectManager, ProjectManager} from "../../../../managers/project-manager/project-manager";
import {ILoadingOverlayService, LoadingOverlayService} from "../../../../core/loading-overlay/loading-overlay.svc";

describe("DeleteAction", () => {
    let $q_: ng.IQService;
    let $scope: ng.IScope;

    beforeEach(angular.mock.module("app.main"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("statefulArtifactFactory", StatefulArtifactFactoryMock);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("messageService", MessageServiceMock);        
        $provide.service("dialogService", DialogServiceMock);
        $provide.service("projectManager", ProjectManager); 
        $provide.service("loadingOverlayService", LoadingOverlayService);       
    }));

    beforeEach(inject(($rootScope: ng.IRootScopeService, $q: ng.IQService) => {
        $scope = $rootScope.$new();
        $q_ = $q;
    }));

    it("throws exception when localization is null", inject((
            statefulArtifactFactory: IStatefulArtifactFactory,
            projectManager: IProjectManager,
            messageService: IMessageService,
            loadingOverlayService: ILoadingOverlayService,
            dialogService: IDialogService
            ) => {
        // arrange
        const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact({id: 1});
        const localization: ILocalizationService = null;
        let error: Error = null;

        // act
        try {
            new DeleteAction(artifact, localization, messageService, projectManager, loadingOverlayService, dialogService);
        } catch (exception) {
            error = exception;
        }

        // assert
        expect(error).not.toBeNull();
        expect(error).toEqual(new Error("Localization service not provided or is null"));
    }));

    it("throws exception when dialogService is null", inject((
            statefulArtifactFactory: IStatefulArtifactFactory,
            localization: ILocalizationService,
            projectManager: IProjectManager,
            messageService: IMessageService,
            loadingOverlayService: ILoadingOverlayService,
            ) => {
        // arrange
        const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact({id: 1});
        const dialogService: IDialogService = null;
        let error: Error = null;

        // act
        try {
            new DeleteAction(artifact, localization, messageService, projectManager, loadingOverlayService, dialogService);
        } catch (exception) {
            error = exception;
        }

        // assert
        expect(error).not.toBeNull();
        expect(error).toEqual(new Error("Dialog service not provided or is null"));
    }));


    it("is disabled when artifact is null", inject((
            localization: ILocalizationService,
            projectManager: IProjectManager,
            messageService: IMessageService,
            loadingOverlayService: ILoadingOverlayService,
            dialogService: IDialogService            
            ) => {
            // arrange
            const artifact: IStatefulArtifact = null;

            // act
            const deleteAction = new DeleteAction(artifact, localization, messageService, projectManager, loadingOverlayService, dialogService);

            // assert
            expect(deleteAction.disabled).toBe(true);
        }));

    it("is disabled when artifact is read-only", inject((
            statefulArtifactFactory: IStatefulArtifactFactory,            
            localization: ILocalizationService,
            projectManager: IProjectManager,
            messageService: IMessageService,
            loadingOverlayService: ILoadingOverlayService,
            dialogService: IDialogService            
            ) => {
            // arrange
            const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact({id: 1});
            artifact.artifactState.readonly = true;

            // act
            const deleteAction = new DeleteAction(artifact, localization, messageService, projectManager, loadingOverlayService, dialogService);

            // assert
            expect(deleteAction.disabled).toBe(true);
        }));

    it("is disabled when artifact is Project", inject((
            statefulArtifactFactory: IStatefulArtifactFactory,            
            localization: ILocalizationService,
            projectManager: IProjectManager,
            messageService: IMessageService,
            loadingOverlayService: ILoadingOverlayService,
            dialogService: IDialogService            
            ) => {
            // arrange
            const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact(
                {
                    id: 1,
                    predefinedType: ItemTypePredefined.Project
                });

            // act
            const deleteAction = new DeleteAction(artifact, localization, messageService, projectManager, loadingOverlayService, dialogService);

            // assert
            expect(deleteAction.disabled).toBe(true);
        }));

    it("is disabled when artifact is Collections", inject((
            statefulArtifactFactory: IStatefulArtifactFactory,            
            localization: ILocalizationService,
            projectManager: IProjectManager,
            messageService: IMessageService,
            loadingOverlayService: ILoadingOverlayService,
            dialogService: IDialogService            
            ) => {
            // arrange
            const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact(
                {
                    id: 1,
                    predefinedType: ItemTypePredefined.Collections
                });

            // act
            const deleteAction = new DeleteAction(artifact, localization, messageService, projectManager, loadingOverlayService, dialogService);

            // assert
            expect(deleteAction.disabled).toBe(true);
        }));

    it("is enabled when artifact is valid", inject((
            statefulArtifactFactory: IStatefulArtifactFactory,            
            localization: ILocalizationService,
            projectManager: IProjectManager,
            messageService: IMessageService,
            loadingOverlayService: ILoadingOverlayService,
            dialogService: IDialogService            
            ) => {
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
            const deleteAction = new DeleteAction(artifact, localization, messageService, projectManager, loadingOverlayService, dialogService);

            // assert
            expect(deleteAction.disabled).toBe(false);
        }));

    describe("when executed", () => {
        let deleteAction: DeleteAction;
        let beginLoadingSpy: jasmine.Spy;
        let deleteSpy: jasmine.Spy;
        let refreshSpy: jasmine.Spy;
        let getDescendantsSpy: jasmine.Spy;
        let endLoadingSpy: jasmine.Spy;    
        let dialogOpenSpy: jasmine.Spy;    

        beforeEach(inject((
            statefulArtifactFactory: IStatefulArtifactFactory,            
            localization: ILocalizationService,
            projectManager: IProjectManager,
            messageService: IMessageService,
            loadingOverlayService: ILoadingOverlayService,
            dialogService: IDialogService            
            ) => {
            const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact(
                {
                    id: 1,
                    predefinedType: ItemTypePredefined.TextualRequirement,
                    lockedByUser: null,
                    lockedDateTime: null,
                    permissions: RolePermissions.Edit
                });
                deleteAction = new DeleteAction(artifact, localization, messageService, projectManager, loadingOverlayService, dialogService);
                beginLoadingSpy = spyOn(loadingOverlayService, "beginLoading").and.callThrough();
                endLoadingSpy = spyOn(loadingOverlayService, "endLoading").and.callThrough();
                dialogOpenSpy = spyOn(dialogService, "open");
                deleteSpy = spyOn(artifact, "delete");
                refreshSpy = spyOn(artifact, "refresh").and.callFake(() => {
                    const deferred = $q_.defer();
                    deferred.resolve(true);
                    return deferred.promise;
                });
                getDescendantsSpy = spyOn(projectManager, "getDescendantsToBeDeleted").and.callFake(() => {
                    const deferred = $q_.defer();
                    deferred.resolve({
                        id: 1, name: "Test", children: null
                    });
                    return deferred.promise;
                });
                
            }));

             it("confirm and delete", () => {
                 // assert
                dialogOpenSpy.and.callFake(() => {
                    const deferred = $q_.defer();
                    deferred.resolve(true);
                    return deferred.promise;
                });
                deleteSpy.and.callFake(() => {
                    const deferred = $q_.defer();
                    deferred.resolve();
                    return deferred.promise;
                });

                deleteAction.execute();
                $scope.$digest();
                 
                 expect(getDescendantsSpy).toHaveBeenCalled();
                 expect(deleteSpy).toHaveBeenCalled();

             });

             it("shows/hides loading screen", () => {
                 // assert
                dialogOpenSpy.and.callFake(() => {
                    const deferred = $q_.defer();
                    deferred.resolve(true);
                    return deferred.promise;
                });
                deleteSpy.and.callFake(() => {
                    const deferred = $q_.defer();
                    deferred.resolve();
                    return deferred.promise;
                });


                deleteAction.execute();
                $scope.$digest();
                 
                expect(beginLoadingSpy).toHaveBeenCalledTimes(2);
                expect(endLoadingSpy).toHaveBeenCalledTimes(2);
             });

            it("rejects delete", () => {
                // assert
                dialogOpenSpy.and.callFake(() => {
                    const deferred = $q_.defer();
                    deferred.reject();
                    return deferred.promise;
                });


                deleteAction.execute();
                $scope.$digest();
                 

                expect(deleteSpy).not.toHaveBeenCalled();
            });

        });
});
