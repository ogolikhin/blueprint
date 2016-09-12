import { IProjectRepository, ProjectRepository } from "./project-repository";
import { IArtifactService, ArtifactService } from "./artifact.svc";
import { IProjectManager, ProjectManager } from "./project-manager";
import { ISelectionManager, SelectionManager, SelectionSource, ISelection } from "./selection-manager";
import { IWindowManager, WindowManager, IMainWindow, ResizeCause } from "./window-manager";

angular.module("bp.main.services", [])
    .service("projectRepository", ProjectRepository)
//    .service("projectManager", ProjectManager)
    .service("selectionManager", SelectionManager)
    .service("artifactService", ArtifactService)
    .service("windowManager", WindowManager);

export {
    IProjectRepository, ProjectRepository,
    IArtifactService, ArtifactService,
    IProjectManager, ProjectManager,
    ISelectionManager, ISelection, SelectionManager, SelectionSource,
    IWindowManager, WindowManager, IMainWindow, ResizeCause
}