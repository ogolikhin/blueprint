using System.Threading.Tasks;
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

            var reviewData = await _reviewsRepository.GetReviewAsync(reviewId, userId, revisionId);
            var reviewSettings = new ReviewSettings(reviewData.ReviewPackageRawData);

            var reviewType = await _reviewsRepository.GetReviewTypeAsync(reviewId, userId, revisionId);

            // We never ignore folders for formal reviews - Jira Bug STOR-4636
            reviewSettings.IgnoreFolders = reviewData.ReviewType == ReviewType.Formal || reviewData.BaselineId.HasValue ? false : reviewSettings.IgnoreFolders;

            reviewSettings.CanEditRequireESignature = reviewData.ReviewStatus == ReviewPackageStatus.Draft
                || (reviewData.ReviewStatus == ReviewPackageStatus.Active && reviewData.ReviewType != ReviewType.Formal);

            var projectPermissions = await _permissionsRepository.GetProjectPermissions(reviewInfo.ProjectId);

            reviewSettings.IsMeaningOfSignatureEnabledInProject =
                projectPermissions.HasFlag(ProjectPermissions.IsMeaningOfSignatureEnabled);

            reviewSettings.CanEditRequireMeaningOfSignature = reviewSettings.CanEditRequireESignature
                && reviewSettings.IsMeaningOfSignatureEnabledInProject;

            return reviewSettings;
        }

        public async Task UpdateReviewSettingsAsync(int reviewId, ReviewSettings updatedReviewSettings, int userId)
        {
            var reviewInfo = await GetReviewInfoAsync(reviewId, userId);

            if (!await _permissionsRepository.HasEditPermissions(reviewId, userId))
            {
                throw ReviewsExceptionHelper.UserCannotModifyReviewException(reviewId);
            }

            var reviewData = await _reviewsRepository.GetReviewAsync(reviewId, userId);

            if (reviewData.ReviewStatus == ReviewPackageStatus.Closed)
            {
                throw new ConflictException(I18NHelper.FormatInvariant(ErrorMessages.ReviewIsClosed, reviewId), ErrorCodes.ReviewClosed);
            }

            await LockReviewAsync(reviewId, userId, reviewInfo);

            UpdateEndDate(updatedReviewSettings, reviewData.ReviewPackageRawData);
            UpdateShowOnlyDescription(updatedReviewSettings, reviewData.ReviewPackageRawData);
            UpdateCanMarkAsComplete(reviewId, updatedReviewSettings, reviewData.ReviewPackageRawData);

            var reviewType = await _reviewsRepository.GetReviewTypeAsync(reviewId, userId);

            UpdateRequireESignature(reviewType, updatedReviewSettings, reviewData.ReviewPackageRawData);
            await UpdateRequireMeaningOfSignatureAsync(reviewInfo.ItemId, userId, reviewInfo.ProjectId, reviewType, updatedReviewSettings, reviewData.ReviewPackageRawData);

            await _reviewsRepository.UpdateReviewPackageRawDataAsync(reviewId, reviewData.ReviewPackageRawData, userId);
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

        private async Task UpdateRequireMeaningOfSignatureAsync(int reviewId, int userId, int projectId, ReviewType reviewType, ReviewSettings updatedReviewSettings, ReviewPackageRawData reviewRawData)
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

                await UpdateMeaningOfSignaturesInternalAsync(reviewId, userId, reviewRawData, meaningOfSignatureParameters, new MeaningOfSignatureUpdateSetDefaultsStrategy());
            }
        }

        private async Task<ArtifactBasicDetails> GetReviewInfoAsync(int reviewId, int userId, int revisionId = int.MaxValue)
        {
            var artifactInfo = await _artifactRepository.GetArtifactBasicDetails(reviewId, userId);
            if (artifactInfo == null)
            {
                throw ReviewsExceptionHelper.ReviewNotFoundException(reviewId, revisionId);
            }

            if (revisionId == int.MaxValue && (artifactInfo.DraftDeleted || artifactInfo.LatestDeleted))
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

            var reviewData = await _reviewsRepository.GetReviewAsync(reviewId, userId);

            if (reviewData.ReviewStatus == ReviewPackageStatus.Closed)
            {
                throw ReviewsExceptionHelper.ReviewClosedException();
            }

            if (!reviewData.ReviewPackageRawData.IsMoSEnabled)
            {
                throw new ConflictException("Could not update review because meaning of signature is not enabled.", ErrorCodes.MeaningOfSignatureNotEnabled);
            }

            await LockReviewAsync(reviewId, userId, reviewInfo);

            await UpdateMeaningOfSignaturesInternalAsync(reviewId, userId, reviewData.ReviewPackageRawData, meaningOfSignatureParameters, new MeaningOfSignatureUpdateSpecificStrategy());

            await _reviewsRepository.UpdateReviewPackageRawDataAsync(reviewId, reviewData.ReviewPackageRawData, userId);
        }

        private async Task UpdateMeaningOfSignaturesInternalAsync(int reviewId, int userId, ReviewPackageRawData reviewRawData,
            IEnumerable<MeaningOfSignatureParameter> meaningOfSignatureParameters, IMeaningOfSignatureUpdateStrategy updateStrategy)
        {
            var meaningOfSignatureParamList = meaningOfSignatureParameters.ToList();

            var participantIds = meaningOfSignatureParamList.Select(mos => mos.ParticipantId).ToList();

            var possibleMeaningOfSignatures = await _reviewsRepository.GetPossibleMeaningOfSignaturesForParticipantsAsync(reviewId, userId, participantIds);

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

                    var participantMeaningOfSignature = participant.SelectedRoleMoSAssignments.FirstOrDefault(pmos => pmos.RoleId == meaningOfSignature.RoleId);

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

            if (!await _permissionsRepository.HasEditPermissions(reviewId, userId))
            {
                throw ReviewsExceptionHelper.UserCannotModifyReviewException(reviewId);
            }

            var reviewInfo = await GetReviewInfoAsync(reviewId, userId);
            if (reviewInfo.LockedByUserId.GetValueOrDefault() != userId)
            {
                throw ExceptionHelper.ArtifactNotLockedException(reviewId, userId);
            }

            var reviewData = await _reviewsRepository.GetReviewAsync(reviewId, userId);
            if (reviewData.ReviewStatus == ReviewPackageStatus.Closed)
            {
                const string errorMessage = "The approval status could not be updated because another user has changed the Review status.";
                throw new ConflictException(errorMessage, ErrorCodes.ApprovalRequiredIsReadonlyForReview);
            }

            if (reviewData.ReviewPackageRawData.Reviewers == null
                || !reviewData.ReviewPackageRawData.Reviewers.Any())
            {
                throw ExceptionHelper.ArtifactDoesNotSupportOperation(reviewId);
            }

            if (reviewData.ReviewStatus == ReviewPackageStatus.Active)
            {
                if (content.Role == ReviewParticipantRole.Approver)
                {
                    var hasApproversAlready = reviewData.ReviewPackageRawData.Reviewers.FirstOrDefault(
                        r => r.Permission == ReviewParticipantRole.Approver) != null;
                    // If we have approvers before current action, it means that review already was converted to formal
                    if (!hasApproversAlready)
                    {
                        var artifactRequredApproval =
                            reviewData.Contents.Artifacts?.FirstOrDefault(a => !a.ApprovalNotRequested ?? true);
                        if (artifactRequredApproval != null)
                        {
                            throw new ConflictException(
                                "Could not update review participants because review needs to be converted to Formal.",
                                ErrorCodes.ReviewNeedsToMoveBackToDraftState);
                        }
                    }
                }
                else // If new role is reviewer
                {
                    ReviewsExceptionHelper.VerifyNotLastApproverInFormalReview(content, reviewData);
                }
            }

            var resultErrors = new List<ReviewChangeItemsError>();

            UpdateParticipantRole(reviewData.ReviewPackageRawData, content, resultErrors);

            await UpdateMeaningOfSignatureWhenAssignApprovalRoles(reviewId, userId, content, reviewData.ReviewPackageRawData);

            await _reviewsRepository.UpdateReviewPackageRawDataAsync(reviewId, reviewData.ReviewPackageRawData, userId);

            var changeResult = new ReviewChangeParticipantsStatusResult
            {
                ReviewType = await _reviewsRepository.GetReviewTypeAsync(reviewId, userId)
            };

            if (content.Role == ReviewParticipantRole.Approver)
            {
                await EnableRequireESignatureWhenProjectESignatureEnabledByDefaultAsync(reviewId, userId, reviewInfo.ProjectId, reviewData.ReviewPackageRawData);

                if (reviewData.ReviewPackageRawData.IsMoSEnabled && content.SelectionType == SelectionType.Selected && content.ItemIds.Count() == 1)
                {
                    changeResult.DropdownItems = reviewData.ReviewPackageRawData.Reviewers
                        .First(r => r.UserId == content.ItemIds.FirstOrDefault())
                        .SelectedRoleMoSAssignments.Select(mos => new DropdownItem(mos.GetMeaningOfSignatureDisplayValue(), mos.RoleId));
                }
            }

            if (resultErrors.Any())
            {
                changeResult.ReviewChangeItemErrors = resultErrors;
            }

            return changeResult;
        }

        private async Task UpdateMeaningOfSignatureWhenAssignApprovalRoles(int reviewId, int userId, AssignParticipantRoleParameter content,
            ReviewPackageRawData reviewRawData)
        {
            if (reviewRawData.IsMoSEnabled && content.Role == ReviewParticipantRole.Approver)
            {
                IEnumerable<MeaningOfSignatureParameter> meaningOfSignatureParameter;

                if (content.SelectionType == SelectionType.Selected)
                {
                    meaningOfSignatureParameter = content.ItemIds
                        .Select(i => new MeaningOfSignatureParameter { ParticipantId = i });
                }
                else
                {
                    if (!content.ItemIds.Any())
                    {
                        meaningOfSignatureParameter =
                            reviewRawData.Reviewers.Select(
                                reviewer => new MeaningOfSignatureParameter { ParticipantId = reviewer.UserId });
                    }
                    else
                    {
                        var meaningOfSignaturelist = new List<MeaningOfSignatureParameter>();

                        foreach (var reviewer in reviewRawData.Reviewers)
                        {
                            if (!content.ItemIds.Contains(reviewer.UserId))
                            {
                                meaningOfSignaturelist.Add(new MeaningOfSignatureParameter
                                {
                                    ParticipantId = reviewer.UserId
                                });
                            }
                        }

                        meaningOfSignatureParameter = meaningOfSignaturelist;
                    }
                }

                await
                    UpdateMeaningOfSignaturesInternalAsync(reviewId, userId, reviewRawData, meaningOfSignatureParameter,
                        new MeaningOfSignatureUpdateSetDefaultsStrategy());
            }
        }

        private static void UpdateParticipantRole(ReviewPackageRawData reviewPackageRawData, AssignParticipantRoleParameter content, IList<ReviewChangeItemsError> resultErrors)
        {
            int nonIntersecCount = 0;

            if (content.SelectionType == SelectionType.Selected)
            {
                foreach (var reviewer in reviewPackageRawData.Reviewers)
                {
                    if (content.ItemIds.Contains(reviewer.UserId))
                    {
                        reviewer.Permission = content.Role;
                    }
                }

                nonIntersecCount = content.ItemIds.Count() - content.ItemIds.Intersect(reviewPackageRawData.Reviewers.Select(r => r.UserId)).Count();
            }
            else
            {
                if (content.ItemIds != null && content.ItemIds.Any())
                {
                    foreach (var reviewer in reviewPackageRawData.Reviewers)
                    {
                        if (!content.ItemIds.Contains(reviewer.UserId))
                        {
                            reviewer.Permission = content.Role;
                        }
                    }
                }
                else
                {
                    foreach (var reviewer in reviewPackageRawData.Reviewers)
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
        }

        public async Task<ReviewChangeItemsStatusResult> AssignApprovalRequiredToArtifactsAsync(int reviewId, AssignArtifactsApprovalParameter content, int userId)
        {
            if ((content.ItemIds == null || !content.ItemIds.Any()) && content.SelectionType == SelectionType.Selected)
            {
                throw new BadRequestException("Incorrect input parameters", ErrorCodes.OutOfRangeParameter);
            }

            if (!await _permissionsRepository.HasEditPermissions(reviewId, userId))
            {
                throw ReviewsExceptionHelper.UserCannotModifyReviewException(reviewId);
            }

            var reviewInfo = await GetReviewInfoAsync(reviewId, userId);
            if (reviewInfo.LockedByUserId.GetValueOrDefault() != userId)
            {
                throw ExceptionHelper.ArtifactNotLockedException(reviewId, userId);
            }

            var review = await _reviewsRepository.GetReviewAsync(reviewId, userId);
            if (review.ReviewStatus == ReviewPackageStatus.Closed)
            {
                throw ReviewsExceptionHelper.ReviewClosedException();
            }

            if (review.Contents.Artifacts == null
                || !review.Contents.Artifacts.Any())
            {
                throw ExceptionHelper.ArtifactDoesNotSupportOperation(reviewId);
            }

            // If review is active and formal we throw conflict exception. No changes allowed
            if (review.ReviewStatus == ReviewPackageStatus.Active &&
                review.ReviewType == ReviewType.Formal)
            {
                throw ReviewsExceptionHelper.ReviewActiveFormalException();
            }
            foreach (var artifact in review.Contents.Artifacts)
            {
                if (artifact.ApprovalNotRequested == null)
                {
                    artifact.ApprovalNotRequested = (review.BaselineId == null);
                }
            }
            var resultErrors = new List<ReviewChangeItemsError>();

            var updatingArtifacts = GetReviewArtifacts(content, resultErrors, review.Contents);

            if (updatingArtifacts.Any())
            {
                // For Informal review
                await ExcludeDeletedAndNotInProjectArtifacts(content, review, reviewInfo.ProjectId, resultErrors, updatingArtifacts);
                await ExcludeArtifactsWithoutReadPermissions(content, userId, resultErrors, updatingArtifacts);

                var reviewRawData = review.ReviewPackageRawData;
                if (review.ReviewStatus == ReviewPackageStatus.Active &&
                    updatingArtifacts.Any() &&
                    content.ApprovalRequired)
                {
                    var hasArtifactsRequireApproval = review.Contents.Artifacts.FirstOrDefault(a => a.ApprovalNotRequested == false) != null;
                    // if Review has already artifacts require approval before current action it means that it is already converted to formal
                    if (!hasArtifactsRequireApproval)
                    {
                        var approver =
                            reviewRawData.Reviewers?.FirstOrDefault(r => r.Permission == ReviewParticipantRole.Approver);
                        if (approver != null)
                        {
                            throw new ConflictException(
                                "Could not update review artifacts because review needs to be converted to Formal.",
                                ErrorCodes.ReviewNeedsToMoveBackToDraftState);
                        }
                    }
                }

                foreach (var updatingArtifact in updatingArtifacts)
                {
                    updatingArtifact.ApprovalNotRequested = !content.ApprovalRequired;
                }

                var resultArtifactsXml = ReviewRawDataHelper.GetStoreData(review.Contents);

                await _reviewsRepository.UpdateReviewArtifactsAsync(reviewId, userId, resultArtifactsXml, null, false);

                if (content.ApprovalRequired)
                {
                    await EnableRequireESignatureWhenProjectESignatureEnabledByDefaultAsync(reviewId, userId, reviewInfo.ProjectId, reviewRawData);
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

        private async Task ExcludeDeletedAndNotInProjectArtifacts(AssignArtifactsApprovalParameter content, Review review, int projectId, ICollection<ReviewChangeItemsError> resultErrors, List<RDArtifact> updatingArtifacts)
        {
            if (review.BaselineId == null || review.BaselineId < 1)
            {
                var updatingArtifactIdsOnly = updatingArtifacts.Select(ua => ua.Id);
                var deletedAndNotInProjectItemIds = await _artifactVersionsRepository.GetDeletedAndNotInProjectItems(updatingArtifactIdsOnly, projectId);

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
