// Project Include(s)
#include "PercentageDisplayText.h"

PercentageDisplayText::PercentageDisplayText(QWidget* parent)
    : QLabel(parent), m_mode(FontColorMode), m_value(0.0) {
    setAlignment(Qt::AlignCenter); // 居中显示
    setFixedSize(100, 40);        // 设置固定大小
    setStyleSheet("font-size: 16px; font-weight: bold;"); // 设置字体样式
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

        // 绘制圆角背景
        painter.setBrush(QBrush(backgroundColor()));
        painter.setPen(Qt::NoPen);
        painter.drawRoundedRect(rect(), 7, 7);
        painter.restore();
    }

    // 调用父类的绘制方法
    QLabel::paintEvent(event);
}

QColor PercentageDisplayText::backgroundColor() const {
    if (m_value < 0) {
        return QColor(82, 189, 135); // 跌:绿色
    }
    else if (m_value > 0) {
        return QColor(242, 80, 95); // 涨:红色
    }
    else {
        return QColor(153, 153, 153); // 平:灰色
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
        return QString("0.%1%").arg(QString(decimalPlaces, '0')); // 动态生成小数位数
    }
}

void PercentageDisplayText::updateAppearance() {
    if (m_mode == FontColorMode) {
        // 字体变色模式
        QColor color = backgroundColor();
        // 将 QColor 转换为字符串形式
        QString colorString = QString("rgb(%1, %2, %3)")
            .arg(color.red())
            .arg(color.green())
            .arg(color.blue());

        // 设置样式表
        setStyleSheet(QString("color: %1; font-size: 16px; font-weight: bold;").arg(colorString));
    }
    else if (m_mode == BackgroundColorMode) {
        // 背景变色模式
        setStyleSheet("color: white; font-size: 16px; font-weight: bold;");
    }

    setText(formattedText());
    update(); // 触发重绘
}
