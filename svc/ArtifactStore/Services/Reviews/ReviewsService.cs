﻿using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using ArtifactStore.Helpers;
using ArtifactStore.Models.Review;
using ArtifactStore.Repositories;
using ArtifactStore.Services.Reviews.MeaningOfSignature;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;

namespace ArtifactStore.Services.Reviews
{
    public class ReviewsService : IReviewsService
    {
        private readonly IReviewsRepository _reviewsRepository;
        private readonly IArtifactRepository _artifactRepository;
        private readonly IArtifactPermissionsRepository _permissionsRepository;
        private readonly IArtifactVersionsRepository _artifactVersionsRepository;
        private readonly ILockArtifactsRepository _lockArtifactsRepository;
        private readonly IItemInfoRepository _itemInfoRepository;

        public ReviewsService() : this(
            new SqlReviewsRepository(),
            new SqlArtifactRepository(),
            new SqlArtifactPermissionsRepository(),
            new SqlArtifactVersionsRepository(),
            new SqlLockArtifactsRepository(),
            new SqlItemInfoRepository())
        {
        }

        public ReviewsService(
            IReviewsRepository reviewsRepository,
            IArtifactRepository artifactRepository,
            IArtifactPermissionsRepository permissionsRepository,
            IArtifactVersionsRepository artifactVersionsRepository,
            ILockArtifactsRepository lockArtifactsRepository,
            IItemInfoRepository itemInfoRepository)
        {
            _reviewsRepository = reviewsRepository;
            _artifactRepository = artifactRepository;
            _permissionsRepository = permissionsRepository;
            _artifactVersionsRepository = artifactVersionsRepository;
            _lockArtifactsRepository = lockArtifactsRepository;
            _itemInfoRepository = itemInfoRepository;
        }

        public async Task<ReviewSettings> GetReviewSettingsAsync(int reviewId, int userId, int? versionId = null)
        {
            var revisionId = await _itemInfoRepository.GetRevisionId(reviewId, userId, versionId);
            var reviewInfo = await GetReviewInfoAsync(reviewId, userId, revisionId);

            if (!await _permissionsRepository.HasReadPermissions(reviewId, userId, revisionId: revisionId))
            {
                throw ReviewsExceptionHelper.UserCannotAccessReviewException(reviewId);
            }

            var reviewRawData = await _reviewsRepository.GetReviewPackageRawDataAsync(reviewId, userId, revisionId);
            var reviewSettings = new ReviewSettings(reviewRawData);

            var reviewType = await _reviewsRepository.GetReviewTypeAsync(reviewId, userId, revisionId);

            reviewSettings.CanEditRequireESignature = reviewRawData.Status == ReviewPackageStatus.Draft
                || (reviewRawData.Status == ReviewPackageStatus.Active && reviewType != ReviewType.Formal);

            var projectPermissions = await _permissionsRepository.GetProjectPermissions(reviewInfo.ProjectId);

            reviewSettings.CanEditRequireMeaningOfSignature = reviewSettings.CanEditRequireESignature
                && projectPermissions.HasFlag(ProjectPermissions.IsMeaningOfSignatureEnabled);

            return reviewSettings;
        }

        public async Task UpdateReviewSettingsAsync(int reviewId, ReviewSettings updatedReviewSettings, int userId)
        {
            var reviewInfo = await GetReviewInfoAsync(reviewId, userId);

            if (!await _permissionsRepository.HasEditPermissions(reviewId, userId))
            {
                throw ReviewsExceptionHelper.UserCannotModifyReviewException(reviewId);
            }

            var reviewRawData = await _reviewsRepository.GetReviewPackageRawDataAsync(reviewId, userId);

            if (reviewRawData.Status == ReviewPackageStatus.Closed)
            {
                throw new ConflictException(I18NHelper.FormatInvariant(ErrorMessages.ReviewIsClosed, reviewId), ErrorCodes.ReviewClosed);
            }

            await LockReviewAsync(reviewId, userId, reviewInfo);

            UpdateEndDate(updatedReviewSettings, reviewRawData);
            UpdateShowOnlyDescription(updatedReviewSettings, reviewRawData);
            UpdateCanMarkAsComplete(reviewId, updatedReviewSettings, reviewRawData);

            var reviewType = await _reviewsRepository.GetReviewTypeAsync(reviewId, userId);

            UpdateRequireESignature(reviewType, updatedReviewSettings, reviewRawData);
            await UpdateRequireMeaningOfSignatureAsync(reviewInfo.ItemId, reviewInfo.ProjectId, reviewType, updatedReviewSettings, reviewRawData);

            await _reviewsRepository.UpdateReviewPackageRawDataAsync(reviewId, reviewRawData, userId);
        }

