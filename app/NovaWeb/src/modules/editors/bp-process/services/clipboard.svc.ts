
export enum ClipboardDataType {
    Process,
    Unknown
}

export interface IClipboardData { 
    getType(): ClipboardDataType;
    getData: any;
    dispose();
}

export interface IClipboardService {
    getData(): IClipboardData;
    setData (newVal: IClipboardData): void;
    getDataType(): ClipboardDataType;
    isEmpty(): boolean;
    clearData();
}

export class ClipboardService implements IClipboardService {
    private _data: IClipboardData;

    public getData(): IClipboardData {
        const deepClone = _.cloneDeep(this._data);
        return deepClone;
    }

    public setData (newVal: IClipboardData): void {
        this.clearData();
        this._data = _.cloneDeep(newVal);
    }

    public getDataType(): ClipboardDataType {
        if (this.isEmpty()) {
            return ClipboardDataType.Unknown;
        }
        return this._data.getType();
    }

    public isEmpty(): boolean {
        return !this._data || !this._data.getData();
    }    

    public clearData(): void {
        if (!!this._data) {
            this._data.dispose();
            this._data = null;
        }
    } 
}
