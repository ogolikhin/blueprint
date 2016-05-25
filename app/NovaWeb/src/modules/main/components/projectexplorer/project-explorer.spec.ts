import "angular";
import "angular-mocks";
import { NotificationService} from "../../../core/notification";
import {ILocalizationService} from "../../../core/localization";
import {IBPTreeController, ITreeNode, BPTreeController } from "../../../core/widgets/bp-tree/bp-tree";
import {ProjectRepositoryMock} from "../../services/project-repository.mock";
import {ProjectManager, Models, SubscriptionEnum} from "../../managers/project-manager";
import {ProjectExplorerController} from "./project-explorer"

class BPTreeControllerMock implements IBPTreeController {
    private add(id: number) {
        return {
            id: id,
            name: `Artifact ${id}`,
            type: 1,
        } as ITreeNode;

    }
    public _datasource: ITreeNode[] = [];
    public addNode(data: any[], index?: number, propertyMap?: any) {
        for (let i = 0; i < 10; i++) {
            this._datasource.push(this.add(i));
        }
    }
    
    public addNodeChildren(id: number, data: any[], propertyMap?: any) {
        let node = this._datasource[0];
        node.children = [];
        for (let i = 100; i < 105; i++) {
            this._datasource.push(this.add(i));
        }
        node.hasChildren = true;
        node.loaded = true;
    }

    public removeNode(id: number) {
        this._datasource = this._datasource.filter(function (it) {
            return it.id !== id;
        });
    }

    public selectNode(id: number) { }

    public refresh() {}
}



describe("Project Explorer Test", () => {

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("projectRepository", ProjectRepositoryMock);
        $provide.service("notification", NotificationService);
        $provide.service("manager", ProjectManager);
    }));

    describe("Load project: ", () => {
        it("load project", inject(($rootScope: ng.IRootScopeService, manager: ProjectManager) => {
            let explorer = new ProjectExplorerController(manager);
            explorer.tree = new BPTreeControllerMock();
            manager.notify(SubscriptionEnum.ProjectLoaded, {
                id: 1,
                name: "Project 1",
                artifacts: []
            } as Models.IProject);
        }));
        
        //TODO: finish unit test

    });

    describe("Remove project:", () => {

    });


});