import * as Models from "../models/models";
import  "./interfaces" 

export interface IStatefulArtifactManagerService {
    list(): IStatefulArtifact[];
    add(artifact: IStatefulArtifact);
    get(id: number): IStatefulArtifact;
    // getObeservable(id: number): Rx.Observable<IStatefulArtifact>;
    remove(id: number);
    removeAll(); // when closing all projects
    refresh(id); // refresh lightweight artifact
    refreshAll(id);
}

this.get(10).getAttachment().observable();
this.get(10).getAttachment().value(); // ng.Promiss<IAttachment[]>
this.get(10).getAttachment().remove(attachment: IAttachment); // ng.Promiss<IAttachment[]>
this.get(10).getAttachment().add(attachment: IAttachment); // ng.Promiss<IAttachment[]>
this.get(10).getAttachment().changes(); // ng.Promiss<IAttachment[]>

this.get(10).properties.get('name')
this.get(10).getState().list()
this.get(10).getState().locked()
this.get(10).getState().dirty()
this.get(10).getState().published()
this.get(10).getState().readonly()
this.get(10).getState().observable()

this.get(10).getState().addChangeSet()

this.get(10).getStateObservable()

this.get(10).value(); // IArtifact


// this.get(10).getHistory()

interface IAttachments extends IBlock<IAttachment> {

}

class StatefullArtifact {

    /**
     *
     */
    constructor(private superFactory: ISuperFactory) {

    }

    private attachemnts: IAttachments;
    public getAttachment() {
        if (!this.attachemnts) {
            this.attachemnts = new Attachment(<IFullState>null, this.superFactory.attachmentFactory));
        }
    }

}

interface ISuperFactory {

}

interface ChangeTracker {

}

interface ILimitedState {

}

interface IFullState extends ILimitedState {

}

class ItemState implements IFullState{

}

class Attachment implements IAttachments {

    /**
     *
     */
    constructor(state: IFullState, svc: IAttachmentService) { //Lock

    }
    private state;

    private _attachment: IAttachment[] = null;

    public value() {
        if (!this._attachment) {
            //Call to server
        }
        return this._attachment;
    }

    public add(){
        //locking
        //IStatefulArtifact.Islo
        //onNext
    }
}

interface IBlock<T> {
    remove(T): ng.Promiss<T[]>
    update(T): ng.Promiss<T[]>
    add(T): ng.Promiss<T[]>
}