        private async Task LockReviewAsync(int reviewId, int userId, ArtifactBasicDetails reviewInfo)
        {
            if (reviewInfo.LockedByUserId.HasValue)
            {
                if (reviewInfo.LockedByUserId.Value != userId)
                {
                    throw ExceptionHelper.ArtifactNotLockedException(reviewId, userId);
                }

                return;
            }

            if (!await _lockArtifactsRepository.LockArtifactAsync(reviewId, userId))
            {
                throw ExceptionHelper.ArtifactNotLockedException(reviewId, userId);
            }
        }

        private static void UpdateEndDate(ReviewSettings updatedReviewSettings, ReviewPackageRawData reviewRawData)
        {
            reviewRawData.EndDate = updatedReviewSettings.EndDate;
        }

        private static void UpdateShowOnlyDescription(ReviewSettings updatedReviewSettings, ReviewPackageRawData reviewRawData)
        {
            reviewRawData.ShowOnlyDescription = updatedReviewSettings.ShowOnlyDescription;
        }

        private static void UpdateCanMarkAsComplete(int reviewId, ReviewSettings updatedReviewSettings, ReviewPackageRawData reviewRawData)
        {
            var settingChanged =
                reviewRawData.IsAllowToMarkReviewAsCompleteWhenAllArtifactsReviewed != updatedReviewSettings.CanMarkAsComplete;

            if (!settingChanged)
            {
                return;
            }

            if (reviewRawData.Status != ReviewPackageStatus.Draft)
            {
                var errorMessage = I18NHelper.FormatInvariant(ErrorMessages.ReviewIsNotDraft, reviewId);
                throw new ConflictException(errorMessage, ErrorCodes.Conflict);
            }

            reviewRawData.IsAllowToMarkReviewAsCompleteWhenAllArtifactsReviewed = updatedReviewSettings.CanMarkAsComplete;
        }

        private static void UpdateRequireESignature(ReviewType reviewType, ReviewSettings updatedReviewSettings, ReviewPackageRawData reviewRawData)
        {
            var settingChanged = (!reviewRawData.IsESignatureEnabled.HasValue && updatedReviewSettings.RequireESignature)
                || (reviewRawData.IsESignatureEnabled.HasValue && reviewRawData.IsESignatureEnabled.Value != updatedReviewSettings.RequireESignature);

            if (!settingChanged)
            {
                return;
            }

            if (reviewType == ReviewType.Formal && reviewRawData.Status == ReviewPackageStatus.Active)
            {
                throw ReviewsExceptionHelper.ReviewActiveFormalException();
            }

            reviewRawData.IsESignatureEnabled = updatedReviewSettings.RequireESignature;
        }

