import "angular";
import { IProjectManager } from "../../main/services/project-manager";

export class StorytellerState implements ng.ui.IState  {
    // public url = "storyteller";
    public template = "<div>Main.Artifact.Storyteller</div>";
    public controller = "storytellerStateController";

    public onEnter = () => {
        let enter = "test";
    };

    public onExit = () => {
        let ex = "test";
    };
}

export class StorytellerStateController {

    public static $inject = ["$rootScope", "$state", "projectManager"];

    constructor(
        private $rootScope,  
        private $state: any, 
        private projectManager: IProjectManager ) {

            var test = "test";
    }
}