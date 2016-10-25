import { ISession } from "./login/session.svc";
import { IUser } from "./login/auth.svc";
import { ISettingsService } from "./../core";
import { INavigationService } from "./../core/navigation";
import { IProjectManager } from "./../managers/project-manager/";
import { ISelectionManager } from "./../managers/selection-manager";

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
    static $inject: [string] = ["navigationService", "projectManager", "selectionManager", "session", "settings", "$window"];

    constructor(private navigation: INavigationService,
                private projectManager: IProjectManager,
                private selectionManager: ISelectionManager,
                private session: ISession,
                private settings: ISettingsService,
                private $window: ng.IWindowService) {
    }

    public get currentUser(): IUser {
        return this.session.currentUser;
    }

    public logout(evt: ng.IAngularEvent) {
        evt.preventDefault();
        this.session.logout().finally(() => {
            this.navigation.navigateToMain().finally(() => {
                this.projectManager.removeAll();
                this.selectionManager.clearAll();
                this.session.ensureAuthenticated();
            });
        });
    }

    public navigateToHelpUrl(evt: ng.IAngularEvent) {
        evt.preventDefault();

        //We want to open a new window, not a tab, to match old Silverlight behaviour.
        this.popUpWindowInCenterOfParent(this.settings.get("HelpURL"), "_blank", 1300, 800, this.$window);
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
        /* tslint:disable */
        const windowFeatures: string = "toolbar = no, location = no, directories = no, status = no, menubar = no, titlebar = no, scrollbars = no, resizable = yes, copyhistory = no, width = " + width + ", height = " + height + ", top = " + top + ", left = " + left;
        /*tslint:enable*/

        return $window.open(url, title, windowFeatures);
    }

}
