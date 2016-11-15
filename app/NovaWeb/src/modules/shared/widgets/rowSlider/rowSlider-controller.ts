export interface IRowSliderControllerApi {
    getWrapperElement(): HTMLElement;
    updateWidth(availableWidth?: number);
}

export interface IRowSliderController {
    api: IRowSliderControllerApi;
}

export class RowSliderController {
    static $inject: [string] = ["$scope", "$element"];

    public slideSelector: string;

    private slides: HTMLElement[];
    private slideContainer: HTMLElement;
    private availableWidth: number;

    constructor(private $scope: ng.IScope, private $element: ng.IAugmentedJQuery) {
        this.slideSelector = !_.isString(this.slideSelector) ? "li" : this.slideSelector;
    }

    public $onDestroy = () => {
        this.api = null;
    };

    public $postLink = () => {
        this.recalculate();
    };

    public api: IRowSliderControllerApi = {
        getWrapperElement: (): HTMLElement => {
            return this.$element[0].firstElementChild as HTMLElement;
        },
        updateWidth: (availableWidth?: number) => {
            this.recalculate(availableWidth);
        }
    };

    public previousSlide = (): void => {
        // temporary
    };

    public nextSlide = (): void => {
        // temporary
    };

    private recalculate = (availableWidth?: number): void => {
        this.$scope.$applyAsync(() => {
            if (!this.slides || !this.slides.length) {
                this.slides = this.getSlides();
                this.slideContainer = this.slides[0].parentElement;
            }

            if (this.slides.length) {
                if (_.isFinite(availableWidth)) {
                    this.availableWidth = availableWidth;
                } else {
                    this.availableWidth = this.slideContainer.offsetWidth;
                }

                // console.log(this.availableWidth);
            }
        });
    };

    private getSlides = (): HTMLElement[] => {
        let slides: HTMLElement[] = [];

        // we use querySelector instead of querySelectorAll so we can make sure we get only real siblings
        // of the first matched element by iterating with nextElementSibling
        let slide: HTMLElement = this.$element[0].querySelector(this.slideSelector) as HTMLElement;
        while (slide) {
            slides.push(slide);
            slide = slide.nextElementSibling as HTMLElement;
        }

        return slides;
    };
}
