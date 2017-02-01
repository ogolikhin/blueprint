import {IBPButtonOrDropdownAction, BPButtonOrDropdownSeparator} from "../actions";

export class BPToolbarMenu implements ng.IComponentOptions {
    public controller: ng.Injectable<ng.IControllerConstructor> = BPToolbarMenuController;
    public template: string = require("./bp-toolbar-menu.html");
    public bindings: {[boundProperty: string]: string} = {
        icon: "@",
        actions: "<",
        disabled: "=?",
        tooltip: "@?"
    };
}

export class BPToolbarMenuController implements ng.IComponentController {
    public icon: string;
    public actions: IBPButtonOrDropdownAction[];
    public disabled: boolean;
    public tooltip?: string;

    constructor() {
        // Pre filter items in actions list for easier checks
        this.actions = _.filter(this.actions, (action: any) => {
            return !action.disabled || (action.disabled && action.separator);
        });

        // Above looping can still potentially leave a separator as a last item so the following check is required
        let last = <BPButtonOrDropdownSeparator>_.last(this.actions);
        if (last.separator) {
            this.actions.pop();
        }
    }
}
