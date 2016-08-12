import {ProcessShapeType, ProcessType} from "./enums";
import {IProcess, IProcessShape, ProcessModel, ProcessShapeModel} from "./processModels";
import {ShapesFactory} from "../components/diagram/presentation/graph/shapes/shapes-factory";

export function createProcessModel(id: number = 1, type: ProcessType = ProcessType.BusinessProcess): ProcessModel {
    let process = new ProcessModel(id);
    process.propertyValues = {
        "clientType": {
            typeId: 0,
            typePredefined: null,
            propertyName: "clientType",
            value: type
        }
    };

    return process;
}
export function createShapeModel(type: ProcessShapeType, id: number, x?: number, y?: number): IProcessShape {
    let shapeModel = new ProcessShapeModel(id);
    shapeModel.name = id.toString();
    shapeModel.propertyValues = {
        "clientType": {
            typeId: 0,
            typePredefined: null,
            propertyName: "clientType",
            value: type
        },
        "x": {
            typeId: 0,
            typePredefined: null,
            propertyName: "x",
            value: x ? x : 0
        },
        "y": {
            typeId: 0,
            typePredefined: null,
            propertyName: "x",
            value: y ? y : 0
        }
    };

    return shapeModel;
}

export function createDefaultProcessModel(): IProcess {
    let process: IProcess = createDefaultProcessModelWithoutXAndY();

    process.shapes[0].propertyValues["x"].value = 0;
    process.shapes[0].propertyValues["y"].value = 0;
    process.shapes[1].propertyValues["x"].value = 1;
    process.shapes[1].propertyValues["y"].value = 0;
    process.shapes[2].propertyValues["x"].value = 2;
    process.shapes[2].propertyValues["y"].value = 0;
    process.shapes[3].propertyValues["x"].value = 3;
    process.shapes[3].propertyValues["y"].value = 0;
    process.shapes[4].propertyValues["x"].value = 4;
    process.shapes[4].propertyValues["y"].value = 0;

    return process;
}

export function createDefaultProcessModelWithoutXAndY(): IProcess {
    let process: IProcess = createProcessModel(1, ProcessType.BusinessProcess);

    let start = createShapeModel(ProcessShapeType.Start, 10, 0, 0);
    let pre = createShapeModel(ProcessShapeType.PreconditionSystemTask, 15, 0, 0);
    let ut1 = createShapeModel(ProcessShapeType.UserTask, 20, 0, 0);
    let st2 = createShapeModel(ProcessShapeType.SystemTask, 25, 0, 0);
    let end = createShapeModel(ProcessShapeType.End, 30, 0, 0);

    process.shapes.push(start);
    process.shapes.push(pre);
    process.shapes.push(ut1);
    process.shapes.push(st2);
    process.shapes.push(end);

    process.links.push({ sourceId: 10, destinationId: 15, orderindex: 0, label: null });
    process.links.push({ sourceId: 15, destinationId: 20, orderindex: 0, label: null });
    process.links.push({ sourceId: 20, destinationId: 25, orderindex: 0, label: null });
    process.links.push({ sourceId: 25, destinationId: 30, orderindex: 0, label: null });

    populatePropertyValues(process.shapes[0], "Start", 0, 0, ProcessShapeType.Start);
    populatePropertyValues(process.shapes[1], "Precondition", 0, 0, ProcessShapeType.PreconditionSystemTask);
    populatePropertyValues(process.shapes[2], "User Task 1", 0, 0, ProcessShapeType.UserTask);
    populatePropertyValues(process.shapes[3], "System Task 2", 0, 0, ProcessShapeType.SystemTask);
    populatePropertyValues(process.shapes[4], "End", 0, 0, ProcessShapeType.End);

    return process;
}

export function createTwoNestedUserTasksWithSystemTaskModelWithoutXAndY(): IProcess {
    let process: IProcess = createProcessModel(0);

    let start = createShapeModel(ProcessShapeType.Start, 1, 0, 0);
    let pre = createShapeModel(ProcessShapeType.PreconditionSystemTask, 2, 0, 0);
    let ud1 = createShapeModel(ProcessShapeType.UserDecision, 3, 0, 0);
    let ut1 = createShapeModel(ProcessShapeType.UserTask, 4, 0, 0);
    let sd1 = createShapeModel(ProcessShapeType.SystemDecision, 5, 0, 0);
    let st1 = createShapeModel(ProcessShapeType.SystemTask, 6, 0, 0);
    let st2 = createShapeModel(ProcessShapeType.SystemTask, 7, 0, 0);
    let ud2 = createShapeModel(ProcessShapeType.UserDecision, 8, 0, 0);
    let ut2 = createShapeModel(ProcessShapeType.UserTask, 9, 0, 0);
    let st3 = createShapeModel(ProcessShapeType.SystemTask, 10, 0, 0);
    let ut3 = createShapeModel(ProcessShapeType.UserTask, 11, 0, 0);
    let st4 = createShapeModel(ProcessShapeType.SystemTask, 12, 0, 0);
    let ut4 = createShapeModel(ProcessShapeType.UserTask, 13, 0, 0);
    let st5 = createShapeModel(ProcessShapeType.SystemTask, 14, 0, 0);
    let ut5 = createShapeModel(ProcessShapeType.UserTask, 15, 0, 0);
    let st6 = createShapeModel(ProcessShapeType.SystemTask, 16, 0, 0);
    let end = createShapeModel(ProcessShapeType.End, 17, 0, 0);

    process.shapes.push(start, pre, ud1, ut1, sd1, st1, st2, ud2, ut2, st3, ut3, st4, ut4, st5, ut5, st6, end);

    // Start -> Pre -> UD1 -> UT1 -> SD -> ST1 -> End
    //                                     ST2 -> UD2 -> UT2 -> ST3 -> UT5
    //                                                   UT3 -> ST4 -> UT5
    //                        UT4 -> ST5 -> UT5 -> ST6 -> End
    process.links.push(
        { sourceId: start.id, destinationId: pre.id, orderindex: 0, label: null },
        { sourceId: pre.id, destinationId: ud1.id, orderindex: 0, label: null },
        { sourceId: ud1.id, destinationId: ut1.id, orderindex: 0, label: null },
        { sourceId: ut1.id, destinationId: sd1.id, orderindex: 0, label: null },
        { sourceId: sd1.id, destinationId: st1.id, orderindex: 0, label: null },
        { sourceId: st1.id, destinationId: end.id, orderindex: 0, label: null },
        { sourceId: sd1.id, destinationId: st2.id, orderindex: 1, label: null },
        { sourceId: st2.id, destinationId: ud2.id, orderindex: 0, label: null },
        { sourceId: ud2.id, destinationId: ut2.id, orderindex: 0, label: null },
        { sourceId: ut2.id, destinationId: st3.id, orderindex: 0, label: null },
        { sourceId: st3.id, destinationId: ut5.id, orderindex: 0, label: null },
        { sourceId: ud2.id, destinationId: ut3.id, orderindex: 1, label: null },
        { sourceId: ut3.id, destinationId: st4.id, orderindex: 0, label: null },
        { sourceId: st4.id, destinationId: ut5.id, orderindex: 0, label: null },
        { sourceId: ud1.id, destinationId: ut4.id, orderindex: 1, label: null },
        { sourceId: ut4.id, destinationId: st5.id, orderindex: 0, label: null },
        { sourceId: st5.id, destinationId: ut5.id, orderindex: 0, label: null },
        { sourceId: ut5.id, destinationId: st6.id, orderindex: 1, label: null },
        { sourceId: st6.id, destinationId: end.id, orderindex: 0, label: null }
    );

    process.decisionBranchDestinationLinks.push(
        { sourceId: ud1.id, destinationId: end.id, orderindex: 1, label: null },
        { sourceId: sd1.id, destinationId: ut5.id, orderindex: 1, label: null },
        { sourceId: ud2.id, destinationId: ut5.id, orderindex: 1, label: null }
    );

    return process;
}
export function createTwoNestedUserTasksWithSystemTaskModel(): IProcess {
        // Start -> Pre -> UD1 -> UT1 -> SD -> ST1 -> End
        //                                     ST2 -> UD2 -> UT2 -> ST3 -> UT5
        //                                                   UT3 -> ST4 -> UT5
        //                        UT4 -> ST5 -> UT5 -> ST6 -> End
    let process: IProcess = createTwoNestedUserTasksWithSystemTaskModelWithoutXAndY();

    process.shapes[1].propertyValues["x"].value = 1;  // Pre
    process.shapes[2].propertyValues["x"].value = 2;  // UD1
    process.shapes[3].propertyValues["x"].value = 3;  // UT1
    process.shapes[4].propertyValues["x"].value = 4;  // SD
    process.shapes[5].propertyValues["x"].value = 5;  // ST1
    process.shapes[6].propertyValues["x"].value = 5;  // ST2
    process.shapes[6].propertyValues["y"].value = 1;
    process.shapes[7].propertyValues["x"].value = 6;  // UD2
    process.shapes[7].propertyValues["y"].value = 1;
    process.shapes[8].propertyValues["x"].value = 7;  // UT2
    process.shapes[8].propertyValues["y"].value = 1;
    process.shapes[9].propertyValues["x"].value = 8;  // ST3
    process.shapes[9].propertyValues["y"].value = 1;
    process.shapes[10].propertyValues["x"].value = 7; // UT3
    process.shapes[10].propertyValues["y"].value = 2;
    process.shapes[11].propertyValues["x"].value = 8; // ST4
    process.shapes[11].propertyValues["y"].value = 2;
    process.shapes[12].propertyValues["x"].value = 3; // UT4
    process.shapes[12].propertyValues["y"].value = 3;
    process.shapes[13].propertyValues["x"].value = 4; // ST5
    process.shapes[13].propertyValues["y"].value = 3;
    process.shapes[14].propertyValues["x"].value = 6; // UT5
    process.shapes[14].propertyValues["y"].value = 3;
    process.shapes[15].propertyValues["x"].value = 7; // ST6
    process.shapes[15].propertyValues["y"].value = 3;
    process.shapes[16].propertyValues["x"].value = 9; // End

    return process;
}


