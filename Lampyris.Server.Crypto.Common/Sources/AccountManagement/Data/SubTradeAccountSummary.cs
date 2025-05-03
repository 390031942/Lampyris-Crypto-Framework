namespace Lampyris.Server.Crypto.Common;

public class SubTradeAccountSummary
{
    /// <summary>
    /// 指示该账户是否可以存款的布尔值
    /// </summary>
    public bool CanDeposit { get; set; }

    /// <summary>
    ///账户是否可以交易
    /// </summary>
    public bool CanTrade { get; set; }

    /// <summary>
    /// 账户是否可以提现
    /// </summary>
    public bool CanWithdraw { get; set; }

    /// <summary>
    /// 挂单手续费（Maker Fee），单位：%s
    /// </summary>
    public decimal MakerFee { get; set; }

    /// <summary>
    /// 吃单手续费（Taker Fee），单位：%s
    /// </summary>
    public decimal TakerFee { get; set; }

    /// <summary>
    /// 最大可提现数量
    /// </summary>
    public decimal MaxWithdrawQuantity { get; set; }

    /// <summary>
    /// 总初始保证金（表示账户中所有挂单和持仓所需的初始保证金总和）
    /// </summary>
    public string TotalInitialMargin { get; set; }

    /// <summary>
    /// 总维持保证金（表示账户中所有持仓所需的最低维持保证金总和）
    /// </summary>
    public string TotalMaintMargin { get; set; }

    /// <summary>
    /// 总保证金余额（表示账户中的总保证金余额，包括未实现盈亏。它是账户的核心资金，用于支持交易和持仓）
    /// </summary>
    public string TotalMarginBalance { get; set; }

    /// <summary>
    /// 总挂单初始保证金（表示账户中所有挂单（未成交订单）所冻结的初始保证金总和）
    /// </summary>
    public string TotalOpenOrderInitialMargin { get; set; }

    /// <summary>
    /// 总持仓初始保证金（表示账户中所有已成交持仓所冻结的初始保证金总和。）
    /// </summary>
    public string TotalPositionInitialMargin { get; set; }

    /// <summary>
    /// 总未实现盈亏（表示账户中所有持仓的未实现盈亏总和）
    /// </summary>
    public string TotalUnrealizedProfit { get; set; }

    /// <summary>
    /// 总钱包余额（表示账户中的总资金余额，不包括未实现盈亏。）
    /// </summary>
    public string TotalWalletBalance { get; set; }

    /// <summary>
    /// 可用余额（表示账户中可以用于开仓或挂单的资金余额。它是总保证金余额减去已冻结的保证金（包括挂单和持仓保证金））
    /// </summary>
    public string AvailableBalance { get; set; }

    /// <summary>
    /// 账户信息的更新时间
    /// </summary>
    public DateTime? UpdateTime { get; set; }
}
