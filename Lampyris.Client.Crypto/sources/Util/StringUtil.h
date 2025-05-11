#pragma once

// Project Include(s)
#include <QColor>

class StringUtil {
public:
    // 获得涨幅百分比文本字符串
    static QString getPercentageText(double value, int decimalPlaces = 2) {
        if (value > 0) {
            return QString("+%1%").arg(QString::number(value, 'f', decimalPlaces));
        }
        else if (value < 0) {
            return QString("%1%").arg(QString::number(value, 'f', decimalPlaces));
        }
        else {
            return QString("0.%1%").arg(QString(decimalPlaces, '0')); // 动态生成小数位数
        }
    }
};
