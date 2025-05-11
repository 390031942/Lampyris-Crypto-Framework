// Project Include(s)
#include "PriceText.h"

PriceText::PriceText(QWidget* parent) : QLabel(parent), m_value(0.0), m_minTick(0.01) {
    setAlignment(Qt::AlignCenter); // 文本居中
}

// 设置价格
void PriceText::setValue(double value) {
    m_value = value;
    updateText();
}

// 设置价格最小变化单位
void PriceText::setMinTick(double minTick) {
    m_minTick = minTick;
    updateText();
}

// 格式化数字，添加逗号分隔
QString PriceText::formatNumberWithCommas(double value, int decimalPlaces) const {
    QString numberString = QString::number(value, 'f', decimalPlaces); // 保留指定小数位
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

// 更新显示文本
void PriceText::updateText() {
    // 根据最小变化单位计算小数点后精度
    int decimalPlaces = std::ceil(-std::log10(m_minTick)); // 例如 minTick=0.0001 -> decimalPlaces=4

    // 格式化价格并设置文本
    QString formattedPrice = formatNumberWithCommas(m_value, decimalPlaces);
    setText(formattedPrice);
}
