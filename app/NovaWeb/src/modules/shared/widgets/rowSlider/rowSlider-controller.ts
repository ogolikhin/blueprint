export interface IRowSliderControllerApi {
    recalculate();
}

export interface IRowSliderController {
    api: IRowSliderControllerApi;
}

export class RowSliderController {
    static $inject: [string] = ["$scope", "$element"];

    public availableWidth: number;
    public slideSelector: string;

    private slides: NodeList;

    constructor(private $scope: ng.IScope, private $element: ng.IAugmentedJQuery) {
        this.slideSelector = !_.isString(this.slideSelector) ? "li" : this.slideSelector;
    }

    public $postLink = () => {
        this.recalculate();
    };

    public api: IRowSliderControllerApi = {
        recalculate: () => {
            this.recalculate();
        }
    };

    private recalculate = (): void => {
        this.$scope.$applyAsync(() => {
            if (!this.slides || !this.slides.length) {
                this.slides = this.getSlides();
            }

            if (this.slides.length) {
                const availableWidth = this.slides[0].parentElement.offsetWidth;
                console.log(availableWidth);
            }
        });
    };

    private getSlides = (): NodeList => {
        return this.$element[0].querySelectorAll(this.slideSelector);
    };
}
