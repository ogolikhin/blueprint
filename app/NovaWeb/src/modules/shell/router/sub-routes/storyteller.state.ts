import "angular";

export class StorytellerState implements ng.ui.IState  {
    public template = "<bp-storyteller-editor></bp-storyteller-editor>";

    public onEnter = () => {
        let enter = "test";
        console.log("storyteller");
    };

    public onExit = () => {
        let ex = "test";
    };
}
