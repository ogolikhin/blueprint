using System;
using System.Threading;
using System.Threading.Tasks;
using BlueprintSys.RC.Services.Helpers;
using BluePrintSys.Messaging.CrossCutting.Models.Exceptions;
using BluePrintSys.Messaging.Models.Actions;

namespace BlueprintSys.RC.Services.MessageHandlers
{
    public enum TransactionStatus
    {
        RolledBack = -1,
        Committed = 0,
        Uncommitted = 1
    }

    public interface ITransactionValidator
    {
        Task<TransactionStatus> GetStatus(ActionMessage message, TenantInformation tenant, IBaseRepository repository);
    }

    public class TransactionValidator : ITransactionValidator
    {
        public const int TriesMax = 5;
        public const int MillisecondsTimeout = 500;

        public async Task<TransactionStatus> GetStatus(ActionMessage message, TenantInformation tenant, IBaseRepository repository)
        {
            var transactionId = message.TransactionId;
            var tries = 1;
            while (true)
            {
                // TODO remove this check when transaction IDs are implemented in messages; for now, we allow messages that do not have a transaction ID
                var status = transactionId == 0 ? 0 : await repository.GetTransactionStatus(transactionId);
                if (!Enum.IsDefined(typeof(TransactionStatus), status))
                {
                    throw new ArgumentOutOfRangeException($"Invalid Transaction Status: {status}");
                }
                var transactionStatus = (TransactionStatus) status;
                switch (transactionStatus)
                {
                    case TransactionStatus.Committed:
                        Logger.Log($"Transaction {transactionId} has been committed", message, tenant);
                        return transactionStatus;
                    case TransactionStatus.Uncommitted:
                        if (tries < TriesMax)
                        {
                            Logger.Log($"Transaction {transactionId} is uncommitted. Trying again", message, tenant);
                            Thread.Sleep(MillisecondsTimeout);
                            tries++;
                            continue;
                        }
                        var errorMessage = $"Transaction {transactionId} is still uncommitted. Unable to handle the message at this time";
                        Logger.Log(errorMessage, message, tenant);
                        throw new EntityNotFoundException(errorMessage);
                    case TransactionStatus.RolledBack:
                        Logger.Log($"Transaction {transactionId} was rolled back. The message will not be handled", message, tenant);
                        return transactionStatus;
                    default:
                        throw new ArgumentOutOfRangeException($"Unhandled Transaction Status {transactionStatus}");
                }
            }
        }
    }
}
