
export interface ILocalStorageService {

    read(path: string): any;
    readObject<T>(path): T;
    write(path: string, text: string): void;
    writeObject(path: string, data: any): void;
    remove(path: string): void;
    clear(): void;
}

export class LocalStorageService implements ILocalStorageService {
    
    public static $inject = [
        "$log"       
    ];

    constructor(private $log: ng.ILogService) {
    }

    public read(path: string): any {      
        const text: string = localStorage.getItem(path);
        if (_.isNil(text)) {
            this.$log.debug("LocalStorageService::read(" + path + ") - path not found, returned null");
            return null;
        }
        else {
            this.$log.debug("LocalStorageService::read(" + path + ")");
            return text;
        }
    }

    public readObject<T>(path: string): T {
        const text: any = this.read(path);
        let data: T;
        try {
            data = <T>JSON.parse(text);
        } catch (error) {
            this.$log.error("LocalStorageService::readObject: can't convert string from local storage to object using JSON.parse(). Error: " + error);
            data = null;
        }

        return data;
    }

    public write(path: string, text: string): void {
        this.$log.debug("LocalStorageService::write(" + path + ")");
        localStorage.setItem(path, text);
    }

    public writeObject(path: string, data: any): void {
        const text: string = JSON.stringify(data);
        this.write(path, text);
    }

    public remove(path: string): void {
        this.$log.debug("LocalStorageService::remove(" + path + ")");
        localStorage.removeItem(path);
    }

    public clear(): void {
        this.$log.debug("LocalStorageService::clear - all items removed from local storage");
        localStorage.clear();
    }
}
