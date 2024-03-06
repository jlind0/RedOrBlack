using Microsoft.Extensions.Configuration;
using Nethereum.Web3;
using ReactiveUI;
using RedOrBlack.Contracts.Wheel;
using RedOrBlack.Contracts.Wheel.ContractDefinition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedOrBlack.ViewModels
{
    public struct TransactionData
    {
        public string ContractAddress { get; set; }
        public string TXData { get; set; }
        public BigInteger Value { get; set; }
    }
    public class NumberViewModel : ReactiveObject
    {
        protected HomeViewModel Root { get; }
        public Number Data { get; }
    }
    
    public class HomeViewModel : ReactiveObject, IDisposable
    {
        public Interaction<string, bool> Alert { get; } = new Interaction<string, bool>();
        public Interaction<TransactionData, string> SignatureRequest { get; } = new Interaction<TransactionData, string>();
        public ReactiveCommand<Unit, Unit> Load { get; }
        public ReactiveCommand<Unit, Unit> Deploy { get; }
        internal Web3 W3 { get; }
        private string? accountNumber;
        public string? AccountNumber
        {
            get => accountNumber;
            set => this.RaiseAndSetIfChanged(ref accountNumber, value);
        }
        private bool isDeployed;
        public bool IsDeployed
        {
            get => isDeployed;
            set => this.RaiseAndSetIfChanged(ref isDeployed, value);
        }
        private bool isLoading;
        public bool IsLoading
        {
            get => isLoading;
            set => this.RaiseAndSetIfChanged(ref isLoading, value);
        }
        private string? contractAddress;
        private bool disposedValue;

        public string? ContractAddress
        {
            get => contractAddress;
            set => this.RaiseAndSetIfChanged(ref contractAddress, value);
        }
        protected ulong SubscriptionId { get; }
        private readonly CompositeDisposable disposable = new CompositeDisposable();
        private string? ownerAddress;
        public string? OwnerAddress
        {
            get => ownerAddress;
            set
            {
                this.RaiseAndSetIfChanged(ref ownerAddress, value);
                this.RaisePropertyChanged(nameof(IsOwner));
            }
        }
        public bool IsOwner
        {
            get => OwnerAddress == AccountNumber;
        }
        public HomeViewModel(Web3 w3, IConfiguration config)
        {
            W3 = w3;
            ContractAddress = config["Roulette:Address"];
            IsDeployed = bool.Parse(config["Roulette:IsDeployed"] ?? throw new InvalidDataException());
            SubscriptionId = ulong.Parse(config["VRF:SubscriptionId"] ?? throw new InvalidDataException());
            Deploy = ReactiveCommand.CreateFromTask(DoDeploy);
            Load = ReactiveCommand.CreateFromTask(DoLoad);
        }
        protected async Task DoLoad()
        {
            try
            {
                if(IsDeployed)
                {
                    IsLoading = true;
                    WheelService srv = new WheelService(W3, ContractAddress ?? throw new InvalidDataException());
                    OwnerAddress = await srv.OwnerQueryAsync(new OwnerFunction()
                    {
                        FromAddress = AccountNumber
                    });
                    try
                    {
                        var acctDto = await srv.GetAccountQueryAsync(new GetAccountFunction()
                        {
                            FromAddress = AccountNumber
                        });

                    }
                    catch { }
                }
                
            }
            catch (Exception ex)
            {
                await Alert.Handle(ex.Message).GetAwaiter();
            }
            finally
            {
                IsLoading = false;
            }
        }
        protected async Task DoDeploy()
        {
            try
            {
                if (!IsDeployed)
                {
                    IsLoading = true;
                    var rcpt = await WheelService.DeployContractAndWaitForReceiptAsync(W3, new WheelDeployment()
                    {
                        SubscriptionId = SubscriptionId
                    });
                    ContractAddress = rcpt.ContractAddress;
                    IsDeployed = true;
                }
            }
            catch(Exception ex)
            {
                await Alert.Handle(ex.Message).GetAwaiter();
            }
            finally
            {
                IsLoading = false;
            }
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }
                disposable.Dispose();
                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~HomeViewModel()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