        private async Task UpdateRequireMeaningOfSignatureAsync(int reviewId, int projectId, ReviewType reviewType, ReviewSettings updatedReviewSettings, ReviewPackageRawData reviewRawData)
        {
            var settingChanged = reviewRawData.IsMoSEnabled != updatedReviewSettings.RequireMeaningOfSignature;

            if (!settingChanged)
            {
                return;
            }

            if (reviewType == ReviewType.Formal && reviewRawData.Status == ReviewPackageStatus.Active)
            {
                throw ReviewsExceptionHelper.ReviewActiveFormalException();
            }

            if (updatedReviewSettings.RequireMeaningOfSignature && (!reviewRawData.IsESignatureEnabled.HasValue || !reviewRawData.IsESignatureEnabled.Value))
            {
                throw ReviewsExceptionHelper.RequireESignatureDisabledException(reviewId);
            }

            var projectPermissions = await _permissionsRepository.GetProjectPermissions(projectId);
            if (!projectPermissions.HasFlag(ProjectPermissions.IsMeaningOfSignatureEnabled))
            {
                throw ReviewsExceptionHelper.MeaningOfSignatureIsDisabledInProjectException();
            }

            reviewRawData.IsMoSEnabled = updatedReviewSettings.RequireMeaningOfSignature;

            if (reviewRawData.IsMoSEnabled && reviewRawData.Reviewers != null)
            {
                var meaningOfSignatureParameters = reviewRawData.Reviewers
                    .Where(r => r.Permission == ReviewParticipantRole.Approver)
                    .Select(r => new MeaningOfSignatureParameter { ParticipantId = r.UserId });

                await UpdateMeaningOfSignaturesInternalAsync(reviewId, reviewRawData, meaningOfSignatureParameters, new MeaningOfSignatureUpdateSetDefaultsStrategy());
            }
        }

        private async Task<ArtifactBasicDetails> GetReviewInfoAsync(int reviewId, int userId, int revisionId = int.MaxValue)
        {
            var artifactInfo = await _artifactRepository.GetArtifactBasicDetails(reviewId, userId);
            if (artifactInfo == null)
            {
                throw ReviewsExceptionHelper.ReviewNotFoundException(reviewId, revisionId);
            }

            if (artifactInfo.PrimitiveItemTypePredefined != (int)ItemTypePredefined.ArtifactReviewPackage)
            {
                throw new BadRequestException(I18NHelper.FormatInvariant(ErrorMessages.ArtifactIsNotReview, reviewId), ErrorCodes.BadRequest);
            }

            return artifactInfo;
        }

        public async Task UpdateMeaningOfSignaturesAsync(int reviewId, IEnumerable<MeaningOfSignatureParameter> meaningOfSignatureParameters, int userId)
        {
            var reviewInfo = await GetReviewInfoAsync(reviewId, userId);

            if (!await _permissionsRepository.HasEditPermissions(reviewId, userId))
            {
                throw ReviewsExceptionHelper.UserCannotModifyReviewException(reviewId);
            }

            var reviewRawData = await _reviewsRepository.GetReviewPackageRawDataAsync(reviewId, userId);

            if (reviewRawData.Status == ReviewPackageStatus.Closed)
            {
                throw ReviewsExceptionHelper.ReviewClosedException();
            }

            if (!reviewRawData.IsMoSEnabled)
            {
                throw new ConflictException("Could not update review because meaning of signature is not enabled.", ErrorCodes.MeaningOfSignatureNotEnabled);
            }

            await LockReviewAsync(reviewId, userId, reviewInfo);

            await UpdateMeaningOfSignaturesInternalAsync(reviewId, reviewRawData, meaningOfSignatureParameters, new MeaningOfSignatureUpdateSpecificStrategy());

            await _reviewsRepository.UpdateReviewPackageRawDataAsync(reviewId, reviewRawData, userId);
        }

