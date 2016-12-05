import * as TestModels from "./test-model-factory";
import {UserTask} from "../components/diagram/presentation/graph/shapes/user-task";
import {UserDecision} from "../components/diagram/presentation/graph/shapes/user-decision";
import {SystemTask} from "../components/diagram/presentation/graph/shapes/system-task";
import {SystemDecision} from "../components/diagram/presentation/graph/shapes/system-decision";
import {ProcessStart} from "../components/diagram/presentation/graph/shapes/process-start";
import {ProcessEnd} from "../components/diagram/presentation/graph/shapes/process-end";

export function createStart(id: number): ProcessStart {
    return new ProcessStart(TestModels.createStart(id), undefined);
}

export function createEnd(id: number): ProcessEnd {
    return new ProcessEnd(TestModels.createEnd(id), undefined);
}

export function createUserTask(id: number, $rootScope: ng.IRootScopeService): UserTask {
    return new UserTask(TestModels.createUserTask(id), $rootScope, undefined, undefined);
}

export function createUserDecision(id: number, $rootScope: ng.IRootScopeService): UserDecision {
    return new UserDecision(TestModels.createUserDecision(id), $rootScope, undefined);
}

export function createSystemTask(id: number, $rootScope: ng.IRootScopeService): SystemTask {
    return new SystemTask(TestModels.createSystemTask(id), $rootScope, undefined, undefined, undefined);
}

export function createSystemDecision(id: number, $rootScope: ng.IRootScopeService): SystemDecision {
    return new SystemDecision(TestModels.createSystemDecision(id), $rootScope, undefined);
}
