using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Web3;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Contracts.CQS;
using Nethereum.Contracts.ContractHandlers;
using Nethereum.Contracts;
using System.Threading;
using RedOrBlack.Contracts.Wheel.ContractDefinition;

namespace RedOrBlack.Contracts.Wheel
{
    public partial class WheelService
    {
        public static Task<TransactionReceipt> DeployContractAndWaitForReceiptAsync(Nethereum.Web3.Web3 web3, WheelDeployment wheelDeployment, CancellationTokenSource cancellationTokenSource = null)
        {
            return web3.Eth.GetContractDeploymentHandler<WheelDeployment>().SendRequestAndWaitForReceiptAsync(wheelDeployment, cancellationTokenSource);
        }

        public static Task<string> DeployContractAsync(Nethereum.Web3.Web3 web3, WheelDeployment wheelDeployment)
        {
            return web3.Eth.GetContractDeploymentHandler<WheelDeployment>().SendRequestAsync(wheelDeployment);
        }

        public static async Task<WheelService> DeployContractAndGetServiceAsync(Nethereum.Web3.Web3 web3, WheelDeployment wheelDeployment, CancellationTokenSource cancellationTokenSource = null)
        {
            var receipt = await DeployContractAndWaitForReceiptAsync(web3, wheelDeployment, cancellationTokenSource);
            return new WheelService(web3, receipt.ContractAddress);
        }

        protected Nethereum.Web3.IWeb3 Web3{ get; }

        public ContractHandler ContractHandler { get; }

        public WheelService(Nethereum.Web3.Web3 web3, string contractAddress)
        {
            Web3 = web3;
            ContractHandler = web3.Eth.GetContractHandler(contractAddress);
        }

        public WheelService(Nethereum.Web3.IWeb3 web3, string contractAddress)
        {
            Web3 = web3;
            ContractHandler = web3.Eth.GetContractHandler(contractAddress);
        }

        public Task<BigInteger> CalculateExposureQueryAsync(CalculateExposureFunction calculateExposureFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<CalculateExposureFunction, BigInteger>(calculateExposureFunction, blockParameter);
        }

        
        public Task<BigInteger> CalculateExposureQueryAsync(BigInteger spinId, BlockParameter blockParameter = null)
        {
            var calculateExposureFunction = new CalculateExposureFunction();
                calculateExposureFunction.SpinId = spinId;
            
            return ContractHandler.QueryAsync<CalculateExposureFunction, BigInteger>(calculateExposureFunction, blockParameter);
        }

        public Task<string> CloseAccountRequestAsync(CloseAccountFunction closeAccountFunction)
        {
             return ContractHandler.SendRequestAsync(closeAccountFunction);
        }

        public Task<string> CloseAccountRequestAsync()
        {
             return ContractHandler.SendRequestAsync<CloseAccountFunction>();
        }

        public Task<TransactionReceipt> CloseAccountRequestAndWaitForReceiptAsync(CloseAccountFunction closeAccountFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(closeAccountFunction, cancellationToken);
        }

        public Task<TransactionReceipt> CloseAccountRequestAndWaitForReceiptAsync(CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync<CloseAccountFunction>(null, cancellationToken);
        }

        public Task<BigInteger> CurrentBalanceQueryAsync(CurrentBalanceFunction currentBalanceFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<CurrentBalanceFunction, BigInteger>(currentBalanceFunction, blockParameter);
        }

        
        public Task<BigInteger> CurrentBalanceQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<CurrentBalanceFunction, BigInteger>(null, blockParameter);
        }

        public Task<string> FundRequestAsync(FundFunction fundFunction)
        {
             return ContractHandler.SendRequestAsync(fundFunction);
        }

        public Task<string> FundRequestAsync()
        {
             return ContractHandler.SendRequestAsync<FundFunction>();
        }

        public Task<TransactionReceipt> FundRequestAndWaitForReceiptAsync(FundFunction fundFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(fundFunction, cancellationToken);
        }

        public Task<TransactionReceipt> FundRequestAndWaitForReceiptAsync(CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync<FundFunction>(null, cancellationToken);
        }

