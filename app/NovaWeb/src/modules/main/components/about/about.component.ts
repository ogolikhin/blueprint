import "angular";

export class PageAboutComponent implements ng.IComponentOptions {
    public template: string = require("./about.html");

    public controller: Function = AboutCtrl;
}

class AboutCtrl
{
    public oneAtATime = true;

    public groups = [
        {
            title: 'Dynamic Group Header - 1',
            content: 'Dynamic Group Body - 1'
        },
        {
            title: 'Dynamic Group Header - 2',
            content: 'Dynamic Group Body - 2'
        }
    ];

    public items = ['Item 1', 'Item 2', 'Item 3'];

    public addItem() {
        var newItemNo = this.items.length + 1;
        this.items.push('Item ' + newItemNo);
    };

    public status = {
        isFirstOpen: true,
        isFirstDisabled: false
    };
}
