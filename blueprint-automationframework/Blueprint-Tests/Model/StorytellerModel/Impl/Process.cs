using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Common;
using Model.ArtifactModel;
using Model.ArtifactModel.Enums;
using Newtonsoft.Json;
using NUnit.Framework;
using Utilities;
using Utilities.Factories;
using Model.ArtifactModel.Impl;

namespace Model.StorytellerModel.Impl
{
    /// <summary>
    /// The Storyteller Process Model
    /// </summary>
    public class Process: IProcess
    {
        #region Constants

        public const string StartName = "Start";

        public const string DefaultPreconditionName = "Precondition";

        public const string DefaultUserTaskName = "UT";

        public const string DefaultSystemTaskName = "ST";

        public const string EndName = "End";

        public const double DefaultOrderIndex = 0;

        private static readonly string Description = PropertyTypePredefined.Description.ToString();

        private static readonly string Label = PropertyTypePredefined.Label.ToString();

        private static readonly string X = PropertyTypePredefined.X.ToString();

        private static readonly string Y = PropertyTypePredefined.Y.ToString();

        private static readonly string Height = PropertyTypePredefined.Height.ToString();

        private static readonly string Width = PropertyTypePredefined.Width.ToString();

        private static readonly string ClientType = PropertyTypePredefined.ClientType.ToString();

        private static readonly string Persona = PropertyTypePredefined.Persona.ToString();

        private static readonly string ImageId = PropertyTypePredefined.ImageId.ToString();

        private const string AssociatedImageUrl = "AssociatedImageUrl";

        private const string StoryLinks = "StoryLinks";

        private const string ItemLabel = "ItemLabel";

        private const string SystemTaskNamePrefix = "ST";

        private const string UserTaskNamePrefix = "UT";

        private const string ProcessShapeTypePrefix = "PROS";

        private const string UserDecisionNamePrefix = "UD";

        private const string SystemDecisionNamePrefix = "SD";

        private const string DefaultDecisionLabelPrefix = "Condition";

        public const int NumberOfShapesInDefaultProcess = 5;
        #endregion Constants

        #region Private Properties

        private int _tempId;

        #endregion Private Properties

        #region Public Properties

        public int ProjectId { get; set; }

        public int Id { get; set; }

        public string Name { get; set; }

        public string TypePrefix { get; set; }

        public ItemTypePredefined BaseItemTypePredefined { get; set; }

