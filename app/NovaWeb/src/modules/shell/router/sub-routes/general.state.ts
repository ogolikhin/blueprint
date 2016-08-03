import "angular";

export class GeneralState implements ng.ui.IState {
    public template = "<bp-general-editor></bp-general-editor>";

    public onEnter = () => {
        let enter = "test";
        console.log("general");
    };

    public onExit = () => {
        let ex = "test";
    };
}