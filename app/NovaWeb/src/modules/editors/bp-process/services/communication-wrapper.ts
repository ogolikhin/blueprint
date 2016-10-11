export interface ICommunicationWrapper {
    subscribe(observer: any): string;
    notify(param: any);
    disposeObserver(handler: string);
    dispose();
}

export class CommunicationWrapper implements ICommunicationWrapper {
    private subject: Rx.ReplaySubject<any>; 
    private handlersHash = [];
    constructor () {
        this.subject = new Rx.ReplaySubject<any>(0);
    }

    public subscribe(observer: any): string {
        let disposable: Rx.IDisposable = this.subject.subscribeOnNext(observer);
        let uuid = this.uuid();
        this.handlersHash[uuid] = disposable;
        return uuid;
    }

    public notify(param: any) {
        this.subject.onNext(param);
    }

    public disposeObserver(handler: string) {
        let disposable: Rx.IDisposable = this.handlersHash[handler];
        if (disposable != null) {
            disposable.dispose();
        }
    }

    public dispose() {
        for (let handler of this.handlersHash) {
            this.disposeObserver(handler);
        }
        this.handlersHash = [];
        this.subject.dispose();
    }

    private uuid(): string {
        return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, (c) => {
            var r = Math.random() * 16 | 0, v = c === "x" ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    }
}

