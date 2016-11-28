import {IWindowManager} from "../../../main/services/window-manager";

enum SlidePosition {
    HiddenLeft = -1,
    HiddenRight = 1,
    Visible = 0
}

export class TabSliderController {
    static $inject: [string] = ["$scope", "$element", "$templateCache", "$compile", "$timeout", "windowManager"];

    public slideSelector: string;
    public invalidClass: string;
    public activeClass: string;
    public responsive: boolean;
    public slideSelect: Function;

    public showButtons: boolean;
    public isButtonPrevInvalid: boolean;
    public isButtonNextInvalid: boolean;

    private subscribers: Rx.IDisposable[];
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
                private $compile: ng.ICompileService,
                private $timeout: ng.ITimeoutService,
                private windowManager: IWindowManager) {
        this.subscribers = [];
        this.slideSelector = !_.isString(this.slideSelector) ? "li" : this.slideSelector;
        this.invalidClass = !_.isString(this.invalidClass) ? "invalid" : this.invalidClass;
        this.activeClass = !_.isString(this.activeClass) ? "active" : this.activeClass;
        this.responsive = !!this.responsive;

        this.slidesTotalWidth = 0;
        this.scrollPosition = 0;

        this.showButtons = false;
        this.isButtonPrevInvalid = false;
        this.isButtonNextInvalid = false;
    }

    public $onDestroy = () => {
        this.subscribers.forEach(subscriber => {
            subscriber.dispose();
        });
        delete this.subscribers;
    };

    public $postLink = () => {
        this.setupSlides();
        if (this.responsive) {
            this.subscribers.push(this.windowManager.mainWindow.subscribeOnNext(this.recalculate, this));
        } else {
            this.recalculate();
        }
    };

    public showButtonPrev(): boolean {
        const isFirtSlideVisible = !Math.round(this.scrollPosition);
        return (this.scrollIndex > 0 && !isFirtSlideVisible);
    }

    public showButtonNext(): boolean {
        const isLastSlideVisible = this.slidesTotalWidth - this.scrollPosition < this.availableWidth;
        return (this.scrollIndex < this.slidesWidth.length - 1 && !isLastSlideVisible);
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
        this.scrollPosition = scrollPosition;
        this.slidesContainer.style.left = "-" + scrollPosition.toString() + "px";
        this.checkIfInvalid();
        this.ensureActiveVisible();
    }

    public previousSlide(): void {
        this.moveSlide(-1);
    }

    public nextSlide(): void {
        this.moveSlide(1);
    }

    private isSlideHidden(index: number): SlidePosition {
        const slide = this.slides[index] as HTMLElement;
        const slideWidth = slide.getBoundingClientRect().width;
        const wiggleRoom = slideWidth / 3; // we want to consider also partially hidden slides

        if (slide.offsetLeft + wiggleRoom < this.scrollPosition) {
            return SlidePosition.HiddenLeft;
        } else if (slide.offsetLeft + (slideWidth - wiggleRoom) > this.availableWidth + this.scrollPosition) {
            return SlidePosition.HiddenRight;
        } else {
            return SlidePosition.Visible;
        }
    }

    private ensureActiveVisible(): void {
        if (this.slides && this.slides.length) {
            for (let i = 0; i < this.slides.length; i++) {
                const slide = this.slides[i] as HTMLElement;
                if (slide.classList.contains(this.activeClass)) {
                    const slidePosition = this.isSlideHidden(i);
                    if (slidePosition === SlidePosition.HiddenLeft || slidePosition === SlidePosition.HiddenRight) {
                        this.setFirstVisible(slidePosition);
                    }
                }
            }
        }
    }

    private setFirstVisible(direction: number): void {
        if (_.isFunction(this.slideSelect)) {
            // the timeout is needed because, if we change the tab before the sliding animation ends,
            // the calculation of which tab is visible can have unexpected results
            this.$timeout(() => { // the timeout because w
                let slideIndex: number;
                if (direction === SlidePosition.HiddenLeft) {
                    for (let i = 0; i < this.slides.length; i++) {
                        if (this.isSlideHidden(i) === 0) {
                            slideIndex = i;
                            break;
                        }
                    }
                } else {
                    for (let i = this.slides.length - 1; i >= 0; i--) {
                        if (this.isSlideHidden(i) === 0) {
                            slideIndex = i;
                            break;
                        }
                    }
                }
                if (_.isFinite(slideIndex)) {
                    this.slideSelect()(slideIndex);
                }
            }, 300);
        }
    }

    private checkIfInvalid(): void {
        if (this.slides && this.slides.length) {
            this.isButtonPrevInvalid = false;
            this.isButtonNextInvalid = false;
            for (let i = 0; i < this.slides.length; i++) {
                const slide = this.slides[i] as HTMLElement;
                const slidePosition = this.isSlideHidden(i);

                if (slidePosition === SlidePosition.HiddenLeft && slide.classList.contains(this.invalidClass)) {
                    this.isButtonPrevInvalid = true;
                } else if (slidePosition === SlidePosition.HiddenRight && slide.classList.contains(this.invalidClass)) {
                    this.isButtonNextInvalid = true;
                }
            }
        }
    }

    private setupSlides = (): void => {
        this.$scope.$applyAsync(() => {
            const template = this.$templateCache.get("tabSliderWrapper.html") as string;
            const wrapper = this.$compile(template)(this.$scope)[0] as HTMLElement;
            const container = wrapper.querySelector(".tab-slider__container") as HTMLElement;

            this.slides = this.getSlides();
            this.slidesContainer = this.slides[0].parentElement;
            if (this.slidesContainer) {
                this.slidesContainer.parentElement.insertBefore(wrapper, this.slidesContainer);
                container.appendChild(this.slidesContainer);
                this.slidesTotalWidth = 0;
                this.scrollPosition = 0;
                this.scrollIndex = 0;
                this.slidesContainer.classList.add("tab-slider__content");

                for (let i = 0; i < this.slides.length; i++) {
                    const slide = this.slides[i] as HTMLElement;
                    slide.classList.add("tab-slider__slide");
                }
            }
        });
    };

    private recalculate = (): void => {
        this.$scope.$applyAsync(() => {
            if (this.slides && this.slides.length) {
                this.availableWidth = this.slidesContainer.offsetWidth;
                this.slidesTotalWidth = 0;
                this.slidesWidth = [];

                for (let i = 0; i < this.slides.length; i++) {
                    const slide = this.slides[i] as HTMLElement;
                    const slideWidth = slide.getBoundingClientRect().width;
                    this.slidesTotalWidth += slideWidth;
                    this.slidesWidth.push(slideWidth);
                }

                this.showButtons = this.slidesTotalWidth > this.availableWidth;
                this.checkIfInvalid();
                this.ensureActiveVisible();
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
