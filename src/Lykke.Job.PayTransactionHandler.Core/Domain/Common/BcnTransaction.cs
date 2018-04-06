﻿using System;

namespace Lykke.Job.PayTransactionHandler.Core.Domain.Common
{
    /// <summary>
    /// Represents blockchain transaction.
    /// </summary>
    public class BcnTransaction : IEquatable<BcnTransaction>
    {
        public string Id { get; set; }
        public decimal Amount { get; set; }
        public string AssetId { get; set; }
        public int Confirmations { get; set; }
        public string BlockId { get; set; }
        public BlockchainType Blockchain { get; set; }

        public bool Equals(BcnTransaction other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return string.Equals(Id, other.Id) && Amount.Equals(other.Amount) &&
                   string.Equals(AssetId, other.AssetId) && Confirmations == other.Confirmations &&
                   string.Equals(BlockId, other.BlockId) && Blockchain == other.Blockchain;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;

            return Equals((BcnTransaction) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Id != null ? Id.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Amount.GetHashCode();
                hashCode = (hashCode * 397) ^ (AssetId != null ? AssetId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Confirmations;
                hashCode = (hashCode * 397) ^ (BlockId != null ? BlockId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Blockchain != null ? Blockchain.GetHashCode() : 0);

                return hashCode;
            }
        }
    }
}