export function createTwoUserTaskModel(): IProcess {
    let process: IProcess = createDefaultProcessModel();

    let st1 = process.shapes[3];
    let end = process.shapes[4];
    let ut2 = createShapeModel(ProcessShapeType.UserTask, 35, 0, 0);
    let st2 = createShapeModel(ProcessShapeType.SystemTask, 40, 0, 0);

    process.shapes.splice(4, 0, ut2, st2);
    process.links[process.links.length - 1].destinationId = ut2.id;
    process.links.push(
        { sourceId: ut2.id, destinationId: st2.id, orderindex: 0, label: null },
        { sourceId: st2.id, destinationId: end.id, orderindex: 0, label: null }
    );

    return process;
}

export function createUserDecisionWithTwoBranchesModel(): IProcess {
    var shapesFactory = createShapesFactoryService();
    let model: IProcess = createProcessModel(1, ProcessType.UserToSystemProcess);

    let start = createShapeModel(ProcessShapeType.Start, 2, 0, 0);
    let pre = shapesFactory.createModelSystemTaskShape(1, 0, 3, 1, 0);
    let ud = shapesFactory.createModelUserDecisionShape(1, 0, 4, 2, 0);
    let ut1 = shapesFactory.createModelUserTaskShape(1, 0, 5, 3, 0);
    let st1 = shapesFactory.createModelSystemTaskShape(1, 0, 6, 4, 0);
    let ut2 = shapesFactory.createModelUserTaskShape(1, 0, 7, 3, 1);
    let st2 = shapesFactory.createModelSystemTaskShape(1, 0, 8, 4, 1);
    let end = createShapeModel(ProcessShapeType.End, 9, 5, 0);

    model.shapes.push(start, pre, ud, ut1, st1, ut2, st2, end);

    model.links.push({ sourceId: start.id, destinationId: pre.id, orderindex: 0, label: null });
    model.links.push({ sourceId: pre.id, destinationId: ud.id, orderindex: 0, label: null });
    model.links.push({ sourceId: ud.id, destinationId: ut1.id, orderindex: 0, label: null });
    model.links.push({ sourceId: ut1.id, destinationId: st1.id, orderindex: 0, label: null });
    model.links.push({ sourceId: st1.id, destinationId: end.id, orderindex: 0, label: null });
    model.links.push({ sourceId: ud.id, destinationId: ut2.id, orderindex: 1, label: null });
    model.links.push({ sourceId: ut2.id, destinationId: st2.id, orderindex: 0, label: null });
    model.links.push({ sourceId: st2.id, destinationId: end.id, orderindex: 0, label: null });

    model.decisionBranchDestinationLinks.push(
        { sourceId: ud.id, destinationId: end.id, orderindex: 1, label: null }
    );

    return model;
}

export function createUserDecisionWithThreeConditionsModel() {
    let process: IProcess = createProcessModel(0);

    let start = createShapeModel(ProcessShapeType.Start, 10, 0, 0);
    let pre = createShapeModel(ProcessShapeType.PreconditionSystemTask, 20, 0, 0);
    let ud = createShapeModel(ProcessShapeType.UserDecision, 30, 0, 0);
    let ut1 = createShapeModel(ProcessShapeType.UserTask, 40, 0, 0);
    let st1 = createShapeModel(ProcessShapeType.SystemTask, 50, 0, 0);
    let ut2 = createShapeModel(ProcessShapeType.UserTask, 60, 0, 0);
    let st2 = createShapeModel(ProcessShapeType.SystemTask, 70, 0, 0);
    let ut3 = createShapeModel(ProcessShapeType.UserTask, 80, 0, 0);
    let st3 = createShapeModel(ProcessShapeType.SystemTask, 90, 0, 0);
    let end = createShapeModel(ProcessShapeType.End, 100, 0, 0);

    process.shapes.push(start, pre, ud, ut1, st1, ut2, st2, ut3, st3, end);

    // Start -> Pre -> UD -> UT1 -> ST1 -> End
    //                       UT2 -> ST2 -> End
    //                       UT3 -> ST3 -> End
    process.links.push(
        { sourceId: start.id, destinationId: pre.id, orderindex: 0, label: null },
        { sourceId: pre.id, destinationId: ud.id, orderindex: 0, label: null },
        { sourceId: ud.id, destinationId: ut1.id, orderindex: 0, label: null },
        { sourceId: ut1.id, destinationId: st1.id, orderindex: 0, label: null },
        { sourceId: st1.id, destinationId: end.id, orderindex: 0, label: null },
        { sourceId: ud.id, destinationId: ut2.id, orderindex: 1, label: null },
        { sourceId: ut2.id, destinationId: st2.id, orderindex: 0, label: null },
        { sourceId: st2.id, destinationId: end.id, orderindex: 0, label: null },
        { sourceId: ud.id, destinationId: ut3.id, orderindex: 2, label: null },
        { sourceId: ut3.id, destinationId: st3.id, orderindex: 0, label: null },
        { sourceId: st3.id, destinationId: end.id, orderindex: 0, label: null }
    );

    process.decisionBranchDestinationLinks.push(
        { sourceId: ud.id, destinationId: end.id, orderindex: 1, label: null },
        { sourceId: ud.id, destinationId: end.id, orderindex: 2, label: null }
    );

    return process;
}

export function createUserDecisionWithThreeConditionsAndTwoUserTasksModel(): IProcess {
    let process: IProcess = createProcessModel(0);

    let start = createShapeModel(ProcessShapeType.Start, 10, 0, 0);
    let pre = createShapeModel(ProcessShapeType.PreconditionSystemTask, 20, 0, 0);
    let ud = createShapeModel(ProcessShapeType.UserDecision, 30, 0, 0);
    let ut1 = createShapeModel(ProcessShapeType.UserTask, 40, 0, 0);
    let st1 = createShapeModel(ProcessShapeType.SystemTask, 50, 0, 0);
    let ut2 = createShapeModel(ProcessShapeType.UserTask, 60, 0, 0);
    let st2 = createShapeModel(ProcessShapeType.SystemTask, 70, 0, 0);
    let ut3 = createShapeModel(ProcessShapeType.UserTask, 80, 0, 0);
    let st3 = createShapeModel(ProcessShapeType.SystemTask, 90, 0, 0);
    let ut4 = createShapeModel(ProcessShapeType.UserTask, 100, 0, 0);
    let st4 = createShapeModel(ProcessShapeType.SystemTask, 110, 0, 0);
    let ut5 = createShapeModel(ProcessShapeType.UserTask, 120, 0, 0);
    let st5 = createShapeModel(ProcessShapeType.SystemTask, 130, 0, 0);
    let ut6 = createShapeModel(ProcessShapeType.UserTask, 140, 0, 0);
    let st6 = createShapeModel(ProcessShapeType.SystemTask, 150, 0, 0);
    let end = createShapeModel(ProcessShapeType.End, 160, 0, 0);

    process.shapes.push(start, pre, ud, ut1, st1, ut2, st2, ut3, st3, ut4, st4, ut5, st5, ut6, st6, end);

    // Start -> Pre -> UD -> UT1 -> ST1 -> UT2 -> ST2 -> End
    //                       UT3 -> ST3 -> UT4 -> ST4 -> End
    //                       UT5 -> ST5 -> UT6 -> ST6 -> End
    process.links.push(
        { sourceId: start.id, destinationId: pre.id, orderindex: 0, label: null },
        { sourceId: pre.id, destinationId: ud.id, orderindex: 0, label: null },
        { sourceId: ud.id, destinationId: ut1.id, orderindex: 0, label: null },
        { sourceId: ut1.id, destinationId: st1.id, orderindex: 0, label: null },
        { sourceId: st1.id, destinationId: ut2.id, orderindex: 0, label: null },
        { sourceId: ut2.id, destinationId: st2.id, orderindex: 0, label: null },
        { sourceId: st2.id, destinationId: end.id, orderindex: 0, label: null },
        { sourceId: ud.id, destinationId: ut3.id, orderindex: 1, label: null },
        { sourceId: ut3.id, destinationId: st3.id, orderindex: 0, label: null },
        { sourceId: st3.id, destinationId: ut4.id, orderindex: 0, label: null },
        { sourceId: ut4.id, destinationId: st4.id, orderindex: 0, label: null },
        { sourceId: st4.id, destinationId: end.id, orderindex: 0, label: null },
        { sourceId: ud.id, destinationId: ut5.id, orderindex: 2, label: null },
        { sourceId: ut5.id, destinationId: st5.id, orderindex: 0, label: null },
        { sourceId: st5.id, destinationId: ut6.id, orderindex: 0, label: null },
        { sourceId: ut6.id, destinationId: st6.id, orderindex: 0, label: null },
        { sourceId: st6.id, destinationId: end.id, orderindex: 0, label: null }
    );

    process.decisionBranchDestinationLinks.push(
        { sourceId: ud.id, destinationId: end.id, orderindex: 1, label: null },
        { sourceId: ud.id, destinationId: end.id, orderindex: 2, label: null }
    );

    return process;
}

