using DynamicData;
using DynamicData.Binding;
using Microsoft.Extensions.Configuration;
using Nethereum.Contracts;
using Nethereum.Util;
using Nethereum.Web3;
using ReactiveUI;
using RedOrBlack.Contracts.Wheel;
using RedOrBlack.Contracts.Wheel.ContractDefinition;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
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
        public BigInteger Gas { get; set; }
    }
    public class OwnerViewModel : ReactiveObject
    {
        protected HomeViewModel Root { get; }
        public ReactiveCommand<Unit, Unit> Fund { get; }
        public ReactiveCommand<Unit, Unit> Widthdraw { get; }
        public ReactiveCommand<Unit, Unit> Load;
        private decimal amount;
        public decimal Amount
        {
            get => amount;
            set => this.RaiseAndSetIfChanged(ref amount, value);
        }
        private decimal currentBalance;
        public decimal CurrentBalance
        {
            get => currentBalance;
            set => this.RaiseAndSetIfChanged(ref currentBalance, value);
        }
        public OwnerViewModel(HomeViewModel root)
        {
            Root = root;
            Fund = ReactiveCommand.CreateFromTask(DoFund);
            Widthdraw = ReactiveCommand.CreateFromTask(DoWithdrawl);
            Load = ReactiveCommand.CreateFromTask(DoLoad);
        }
        protected async Task DoLoad()
        {
            try
            {
                var srv = new WheelService(Root.W3, Root.ContractAddress ?? throw new InvalidDataException());
                var balance = await srv.CurrentBalanceQueryAsync(new CurrentBalanceFunction()
                {
                    FromAddress = Root.AccountNumber
                });
                CurrentBalance = UnitConversion.Convert.FromWei(balance, UnitConversion.EthUnit.Ether);
            }
            catch (Exception ex)
            {
                await Root.Alert.Handle(ex.Message).GetAwaiter();
            }
        }
        protected async Task DoFund()
        {
            try
            {
                if (!Root.IsOwner || amount <= 0)
                    return;
                var tx = new FundFunction()
                {
                    AmountToSend = UnitConversion.Convert.ToWei(Amount, UnitConversion.EthUnit.Ether),
                    Gas = 150000,
                    FromAddress = Root.OwnerAddress
                };
                var data = Convert.ToHexString(tx.GetCallData()).ToLower();
                var signedData = await Root.SignatureRequest.Handle(new TransactionData()
                {
                    ContractAddress = Root.ContractAddress ?? throw new InvalidDataException(),
                    TXData = data,
                    Value = tx.AmountToSend,
                    Gas = tx.Gas ?? throw new InvalidDataException()

                }).GetAwaiter();
                var str = await Root.W3.Eth.Transactions.SendRawTransaction.SendRequestAsync(signedData);
            }
            catch (Exception ex) 
            {
                await Root.Alert.Handle(ex.Message).GetAwaiter();
            }
        }
        protected async Task DoWithdrawl()
        {
            try
            {
                if (!Root.IsOwner || Amount <= 0)
                    return;
                var tx = new WidthdrawFundFunction()
                {
                    AmountToSend = BigInteger.Zero,
                    Gas = 150000,
                    FromAddress = Root.OwnerAddress,
                    Amount = UnitConversion.Convert.ToWei(Amount, UnitConversion.EthUnit.Ether)
                };
                var data = Convert.ToHexString(tx.GetCallData()).ToLower();
                var signedData = await Root.SignatureRequest.Handle(new TransactionData()
                {
                    ContractAddress = Root.ContractAddress ?? throw new InvalidDataException(),
                    TXData = data,
                    Value = tx.AmountToSend,
                    Gas = tx.Gas ?? throw new InvalidDataException()

                }).GetAwaiter();
                var str = await Root.W3.Eth.Transactions.SendRawTransaction.SendRequestAsync(signedData);
            }
            catch (Exception ex)
            {
                await Root.Alert.Handle(ex.Message).GetAwaiter();
            }
        }
    }
    public class HomeViewModel : ReactiveObject, IDisposable
    {
        public Interaction<string, bool> Alert { get; } = new Interaction<string, bool>();
        public Interaction<TransactionData, string> SignatureRequest { get; } = new Interaction<TransactionData, string>();
        public ReactiveCommand<Unit, Unit> Load { get; }
        public ReactiveCommand<Unit, Unit> Deploy { get; }
        public ReactiveCommand<Unit, Unit> OpenBets { get; }
        public ReactiveCommand<Unit, Unit> Spin { get; }
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
        private bool hasAccount;
        public bool HasAccount
        {
            get => hasAccount;
            set => this.RaiseAndSetIfChanged(ref hasAccount, value);
        }
        public bool IsOwner
        {
            get => OwnerAddress?.ToLower() == AccountNumber?.ToLower();
        }
        private AccountViewModel? currentAccount;
        public AccountViewModel? CurrentAccount
        {
            get => currentAccount;
            set => this.RaiseAndSetIfChanged(ref currentAccount, value);
        }
        private OwnerViewModel? ownerVM;
        public OwnerViewModel? OwnerVM
        {
            get => ownerVM;
            set => this.RaiseAndSetIfChanged(ref ownerVM, value);
        }
        private bool isOpenForWithdrawl;
        public bool IsOpenForWithdrawl
        {
            get => isOpenForWithdrawl;
            set => this.RaiseAndSetIfChanged(ref isOpenForWithdrawl, value);
        }
        private WheelViewModel? wheelVM;
        public WheelViewModel? WheelVM
        {
            get => wheelVM;
            set => this.RaiseAndSetIfChanged(ref wheelVM, value);
        }
        public HomeViewModel(Web3 w3, IConfiguration config)
        {
            W3 = w3;
            ContractAddress = config["Roulette:Address"];
            IsDeployed = bool.Parse(config["Roulette:IsDeployed"] ?? throw new InvalidDataException());
            SubscriptionId = ulong.Parse(config["VRF:SubscriptionId"] ?? throw new InvalidDataException());
            Deploy = ReactiveCommand.CreateFromTask(DoDeploy);
            Load = ReactiveCommand.CreateFromTask(DoLoad);
            Spin = ReactiveCommand.CreateFromTask(DoSpin);
            OpenBets = ReactiveCommand.CreateFromTask(DoOpenBets);
            this.WhenPropertyChanged(p => p.AccountNumber).Subscribe(async p =>
            {
                if (!string.IsNullOrWhiteSpace(p.Value))
                    await DoLoad();
            }).DisposeWith(disposable);
        }
        protected async Task DoSpin()
        {
            try
            {
                if (IsDeployed)
                {
                    WheelService srv = new WheelService(W3, ContractAddress ?? throw new InvalidDataException());
                    await srv.SpinTheWheelRequestAndWaitForReceiptAsync(new SpinTheWheelFunction()
                    {
                        FromAddress = OwnerAddress ?? throw new InvalidDataException(),
                        Gas = 1500000
                    });
                    
                }
            }
            catch (Exception ex)
            {
                await Alert.Handle(ex.Message).GetAwaiter();
            }
        }
        protected async Task DoOpenBets()
        {
            try
            {
                if (IsDeployed)
                {
                    WheelService srv = new WheelService(W3, ContractAddress ?? throw new InvalidDataException());
                    await srv.OpenBetsRequestAndWaitForReceiptAsync();

                }
            }
            catch (Exception ex)
            {
                await Alert.Handle(ex.Message).GetAwaiter();
            }
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
                        CurrentAccount = new AccountViewModel(this, acctDto.ReturnValue1);
                        HasAccount = true;
                    }
                    catch 
                    {
                        HasAccount = false;
                        CurrentAccount = new AccountViewModel(this);
                    }
                    OwnerVM = new OwnerViewModel(this);
                    await OwnerVM.Load.Execute().GetAwaiter();
                    IsOpenForWithdrawl = await srv.IsOpenForWithdrawlQueryAsync();
                    var numbersDTO = await srv.GetNumbersQueryAsync();
                    WheelVM = new WheelViewModel(this, numbersDTO.Ns);
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
    public class WheelViewModel : ReactiveObject
    {
        protected HomeViewModel Root { get; }
        public ObservableCollection<NumberViewModel> Numbers { get; } = new ObservableCollection<NumberViewModel>();
        public WheelViewModel(HomeViewModel root, IEnumerable<Number> numbers)
        {
            Root = root;
            Numbers.AddRange(numbers.Select(n => new NumberViewModel(root, this, n)));
        }
    }
    public class NumberViewModel : ReactiveObject
    {
        public enum NumberColors : byte
        {
            green,
            red,
            black
        }
        public Number Data { get; }
        protected HomeViewModel Root { get; }
        protected WheelViewModel Wheel { get; }
        public NumberColors Color { get => (NumberColors)Data.Color; }
        public NumberViewModel(HomeViewModel root, WheelViewModel wheel, Number data)
        {
            Data = data;
            Root = root;
            Wheel = wheel;
        }
    }
    public class AccountViewModel : ReactiveObject
    {
        private bool isOpen;
        public bool IsOpen
        {
            get => isOpen;
            set => this.RaiseAndSetIfChanged(ref isOpen, value);
        }
        protected HomeViewModel Root { get; }
        private string? nick;
        [Required]
        public string? Nick
        {
            get => nick;
            set => this.RaiseAndSetIfChanged(ref nick, value);
        }
        private string? owner;
        public string? Owner
        {
            get => owner;
            set => this.RaiseAndSetIfChanged(ref owner, value);
        }
        private decimal val;
        public decimal Value
        {
            get => val;
            set => this.RaiseAndSetIfChanged(ref val, value);
        }
        private decimal? changeValue;
        public decimal? ChangeValue
        {
            get => changeValue;
            set => this.RaiseAndSetIfChanged(ref changeValue, value);
        }
        public bool IsNew { get; } = true;
        public ReactiveCommand<Unit, Unit> OpenAccount { get; }
        public ReactiveCommand<Unit, Unit> FundAccount { get; }
        public ReactiveCommand<Unit, Unit> Withdraw { get; }
        public AccountViewModel(HomeViewModel root)
        {
            Root = root;
            OpenAccount = ReactiveCommand.CreateFromTask(DoOpenAccount);
            FundAccount = ReactiveCommand.CreateFromTask(DoFundAccount);
            Withdraw = ReactiveCommand.CreateFromTask(DoWithdraw);
        }
        public AccountViewModel(HomeViewModel root, Account acct) :this(root)
        {
            IsNew = false;
            Nick = acct.Nick;
            Owner = acct.Owner;
            Value = UnitConversion.Convert.FromWei(acct.Value, UnitConversion.EthUnit.Ether);
        }
        protected async Task DoOpenAccount()
        {
            if (IsNew && Value > 0)
            {
                try
                {
                    var tx = new OpenAccountFunction()
                    {
                        AmountToSend = UnitConversion.Convert.ToWei(Value, UnitConversion.EthUnit.Ether),
                        FromAddress = Root.AccountNumber ?? throw new InvalidDataException(),
                        Nick = Nick ?? throw new InvalidDataException(),
                        Gas = 150000
                    };
                    var data = Convert.ToHexString(tx.GetCallData()).ToLower();
                    var signedData = await Root.SignatureRequest.Handle(new TransactionData()
                    {
                        ContractAddress = Root.ContractAddress ?? throw new InvalidDataException(),
                        TXData = data,
                        Value = tx.AmountToSend,
                        Gas = tx.Gas ?? throw new InvalidDataException()

                    }).GetAwaiter();
                    var str = await Root.W3.Eth.Transactions.SendRawTransaction.SendRequestAsync(signedData);
                }
                catch(Exception ex)
                {
                    await Root.Alert.Handle(ex.Message).GetAwaiter();
                }
            }
        }
        protected async Task DoFundAccount()
        {
            if(!IsNew && ChangeValue > 0)
            {
                try
                {
                    var tx = new FundAccountFunction()
                    {
                        AmountToSend = UnitConversion.Convert.ToWei(ChangeValue ?? throw new InvalidDataException(), UnitConversion.EthUnit.Ether),
                        FromAddress = Root.AccountNumber ?? throw new InvalidDataException(),
                        Gas = 150000
                    };
                    var data = Convert.ToHexString(tx.GetCallData()).ToLower();
                    var signedData = await Root.SignatureRequest.Handle(new TransactionData()
                    {
                        ContractAddress = Root.ContractAddress ?? throw new InvalidDataException(),
                        TXData = data,
                        Value = tx.AmountToSend,
                        Gas = tx.Gas ?? throw new InvalidDataException()

                    }).GetAwaiter();
                    var str = await Root.W3.Eth.Transactions.SendRawTransaction.SendRequestAsync(signedData);
                }
                catch (Exception ex)
                {
                    await Root.Alert.Handle(ex.Message).GetAwaiter();
                }
            }
        }
        protected async Task DoWithdraw()
        {
            if (!IsNew && ChangeValue > 0)
            {
                try
                {
                    var tx = new WithdrawFunction()
                    {
                        AmountToSend = BigInteger.Zero,
                        Amount = UnitConversion.Convert.ToWei(ChangeValue ?? throw new InvalidDataException(), UnitConversion.EthUnit.Ether),
                        FromAddress = Root.AccountNumber ?? throw new InvalidDataException(),
                        Gas = 150000
                    };
                    var data = Convert.ToHexString(tx.GetCallData()).ToLower();
                    var signedData = await Root.SignatureRequest.Handle(new TransactionData()
                    {
                        ContractAddress = Root.ContractAddress ?? throw new InvalidDataException(),
                        TXData = data,
                        Value = tx.AmountToSend,
                        Gas = tx.Gas ?? throw new InvalidDataException()

                    }).GetAwaiter();
                    var str = await Root.W3.Eth.Transactions.SendRawTransaction.SendRequestAsync(signedData);
                }
                catch (Exception ex)
                {
                    await Root.Alert.Handle(ex.Message).GetAwaiter();
                }
            }
        }
    }
}
