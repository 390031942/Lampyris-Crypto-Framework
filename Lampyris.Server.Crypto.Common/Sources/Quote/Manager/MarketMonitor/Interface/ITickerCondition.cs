namespace Lampyris.Server.Crypto.Common;

/// <summary>
///  ��Ticker���� �������� �ӿ���
/// </summary>
public abstract class ITickerCondition
{
    public abstract bool Test(QuoteTickerData quoteTickerData);
}