export interface IAppConstants {
    draftVersion: number;
}

export class AppConstants implements IAppConstants {
    public draftVersion: number = 2147483647;
}
