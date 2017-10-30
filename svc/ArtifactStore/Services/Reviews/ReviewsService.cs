using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using ArtifactStore.Helpers;
using ArtifactStore.Models.Review;
using ArtifactStore.Repositories;
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
            var reviewInfo = await GetReviewInfoAsync(reviewId, userId, int.MaxValue);

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
        }

        private async Task<ArtifactBasicDetails> GetReviewInfoAsync(int reviewId, int userId, int revisionId)
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
                throw new AuthorizationException(I18NHelper.FormatInvariant(ErrorMessages.CannotAccessReview, reviewId), ErrorCodes.Forbidden);
            }

            return artifactInfo;
        }

        public async Task UpdateMeaningOfSignaturesAsync(int reviewId, int userId, IEnumerable<MeaningOfSignatureParameter> meaningOfSignatureParameters)
        {
            if (!await _permissionsRepository.HasReadPermissions(reviewId, userId))
            {
                throw ReviewsExceptionHelper.UserCannotAccessReviewException(reviewId);
            }

            var meaningOfSignatureParamList = meaningOfSignatureParameters.ToList();

            var reviewPackage = await _reviewsRepository.GetReviewPackageRawDataAsync(reviewId, userId);

            if (reviewPackage.Status == ReviewPackageStatus.Closed)
            {
                throw ReviewsExceptionHelper.ReviewClosedException();
            }

            if (!reviewPackage.IsMoSEnabled)
            {
                throw new ConflictException("Could not update review because meaning of signature is not enabled.", ErrorCodes.MeaningOfSignatureNotEnabled);
            }

            var participantIds = meaningOfSignatureParamList.Select(mos => mos.ParticipantId);

            var possibleMeaningOfSignatures = await _reviewsRepository.GetPossibleMeaningOfSignaturesForParticipantsAsync(participantIds);

            foreach (var meaningOfSignatureParameter in meaningOfSignatureParamList)
            {
                var participantId = meaningOfSignatureParameter.ParticipantId;

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

                var meaningOfSignature = possibleMeaningOfSignatures[participantId].FirstOrDefault(mos => mos.MeaningOfSignatureId == meaningOfSignatureParameter.MeaningOfSignatureId);

                if (meaningOfSignature == null)
                {
                    throw new ConflictException("Could not update meaning of signature because meaning of signature is not possible for a participant.", ErrorCodes.MeaningOfSignatureNotPossible);
                }

                ParticipantMeaningOfSignature participantMeaningOfSignature;

                if (participant.SelectedRoleMoSAssignments == null)
                {
                    participant.SelectedRoleMoSAssignments = new List<ParticipantMeaningOfSignature>();
                }

                if ((participantMeaningOfSignature = participant.SelectedRoleMoSAssignments.FirstOrDefault(pmos => pmos.MeaningOfSignatureId == meaningOfSignature.MeaningOfSignatureId)) == null)
                {
                    participantMeaningOfSignature = new ParticipantMeaningOfSignature();

                    participant.SelectedRoleMoSAssignments.Add(participantMeaningOfSignature);
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

            await _reviewsRepository.UpdateReviewPackageRawDataAsync(reviewId, reviewPackage, userId);
        }
    }
}