        private async Task UpdateMeaningOfSignaturesInternalAsync(int reviewId, ReviewPackageRawData reviewRawData,
            IEnumerable<MeaningOfSignatureParameter> meaningOfSignatureParameters, IMeaningOfSignatureUpdateStrategy updateStrategy)
        {
            var meaningOfSignatureParamList = meaningOfSignatureParameters.ToList();

            var participantIds = meaningOfSignatureParamList.Select(mos => mos.ParticipantId).ToList();

            var possibleMeaningOfSignatures = await _reviewsRepository.GetPossibleMeaningOfSignaturesForParticipantsAsync(participantIds);

            if (reviewRawData.Reviewers == null)
            {
                throw new BadRequestException("Could not update meaning of signature because participant is not in review.", ErrorCodes.UserNotInReview);
            }

            foreach (var participantId in participantIds)
            {
                var participant = reviewRawData.Reviewers.FirstOrDefault(r => r.UserId == participantId);

                if (participant == null)
                {
                    throw new BadRequestException("Could not update meaning of signature because participant is not in review.", ErrorCodes.UserNotInReview);
                }

                if (participant.Permission != ReviewParticipantRole.Approver)
                {
                    throw new BadRequestException("Could not update meaning of signature because participant is not an approver.", ErrorCodes.ParticipantIsNotAnApprover);
                }

                var meaningOfSignatureUpdates = updateStrategy.GetMeaningOfSignatureUpdates(participantId, possibleMeaningOfSignatures, meaningOfSignatureParamList);

                if (participant.SelectedRoleMoSAssignments == null)
                {
                    participant.SelectedRoleMoSAssignments = new List<ParticipantMeaningOfSignature>();
                }

                foreach (var meaningOfSignatureUpdate in meaningOfSignatureUpdates)
                {
                    var meaningOfSignature = meaningOfSignatureUpdate.MeaningOfSignature;

                    var participantMeaningOfSignature = participant.SelectedRoleMoSAssignments.FirstOrDefault(pmos => pmos.RoleAssignmentId == meaningOfSignature.RoleAssignmentId);

                    if (participantMeaningOfSignature == null)
                    {
                        if (!meaningOfSignatureUpdate.Adding)
                        {
                            continue;
                        }

                        participantMeaningOfSignature = new ParticipantMeaningOfSignature();

                        participant.SelectedRoleMoSAssignments.Add(participantMeaningOfSignature);
                    }
                    else if (!meaningOfSignatureUpdate.Adding)
                    {
                        participant.SelectedRoleMoSAssignments.Remove(participantMeaningOfSignature);

                        continue;
                    }

                    participantMeaningOfSignature.ParticipantId = participantId;
                    participantMeaningOfSignature.ReviewId = reviewId;
                    participantMeaningOfSignature.MeaningOfSignatureId = meaningOfSignature.MeaningOfSignatureId;
                    participantMeaningOfSignature.MeaningOfSignatureValue = meaningOfSignature.MeaningOfSignatureValue;
                    participantMeaningOfSignature.RoleId = meaningOfSignature.RoleId;
                    participantMeaningOfSignature.RoleName = meaningOfSignature.RoleName;
                    participantMeaningOfSignature.RoleAssignmentId = meaningOfSignature.RoleAssignmentId;
                    participantMeaningOfSignature.GroupId = meaningOfSignature.GroupId;
                }
            }
        }