export function createUserDecisionInSecondConditionModel(): IProcess {
    let process: IProcess = createProcessModel(0);

    let start = createShapeModel(ProcessShapeType.Start, 10);
    let pre = createShapeModel(ProcessShapeType.PreconditionSystemTask, 20);
    let ud1 = createShapeModel(ProcessShapeType.UserDecision, 30);
    let ut1 = createShapeModel(ProcessShapeType.UserTask, 40);
    let st1 = createShapeModel(ProcessShapeType.SystemTask, 50);
    let ut2 = createShapeModel(ProcessShapeType.UserTask, 60);
    let st2 = createShapeModel(ProcessShapeType.SystemTask, 70);
    let ud2 = createShapeModel(ProcessShapeType.UserDecision, 80);
    let ut3 = createShapeModel(ProcessShapeType.UserTask, 90);
    let st3 = createShapeModel(ProcessShapeType.SystemTask, 100);
    let ut4 = createShapeModel(ProcessShapeType.UserTask, 110);
    let st4 = createShapeModel(ProcessShapeType.SystemTask, 120);
    let ud3 = createShapeModel(ProcessShapeType.UserDecision, 130);
    let ut5 = createShapeModel(ProcessShapeType.UserTask, 140);
    let st5 = createShapeModel(ProcessShapeType.SystemTask, 150);
    let ut6 = createShapeModel(ProcessShapeType.UserTask, 160);
    let st6 = createShapeModel(ProcessShapeType.SystemTask, 170);
    let end = createShapeModel(ProcessShapeType.End, 180);

    process.shapes.push(start, pre, ud1, ut1, st1, ut2, st2, ud2, ut3, st3, ut4, st4, ud3, ut5, st5, ut6, st6, end);

    // Start -> Pre -> UD1 -> UT1 -> ST1 -> UD3 -> UT5 -> ST5 -> End
    //                                             UT6 -> ST6 -> End
    //                        UT2 -> ST2 -> UD2 -> UT3 -> ST3 -> UD3
    //                                          -> UT4 -> ST4 -> UD3

    process.links.push(
        { sourceId: start.id, destinationId: pre.id, orderindex: 0, label: null },
        { sourceId: pre.id, destinationId: ud1.id, orderindex: 0, label: null },
        { sourceId: ud1.id, destinationId: ut1.id, orderindex: 0, label: null },
        { sourceId: ut1.id, destinationId: st1.id, orderindex: 0, label: null },
        { sourceId: st1.id, destinationId: ud3.id, orderindex: 0, label: null },
        { sourceId: ud3.id, destinationId: ut5.id, orderindex: 0, label: null },
        { sourceId: ut5.id, destinationId: st5.id, orderindex: 0, label: null },
        { sourceId: st5.id, destinationId: end.id, orderindex: 0, label: null },
        { sourceId: ud3.id, destinationId: ut6.id, orderindex: 1, label: null },
        { sourceId: ut6.id, destinationId: st6.id, orderindex: 0, label: null },
        { sourceId: st6.id, destinationId: end.id, orderindex: 0, label: null },
        { sourceId: ud1.id, destinationId: ut2.id, orderindex: 1, label: null },
        { sourceId: ut2.id, destinationId: st2.id, orderindex: 0, label: null },
        { sourceId: st2.id, destinationId: ud2.id, orderindex: 0, label: null },
        { sourceId: ud2.id, destinationId: ut3.id, orderindex: 0, label: null },
        { sourceId: ut3.id, destinationId: st3.id, orderindex: 0, label: null },
        { sourceId: st3.id, destinationId: ud3.id, orderindex: 0, label: null },
        { sourceId: ud2.id, destinationId: ut4.id, orderindex: 1, label: null },
        { sourceId: ut4.id, destinationId: st4.id, orderindex: 0, label: null },
        { sourceId: st4.id, destinationId: ud3.id, orderindex: 0, label: null }
    );

    process.decisionBranchDestinationLinks.push(
        { sourceId: ud1.id, destinationId: ud3.id, orderindex: 1, label: null },
        { sourceId: ud2.id, destinationId: ud3.id, orderindex: 1, label: null },
        { sourceId: ud3.id, destinationId: end.id, orderindex: 1, label: null }
    );

    return process;
}
export function createMergingSystemDecisionsModel(): IProcess {
    let process: IProcess = createProcessModel();

    let start = createShapeModel(ProcessShapeType.Start, 20);
    let pre = createShapeModel(ProcessShapeType.PreconditionSystemTask, 30);
    let ut1 = createShapeModel(ProcessShapeType.UserTask, 40);
    let sd1 = createShapeModel(ProcessShapeType.SystemDecision, 50);
    let st1 = createShapeModel(ProcessShapeType.SystemTask, 60);
    let ut2 = createShapeModel(ProcessShapeType.UserTask, 70);
    let sd2 = createShapeModel(ProcessShapeType.SystemDecision, 80);
    let st2 = createShapeModel(ProcessShapeType.SystemTask, 90);
    let st3 = createShapeModel(ProcessShapeType.SystemTask, 100);
    let ut3 = createShapeModel(ProcessShapeType.UserTask, 110);
    let st4 = createShapeModel(ProcessShapeType.SystemTask, 120);
    let st5 = createShapeModel(ProcessShapeType.SystemTask, 130);
    let end = createShapeModel(ProcessShapeType.End, 140);

    process.shapes.push(start, pre, ut1, sd1, st1, ut2, sd2, st2, st3, ut3, st4, st5, end);

    // Start -> Pre -> UT1 -> SD1 -> ST1 -> UT2 -> SD2 -> ST2 -> End
    //                            -> ST5 -> UT3        -> ST3 -> UT3 -> ST4 -> End

    process.links.push(
        { sourceId: start.id, destinationId: pre.id, orderindex: 0, label: null },
        { sourceId: pre.id, destinationId: ut1.id, orderindex: 0, label: null },
        { sourceId: ut1.id, destinationId: sd1.id, orderindex: 0, label: null },
        { sourceId: sd1.id, destinationId: st1.id, orderindex: 0, label: null },
        { sourceId: st1.id, destinationId: ut2.id, orderindex: 0, label: null },
        { sourceId: ut2.id, destinationId: sd2.id, orderindex: 0, label: null },
        { sourceId: sd2.id, destinationId: st2.id, orderindex: 0, label: null },
        { sourceId: st2.id, destinationId: end.id, orderindex: 0, label: null },
        { sourceId: sd2.id, destinationId: st3.id, orderindex: 1, label: null },
        { sourceId: st3.id, destinationId: ut3.id, orderindex: 0, label: null },
        { sourceId: ut3.id, destinationId: st4.id, orderindex: 0, label: null },
        { sourceId: st4.id, destinationId: end.id, orderindex: 0, label: null },
        { sourceId: sd1.id, destinationId: st5.id, orderindex: 1, label: null },
        { sourceId: st5.id, destinationId: ut3.id, orderindex: 0, label: null }
    );

    process.decisionBranchDestinationLinks.push(
        { sourceId: sd1.id, destinationId: ut3.id, orderindex: 1, label: null },
        { sourceId: sd2.id, destinationId: end.id, orderindex: 1, label: null }
    );

    return process;
}
export function createMergingSystemDecisionsWithInfiniteLoopModel(): IProcess {
    let process: IProcess = createProcessModel();

    let start = createShapeModel(ProcessShapeType.Start, 20);
    let pre = createShapeModel(ProcessShapeType.PreconditionSystemTask, 30);
    let ut1 = createShapeModel(ProcessShapeType.UserTask, 40);
    let sd1 = createShapeModel(ProcessShapeType.SystemDecision, 50);
    let st1 = createShapeModel(ProcessShapeType.SystemTask, 60);
    let ut2 = createShapeModel(ProcessShapeType.UserTask, 70);
    let sd2 = createShapeModel(ProcessShapeType.SystemDecision, 80);
    let st2 = createShapeModel(ProcessShapeType.SystemTask, 90);
    let st3 = createShapeModel(ProcessShapeType.SystemTask, 100);
    let ut3 = createShapeModel(ProcessShapeType.UserTask, 110);
    let st4 = createShapeModel(ProcessShapeType.SystemTask, 120);
    let st5 = createShapeModel(ProcessShapeType.SystemTask, 130);
    let end = createShapeModel(ProcessShapeType.End, 140);

    process.shapes.push(start, pre, ut1, sd1, st1, ut2, sd2, st2, st3, ut3, st4, st5, end);

    // Start -> Pre -> UT1 -> SD1 -> ST1 -> UT2 -> SD2 -> ST2 -> End
    //                            -> ST5 -> UT3        -> ST3 -> UT3 -> ST4 -> UT1

    process.links.push(
        { sourceId: start.id, destinationId: pre.id, orderindex: 0, label: null },
        { sourceId: pre.id, destinationId: ut1.id, orderindex: 0, label: null },
        { sourceId: ut1.id, destinationId: sd1.id, orderindex: 0, label: null },
        { sourceId: sd1.id, destinationId: st1.id, orderindex: 0, label: null },
        { sourceId: st1.id, destinationId: ut2.id, orderindex: 0, label: null },
        { sourceId: ut2.id, destinationId: sd2.id, orderindex: 0, label: null },
        { sourceId: sd2.id, destinationId: st2.id, orderindex: 0, label: null },
        { sourceId: st2.id, destinationId: end.id, orderindex: 0, label: null },
        { sourceId: sd2.id, destinationId: st3.id, orderindex: 1, label: null },
        { sourceId: st3.id, destinationId: ut3.id, orderindex: 0, label: null },
        { sourceId: ut3.id, destinationId: st4.id, orderindex: 0, label: null },
        { sourceId: st4.id, destinationId: ut1.id, orderindex: 0, label: null },
        { sourceId: sd1.id, destinationId: st5.id, orderindex: 1, label: null },
        { sourceId: st5.id, destinationId: ut3.id, orderindex: 0, label: null }
    );

    process.decisionBranchDestinationLinks.push(
        { sourceId: sd1.id, destinationId: ut3.id, orderindex: 1, label: null },
        { sourceId: sd2.id, destinationId: ut1.id, orderindex: 1, label: null }
    );

    return process;
}


