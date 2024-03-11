// SPDX-License-Identifier: MIT
pragma solidity ^0.8.7;

import "@openzeppelin/contracts/utils/math/Math.sol";
import "@chainlink/contracts/src/v0.8/interfaces/VRFCoordinatorV2Interface.sol";
import "@chainlink/contracts/src/v0.8/vrf/VRFConsumerBaseV2.sol";
import "@openzeppelin/contracts/access/Ownable.sol";
import "@openzeppelin/contracts/utils/ReentrancyGuard.sol";

contract Wheel is VRFConsumerBaseV2, Ownable, ReentrancyGuard{
    uint64 s_subscriptionId;
    VRFCoordinatorV2Interface COORDINATOR;
    address constant vrfCoordinator = 0x8103B0A8A00be2DDC778e6e7eaa21791Cd364625;
    bytes32 constant s_keyHash = 0x474e34a077df58807dbe9c96d3c009b23b3c6d0cce433e59bbf5b34f823bc56c;
    uint32 constant callbackGasLimit = 1000000;
    uint16 constant requestConfirmations = 3;
    uint[] numberIds;
    mapping(uint => Number) numbers;
    mapping(NumberParity => Number[]) numbersByParity;
    mapping(NumberColor => Number[]) numbersByColor;
    mapping(uint8 => Number[]) numbersByRow;
    mapping(uint8 => Number[]) numbersByColumn;
    mapping(uint8 => Number[]) numbersByDozen;
    mapping(uint8 => Number[]) numbersBy18;
    uint public mindeposit = 0.05 ether;
    uint public minbetinside = 0.0001 ether;
    uint public minbetoutside = 0.001 ether;
    mapping(address => Account) accounts;
    address[] accountAddresses;
    bool public isOpenForWithdrawl = true;
    uint constant timeBetweenBets = 3 minutes;
    uint constant timeForBet = 5 minutes;
    mapping(uint => Spin) spins;
    uint[] spinIds;
    uint public currentBalance;
    struct Spin{
        uint spinId;
        uint startTime;
        bool hasSpun;
        uint spunNumberId;
        uint spunTime;
    }
    mapping(uint => Bet[]) spinBets;
    struct Bet{
        address account;
        uint spinId;
        uint amount;
        BetType betType;
        uint number;
        uint8 row;
        uint8 column;
        NumberColor color;
        NumberParity parity;
        uint[] numbers;
        uint8 byTheDozen;
        uint8 byThe18;
    }
    mapping(BetType => uint8) payouts; 
    enum BetType{
        StraightUp,
        Row,
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
    struct Number{
        uint id;
        string name;
        NumberParity parity;
        NumberColor color;
        uint8 row;
        uint8 column;
        uint8 which18;
        uint8 whichDozen;
    }
    enum NumberParity{
        Green,
        Even,
        Odd
    }
    enum NumberColor{
        Green,
        Red,
        Black
    }
    struct Account{
        address owner;
        string nick;
        uint value;
    }
     constructor(uint64 subscriptionId) VRFConsumerBaseV2(vrfCoordinator) Ownable(msg.sender) {
        COORDINATOR = VRFCoordinatorV2Interface(vrfCoordinator);
        s_subscriptionId = subscriptionId;
        addNumber("0", NumberParity.Green, NumberColor.Green, 0, 0, 0, 0);
        addNumber("00", NumberParity.Green, NumberColor.Green, 0, 1, 0 ,0);
        addNumber("1", NumberParity.Odd, NumberColor.Red, 1, 0, 1, 1);
        addNumber("2", NumberParity.Even, NumberColor.Black, 1, 1, 1, 1);
        addNumber("3", NumberParity.Odd, NumberColor.Red, 1, 2, 1, 1);
        addNumber("4", NumberParity.Even, NumberColor.Black, 2, 0, 1, 1);
        addNumber("5", NumberParity.Odd, NumberColor.Red, 2, 1, 1, 1);
        addNumber("6", NumberParity.Even, NumberColor.Black, 2, 2, 1, 1);
        addNumber("7", NumberParity.Odd, NumberColor.Red, 3, 0, 1, 1);
        addNumber("8", NumberParity.Even, NumberColor.Black, 3, 1, 1, 1);
        addNumber("9", NumberParity.Odd, NumberColor.Red, 3, 2, 1, 1);
        addNumber("10", NumberParity.Even, NumberColor.Black, 4, 0, 1, 1);
        addNumber("11", NumberParity.Odd, NumberColor.Red, 4, 1, 1, 1);
        addNumber("12", NumberParity.Even, NumberColor.Black, 4, 2, 1, 1);
        addNumber("13", NumberParity.Odd, NumberColor.Red, 5, 0, 1, 2);
        addNumber("14", NumberParity.Even, NumberColor.Black, 5, 1, 1, 2);
        addNumber("15", NumberParity.Odd, NumberColor.Red, 5, 2, 1, 2);
        addNumber("16", NumberParity.Even, NumberColor.Black, 6, 0, 1, 2);
        addNumber("17", NumberParity.Odd, NumberColor.Red, 6, 1, 1, 2);
        addNumber("18", NumberParity.Even, NumberColor.Black, 6, 2, 1, 2);
        addNumber("19", NumberParity.Odd, NumberColor.Red, 7, 0, 2, 2);
        addNumber("20", NumberParity.Even, NumberColor.Black, 7, 1, 2, 2);
        addNumber("21", NumberParity.Odd, NumberColor.Red, 7, 2, 2, 2);
        addNumber("22", NumberParity.Even, NumberColor.Black, 8, 0, 2, 2);
        addNumber("23", NumberParity.Odd, NumberColor.Red, 8, 1, 2, 2);
        addNumber("24", NumberParity.Even, NumberColor.Black, 8, 2, 2, 2);
        addNumber("25", NumberParity.Odd, NumberColor.Red, 9, 0, 2, 3);
        addNumber("26", NumberParity.Even, NumberColor.Black, 9, 1, 2, 3);
        addNumber("27", NumberParity.Odd, NumberColor.Red, 9, 2, 2, 3);
        addNumber("28", NumberParity.Even, NumberColor.Black, 10, 0, 2, 3);
        addNumber("29", NumberParity.Odd, NumberColor.Red, 10, 1, 2, 3);
        addNumber("30", NumberParity.Even, NumberColor.Black, 10, 2, 2, 3);
        addNumber("31", NumberParity.Odd, NumberColor.Red, 11, 0, 2, 3);
        addNumber("32", NumberParity.Even, NumberColor.Black, 11, 1, 2, 3);
        addNumber("33", NumberParity.Odd, NumberColor.Red, 11, 2, 2, 3);
        addNumber("34", NumberParity.Even, NumberColor.Black, 12, 0, 2, 3);
        addNumber("35", NumberParity.Odd, NumberColor.Red, 12, 1, 2, 3);
        addNumber("36", NumberParity.Even, NumberColor.Black, 12, 2, 2, 3);
        payouts[BetType.Color] = 1;
        payouts[BetType.Column] = 2;
        payouts[BetType.Corner] = 8;
        payouts[BetType.DoubleStreet] = 5;
        payouts[BetType.Dozen] = 2;
        payouts[BetType.Eighteen] = 1;
        payouts[BetType.Row] = 17;
        payouts[BetType.Split] = 17;
        payouts[BetType.StraightUp] = 35;
        payouts[BetType.Street] = 11;
        payouts[BetType.TopLine] = 6;
    }
    error AccountExists();
    error AccountNotFound();
    error MoreFundsRequired();
    error InsufficentFunds();
    error TransferFailed();
    error IsNotOpenForWithdrawl();
    error NoSpinsAvailable();
    function openAccount(string memory nick) public payable{
        if(accounts[msg.sender].owner == msg.sender)
            revert AccountExists();
        if(msg.value < mindeposit)
            revert MoreFundsRequired();
        accountAddresses.push(msg.sender);
        accounts[msg.sender] = Account(msg.sender, nick, msg.value);
    }
    function getCurrentSpin() public view returns(Spin memory)
    {
        if(spinIds.length == 0)
            revert NoSpinsAvailable();
        return spins[spinIds.length - 1];
    }
    function getLastSpins(uint count) public view returns(Spin[] memory){
        if(count > spinIds.length)
            count = spinIds.length;
        Spin[] memory yieldSpins = new Spin[](count);
        uint i = 0;
        while(i < count){
            yieldSpins[i] = spins[spinIds[spinIds.length - i - 1]];
            i++;
        }
        return yieldSpins;
    }
    function getAccount() public view returns(Account memory){
        if(accounts[msg.sender].owner != msg.sender)
            revert AccountNotFound();
        return accounts[msg.sender];
    }
    function fundAccount() public payable{
         if(accounts[msg.sender].owner != msg.sender)
            revert AccountNotFound();
        Account storage acct = accounts[msg.sender];
        if(acct.value + msg.value < mindeposit)
            revert MoreFundsRequired();
        acct.value += msg.value;
    }
    function withdraw(uint amount) public nonReentrant payable{
        if(!isOpenForWithdrawl)
            revert IsNotOpenForWithdrawl();
        if(accounts[msg.sender].owner != msg.sender)
            revert AccountNotFound();
        if(amount > address(this).balance)
            revert InsufficentFunds();
        Account storage sender = accounts[msg.sender];
        if((sender.value - amount) < mindeposit)
            revert InsufficentFunds();
        sender.value -= amount;
         (bool success, ) = msg.sender.call{value: amount}("");
        if(!success)
            revert TransferFailed();
    }
    function fund() public onlyOwner payable{
        currentBalance += msg.value;
    }
    function widthdrawFund(uint amount) public onlyOwner nonReentrant payable{
        if(!isOpenForWithdrawl)
            revert IsNotOpenForWithdrawl();
        if(currentBalance < amount)
            revert InsufficentFunds();
        if(address(this).balance < amount)
            revert InsufficentFunds();
        currentBalance -= amount;
        (bool success, ) = msg.sender.call{value: amount}("");
        if(!success)
            revert TransferFailed();
    }
    function closeAccount() public nonReentrant payable{
        if(accounts[msg.sender].owner != msg.sender)
            revert AccountNotFound();
        uint value = accounts[msg.sender].value;
        if(value > address(this).balance)
            revert InsufficentFunds();
        uint i = 0;
        while(i < accountAddresses.length){
            if(accountAddresses[i] == msg.sender)
                break;
            i++;
        }
        if(i == accountAddresses.length - 1)
            accountAddresses.pop();
        else{
            accountAddresses[i] = accountAddresses[accountAddresses.length - 1];
            accountAddresses.pop();
        }
        delete accounts[msg.sender];
        (bool success, ) = msg.sender.call{value: value}("");
        if(!success)
            revert TransferFailed();
    }
    error NotEnoughTimePassed();
    function openBets() public onlyOwner returns (uint){
        if(!isOpenForWithdrawl)
            revert IsNotOpenForWithdrawl();
        if(spinIds.length > 1)
        {
            Spin memory lastSpin = spins[spinIds[spinIds.length - 1]];
            if(block.timestamp < lastSpin.spunTime + timeBetweenBets)
                revert NotEnoughTimePassed();
        }
        isOpenForWithdrawl = false;
        uint spinId = spinIds.length + 1;
        spinIds.push(spinId);
        spins[spinId] = Spin(spinId, block.timestamp, false, 42, 0);
        return spinId;
    }
    error InvalidBetAmount(bool tooHigh);
    error SpinNotFound();
    error SpinClosedForBets();
    function getCurrentBets() public view returns (Bet[] memory){
        if(spinIds.length == 0)
            revert NoSpinsAvailable();
        uint spinId = spinIds[spinIds.length - 1];
        return spinBets[spinId];
    }
    function getBetsForSpin(uint spinId) public view returns(Bet[] memory){
        return spinBets[spinId];
    }
    function getNumbers() public view returns(Number[] memory ns){
        ns = new Number[](numberIds.length);
        uint i = 0;
        while(i < numberIds.length){
            ns[i] = numbers[numberIds[i]];
            i++;
        }
    }
    function placeColorBet(NumberColor color, uint amount) public{
        if(color == NumberColor.Green)
            revert InvalidBet();
        acceptBet(Bet(msg.sender, spinIds[spinIds.length-1], amount, BetType.Color, 42, 42, 42, color, NumberParity.Green, new uint[](0), 0, 0));
    }
    function placeParityBet(NumberParity parity, uint amount) public{
         if(parity == NumberParity.Green)
            revert InvalidBet();
         acceptBet(Bet(msg.sender, spinIds[spinIds.length-1], amount, BetType.Parity, 42, 42, 42, NumberColor.Green, parity, new uint[](0), 0, 0));
    }
    error InvalidBet();
    error BetsNotClosed();
    mapping(uint => uint) requestToSpin;
    function spinTheWheel() onlyOwner public{
        Spin storage currentSpin = spins[spinIds[spinIds.length - 1]];
        if(block.timestamp < currentSpin.startTime + timeForBet)
            revert BetsNotClosed();
        if(spinBets[currentSpin.spinId].length == 0)
        {
            isOpenForWithdrawl = true;
            currentSpin.spunTime = block.timestamp;
            return;
        }
        uint requestId = COORDINATOR.requestRandomWords(
            s_keyHash,
            s_subscriptionId,
            requestConfirmations,
            callbackGasLimit,
            1
            );
        requestToSpin[requestId] = currentSpin.spinId;
    }
    function placeStraightUpBet(uint numberId, uint amount) public{
         if(numbers[numberId].id != numberId)
            revert InvalidBet();
         acceptBet(Bet(msg.sender, spinIds[spinIds.length-1], amount, BetType.StraightUp, numberId, 42, 42, NumberColor.Green, NumberParity.Green, new uint[](0), 0, 0));
    }
    function placeRowBet(uint amount) public{
         acceptBet(Bet(msg.sender, spinIds[spinIds.length-1], amount, BetType.Row, 42, 0, 42, NumberColor.Green, NumberParity.Green, new uint[](0), 0, 0));
    }
     function placeStreetBet(uint8 street, uint amount) public{
        if(street == 0 || street > 12)
            revert InvalidBet();
         acceptBet(Bet(msg.sender, spinIds[spinIds.length-1], amount, BetType.Street, 42, street, 42, NumberColor.Green, NumberParity.Green, new uint[](0), 0, 0));
    }
    function placeDozenBet(uint8 whichDozen, uint amount) public{
        if(whichDozen == 0 || whichDozen > 3)
            revert InvalidBet();
        acceptBet(Bet(msg.sender, spinIds[spinIds.length-1], amount, BetType.Dozen, 42, 42, 42, NumberColor.Green, NumberParity.Green, new uint[](0), whichDozen, 0));
    }
    function place18Bet(uint8 which18, uint amount) public{
        if(which18 == 0 || which18 > 2)
            revert InvalidBet();
        acceptBet(Bet(msg.sender, spinIds[spinIds.length-1], amount, BetType.Eighteen, 42, 42, 42, NumberColor.Green, NumberParity.Green, new uint[](0), 0, which18));
    }
    function placeSplitBet(uint number1, uint number2, uint amount) public{
        Number memory n1 = numbers[number1];
        Number memory n2 = numbers[number2];
        bool isValid = (n1.row == n2.row && (n1.column - 1 == n2.column || n2.column -1 == n2.column)) || (n1.column == n2.column && (n2.row -1 == n1.row || n1.row - 1 == n2.row));
        if(!isValid)
            revert InvalidBet();
        uint[] memory ns = new uint[](2);
        ns[0] = number1;
        ns[1] = number2;
        acceptBet(Bet(msg.sender, spinIds[spinIds.length-1], amount, BetType.Split, 42, 42, 42, NumberColor.Green, NumberParity.Green, ns, 0, 0));
    }
    function placeColumnBet(uint8 column, uint amount) public{
         if(column > 2)
            revert InvalidBet();
         acceptBet(Bet(msg.sender, spinIds[spinIds.length-1], amount, BetType.Column, 42, 42, column, NumberColor.Green, NumberParity.Green, new uint[](0), 0, 0));
    }
    function placeTopLineBet(uint amount) public{
        uint[] memory topline = new uint[](5);
        topline[0] = numberIds[0];
        topline[1] = numberIds[1];
        topline[2] = numberIds[2];
        topline[3] = numberIds[3];
        topline[4] = numberIds[4];
        acceptBet(Bet(msg.sender, spinIds[spinIds.length-1], amount, BetType.TopLine, 42, 42, 42, NumberColor.Green, NumberParity.Green, topline, 0, 0));
    }
    function acceptBet(Bet memory bet) private{
        if(accounts[bet.account].owner != bet.account)
            revert AccountNotFound();
        if(accounts[bet.account].value < bet.amount)
            revert InsufficentFunds();
        InsideOrOutside io = getBetPosition(bet.betType);
        if(io == InsideOrOutside.Inside && bet.amount < minbetinside)
            revert InvalidBetAmount(false);
        else if(io == InsideOrOutside.Outside && bet.amount < minbetoutside)
            revert InvalidBetAmount(false);
        Spin memory spin = spins[bet.spinId];
        if(spin.spinId != bet.spinId)
            revert SpinNotFound();
         if(block.timestamp > spin.startTime + timeForBet)
            revert SpinClosedForBets();
        if((calculateExposure(spin.spinId) + (bet.amount * payouts[bet.betType])) > currentBalance)
            revert InvalidBetAmount(true);
        Account storage acct = accounts[bet.account];
        acct.value -= bet.amount;
        spinBets[spin.spinId].push(bet);
    }
    function calculateExposure(uint spinId) public view returns(uint){
        Bet[] memory bets = spinBets[spinId];
        uint i = 0;
        uint exposure = 0;
        while(i < bets.length){
            Bet memory bet = bets[i];
            exposure += bet.amount * payouts[bet.betType];
            i++;
        }
        return exposure;
    }
    enum InsideOrOutside{
        Inside,
        Outside
    }
    function getBetPosition(BetType bt) private pure returns(InsideOrOutside){
        if(bt == BetType.Color || bt == BetType.Column || bt == BetType.Dozen || bt == BetType.Eighteen || bt == BetType.Parity)
            return InsideOrOutside.Outside;
        return InsideOrOutside.Inside;
    }
    function addNumber(string memory name, NumberParity parity, NumberColor color, uint8 row, uint8 column, uint8 which18, uint8 whichDozen) private{
        numberIds.push();
        uint idx = numberIds.length - 1;
        Number memory n = Number(numberIds.length, name, parity, color, row, column, which18, whichDozen);
        numberIds[idx] = n.id;
        numbers[n.id] = n;
        numbersByColor[color].push(n);
        numbersByParity[parity].push(n);
        numbersByRow[row].push(n);
        numbersByColumn[column].push(n);
        numbersByDozen[whichDozen].push(n);
        numbersBy18[which18].push(n);
    }
    function fulfillRandomWords(uint256 requestId, uint256[] memory randomWords) 
    internal override {
        Spin storage spin = spins[requestToSpin[requestId]];
        uint spinIndex = numberIds.length % randomWords[0];
        spin.spunNumberId = numberIds[spinIndex];
        spin.spunTime = block.timestamp;
        Number memory n = numbers[spin.spunNumberId];
        Bet[] memory bets = spinBets[spin.spinId];
        uint i = 0;
        while(i < bets.length)
        {
            Bet memory b = bets[i];
            
            bool winner = false;
            if(b.betType == BetType.StraightUp && b.number == n.id)
                winner = true;
            else if((b.betType == BetType.Row || b.betType == BetType.Street) && b.row == n.row)
                winner = true;
            else if(b.betType == BetType.Column && b.column == n.column && n.color != NumberColor.Green)
                winner = true;
            else if(b.betType == BetType.Split || b.betType == BetType.TopLine)
            {
                uint k = 0;
                while(k < b.numbers.length){
                    winner = winner || b.numbers[k] ==  n.id;
                    k++;
                }
            }
            else if(b.betType == BetType.Color && b.color == n.color)
                winner = true;
            else if(b.betType == BetType.Parity && b.parity == n.parity)
                winner = true;
            else if(b.betType == BetType.Eighteen && b.byThe18 == n.which18)
                winner = true;
            else if(b.betType == BetType.Dozen && b.byTheDozen == n.whichDozen)
                winner = true;
            if(winner){
                uint payout = payouts[b.betType] * b.amount;
                accounts[b.account].value += payout;
                currentBalance -= payout;
            }
            else
                currentBalance += b.amount; 
            i++;
        }
        isOpenForWithdrawl = true;
    }
}