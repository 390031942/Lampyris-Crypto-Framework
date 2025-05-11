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
    // �����ָ�ʽ��Ϊ�����ŷָ����ַ���
    QString numberString = QString::number(value, 'f', 2); // ����С�������λ
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

void AmountText::updateText() {
    QString displayText;
    if (m_amount < 1000000) {
        // С�� 100 ����ʾԭʼ��ֵ
        displayText = formatNumberWithCommas(m_amount);
    }
    else if (m_amount >= 1000000 && m_amount < 100000000) {
        // ���ڵ��� 100 ��С�� 1 �ڣ���ʾ XXX ��
        double valueInMillions = m_amount / 1000000.0;
        displayText = QString("%1 ��").arg(formatNumberWithCommas(valueInMillions));
    }
    else {
        // ���ڵ��� 1 �ڣ���ʾ XX ��
        double valueInBillions = m_amount / 100000000.0;
        displayText = QString("%1 ��").arg(formatNumberWithCommas(valueInBillions));
    }

    setText(displayText + " " + m_unit);
}
