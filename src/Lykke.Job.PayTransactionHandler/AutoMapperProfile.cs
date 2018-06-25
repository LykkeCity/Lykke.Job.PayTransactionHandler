using AutoMapper;
using Lykke.Job.EthereumCore.Contracts.Enums.LykkePay;
using Lykke.Job.EthereumCore.Contracts.Events.LykkePay;
using Lykke.Job.PayTransactionHandler.Core;
using Lykke.Service.PayInternal.Client.Models.Transactions;
using Lykke.Service.PayInternal.Client.Models.Transactions.Ethereum;

namespace Lykke.Job.PayTransactionHandler
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<TransferEvent, RegisterInboundTxModel>(MemberList.Destination)
                .ForMember(dest => dest.Amount,
                    opt => opt.ResolveUsing((src, dest, destMember, resContext) =>
                        dest.Amount = src.Amount.ToAmount((int) resContext.Items["AssetMultiplier"],
                            (int) resContext.Items["AssetAccuracy"])))
                .ForMember(dest => dest.AssetId,
                    opt => opt.ResolveUsing((src, dest, destMember, resContext) =>
                        dest.AssetId = (string) resContext.Items["AssetId"]))
                .ForMember(dest => dest.BlockId, opt => opt.MapFrom(src => src.BlockHash))
                .ForMember(dest => dest.Blockchain,
                    opt => opt.MapFrom(src =>
                        src.WorkflowType == WorkflowType.Airlines
                            ? BlockchainType.EthereumIata
                            : BlockchainType.Ethereum))
                .ForMember(dest => dest.FirstSeen, opt => opt.MapFrom(src => src.DetectedTime))
                .ForMember(dest => dest.Hash, opt => opt.MapFrom(src => src.TransactionHash))
                .ForMember(dest => dest.Identity, opt => opt.MapFrom(src => src.TransactionHash))
                .ForMember(dest => dest.IdentityType, opt => opt.UseValue(TransactionIdentityType.Hash));

            CreateMap<TransferEvent, RegisterOutboundTxModel>(MemberList.Destination)
                .ForMember(dest => dest.Amount,
                    opt => opt.ResolveUsing((src, dest, destMember, resContext) =>
                        dest.Amount = src.Amount.ToAmount((int) resContext.Items["AssetMultiplier"],
                            (int) resContext.Items["AssetAccuracy"])))
                .ForMember(dest => dest.AssetId,
                    opt => opt.ResolveUsing((src, dest, destMember, resContext) =>
                        dest.AssetId = (string) resContext.Items["AssetId"]))
                .ForMember(dest => dest.BlockId, opt => opt.MapFrom(src => src.BlockHash))
                .ForMember(dest => dest.Blockchain,
                    opt => opt.MapFrom(src =>
                        src.WorkflowType == WorkflowType.Airlines
                            ? BlockchainType.EthereumIata
                            : BlockchainType.Ethereum))
                .ForMember(dest => dest.FirstSeen, opt => opt.MapFrom(src => src.DetectedTime))
                .ForMember(dest => dest.Hash, opt => opt.MapFrom(src => src.TransactionHash))
                .ForMember(dest => dest.Identity, opt => opt.MapFrom(src => src.OperationId))
                .ForMember(dest => dest.IdentityType, opt => opt.UseValue(TransactionIdentityType.Specific));

            CreateMap<TransferEvent, FailOutboundTxModel>(MemberList.Destination)
                .ForMember(dest => dest.Identity, opt => opt.MapFrom(src => src.OperationId))
                .ForMember(dest => dest.IdentityType, opt => opt.UseValue(TransactionIdentityType.Specific));

            CreateMap<TransferEvent, CompleteOutboundTxModel>(MemberList.Destination)
                .ForMember(dest => dest.Identity, opt => opt.MapFrom(src => src.OperationId))
                .ForMember(dest => dest.IdentityType, opt => opt.UseValue(TransactionIdentityType.Specific));
        }
    }
}
