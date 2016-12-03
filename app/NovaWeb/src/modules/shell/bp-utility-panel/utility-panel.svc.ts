export enum PanelType {
    Properties,
    Relationships,
    Discussions,
    Files,
    History
}

export interface IUtilityPanelController {
    panelEnabled: boolean;
    openPanel(panelType: PanelType);
}

export interface IUtilityPanelService {
    panelEnabled: boolean;
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

    public get panelEnabled(): boolean {
        return this.ctrl ? this.ctrl.panelEnabled : false;
    }

    public set panelEnabled(value: boolean) {
        if (this.ctrl &&
            value !== this.ctrl.panelEnabled) {
            this.ctrl.panelEnabled = value;
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
