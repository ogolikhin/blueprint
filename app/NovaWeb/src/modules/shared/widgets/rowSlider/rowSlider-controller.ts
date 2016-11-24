export interface IRowSliderControllerApi {
    getWrapperElement(): HTMLElement;
    updateWidth(availableWidth?: number);
}

export interface IRowSliderController {
    api: IRowSliderControllerApi;
}

export class RowSliderController {
    static $inject: [string] = ["$scope", "$element", "$templateCache", "$compile"];

    public slideSelector: string;
    public showButtons: boolean;

    private slides: HTMLElement[];
    private slidesWidth: number[];
    private slidesContainer: HTMLElement;
    private slidesTotalWidth: number;
    private availableWidth: number;
    private scrollPosition: number;
    private scrollIndex: number;

    constructor(private $scope: ng.IScope,
                private $element: ng.IAugmentedJQuery,
                private $templateCache: ng.ITemplateCacheService,
                private $compile: ng.ICompileService) {
        this.slideSelector = !_.isString(this.slideSelector) ? "li" : this.slideSelector;
        this.slidesTotalWidth = 0;
        this.scrollPosition = 0;

        this.showButtons = false;
    }

    public $onDestroy = () => {
        this.api = null;
    };

    public $postLink = () => {
        this.setupSlides();
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

    public showButtonPrev(): boolean {
        return this.scrollIndex > 0;
    }

    public showButtonNext(): boolean {
        return this.scrollIndex < this.slidesWidth.length - 1;
    }

    private moveSlide(direction: number) {
        let scrollPosition = 0;
        let idx = this.scrollIndex;
        const max = this.slidesWidth.length - 1;

        idx += direction;
        idx = idx < 0 ? 0 : (idx > max ? max : idx);
        this.scrollIndex = idx;

        for (let i = 0; i < this.scrollIndex; i++) {
            scrollPosition += this.slidesWidth[i];
        }
        this.slidesContainer.style.left = "-" + scrollPosition.toString() + "px";
    }

    public previousSlide = (): void => {
        this.moveSlide(-1);
    };

    public nextSlide = (): void => {
        this.moveSlide(1);
    };

    private setupSlides = (): void => {
        this.$scope.$applyAsync(() => {
            const template = this.$templateCache.get("rowSliderWrapper.html") as string;
            const wrapper = this.$compile(template)(this.$scope)[0] as HTMLElement;
            const container = wrapper.querySelector(".row-slider__container") as HTMLElement;

            this.slides = this.getSlides();
            this.slidesContainer = this.slides[0].parentElement;
            if (this.slidesContainer) {
                this.slidesContainer.parentElement.insertBefore(wrapper, this.slidesContainer);
                container.appendChild(this.slidesContainer);
                this.slidesTotalWidth = 0;
                this.scrollPosition = 0;
                this.scrollIndex = 0;
                this.slidesContainer.classList.add("row-slider__content");

                for (let i = 0; i < this.slides.length; i++) {
                    const slide = this.slides[i] as HTMLElement;
                    slide.classList.add("row-slider__slide");
                }
            }
        });
    };

    private recalculate = (availableWidth?: number): void => {
        this.$scope.$applyAsync(() => {
            if (this.slides.length) {
                if (_.isFinite(availableWidth)) {
                    this.availableWidth = availableWidth;
                } else {
                    this.availableWidth = this.slidesContainer.offsetWidth;
                }

                this.slidesTotalWidth = 0;
                this.slidesWidth = [];

                for (let i = 0; i < this.slides.length; i++) {
                    const slide = this.slides[i] as HTMLElement;
                    const rect = slide.getBoundingClientRect();
                    this.slidesTotalWidth += rect.width;
                    this.slidesWidth.push(rect.width);
                }

                this.showButtons = this.slidesTotalWidth > this.availableWidth;
            }
        });
    };

    private getSlides = (): HTMLElement[] => {
        let slides: HTMLElement[] = [];

        // we use querySelector instead of querySelectorAll so we can make sure we get only real siblings
        // of the first matched element by iterating with nextElementSibling
        let slide: HTMLElement = this.$element[0].querySelector(this.slideSelector) as HTMLElement;
        const tag = slide.tagName.toUpperCase();
        while (slide) {
            // we need to make sure that all the slides are the same type of element
            if (slide.tagName.toUpperCase() !== tag) {
                return [] as HTMLElement[];
            }
            slides.push(slide);
            slide = slide.nextElementSibling as HTMLElement;
        }

        return slides;
    };
}
