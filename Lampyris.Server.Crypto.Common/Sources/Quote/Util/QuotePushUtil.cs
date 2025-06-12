namespace Lampyris.Server.Crypto.Common;

using Lampyris.Crypto.Protocol.Quote;
using System;

public static class QuotePushUtil
{
    public static void PushTradeRuleBean(WebSocketService webSocketService, int clientUserId, SymbolTradeRule data)
    {
        SymbolTradeRuleBean bean = ToSymbolTradeDataBean(data);
        webSocketService.PushMessge(clientUserId, bean);
    }

    public static void PushTradeRuleBeanList(WebSocketService webSocketService,int clientUserId, IEnumerable<SymbolTradeRule> dataList)
    {
        ResTradeRule resTradeRule = new ResTradeRule();

        foreach (SymbolTradeRule data in dataList)
        {
            var bean = ToSymbolTradeDataBean(data);
        }

        webSocketService.PushMessge(clientUserId, resTradeRule);
    }

    private static ResMarketPreviewData ValueOf(MarketSummaryData marketSummaryData)
    {
        ResMarketPreviewData resMarketPreviewData = new ResMarketPreviewData();
        resMarketPreviewData.RiseCount = marketSummaryData.RiseCount;
        resMarketPreviewData.FallCount = marketSummaryData.FallCount;
        resMarketPreviewData.FlatCount = marketSummaryData.UnchangedCount;
        resMarketPreviewData.AvgPerc = (double)marketSummaryData.AvgChangePerc;
        resMarketPreviewData.TopGainerAvgPerc = (double)marketSummaryData.Top10AvgChangePerc;
        resMarketPreviewData.BottomGainerAvgPerc = (double)marketSummaryData.Last10AvgChangePerc;
        resMarketPreviewData.MainstreamAvgPerc = (double)marketSummaryData.MainStreamAvgChangePerc;

        foreach(var data in marketSummaryData.IntervalDatas)
        {
            resMarketPreviewData.IntervalDataList.Add(new MarketPreviewIntervalDataBean()
            {
                UpperBoundPerc = data.UpperBoundPerc,
                LowerBoundPerc = data.LowerBoundPerc,
                Count = data.Count
            });
        }
        return resMarketPreviewData;
    }

    public static void PushMarketPreviewData(WebSocketService webSocketService, MarketSummaryData marketSummaryData)
    {
        ResMarketPreviewData resMarketPreviewData = ValueOf(marketSummaryData);
        webSocketService.BroadcastMessage(resMarketPreviewData);
    }

    private static SymbolTradeRuleBean ToSymbolTradeDataBean(SymbolTradeRule data)
    {
        SymbolTradeRuleBean bean = new SymbolTradeRuleBean();
        bean.Symbol = data.Symbol;
        bean.MaxPrice = (double)data.MaxPrice;
        bean.MinPrice = (double)data.MinPrice;
        bean.PriceTickSize = (double)data.PriceStep;
        bean.MaxQuantity = (double)data.MaxQuantity;
        bean.MinQuantity = (double)data.MinQuantity;
        bean.QuantityTickSize = (double)data.QuantityStep;
        bean.MinNotional = (double)data.MinNotional;

        return bean;
    }
}
