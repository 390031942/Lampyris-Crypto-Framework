// Project Include(s)
#include "PriceText.h"
#include "UI/Common/GlobalUIStyle.h"

PriceText::PriceText(QWidget* parent) : 
    ColoredLabel(parent), m_value(0.0), m_minTick(0.01), m_percentage(0) {
    setAlignment(Qt::AlignCenter); // �ı�����
}

// ���ü۸�
void PriceText::setValue(double value, double percentage) {
    m_value = value;
    m_percentage = percentage;
    updateText();
}

// ���ü۸���С�仯��λ
void PriceText::setMinTick(double minTick) {
    m_minTick = minTick;
    updateText();
}

// ��ʽ�����֣���Ӷ��ŷָ�
QString PriceText::formatNumberWithCommas(double value, int decimalPlaces) const {
    QString numberString = QString::number(value, 'f', decimalPlaces); // ����ָ��С��λ
    int dotIndex = numberString.indexOf('.'); // �ҵ�С�����λ��
    QString integerPart = numberString.left(dotIndex); // ��������
    QString decimalPart = numberString.mid(dotIndex); // С������

    // ������������Ӷ��ŷָ�
    QString formattedIntegerPart;
    int count = 0;
    for (int i = integerPart.size() - 1; i >= 0; --i) {
        formattedIntegerPart.prepend(integerPart[i]);
        count++;
        if (count % 3 == 0 && i > 0) {
            formattedIntegerPart.prepend(',');
        }
    }

    return formattedIntegerPart + decimalPart; // ƴ���������ֺ�С������
}

// ������ʾ�ı�
void PriceText::updateText() {
    // ������С�仯��λ����С����󾫶�
    int decimalPlaces = std::ceil(-std::log10(m_minTick)); // ���� minTick=0.0001 -> decimalPlaces=4

    // ��ʽ���۸������ı�
    QString formattedPrice = formatNumberWithCommas(m_value, decimalPlaces);
    setText(formattedPrice);
}
