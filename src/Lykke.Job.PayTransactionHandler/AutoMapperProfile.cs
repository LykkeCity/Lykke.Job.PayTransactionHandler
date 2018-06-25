using AutoMapper;
using Lykke.Job.EthereumCore.Contracts.Enums.LykkePay;
using Lykke.Job.EthereumCore.Contracts.Events.LykkePay;
using Lykke.Job.PayTransactionHandler.Core;
using Lykke.Service.PayInternal.Client.Models.Transactions;

namespace Lykke.Job.PayTransactionHandler
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<TransferEvent, CreateTransactionRequest>(MemberList.Destination)
                .ForMember(dest => dest.WalletAddress, opt => opt.MapFrom(src => src.ToAddress))
                .ForMember(dest => dest.Amount,
                    opt => opt.ResolveUsing((src, dest, destMember, resContext) =>
                        dest.Amount = src.Amount.ToAmount((int) resContext.Items["AssetMultiplier"],
                            (int) resContext.Items["AssetAccuracy"])))
                .ForMember(dest => dest.FirstSeen, opt => opt.MapFrom(src => src.DetectedTime))
                .ForMember(dest => dest.BlockId, opt => opt.MapFrom(src => src.BlockHash))
                .ForMember(dest => dest.AssetId,
                    opt => opt.ResolveUsing((src, dest, destMember, resContext) =>
                        dest.AssetId = (string) resContext.Items["AssetId"]))
                .ForMember(dest => dest.Blockchain,
                    opt => opt.MapFrom(src =>
                        src.WorkflowType == WorkflowType.Airlines
                            ? BlockchainType.EthereumIata
                            : BlockchainType.Ethereum))
                .ForMember(dest => dest.Hash, opt => opt.MapFrom(src => src.TransactionHash))
                .ForMember(dest => dest.SourceWalletAddresses, opt => opt.MapFrom(src => new[] {src.FromAddress}))
                .ForMember(dest => dest.IdentityType, opt => opt.UseValue(TransactionIdentityType.Hash))
                .ForMember(dest => dest.Identity, opt => opt.MapFrom(src => src.TransactionHash))
                .ForMember(dest => dest.Confirmations,
                    opt => opt.ResolveUsing((src, dest, destMember, resContext) =>
                        dest.Confirmations = (int) resContext.Items["ConfirmationsToSucceed"]));

            CreateMap<TransferEvent, UpdateTransactionRequest>(MemberList.Destination)
                .ForMember(dest => dest.WalletAddress, opt => opt.MapFrom(src => src.FromAddress))
                .ForMember(dest => dest.Amount,
                    opt => opt.ResolveUsing((src, dest, destMember, resContext) =>
                        dest.Amount = src.Amount.ToAmount((int) resContext.Items["AssetMultiplier"],
                            (int) resContext.Items["AssetAccuracy"])))
                .ForMember(dest => dest.FirstSeen, opt => opt.MapFrom(src => src.DetectedTime))
                .ForMember(dest => dest.BlockId, opt => opt.MapFrom(src => src.BlockHash))
                .ForMember(dest => dest.Blockchain,
                    opt => opt.MapFrom(src =>
                        src.WorkflowType == WorkflowType.Airlines
                            ? BlockchainType.EthereumIata
                            : BlockchainType.Ethereum))
                .ForMember(dest => dest.Hash, opt => opt.MapFrom(src => src.TransactionHash))
                .ForMember(dest => dest.IdentityType, opt => opt.UseValue(TransactionIdentityType.Specific))
                .ForMember(dest => dest.Identity, opt => opt.MapFrom(src => src.OperationId))
                .ForMember(dest => dest.Confirmations,
                    opt => opt.ResolveUsing((src, dest, destMember, resContext) => dest.Confirmations =
                        src.EventType == EventType.Completed ? (int) resContext.Items["ConfirmationsToSucceed"] : 0));
        }
    }
}
