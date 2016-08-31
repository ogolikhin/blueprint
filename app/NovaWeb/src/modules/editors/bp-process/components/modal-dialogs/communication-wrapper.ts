export interface ICommunicationWrapper {
    subscribe(observer: any);
    notify(param: any);
    disposeObserver(observer: any);
    dispose();
}

export class CommunicationWrapper implements ICommunicationWrapper {
    private subject : Rx.ReplaySubject<any>; 
    private observersHash = [];
    constructor () {
        this.subject = new Rx.ReplaySubject<any>(0);
    }

    public subscribe(observer: any) {
        this.disposeObserver(observer);
        let disposable: Rx.IDisposable = this.subject.subscribeOnNext(observer);
        this.observersHash[observer.toString()] = disposable;
    }

    public notify(param: any) {
        this.subject.onNext(param);
    }

    public disposeObserver(observer: any) {
        let disposable: Rx.IDisposable = this.observersHash[observer.toString()];
        if (disposable != null) {
            disposable.dispose();
        }
    }

    public dispose() {
        this.subject.dispose();
    }
}

