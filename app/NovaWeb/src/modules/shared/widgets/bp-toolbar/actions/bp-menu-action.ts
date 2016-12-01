import {IBPAction} from "./bp-action";

export interface IBPButtonOrDropdownAction {
    execute: () => void;
    icon?: string;
    disabled?: boolean;
    label?: string;
    tooltip?: string;
}

export class BPButtonOrDropdownAction implements IBPButtonOrDropdownAction {
    constructor(private _execute?: () => void,
                private _canExecute?: () => boolean,
                private _icon?: string,
                private _tooltip?: string,
                private _label?: string) {
    }

    public get execute(): () => void {
        return this._execute;
    }

    public get icon(): string {
        return this._icon;
    }

    public get disabled(): boolean {
        return this._canExecute && !this._canExecute();
    }

    public get label(): string {
        return this._label;
    }

    public get tooltip(): string {
        return this._tooltip;
    }
}

export class BPButtonOrDropdownSeparator extends BPButtonOrDropdownAction {
    public separator: boolean = true;

    constructor() {
        super();
    }
}

export interface IBPMenuAction extends IBPAction {
    icon: string;
    actions: IBPButtonOrDropdownAction[];
    tooltip?: string;
}

export class BPMenuAction implements IBPMenuAction {
    private _actions: IBPButtonOrDropdownAction[];

    constructor(private _tooltip?: string, ...actions: IBPButtonOrDropdownAction[]) {
        this._actions = actions;
    }

    public get type(): string {
        return "menu";
    }

    public get icon(): string {
        return "fonticon2-more-menu";
    }

    public get actions(): IBPButtonOrDropdownAction[] {
        return this._actions;
    }

    public get tooltip(): string {
        return this._tooltip;
    }
}
