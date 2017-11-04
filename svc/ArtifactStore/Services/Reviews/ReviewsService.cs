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
        private readonly ILockArtifactsRepository _lockArtifactsRepository;

        public ReviewsService() : this(
                new SqlReviewsRepository(),
                new SqlArtifactRepository(),
                new SqlArtifactPermissionsRepository(),
                new SqlLockArtifactsRepository())
        {
        }

        public ReviewsService(
            IReviewsRepository reviewsRepository,
            IArtifactRepository artifactRepository,
            IArtifactPermissionsRepository permissionsRepository,
            ILockArtifactsRepository lockArtifactsRepository)
        {
            _reviewsRepository = reviewsRepository;
            _artifactRepository = artifactRepository;
            _permissionsRepository = permissionsRepository;
            _lockArtifactsRepository = lockArtifactsRepository;
        }

        public async Task<ReviewSettings> GetReviewSettingsAsync(int reviewId, int userId, int revisionId = int.MaxValue)
        {
            await GetReviewInfoAsync(reviewId, userId, revisionId);

            var reviewPackageRawData = await _reviewsRepository.GetReviewPackageRawDataAsync(reviewId, userId);
            return new ReviewSettings(reviewPackageRawData);
        }

        public async Task UpdateReviewSettingsAsync(int reviewId, ReviewSettings updatedReviewSettings, int userId)
        {
            var reviewInfo = await GetReviewInfoAsync(reviewId, userId);

            var reviewPackageRawData = await _reviewsRepository.GetReviewPackageRawDataAsync(reviewId, userId) ?? new ReviewPackageRawData();

            if (reviewPackageRawData.Status == ReviewPackageStatus.Closed)
            {
                throw new ConflictException(I18NHelper.FormatInvariant(ErrorMessages.ReviewIsClosed, reviewId), ErrorCodes.ReviewClosed);
            }

            await LockReviewAsync(reviewId, userId, reviewInfo);

            UpdateEndDate(updatedReviewSettings, reviewPackageRawData);
            UpdateShowOnlyDescription(updatedReviewSettings, reviewPackageRawData);
            UpdateCanMarkAsComplete(reviewId, updatedReviewSettings, reviewPackageRawData);
            UpdateRequireESignature(updatedReviewSettings, reviewPackageRawData);
            await UpdateRequireMeaningOfSignatureAsync(reviewInfo.ItemId, reviewInfo.ProjectId, updatedReviewSettings, reviewPackageRawData);

            await _reviewsRepository.UpdateReviewPackageRawDataAsync(reviewId, reviewPackageRawData, userId);
        }

        private async Task LockReviewAsync(int reviewId, int userId, ArtifactBasicDetails reviewInfo)
        {
            if (reviewInfo.LockedByUserId.HasValue)
            {
                if (reviewInfo.LockedByUserId.Value != userId)
                {
                    var errorMessage = I18NHelper.FormatInvariant(ErrorMessages.ArtifactNotLockedByUser, reviewId, userId);
                    throw new ConflictException(errorMessage, ErrorCodes.LockedByOtherUser);
                }
            }
            else
            {
                if (!await _lockArtifactsRepository.LockArtifactAsync(reviewId, userId))
                {
                    var errorMessage = I18NHelper.FormatInvariant(ErrorMessages.ArtifactNotLockedByUser, reviewId, userId);
                    throw new ConflictException(errorMessage, ErrorCodes.LockedByOtherUser);
                }
            }
        }

        private static void UpdateEndDate(ReviewSettings updatedReviewSettings, ReviewPackageRawData reviewPackageRawData)
        {
            reviewPackageRawData.EndDate = updatedReviewSettings.EndDate;
        }

        private static void UpdateShowOnlyDescription(ReviewSettings updatedReviewSettings, ReviewPackageRawData reviewPackageRawData)
        {
            reviewPackageRawData.ShowOnlyDescription = updatedReviewSettings.ShowOnlyDescription;
        }

        private static void UpdateCanMarkAsComplete(int reviewId, ReviewSettings updatedReviewSettings, ReviewPackageRawData reviewPackageRawData)
        {
            var settingChanged =
                reviewPackageRawData.IsAllowToMarkReviewAsCompleteWhenAllArtifactsReviewed != updatedReviewSettings.CanMarkAsComplete;

            if (!settingChanged)
            {
                return;
            }

            if (reviewPackageRawData.Status != ReviewPackageStatus.Draft)
            {
                var errorMessage = I18NHelper.FormatInvariant(ErrorMessages.ReviewIsNotDraft, reviewId);
                throw new ConflictException(errorMessage, ErrorCodes.Conflict);
            }

            reviewPackageRawData.IsAllowToMarkReviewAsCompleteWhenAllArtifactsReviewed = updatedReviewSettings.CanMarkAsComplete;
        }

        private static void UpdateRequireESignature(ReviewSettings updatedReviewSettings, ReviewPackageRawData reviewPackageRawData)
        {
            reviewPackageRawData.IsESignatureEnabled = updatedReviewSettings.RequireESignature;
        }

        private async Task UpdateRequireMeaningOfSignatureAsync(int reviewId, int projectId, ReviewSettings updatedReviewSettings, ReviewPackageRawData reviewPackageRawData)
        {
            var settingChanged = reviewPackageRawData.IsMoSEnabled != updatedReviewSettings.RequireMeaningOfSignature;

            if (!settingChanged)
            {
                return;
            }

            if (reviewPackageRawData.Status != ReviewPackageStatus.Draft)
            {
                var errorMessage = I18NHelper.FormatInvariant(ErrorMessages.ReviewIsNotDraft, reviewId);
                throw new ConflictException(errorMessage, ErrorCodes.Conflict);
            }

            if (!reviewPackageRawData.IsESignatureEnabled)
            {
                var errorMessage = I18NHelper.FormatInvariant(ErrorMessages.RequireESignatureDisabled, reviewId);
                throw new ConflictException(errorMessage, ErrorCodes.Conflict);
            }

            var projectPermissions = await _permissionsRepository.GetProjectPermissions(projectId);
            if (!projectPermissions.HasFlag(ProjectPermissions.IsMeaningOfSignatureEnabled))
            {
                throw new ConflictException(ErrorMessages.MeaningOfSignatureDisabledInProject, ErrorCodes.Conflict);
            }

            reviewPackageRawData.IsMoSEnabled = updatedReviewSettings.RequireMeaningOfSignature;

            var meaningOfSignatureParameters = reviewPackageRawData.Reviewers
                                                                   .Where(r => r.Permission == ReviewParticipantRole.Approver)
                                                                   .Select(r => new MeaningOfSignatureParameter()
                                                                   {
                                                                       ParticipantId = r.UserId
                                                                   });

            await UpdateMeaningOfSignaturesInternalAsync(reviewId, reviewPackageRawData, meaningOfSignatureParameters, new MeaningOfSignatureUpdateSetDefaultsStrategy());
        }

        private async Task<ArtifactBasicDetails> GetReviewInfoAsync(int reviewId, int userId, int revisionId = int.MaxValue)
        {
            var artifactInfo = await _artifactRepository.GetArtifactBasicDetails(reviewId, userId);
            if (artifactInfo == null)
            {
                var errorMessage = revisionId != int.MaxValue ?
                    I18NHelper.FormatInvariant(ErrorMessages.ReviewOrRevisionNotFound, reviewId, revisionId) :
                    I18NHelper.FormatInvariant(ErrorMessages.ReviewNotFound, reviewId);
                throw new ResourceNotFoundException(errorMessage, ErrorCodes.ResourceNotFound);
            }

            if (artifactInfo.PrimitiveItemTypePredefined != (int)ItemTypePredefined.ArtifactReviewPackage)
            {
                throw new BadRequestException(I18NHelper.FormatInvariant(ErrorMessages.ArtifactIsNotReview, reviewId), ErrorCodes.BadRequest);
            }

            if (!await _permissionsRepository.HasReadPermissions(reviewId, userId))
            {
                throw ReviewsExceptionHelper.UserCannotAccessReviewException(reviewId);
            }

            return artifactInfo;
        }

        public async Task UpdateMeaningOfSignaturesAsync(int reviewId, int userId, IEnumerable<MeaningOfSignatureParameter> meaningOfSignatureParameters)
        {
            var reviewInfo = await GetReviewInfoAsync(reviewId, userId);

            var reviewPackage = await _reviewsRepository.GetReviewPackageRawDataAsync(reviewId, userId) ?? new ReviewPackageRawData();

            if (reviewPackage.Status == ReviewPackageStatus.Closed)
            {
                throw ReviewsExceptionHelper.ReviewClosedException();
            }

            if (!reviewPackage.IsMoSEnabled)
            {
                throw new ConflictException("Could not update review because meaning of signature is not enabled.", ErrorCodes.MeaningOfSignatureNotEnabled);
            }

            await LockReviewAsync(reviewId, userId, reviewInfo);

            await UpdateMeaningOfSignaturesInternalAsync(reviewId, reviewPackage, meaningOfSignatureParameters, new MeaningOfSignatureUpdateSpecificStrategy());

            await _reviewsRepository.UpdateReviewPackageRawDataAsync(reviewId, reviewPackage, userId);
        }

        private async Task UpdateMeaningOfSignaturesInternalAsync(int reviewId, ReviewPackageRawData reviewPackage, IEnumerable<MeaningOfSignatureParameter> meaningOfSignatureParameters,
                                                                  IMeaningOfSignatureUpdateStrategy updateStrategy)
        {
            var meaningOfSignatureParamList = meaningOfSignatureParameters.ToList();

            var participantIds = meaningOfSignatureParamList.Select(mos => mos.ParticipantId).ToList();

            var possibleMeaningOfSignatures = await _reviewsRepository.GetPossibleMeaningOfSignaturesForParticipantsAsync(participantIds);

            foreach (var participantId in participantIds)
            {
                var participant = reviewPackage.Reviewers.FirstOrDefault(r => r.UserId == participantId);

                if (participant == null)
                {
                    throw new BadRequestException("Could not update meaning of signature because participant is not in review.", ErrorCodes.UserNotInReview);
                }

                if (participant.Permission != ReviewParticipantRole.Approver)
                {
                    throw new BadRequestException("Could not update meaning of signature because participant is not an approver.", ErrorCodes.ParticipantIsNotAnApprover);
                }

                if (!possibleMeaningOfSignatures.ContainsKey(participantId))
                {
                    throw new ConflictException("Could not update meaning of signature because meaning of signature is not possible for a participant.", ErrorCodes.MeaningOfSignatureNotPossible);
                }

                var meaningOfSignatureUpdates = updateStrategy.GetMeaningOfSignatureUpdates(participantId, possibleMeaningOfSignatures, meaningOfSignatureParamList);

                if (participant.SelectedRoleMoSAssignments == null)
                {
                    participant.SelectedRoleMoSAssignments = new List<ParticipantMeaningOfSignature>();
                }

                foreach (var meaningOfSignatureUpdate in meaningOfSignatureUpdates)
                {
                    var meaningOfSignature = meaningOfSignatureUpdate.MeaningOfSignature;

                    ParticipantMeaningOfSignature participantMeaningOfSignature = participant.SelectedRoleMoSAssignments
                                                                                             .FirstOrDefault(pmos => pmos.RoleAssignmentId == meaningOfSignature.RoleAssignmentId);

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

        public async Task<IEnumerable<DropdownItem>> AssignRoleToParticipantAsync(int reviewId, AssignParticipantRoleParameter content, int userId)
        {
            var propertyResult = await _reviewsRepository.GetReviewApprovalRolesInfoAsync(reviewId, userId, content.UserId);

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
                ExceptionHelper.ThrowArtifactNotLockedException(reviewId, content.UserId);
            }

            if (string.IsNullOrEmpty(propertyResult.ArtifactXml))
            {
                ExceptionHelper.ThrowArtifactDoesNotSupportOperation(reviewId);
            }

            var reviewPackage = UpdateParticipantRole(propertyResult.ArtifactXml, content, reviewId);

            if (reviewPackage.IsMoSEnabled && content.Role == ReviewParticipantRole.Approver)
            {
                var meaningOfSignatureParameter = new MeaningOfSignatureParameter()
                {
                    ParticipantId = content.UserId
                };

                await UpdateMeaningOfSignaturesInternalAsync(reviewId, reviewPackage, new[] { meaningOfSignatureParameter }, new MeaningOfSignatureUpdateSetDefaultsStrategy());
            }

            await _reviewsRepository.UpdateReviewPackageRawDataAsync(reviewId, reviewPackage, userId);

            if (reviewPackage.IsMoSEnabled && content.Role == ReviewParticipantRole.Approver)
            {
                return reviewPackage.Reviewers.First(r => r.UserId == content.UserId).SelectedRoleMoSAssignments.Select(mos =>
                    new DropdownItem($"{mos.MeaningOfSignatureValue} ({mos.RoleName})", mos.RoleAssignmentId));
            }

            return null;
        }

        private static ReviewPackageRawData UpdateParticipantRole(string reviewPackageXml, AssignParticipantRoleParameter content, int reviewId)
        {
            var reviewPackageRawData = ReviewRawDataHelper.RestoreData<ReviewPackageRawData>(reviewPackageXml);

            var participant = reviewPackageRawData.Reviewers.FirstOrDefault(a => a.UserId == content.UserId);

            if (participant == null)
            {
                ExceptionHelper.ThrowArtifactDoesNotSupportOperation(reviewId);
            }

            participant.Permission = content.Role;

            return reviewPackageRawData;
        }
    }
}
