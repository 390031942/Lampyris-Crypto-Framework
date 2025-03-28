#include "MathUtil.h"

void MathUtil::extractSignificantDigits(double num, int N, int& significantDigits, double& magnitude) {
    if (num == 0) {
        significantDigits = 0;
        magnitude = 1;
        return;
    }

    // ������������10 ���ݴΣ�
    magnitude = std::pow(10, std::floor(std::log10(std::fabs(num))) + 1 - N);

    // ��ȡǰ N ����Ч���ֲ�ת��Ϊ����
    significantDigits = static_cast<int>(std::round(num / magnitude));
}

double MathUtil::ceilModulo(double num, int N, int M) {
    int significantDigits;
    double magnitude;

    // ��ȡǰ N ����Ч����
    extractSignificantDigits(num, N, significantDigits, magnitude);

    // ��ȡ�������ϵ���������� M �ı���
    int ceilInt = (significantDigits % M == 0) ? significantDigits : ((significantDigits / M) + 1) * M;

    // �ָ�Ϊ������
    return ceilInt * magnitude;
}

double MathUtil::floorModulo(double num, int N, int M) {
    int significantDigits;
    double magnitude;

    // ��ȡǰ N ����Ч����
    extractSignificantDigits(num, N, significantDigits, magnitude);

    // ��ȡ�������µ���������� M �ı���
    int floorInt = (significantDigits / M) * M;

    // �ָ�Ϊ������
    return floorInt * magnitude;
}

QString MathUtil::formatDoubleWithStep(double value, const QString& step) {
    // ������С�����ַ���������С�������Ҫ������λ��
    int decimalPlaces = 0;
    int dotIndex = step.indexOf('.');
    if (dotIndex != -1) {
        decimalPlaces = step.length() - dotIndex - 1; // С������λ��
    }

    // ʹ�� QString::number ��ʽ�� double ��ֵ
    QString formattedValue = QString::number(value, 'f', decimalPlaces);

    return formattedValue;
}
