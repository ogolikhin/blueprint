import "angular";

export class GlossaryState implements ng.ui.IState {
    public template = "<bp-glossary></bp-glossary>";

    public onEnter = () => {
        let enter = "test";
    };

    public onExit = () => {
        let ex = "test";
    };
}