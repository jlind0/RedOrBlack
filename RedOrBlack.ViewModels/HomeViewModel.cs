using DynamicData;
using DynamicData.Binding;
using Microsoft.Extensions.Configuration;
using Nethereum.Contracts;
using Nethereum.Util;
using Nethereum.Web3;
using ReactiveUI;
using RedOrBlack.Contracts.Wheel;
using RedOrBlack.Contracts.Wheel.ContractDefinition;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Numerics;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

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
        private DateTime timeToSpin;
        public DateTime TimeToSpin
        {
            get => timeToSpin;
            set => this.RaiseAndSetIfChanged(ref timeToSpin, value);
        }
        public Dictionary<string, AccountViewModel> Accounts { get; } = new Dictionary<string, AccountViewModel>();
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
                        Gas = 6000000
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
                if (IsDeployed)
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
                    var acctDTO = await srv.GetAccountsQueryAsync();
                    Accounts.Clear();
                    foreach (var acct in acctDTO.ReturnValue1)
                        Accounts.Add(acct.Owner, new AccountViewModel(this, acct));
                    OwnerVM = new OwnerViewModel(this);
                    await OwnerVM.Load.Execute().GetAwaiter();
                    IsOpenForWithdrawl = await srv.IsOpenForWithdrawlQueryAsync();
                    var numbersDTO = await srv.GetNumbersQueryAsync();
                    WheelVM = new WheelViewModel(this, numbersDTO.Ns);
                    await WheelVM.LoadBets.Execute().GetAwaiter();
                    try
                    {
                        var spin = await srv.GetCurrentSpinQueryAsync();
                        TimeToSpin = spin.ReturnValue1.StartTime.FromUnixTimestamp().AddMinutes(5);
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
            catch (Exception ex)
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
    public static class DateTimeExtensions
    {
        public static DateTime FromUnixTimestamp(this BigInteger timestamp)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return dateTime.AddSeconds(((long)timestamp));
        }
    }
    public enum BetType : byte
    {
        StraightUp,
        Split,
        Street,
        Corner,
        TopLine,
        DoubleStreet,
        Column,
        Dozen,
        Color,
        Eighteen,
        Parity
    }
    public enum NumberParity : byte
    {
        Green,
        Even,
        Odd
    }
    public enum NumberColor : byte
    {
        Green,
        Red,
        Black
    }
    public class PlacedBetViewModel : ReactiveObject
    {
        protected HomeViewModel Root { get; }
        protected WheelViewModel WheelVM { get; }
        protected Bet Data { get; }
        public AccountViewModel Account { get; }
        public BetType Type { get => (BetType)Data.BetType; }
        public string SpinId { get => Data.SpinId.ToString(); }
        public decimal Amount { get => UnitConversion.Convert.FromWei(Data.Amount); }
        public Number? Number { get => Type == BetType.StraightUp ? WheelVM.NumbersById[Data.Number] : null; }
        public int? Row { get => Type == BetType.Street || Type == BetType.DoubleStreet ? Data.Row : null; }
        public int? Column { get => Type == BetType.Column ? Data.Column : null; }
        public NumberColor? Color { get => Type == BetType.Color ? (NumberColor)Data.Color : null; }
        public NumberParity? Parity { get => Type == BetType.Parity ? (NumberParity)Data.Parity : null; }
        public Number[]? Numbers { get => Type == BetType.TopLine || Type == BetType.Split || Type == BetType.Corner ?
                WheelVM.NumbersById.Where(n => Data.Numbers.Any(nn => nn == n.Key)).Select(n => n.Value).ToArray() : null;
        }
        public int? WhichDozen { get => Type == BetType.Dozen ? Data.ByTheDozen : null; }
        public int? Which18 { get => Type == BetType.Eighteen ? Data.ByThe18 : null; }
        public PlacedBetViewModel(HomeViewModel root, WheelViewModel wheel, Bet data)
        {
            Root = root;
            WheelVM = wheel;
            Data = data;
            Account = root.Accounts[data.Account];
        }
    }
    public class WheelViewModel : ReactiveObject
    {
        protected HomeViewModel Root { get; }
        public ObservableCollection<PlacedBetViewModel> Bets { get; } = new ObservableCollection<PlacedBetViewModel>();
        public Dictionary<BigInteger, Number> NumbersById { get; } = new Dictionary<BigInteger, Number>();
        public Dictionary<string, Number> Numbers { get; } = new Dictionary<string, Number>();
        public SplitBetViewModel SplitBetVM { get; }
        public StreetBetViewModel StreetBetVM { get; }
        public DoubleStreetBetViewModel DoubleStreetVM { get; }
        public TopLineBetViewModel TopLineVM { get; }
        public CornerBetViewModel CornerVM { get; }
        public StraightUpBetViewModel StraightUpVM { get; }
        public ColumnBetViewModel ColumnVM { get; }
        public DozenBetViewModel DozenVM { get; }
        public ReactiveCommand<Unit, Unit> LoadBets { get; }
        public EighteenBetViewModel EighteenVM { get; }
        public ParityBetViewModel ParityVM { get; }
        public ColorBetViewModel ColorVM { get; }
        public WheelViewModel(HomeViewModel root, IEnumerable<Number> numbers)
        {
            Root = root;
            foreach (var n in numbers)
            {
                NumbersById.Add(n.Id, n);
                Numbers.Add(n.Name, n);
            }
            SplitBetVM = new SplitBetViewModel(this, Root);
            StreetBetVM = new StreetBetViewModel(this, Root);
            DoubleStreetVM = new DoubleStreetBetViewModel(this, Root);
            TopLineVM = new TopLineBetViewModel(this, Root);
            CornerVM = new CornerBetViewModel(this, Root);
            StraightUpVM = new StraightUpBetViewModel(this, Root);
            ColumnVM = new ColumnBetViewModel(this, Root);
            DozenVM = new DozenBetViewModel(this, Root);
            EighteenVM = new EighteenBetViewModel(this, Root);
            ColorVM = new ColorBetViewModel(this, Root);
            LoadBets = ReactiveCommand.CreateFromTask(DoLoadBets);
            ParityVM = new ParityBetViewModel(this, Root);
        }
        protected async Task DoLoadBets()
        {
            try
            {
                WheelService svc = new WheelService(Root.W3, Root.ContractAddress ?? throw new InvalidDataException());
                Bets.Clear();
                var betsDto = await svc.GetCurrentBetsQueryAsync();
                Bets.AddRange(betsDto.ReturnValue1.Select(b => new PlacedBetViewModel(Root, this, b)));

            }
            catch
            {
                //await Root.Alert.Handle(ex.Message).GetAwaiter();
            }
        }
    }
    public abstract class BetViewModel<TParam> : ReactiveObject
    {
        private bool isOpen;
        public bool IsOpen
        {
            get => isOpen;
            set => this.RaiseAndSetIfChanged(ref isOpen, value);
        }
        private decimal amount;
        public decimal Amount
        {
            get => amount;
            set => this.RaiseAndSetIfChanged(ref amount, value);
        }
        private TParam? val;
        public virtual TParam? Value
        {
            get => val;
            set => this.RaiseAndSetIfChanged(ref val, value);
        }
        public ReactiveCommand<Unit, Unit> Bet { get; }
        public ReactiveCommand<TParam, Unit> Open { get; }
        protected WheelViewModel WheelVM { get; }
        protected HomeViewModel Root { get; }
        public BetViewModel(WheelViewModel wheelVM, HomeViewModel root)
        {
            WheelVM = wheelVM;
            Root = root;
            Bet = ReactiveCommand.CreateFromTask(DoBet);
            Open = ReactiveCommand.Create<TParam>(DoOpen);
        }
        protected virtual void DoOpen(TParam value)
        {
            Value = value;
            IsOpen = true;
        }
        protected abstract Task DoBet();
    }
    public class StreetBetViewModel : BetViewModel<byte>
    {
        public override byte Value
        {
            set { base.Value = value; this.RaisePropertyChanged(nameof(Numbers)); }
        }
        public Number[] Numbers
        {
            get => WheelVM.Numbers.Values.Where(n => n.Row == Value).ToArray();
        }
        public StreetBetViewModel(WheelViewModel wheelVM, HomeViewModel root) : base(wheelVM, root)
        {
        }

        protected override async Task DoBet()
        {
            try
            {
                PlaceStreetBetFunction tx = new PlaceStreetBetFunction()
                {
                    FromAddress = Root.AccountNumber ?? throw new InvalidDataException(),
                    Gas = 1500000,
                    Street = Value,
                    Amount = UnitConversion.Convert.ToWei(Amount, UnitConversion.EthUnit.Ether)
                };
                var data = Convert.ToHexString(tx.GetCallData()).ToLower();
                var signedData = await Root.SignatureRequest.Handle(new TransactionData()
                {
                    ContractAddress = Root.ContractAddress ?? throw new InvalidDataException(),
                    TXData = data,
                    Gas = tx.Gas ?? throw new InvalidDataException()

                }).GetAwaiter();
                await Root.W3.Eth.Transactions.SendRawTransaction.SendRequestAsync(signedData);
            }
            catch(Exception ex)
            {
                await Root.Alert.Handle(ex.Message).GetAwaiter();
            }
        }
    }
    public class ColumnBetViewModel : BetViewModel<byte>
    {
        public ColumnBetViewModel(WheelViewModel wheelVM, HomeViewModel root) : base(wheelVM, root)
        {
        }

        protected override async Task DoBet()
        {
            try
            {
                PlaceColumnBetFunction tx = new PlaceColumnBetFunction()
                {
                    FromAddress = Root.AccountNumber ?? throw new InvalidDataException(),
                    Gas = 1500000,
                    Column = Value,
                    Amount = UnitConversion.Convert.ToWei(Amount, UnitConversion.EthUnit.Ether)
                };
                var data = Convert.ToHexString(tx.GetCallData()).ToLower();
                var signedData = await Root.SignatureRequest.Handle(new TransactionData()
                {
                    ContractAddress = Root.ContractAddress ?? throw new InvalidDataException(),
                    TXData = data,
                    Gas = tx.Gas ?? throw new InvalidDataException()

                }).GetAwaiter();
                await Root.W3.Eth.Transactions.SendRawTransaction.SendRequestAsync(signedData);
            }
            catch (Exception ex)
            {
                await Root.Alert.Handle(ex.Message).GetAwaiter();
            }
        }
    }
    public class ParityBetViewModel : BetViewModel<NumberParity>
    {
        public ParityBetViewModel(WheelViewModel wheelVM, HomeViewModel root) : base(wheelVM, root)
        {
        }

        protected override async Task DoBet()
        {
            try
            {
                PlaceParityBetFunction tx = new PlaceParityBetFunction()
                {
                    FromAddress = Root.AccountNumber ?? throw new InvalidDataException(),
                    Gas = 1500000,
                    Parity = (byte)Value,
                    Amount = UnitConversion.Convert.ToWei(Amount, UnitConversion.EthUnit.Ether)
                };
                var data = Convert.ToHexString(tx.GetCallData()).ToLower();
                var signedData = await Root.SignatureRequest.Handle(new TransactionData()
                {
                    ContractAddress = Root.ContractAddress ?? throw new InvalidDataException(),
                    TXData = data,
                    Gas = tx.Gas ?? throw new InvalidDataException()

                }).GetAwaiter();
                await Root.W3.Eth.Transactions.SendRawTransaction.SendRequestAsync(signedData);
            }
            catch (Exception ex)
            {
                await Root.Alert.Handle(ex.Message).GetAwaiter();
            }
        }
    }
    public class ColorBetViewModel : BetViewModel<NumberColor>
    {
        public ColorBetViewModel(WheelViewModel wheelVM, HomeViewModel root) : base(wheelVM, root)
        {
        }

        protected override async Task DoBet()
        {
            try
            {
                PlaceColorBetFunction tx = new PlaceColorBetFunction()
                {
                    FromAddress = Root.AccountNumber ?? throw new InvalidDataException(),
                    Gas = 1500000,
                    Color = (byte)Value,
                    Amount = UnitConversion.Convert.ToWei(Amount, UnitConversion.EthUnit.Ether)
                };
                var data = Convert.ToHexString(tx.GetCallData()).ToLower();
                var signedData = await Root.SignatureRequest.Handle(new TransactionData()
                {
                    ContractAddress = Root.ContractAddress ?? throw new InvalidDataException(),
                    TXData = data,
                    Gas = tx.Gas ?? throw new InvalidDataException()

                }).GetAwaiter();
                await Root.W3.Eth.Transactions.SendRawTransaction.SendRequestAsync(signedData);
            }
            catch (Exception ex)
            {
                await Root.Alert.Handle(ex.Message).GetAwaiter();
            }
        }
    }
    public class DozenBetViewModel : BetViewModel<byte>
    {
        public DozenBetViewModel(WheelViewModel wheelVM, HomeViewModel root) : base(wheelVM, root)
        {
        }

        protected override async Task DoBet()
        {
            try
            {
                PlaceDozenBetFunction tx = new PlaceDozenBetFunction()
                {
                    FromAddress = Root.AccountNumber ?? throw new InvalidDataException(),
                    Gas = 1500000,
                    WhichDozen = Value,
                    Amount = UnitConversion.Convert.ToWei(Amount, UnitConversion.EthUnit.Ether)
                };
                var data = Convert.ToHexString(tx.GetCallData()).ToLower();
                var signedData = await Root.SignatureRequest.Handle(new TransactionData()
                {
                    ContractAddress = Root.ContractAddress ?? throw new InvalidDataException(),
                    TXData = data,
                    Gas = tx.Gas ?? throw new InvalidDataException()

                }).GetAwaiter();
                await Root.W3.Eth.Transactions.SendRawTransaction.SendRequestAsync(signedData);
            }
            catch (Exception ex)
            {
                await Root.Alert.Handle(ex.Message).GetAwaiter();
            }
        }
    }
    public class EighteenBetViewModel : BetViewModel<byte>
    {
        public EighteenBetViewModel(WheelViewModel wheelVM, HomeViewModel root) : base(wheelVM, root)
        {
        }

        protected override async Task DoBet()
        {
            try
            {
                PlaceDozenBetFunction tx = new PlaceDozenBetFunction()
                {
                    FromAddress = Root.AccountNumber ?? throw new InvalidDataException(),
                    Gas = 1500000,
                    WhichDozen = Value,
                    Amount = UnitConversion.Convert.ToWei(Amount, UnitConversion.EthUnit.Ether)
                };
                var data = Convert.ToHexString(tx.GetCallData()).ToLower();
                var signedData = await Root.SignatureRequest.Handle(new TransactionData()
                {
                    ContractAddress = Root.ContractAddress ?? throw new InvalidDataException(),
                    TXData = data,
                    Gas = tx.Gas ?? throw new InvalidDataException()

                }).GetAwaiter();
                await Root.W3.Eth.Transactions.SendRawTransaction.SendRequestAsync(signedData);
            }
            catch (Exception ex)
            {
                await Root.Alert.Handle(ex.Message).GetAwaiter();
            }
        }
    }
    public class DoubleStreetBetViewModel : BetViewModel<byte>
    {
        public override byte Value
        {
            set { base.Value = value; this.RaisePropertyChanged(nameof(Numbers)); }
        }
        public Number[] Numbers
        {
            get => WheelVM.Numbers.Values.Where(n => n.Row == Value || n.Row == Value + 1).ToArray();
        }
        public DoubleStreetBetViewModel(WheelViewModel wheelVM, HomeViewModel root) : base(wheelVM, root)
        {
        }

        protected override async Task DoBet()
        {
            try
            {
                PlaceDoubleStreetBetFunction tx = new PlaceDoubleStreetBetFunction()
                {
                    FromAddress = Root.AccountNumber ?? throw new InvalidDataException(),
                    Gas = 1500000,
                    Street = Value,
                    Amount = UnitConversion.Convert.ToWei(Amount, UnitConversion.EthUnit.Ether)
                };
                var data = Convert.ToHexString(tx.GetCallData()).ToLower();
                var signedData = await Root.SignatureRequest.Handle(new TransactionData()
                {
                    ContractAddress = Root.ContractAddress ?? throw new InvalidDataException(),
                    TXData = data,
                    Gas = tx.Gas ?? throw new InvalidDataException()

                }).GetAwaiter();
                await Root.W3.Eth.Transactions.SendRawTransaction.SendRequestAsync(signedData);
            }
            catch (Exception ex)
            {
                await Root.Alert.Handle(ex.Message).GetAwaiter();
            }
        }
    }
    public class SplitBetViewModel : BetViewModel<string>
    {
        public SplitBetViewModel(WheelViewModel wheelVM, HomeViewModel homeVM) :base(wheelVM, homeVM)
        {

        } 
        protected override async Task DoBet()
        {
            try
            {
                if (Value == null)
                    throw new InvalidDataException();
                string[] ns = Value.Split('-');
                if (ns.Length != 2)
                    throw new InvalidDataException();
                var n1 = WheelVM.Numbers[ns[0]];
                var n2 = WheelVM.Numbers[ns[1]];
                PlaceSplitBetFunction tx = new PlaceSplitBetFunction()
                {
                    FromAddress = Root.AccountNumber ?? throw new InvalidDataException(),
                    Gas = 1500000,
                    Number1 = n1.Id,
                    Number2 = n2.Id,
                    Amount = UnitConversion.Convert.ToWei(Amount, UnitConversion.EthUnit.Ether)
                };
                var data = Convert.ToHexString(tx.GetCallData()).ToLower();
                var signedData = await Root.SignatureRequest.Handle(new TransactionData()
                {
                    ContractAddress = Root.ContractAddress ?? throw new InvalidDataException(),
                    TXData = data,
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
    public class CornerBetViewModel : BetViewModel<string>
    {
        public CornerBetViewModel(WheelViewModel wheelVM, HomeViewModel homeVM) : base(wheelVM, homeVM)
        {

        }
        protected override async Task DoBet()
        {
            try
            {
                if (Value == null)
                    throw new InvalidDataException();
                string[] ns = Value.Split('-');
                if (ns.Length != 4)
                    throw new InvalidDataException();
                var n1 = WheelVM.Numbers[ns[0]];
                var n2 = WheelVM.Numbers[ns[1]];
                var n3 = WheelVM.Numbers[ns[2]];
                var n4 = WheelVM.Numbers[ns[3]];
                PlaceCornerBetFunction tx = new PlaceCornerBetFunction()
                {
                    FromAddress = Root.AccountNumber ?? throw new InvalidDataException(),
                    Gas = 1500000,
                    Number1 = n1.Id,
                    Number2 = n2.Id,
                    Number3 = n3.Id,
                    Number4 = n4.Id,
                    Amount = UnitConversion.Convert.ToWei(Amount, UnitConversion.EthUnit.Ether)
                };
                var data = Convert.ToHexString(tx.GetCallData()).ToLower();
                var signedData = await Root.SignatureRequest.Handle(new TransactionData()
                {
                    ContractAddress = Root.ContractAddress ?? throw new InvalidDataException(),
                    TXData = data,
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
    public class StraightUpBetViewModel : BetViewModel<string>
    {
        public StraightUpBetViewModel(WheelViewModel wheelVM, HomeViewModel homeVM) : base(wheelVM, homeVM)
        {

        }
        protected override async Task DoBet()
        {
            try
            {
                if (Value == null)
                    throw new InvalidDataException();
                var n1 = WheelVM.Numbers[Value];
                PlaceStraightUpBetFunction tx = new PlaceStraightUpBetFunction()
                {
                    FromAddress = Root.AccountNumber ?? throw new InvalidDataException(),
                    Gas = 1500000,
                    NumberId = n1.Id,
                    Amount = UnitConversion.Convert.ToWei(Amount, UnitConversion.EthUnit.Ether)
                };
                var data = Convert.ToHexString(tx.GetCallData()).ToLower();
                var signedData = await Root.SignatureRequest.Handle(new TransactionData()
                {
                    ContractAddress = Root.ContractAddress ?? throw new InvalidDataException(),
                    TXData = data,
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
    public class TopLineBetViewModel : BetViewModel<Unit>
    {
        public TopLineBetViewModel(WheelViewModel wheelVM, HomeViewModel homeVM) : base(wheelVM, homeVM)
        {

        }
        protected override async Task DoBet()
        {
            try
            {
                PlaceTopLineBetFunction tx = new PlaceTopLineBetFunction()
                {
                    FromAddress = Root.AccountNumber ?? throw new InvalidDataException(),
                    Gas = 1500000,
                    Amount = UnitConversion.Convert.ToWei(Amount, UnitConversion.EthUnit.Ether)
                };
                var data = Convert.ToHexString(tx.GetCallData()).ToLower();
                var signedData = await Root.SignatureRequest.Handle(new TransactionData()
                {
                    ContractAddress = Root.ContractAddress ?? throw new InvalidDataException(),
                    TXData = data,
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
