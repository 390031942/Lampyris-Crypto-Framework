namespace Lampyris.Server.Crypto.Common;

/// <summary>
///  ��Ticker���� �������� �ӿ���
/// </summary>
public abstract class ITickerCondition:IDummyMarketAnomalyCondition
{
    /// <summary>
    /// ����Ticker�����Ƿ���������
    /// </summary>
    /// <param name="quoteTickerData">ticker����</param>
    /// <param name="value">�춯��ֵ</param>
    /// <returns></returns>
    public abstract bool Test(QuoteTickerData quoteTickerData, out decimal value);
}