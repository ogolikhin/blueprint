
export enum ClipboardDataType {
    Process,
    Unknown
}

export interface IClipboardData {
    type: ClipboardDataType;
    data: any;
    clearData();
}

export interface IClipboard {
    getData(): IClipboardData;
    setData (newVal: IClipboardData): void;
    clearData();
}

export class Clipboard implements IClipboard {
    private _data: IClipboardData;

    public getData(): IClipboardData {
        return this._data;
    }

    public setData (newVal: IClipboardData): void {
        this._data = newVal;
    }

    clearData(): void {
        this._data.clearData();
    } 
}
