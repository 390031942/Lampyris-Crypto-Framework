#pragma once

// QT Include(s)
#include <QString>

// STD Include(s)
#include <cmath>

class MathUtil {
public:
    /// <summary>
    /// 提取浮点数的前 N 个有效数字，并返回缩放后的整数和数量级。
    /// </summary>
    /// <param name="num">输入的浮点数。</param>
    /// <param name="N">要提取的有效数字的个数。</param>
    /// <param name="significantDigits">返回的缩放后的整数值（前 N 个有效数字）。</param>
    /// <param name="magnitude">返回的数量级（10 的幂次）。</param>
    static void extractSignificantDigits(double num, int N, int& significantDigits, double& magnitude);

    /// <summary>
    /// 对浮点数的前 N 个有效数字进行对模 M 的上取整。
    /// </summary>
    /// <param name="num">输入的浮点数。</param>
    /// <param name="N">要提取的有效数字的个数。</param>
    /// <param name="M">模值，用于计算上取整。</param>
    /// <returns>返回经过处理后的浮点数。</returns>
    static double ceilModulo(double num, int N, int M);

    /// <summary>
    /// 对浮点数的前 N 个有效数字进行对模 M 的下取整。
    /// </summary>
    /// <param name="num">输入的浮点数。</param>
    /// <param name="N">要提取的有效数字的个数。</param>
    /// <param name="M">模值，用于计算下取整。</param>
    /// <returns>返回经过处理后的浮点数。</returns>
    static double floorModulo(double num, int N, int M);

    /// <summary>
    /// 将浮点数转换为满足最小步进的字符串。
    /// </summary>
    /// <param name="value">输入的浮点数。</param>
    /// <param name="step">最小步进值，表示结果需要满足的精度要求（例如 "0.01" 表示精确到小数点后两位）。</param>
    /// <returns>返回格式化后的字符串。</returns>
    static QString formatDoubleWithStep(double value, const QString& step);
};
