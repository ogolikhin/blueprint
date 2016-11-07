export interface INavigationPathItem {
    id: number;
    version?: number;
}

export interface INavigationState {
    id?: number;
    version?: number;
    path?: INavigationPathItem[];
}