// Start -> Pre -> UT1 -> SD1 -> ST1               -> End
//                            -> ST2 -> UT2 -> ST3 -> End
export function createSimpleProcessModelWithSystemDecision() {
    let process: IProcess = createProcessModel(0, ProcessType.UserToSystemProcess);

    let start = createShapeModel(ProcessShapeType.Start, 10, 0, 0);
    let pre = createShapeModel(ProcessShapeType.PreconditionSystemTask, 15, 1, 0);
    let ut1 = createShapeModel(ProcessShapeType.UserTask, 20, 2, 0);
    let sd1 = createShapeModel(ProcessShapeType.SystemDecision, 25, 3, 0);
    let st1 = createShapeModel(ProcessShapeType.SystemTask, 26);
    let st2 = createShapeModel(ProcessShapeType.SystemTask, 27);
    let ut2 = createShapeModel(ProcessShapeType.UserTask, 28);
    let st3 = createShapeModel(ProcessShapeType.SystemTask, 29);
    let end = createShapeModel(ProcessShapeType.End, 30, 6, 0);

    process.shapes.push(start, pre, ut1, sd1, st1, st2, ut2, st3, end);

    process.links.push(
        { sourceId: start.id, destinationId: pre.id, orderindex: 0, label: null },
        { sourceId: pre.id, destinationId: ut1.id, orderindex: 0, label: null },
        { sourceId: ut1.id, destinationId: sd1.id, orderindex: 0, label: null },
        { sourceId: sd1.id, destinationId: st1.id, orderindex: 0, label: null },
        { sourceId: st1.id, destinationId: end.id, orderindex: 0, label: null },
        { sourceId: sd1.id, destinationId: st2.id, orderindex: 1, label: null },
        { sourceId: st2.id, destinationId: ut2.id, orderindex: 0, label: null },
        { sourceId: ut2.id, destinationId: st3.id, orderindex: 0, label: null },
        { sourceId: st3.id, destinationId: end.id, orderindex: 0, label: null }
    );
    process.decisionBranchDestinationLinks.push(
        { sourceId: sd1.id, destinationId: st2.id, orderindex: 1, label: null }
    );

    return process;
}

// Start-> Pre -> UT1 -> ST1 -> UT2 -> ST2 -> End
export function createDeleteUserTaskSimpleModel(ut1: IProcessShape): IProcess {
    let process: IProcess = createProcessModel(1, ProcessType.UserToSystemProcess);

    let start = createShapeModel(ProcessShapeType.Start, 10, 0, 0);
    let pre = createShapeModel(ProcessShapeType.PreconditionSystemTask, 15, 1, 0);
    let st1 = createShapeModel(ProcessShapeType.SystemTask, 25, 4, 0);
    let ut2 = createShapeModel(ProcessShapeType.UserTask, 30, 3, 0);
    let st2 = createShapeModel(ProcessShapeType.SystemTask, 35, 7, 0);
    let end = createShapeModel(ProcessShapeType.End, 40, 8, 0);

    process.shapes.push(start, pre, ut1, st1, ut2, st2, end);

    process.links.push(
        { sourceId: start.id, destinationId: pre.id, orderindex: 0, label: null },
        { sourceId: pre.id, destinationId: ut1.id, orderindex: 0, label: null },
        { sourceId: ut1.id, destinationId: st1.id, orderindex: 0, label: null },
        { sourceId: st1.id, destinationId: ut2.id, orderindex: 0, label: null },
        { sourceId: ut2.id, destinationId: st2.id, orderindex: 0, label: null },
        { sourceId: st2.id, destinationId: end.id, orderindex: 1, label: null }
    );

    return process;
}

//  Start-> Pre -> UT1 -> ST1 -> UT2 -> SD -> ST2 -> End
//                                         -> ST3 -> End
export function createUserTaskFollowedBySystemDecision(ut2: IProcessShape): IProcess {
    let process: IProcess = createProcessModel(1, ProcessType.UserToSystemProcess);

    let start = createShapeModel(ProcessShapeType.Start, 10, 0, 0);
    let pre = createShapeModel(ProcessShapeType.PreconditionSystemTask, 15, 1, 0);
    let ut1 = createShapeModel(ProcessShapeType.UserTask, 20, 3, 0);
    let st1 = createShapeModel(ProcessShapeType.SystemTask, 25, 4, 0);
    let sd = createShapeModel(ProcessShapeType.SystemDecision, 35, 6, 0);
    let st2 = createShapeModel(ProcessShapeType.SystemTask, 40, 7, 0);
    let st3 = createShapeModel(ProcessShapeType.SystemTask, 45, 7, 1);
    let end = createShapeModel(ProcessShapeType.End, 50, 8, 0);

    process.shapes.push(start, pre, ut1, st1, ut2, sd, st2, st3, end);

    process.links.push(
        { sourceId: start.id, destinationId: pre.id, orderindex: 0, label: null },
        { sourceId: pre.id, destinationId: ut1.id, orderindex: 0, label: null },
        { sourceId: ut1.id, destinationId: st1.id, orderindex: 0, label: null },
        { sourceId: st1.id, destinationId: ut2.id, orderindex: 0, label: null },
        { sourceId: ut2.id, destinationId: sd.id, orderindex: 0, label: null },
        { sourceId: sd.id, destinationId: st2.id, orderindex: 0, label: null },
        { sourceId: st2.id, destinationId: end.id, orderindex: 0, label: null },
        { sourceId: sd.id, destinationId: st3.id, orderindex: 1, label: null },
        { sourceId: st3.id, destinationId: end.id, orderindex: 0, label: null }
    );

    process.decisionBranchDestinationLinks.push(
        { sourceId: sd.id, destinationId: end.id, orderindex: 1, label: null }
    );

    return process;
}

export function createUserDecisionInfiniteLoopModelWithoutXAndY(): IProcess {
    // Start -> Pre -> UD -> UT1 -> ST1 -> End
    //                       UT2 -> ST2 -> UT3 -> ST3 -> UT5
    //                       UT4 -> ST4 -> UT5 -> ST5 -> UT3
    let process: IProcess = createProcessModel(0);

    let start = createShapeModel(ProcessShapeType.Start, 10, 0, 0);
    let pre = createShapeModel(ProcessShapeType.PreconditionSystemTask, 20, 0, 0);
    let ud = createShapeModel(ProcessShapeType.UserDecision, 30, 0, 0);
    let ut1 = createShapeModel(ProcessShapeType.UserTask, 40, 0, 0);
    let st1 = createShapeModel(ProcessShapeType.SystemTask, 50, 0, 0);
    let ut2 = createShapeModel(ProcessShapeType.UserTask, 60, 0, 0);
    let st2 = createShapeModel(ProcessShapeType.SystemTask, 70, 0, 0);
    let ut3 = createShapeModel(ProcessShapeType.UserTask, 80, 0, 0);
    let st3 = createShapeModel(ProcessShapeType.SystemTask, 90, 0, 0);
    let ut4 = createShapeModel(ProcessShapeType.UserTask, 100, 0, 0);
    let st4 = createShapeModel(ProcessShapeType.SystemTask, 110, 0, 0);
    let ut5 = createShapeModel(ProcessShapeType.UserTask, 120, 0, 0);
    let st5 = createShapeModel(ProcessShapeType.SystemTask, 130, 0, 0);
    let end = createShapeModel(ProcessShapeType.End, 140, 0, 0);

    process.shapes.push(start, pre, ud, ut1, st1, ut2, st2, ut3, st3, ut4, st4, ut5, st5, end);

    process.links.push(
        { sourceId: start.id, destinationId: pre.id, orderindex: 0, label: null },
        { sourceId: pre.id, destinationId: ud.id, orderindex: 0, label: null },
        { sourceId: ud.id, destinationId: ut1.id, orderindex: 0, label: null },
        { sourceId: ut1.id, destinationId: st1.id, orderindex: 0, label: null },
        { sourceId: st1.id, destinationId: end.id, orderindex: 0, label: null },
        { sourceId: ud.id, destinationId: ut2.id, orderindex: 1, label: null },
        { sourceId: ut2.id, destinationId: st2.id, orderindex: 0, label: null },
        { sourceId: st2.id, destinationId: ut3.id, orderindex: 0, label: null },
        { sourceId: ut3.id, destinationId: st3.id, orderindex: 0, label: null },
        { sourceId: st3.id, destinationId: ut5.id, orderindex: 0, label: null },
        { sourceId: ud.id, destinationId: ut4.id, orderindex: 2, label: null },
        { sourceId: ut4.id, destinationId: st4.id, orderindex: 0, label: null },
        { sourceId: st4.id, destinationId: ut5.id, orderindex: 0, label: null },
        { sourceId: ut5.id, destinationId: st5.id, orderindex: 0, label: null },
        { sourceId: st5.id, destinationId: ut3.id, orderindex: 0, label: null }
    );

    process.decisionBranchDestinationLinks.push(
        { sourceId: ud.id, destinationId: ut5.id, orderindex: 1, label: null },
        { sourceId: ud.id, destinationId: ut3.id, orderindex: 2, label: null }
    );

    return process;
}

export function createUserDecisionInfiniteLoopModel(): IProcess {
    // Start -> Pre -> UD -> UT1 -> ST1 -> End
    //                       UT2 -> ST2 -> UT3 -> ST3 -> UT5
    //                       UT4 -> ST4 -> UT5 -> ST5 -> UT3
    let process: IProcess = createUserDecisionInfiniteLoopModelWithoutXAndY();

    process.shapes[1].propertyValues["x"].value = 1;
    process.shapes[2].propertyValues["x"].value = 2;
    process.shapes[3].propertyValues["x"].value = 3;
    process.shapes[4].propertyValues["x"].value = 4;
    process.shapes[5].propertyValues["x"].value = 3;
    process.shapes[5].propertyValues["y"].value = 1;
    process.shapes[6].propertyValues["x"].value = 4;
    process.shapes[6].propertyValues["y"].value = 1;
    process.shapes[7].propertyValues["x"].value = 9;
    process.shapes[7].propertyValues["y"].value = 1;
    process.shapes[8].propertyValues["x"].value = 10;
    process.shapes[8].propertyValues["y"].value = 1;
    process.shapes[9].propertyValues["x"].value = 3;
    process.shapes[9].propertyValues["y"].value = 2;
    process.shapes[10].propertyValues["x"].value = 4;
    process.shapes[10].propertyValues["y"].value = 2;
    process.shapes[11].propertyValues["x"].value = 6;
    process.shapes[11].propertyValues["y"].value = 2;
    process.shapes[12].propertyValues["x"].value = 7;
    process.shapes[12].propertyValues["y"].value = 2;
    process.shapes[13].propertyValues["x"].value = 5;

    return process;
}


