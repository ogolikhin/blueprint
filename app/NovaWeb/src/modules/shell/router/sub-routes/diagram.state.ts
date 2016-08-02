import "angular";

export class DiagramState implements ng.ui.IState {
    public template = "<bp-diagram></bp-diagram>";

    public onEnter = () => {
        let enter = "test";
        console.log("diagram");
    };

    public onExit = () => {
        let ex = "test";
    };
}