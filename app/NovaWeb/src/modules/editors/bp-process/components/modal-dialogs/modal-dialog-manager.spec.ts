import "Rx";
import {IModalDialogCommunication, ModalDialogCommunication} from "./modal-dialog-communication";
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
    let dm: IModalDialogCommunication;

    beforeEach(() => {
        dm = new ModalDialogCommunication();
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
        let h1 = dm.registerOpenDialogObserver(observer1.openDialog);
        let h2 = dm.registerOpenDialogObserver(observer2.openDialog);

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
        let h1 = dm.registerSetGraphObserver(observer1.setGraph);
        let h2 = dm.registerSetGraphObserver(observer2.setGraph);

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
        let h1 = dm.registerSetGraphObserver(observer1.setGraph);
        let h2 = dm.registerSetGraphObserver(observer2.setGraph);

        dm.removeSetGraphObserver(h1);

        // Act
        dm.setGraph(1);

        // Assert
        expect(observerSpy1).not.toHaveBeenCalled();
        expect(observerSpy2).toHaveBeenCalled();
    });

});

