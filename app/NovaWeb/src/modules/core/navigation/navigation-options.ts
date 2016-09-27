// Used for navigating to associated artifacts (included processes)
export class ForwardNavigationOptions {
    constructor(private _enableTracking: boolean) {}

    public get enableTracking(): boolean {
        return this._enableTracking;
    }
}

// Used for navigating to previously viewed artifacts (breadcrumb)
export class BackNavigationOptions {
    constructor(private _pathIndex: number) {}
    
    public get pathIndex(): number {
        return this._pathIndex;
    }
}