import {IWindowManager, IMainWindow, ResizeCause} from "./window-manager";

export class MainWindowMock implements IMainWindow {
    public width: number;
    public height: number;
    public contentWidth: number;
    public contentHeight: number;
    public isLeftSidebarOpen: boolean;
    public isRightSidebarOpen: boolean;
    public causeOfChange: ResizeCause;
}

export class WindowManagerMock implements IWindowManager {
    private subject: Rx.BehaviorSubject<IMainWindow>;

    constructor() {
        this.subject = new Rx.BehaviorSubject<IMainWindow>(new MainWindowMock());
    }

    public get mainWindow(): Rx.Observable<IMainWindow> {
        return this.subject.asObservable();
    }
}