        [SuppressMessage("Microsoft.Usage",
            "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonConverter(typeof(Deserialization.ConcreteConverter<List<ProcessShape>>))]
        public List<ProcessShape> Shapes { get; set; }

        [SuppressMessage("Microsoft.Usage",
            "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonConverter(typeof(Deserialization.ConcreteConverter<List<ProcessLink>>))]
        public List<ProcessLink> Links { get; set; }

        [SuppressMessage("Microsoft.Usage",
            "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonConverter(typeof(Deserialization.ConcreteConverter<List<DecisionBranchDestinationLink>>))]
        public List<DecisionBranchDestinationLink> DecisionBranchDestinationLinks { get; set; }

        [SuppressMessage("Microsoft.Usage",
            "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonConverter(typeof(Deserialization.ConcreteDictionaryConverter<Dictionary<string, PropertyValueInformation>, PropertyValueInformation>))]
        public Dictionary<string, PropertyValueInformation> PropertyValues { get; set; }

        public ProcessStatus Status { get; set; }

        public VersionInfo RequestedVersionInfo { get; set; }

        public ProcessType ProcessType
        {
            get
            {
                // Must lower the case of the first character to create lower case property name
                var clientTypePropertyName = PropertyTypePredefined.ClientType.ToString().LowerCaseFirstCharacter();

                var processType = (ProcessType)PropertyValues[clientTypePropertyName].Value.ToInt32Invariant();

                return processType;
            } 
            set
            {
                // Must lower the case of the first character to create lower case property name
                var clientTypePropertyName = PropertyTypePredefined.ClientType.ToString().LowerCaseFirstCharacter();

                PropertyValues[clientTypePropertyName].Value = value;
            }
        }

        #endregion Public Properties

        #region Constructors

        public Process()
        {
            Shapes = new List<ProcessShape>();
            Links = new List<ProcessLink>();
            DecisionBranchDestinationLinks = new List<DecisionBranchDestinationLink>();
            PropertyValues = new Dictionary<string, PropertyValueInformation>();
        }

        #endregion Constructors

        #region Public Methods

        public IProcessShape AddUserAndSystemTask(ProcessLink processLink)
        {
            /*
            If you start with this:
                --[??]--+--[??]--

            It becomes this:
                --[??]--+--[UT]--+--[ST]--+--[??]--
            */

            ThrowIf.ArgumentNull(processLink, nameof(processLink));

            // Add a user task
            var userTask = AddUserTask(processLink);

            var userLink = GetOutgoingLinkForShape(userTask);

            // Add a system task to be paired with the user task just created
            AddSystemTask(userLink);

            return userTask;
        }

        public IProcessShape AddXUserTaskAndSystemTask(IProcessShape processShape, int numberOfPairs)
        {
            ThrowIf.ArgumentNull(processShape, nameof(processShape));
            IProcessShape addedUserTaskShape = processShape;

            for (int i = 0; i < numberOfPairs; i++)
            {
                ProcessLink outgoingLink = GetOutgoingLinkForShape(processShape);
                addedUserTaskShape = AddUserAndSystemTask(outgoingLink);
                processShape = GetNextShape(addedUserTaskShape);
            }

            return addedUserTaskShape;
        }

        public IProcessShape AddUserDecisionPointWithBranchBeforeShape(
            IProcessShape nextShape, 
            double orderIndexOfBranch, 
            int? idOfBranchMergePoint = null)
        {
            /*
            If you start with this:
                --[UT]--+--[ST]--

            It becomes this:
                --<UD>--+--[UT]--+--[ST]--+--
                   |                      |
                   +-------[UT]--+--[ST]--+
            */

            // Find the incoming link for the next shape
            var processLink = GetIncomingLinkForShape(nextShape);

            // Add user decision point before next shape
            var userDecisionPoint = AddUserDecisionPoint(processLink);

            // Add a branch to user decision point
            AddBranchToUserDecisionPoint(orderIndexOfBranch, idOfBranchMergePoint, userDecisionPoint);

            return userDecisionPoint;
        }

        public IProcessShape AddUserDecisionPointWithBranchAfterShape(
            IProcessShape previousShape, 
            double orderIndexOfBranch, 
            int? idOfBranchMergePoint = null)
        {
            /*
            If you start with this:
                --[??]--+--

            It becomes this:
                --[??]--+--<UD>--+--[UT]--+--[ST]--+--
                            |                      |
                            +-------[UT]--+--[ST]--+
            */

            // Find the outgoing link for the previous shape
            var outgoingLinkForPreviousShape = GetOutgoingLinkForShape(previousShape);

            var shapeAfterNewUserDecisionPoint = GetProcessShapeTypeById(outgoingLinkForPreviousShape.DestinationId);

            Assert.That(shapeAfterNewUserDecisionPoint != ProcessShapeType.UserDecision, "A user decision point cannot be inserted before an existing user decision point");

            // Add user decision point after the previous shape
            var userDecisionPoint = AddUserDecisionPoint(outgoingLinkForPreviousShape);

            // Add a branch to user decision point
            AddBranchToUserDecisionPoint(orderIndexOfBranch, idOfBranchMergePoint, userDecisionPoint);

            return userDecisionPoint;
        }

        public IProcessShape AddSystemDecisionPointWithBranchBeforeSystemTask(
            IProcessShape nextSystemTaskShape, 
            double orderIndexOfBranch, 
            int? idOfBranchMergePoint = null)
        {
            /*
            If you start with this:
                --[UT]--+--[ST]--+--

            It becomes this:
                --[UT]--+--<SD>--+--[ST]--+--
                            |             |
                            +-------[ST]--+
            */

            // Find the outgoing link for the next system taskshape
            var outgoingProcessLink = GetOutgoingLinkForShape(nextSystemTaskShape);

            // Determine the artifact Id of the branch end point
            int branchEndPointId = idOfBranchMergePoint ?? outgoingProcessLink.DestinationId;

            // Find the incoming link for the next system taskshape
            var incomingProcessLink = GetIncomingLinkForShape(nextSystemTaskShape);

            // Add system decision point before next shape
            var systemDecisionPoint = AddSystemDecisionPoint(incomingProcessLink);

            // Add new branch to system decision point
            AddBranchWithSystemTaskToSystemDecisionPoint(systemDecisionPoint, orderIndexOfBranch, branchEndPointId);

            return systemDecisionPoint;
        }

        public void AddBranchWithSystemTaskToSystemDecisionPoint(
            IProcessShape decisionPoint, 
            double orderIndex, 
            int destinationId)
        {
            /*
            If you start with this:
                --[UT]--+--<SD>--+--[ST]--+--

            It becomes this:
                --[UT]--+--<SD>--+--[ST]--+--
                            |             |
                            +-------[ST]--+
            */

            // Add a process link to the system decision point

            ThrowIf.ArgumentNull(decisionPoint, nameof(decisionPoint));

            var processLink = AddLink(decisionPoint.Id, destinationId, orderIndex);

            // Add default link label            
            processLink.Label = I18NHelper.FormatInvariant("{0} {1}", DefaultDecisionLabelPrefix, (int) orderIndex + 1);

            // Add a system task to the branch and return the system task shape object
            AddSystemTask(processLink);

            // Add a DecisionBranchDestinationLink to register the added branch merge point
            AddDecisionBranchDestinationLink(destinationId, orderIndex, decisionPoint.Id);
        }


        public IProcessShape AddBranchWithUserAndSystemTaskToUserDecisionPoint(
            IProcessShape decisionPoint, 
            double orderIndex, 
            int destinationId)
        {
            /*
            If you start with this:
                --+--<UD>--+--[UT]--+--[ST]--+--

            It becomes this:
                --+--<UD>--+--[UT]--+--[ST]--+--
                      |                      |
                      +-------[UT]--+--[ST]--+
            */

            ThrowIf.ArgumentNull(decisionPoint, nameof(decisionPoint));

            // Add a process link to the user decision point
            var processLink = AddLink(decisionPoint.Id, destinationId, orderIndex);

            // Add default link label
            processLink.Label = I18NHelper.FormatInvariant("{0} {1}", DefaultDecisionLabelPrefix, (int) orderIndex + 1);

            // Add a DecisionBranchDestinationLink to register the added branch merge point
            AddDecisionBranchDestinationLink(destinationId, orderIndex, decisionPoint.Id);

            // Add a user task to the branch and return the user task shape object
            return AddUserAndSystemTask(processLink);
        }

        public ProcessLink AddLink(int sourceId, int destinationId, double orderIndex)
        {
            // Create a process link
            var processLink = new ProcessLink
            {
                DestinationId = destinationId,
                Label = null,
                Orderindex = orderIndex,
                SourceId = sourceId
            };

            // Add the process link to the list of links in the process
            Links.Add(processLink);

            return processLink;
        }

        public DecisionBranchDestinationLink AddDecisionBranchDestinationLink(int destinationId, double orderIndex, int sourceId)
        {
            // Create a DecisionBranchDestinationLink
            var decisionBranchDestinationLink = new DecisionBranchDestinationLink
            {
                DestinationId = destinationId,
                Label = null,
                Orderindex = orderIndex,
                SourceId = sourceId
            };

            if (DecisionBranchDestinationLinks == null)
            {
                DecisionBranchDestinationLinks = new List<DecisionBranchDestinationLink>();
            }
            DecisionBranchDestinationLinks.Add(decisionBranchDestinationLink);

            return decisionBranchDestinationLink;
        }

        public void ChangeBranchMergePoint(IProcessShape decisionPoint, double orderIndex, ProcessLink branchMergeLink, IProcessShape mergePoint)
        {
            ThrowIf.ArgumentNull(decisionPoint, nameof(decisionPoint));
            ThrowIf.ArgumentNull(branchMergeLink, nameof(branchMergeLink));
            ThrowIf.ArgumentNull(mergePoint, nameof(mergePoint));

            // Update destination id of the merge link with id of the next shape
            branchMergeLink.DestinationId = mergePoint.Id;

            // Find the DecisionBranchDestinationLink to update
            var targetDecisionBranchDestinationLink = DecisionBranchDestinationLinks.Find(dbd => dbd.Orderindex.Equals(orderIndex));

            // Update the destination id of the DecisionBranchDestinationLink with id of the next shape
            targetDecisionBranchDestinationLink.DestinationId = mergePoint.Id;
        }

        public List<IProcessShape> GetProcessShapesByShapeType(ProcessShapeType processShapeType)
        {
            string clientType = PropertyTypeName.ClientType.ToString();

            clientType = Shapes.Exists(shape => shape.PropertyValues.ContainsKey(clientType)) ? clientType : clientType.LowerCaseFirstCharacter();

            var shapesFound =
                Shapes.FindAll(
                    shape =>
                        Convert.ToInt32(shape.PropertyValues[clientType].Value, CultureInfo.InvariantCulture).
                        Equals((int)processShapeType));

            return shapesFound.ConvertAll(o => (IProcessShape) o);
        }

        public ProcessLink GetIncomingLinkForShape(IProcessShape processShape)
        {
            ThrowIf.ArgumentNull(processShape, nameof(processShape)); 

            // Find the incoming link for the process shape
            var link = Links.ToList().Find(l => l.DestinationId == processShape.Id);

            return link;
        }

        public List<ProcessLink> GetIncomingLinksForShape(IProcessShape processShape)
        {
            ThrowIf.ArgumentNull(processShape, nameof(processShape));

            // Find the incoming links for the process shape
            var links = Links.FindAll(l => l.DestinationId == processShape.Id);

            return links;
        }

        public ProcessLink GetOutgoingLinkForShape(IProcessShape processShape, double? orderIndex = null)
        {
            ThrowIf.ArgumentNull(processShape, nameof(processShape));

            // Find all outgoing links for shape
            var links = Links.FindAll(l => l.SourceId == processShape.Id);

            // Find the outgoing link for the process shape
            var processLink = orderIndex == null ? links.First() 
                : links.Find(link => link.Orderindex == orderIndex);

            return processLink;
        }

        public List<ProcessLink> GetOutgoingLinksForShape(IProcessShape processShape)
        {
            ThrowIf.ArgumentNull(processShape, nameof(processShape));

            // Find all outgoing links for the process shape
            var links = Links.FindAll(l => l.SourceId == processShape.Id);

            return links;
        }

        public IProcessShape GetNextShape(IProcessShape shape, double? orderIndex = null)
        {
            // Find the outgoing link for the proess shape
            var links = Links.FindAll(l => l.SourceId == shape.Id);

            var outgoingLink = orderIndex == null ? links.First() : links.Find(link => link.Orderindex == orderIndex);
            
            // Return the next shape which is located via the link destination Id
            return GetProcessShapeById(outgoingLink.DestinationId);
        }

        public DecisionBranchDestinationLink GetDecisionBranchDestinationLinkForDecisionShape(IProcessShape decisionShape,
            double orderIndex)
        {
            return DecisionBranchDestinationLinks.Find(
                dbd => dbd.SourceId.Equals(decisionShape.Id) && dbd.Orderindex.Equals(orderIndex));
        }

        public IProcessShape GetProcessShapeByShapeName(string shapeName)
        {
            // Find the process shape by the process shape name
            var shape = Shapes.ToList().Find(s => s.Name == shapeName);

            return shape;
        }

        public IProcessShape GetProcessShapeById(int shapeId)
        {
            // Find the process shape by the process shape artifact Id
            var shape = Shapes.ToList().Find(s => s.Id == shapeId);

            return shape;
        }

        public ProcessShapeType GetProcessShapeTypeById(int shapeId)
        {
            // Find the process shape by the process shape artifact Id
            var shape = Shapes.ToList().Find(s => s.Id == shapeId);

            // Get the property information value for the shape type
            var clientTypePropertyInformation =
                shape.PropertyValues.ToList()
                    .Find(p => string.Equals(p.Key, ClientType, StringComparison.CurrentCultureIgnoreCase))
                    .Value;

            // Get the integer representation of the process shape type
            var shapeType = Convert.ToInt32(clientTypePropertyInformation.Value, CultureInfo.InvariantCulture);

            // Return the process shape type
            return (ProcessShapeType)shapeType;
        }

        public void DeleteSystemDecisionWithBranchesNotOfTheLowestOrder(IProcessShape systemDecision, IProcessShape mergePointShape)
        {
            ThrowIf.ArgumentNull(systemDecision, nameof(systemDecision));
            ThrowIf.ArgumentNull(mergePointShape, nameof(mergePointShape));

            // Get system decision outgoing process link of the lowest order
            var outgoingSystemDecisionProcessLinkOfLowestOrder = GetOutgoingLinkForShape(systemDecision, DefaultOrderIndex);

            // Get the system task or system decision after the system decision on the lowest order branch.
            var systemTaskOnLowestOrderBranchOfSystemDecision = GetProcessShapeById(outgoingSystemDecisionProcessLinkOfLowestOrder.DestinationId);

            // Get the shapes to be deleted
            var shapesToDelete = GetShapesBetween(systemDecision, new List<IProcessShape> { mergePointShape }, ignoreLowestBranch: true);

            // Delete all shapes and outgoing links for the shapes
            DeleteShapesAndOutgoingLinks(shapesToDelete);

            // Delete the system decision and update the process link to have the system task or system shape on the lowest order branch
            DeleteSystemDecisionAndUpdateProcessLink(systemDecision, systemTaskOnLowestOrderBranchOfSystemDecision);

            // Delete all DecisionBranchDestinationLink associated with the root userDecision
            DeleteDecisionBranchDestinationLinksForDecision(systemDecision);

            // Delete all DecisionBranchDestinationLinks associated with decisions belongs to shapesToDelete
            var decisions = shapesToDelete.ToList().
                FindAll(
                s => s.IsTypeOf(ProcessShapeType.SystemDecision) || s.IsTypeOf(ProcessShapeType.UserDecision));
            foreach (var decision in decisions)
            {
                DeleteDecisionBranchDestinationLinksForDecision(decision);
            }
        }

        public void DeleteSystemDecisionBranch(IProcessShape systemDecision, double orderIndex,
    IProcessShape branchMergePointShape)
        {
            ThrowIf.ArgumentNull(systemDecision, nameof(systemDecision));
            ThrowIf.ArgumentNull(branchMergePointShape, nameof(branchMergePointShape));

            // Get system decision outgoing process link of the specified branch
            var outgoingSystemDecisionProcessLink = GetOutgoingLinkForShape(systemDecision, orderIndex);

            // Get the target process shape after the system decision point on the specified branch.
            var targetProcessShape = GetProcessShapeById(outgoingSystemDecisionProcessLink.DestinationId);

            // TODO Updates if there will be different implementation getting shapes based on differnt starting shape types
            // e.g. (Probably not necessary at all) Possible target process shape types:
            // 1) system task 2) system decision point 

            // Find the shapes to delete, including all branches before merge point
            List<ProcessShape> shapesToDelete = GetShapesBetween(targetProcessShape, new List<IProcessShape> { branchMergePointShape }).ToList();

            // Add the system task to the list of shapes to delete
            shapesToDelete.Add((ProcessShape)targetProcessShape);

            DeleteShapesAndOutgoingLinks(shapesToDelete);

            DeleteProcessLink(outgoingSystemDecisionProcessLink);

            // Delete a DecisionBranchDestinationLink associated with the deleted branch
            DeleteDecisionBranchDestinationLink(branchMergePointShape.Id, orderIndex, systemDecision.Id);

            // Delete all DecisionBranchDestinationLinks associated with decisions belongs to shapesToDelete
            var decisions = shapesToDelete.ToList().
                FindAll(
                s => s.IsTypeOf(ProcessShapeType.SystemDecision) || s.IsTypeOf(ProcessShapeType.UserDecision));
            foreach (var decision in decisions)
            {
                DeleteDecisionBranchDestinationLinksForDecision(decision);
            }
        }

        public void DeleteUserAndSystemTask(IProcessShape userTask)
        {
            var systemTask = GetNextShape(userTask);
            var shapeAfterSystemTask = GetNextShape(systemTask);

            // Get the shapes to be deleted
            var shapesToDelete = GetShapesBetween(userTask, new List<IProcessShape> { shapeAfterSystemTask });

            // Delete all shapes and outgoing links for the shapes
            DeleteShapesAndOutgoingLinks(shapesToDelete);

            DeleteUserTaskAndUpdateProcessLink(userTask, shapeAfterSystemTask);
        }

        public void DeleteUserAndSystemTaskWithAllBranches(IProcessShape userTask, IProcessShape mergePointShape)
        {
            // Get the shapes to be deleted
            var shapesToDelete = GetShapesBetween(userTask, new List<IProcessShape> { mergePointShape });

            // Delete all shapes and outgoing links for the shapes
            DeleteShapesAndOutgoingLinks(shapesToDelete);

            DeleteUserTaskAndUpdateProcessLink(userTask, mergePointShape);
        }

        public void DeleteUserDecisionWithBranchesNotOfTheLowestOrder(IProcessShape userDecision, IProcessShape mergePointShape)
        {
            ThrowIf.ArgumentNull(userDecision, nameof(userDecision));
            ThrowIf.ArgumentNull(mergePointShape, nameof(mergePointShape));

            // Get user decision outgoing process link of the lowest order
            var outgoingUserDecisionProcessLinkOfLowestOrder = GetOutgoingLinkForShape(userDecision, DefaultOrderIndex);

            // Get the user task after the decision point on the lowest order branch.
            var userTaskOnLowestOrderBranchOfUserDecision = GetProcessShapeById(outgoingUserDecisionProcessLinkOfLowestOrder.DestinationId);
            
            // Get the shapes to be deleted
            var shapesToDelete = GetShapesBetween(userDecision, new List<IProcessShape> { mergePointShape }, ignoreLowestBranch: true);

            // Delete all shapes and outgoing links for the shapes
            DeleteShapesAndOutgoingLinks(shapesToDelete);

            // Delete the user decision and update the process link to have the user task as the merge point
            DeleteUserDecisionAndUpdateProcessLink(userDecision, userTaskOnLowestOrderBranchOfUserDecision);

            // Delete all DecisionBranchDestinationLink associated with the root userDecision
            DeleteDecisionBranchDestinationLinksForDecision(userDecision);

            // Delete all DecisionBranchDestinationLinks associated with decisions belongs to shapesToDelete
            var decisions = shapesToDelete.ToList().
                FindAll(
                s => s.IsTypeOf(ProcessShapeType.SystemDecision) || s.IsTypeOf(ProcessShapeType.UserDecision));
            foreach (var decision in decisions)
            {
                DeleteDecisionBranchDestinationLinksForDecision(decision);
            }
        }

        public void DeleteUserDecisionBranch(IProcessShape userDecision, double orderIndex, IProcessShape branchMergePointShape)
        {
            ThrowIf.ArgumentNull(userDecision, nameof(userDecision));
            ThrowIf.ArgumentNull(branchMergePointShape, nameof(branchMergePointShape));

            // Get user decision outgoing process link of the specified branch
            var outgoingUserDecisionProcessLink = GetOutgoingLinkForShape(userDecision, orderIndex);

            // Get the user task after the decision point on the specified branch.
            var userTask = GetProcessShapeById(outgoingUserDecisionProcessLink.DestinationId);

            // Find the shapes to delete, including all branches before merge point
            var shapesToDelete = GetShapesBetween(userTask, new List<IProcessShape> { branchMergePointShape }).ToList();

            // Add the user task to the list of shapes to delete
            shapesToDelete.Add((ProcessShape)userTask);

            DeleteShapesAndOutgoingLinks(shapesToDelete);

            DeleteProcessLink(outgoingUserDecisionProcessLink);

            // Delete a DecisionBranchDestinationLink associated with the deleted branch
            DeleteDecisionBranchDestinationLink(branchMergePointShape.Id, orderIndex, userDecision.Id);
            
            // Delete all DecisionBranchDestinationLinks associated with decisions belongs to shapesToDelete
            var decisions = shapesToDelete.ToList().
                FindAll(
                s => s.IsTypeOf(ProcessShapeType.SystemDecision) || s.IsTypeOf(ProcessShapeType.UserDecision));
            foreach (var decision in decisions)
            {
                DeleteDecisionBranchDestinationLinksForDecision(decision);
            }
        }

        public void DeleteDecisionBranchDestinationLink(int destinationId, double orderIndex, int sourceId)
        {
            var decisionBranchDestinatilnLink = DecisionBranchDestinationLinks.Find(
                dbd =>
                    dbd.DestinationId.Equals(destinationId) && dbd.Orderindex.Equals(orderIndex)
                         && dbd.SourceId.Equals(sourceId));

            DecisionBranchDestinationLinks.Remove(decisionBranchDestinatilnLink);
        }

        public void DeleteDecisionBranchDestinationLinksForDecision(IProcessShape decision)
        {
            if (DecisionBranchDestinationLinks == null)
            {
                DecisionBranchDestinationLinks = new List<DecisionBranchDestinationLink>();
            }
            DecisionBranchDestinationLinks.RemoveAll(dbd => dbd.SourceId.Equals(decision.Id));
        }

        public void MoveUserAndSystemTaskBeforeShape(IProcessShape userTaskToMove, IProcessShape destinationShape)
        {
            ThrowIf.ArgumentNull(userTaskToMove, nameof(userTaskToMove));
            ThrowIf.ArgumentNull(destinationShape, nameof(destinationShape));

            IProcessShape systemTaskToMove = RemoveUserAndSystemTask(userTaskToMove);

            var nextShapeIncomingLinks = GetIncomingLinksForShape(destinationShape);

            // Update all incoming links to the destination shape so that the destination Id is the the
            // user task that was moved
            foreach (var links in nextShapeIncomingLinks)
            {
                links.DestinationId = userTaskToMove.Id;
            }

            var systemTaskOutgoingLink = GetOutgoingLinkForShape(systemTaskToMove);

            // Update the system task outgoing link destination Id to be the Id of the destination shape
            systemTaskOutgoingLink.DestinationId = destinationShape.Id;
        }

        public void MoveUserAndSystemTaskAfterShape(IProcessShape userTaskToMove, IProcessShape sourceShape)
        {
            ThrowIf.ArgumentNull(userTaskToMove, nameof(userTaskToMove));
            ThrowIf.ArgumentNull(sourceShape, nameof(sourceShape));

            IProcessShape systemTaskToMove = RemoveUserAndSystemTask(userTaskToMove);

            // Find source shape outgoing link
            var sourceShapeOutgoingLink = GetOutgoingLinkForShape(sourceShape);

            // Find shape after source shape before move
            var sourceShapeNextShapeBeforeMove = GetNextShape(sourceShape);

            // Update destination to userTask Id
            sourceShapeOutgoingLink.DestinationId = userTaskToMove.Id;

            var systemTaskOutgoingLink = GetOutgoingLinkForShape(systemTaskToMove);

            // Update the system task outgoing link destination Id to be the Id of the original
            // previous shape destination Id
            systemTaskOutgoingLink.DestinationId = sourceShapeNextShapeBeforeMove.Id;
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Create a User Task
        /// </summary>
        /// <param name="persona">The persona of the user task</param>
        /// <param name="itemLabel">The item label of the user task</param>
        /// <param name="associatedArtifact">The include of the user task</param>
        /// <param name="imageId">The id of the image in the user task</param>
        /// <param name="width">The width of the user task</param>
        /// <param name="height">The height of the user task</param>
        /// <param name="x">The x coordinate of the user task</param>
        /// <param name="y">The y coordinate of the user task</param>
        /// <param name="storyLinkId">The id of the linked user story</param>
        /// <returns>The new user task</returns>
        private IProcessShape CreateUserTask(
            string persona, 
            string itemLabel, 
            AssociatedArtifact associatedArtifact, 
            int? imageId, 
            double width, 
            double height, 
            int x, 
            int y, 
            int storyLinkId = 0)
        {
            // Create a user task
            var userTask = CreateProcessShape(ProcessShapeType.UserTask, UserTaskNamePrefix, itemLabel, associatedArtifact, width, height, x, y);

            // Create a story link for the user task if the story link Id was not 0
            var storyLink = storyLinkId == 0 ? null : new StoryLink(userTask.Id, storyLinkId, 0, storyLinkId);

            userTask.PropertyValues.Add(Persona,
                new PropertyValueInformation
                {
                    PropertyName = Persona,
                    TypePredefined = PropertyTypePredefined.Persona,
                    TypeId = GetPropertyNameTypeId(Persona),
                    Value = persona
                }
                );

            userTask.PropertyValues.Add(ImageId,
                new PropertyValueInformation
                {
                    PropertyName = ImageId,
                    TypePredefined = PropertyTypePredefined.ImageId,
                    TypeId = GetPropertyNameTypeId(ImageId),
                    Value = imageId
                }
                );

            userTask.PropertyValues.Add(StoryLinks,
                new PropertyValueInformation
                {
                    PropertyName = StoryLinks,
                    TypePredefined = PropertyTypePredefined.StoryLink,
                    TypeId = GetPropertyNameTypeId(StoryLinks),
                    Value = storyLink
                }
                );

            return userTask;
        }

        /// <summary>
        /// Create a System Task
        /// </summary>
        /// <param name="associatedImageUrl">The url of the system task image</param>
        /// <param name="persona">The persona of the user task</param>
        /// <param name="itemLabel">The item label of the user task</param>
        /// <param name="associatedArtifact">The include of the user task</param>
        /// <param name="imageId">The id of the image in the system task</param>
        /// <param name="width">The width of the user task</param>
        /// <param name="height">The height of the user task</param>
        /// <param name="x">The x coordinate of the user task</param>
        /// <param name="y">The y coordinate of the user task</param>
        /// <param name="storyLinkId">The id of the linked user story</param>
        /// <returns>The new system task</returns>
        private IProcessShape CreateSystemTask(
            string associatedImageUrl, 
            string persona, 
            string itemLabel, 
            AssociatedArtifact associatedArtifact, 
            int? imageId, 
            double width, 
            double height, 
            int x, 
            int y, 
            int storyLinkId = 0)
        {
            // Create a system task
            var systemTask = CreateProcessShape(ProcessShapeType.SystemTask, SystemTaskNamePrefix, itemLabel, associatedArtifact, width, height, x, y);

            // Create a story link for the system task if the story link Id was not 0
            var storyLink = storyLinkId == 0 ? null : new StoryLink(systemTask.Id, storyLinkId, 0, storyLinkId);

            systemTask.PropertyValues.Add(AssociatedImageUrl,
                new PropertyValueInformation
                {
                    PropertyName = AssociatedImageUrl,
                    TypePredefined = PropertyTypePredefined.None,
                    TypeId = GetPropertyNameTypeId(AssociatedImageUrl),
                    Value = associatedImageUrl
                });


            systemTask.PropertyValues.Add(Persona,
                new PropertyValueInformation
                {
                    PropertyName = Persona,
                    TypePredefined = PropertyTypePredefined.Persona,
                    TypeId = GetPropertyNameTypeId(Persona),
                    Value = persona
                }
                );

            systemTask.PropertyValues.Add(ImageId,
                new PropertyValueInformation
                {
                    PropertyName = ImageId,
                    TypePredefined = PropertyTypePredefined.ImageId,
                    TypeId = GetPropertyNameTypeId(ImageId),
                    Value = imageId
                }
                );

            systemTask.PropertyValues.Add(StoryLinks,
                new PropertyValueInformation
                {
                    PropertyName = StoryLinks,
                    TypePredefined = PropertyTypePredefined.StoryLink,
                    TypeId = GetPropertyNameTypeId(StoryLinks),
                    Value = storyLink
                }
                );

            return systemTask;
        }

        /// <summary>
        /// Create a User Decision Point
        /// </summary>
        /// <param name="itemLabel">The item label of the user decision point</param>
        /// <param name="associatedArtifact">The include of the user decision point</param>
        /// <param name="width">The width of the user decision point</param>
        /// <param name="height">The height of the user decision point</param>
        /// <param name="x">The x coordinate of the user decision pointtask</param>
        /// <param name="y">The y coordinate of the user decision point</param>
        /// <returns>The new user decision point</returns>
        private IProcessShape CreateUserDecisionPoint(
            string itemLabel, 
            AssociatedArtifact associatedArtifact, 
            double width, 
            double height, 
            int x, 
            int y)
        {
            // Create a user decision point
            var userDecisionPoint = CreateProcessShape(ProcessShapeType.UserDecision, UserDecisionNamePrefix, itemLabel, associatedArtifact, width, height, x, y);

            return userDecisionPoint;
        }

        /// <summary>
        /// Create a System Decision Point
        /// </summary>
        /// <param name="itemLabel">The item label of the system decision point</param>
        /// <param name="associatedArtifact">The include of the system decision point</param>
        /// <param name="width">The width of the system decision point</param>
        /// <param name="height">The height of the system decision point</param>
        /// <param name="x">The x coordinate of the system decision pointtask</param>
        /// <param name="y">The y coordinate of the system decision point</param>
        /// <returns>The new system decision point</returns>
        private IProcessShape CreateSystemDecisionPoint(
            string itemLabel, 
            AssociatedArtifact associatedArtifact, 
            double width, 
            double height, 
            int x, 
            int y)
        {
            // Create a system decision point
            var systemDecisionPoint = CreateProcessShape(ProcessShapeType.SystemDecision, SystemDecisionNamePrefix, itemLabel, associatedArtifact, width, height, x, y);

            return systemDecisionPoint;
        }

        /// <summary>
        /// Create a Generic Process Shape
        /// </summary>
        /// <param name="processShapeType">The type of the process shape</param>
        /// <param name="shapeNamePrefix">The prefix for both the shape name and the shape label</param>
        /// <param name="itemLabel">The item label of the process shape</param>
        /// <param name="associatedArtifact">The user story artifact associated with the Process shape</param>
        /// <param name="width">The width of the process shape</param>
        /// <param name="height">The height of the process shape</param>
        /// <param name="x">The x coordinate of the process shape</param>
        /// <param name="y">The y coordinate of the process shape</param>
        /// <returns></returns>
        private IProcessShape CreateProcessShape(
            ProcessShapeType processShapeType,
            string shapeNamePrefix,
            string itemLabel,
            AssociatedArtifact associatedArtifact,
            double width,
            double height,
            int x,
            int y)
        {
            IProcessShape processShape = new ProcessShape();

            processShape.BaseItemTypePredefined = ItemTypePredefined.PROShape;
            // New process shapes require a unique negative ID before being sent to the backend
            // by the Storyteller REST API method 'UpdateProcess'
            processShape.Id = --_tempId;
            processShape.Name = shapeNamePrefix + Math.Abs(processShape.Id);
            processShape.ParentId = Id;
            processShape.ProjectId = ProjectId;
            processShape.TypePrefix = ProcessShapeTypePrefix;
            processShape.AssociatedArtifact = associatedArtifact;

            processShape.PropertyValues.Add(ClientType,
                new PropertyValueInformation
                {
                    PropertyName = ClientType,
                    TypePredefined = PropertyTypePredefined.ClientType,
                    TypeId = GetPropertyNameTypeId(ClientType),
                    Value = (int)processShapeType
                });

            processShape.PropertyValues.Add(Description,
                new PropertyValueInformation
                {
                    PropertyName = Description,
                    TypePredefined = PropertyTypePredefined.Description,
                    TypeId = GetPropertyNameTypeId(Description),
                    // Create a random description
                    Value = AddDivTags(RandomGenerator.RandomValueWithPrefix(Description, 4))
                });

            processShape.PropertyValues.Add(Height,
                new PropertyValueInformation
                {
                    PropertyName = Height,
                    TypePredefined = PropertyTypePredefined.Height,
                    TypeId = GetPropertyNameTypeId(Height),
                    Value = height
                });

            // This is also known as Objective
            processShape.PropertyValues.Add(ItemLabel,
                new PropertyValueInformation
                {
                    PropertyName = ItemLabel,
                    TypePredefined = PropertyTypePredefined.ItemLabel,
                    TypeId = GetPropertyNameTypeId(ItemLabel),
                    Value = itemLabel + " for " + processShape.Name
                }
                );

            processShape.PropertyValues.Add(Label,
                new PropertyValueInformation
                {
                    PropertyName = Label,
                    TypePredefined = PropertyTypePredefined.Label,
                    TypeId = GetPropertyNameTypeId(Label),
                    Value = processShape.Name
                }
                );

            processShape.PropertyValues.Add(Width,
                new PropertyValueInformation
                {
                    PropertyName = Width,
                    TypePredefined = PropertyTypePredefined.Width,
                    TypeId = GetPropertyNameTypeId(Width),
                    Value = width
                }
                );

            processShape.PropertyValues.Add(X,
                new PropertyValueInformation
                {
                    PropertyName = X,
                    TypePredefined = PropertyTypePredefined.X,
                    TypeId = GetPropertyNameTypeId(X),
                    Value = x
                }
                );

            processShape.PropertyValues.Add(Y,
                new PropertyValueInformation
                {
                    PropertyName = Y,
                    TypePredefined = PropertyTypePredefined.Y,
                    TypeId = GetPropertyNameTypeId(Y),
                    Value = y
                }
                );

            return processShape;
        }

        /// <summary>
        /// Add a User Decision Point to the Process
        /// </summary>
        /// <param name="processLink">The process link where the user decision point will be added</param>
        /// <returns>The user decision point that was added</returns>
        private IProcessShape AddUserDecisionPoint(ProcessLink processLink)
        {
            /*
            If you start with this:
                --[??]--+--[UT]--

            It becomes this:
                --[??]--+--<UD>--+--[UT]--
            */

            ThrowIf.ArgumentNull(processLink, nameof(processLink));

            // Get the destination Id of the process link
            var destinationId = processLink.DestinationId;

            // Add a user decision point
            // Using non-default values to ensure values are saved
            var userDecisionPoint = CreateUserDecisionPoint("Objective", null, 120.0, 155.0, 10, 10);
            Shapes.Add((ProcessShape)userDecisionPoint);

            // Modify the destination id of the link preceding the insertion point of the new user decision so
            // that the destination now points to the new user decision
            // Note: Maintains existing order index
            processLink.DestinationId = userDecisionPoint.Id;

            // Add a new link after the new user decision point
            var newProcessLink = AddLink(sourceId: userDecisionPoint.Id, destinationId: destinationId, orderIndex: DefaultOrderIndex);

            // Add default link label
            newProcessLink.Label = I18NHelper.FormatInvariant("{0} {1}", DefaultDecisionLabelPrefix, (int) DefaultOrderIndex + 1);

            return userDecisionPoint;
        }

        /// <summary>
        /// Add a System Decision Point to the Process
        /// </summary>
        /// <param name="processLink">The process link where the system decision point will be added</param>
        /// <returns>The system decision point that was added</returns>
        private IProcessShape AddSystemDecisionPoint(ProcessLink processLink)
        {
            /*
            If you start with this:
                --[UT]--+--[ST]--

            It becomes this:
                --[UT]--+--<SD>--+--[ST]--
            */

            ThrowIf.ArgumentNull(processLink, nameof(processLink));

            // Get the destination Id of the process link
            var destinationId = processLink.DestinationId;

            // Add a system decision point
            // Using non-default values to ensure values are saved
            var systemDecisionPoint = CreateSystemDecisionPoint("Objective", null, 120.0, 155.0, 10, 10);
            Shapes.Add((ProcessShape)systemDecisionPoint);

            // Modify the destination id of the link preceding the insertion point of the new system decision so
            // that the destination now points to the new system decision
            // Note: Maintains existing order index
            processLink.DestinationId = systemDecisionPoint.Id;

            // Add a new link after the new system decision point
            var newProcessLink = AddLink(sourceId: systemDecisionPoint.Id, destinationId: destinationId, orderIndex: DefaultOrderIndex);

            // Add default link label
            newProcessLink.Label = I18NHelper.FormatInvariant("{0} {1}", DefaultDecisionLabelPrefix, (int) DefaultOrderIndex + 1);

            return systemDecisionPoint;
        }

        /// <summary>
        /// Add a System Task to the Process
        /// </summary>
        /// <param name="processLink">The process link where the system task will be added</param>
        /// <returns>The new System Task that was added</returns>
        private IProcessShape AddSystemTask(ProcessLink processLink)
        {
            /*
            If you start with this:
                --[??]--+--[??]--

            It becomes this:
                --[??]--+--[ST]--+--[??]--
            */

            ThrowIf.ArgumentNull(processLink, nameof(processLink));

            // Get the destination Id of the process link
            var destinationId = processLink.DestinationId;

            // Add a system task
            // Using non-default values to ensure values are saved
            var systemTask = CreateSystemTask(null, "NewSystem", "Objective", null, null, 120.0, 160.0, 5, 10);
            Shapes.Add((ProcessShape)systemTask);

            // Modify the destination id of the link preceding the insertion point of the new task so
            // that the destination now points to the new user task
            // Note: Maintains existing order index
            processLink.DestinationId = systemTask.Id;

            // Add a new link between the new system task and the destination
            AddLink(sourceId: systemTask.Id, destinationId: destinationId, orderIndex: DefaultOrderIndex);

            return systemTask;
        }

        /// <summary>
        /// Add a User Task to the Process
        /// </summary>
        /// <param name="processLink">The process link where the user task will be added</param>
        /// <returns>The new User Task that was added</returns>
        private IProcessShape AddUserTask(ProcessLink processLink)
        {
            /*
            If you start with this:
                --[??]--+--[??]--

            It becomes this:
                --[??]--+--[UT]--+--[??]--
            */

            ThrowIf.ArgumentNull(processLink, nameof(processLink));

            // Get the destination Id of the process link
            var destinationId = processLink.DestinationId;

            // Add a user task
            // Using non-default values to ensure values are saved
            var userTask = CreateUserTask("NewUser", "Objective", null, null, 120.0, 160.0, 5, 5);
            Shapes.Add((ProcessShape)userTask);

            // Modify the destination id of the link preceding the insertion point of the new task so
            // that the destination now points to the new user task
            // Note: Maintains existing order index
            processLink.DestinationId = userTask.Id;

            // Add a new link between the new user task and the destination
            AddLink(sourceId: userTask.Id, destinationId: destinationId, orderIndex: DefaultOrderIndex);

            return userTask;
        }

        /// <summary>
        /// Add a Branch with User/System Tasks to a User Decision Point
        /// </summary>
        /// <param name="orderIndexOfBranch">The vertical order index of the branch</param>
        /// <param name="idOfBranchMergePoint">The id of the shape where the branch merges</param>
        /// <param name="userDecisionPoint">The user decision that will receive the new branch</param>
        private void AddBranchToUserDecisionPoint(
            double orderIndexOfBranch,
            int? idOfBranchMergePoint,
            IProcessShape userDecisionPoint)
        {
            // Find outgoing process link for new user decision point
            var linkAferUserDecisionPoint = GetOutgoingLinkForShape(userDecisionPoint);

            // Find process shape immediately after the added user decision point
            var processShapeAfterUserDecisionPoint = GetProcessShapeTypeById(linkAferUserDecisionPoint.DestinationId);

            // Add user/system task immediately after user decision point only if next shape is the end shape
            // or another user decision
            if (processShapeAfterUserDecisionPoint == ProcessShapeType.End ||
                processShapeAfterUserDecisionPoint == ProcessShapeType.UserDecision)
            {
                /*  Special case:
                If next shape is (End):                         If next shape is <UD>:
                    --<UD>--+--(End)                                --<UD>--+--<UD>---

                It becomes this:                                It becomes this:
                    --+--<UD>--+--[UT]--+--[ST]--+--(End)           --+--<UD>--+--[UT]--+--[ST]--+--<UD>--
                          |                      |                        |                      |
                          +-------[UT]--+--[ST]--+                        +-------[UT]--+--[ST]--+
                */

                // Add new user/system task to branch
                AddUserAndSystemTask(linkAferUserDecisionPoint);

                // Find updated outgoing process link for user decision point
                linkAferUserDecisionPoint = GetOutgoingLinkForShape(userDecisionPoint);

                // Add default link label
                linkAferUserDecisionPoint.Label = I18NHelper.FormatInvariant("{0} {1}", DefaultDecisionLabelPrefix, (int) DefaultOrderIndex + 1);

                // Find process shape immediately after the added user decision from the udated link
                // after the added user decision point
                processShapeAfterUserDecisionPoint = GetProcessShapeTypeById(linkAferUserDecisionPoint.DestinationId);
            }

            // Get the branch merge point following the user/system task combination only if
            // the id of the branch merge point was not defined in the passed parameter and
            // the following process shape is a user task
            if (idOfBranchMergePoint == null && processShapeAfterUserDecisionPoint == ProcessShapeType.UserTask)
            {
                var userTaskShape = GetProcessShapeById(linkAferUserDecisionPoint.DestinationId);
                var systemTaskShape = GetNextShape(userTaskShape);
                var shapeAfterSystemTaskShape = GetNextShape(systemTaskShape);

                idOfBranchMergePoint = shapeAfterSystemTaskShape.Id;
            }

            Assert.NotNull(idOfBranchMergePoint, "The Id of the branch merge point is null.");

            // Add a branch with a user/system task to user decision point
            AddBranchWithUserAndSystemTaskToUserDecisionPoint(userDecisionPoint, orderIndexOfBranch,
                    (int)idOfBranchMergePoint);
        }

        /// <summary>
        /// get the Property Name Type Id
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        /// <returns>The type id of the property (returns null if no such property type was found)</returns>
        private int? GetPropertyNameTypeId(string propertyName)
        {
            // Must convert first character of property name to lowercase in order to find the property in the 
            // default process
            propertyName = propertyName.LowerCaseFirstCharacter();

            // Find the property with name propertyName
            var property = Shapes.Find(shape => shape.PropertyValues.ContainsKey(propertyName));

            // Return the property type Id if found, otherwise return null
            return property?.PropertyValues[propertyName].TypeId;
        }

        /// <summary>
        /// Add Div Tags to Text
        /// </summary>
        /// <param name="plainTextString">The plain text string to be modified</param>
        /// <returns>The plain text string surrounded by DIV tags</returns>
        private static string AddDivTags(string plainTextString)
        {
            string formatString = "<div>{0}</div>";

            return I18NHelper.FormatInvariant(formatString, plainTextString);
        }


        /// <summary>
        /// Delete Shapes and Ougoing Links from Shapes
        /// </summary>
        /// <param name="shapesToDelete">The shapes to delete</param>
        private void DeleteShapesAndOutgoingLinks(IEnumerable<IProcessShape> shapesToDelete)
        {
            foreach (var shape in shapesToDelete)
            {
                Shapes.RemoveAll(s => s.Id == shape.Id);
                Links.RemoveAll(l => l.SourceId == shape.Id);
            }
        }

        /// <summary>
        /// Find all the Outgoing Links from a Process Shape
        /// </summary>
        /// <param name="processShape">The process shape</param>
        /// <returns>The outgoing process links for the process shape</returns>
        private List<ProcessLink> GetNextLinks(IProcessShape processShape)
        {
            var nextLinks = Links.FindAll(l => l.SourceId == processShape.Id);

            return nextLinks;
        }

        /// <summary>
        /// Find all the Links Between a Source Shape and a List of Target Shapes
        /// </summary>
        /// <param name="sourceShape">The source shape</param>
        /// <param name="targetShapes">The list of target shapes where the branches for the source shape terminate</param>
        /// <param name="ignoreLowestBranch">(optional) Flag to ignore the branch with the lowest order index 
        /// when finding links.  Used when deleting decision points and lowest order index link should
        /// be preserved</param>
        /// <returns>The list of process links between the source shape and the target shapes</returns>
        private IEnumerable<ProcessLink> GetLinksBetween(IProcessShape sourceShape,List<IProcessShape> targetShapes, bool ignoreLowestBranch = false)
        {
            var nextLinks = GetNextLinks(sourceShape);

            // TODO: Add assert that this should only be done for Decision Points once we create specific process shape types
            if (ignoreLowestBranch)
            {
                nextLinks = nextLinks.OrderBy(l => l.Orderindex).ToList();
                nextLinks.Remove(nextLinks.First());
            }

            var additionalLinks = new List<ProcessLink>();

            foreach (var link in nextLinks)
            {
                // The link destination is not found in the target shapes,
                // then the next shape is retrieved and the links between it
                // and the target shapes are recursively added to the list
                // of links
                if (targetShapes.Find(s => s.Id == link.DestinationId) == null)
                {
                    var nextShape = GetProcessShapeById(link.DestinationId);
                    var linksBetweenShapes = GetLinksBetween(nextShape, targetShapes);
                    additionalLinks.AddRange(linksBetweenShapes);
                }
            }

            nextLinks.AddRange(additionalLinks);

            return nextLinks;
        }

        /// <summary>
        /// Find all the Shapes Between a Source Shape and a List of Target Shapes
        /// </summary>
        /// <param name="sourceShape">The source shape</param>
        /// <param name="targetShapes">The list of target shapes where the branches for the source shape terminate</param>
        /// <param name="ignoreLowestBranch">(optional) Flag to ignore the branch with the lowest order index
        /// when finding links.  Used when deleting decision points and lowest order index link should
        /// be preserved</param>
        /// <returns>The list of process shapes between the source shape and the target shapes</returns>
        private IEnumerable<ProcessShape> GetShapesBetween(IProcessShape sourceShape, List<IProcessShape> targetShapes, bool ignoreLowestBranch = false) 
        {
            var links = GetLinksBetween(sourceShape, targetShapes, ignoreLowestBranch);

            var processShapes = new List<ProcessShape>();

            foreach (var link in links)
            {
                if (targetShapes.Find(s => s.Id == link.DestinationId) == null)
                {
                    var shape = GetProcessShapeById(link.DestinationId);
                    processShapes.Add((ProcessShape) shape);
                }
            }

            return processShapes;
        }


        /// <summary>
        /// Delete a Single System Decision and Update the Process Link
        /// </summary>
        /// <param name="systemDecision">The system decision to be deleted</param>
        /// <param name="nextShape">The shape that immediately follows the deleted system decision</param>
        private void DeleteSystemDecisionAndUpdateProcessLink(IProcessShape systemDecision, IProcessShape nextShape)
        {
            DeleteProcessShapeAndUpdateProcessLink(systemDecision, nextShape);
        }

        /// <summary>
        /// Delete a Single User Task and Update the Process Link
        /// </summary>
        /// <param name="userTask">The user task to be deleted</param>
        /// <param name="nextShape">The shape that immediately follows the deleted user task</param>
        private void DeleteUserTaskAndUpdateProcessLink(IProcessShape userTask, IProcessShape nextShape)
        {
            DeleteProcessShapeAndUpdateProcessLink(userTask, nextShape);
        }

        /// <summary>
        /// Delete a Single User Decision and Update the Process Link
        /// </summary>
        /// <param name="userDecision">The user decision to be deleted</param>
        /// <param name="nextShape">The shape that immediately follows the deleted user decision</param>
        private void DeleteUserDecisionAndUpdateProcessLink(IProcessShape userDecision, IProcessShape nextShape)
        {
            DeleteProcessShapeAndUpdateProcessLink(userDecision, nextShape);
        }

        /// <summary>
        /// Delete a Single Process Shape and Update the Process Link
        /// </summary>
        /// <param name="processShapeToDelete">The process shape to be deleted</param>
        /// <param name="nextShape">The shape that immediately follows the deleted process shape</param>
        private void DeleteProcessShapeAndUpdateProcessLink(IProcessShape processShapeToDelete, IProcessShape nextShape)
        {
            var processShapeIncomingProcessLink = GetIncomingLinkForShape(processShapeToDelete);

            // Remove the process shape from the list of process shapes
            DeleteShapesAndOutgoingLinks(new List<IProcessShape> { processShapeToDelete });

            // Set destination id for the user task incoming link to the id of the next shape
            processShapeIncomingProcessLink.DestinationId = nextShape.Id;
        }

        /// <summary>
        /// Delete a Process Link
        /// </summary>
        /// <param name="processLink">The process link to be deleted</param>
        private void DeleteProcessLink(ProcessLink processLink)
        {
            Links.Remove(processLink);
        }

        /// <summary>
        /// Remove a User and System Task
        /// </summary>
        /// <param name="userTaskToRemove">The user task to remove</param>
        /// <returns>The system task process shape that was removed</returns>
        private IProcessShape RemoveUserAndSystemTask(IProcessShape userTaskToRemove)
        {
            ThrowIf.ArgumentNull(userTaskToRemove, nameof(userTaskToRemove));

            // Find all user task incoming links
            var userTaskIncomingLinks = GetIncomingLinksForShape(userTaskToRemove);

            var systemTaskToRemove = GetNextShape(userTaskToRemove);

            // Find the new destination shape for all incoming links to the user task before the remove
            var nextTaskAfterSystemTaskToRemove = GetNextShape(systemTaskToRemove);

            // Update all incoming links to user Task to so destination Id is to the shape after userTask
            // before the remove
            foreach (var links in userTaskIncomingLinks)
            {
                links.DestinationId = nextTaskAfterSystemTaskToRemove.Id;
            }

            return systemTaskToRemove;
        }

        #endregion Private Methods
    }

    public class ProcessShape: IProcessShape
    {
        private const string StorytellerProcessPrefix = "SP";

        public int Id { get; set; }

        public string Name { get; set; }

        public int ParentId { get; set; }

        public int ProjectId { get; set; }

        public string TypePrefix { get; set; }

        public AssociatedArtifact AssociatedArtifact { get; set; }

        public ItemTypePredefined BaseItemTypePredefined { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonConverter(typeof(Deserialization.ConcreteDictionaryConverter<Dictionary<string, PropertyValueInformation>, PropertyValueInformation>))]
        public Dictionary<string, PropertyValueInformation> PropertyValues { get; set; }

        public ProcessShape()
        {
            PropertyValues = new Dictionary<string, PropertyValueInformation>();
        }

        public AssociatedArtifact AddAssociatedArtifact(IArtifact artifact)
        {
            ThrowIf.ArgumentNull(artifact, nameof(artifact));

            AssociatedArtifact = new AssociatedArtifact()
            {
                Id = artifact.Id,
                Link = null,
                Name = artifact.Name,
                ProjectId = artifact.ProjectId,
                TypePrefix = StorytellerProcessPrefix
            };

            return AssociatedArtifact;
        }

        public bool IsTypeOf(ProcessShapeType processShapeType)
        {
            string clientType = PropertyTypeName.ClientType.ToString();

            clientType = PropertyValues.ContainsKey(clientType) ? clientType : clientType.LowerCaseFirstCharacter();

            return
                Convert.ToInt32(PropertyValues[clientType].Value, CultureInfo.InvariantCulture).Equals(
                    (int) processShapeType);
        }
    }

    public class ProcessLink : IProcessLink
    {

        /// <summary>
        /// Source Id for the process link
        /// </summary>
        public int SourceId { get; set; }

        /// <summary>		
        /// Destination Id for the process link
        /// </summary>
        public int DestinationId { get; set; }

        /// <summary>		
        /// Order index for the process link (Order in which the links are drawn for decision points)
        /// </summary>
        public double Orderindex { get; set; }

        /// <summary>		
        /// Label for the process link
        /// </summary>
        public string Label { get; set; }
    }

    public class DecisionBranchDestinationLink : IProcessLink
    {
        /*
        /// This class is also used to represents DecisionBranchDestinationLink
        /// The Id of the shape after the merge point
        /// (e.g. endShape ID in the picture)	
        [S]--[P]--+--<UD1>--+--[UT1]--+--[ST2]--+--[E]
                       |                        |
                       +-------[UT3]--+--[ST4]--+
        */

        /// <summary>
        /// The Id of source decision process shape (e.g. UD1)
        /// </summary>
        public int SourceId { get; set; }

        /// <summary>		
        /// The Id of the shape after the merge point
        /// (e.g. endShape ID in the picture)
        /// </summary>
        public int DestinationId { get; set; }

        /// <summary>		
        /// Order index for the process link (Order in which the links are drawn for decision points)
        /// (e.g. orderindex value of the first branch of UD1)
        /// </summary>
        public double Orderindex { get; set; }

        /// <summary>		
        /// Label for the DecisionBranchDestinationLink
        /// </summary>
        public string Label { get; set; }

    }

    public class ProcessStatus
    {
        public ProcessStatus (
            bool isLocked, 
            bool isLockedByMe, 
            bool isDeleted, 
            bool isReadOnly, 
            bool isUnpublished, 
            bool hasEverBeenPublished)
        {
            IsLocked = isLocked;
            IsLockedByMe = isLockedByMe;
            IsDeleted = isDeleted;
            IsReadOnly = isReadOnly;
            IsUnpublished = isUnpublished;
            HasEverBeenPublished = hasEverBeenPublished;
        }

        /// <summary>
        /// Check if the process is locked
        /// </summary>
        public bool IsLocked { get; set; }

        /// <summary>
        /// Check if the process is locked by the user who retrieve the process model
        /// </summary>
        public bool IsLockedByMe { get; set; }

        /// <summary>
        /// Check if the process is deleted by the user who retrieve the process model
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Check if the process is read-only
        /// </summary>
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// Check if the process has saved changes which are not published yet
        /// </summary>
        public bool IsUnpublished { get; set; }

        /// <summary>
        /// Check if the process is ever been published
        /// </summary>
        public bool HasEverBeenPublished { get; set; }

        //TODO: process model contains the property but not clear if it is being used to verify the status of the process yet
        public bool HasReadOnlyReuse { get; set; }

        //TODO: process model contains the property but not clear if it is being used to verify the status of the process yet
        public bool HasReuse { get; set; }

        //TODO: process model contains the property but not clear if it is being used to verify the status of the process yet
        public int LockOwnerId { get; set; }

        //TODO: process model contains the property but not clear if it is being used to verify the status of the process yet
        public int RevisionId { get; set; }

        //TODO: process model contains the property but not clear if it is being used to verify the status of the process yet
        public int UserId { get; set; }

        //TODO: process model contains the property but not clear if it is being used to verify the status of the process yet
        public int VersionId { get; set; } 
    }

    public class VersionInfo
    {
        public VersionInfo(
            int? artifactId,
            DateTime? utcLockedDateTime,
            string lockOwnerLogin,
            int? projectId,
            int? versionId,
            int? revisionId,
            int? baselineId)
        {
            ArtifactId = artifactId;
            UtcLockedDateTime = utcLockedDateTime;
            LockOwnerLogin = lockOwnerLogin;
            ProjectId = projectId;
            VersionId = versionId;
            RevisionId = revisionId;
            BaselineId = baselineId;
        }

        /// <summary>
        /// Artifact Id of the Process artifact containing the process model
        /// </summary>
        public int? ArtifactId { get; set; }

        /// <summary>
        /// UTC Date/Time when the artifact was locked
        /// </summary>
        public DateTime? UtcLockedDateTime { get; set; }

        /// <summary>
        /// Login user that has a lock on the artifact
        /// </summary>
        public string LockOwnerLogin { get; set; }

        /// <summary>
        /// Project ID for the project containing the artifact
        /// </summary>
        public int? ProjectId { get; set; }

        /// <summary>
        /// Version Id of the artifact
        /// </summary>
        public int? VersionId { get; set; }

        /// <summary>
        /// Revision Id of the artifact
        /// </summary>
        public int? RevisionId { get; set; }

        /// <summary>
        /// Baseline Id of the baseline containing the artifact (if it exists)
        /// </summary>
        public int? BaselineId { get; set; }

        /// <summary>
        /// Flag indicating if full version information is provided
        /// </summary>
        public bool IsVersionInformationProvided { get; set; }

        /// <summary>
        /// Flag indicating if the artifact is at Head or Saved Draft version
        /// </summary>
        public bool IsHeadOrSavedDraftVersion { get; set; }
    }

    public class AssociatedArtifact : ArtifactReference
    {
    }

    public class PropertyValueInformation
    {
        /// <summary>
        /// The name of the property
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// The predefined property type
        /// </summary>
        public PropertyTypePredefined TypePredefined { get; set; }

        /// <summary>
        /// Property Type Id as defined in the blueprint project metadata
        /// </summary>
        public int? TypeId { get; set; }

        /// <summary>
        /// The value of the property
        /// </summary>
        public object Value { get; set; }
    }

    public class StoryLink
    {
        /// <summary>
        /// The Artifact Id of referenced User Story
        /// </summary>
        public int AssociatedReferenceArtifactId { get; set; }

        /// <summary>
        /// The Destination Id of the Story Link
        /// </summary>
        public int DestinationId { get; set; }

        /// <summary>
        /// The vertical order index
        /// </summary>
        public double Orderindex { get; set; }

        /// <summary>
        /// The Source Id of the Story Link
        /// </summary>
        public int SourceId { get; set; }

        /// <summary>
        /// Storylink Constructor
        /// </summary>
        /// <param name="sourceId">The source id of the story link</param>
        /// <param name="destinationId">The destination id of the story link</param>
        /// <param name="orderIndex">The vertical order index</param>
        /// <param name="associatedReferenceId">The artifact id of referenced user story</param>
        public StoryLink(int sourceId, int destinationId, double orderIndex, int associatedReferenceId)
        {
            AssociatedReferenceArtifactId = associatedReferenceId;
            DestinationId = destinationId;
            Orderindex = orderIndex;
            SourceId = sourceId;
        }
    }
}
