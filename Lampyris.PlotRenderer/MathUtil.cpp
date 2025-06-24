#include "MathUtil.h"

void MathUtil::extractSignificantDigits(double num, int N, int& significantDigits, double& magnitude) {
    if (num == 0) {
        significantDigits = 0;
        magnitude = 1;
        return;
    }

    // 计算数量级（10 的幂次）
    magnitude = std::pow(10, std::floor(std::log10(std::fabs(num))) + 1 - N);

    // 提取前 N 个有效数字并转换为整数
    significantDigits = static_cast<int>(std::round(num / magnitude));
}

double MathUtil::ceilModulo(double num, int N, int M) {
    int significantDigits;
    double magnitude;

    // 提取前 N 个有效数字
    extractSignificantDigits(num, N, significantDigits, magnitude);

    // 上取整：向上调整到最近的 M 的倍数
    int ceilInt = (significantDigits % M == 0) ? significantDigits : ((significantDigits / M) + 1) * M;

    // 恢复为浮点数
    return ceilInt * magnitude;
}

double MathUtil::floorModulo(double num, int N, int M) {
    int significantDigits;
    double magnitude;

    // 提取前 N 个有效数字
    extractSignificantDigits(num, N, significantDigits, magnitude);

    // 下取整：向下调整到最近的 M 的倍数
    int floorInt = (significantDigits / M) * M;

    // 恢复为浮点数
    return floorInt * magnitude;
}

QString MathUtil::formatDoubleWithStep(double value, const QString& step) {
    // 解析最小步进字符串，计算小数点后需要保留的位数
    int decimalPlaces = 0;
    int dotIndex = step.indexOf('.');
    if (dotIndex != -1) {
        decimalPlaces = step.length() - dotIndex - 1; // 小数点后的位数
    }

    // 使用 QString::number 格式化 double 数值
    QString formattedValue = QString::number(value, 'f', decimalPlaces);

    return formattedValue;
}
