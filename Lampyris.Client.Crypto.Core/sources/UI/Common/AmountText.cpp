// Project Include(s)
#include "AmountText.h"

AmountText::AmountText(QWidget* parent) : QLabel(parent), m_amount(0) {
}

void AmountText::setValue(double amount, QString unit) {
    m_amount = amount;
    m_unit = unit;
    updateText();
}

QString AmountText::formatNumberWithCommas(double value) const {
    // 将数字格式化为带逗号分隔的字符串
    QString numberString = QString::number(value, 'f', 2); // 保留小数点后两位
    int dotIndex = numberString.indexOf('.'); // 找到小数点的位置
    QString integerPart = numberString.left(dotIndex); // 整数部分
    QString decimalPart = numberString.mid(dotIndex); // 小数部分

    // 在整数部分添加逗号分隔
    QString formattedIntegerPart;
    int count = 0;
    for (int i = integerPart.size() - 1; i >= 0; --i) {
        formattedIntegerPart.prepend(integerPart[i]);
        count++;
        if (count % 3 == 0 && i > 0) {
            formattedIntegerPart.prepend(',');
        }
    }

    return formattedIntegerPart + decimalPart; // 拼接整数部分和小数部分
}

void AmountText::updateText() {
    QString displayText;
    if (m_amount < 1000000) {
        // 小于 100 万，显示原始数值
        displayText = formatNumberWithCommas(m_amount);
    }
    else if (m_amount >= 1000000 && m_amount < 100000000) {
        // 大于等于 100 万，小于 1 亿，显示 XXX 万
        double valueInMillions = m_amount / 1000000.0;
        displayText = QString("%1 万").arg(formatNumberWithCommas(valueInMillions));
    }
    else {
        // 大于等于 1 亿，显示 XX 亿
        double valueInBillions = m_amount / 100000000.0;
        displayText = QString("%1 亿").arg(formatNumberWithCommas(valueInBillions));
    }

    setText(displayText + " " + m_unit);
}