        public async Task<ReviewChangeParticipantsStatusResult> AssignRoleToParticipantsAsync(int reviewId, AssignParticipantRoleParameter content, int userId)
        {
            if ((content.ItemIds == null || !content.ItemIds.Any()) && content.SelectionType == SelectionType.Selected)
            {
                throw new BadRequestException("Incorrect input parameters", ErrorCodes.OutOfRangeParameter);
            }
            var propertyResult = await _reviewsRepository.GetReviewApprovalRolesInfoAsync(reviewId, userId);

            if (propertyResult == null)
            {
                throw new BadRequestException("Cannot update approval role as project or review couldn't be found", ErrorCodes.ResourceNotFound);
            }

            if (propertyResult.IsUserDisabled.HasValue && propertyResult.IsUserDisabled.Value)
            {
                throw new ConflictException("User deleted or not active", ErrorCodes.UserDisabled);
            }

            if (propertyResult.IsReviewDeleted)
            {
                throw ReviewsExceptionHelper.ReviewNotFoundException(reviewId);
            }

            if (propertyResult.IsReviewReadOnly)
            {
                var errorMessage = "The approval status could not be updated because another user has changed the Review status.";
                throw new ConflictException(errorMessage, ErrorCodes.ApprovalRequiredIsReadonlyForReview);
            }

            if (propertyResult.LockedByUserId.GetValueOrDefault() != userId)
            {
                throw ExceptionHelper.ArtifactNotLockedException(reviewId, userId);
            }

            if (string.IsNullOrEmpty(propertyResult.ArtifactXml))
            {
                throw ExceptionHelper.ArtifactDoesNotSupportOperation(reviewId);
            }
            var resultErrors = new List<ReviewChangeItemsError>();

            var reviewRawData = UpdateParticipantRole(propertyResult.ArtifactXml, content, reviewId, resultErrors);

            if (reviewRawData.IsMoSEnabled && content.Role == ReviewParticipantRole.Approver)
            {
                var meaningOfSignatureParameter = content.ItemIds
                    .Select(i => new MeaningOfSignatureParameter { ParticipantId = i })
                    .ToArray();

                await UpdateMeaningOfSignaturesInternalAsync(reviewId, reviewRawData, meaningOfSignatureParameter, new MeaningOfSignatureUpdateSetDefaultsStrategy());
            }

            await _reviewsRepository.UpdateReviewPackageRawDataAsync(reviewId, reviewRawData, userId);

            var changeResult = new ReviewChangeParticipantsStatusResult();
            changeResult.ReviewType = await _reviewsRepository.GetReviewTypeAsync(reviewId, userId);

            if (content.Role == ReviewParticipantRole.Approver)
            {
                await EnableRequireESignatureWhenProjectESignatureEnabledByDefaultAsync(reviewId, userId, propertyResult.ProjectId.Value, reviewRawData);

                if (reviewRawData.IsMoSEnabled && content.ItemIds.Count() == 1 && content.SelectionType == SelectionType.Selected)
                {
                    changeResult.DropdownItems = reviewRawData.Reviewers
                        .First(r => r.UserId == content.ItemIds.FirstOrDefault())
                        .SelectedRoleMoSAssignments.Select(mos => new DropdownItem(mos.GetMeaningOfSignatureDisplayValue(), mos.RoleAssignmentId));
                }
            }

            if (resultErrors.Any())
            {
                changeResult.ReviewChangeItemErrors = resultErrors;
            }

            return changeResult;
        }

        private static ReviewPackageRawData UpdateParticipantRole(string reviewPackageXml, AssignParticipantRoleParameter content, int reviewId, List<ReviewChangeItemsError> resultErrors)
        {
            var reviewRawData = ReviewRawDataHelper.RestoreData<ReviewPackageRawData>(reviewPackageXml);
            int nonIntersecCount = 0;

            if (content.SelectionType == SelectionType.Selected)
            {
                foreach (var reviewer in reviewRawData.Reviewers)
                {
                    if (content.ItemIds.Contains(reviewer.UserId))
                    {
                        reviewer.Permission = content.Role;
                    }
                }

                nonIntersecCount = content.ItemIds.Count() - content.ItemIds.Intersect(reviewRawData.Reviewers.Select(r => r.UserId)).Count();
            }
            else
            {
                if (content.ItemIds != null && content.ItemIds.Any())
                {
                    foreach (var reviewer in reviewRawData.Reviewers)
                    {
                        if (!content.ItemIds.Contains(reviewer.UserId))
                        {
                            reviewer.Permission = content.Role;
                        }
                    }
                }
                else
                {
                    foreach (var reviewer in reviewRawData.Reviewers)
                    {
                        reviewer.Permission = content.Role;
                    }
                }
            }

            if (nonIntersecCount > 0)
            {
                resultErrors.Add(
                    new ReviewChangeItemsError
                    {
                        ItemsCount = nonIntersecCount,
                        ErrorCode = ErrorCodes.UserNotInReview,
                        ErrorMessage = "Some users are not in the review."

                    });
            }

            return reviewRawData;
        }

