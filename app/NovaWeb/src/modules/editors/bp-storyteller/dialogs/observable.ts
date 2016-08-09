export interface IObserver<T> {
    update(...arg: any[]);
}

export interface IObservable<T> {
    registerObserver(observer: any): void;
    removeObserver(observer: any): void;
    notifyObservers(...arg: any[]): void;
}


export class Observer<T> implements IObserver<T> {
    update(...arg: any[]) {
    }
}

export class Observable<T> implements IObservable<T> {
    private _observers: any;

    constructor() {
        this._observers = [];
    }

    public registerObserver(observer: any): void {
        for (var i = 0; i < this._observers.length; i++) {
            if (this._observers[i] === observer) {
                throw new Error("Observer registration duplcation error!");
            }
        }
        this._observers.push(observer);
    }

    public removeObserver(observer: any): void {
        var wasRemoved: boolean = false;
        for (var i = 0; i < this._observers.length; i++) {
            if (this._observers[i] === observer) {
                this._observers.splice(i, 1);
                wasRemoved = true;
                break;
            }
        }
        if (!wasRemoved) {
            throw new Error("The Observer was not registered!");
        }
    }

    public notifyObservers(...arg: any[]): void {
        this._observers.forEach((observer: any) => {
            try {
                observer(...arg);
            } catch (e) {
                this.removeObserver(observer);
            }
        });
    }
}
