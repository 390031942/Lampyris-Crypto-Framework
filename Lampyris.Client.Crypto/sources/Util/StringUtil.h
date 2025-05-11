#pragma once

// Project Include(s)
#include <QColor>

class StringUtil {
public:
    // ����Ƿ��ٷֱ��ı��ַ���
    static QString getPercentageText(double value, int decimalPlaces = 2) {
        if (value > 0) {
            return QString("+%1%").arg(QString::number(value, 'f', decimalPlaces));
        }
        else if (value < 0) {
            return QString("%1%").arg(QString::number(value, 'f', decimalPlaces));
        }
        else {
            return QString("0.%1%").arg(QString(decimalPlaces, '0')); // ��̬����С��λ��
        }
    }
};
