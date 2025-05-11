// Project Include(s)
#include "PercentageDisplayText.h"

PercentageDisplayText::PercentageDisplayText(QWidget* parent)
    : QLabel(parent), m_mode(FontColorMode), m_value(0.0) {
    setAlignment(Qt::AlignCenter); // ������ʾ
    setFixedSize(100, 40);        // ���ù̶���С
    setStyleSheet("font-size: 16px; font-weight: bold;"); // ����������ʽ
}

void PercentageDisplayText::setDisplayMode(DisplayMode displayMode) {
    m_mode = displayMode;
    updateAppearance();
}

void PercentageDisplayText::setValue(double newValue) {
    m_value = newValue;
    updateAppearance();
}

void PercentageDisplayText::paintEvent(QPaintEvent* event) {
    if (m_mode == BackgroundColorMode) {
        QPainter painter(this);
        painter.save();
        painter.setRenderHint(QPainter::Antialiasing);

        // ����Բ�Ǳ���
        painter.setBrush(QBrush(backgroundColor()));
        painter.setPen(Qt::NoPen);
        painter.drawRoundedRect(rect(), 7, 7);
        painter.restore();
    }

    // ���ø���Ļ��Ʒ���
    QLabel::paintEvent(event);
}

QColor PercentageDisplayText::backgroundColor() const {
    if (m_value < 0) {
        return QColor(82, 189, 135); // ��:��ɫ
    }
    else if (m_value > 0) {
        return QColor(242, 80, 95); // ��:��ɫ
    }
    else {
        return QColor(153, 153, 153); // ƽ:��ɫ
    }
}

QString PercentageDisplayText::formattedText(int decimalPlaces) const {
    if (m_value > 0) {
        return QString("+%1%").arg(QString::number(m_value, 'f', decimalPlaces));
    }
    else if (m_value < 0) {
        return QString("%1%").arg(QString::number(m_value, 'f', decimalPlaces));
    }
    else {
        return QString("0.%1%").arg(QString(decimalPlaces, '0')); // ��̬����С��λ��
    }
}

void PercentageDisplayText::updateAppearance() {
    if (m_mode == FontColorMode) {
        // �����ɫģʽ
        QColor color = backgroundColor();
        // �� QColor ת��Ϊ�ַ�����ʽ
        QString colorString = QString("rgb(%1, %2, %3)")
            .arg(color.red())
            .arg(color.green())
            .arg(color.blue());

        // ������ʽ��
        setStyleSheet(QString("color: %1; font-size: 16px; font-weight: bold;").arg(colorString));
    }
    else if (m_mode == BackgroundColorMode) {
        // ������ɫģʽ
        setStyleSheet("color: white; font-size: 16px; font-weight: bold;");
    }

    setText(formattedText());
    update(); // �����ػ�
}