export function createInfiniteLoopFromDifferentDecisions(): IProcess {

    /*
    start->pre->ud1->ut1->st1->                                 ud2->ut5->st5->end
                    ->ut2->sd2->st2a ->ud2                         ->ut6->sd6->st6a->end
                              ->st2b -> ut3 -> st3 -> ut7                    ->st6b->ut7->st7->ut3
                    -> ut4 -> st4 -> ut3
    */
    let process: IProcess = createProcessModel(0);
    let start = createShapeModel(ProcessShapeType.Start, 10);
    let pre = createShapeModel(ProcessShapeType.PreconditionSystemTask, 20);
    let ud1 = createShapeModel(ProcessShapeType.UserDecision, 30);
    let ut1 = createShapeModel(ProcessShapeType.UserTask, 40);
    let st1 = createShapeModel(ProcessShapeType.SystemTask, 50);
    let ut2 = createShapeModel(ProcessShapeType.UserTask, 60);
    let sd2 = createShapeModel(ProcessShapeType.SystemDecision, 70);
    let st2a = createShapeModel(ProcessShapeType.SystemTask, 80);
    let st2b = createShapeModel(ProcessShapeType.SystemTask, 90);
    let ut3 = createShapeModel(ProcessShapeType.UserTask, 100);
    let st3 = createShapeModel(ProcessShapeType.SystemTask, 110);
    let ut4 = createShapeModel(ProcessShapeType.UserTask, 120);
    let st4 = createShapeModel(ProcessShapeType.SystemTask, 130);

    let ud2 = createShapeModel(ProcessShapeType.UserDecision, 140);
    let ut5 = createShapeModel(ProcessShapeType.UserTask, 150);
    let st5 = createShapeModel(ProcessShapeType.SystemTask, 160);
    let ut6 = createShapeModel(ProcessShapeType.UserTask, 170);
    let sd6 = createShapeModel(ProcessShapeType.SystemDecision, 180);
    let st6a = createShapeModel(ProcessShapeType.SystemTask, 190);
    let st6b = createShapeModel(ProcessShapeType.SystemTask, 200);
    let ut7 = createShapeModel(ProcessShapeType.UserTask, 210);
    let st7 = createShapeModel(ProcessShapeType.SystemTask, 220);

    let end = createShapeModel(ProcessShapeType.End, 230);

    process.shapes.push(start, pre, ud1, ut1, st1, ut2, sd2, st2a,
        st2b, ut3, st3, ut4, st4, ud2, ut5, st5,
        ut6, sd6, st6a, st6b, ut7, st7, end);

    process.links.push(
        { sourceId: start.id, destinationId: pre.id, orderindex: 0, label: null },
        { sourceId: pre.id, destinationId: ud1.id, orderindex: 0, label: null },
        { sourceId: ud1.id, destinationId: ut1.id, orderindex: 0, label: null },
        { sourceId: ut1.id, destinationId: st1.id, orderindex: 0, label: null },
        { sourceId: ud1.id, destinationId: ut2.id, orderindex: 1, label: null },
        { sourceId: ut2.id, destinationId: sd2.id, orderindex: 0, label: null },
        { sourceId: sd2.id, destinationId: st2a.id, orderindex: 0, label: null },
        { sourceId: sd2.id, destinationId: st2b.id, orderindex: 1, label: null },
        { sourceId: st2b.id, destinationId: ut3.id, orderindex: 0, label: null },
        { sourceId: ut3.id, destinationId: st3.id, orderindex: 0, label: null },
        { sourceId: st3.id, destinationId: ut7.id, orderindex: 0, label: null },

        { sourceId: ud1.id, destinationId: ut4.id, orderindex: 2, label: null },
        { sourceId: ut4.id, destinationId: st4.id, orderindex: 0, label: null },
        { sourceId: st4.id, destinationId: ut3.id, orderindex: 0, label: null },

        { sourceId: st1.id, destinationId: ud2.id, orderindex: 0, label: null },
        { sourceId: st2a.id, destinationId: ud2.id, orderindex: 0, label: null },

        { sourceId: ud2.id, destinationId: ut5.id, orderindex: 0, label: null },
        { sourceId: ut5.id, destinationId: st5.id, orderindex: 0, label: null },
        { sourceId: ud2.id, destinationId: ut6.id, orderindex: 1, label: null },
        { sourceId: ut6.id, destinationId: sd6.id, orderindex: 0, label: null },
        { sourceId: sd6.id, destinationId: st6a.id, orderindex: 0, label: null },
        { sourceId: sd6.id, destinationId: st6b.id, orderindex: 1, label: null },
        { sourceId: st6b.id, destinationId: ut7.id, orderindex: 0, label: null },
        { sourceId: ut7.id, destinationId: st7.id, orderindex: 0, label: null },
        { sourceId: st7.id, destinationId: ut3.id, orderindex: 0, label: null },

        { sourceId: st5.id, destinationId: end.id, orderindex: 0, label: null },
        { sourceId: st6a.id, destinationId: end.id, orderindex: 0, label: null }
    );


    process.decisionBranchDestinationLinks.push(
        { sourceId: ud1.id, destinationId: ud2.id, orderindex: 1, label: null },
        { sourceId: ud1.id, destinationId: ut3.id, orderindex: 2, label: null },
        { sourceId: sd2.id, destinationId: ut7.id, orderindex: 1, label: null },
        { sourceId: ud2.id, destinationId: end.id, orderindex: 1, label: null },
        { sourceId: sd6.id, destinationId: ut3.id, orderindex: 1, label: null }
    );

    return process;
}

export function createUserDecisionTestModel(decisionShape: IProcessShape): IProcess {
    var shapesFactory = createShapesFactoryService();
    let model: IProcess = createProcessModel(1, ProcessType.UserToSystemProcess);

    let start = createShapeModel(ProcessShapeType.Start, 10, 0, 0);
    let pre = shapesFactory.createModelSystemTaskShape(1, 0, 15, 1, 0);
    let ut1 = shapesFactory.createModelUserTaskShape(1, 0, 20, 2, 0);
    let st2 = shapesFactory.createModelSystemTaskShape(1, 0, 25, 4, 0);
    let ut2 = shapesFactory.createModelUserTaskShape(1, 0, 35, 2, 0);
    let st3 = shapesFactory.createModelSystemTaskShape(1, 0, 40, 4, 0);
    let ut3 = shapesFactory.createModelUserTaskShape(1, 0, 45, 2, 0);
    let st4 = shapesFactory.createModelSystemTaskShape(1, 0, 50, 4, 0);
    let end = createShapeModel(ProcessShapeType.End, 55, 5, 0);


    model.shapes.push(start, pre, ut1, st2, decisionShape, ut2, st3, ut3, st4, end);
    /*
        start-> pre -> UT1 -> ST2 -> UD -> UT2 -> ST3 -> END
                                        -> UT3 -> ST4 -> END
    */

    model.links.push({ sourceId: start.id, destinationId: pre.id, orderindex: 0, label: null });
    model.links.push({ sourceId: pre.id, destinationId: ut1.id, orderindex: 0, label: null });
    model.links.push({ sourceId: ut1.id, destinationId: st2.id, orderindex: 0, label: null });
    model.links.push({ sourceId: st2.id, destinationId: decisionShape.id, orderindex: 0, label: null });
    model.links.push({ sourceId: decisionShape.id, destinationId: ut2.id, orderindex: 0, label: null });
    model.links.push({ sourceId: decisionShape.id, destinationId: ut3.id, orderindex: 1, label: null });
    model.links.push({ sourceId: ut2.id, destinationId: st3.id, orderindex: 0, label: null });
    model.links.push({ sourceId: ut3.id, destinationId: st4.id, orderindex: 0, label: null });
    model.links.push({ sourceId: st3.id, destinationId: end.id, orderindex: 0, label: null });
    model.links.push({ sourceId: st4.id, destinationId: end.id, orderindex: 0, label: null });

    model.decisionBranchDestinationLinks.push(
        { sourceId: decisionShape.id, destinationId: end.id, orderindex: 1, label: null }
    );

    return model;
}


