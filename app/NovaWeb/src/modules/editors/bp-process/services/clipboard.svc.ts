
export enum ClipboardDataType {
    Process,
    Unknown
}

export interface IClipboardData {
    type: ClipboardDataType;
    data: any;
    dispose();
}

export interface IClipboardService {
    getData(): IClipboardData;
    setData (newVal: IClipboardData): void;
    clearData();
}

export class Clipboard implements IClipboardService {
    private _data: IClipboardData;

    public getData(): IClipboardData {
        return this._data;
    }

    public setData (newVal: IClipboardData): void {
        this.clearData();
        this._data = newVal;
    }

    public clearData(): void {
        if (!!this._data) {
            this._data.dispose();
        }
    } 
}
