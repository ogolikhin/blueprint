module Storyteller {
    export interface IUserStoryProvider {
        canGenerateUserStory(): boolean;
    }
}