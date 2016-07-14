// import interfaces defined in TypeLite.Net4.d.ts

// BluePrintSys.RC.CrossCutting
import ProcessType = BluePrintSys.RC.CrossCutting.ProcessType;
import ProcessShapeType = BluePrintSys.RC.CrossCutting.ProcessShapeType;
import IVersionInfo = BluePrintSys.RC.CrossCutting.IVersionInfo; 

// BluePrintSys.RC.Service.Business.Repository.Models.Storyteller
//import IProcess = BluePrintSys.RC.Business.Internal.Components.Storyteller.Models.IProcess;
//import IProcessShape = BluePrintSys.RC.Business.Internal.Components.Storyteller.Models.IProcessShape;
import IProcess = BluePrintSys.RC.Service.Business.Repository.Models.Storyteller.IProcess;
import IProcessShape = BluePrintSys.RC.Service.Business.Repository.Models.Storyteller.IProcessShape;
import IProcessLink = BluePrintSys.RC.Service.Business.Repository.Models.Storyteller.IProcessLink;
import ITaskShape = BluePrintSys.RC.Service.Business.Repository.Models.Storyteller.ITaskShape;
import IUserTaskShape = BluePrintSys.RC.Service.Business.Repository.Models.Storyteller.IUserTaskShape;
import ISystemTaskShape = BluePrintSys.RC.Service.Business.Repository.Models.Storyteller.ISystemTaskShape;
import IArtifactReference = BluePrintSys.RC.Service.Business.Repository.Models.Storyteller.IArtifactReference;
import IPropertyValueInformation = BluePrintSys.RC.Service.Business.Models.Common.IPropertyValueInformation;
import IArtifactReferenceLink = BluePrintSys.RC.Service.Business.Repository.Models.Storyteller.IArtifactReferenceLink;
import IUserStory = BluePrintSys.RC.Service.Business.Repository.Models.Storyteller.IUserStory;
import IHashMapOfPropertyValues = BluePrintSys.RC.Business.Internal.Models.IHashMapOfPropertyValues;
import IItemStatus = BluePrintSys.RC.Service.Business.Repository.Models.Storyteller.IItemStatus;
import ITaskFlags = BluePrintSys.RC.Service.Business.Repository.Models.Storyteller.ITaskFlags;

// BluePrintSys.RC.Business.Internal.Components
import IFileResult = BluePrintSys.RC.Business.Internal.Components.FileStore.Model.IFileResult;
import IProcessUpdateResult = BluePrintSys.RC.Business.Internal.Components.Storyteller.Models.IProcessUpdateResult;
import IOperationMessageResult = BluePrintSys.RC.Business.Internal.Components.Shared.Models.IOperationMessageResult;
import LockResult = BluePrintSys.RC.Service.Business.Models.Api.LockResult;
import ILockResultInfo = BluePrintSys.RC.Business.Internal.Models.ILockResultInfo;