        public async Task<ReviewChangeItemsStatusResult> AssignApprovalRequiredToArtifactsAsync(int reviewId, AssignArtifactsApprovalParameter content, int userId)
        {
            if ((content.ItemIds == null || !content.ItemIds.Any()) && content.SelectionType == SelectionType.Selected)
            {
                throw new BadRequestException("Incorrect input parameters", ErrorCodes.OutOfRangeParameter);
            }

            var propertyResult = await _reviewsRepository.GetReviewPropertyStringAsync(reviewId, userId);

            if (propertyResult.IsReviewDeleted)
            {
                throw ReviewsExceptionHelper.ReviewNotFoundException(reviewId);
            }

            if (propertyResult.ReviewStatus == ReviewPackageStatus.Closed)
            {
                throw ReviewsExceptionHelper.ReviewClosedException();
            }

            if (propertyResult.LockedByUserId.GetValueOrDefault() != userId)
            {
                throw ExceptionHelper.ArtifactNotLockedException(reviewId, userId);
            }

            if (propertyResult.ProjectId == null || propertyResult.ProjectId < 1 || string.IsNullOrEmpty(propertyResult.ArtifactXml))
            {
                throw ExceptionHelper.ArtifactDoesNotSupportOperation(reviewId);
            }

            // If review is active and formal we throw conflict exception. No changes allowed
            if (propertyResult.ReviewStatus == ReviewPackageStatus.Active &&
                propertyResult.ReviewType == ReviewType.Formal)
            {
                throw ReviewsExceptionHelper.ReviewActiveFormalException();
            }

            var resultErrors = new List<ReviewChangeItemsError>();

            var rdReviewContents = ReviewRawDataHelper.RestoreData<RDReviewContents>(propertyResult.ArtifactXml);
            var updatingArtifacts = GetReviewArtifacts(content, resultErrors, rdReviewContents);

            if (updatingArtifacts.Any())
            {
                // For Informal review
                await ExcludeDeletedAndNotInProjectArtifacts(content, propertyResult, resultErrors, updatingArtifacts);
                await ExcludeArtifactsWithoutReadPermissions(content, userId, resultErrors, updatingArtifacts);

                foreach (var updatingArtifact in updatingArtifacts)
                {
                    updatingArtifact.ApprovalNotRequested = !content.ApprovalRequired;
                }

                var resultArtifactsXml = ReviewRawDataHelper.GetStoreData(rdReviewContents);

                await _reviewsRepository.UpdateReviewArtifactsAsync(reviewId, userId, resultArtifactsXml, null, false);

                if (content.ApprovalRequired)
                {
                    var reviewRawData = await _reviewsRepository.GetReviewPackageRawDataAsync(reviewId, userId);
                    await EnableRequireESignatureWhenProjectESignatureEnabledByDefaultAsync(reviewId, userId, propertyResult.ProjectId.Value, reviewRawData);
                }
            }

            var result = new ReviewChangeItemsStatusResult();
            result.ReviewType = await _reviewsRepository.GetReviewTypeAsync(reviewId, userId);

            if (resultErrors.Any())
            {
                result.ReviewChangeItemErrors = resultErrors;
            }

            return result;
        }

        private async Task ExcludeArtifactsWithoutReadPermissions(AssignArtifactsApprovalParameter content, int userId, ICollection<ReviewChangeItemsError> resultErrors, List<RDArtifact> updatingArtifacts)
        {
            var updatingArtifactIds = updatingArtifacts.Select(ua => ua.Id).ToList();
            var artifactPermissionsDictionary = await _permissionsRepository.GetArtifactPermissions(updatingArtifactIds, userId);
            var artifactsWithReadPermissions = artifactPermissionsDictionary
                .Where(p => p.Value.HasFlag(RolePermissions.Read))
                .Select(p => p.Key);

            var updatingArtifactIdsWithReadPermissions = artifactsWithReadPermissions.Intersect(updatingArtifactIds).ToList();
            var artifactsWithReadPermissionsCount = updatingArtifactIdsWithReadPermissions.Count;
            var updatingArtifactsCount = updatingArtifactIds.Count;

            // Only show error message if on client side user have an outdated data about deleted artifacts from review
            if (content.SelectionType == SelectionType.Selected && artifactsWithReadPermissionsCount != updatingArtifactsCount)
            {
                resultErrors.Add(
                    new ReviewChangeItemsError
                    {
                        ItemsCount = updatingArtifactsCount - artifactsWithReadPermissionsCount,
                        ErrorCode = ErrorCodes.UnauthorizedAccess,
                        ErrorMessage = "There is no read permissions for some artifacts."
                    });
            }

            // Remove deleted items from the result
            updatingArtifacts.RemoveAll(ua => !updatingArtifactIdsWithReadPermissions.Contains(ua.Id));
        }

