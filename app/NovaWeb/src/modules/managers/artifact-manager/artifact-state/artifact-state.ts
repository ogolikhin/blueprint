
export interface IState {
    locked : boolean;
    readonly: boolean;
    dirty: boolean;
    published: boolean;

} 

export interface IArtifactState {

} 


export class ArtifactState implements IArtifactState{
    private _lock: any;
    private _state: IState;
    private subject: Rx.BehaviorSubject<IState>;

    constructor() {
        this._state = {} as IState;
        this.subject = new Rx.BehaviorSubject<IState>(null);

    }

    public get observable(): Rx.IObservable<IState> {
        return this.subject.filter(it => it !== null).asObservable();
    }    


    public list: StateEnum[];
    
    public set lock(value: any) {
        this._lock = value;
    }

    public set readonly(value:boolean){
        this._state.readonly = value;
        this.subject.onNext(this._state);

    }
    public dirty: boolean;
    public published: boolean;


}