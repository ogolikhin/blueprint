import {ISession} from "./login/session.svc";
import {IUser} from "./login/auth.svc";
import {LoginCtrl, ILoginModalDialogData} from "./login/login.ctrl";
import {ISelectionManager} from "./../managers/selection-manager";
import {ISettingsService} from "../commonModule/configuration/settings.service";
import {INavigationService} from "../commonModule/navigation/navigation.service";
import {ILocalizationService} from "../commonModule/localization/localization.service";
import {ILoadingOverlayService} from "../commonModule/loadingOverlay/loadingOverlay.service";
import {IDialogService, IDialogSettings} from "../shared";
import {IUnpublishedArtifactsService} from "../editorsModule/unpublished/unpublished.service";
import {BPTourController} from "../main/components/dialogs/bp-tour/bp-tour";

export class AppComponent implements ng.IComponentOptions {
    // Inline template
    //public template: string = "<ui-view></ui-view>";

    // Template will be injected on the build time
    public template: string = require("./app.html");

    // 'External' template should ends with *.view.html to be copied to the dest folder
    //public templateUrl: string = "/modules/application/app.view.html"

    public controller: ng.Injectable<ng.IControllerConstructor> = AppController;
    public controllerAs = "app";
}

export class AppController {
    static $inject: [string] = [
        "navigationService",
        "selectionManager",
        "session",
        "settings",
        "$window",
        "localization",
        "loadingOverlayService",
        "dialogService",
        "publishService"];

    constructor(private navigationService: INavigationService,
                private selectionManager: ISelectionManager,
                private session: ISession,
                private settings: ISettingsService,
                private $window: ng.IWindowService,
                private localization: ILocalizationService,
                private loadingOverlayService: ILoadingOverlayService,
                private dialogService: IDialogService,
                private publishService: IUnpublishedArtifactsService) {


        this.$window.onbeforeunload = (e) => {
            const currentArtifact = selectionManager.getArtifact();
            if (currentArtifact && currentArtifact.artifactState.dirty) {
                //Show a Stay/Leave confirmation dialog if the current artifact is unsaved.
                //The message is only displayed in IE
                const windowMessage = localization.get("App_CloseTabWithUnsavedChanges");
                e = e || this.$window.event;
                e.returnValue = windowMessage;
                return windowMessage;
            }
        };

    }

    public get currentUser(): IUser {
        return this.session.currentUser;
    }

    public isDatabaseUser(): boolean {
        return this.session.currentUser.source === 0;
    }

    public logout(evt: ng.IAngularEvent) {
        const id = this.loadingOverlayService.beginLoading();
        if (evt) {
            evt.preventDefault();
        }
        this.publishService.getUnpublishedArtifacts().then((unpublishedArtifactSet) => {
            if (unpublishedArtifactSet.artifacts.length > 0) {
                const dialogMessage = this.localization.get("App_ConfirmLogout_WithUnpublishedArtifacts")
                    .replace(`{0}`, unpublishedArtifactSet.artifacts.length.toString());
                return this.dialogService.alert(dialogMessage, null, "App_ConfirmLogout_Logout", "App_ConfirmLogout_Cancel")
                    .then((success) => { return this.navigationService.navigateToLogout(); });
            } else {
                return this.navigationService.navigateToLogout();
            }
        }).finally(() => {
            this.loadingOverlayService.endLoading(id);
        });
    }

    public changePassword(evt?: ng.IAngularEvent) {
        if (evt) {
            evt.preventDefault();
        }

        const dialogSettings: IDialogSettings = {
            template: require("./login/changePassword.html"),
            css: "nova-login change-password",
            controller: LoginCtrl,
            controllerAs: "ctrl",
            backdrop: true,
            okButton: this.localization.get("App_Button_Ok"),
            cancelButton: this.localization.get("App_Button_Cancel")
        };
        const dialogData: ILoginModalDialogData = {
            isChangePasswordScreenEnabled: true,
            changePasswordScreenMessage: "" //this.localization.get("Change_Password_Dialog_Message")
        };

        this.dialogService.open(dialogSettings, dialogData);
    }

    public navigateToHelpUrl(evt: ng.IAngularEvent) {
        evt.preventDefault();

        //We want to open a new window, not a tab, to match old Silverlight behaviour.
        this.popUpWindowInCenterOfParent(this.settings.get("HelpURL"), "_blank", 1300, 800, this.$window);
    }

    public openTour (evt?: ng.IAngularEvent) {
        if (evt) {
            evt.preventDefault();
        }
        this.dialogService.open(<IDialogSettings>{
            template: require("../main/components/dialogs/bp-tour/bp-tour.html"),
            controller: BPTourController,
            backdrop: true,
            css: "nova-tour"
        });
    }

    private popUpWindowInCenterOfParent(url: string, title: string, width: number, height: number, $window: ng.IWindowService) {
        //Calculate position for new window based on parent's center. http://stackoverflow.com/a/5681473
        const parentLeft: number = $window.screenLeft ? $window.screenLeft : $window.screenX;
        const parentTop: number = $window.screenTop ? $window.screenTop : $window.screenY;
        const parentCenterX: number = parentLeft + ($window.outerWidth / 2);
        const parentCenterY: number = parentTop + ($window.outerHeight / 2);
        const left: number = parentCenterX - (width / 2);
        const top: number = parentCenterY - (height / 2);

        //Note: Hiding the URL bar is no longer possible in most browsers (security feature).
        //Note2: Chrome ignores 'width' if you don't also specify 'height
        // tslint:disable-next-line: max-line-length
        const windowFeatures: string = "toolbar = no, location = no, directories = no, status = no, menubar = no, titlebar = no, scrollbars = yes, resizable = yes, copyhistory = no, width = " + width + ", height = " + height + ", top = " + top + ", left = " + left;

        return $window.open(url, title, windowFeatures);
    }

}