        private async Task ExcludeDeletedAndNotInProjectArtifacts(AssignArtifactsApprovalParameter content, PropertyValueString propertyResult, ICollection<ReviewChangeItemsError> resultErrors, List<RDArtifact> updatingArtifacts)
        {
            if (propertyResult.BaselineId == null || propertyResult.BaselineId < 1)
            {
                var updatingArtifactIdsOnly = updatingArtifacts.Select(ua => ua.Id);
                var deletedAndNotInProjectItemIds = await _artifactVersionsRepository.GetDeletedAndNotInProjectItems(updatingArtifactIdsOnly, propertyResult.ProjectId.Value);

                // Only show error message if on client side user have an outdated data about deleted artifacts from review
                if (content.SelectionType == SelectionType.Selected && deletedAndNotInProjectItemIds != null && deletedAndNotInProjectItemIds.Any())
                {
                    resultErrors.Add(
                        new ReviewChangeItemsError
                        {
                            ItemsCount = deletedAndNotInProjectItemIds.Count(),
                            ErrorCode = ErrorCodes.ArtifactNotFound,
                            ErrorMessage = "Some artifacts are deleted from the project."
                        });
                }

                // Remove deleted items from the result
                updatingArtifacts.RemoveAll(ua => deletedAndNotInProjectItemIds.Contains(ua.Id));
            }
        }

        private static List<RDArtifact> GetReviewArtifacts(AssignArtifactsApprovalParameter content, ICollection<ReviewChangeItemsError> resultErrors, RDReviewContents rdReviewContents)
        {
            if (content.SelectionType != SelectionType.Selected)
            {
                return rdReviewContents.Artifacts
                    .Where(a => a.ApprovalNotRequested == content.ApprovalRequired && !content.ItemIds.Contains(a.Id))
                    .ToList();
            }

            var updatingArtifacts = rdReviewContents.Artifacts.Where(a => content.ItemIds.Contains(a.Id)).ToList();

            var foundArtifactsCount = updatingArtifacts.Count;
            var requestedArtifactsCount = content.ItemIds.Count();

            if (foundArtifactsCount != requestedArtifactsCount)
            {
                resultErrors.Add(
                    new ReviewChangeItemsError
                    {
                        ItemsCount = requestedArtifactsCount - foundArtifactsCount,
                        ErrorCode = ErrorCodes.ApprovalRequiredArtifactNotInReview,
                        ErrorMessage = "Some of the artifacts are not in the review."
                    });
            }

            updatingArtifacts = updatingArtifacts.Where(a => a.ApprovalNotRequested == content.ApprovalRequired).ToList();

            return updatingArtifacts;
        }

        private async Task EnableRequireESignatureWhenProjectESignatureEnabledByDefaultAsync(int reviewId, int userId, int projectId, ReviewPackageRawData reviewRawData)
        {
            if (reviewRawData.IsESignatureEnabled.HasValue)
            {
                return;
            }

            var reviewType = await _reviewsRepository.GetReviewTypeAsync(reviewId, userId);
            if (reviewType != ReviewType.Formal)
            {
                return;
            }

            var projectPermissions = await _permissionsRepository.GetProjectPermissions(projectId);
            if (projectPermissions.HasFlag(ProjectPermissions.IsReviewESignatureEnabled))
            {
                reviewRawData.IsESignatureEnabled = true;
                await _reviewsRepository.UpdateReviewPackageRawDataAsync(reviewId, reviewRawData, userId);
            }
        }
    }
}
