import {IStatefulArtifact, IStatefulSubArtifact} from "../../managers/artifact-manager";

export enum PanelType {
    Properties,
    Relationships,
    Discussions,
    Files,
    History
}

export interface IOnPanelChangesObject extends ng.IOnChangesObject {
    context: ng.IChangesObject<IUtilityPanelContext>;
}

export interface IUtilityPanelContext {
    artifact?: IStatefulArtifact;
    subArtifact?: IStatefulSubArtifact;
    panelType: PanelType;
}

export interface IUtilityPanelController extends ng.IController {
    openPanel(panelType: PanelType);
}

export interface IUtilityPanelService {
    isUtilityPanelOpened: boolean;
    openPanel(panelType: PanelType);
    openPanelAsync(panelType: PanelType);
}

export class UtilityPanelService implements IUtilityPanelService {

    public isUtilityPanelOpened: boolean;
    private ctrl: IUtilityPanelController;

    public static $inject: [string] = [
        "$rootScope"
    ];

    constructor(private $rootScope: ng.IRootScopeService) {

    }

    public initialize(ctrl: IUtilityPanelController) {
        this.ctrl = ctrl;
    }

    public openPanel(panelType: PanelType) {
        this.isUtilityPanelOpened = true;
        this.ctrl.openPanel(panelType);
    }

    /**
     * Opens Utility Panel by panel type
     * 
     * The method can be called outside AngularJS digest cycle,
     * since $rootScope.$applyAsync is used 
     * 
     * @memberOf UtilityPanelService
     */
    public openPanelAsync(panelType: PanelType) {
        this.$rootScope.$applyAsync(() => {
            this.openPanel(panelType);
        });
    }

}
