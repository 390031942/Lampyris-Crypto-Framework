#pragma once

// QT Include(s)
#include <QString>

// STD Include(s)
#include <cmath>

class MathUtil {
public:
    /// <summary>
    /// ��ȡ��������ǰ N ����Ч���֣����������ź����������������
    /// </summary>
    /// <param name="num">����ĸ�������</param>
    /// <param name="N">Ҫ��ȡ����Ч���ֵĸ�����</param>
    /// <param name="significantDigits">���ص����ź������ֵ��ǰ N ����Ч���֣���</param>
    /// <param name="magnitude">���ص���������10 ���ݴΣ���</param>
    static void extractSignificantDigits(double num, int N, int& significantDigits, double& magnitude);

    /// <summary>
    /// �Ը�������ǰ N ����Ч���ֽ��ж�ģ M ����ȡ����
    /// </summary>
    /// <param name="num">����ĸ�������</param>
    /// <param name="N">Ҫ��ȡ����Ч���ֵĸ�����</param>
    /// <param name="M">ģֵ�����ڼ�����ȡ����</param>
    /// <returns>���ؾ��������ĸ�������</returns>
    static double ceilModulo(double num, int N, int M);

    /// <summary>
    /// �Ը�������ǰ N ����Ч���ֽ��ж�ģ M ����ȡ����
    /// </summary>
    /// <param name="num">����ĸ�������</param>
    /// <param name="N">Ҫ��ȡ����Ч���ֵĸ�����</param>
    /// <param name="M">ģֵ�����ڼ�����ȡ����</param>
    /// <returns>���ؾ��������ĸ�������</returns>
    static double floorModulo(double num, int N, int M);

    /// <summary>
    /// ��������ת��Ϊ������С�������ַ�����
    /// </summary>
    /// <param name="value">����ĸ�������</param>
    /// <param name="step">��С����ֵ����ʾ�����Ҫ����ľ���Ҫ������ "0.01" ��ʾ��ȷ��С�������λ����</param>
    /// <returns>���ظ�ʽ������ַ�����</returns>
    static QString formatDoubleWithStep(double value, const QString& step);
};
