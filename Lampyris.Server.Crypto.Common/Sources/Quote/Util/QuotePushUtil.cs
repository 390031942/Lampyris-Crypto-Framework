namespace Lampyris.Server.Crypto.Common;

using Lampyris.Crypto.Protocol.Quote;

public static class QuotePushUtil
{
    public static void PushTradeRuleBean(WebSocketService webSocketService, int clientUserId, SymbolTradeRule data)
    {
        SymbolTradeRuleBean bean = ToSymbolTradeDataBean(data);
        webSocketService.PushMessge(clientUserId, bean);
    }

    public static void PushTradeRuleBeanList(WebSocketService webSocketService,int clientUserId, List<SymbolTradeRule> dataList)
    {
        ResTradeRule resTradeRule = new ResTradeRule();

        foreach (SymbolTradeRule data in dataList)
        {
            var bean = ToSymbolTradeDataBean(data);
        }

        webSocketService.PushMessge(clientUserId, resTradeRule);
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
