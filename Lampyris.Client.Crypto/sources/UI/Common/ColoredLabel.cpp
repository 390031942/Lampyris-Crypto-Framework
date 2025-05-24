// Project Include(s)
#include "ColoredLabel.h"

ColoredLabel::ColoredLabel(QWidget* parent)
    : QLabel(parent), m_currentColor(Qt::black) { // 默认颜色为黑色
    // 初始化时设置默认颜色
    setStyleSheet(QString("color: %1;").arg(m_currentColor.name()));
}

void ColoredLabel::setTextColor(const QColor& color) {
    // 如果新颜色与当前颜色相同，则不设置
    if (color == m_currentColor) {
        return;
    }

    // 更新颜色
    m_currentColor = color;

    // 使用样式表设置颜色
    setStyleSheet(QString("color: %1;").arg(m_currentColor.name()));
}

QColor ColoredLabel::textColor() const {
    return m_currentColor;
}