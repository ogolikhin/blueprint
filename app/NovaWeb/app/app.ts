module nova {
    export class App {
        
        public static version: string = "1.0.0";
        
        
        private _value : string;
        public get value() : string {
            return this._value;
        }
        public set value(v : string) {
            this._value = v;
        }
        
        
        /**
         * config
         */
        public config() {
            //todo
            var result = new nova.Reverse("test", false);
        }
    }
    angular.module('nova',['ngSanitize', 'ui.bootstrap', 'ui.router'])
        .config(nova.App);
}