        public Task<string> FundAccountRequestAsync(FundAccountFunction fundAccountFunction)
        {
             return ContractHandler.SendRequestAsync(fundAccountFunction);
        }

        public Task<string> FundAccountRequestAsync()
        {
             return ContractHandler.SendRequestAsync<FundAccountFunction>();
        }

        public Task<TransactionReceipt> FundAccountRequestAndWaitForReceiptAsync(FundAccountFunction fundAccountFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(fundAccountFunction, cancellationToken);
        }

        public Task<TransactionReceipt> FundAccountRequestAndWaitForReceiptAsync(CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync<FundAccountFunction>(null, cancellationToken);
        }

        public Task<GetAccountOutputDTO> GetAccountQueryAsync(GetAccountFunction getAccountFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryDeserializingToObjectAsync<GetAccountFunction, GetAccountOutputDTO>(getAccountFunction, blockParameter);
        }

        public Task<GetAccountOutputDTO> GetAccountQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryDeserializingToObjectAsync<GetAccountFunction, GetAccountOutputDTO>(null, blockParameter);
        }

        public Task<GetBetsForSpinOutputDTO> GetBetsForSpinQueryAsync(GetBetsForSpinFunction getBetsForSpinFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryDeserializingToObjectAsync<GetBetsForSpinFunction, GetBetsForSpinOutputDTO>(getBetsForSpinFunction, blockParameter);
        }

        public Task<GetBetsForSpinOutputDTO> GetBetsForSpinQueryAsync(BigInteger spinId, BlockParameter blockParameter = null)
        {
            var getBetsForSpinFunction = new GetBetsForSpinFunction();
                getBetsForSpinFunction.SpinId = spinId;
            
            return ContractHandler.QueryDeserializingToObjectAsync<GetBetsForSpinFunction, GetBetsForSpinOutputDTO>(getBetsForSpinFunction, blockParameter);
        }

        public Task<GetCurrentBetsOutputDTO> GetCurrentBetsQueryAsync(GetCurrentBetsFunction getCurrentBetsFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryDeserializingToObjectAsync<GetCurrentBetsFunction, GetCurrentBetsOutputDTO>(getCurrentBetsFunction, blockParameter);
        }

        public Task<GetCurrentBetsOutputDTO> GetCurrentBetsQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryDeserializingToObjectAsync<GetCurrentBetsFunction, GetCurrentBetsOutputDTO>(null, blockParameter);
        }

        public Task<GetCurrentSpinOutputDTO> GetCurrentSpinQueryAsync(GetCurrentSpinFunction getCurrentSpinFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryDeserializingToObjectAsync<GetCurrentSpinFunction, GetCurrentSpinOutputDTO>(getCurrentSpinFunction, blockParameter);
        }

        public Task<GetCurrentSpinOutputDTO> GetCurrentSpinQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryDeserializingToObjectAsync<GetCurrentSpinFunction, GetCurrentSpinOutputDTO>(null, blockParameter);
        }

        public Task<GetLastSpinsOutputDTO> GetLastSpinsQueryAsync(GetLastSpinsFunction getLastSpinsFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryDeserializingToObjectAsync<GetLastSpinsFunction, GetLastSpinsOutputDTO>(getLastSpinsFunction, blockParameter);
        }

        public Task<GetLastSpinsOutputDTO> GetLastSpinsQueryAsync(BigInteger count, BlockParameter blockParameter = null)
        {
            var getLastSpinsFunction = new GetLastSpinsFunction();
                getLastSpinsFunction.Count = count;
            
            return ContractHandler.QueryDeserializingToObjectAsync<GetLastSpinsFunction, GetLastSpinsOutputDTO>(getLastSpinsFunction, blockParameter);
        }

        public Task<GetNumbersOutputDTO> GetNumbersQueryAsync(GetNumbersFunction getNumbersFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryDeserializingToObjectAsync<GetNumbersFunction, GetNumbersOutputDTO>(getNumbersFunction, blockParameter);
        }

        public Task<GetNumbersOutputDTO> GetNumbersQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryDeserializingToObjectAsync<GetNumbersFunction, GetNumbersOutputDTO>(null, blockParameter);
        }

        public Task<bool> IsOpenForWithdrawlQueryAsync(IsOpenForWithdrawlFunction isOpenForWithdrawlFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<IsOpenForWithdrawlFunction, bool>(isOpenForWithdrawlFunction, blockParameter);
        }

        
        public Task<bool> IsOpenForWithdrawlQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<IsOpenForWithdrawlFunction, bool>(null, blockParameter);
        }

        public Task<BigInteger> MinbetinsideQueryAsync(MinbetinsideFunction minbetinsideFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<MinbetinsideFunction, BigInteger>(minbetinsideFunction, blockParameter);
        }

        
        public Task<BigInteger> MinbetinsideQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<MinbetinsideFunction, BigInteger>(null, blockParameter);
        }

        public Task<BigInteger> MinbetoutsideQueryAsync(MinbetoutsideFunction minbetoutsideFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<MinbetoutsideFunction, BigInteger>(minbetoutsideFunction, blockParameter);
        }

        
        public Task<BigInteger> MinbetoutsideQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<MinbetoutsideFunction, BigInteger>(null, blockParameter);
        }

        public Task<BigInteger> MindepositQueryAsync(MindepositFunction mindepositFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<MindepositFunction, BigInteger>(mindepositFunction, blockParameter);
        }

        
        public Task<BigInteger> MindepositQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<MindepositFunction, BigInteger>(null, blockParameter);
        }

        public Task<string> OpenAccountRequestAsync(OpenAccountFunction openAccountFunction)
        {
             return ContractHandler.SendRequestAsync(openAccountFunction);
        }

        public Task<TransactionReceipt> OpenAccountRequestAndWaitForReceiptAsync(OpenAccountFunction openAccountFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(openAccountFunction, cancellationToken);
        }

        public Task<string> OpenAccountRequestAsync(string nick)
        {
            var openAccountFunction = new OpenAccountFunction();
                openAccountFunction.Nick = nick;
            
             return ContractHandler.SendRequestAsync(openAccountFunction);
        }

        public Task<TransactionReceipt> OpenAccountRequestAndWaitForReceiptAsync(string nick, CancellationTokenSource cancellationToken = null)
        {
            var openAccountFunction = new OpenAccountFunction();
                openAccountFunction.Nick = nick;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(openAccountFunction, cancellationToken);
        }

        public Task<string> OpenBetsRequestAsync(OpenBetsFunction openBetsFunction)
        {
             return ContractHandler.SendRequestAsync(openBetsFunction);
        }

        public Task<string> OpenBetsRequestAsync()
        {
             return ContractHandler.SendRequestAsync<OpenBetsFunction>();
        }

        public Task<TransactionReceipt> OpenBetsRequestAndWaitForReceiptAsync(OpenBetsFunction openBetsFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(openBetsFunction, cancellationToken);
        }

        public Task<TransactionReceipt> OpenBetsRequestAndWaitForReceiptAsync(CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync<OpenBetsFunction>(null, cancellationToken);
        }

        public Task<string> OwnerQueryAsync(OwnerFunction ownerFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<OwnerFunction, string>(ownerFunction, blockParameter);
        }

        
        public Task<string> OwnerQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<OwnerFunction, string>(null, blockParameter);
        }

        public Task<string> Place18BetRequestAsync(Place18BetFunction place18BetFunction)
        {
             return ContractHandler.SendRequestAsync(place18BetFunction);
        }

        public Task<TransactionReceipt> Place18BetRequestAndWaitForReceiptAsync(Place18BetFunction place18BetFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(place18BetFunction, cancellationToken);
        }

        public Task<string> Place18BetRequestAsync(byte which18, BigInteger amount)
        {
            var place18BetFunction = new Place18BetFunction();
                place18BetFunction.Which18 = which18;
                place18BetFunction.Amount = amount;
            
             return ContractHandler.SendRequestAsync(place18BetFunction);
        }

        public Task<TransactionReceipt> Place18BetRequestAndWaitForReceiptAsync(byte which18, BigInteger amount, CancellationTokenSource cancellationToken = null)
        {
            var place18BetFunction = new Place18BetFunction();
                place18BetFunction.Which18 = which18;
                place18BetFunction.Amount = amount;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(place18BetFunction, cancellationToken);
        }

        public Task<string> PlaceColorBetRequestAsync(PlaceColorBetFunction placeColorBetFunction)
        {
             return ContractHandler.SendRequestAsync(placeColorBetFunction);
        }

        public Task<TransactionReceipt> PlaceColorBetRequestAndWaitForReceiptAsync(PlaceColorBetFunction placeColorBetFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(placeColorBetFunction, cancellationToken);
        }

        public Task<string> PlaceColorBetRequestAsync(byte color, BigInteger amount)
        {
            var placeColorBetFunction = new PlaceColorBetFunction();
                placeColorBetFunction.Color = color;
                placeColorBetFunction.Amount = amount;
            
             return ContractHandler.SendRequestAsync(placeColorBetFunction);
        }

        public Task<TransactionReceipt> PlaceColorBetRequestAndWaitForReceiptAsync(byte color, BigInteger amount, CancellationTokenSource cancellationToken = null)
        {
            var placeColorBetFunction = new PlaceColorBetFunction();
                placeColorBetFunction.Color = color;
                placeColorBetFunction.Amount = amount;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(placeColorBetFunction, cancellationToken);
        }

        public Task<string> PlaceColumnBetRequestAsync(PlaceColumnBetFunction placeColumnBetFunction)
        {
             return ContractHandler.SendRequestAsync(placeColumnBetFunction);
        }

        public Task<TransactionReceipt> PlaceColumnBetRequestAndWaitForReceiptAsync(PlaceColumnBetFunction placeColumnBetFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(placeColumnBetFunction, cancellationToken);
        }

        public Task<string> PlaceColumnBetRequestAsync(byte column, BigInteger amount)
        {
            var placeColumnBetFunction = new PlaceColumnBetFunction();
                placeColumnBetFunction.Column = column;
                placeColumnBetFunction.Amount = amount;
            
             return ContractHandler.SendRequestAsync(placeColumnBetFunction);
        }

        public Task<TransactionReceipt> PlaceColumnBetRequestAndWaitForReceiptAsync(byte column, BigInteger amount, CancellationTokenSource cancellationToken = null)
        {
            var placeColumnBetFunction = new PlaceColumnBetFunction();
                placeColumnBetFunction.Column = column;
                placeColumnBetFunction.Amount = amount;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(placeColumnBetFunction, cancellationToken);
        }

        public Task<string> PlaceDozenBetRequestAsync(PlaceDozenBetFunction placeDozenBetFunction)
        {
             return ContractHandler.SendRequestAsync(placeDozenBetFunction);
        }

        public Task<TransactionReceipt> PlaceDozenBetRequestAndWaitForReceiptAsync(PlaceDozenBetFunction placeDozenBetFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(placeDozenBetFunction, cancellationToken);
        }

        public Task<string> PlaceDozenBetRequestAsync(byte whichDozen, BigInteger amount)
        {
            var placeDozenBetFunction = new PlaceDozenBetFunction();
                placeDozenBetFunction.WhichDozen = whichDozen;
                placeDozenBetFunction.Amount = amount;
            
             return ContractHandler.SendRequestAsync(placeDozenBetFunction);
        }

        public Task<TransactionReceipt> PlaceDozenBetRequestAndWaitForReceiptAsync(byte whichDozen, BigInteger amount, CancellationTokenSource cancellationToken = null)
        {
            var placeDozenBetFunction = new PlaceDozenBetFunction();
                placeDozenBetFunction.WhichDozen = whichDozen;
                placeDozenBetFunction.Amount = amount;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(placeDozenBetFunction, cancellationToken);
        }

        public Task<string> PlaceParityBetRequestAsync(PlaceParityBetFunction placeParityBetFunction)
        {
             return ContractHandler.SendRequestAsync(placeParityBetFunction);
        }

        public Task<TransactionReceipt> PlaceParityBetRequestAndWaitForReceiptAsync(PlaceParityBetFunction placeParityBetFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(placeParityBetFunction, cancellationToken);
        }

        public Task<string> PlaceParityBetRequestAsync(byte parity, BigInteger amount)
        {
            var placeParityBetFunction = new PlaceParityBetFunction();
                placeParityBetFunction.Parity = parity;
                placeParityBetFunction.Amount = amount;
            
             return ContractHandler.SendRequestAsync(placeParityBetFunction);
        }

        public Task<TransactionReceipt> PlaceParityBetRequestAndWaitForReceiptAsync(byte parity, BigInteger amount, CancellationTokenSource cancellationToken = null)
        {
            var placeParityBetFunction = new PlaceParityBetFunction();
                placeParityBetFunction.Parity = parity;
                placeParityBetFunction.Amount = amount;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(placeParityBetFunction, cancellationToken);
        }

        public Task<string> PlaceRowBetRequestAsync(PlaceRowBetFunction placeRowBetFunction)
        {
             return ContractHandler.SendRequestAsync(placeRowBetFunction);
        }

        public Task<TransactionReceipt> PlaceRowBetRequestAndWaitForReceiptAsync(PlaceRowBetFunction placeRowBetFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(placeRowBetFunction, cancellationToken);
        }

        public Task<string> PlaceRowBetRequestAsync(BigInteger amount)
        {
            var placeRowBetFunction = new PlaceRowBetFunction();
                placeRowBetFunction.Amount = amount;
            
             return ContractHandler.SendRequestAsync(placeRowBetFunction);
        }

        public Task<TransactionReceipt> PlaceRowBetRequestAndWaitForReceiptAsync(BigInteger amount, CancellationTokenSource cancellationToken = null)
        {
            var placeRowBetFunction = new PlaceRowBetFunction();
                placeRowBetFunction.Amount = amount;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(placeRowBetFunction, cancellationToken);
        }

        public Task<string> PlaceSplitBetRequestAsync(PlaceSplitBetFunction placeSplitBetFunction)
        {
             return ContractHandler.SendRequestAsync(placeSplitBetFunction);
        }

        public Task<TransactionReceipt> PlaceSplitBetRequestAndWaitForReceiptAsync(PlaceSplitBetFunction placeSplitBetFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(placeSplitBetFunction, cancellationToken);
        }

        public Task<string> PlaceSplitBetRequestAsync(BigInteger number1, BigInteger number2, BigInteger amount)
        {
            var placeSplitBetFunction = new PlaceSplitBetFunction();
                placeSplitBetFunction.Number1 = number1;
                placeSplitBetFunction.Number2 = number2;
                placeSplitBetFunction.Amount = amount;
            
             return ContractHandler.SendRequestAsync(placeSplitBetFunction);
        }

        public Task<TransactionReceipt> PlaceSplitBetRequestAndWaitForReceiptAsync(BigInteger number1, BigInteger number2, BigInteger amount, CancellationTokenSource cancellationToken = null)
        {
            var placeSplitBetFunction = new PlaceSplitBetFunction();
                placeSplitBetFunction.Number1 = number1;
                placeSplitBetFunction.Number2 = number2;
                placeSplitBetFunction.Amount = amount;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(placeSplitBetFunction, cancellationToken);
        }

        public Task<string> PlaceStraightUpBetRequestAsync(PlaceStraightUpBetFunction placeStraightUpBetFunction)
        {
             return ContractHandler.SendRequestAsync(placeStraightUpBetFunction);
        }

        public Task<TransactionReceipt> PlaceStraightUpBetRequestAndWaitForReceiptAsync(PlaceStraightUpBetFunction placeStraightUpBetFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(placeStraightUpBetFunction, cancellationToken);
        }

        public Task<string> PlaceStraightUpBetRequestAsync(BigInteger numberId, BigInteger amount)
        {
            var placeStraightUpBetFunction = new PlaceStraightUpBetFunction();
                placeStraightUpBetFunction.NumberId = numberId;
                placeStraightUpBetFunction.Amount = amount;
            
             return ContractHandler.SendRequestAsync(placeStraightUpBetFunction);
        }

        public Task<TransactionReceipt> PlaceStraightUpBetRequestAndWaitForReceiptAsync(BigInteger numberId, BigInteger amount, CancellationTokenSource cancellationToken = null)
        {
            var placeStraightUpBetFunction = new PlaceStraightUpBetFunction();
                placeStraightUpBetFunction.NumberId = numberId;
                placeStraightUpBetFunction.Amount = amount;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(placeStraightUpBetFunction, cancellationToken);
        }

        public Task<string> PlaceStreetBetRequestAsync(PlaceStreetBetFunction placeStreetBetFunction)
        {
             return ContractHandler.SendRequestAsync(placeStreetBetFunction);
        }

        public Task<TransactionReceipt> PlaceStreetBetRequestAndWaitForReceiptAsync(PlaceStreetBetFunction placeStreetBetFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(placeStreetBetFunction, cancellationToken);
        }

        public Task<string> PlaceStreetBetRequestAsync(byte street, BigInteger amount)
        {
            var placeStreetBetFunction = new PlaceStreetBetFunction();
                placeStreetBetFunction.Street = street;
                placeStreetBetFunction.Amount = amount;
            
             return ContractHandler.SendRequestAsync(placeStreetBetFunction);
        }

        public Task<TransactionReceipt> PlaceStreetBetRequestAndWaitForReceiptAsync(byte street, BigInteger amount, CancellationTokenSource cancellationToken = null)
        {
            var placeStreetBetFunction = new PlaceStreetBetFunction();
                placeStreetBetFunction.Street = street;
                placeStreetBetFunction.Amount = amount;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(placeStreetBetFunction, cancellationToken);
        }

        public Task<string> PlaceTopLineBetRequestAsync(PlaceTopLineBetFunction placeTopLineBetFunction)
        {
             return ContractHandler.SendRequestAsync(placeTopLineBetFunction);
        }

        public Task<TransactionReceipt> PlaceTopLineBetRequestAndWaitForReceiptAsync(PlaceTopLineBetFunction placeTopLineBetFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(placeTopLineBetFunction, cancellationToken);
        }

        public Task<string> PlaceTopLineBetRequestAsync(BigInteger amount)
        {
            var placeTopLineBetFunction = new PlaceTopLineBetFunction();
                placeTopLineBetFunction.Amount = amount;
            
             return ContractHandler.SendRequestAsync(placeTopLineBetFunction);
        }

        public Task<TransactionReceipt> PlaceTopLineBetRequestAndWaitForReceiptAsync(BigInteger amount, CancellationTokenSource cancellationToken = null)
        {
            var placeTopLineBetFunction = new PlaceTopLineBetFunction();
                placeTopLineBetFunction.Amount = amount;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(placeTopLineBetFunction, cancellationToken);
        }

        public Task<string> RawFulfillRandomWordsRequestAsync(RawFulfillRandomWordsFunction rawFulfillRandomWordsFunction)
        {
             return ContractHandler.SendRequestAsync(rawFulfillRandomWordsFunction);
        }

        public Task<TransactionReceipt> RawFulfillRandomWordsRequestAndWaitForReceiptAsync(RawFulfillRandomWordsFunction rawFulfillRandomWordsFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(rawFulfillRandomWordsFunction, cancellationToken);
        }

        public Task<string> RawFulfillRandomWordsRequestAsync(BigInteger requestId, List<BigInteger> randomWords)
        {
            var rawFulfillRandomWordsFunction = new RawFulfillRandomWordsFunction();
                rawFulfillRandomWordsFunction.RequestId = requestId;
                rawFulfillRandomWordsFunction.RandomWords = randomWords;
            
             return ContractHandler.SendRequestAsync(rawFulfillRandomWordsFunction);
        }

        public Task<TransactionReceipt> RawFulfillRandomWordsRequestAndWaitForReceiptAsync(BigInteger requestId, List<BigInteger> randomWords, CancellationTokenSource cancellationToken = null)
        {
            var rawFulfillRandomWordsFunction = new RawFulfillRandomWordsFunction();
                rawFulfillRandomWordsFunction.RequestId = requestId;
                rawFulfillRandomWordsFunction.RandomWords = randomWords;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(rawFulfillRandomWordsFunction, cancellationToken);
        }

        public Task<string> RenounceOwnershipRequestAsync(RenounceOwnershipFunction renounceOwnershipFunction)
        {
             return ContractHandler.SendRequestAsync(renounceOwnershipFunction);
        }

        public Task<string> RenounceOwnershipRequestAsync()
        {
             return ContractHandler.SendRequestAsync<RenounceOwnershipFunction>();
        }

        public Task<TransactionReceipt> RenounceOwnershipRequestAndWaitForReceiptAsync(RenounceOwnershipFunction renounceOwnershipFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(renounceOwnershipFunction, cancellationToken);
        }

        public Task<TransactionReceipt> RenounceOwnershipRequestAndWaitForReceiptAsync(CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync<RenounceOwnershipFunction>(null, cancellationToken);
        }

        public Task<string> SpinTheWheelRequestAsync(SpinTheWheelFunction spinTheWheelFunction)
        {
             return ContractHandler.SendRequestAsync(spinTheWheelFunction);
        }

        public Task<string> SpinTheWheelRequestAsync()
        {
             return ContractHandler.SendRequestAsync<SpinTheWheelFunction>();
        }

        public Task<TransactionReceipt> SpinTheWheelRequestAndWaitForReceiptAsync(SpinTheWheelFunction spinTheWheelFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(spinTheWheelFunction, cancellationToken);
        }

        public Task<TransactionReceipt> SpinTheWheelRequestAndWaitForReceiptAsync(CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync<SpinTheWheelFunction>(null, cancellationToken);
        }

        public Task<string> TransferOwnershipRequestAsync(TransferOwnershipFunction transferOwnershipFunction)
        {
             return ContractHandler.SendRequestAsync(transferOwnershipFunction);
        }

        public Task<TransactionReceipt> TransferOwnershipRequestAndWaitForReceiptAsync(TransferOwnershipFunction transferOwnershipFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(transferOwnershipFunction, cancellationToken);
        }

        public Task<string> TransferOwnershipRequestAsync(string newOwner)
        {
            var transferOwnershipFunction = new TransferOwnershipFunction();
                transferOwnershipFunction.NewOwner = newOwner;
            
             return ContractHandler.SendRequestAsync(transferOwnershipFunction);
        }

        public Task<TransactionReceipt> TransferOwnershipRequestAndWaitForReceiptAsync(string newOwner, CancellationTokenSource cancellationToken = null)
        {
            var transferOwnershipFunction = new TransferOwnershipFunction();
                transferOwnershipFunction.NewOwner = newOwner;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(transferOwnershipFunction, cancellationToken);
        }

        public Task<string> WidthdrawFundRequestAsync(WidthdrawFundFunction widthdrawFundFunction)
        {
             return ContractHandler.SendRequestAsync(widthdrawFundFunction);
        }

        public Task<TransactionReceipt> WidthdrawFundRequestAndWaitForReceiptAsync(WidthdrawFundFunction widthdrawFundFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(widthdrawFundFunction, cancellationToken);
        }

        public Task<string> WidthdrawFundRequestAsync(BigInteger amount)
        {
            var widthdrawFundFunction = new WidthdrawFundFunction();
                widthdrawFundFunction.Amount = amount;
            
             return ContractHandler.SendRequestAsync(widthdrawFundFunction);
        }

        public Task<TransactionReceipt> WidthdrawFundRequestAndWaitForReceiptAsync(BigInteger amount, CancellationTokenSource cancellationToken = null)
        {
            var widthdrawFundFunction = new WidthdrawFundFunction();
                widthdrawFundFunction.Amount = amount;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(widthdrawFundFunction, cancellationToken);
        }

        public Task<string> WithdrawRequestAsync(WithdrawFunction withdrawFunction)
        {
             return ContractHandler.SendRequestAsync(withdrawFunction);
        }

        public Task<TransactionReceipt> WithdrawRequestAndWaitForReceiptAsync(WithdrawFunction withdrawFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(withdrawFunction, cancellationToken);
        }

        public Task<string> WithdrawRequestAsync(BigInteger amount)
        {
            var withdrawFunction = new WithdrawFunction();
                withdrawFunction.Amount = amount;
            
             return ContractHandler.SendRequestAsync(withdrawFunction);
        }

        public Task<TransactionReceipt> WithdrawRequestAndWaitForReceiptAsync(BigInteger amount, CancellationTokenSource cancellationToken = null)
        {
            var withdrawFunction = new WithdrawFunction();
                withdrawFunction.Amount = amount;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(withdrawFunction, cancellationToken);
        }
    }
}
