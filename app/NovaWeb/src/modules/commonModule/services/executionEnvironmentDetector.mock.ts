export class ExecutionEnvironmentDetectorMock {
    private browserInfo: any;

    constructor() {
        this.browserInfo = {msie: false, firefox: false, version: 0};
    }

    public getBrowserInfo(): any {
        return this.browserInfo;
    }
}