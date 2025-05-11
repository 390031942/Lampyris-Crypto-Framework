namespace Lampyris.Server.Crypto.Common;

/// <summary>
///  ��k�������� �������� �ӿ���
/// </summary>
public abstract class ICandleCondition: IDummyMarketAnomalyCondition
{
    /// <summary>
    /// K��ʱ����
    /// </summary>
    public BarSize BarSize { get; protected set; }

    /// <summary>
    /// ������������k����Ŀ
    /// </summary>
    public abstract int ExpectedCount { get; }
    
    public ICandleCondition(BarSize barSize)
    {
        BarSize = barSize;
    }

    /// <summary>
    /// ����k���������Ƿ���������
    /// </summary>
    /// <param name="dataList">k���б�ֻ������</param>
    /// <param name="isEnd">�Ƿ�ǰbarSize���ڽ�������ĳʱ�̵�59�����յ�1min k�߽�����k�����ݣ���ʱisEnd = true</param>
    /// <param name="value">�춯��ֵ</param>
    /// <returns></returns>
    public abstract bool Test(ReadOnlySpan<QuoteCandleData> dataList, bool isEnd, out decimal value);
}   