export function createSystemDecisionTestModel(decisionShape: IProcessShape): IProcess {
    var shapesFactory = createShapesFactoryService();
    let model: IProcess = createProcessModel(1, ProcessType.UserToSystemProcess);

    let start = createShapeModel(ProcessShapeType.Start, 10, 0, 0);
    let pre = shapesFactory.createModelSystemTaskShape(1, 0, 15, 1, 0);
    let ut1 = shapesFactory.createModelUserTaskShape(1, 0, 20, 2, 0);
    let st2 = shapesFactory.createModelSystemTaskShape(1, 0, 30, 4, 0);
    let st3 = shapesFactory.createModelSystemTaskShape(1, 0, 35, 4, 0);
    let end = createShapeModel(ProcessShapeType.End, 40, 5, 0);


    model.shapes.push(start, pre, ut1, decisionShape, st2, st3, end);
    /*
        start -> PRE -> UT1 -> SD ->  ST2 -> END
                                  ->  ST3 -> END
    */

    model.links.push({ sourceId: start.id, destinationId: pre.id, orderindex: 0, label: null });
    model.links.push({ sourceId: pre.id, destinationId: ut1.id, orderindex: 0, label: null });
    model.links.push({ sourceId: ut1.id, destinationId: decisionShape.id, orderindex: 0, label: null });
    model.links.push({ sourceId: decisionShape.id, destinationId: st2.id, orderindex: 0, label: null });
    model.links.push({ sourceId: decisionShape.id, destinationId: st3.id, orderindex: 1, label: null });
    model.links.push({ sourceId: st2.id, destinationId: end.id, orderindex: 0, label: null });
    model.links.push({ sourceId: st3.id, destinationId: end.id, orderindex: 0, label: null });

    model.decisionBranchDestinationLinks.push(
        { sourceId: decisionShape.id, destinationId: end.id, orderindex: 1, label: null }
    );

    return model;
}
export function createUserDecisionWithoutUserTaskInFirstConditionModel(firstConditionLabel: string = null, secondConditionLabel: string = null): IProcess {
    let process: IProcess = createProcessModel();

    let start = createShapeModel(ProcessShapeType.Start, 20);
    let pre = createShapeModel(ProcessShapeType.PreconditionSystemTask, 30);
    let ud = createShapeModel(ProcessShapeType.UserDecision, 40);
    let ut1 = createShapeModel(ProcessShapeType.UserTask, 50);
    let st1 = createShapeModel(ProcessShapeType.SystemTask, 60);
    let end = createShapeModel(ProcessShapeType.End, 70);

    process.shapes.push(start, pre, ud, ut1, st1, end);

    // Start -> Pre -> UD -> End
    //              -> UT1 -> ST1 -> End

    process.links.push(
        { sourceId: start.id, destinationId: pre.id, orderindex: 0, label: null },
        { sourceId: pre.id, destinationId: ud.id, orderindex: 0, label: null },
        { sourceId: ud.id, destinationId: end.id, orderindex: 0, label: firstConditionLabel },
        { sourceId: ud.id, destinationId: ut1.id, orderindex: 1, label: secondConditionLabel },
        { sourceId: ut1.id, destinationId: st1.id, orderindex: 0, label: null },
        { sourceId: st1.id, destinationId: end.id, orderindex: 0, label: null }
    );

    process.decisionBranchDestinationLinks.push(
        { sourceId: ud.id, destinationId: end.id, orderindex: 1, label: null }
    );

    return process;
}

export function createUserDecisionWithMaximumConditionsModel(): IProcess {
    let process: IProcess = createProcessModel();

    let start = createShapeModel(ProcessShapeType.Start, 20);
    let pre = createShapeModel(ProcessShapeType.PreconditionSystemTask, 30);
    let ud = createShapeModel(ProcessShapeType.UserDecision, 40);
    let ut1 = createShapeModel(ProcessShapeType.UserTask, 50);
    let st1 = createShapeModel(ProcessShapeType.SystemTask, 60);
    let ut2 = createShapeModel(ProcessShapeType.UserTask, 70);
    let st2 = createShapeModel(ProcessShapeType.SystemTask, 80);
    let ut3 = createShapeModel(ProcessShapeType.UserTask, 90);
    let st3 = createShapeModel(ProcessShapeType.SystemTask, 100);
    let ut4 = createShapeModel(ProcessShapeType.UserTask, 110);
    let st4 = createShapeModel(ProcessShapeType.SystemTask, 120);
    let ut5 = createShapeModel(ProcessShapeType.UserTask, 130);
    let st5 = createShapeModel(ProcessShapeType.SystemTask, 140);
    let ut6 = createShapeModel(ProcessShapeType.UserTask, 150);
    let st6 = createShapeModel(ProcessShapeType.SystemTask, 160);
    let ut7 = createShapeModel(ProcessShapeType.UserTask, 170);
    let st7 = createShapeModel(ProcessShapeType.SystemTask, 180);
    let ut8 = createShapeModel(ProcessShapeType.UserTask, 190);
    let st8 = createShapeModel(ProcessShapeType.SystemTask, 200);
    let ut9 = createShapeModel(ProcessShapeType.UserTask, 210);
    let st9 = createShapeModel(ProcessShapeType.SystemTask, 220);
    let ut10 = createShapeModel(ProcessShapeType.UserTask, 230);
    let st10 = createShapeModel(ProcessShapeType.SystemTask, 240);
    let end = createShapeModel(ProcessShapeType.End, 250);

    process.shapes.push(start, pre, ud, ut1, st1, ut2, st2, ut3, st3, ut4, st4, ut5, st5, ut6, st6, ut7, st7, ut8, st8, ut9, st9, ut10, st10, end);

    process.links.push(
        { sourceId: start.id, destinationId: pre.id, orderindex: 0, label: null },
        { sourceId: pre.id, destinationId: ud.id, orderindex: 0, label: null },
        { sourceId: ud.id, destinationId: ut1.id, orderindex: 0, label: null },
        { sourceId: ut1.id, destinationId: st1.id, orderindex: 0, label: null },
        { sourceId: st1.id, destinationId: end.id, orderindex: 0, label: null },
        { sourceId: ud.id, destinationId: ut2.id, orderindex: 1, label: null },
        { sourceId: ut2.id, destinationId: st2.id, orderindex: 0, label: null },
        { sourceId: st2.id, destinationId: end.id, orderindex: 0, label: null },
        { sourceId: ud.id, destinationId: ut3.id, orderindex: 2, label: null },
        { sourceId: ut3.id, destinationId: st3.id, orderindex: 0, label: null },
        { sourceId: st3.id, destinationId: end.id, orderindex: 0, label: null },
        { sourceId: ud.id, destinationId: ut4.id, orderindex: 3, label: null },
        { sourceId: ut4.id, destinationId: st4.id, orderindex: 0, label: null },
        { sourceId: st4.id, destinationId: end.id, orderindex: 0, label: null },
        { sourceId: ud.id, destinationId: ut5.id, orderindex: 4, label: null },
        { sourceId: ut5.id, destinationId: st5.id, orderindex: 0, label: null },
        { sourceId: st5.id, destinationId: end.id, orderindex: 0, label: null },
        { sourceId: ud.id, destinationId: ut6.id, orderindex: 5, label: null },
        { sourceId: ut6.id, destinationId: st6.id, orderindex: 0, label: null },
        { sourceId: st6.id, destinationId: end.id, orderindex: 0, label: null },
        { sourceId: ud.id, destinationId: ut7.id, orderindex: 6, label: null },
        { sourceId: ut7.id, destinationId: st7.id, orderindex: 0, label: null },
        { sourceId: st7.id, destinationId: end.id, orderindex: 0, label: null },
        { sourceId: ud.id, destinationId: ut8.id, orderindex: 7, label: null },
        { sourceId: ut8.id, destinationId: st8.id, orderindex: 0, label: null },
        { sourceId: st8.id, destinationId: end.id, orderindex: 0, label: null },
        { sourceId: ud.id, destinationId: ut9.id, orderindex: 8, label: null },
        { sourceId: ut9.id, destinationId: st9.id, orderindex: 0, label: null },
        { sourceId: st9.id, destinationId: end.id, orderindex: 0, label: null },
        { sourceId: ud.id, destinationId: ut10.id, orderindex: 9, label: null },
        { sourceId: ut10.id, destinationId: st10.id, orderindex: 0, label: null },
        { sourceId: st10.id, destinationId: end.id, orderindex: 0, label: null }
    );

    process.decisionBranchDestinationLinks.push(
        { sourceId: ud.id, destinationId: end.id, orderindex: 1, label: null },
        { sourceId: ud.id, destinationId: end.id, orderindex: 2, label: null },
        { sourceId: ud.id, destinationId: end.id, orderindex: 3, label: null },
        { sourceId: ud.id, destinationId: end.id, orderindex: 4, label: null },
        { sourceId: ud.id, destinationId: end.id, orderindex: 5, label: null },
        { sourceId: ud.id, destinationId: end.id, orderindex: 6, label: null },
        { sourceId: ud.id, destinationId: end.id, orderindex: 7, label: null },
        { sourceId: ud.id, destinationId: end.id, orderindex: 8, label: null },
        { sourceId: ud.id, destinationId: end.id, orderindex: 9, label: null }
    );

    return process;
}

export function createSystemDecisionWithTwoBranchesModel(): IProcess {
    var shapesFactory = createShapesFactoryService();
    let model: IProcess = createProcessModel(1, ProcessType.UserToSystemProcess);

    let start = createShapeModel(ProcessShapeType.Start, 2, 0, 0);
    let pre = shapesFactory.createModelSystemTaskShape(1, 0, 3, 1, 0);
    let ut1 = shapesFactory.createModelUserTaskShape(1, 0, 4, 2, 0);
    let sd = shapesFactory.createSystemDecisionShapeModel(5, 1, 0, 3, 0);
    let st1 = shapesFactory.createModelSystemTaskShape(1, 0, 6, 4, 0);
    let st2 = shapesFactory.createModelSystemTaskShape(1, 0, 7, 4, 1);
    let end = createShapeModel(ProcessShapeType.End, 8, 6, 0);

    model.shapes.push(start, pre, ut1, sd, st1, st2, end);

    model.links.push({ sourceId: start.id, destinationId: pre.id, orderindex: 0, label: null });
    model.links.push({ sourceId: pre.id, destinationId: ut1.id, orderindex: 0, label: null });
    model.links.push({ sourceId: ut1.id, destinationId: sd.id, orderindex: 0, label: null });
    model.links.push({ sourceId: sd.id, destinationId: st1.id, orderindex: 0, label: null });
    model.links.push({ sourceId: st1.id, destinationId: end.id, orderindex: 0, label: null });
    model.links.push({ sourceId: sd.id, destinationId: st2.id, orderindex: 1, label: null });
    model.links.push({ sourceId: st2.id, destinationId: end.id, orderindex: 0, label: null });

    model.decisionBranchDestinationLinks.push({ sourceId: sd.id, destinationId: end.id, orderindex: 1, label: null });

    return model;
}

