using Lampyris.Crypto.Protocol.App;
using Lampyris.Crypto.Protocol.Common;
using Lampyris.Crypto.Protocol.Quote;
using Lampyris.CSharp.Common;

namespace Lampyris.Server.Crypto.Common;

[Component]
public class QuoteFacadeService
{
    [Autowired]
    private QuoteSubscriptionService m_QuoteSubscriptionService;

    [Autowired]
    private WebSocketService m_WebSocketService;

    #region Ticker订阅
    [MessageHandler(Request.RequestTypeOneofCase.ReqSubscribeTickerData)]
    public void ReqSubscribeTickerData(ClientUserInfo clientUserInfo, Request request)
    {
        ReqSubscribeTickerData req = request.ReqSubscribeTickerData;
        bool success = m_QuoteSubscriptionService.SubscribeTicker(clientUserInfo.UserId);
        if(!success)
        {
            m_WebSocketService.PushMessge(clientUserInfo.UserId, new ResNotice() {
                Content = "",
                Type = NoticeType.Toast, 
            });
        }
    }
    #endregion

    [MessageHandler(Request.RequestTypeOneofCase.ReqCandlestickQuery)]
    public void ReqCandlestickQuery(ClientUserInfo clientUserInfo, Request request)
    {
        ReqCandlestickQuery req = request.ReqCandlestickQuery;
    }
}
