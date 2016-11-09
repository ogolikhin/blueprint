// import "angular";
// import "angular-mocks";
// import { SettingsService } from "../../../core";
// import { MessageService } from "../../../shell/";
// import { Models} from "../../models";
// import { ProjectManager, IProjectManager} from "../../services";

// import { ProjectExplorerController } from "./bp-explorer";

// import { BPTreeControllerMock, ITreeNode } from "../../../shared/widgets/bp-tree/bp-tree.mock";
// import { LocalizationServiceMock } from "../../../core/localization/localization.mock";
// import { ProjectRepositoryMock } from "../../services/project-repository.mock";
// import { SelectionManager } from "../../../main/services/selection-manager";

// describe("Project Explorer Test", () => {
//     let isReloadCalled: number = 0;

//     beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
//         $provide.service("localization", LocalizationServiceMock);
//         $provide.service("settings", SettingsService);
//         $provide.service("messageService", MessageService);
//         $provide.service("projectRepository", ProjectRepositoryMock);
//         $provide.service("projectManager", ProjectManager);
//         $provide.service("explorer", ProjectExplorerController);
//         $provide.service("selectionManager", SelectionManager);
//     }));

//     beforeEach(inject(($rootScope: ng.IRootScopeService, projectManager: ProjectManager, explorer: ProjectExplorerController) => {
//         $rootScope["config"] = {
//             "settings": {
//                 "StorytellerMessageTimeout": `{ "Warning": 0, "Info": 3000, "Error": 0 }`
//             }
//         };

//         explorer.tree = new BPTreeControllerMock();
//         projectManager.initialize();
//         explorer.$onInit();
//     }));


//     it("check property map", inject((explorer: ProjectExplorerController) => {
//         // Arrange
//         // Act
//         // Assert

//         expect(explorer.propertyMap).toBeDefined();
//         expect(explorer.propertyMap["id"]).toEqual("id");
//         expect(explorer.propertyMap["itemTypeId"]).toEqual("itemTypeId");
//         expect(explorer.propertyMap["name"]).toEqual("name");
//         expect(explorer.propertyMap["hasChildren"]).toEqual("hasChildren");
//         expect(explorer.propertyMap["artifacts"]).toEqual("children");

//     }));

//     it("onDestroy", inject(($rootScope: ng.IRootScopeService, explorer: ProjectExplorerController) => {
//         // Arrange
//         let _before = explorer["_subscribers"] as Rx.IDisposable[];

//         // Act
//         explorer.$onDestroy();
//         let _after = explorer["_subscribers"] as Rx.IDisposable[];

//         // Assert
//         expect(_before.length).toEqual(2);
//         expect(_after.length).toEqual(0);
//     }));


//     it("Load project", inject(($rootScope: ng.IRootScopeService, projectManager: ProjectManager, explorer: ProjectExplorerController) => {
//         // Arrange
//         isReloadCalled = 0;
//         explorer.tree.reload = function (data: any[], id?: number) {
//             isReloadCalled += 1;
//         };


//         // Act
//         projectManager.loadProject(new Models.Project({ id: 1, name: "Project 1" }));
//         $rootScope.$digest();

//         // Assert
//         expect(isReloadCalled).toEqual(1);

//     }));
//     it("Load project children call", inject(($rootScope: ng.IRootScopeService, projectManager: IProjectManager, explorer: ProjectExplorerController) => {
//         // Arrange
//         isReloadCalled = 0;
//         explorer.tree.selectNode = function (id: number) {
//             isReloadCalled += 1;
//         };

//         // Act
//         projectManager.loadProject(new Models.Project({ id: 1, name: "Project 1" }));
//         $rootScope.$digest();

//         projectManager.loadArtifact({ id: 10 } as Models.IArtifact);
//         $rootScope.$digest();

//         // Assert
//         expect(isReloadCalled).toEqual(3);
//     }));

//     it("close project", inject((projectManager: ProjectManager, explorer: ProjectExplorerController) => {
//         // Arrange
//         isReloadCalled = 0;
//         explorer.tree.reload = function (data: any[], id?: number) {
//             isReloadCalled += 1;
//         };

//         // Act
//         projectManager.projectCollection.onNext([{ id: 1, name: "Project 1", artifacts: [] } as Project]);

//         projectManager.closeProject();

//         // Assert
//         expect(isReloadCalled).toEqual(1);

//     }));

//     it("doLoad", inject(($rootScope: ng.IRootScopeService, explorer: ProjectExplorerController) => {
//         // Arrange
//         isReloadCalled = 0;
//         explorer.tree.reload = function (data: any[], id?: number) {
//             isReloadCalled += 1;
//         };
//         explorer.doLoad(new Models.Project({ id: 1, name: "Project 1" }));
//         $rootScope.$digest();

//         // Act
//          //expect(isReloadCalled).toEqual(1);

//     }));

//     it("doLoad (nothing)", inject(($rootScope: ng.IRootScopeService, explorer: ProjectExplorerController) => {
//         // Arrange
//         isReloadCalled = 0;
//         explorer.tree.reload = function (data: any[], id?: number) {
//             isReloadCalled += 1;
//         };
//         explorer.doLoad(null);
//         $rootScope.$digest();

//         // Act
//         expect(isReloadCalled).toEqual(0);

//     }));

//     it("doSelect", inject(($rootScope: ng.IRootScopeService, explorer: ProjectExplorerController, selectionManager: SelectionManager) => {
//         // Arrange
//         isReloadCalled = 0;
//         explorer.tree.selectNode = function (id?: number) {
//             isReloadCalled += 1;
//         };
//         explorer["projectManager"].projectCollection.onNext([new Models.Project({ id: 1, name: "Project 1" })]);
//         $rootScope.$digest();
//         explorer.doSelect(({ id: 1, name: "Project 1"} as ITreeNode));
//         $rootScope.$digest();

//         // Act
//         const selectedArtifact = selectionManager.selection.artifact;

//         expect(selectedArtifact).toBeDefined();
//         expect(selectedArtifact.id).toBe(1);
//         expect(selectedArtifact.name).toBe("Project 1");


//     }));


//     it("close current project", inject(($rootScope: ng.IRootScopeService, projectManager: ProjectManager, selectionManager: SelectionManager) => {
//         // Arrange
//         isReloadCalled = 1;
//         projectManager.loadProject(new Models.Project({ id: 1, name: "Project 1" }));
//         $rootScope.$digest();
//         projectManager.loadProject(new Models.Project({ id: 2, name: "Project 2" }));
//         $rootScope.$digest();


//         // Act
//         projectManager.closeProject();
//         $rootScope.$digest();
//         let current = selectionManager.selection.artifact;

//         // Assert
//         expect(isReloadCalled).toBeTruthy();
//         expect(current).toBeDefined();
//         expect(projectManager.projectCollection.getValue.length).toEqual(0);

//     }));

//     it("close all projects", inject(($rootScope: ng.IRootScopeService, projectManager: ProjectManager, selectionManager: SelectionManager) => {
//         // Arrange
//         isReloadCalled = 1;
//         projectManager.loadProject(new Models.Project({ id: 2, name: "Project 1" }));
//         $rootScope.$digest();
//         projectManager.loadProject(new Models.Project({ id: 2, name: "Project 2" }));
//         $rootScope.$digest();

//         // Act
//         projectManager.closeProject(true);
//         $rootScope.$digest();
//         let current = selectionManager.selection.artifact;

//         // Assert
//         expect(current).toBeNull();
//         expect(projectManager.projectCollection.getValue.length).toEqual(0);

//     }));

// });
