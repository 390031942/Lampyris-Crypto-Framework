namespace Lampyris.Server.Crypto.Common;

/// <summary>
///  ��k�������� �������� �ӿ���
/// </summary>
public abstract class ICandleCondition
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
}   