export function createSystemDecisionWithMaximumConditionsModel(): IProcess {
    let process: IProcess = createProcessModel();

    let start = createShapeModel(ProcessShapeType.Start, 20);
    let pre = createShapeModel(ProcessShapeType.PreconditionSystemTask, 30);
    let ut1 = createShapeModel(ProcessShapeType.UserTask, 40);
    let sd = createShapeModel(ProcessShapeType.SystemDecision, 50);
    let st1 = createShapeModel(ProcessShapeType.SystemTask, 60);
    let st2 = createShapeModel(ProcessShapeType.SystemTask, 70);
    let st3 = createShapeModel(ProcessShapeType.SystemTask, 80);
    let st4 = createShapeModel(ProcessShapeType.SystemTask, 90);
    let st5 = createShapeModel(ProcessShapeType.SystemTask, 100);
    let st6 = createShapeModel(ProcessShapeType.SystemTask, 110);
    let st7 = createShapeModel(ProcessShapeType.SystemTask, 120);
    let st8 = createShapeModel(ProcessShapeType.SystemTask, 130);
    let st9 = createShapeModel(ProcessShapeType.SystemTask, 140);
    let st10 = createShapeModel(ProcessShapeType.SystemTask, 150);
    let end = createShapeModel(ProcessShapeType.End, 160);

    process.shapes.push(start, pre, ut1, sd, st1, st2, st3, st4, st5, st6, st7, st8, st9, st10, end);

    process.links.push(
        { sourceId: start.id, destinationId: pre.id, orderindex: 0, label: null },
        { sourceId: pre.id, destinationId: ut1.id, orderindex: 0, label: null },
        { sourceId: ut1.id, destinationId: sd.id, orderindex: 0, label: null },
        { sourceId: sd.id, destinationId: st1.id, orderindex: 0, label: null },
        { sourceId: st1.id, destinationId: end.id, orderindex: 0, label: null },
        { sourceId: sd.id, destinationId: st2.id, orderindex: 1, label: null },
        { sourceId: st2.id, destinationId: end.id, orderindex: 0, label: null },
        { sourceId: sd.id, destinationId: st3.id, orderindex: 2, label: null },
        { sourceId: st3.id, destinationId: end.id, orderindex: 0, label: null },
        { sourceId: sd.id, destinationId: st4.id, orderindex: 3, label: null },
        { sourceId: st4.id, destinationId: end.id, orderindex: 0, label: null },
        { sourceId: sd.id, destinationId: st5.id, orderindex: 4, label: null },
        { sourceId: st5.id, destinationId: end.id, orderindex: 0, label: null },
        { sourceId: sd.id, destinationId: st6.id, orderindex: 5, label: null },
        { sourceId: st6.id, destinationId: end.id, orderindex: 0, label: null },
        { sourceId: sd.id, destinationId: st7.id, orderindex: 6, label: null },
        { sourceId: st7.id, destinationId: end.id, orderindex: 0, label: null },
        { sourceId: sd.id, destinationId: st8.id, orderindex: 7, label: null },
        { sourceId: st8.id, destinationId: end.id, orderindex: 0, label: null },
        { sourceId: sd.id, destinationId: st9.id, orderindex: 8, label: null },
        { sourceId: st9.id, destinationId: end.id, orderindex: 0, label: null },
        { sourceId: sd.id, destinationId: st10.id, orderindex: 9, label: null },
        { sourceId: st10.id, destinationId: end.id, orderindex: 0, label: null }
    );

    process.decisionBranchDestinationLinks.push(
        { sourceId: sd.id, destinationId: end.id, orderindex: 1, label: null },
        { sourceId: sd.id, destinationId: end.id, orderindex: 2, label: null },
        { sourceId: sd.id, destinationId: end.id, orderindex: 3, label: null },
        { sourceId: sd.id, destinationId: end.id, orderindex: 4, label: null },
        { sourceId: sd.id, destinationId: end.id, orderindex: 5, label: null },
        { sourceId: sd.id, destinationId: end.id, orderindex: 6, label: null },
        { sourceId: sd.id, destinationId: end.id, orderindex: 7, label: null },
        { sourceId: sd.id, destinationId: end.id, orderindex: 8, label: null },
        { sourceId: sd.id, destinationId: end.id, orderindex: 9, label: null }
    );

    return process;
}

export function createUserDecisionWithMultipleBranchesModel(): IProcess {
    var shapesFactory = createShapesFactoryService();
    let model: IProcess = createProcessModel(1, ProcessType.UserToSystemProcess);

    /*
        start -> pre -> ud -> ut1 -> st1 -> end
                            ->ut2 -> st2 -> end
                            ->ut3 -> st3 -> end
                            ->ut4 -> st4 -> end

    */
    let start = createShapeModel(ProcessShapeType.Start, 2, 0, 0);
    let pre = shapesFactory.createModelSystemTaskShape(1, 0, 3, 1, 0);
    let ud = shapesFactory.createModelUserDecisionShape(1, 0, 4, 2, 0);
    let ut1 = shapesFactory.createModelUserTaskShape(1, 0, 5, 3, 0);
    let st1 = shapesFactory.createModelSystemTaskShape(1, 0, 6, 4, 0);
    let ut2 = shapesFactory.createModelUserTaskShape(1, 0, 7, 3, 1);
    let st2 = shapesFactory.createModelSystemTaskShape(1, 0, 8, 4, 1);
    let ut3 = shapesFactory.createModelUserTaskShape(1, 0, 9, 3, 2);
    let st3 = shapesFactory.createModelSystemTaskShape(1, 0, 10, 4, 2);
    let ut4 = shapesFactory.createModelUserTaskShape(1, 0, 11, 3, 3);
    let st4 = shapesFactory.createModelSystemTaskShape(1, 0, 12, 4, 3);
    let end = createShapeModel(ProcessShapeType.End, 13, 5, 0);

    model.shapes.push(start, pre, ud, ut1, st1, ut2, st2, ut3, st3, ut4, st4, end);

    model.links.push({ sourceId: start.id, destinationId: pre.id, orderindex: 0, label: null });
    model.links.push({ sourceId: pre.id, destinationId: ud.id, orderindex: 0, label: null });
    model.links.push({ sourceId: ud.id, destinationId: ut1.id, orderindex: 0, label: null });
    model.links.push({ sourceId: ut1.id, destinationId: st1.id, orderindex: 0, label: null });
    model.links.push({ sourceId: st1.id, destinationId: end.id, orderindex: 0, label: null });
    model.links.push({ sourceId: ud.id, destinationId: ut2.id, orderindex: 1, label: null });
    model.links.push({ sourceId: ut2.id, destinationId: st2.id, orderindex: 0, label: null });
    model.links.push({ sourceId: st2.id, destinationId: end.id, orderindex: 0, label: null });
    model.links.push({ sourceId: ud.id, destinationId: ut3.id, orderindex: 2, label: null });
    model.links.push({ sourceId: ut3.id, destinationId: st3.id, orderindex: 0, label: null });
    model.links.push({ sourceId: st3.id, destinationId: end.id, orderindex: 0, label: null });
    model.links.push({ sourceId: ud.id, destinationId: ut4.id, orderindex: 3, label: null });
    model.links.push({ sourceId: ut4.id, destinationId: st4.id, orderindex: 0, label: null });
    model.links.push({ sourceId: st4.id, destinationId: end.id, orderindex: 0, label: null });

    model.decisionBranchDestinationLinks.push(
        { sourceId: ud.id, destinationId: end.id, orderindex: 1, label: null },
        { sourceId: ud.id, destinationId: end.id, orderindex: 2, label: null },
        { sourceId: ud.id, destinationId: end.id, orderindex: 3, label: null }
    );

    return model;
}

