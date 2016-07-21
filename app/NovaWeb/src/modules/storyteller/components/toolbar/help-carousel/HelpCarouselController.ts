module Shell {
    export class HelpCarouselController {
        private slides: any[];

        public static $inject = [
            "$uibModalInstance"
        ];

        constructor(private $uibModalInstance: angular.ui.bootstrap.IModalServiceInstance) {
            this.slides = [
                { image: "/Areas/Web/Style/images/Storyteller/on-boarding/Storyteller-Onboarding-00.png" },
                { image: "/Areas/Web/Style/images/Storyteller/on-boarding/Storyteller-Onboarding-01.png" },
                { image: "/Areas/Web/Style/images/Storyteller/on-boarding/Storyteller-Onboarding-02.png" },
                { image: "/Areas/Web/Style/images/Storyteller/on-boarding/Storyteller-Onboarding-03.png" },
                { image: "/Areas/Web/Style/images/Storyteller/on-boarding/Storyteller-Onboarding-04.png" },
                { image: "/Areas/Web/Style/images/Storyteller/on-boarding/Storyteller-Onboarding-05.png" },
                { image: "/Areas/Web/Style/images/Storyteller/on-boarding/Storyteller-Onboarding-06.png" },
                { image: "/Areas/Web/Style/images/Storyteller/on-boarding/Storyteller-Onboarding-07.png" },
                { image: "/Areas/Web/Style/images/Storyteller/on-boarding/Storyteller-Onboarding-08.png" },
                { image: "/Areas/Web/Style/images/Storyteller/on-boarding/Storyteller-Onboarding-09.png" },
                { image: "/Areas/Web/Style/images/Storyteller/on-boarding/Storyteller-Onboarding-10.png" },
                { image: "/Areas/Web/Style/images/Storyteller/on-boarding/Storyteller-Onboarding-11.png" },
                { image: "/Areas/Web/Style/images/Storyteller/on-boarding/Storyteller-Onboarding-12.png" },
                { image: "/Areas/Web/Style/images/Storyteller/on-boarding/Storyteller-Onboarding-13.png" },
                { image: "/Areas/Web/Style/images/Storyteller/on-boarding/Storyteller-Onboarding-14.png" },
                { image: "/Areas/Web/Style/images/Storyteller/on-boarding/Storyteller-Onboarding-15.png" },
                { image: "/Areas/Web/Style/images/Storyteller/on-boarding/Storyteller-Onboarding-16.png" },
                { image: "/Areas/Web/Style/images/Storyteller/on-boarding/Storyteller-Onboarding-17.png" },
                { image: "/Areas/Web/Style/images/Storyteller/on-boarding/Storyteller-Onboarding-18.png" },
                { image: "/Areas/Web/Style/images/Storyteller/on-boarding/Storyteller-Onboarding-19.png" },
                { image: "/Areas/Web/Style/images/Storyteller/on-boarding/Storyteller-Onboarding-20.png" }
            ];
        }

        public close = () => {
            this.$uibModalInstance.close();
        };
    }

    angular.module("Shell").controller("HelpCarouselController", HelpCarouselController);
}
