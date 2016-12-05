export enum PanelType {
    Properties,
    Relationships,
    Discussions,
    Files,
    History
}

export interface IUtilityPanelController {
    disableUtilityPanel();
    openPanel(panelType: PanelType);
}

export interface IUtilityPanelService {
    disableUtilityPanel();
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

    public disableUtilityPanel() {
        if (this.ctrl) {
            this.ctrl.disableUtilityPanel();
        }
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
