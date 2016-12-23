import {IBPAction} from "./bp-action";

export interface IBPButtonAction extends IBPAction {
    icon: string;
    tooltip: string;
    disabled: boolean;
    label?: string;
    
    execute(): void;
}

export abstract class BPButtonAction implements IBPButtonAction {
    public get type(): string {
        return "button";
    }

    public abstract get icon(): string;

    public abstract get tooltip(): string;

    public abstract get disabled(): boolean;

    public abstract execute(): void;
}
