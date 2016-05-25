import "angular";
import {ILocalizationService} from "../core/localization";
import {INotificationService} from "../core/notification";
import {IDialogService} from "../services/dialog.svc";


export class MainViewComponent implements ng.IComponentOptions {
    public template: string = require("./main.view.html");
    public controller: Function = MainViewController;
    public transclude: boolean = true;
}

export interface IMainViewController {
    
}

export class MainViewController implements IMainViewController {

    public static $inject: [string] = ["notification", "dialogService"];
    constructor(private _notification: INotificationService, private dialogService: IDialogService) {
        this._notification.attach("main", "exception", this.showError.bind(this));
    }

    
    private showError(error: any) {
        this.dialogService.alert(`Error: ${error["message"] || ""}`);
    }
}
