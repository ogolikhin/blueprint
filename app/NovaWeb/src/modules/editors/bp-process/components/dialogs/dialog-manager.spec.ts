import {IDialogManager, DialogManager} from "./dialog-manager";
import {ModalDialogType} from "./base-modal-dialog-controller";

class Observer1 {
    public getGraph: () => any;
    public setGraph = (graph) => {
        this.getGraph = graph;
    }
    
    public openDialog = (id: number, dialogType: ModalDialogType) => {
        this.somePrivateFunc1();
    }

    private somePrivateFunc1() {

    }
}

class Observer2 {
    public getGraph: () => any;
    public setGraph = (graph) => {
        this.getGraph = graph;
    }
    
    public openDialog = (id: number, dialogType: ModalDialogType) => {
        this.somePrivateFunc2();
    }

    private somePrivateFunc2() {

    }
}

describe("DialogManager test", () => {

    let dm: IDialogManager;

    beforeEach(() => {
        dm = new DialogManager();
    });

    afterEach(() => {
        dm.onDestroy();
        dm = null;
    });

    it("OpenDialog observable", () => {
        let observer1 = new Observer1();
        let observer2 = new Observer2();
        let observerSpy1 = spyOn(observer1, "somePrivateFunc1");
        let observerSpy2 = spyOn(observer2, "somePrivateFunc2");
        dm.registerOpenDialogObserver(observer1.openDialog);
        dm.registerOpenDialogObserver(observer2.openDialog);

        // Act
        dm.openDialog(1, 0);

        // Assert
        expect(observerSpy1).toHaveBeenCalled();
        expect(observerSpy2).toHaveBeenCalled();
    });

    it("SetGraph  observable", () => {
        // Arrange
        let observer1 = new Observer1();
        let observer2 = new Observer2();
        let observerSpy1 = spyOn(observer1, "setGraph");
        let observerSpy2 = spyOn(observer2, "setGraph");
        dm.registerSetGraphObserver(observer1.setGraph);
        dm.registerSetGraphObserver(observer2.setGraph);

        // Act
        dm.setGraph(1);

        // Assert
        expect(observerSpy1).toHaveBeenCalled();
        expect(observerSpy2).toHaveBeenCalled();
    });

    it("do not notify a removed observer", () => {
        // Arrange
        let observer1 = new Observer1();
        let observer2 = new Observer2();
        let observerSpy1 = spyOn(observer1, "setGraph");
        let observerSpy2 = spyOn(observer2, "setGraph");
        dm.registerSetGraphObserver(observer1.setGraph);
        dm.registerSetGraphObserver(observer2.setGraph);

        dm.removeSetGraphObserver(observer1.setGraph)

        // Act
        dm.setGraph(1);

        // Assert
        expect(observerSpy1).not.toHaveBeenCalled();
        expect(observerSpy2).toHaveBeenCalled();
    });

});