export function createUserDecisionWithMultipleBranchesModel_V2(): IProcess {

    /*
    start -> pre - ud -> ut1 -> st1 ->                  ut5 -> st5 ->  end
                      -> ut2 -> st2 -> ut6 -> st6 -> 
                      -> ut3 -> st3 ->
                      -> ut4 -> st4 ->
    */

    var shapesFactory = createShapesFactoryService();
    let model: IProcess = createProcessModel(1, ProcessType.UserToSystemProcess);

    let start = createShapeModel(ProcessShapeType.Start, 2, 0, 0); start.name = "start";
    let pre = shapesFactory.createModelSystemTaskShape(1, 0, 3, 1, 0); pre.name = "pre";
    let ud = shapesFactory.createModelUserDecisionShape(1, 0, 4, 2, 0); ud.name = "ud";
    let ut1 = shapesFactory.createModelUserTaskShape(1, 0, 5, 3, 0); ut1.name = "ut1";
    let st1 = shapesFactory.createModelSystemTaskShape(1, 0, 6, 4, 0); st1.name = "st1";
    let ut2 = shapesFactory.createModelUserTaskShape(1, 0, 7, 3, 1); ut2.name = "ut2";
    let st2 = shapesFactory.createModelSystemTaskShape(1, 0, 8, 4, 1); st2.name = "st2";
    let ut3 = shapesFactory.createModelUserTaskShape(1, 0, 9, 3, 2); ut3.name = "ut3";
    let st3 = shapesFactory.createModelSystemTaskShape(1, 0, 10, 4, 2); st3.name = "st3";
    let ut4 = shapesFactory.createModelUserTaskShape(1, 0, 11, 3, 3); ut4.name = "ut4";
    let st4 = shapesFactory.createModelSystemTaskShape(1, 0, 12, 4, 3); st4.name = "st4";

    let ut5 = shapesFactory.createModelUserTaskShape(1, 0, 13, 7, 0); ut5.name = "ut5";
    let st5 = shapesFactory.createModelSystemTaskShape(1, 0, 14, 8, 0); st5.name = "st5";
    let ut6 = shapesFactory.createModelUserTaskShape(1, 0, 15, 5, 1); ut6.name = "ut6";
    let st6 = shapesFactory.createModelSystemTaskShape(1, 0, 16, 6, 1); st6.name = "st6";

    let end = createShapeModel(ProcessShapeType.End, 17, 9, 0); end.name = "end";

    model.shapes.push(start);
    model.shapes.push(pre);
    model.shapes.push(ud);
    model.shapes.push(ut1);
    model.shapes.push(st1);
    model.shapes.push(ut2);
    model.shapes.push(st2);
    model.shapes.push(ut3);
    model.shapes.push(st3);
    model.shapes.push(ut4);
    model.shapes.push(st4);
    model.shapes.push(ut5);
    model.shapes.push(st5);
    model.shapes.push(ut6);
    model.shapes.push(st6);
    model.shapes.push(end);

    /*
    start -> pre - ud -> ut1 -> st1 ->                  ut5 -> st5 ->  end
                      -> ut2 -> st2 -> ut6 -> st6 -> 
                      -> ut3 -> st3 ->
                      -> ut4 -> st4 ->
    */

    model.links.push({ sourceId: start.id, destinationId: pre.id, orderindex: 0, label: null });
    model.links.push({ sourceId: pre.id, destinationId: ud.id, orderindex: 0, label: null });

    model.links.push({ sourceId: ud.id, destinationId: ut1.id, orderindex: 0, label: null });
    model.links.push({ sourceId: ut1.id, destinationId: st1.id, orderindex: 0, label: null });
    model.links.push({ sourceId: st1.id, destinationId: ut5.id, orderindex: 0, label: null });

    model.links.push({ sourceId: ud.id, destinationId: ut2.id, orderindex: 1, label: null });
    model.links.push({ sourceId: ut2.id, destinationId: st2.id, orderindex: 0, label: null });
    model.links.push({ sourceId: st2.id, destinationId: ut6.id, orderindex: 0, label: null });
    model.links.push({ sourceId: ut6.id, destinationId: st6.id, orderindex: 0, label: null });
    model.links.push({ sourceId: st6.id, destinationId: ut5.id, orderindex: 0, label: null });

    model.links.push({ sourceId: ud.id, destinationId: ut3.id, orderindex: 2, label: null });
    model.links.push({ sourceId: ut3.id, destinationId: st3.id, orderindex: 0, label: null });
    model.links.push({ sourceId: st3.id, destinationId: ut5.id, orderindex: 0, label: null });

    model.links.push({ sourceId: ud.id, destinationId: ut4.id, orderindex: 3, label: null });
    model.links.push({ sourceId: ut4.id, destinationId: st4.id, orderindex: 0, label: null });
    model.links.push({ sourceId: st4.id, destinationId: ut5.id, orderindex: 0, label: null });

    model.links.push({ sourceId: ut5.id, destinationId: st5.id, orderindex: 0, label: null });
    model.links.push({ sourceId: st5.id, destinationId: end.id, orderindex: 0, label: null });

    model.decisionBranchDestinationLinks.push({ sourceId: ud.id, destinationId: ut5.id, orderindex: 1, label: null });
    model.decisionBranchDestinationLinks.push({ sourceId: ud.id, destinationId: ut5.id, orderindex: 2, label: null });
    model.decisionBranchDestinationLinks.push({ sourceId: ud.id, destinationId: ut5.id, orderindex: 3, label: null });

    return model;
}

export function createMultipleUserDecisionsWithMultipleBranchesModel(): IProcess {
    var shapesFactory = createShapesFactoryService();
    let model: IProcess = createProcessModel(1, ProcessType.UserToSystemProcess);

    let start = createShapeModel(ProcessShapeType.Start, 2, 0, 0);
    let pre = shapesFactory.createModelSystemTaskShape(1, 0, 3, 1, 0);

    let ud1 = shapesFactory.createModelUserDecisionShape(1, 0, 4, 2, 0);

    let ut1 = shapesFactory.createModelUserTaskShape(1, 0, 5, 3, 0);
    let st1 = shapesFactory.createModelSystemTaskShape(1, 0, 6, 4, 0);

    let ut2 = shapesFactory.createModelUserTaskShape(1, 0, 7, 3, 1);
    let st2 = shapesFactory.createModelSystemTaskShape(1, 0, 8, 4, 1);

    let ud2 = shapesFactory.createModelUserDecisionShape(1, 0, 9, 2, 0);

    let ut3 = shapesFactory.createModelUserTaskShape(1, 0, 10, 3, 2);
    let st3 = shapesFactory.createModelSystemTaskShape(1, 0, 11, 4, 2);

    let ut4 = shapesFactory.createModelUserTaskShape(1, 0, 12, 3, 3);
    let st4 = shapesFactory.createModelSystemTaskShape(1, 0, 13, 4, 3);

    let end = createShapeModel(ProcessShapeType.End, 14, 5, 0);

    model.shapes.push(start, pre, ud1, ut1, st1, ut2, st2, ud2, ut3, st3, ut4, st4, end);

    model.links.push({ sourceId: start.id, destinationId: pre.id, orderindex: 0, label: null });
    model.links.push({ sourceId: pre.id, destinationId: ud1.id, orderindex: 0, label: null });

    model.links.push({ sourceId: ud1.id, destinationId: ut1.id, orderindex: 0, label: null });
    model.links.push({ sourceId: ut1.id, destinationId: st1.id, orderindex: 0, label: null });
    model.links.push({ sourceId: st1.id, destinationId: ud2.id, orderindex: 0, label: null });

    model.links.push({ sourceId: ud1.id, destinationId: ut2.id, orderindex: 1, label: null });
    model.links.push({ sourceId: ut2.id, destinationId: st2.id, orderindex: 0, label: null });
    model.links.push({ sourceId: st2.id, destinationId: ud2.id, orderindex: 0, label: null });

    model.links.push({ sourceId: ud2.id, destinationId: ut3.id, orderindex: 0, label: null });
    model.links.push({ sourceId: ut3.id, destinationId: st3.id, orderindex: 0, label: null });
    model.links.push({ sourceId: st3.id, destinationId: end.id, orderindex: 0, label: null });

    model.links.push({ sourceId: ud2.id, destinationId: ut4.id, orderindex: 1, label: null });
    model.links.push({ sourceId: ut4.id, destinationId: st4.id, orderindex: 0, label: null });
    model.links.push({ sourceId: st4.id, destinationId: end.id, orderindex: 0, label: null });

    model.decisionBranchDestinationLinks.push(
        { sourceId: ud1.id, destinationId: ud2.id, orderindex: 1, label: null },
        { sourceId: ud2.id, destinationId: end.id, orderindex: 1, label: null }
    );

    return model;
}

export function createSystemDecisionWithMultipleBranchesModel(): IProcess {
    var shapesFactory = createShapesFactoryService();
    let model: IProcess = createProcessModel(1, ProcessType.UserToSystemProcess);

    let start = createShapeModel(ProcessShapeType.Start, 2, 0, 0);
    let pre = shapesFactory.createModelSystemTaskShape(1, 0, 3, 1, 0);
    let ut1 = shapesFactory.createModelUserTaskShape(1, 0, 4, 2, 0);
    let sd = shapesFactory.createSystemDecisionShapeModel(5, 1, 0, 3, 0);
    let st1 = shapesFactory.createModelSystemTaskShape(1, 0, 6, 4, 0);
    let st2 = shapesFactory.createModelSystemTaskShape(1, 0, 7, 4, 1);
    let st3 = shapesFactory.createModelSystemTaskShape(1, 0, 8, 4, 2);
    let end = createShapeModel(ProcessShapeType.End, 9, 5, 0);

    model.shapes.push(start, pre, ut1, sd, st1, st2, st3, end);

    model.links.push({ sourceId: start.id, destinationId: pre.id, orderindex: 0, label: null });
    model.links.push({ sourceId: pre.id, destinationId: ut1.id, orderindex: 0, label: null });
    model.links.push({ sourceId: ut1.id, destinationId: sd.id, orderindex: 0, label: null });
    model.links.push({ sourceId: sd.id, destinationId: st1.id, orderindex: 0, label: null });
    model.links.push({ sourceId: st1.id, destinationId: end.id, orderindex: 0, label: null });
    model.links.push({ sourceId: sd.id, destinationId: st2.id, orderindex: 1, label: null });
    model.links.push({ sourceId: st2.id, destinationId: end.id, orderindex: 0, label: null });
    model.links.push({ sourceId: sd.id, destinationId: st3.id, orderindex: 2, label: null });
    model.links.push({ sourceId: st3.id, destinationId: end.id, orderindex: 0, label: null });

    model.decisionBranchDestinationLinks.push(
        { sourceId: sd.id, destinationId: end.id, orderindex: 1, label: null },
        { sourceId: sd.id, destinationId: end.id, orderindex: 2, label: null }
    );

    return model;
}


function populatePropertyValues(shape: any, labelValue: string, x: number, y: number, clientType: ProcessShapeType) {

    var shapesFactory = createShapesFactoryService();
    shape.propertyValues["label"] = shapesFactory.createLabelValue(labelValue);
    shape.propertyValues["x"] = shapesFactory.createXValue(x);
    shape.propertyValues["y"] = shapesFactory.createYValue(y);
    shape.propertyValues["clientType"] = shapesFactory.createClientTypeValue(clientType);

    if (clientType === ProcessShapeType.UserTask || clientType === ProcessShapeType.SystemTask) {
        shape.propertyValues["persona"] = shapesFactory.createPersonaValue("Persona");
    }

    if (clientType === ProcessShapeType.SystemTask) {
        shape.propertyValues["associatedImageUrl"] = shapesFactory.createAssociatedImageUrlValue("");
    }
}
export function createShapesFactoryService(): ShapesFactory {
    var rootScope: ng.IRootScopeService = {
        index: "",
        $apply: null,
        $applyAsync: null,
        $broadcast: null,
        $emit: null,
        $digest: null,
        $destroy: null,
        $eval: null,
        $evalAsync: null,
        $new: null,
        $on: null,
        $watch: null,
        $watchCollection: null,
        $watchGroup: null,
        $id: null,
        $parent: null,
        $root: null,
        $$isolateBindings: null,
        $$phase: null
    };
    rootScope["config"] = {};
    rootScope["config"].labels = {
        "ST_Persona_Label": "Persona",
        "ST_Colors_Label": "Color",
        "ST_Comments_Label": "Comments"
    };

    return new ShapesFactory(rootScope);
}