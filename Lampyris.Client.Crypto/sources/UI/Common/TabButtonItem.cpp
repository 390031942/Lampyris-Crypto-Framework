// Project Include(s)
#include "TabButtonItem.h"

TabButtonItem::TabButtonItem(const QString& text, QWidget* parent)
    : QWidget(parent), m_text(text), m_selected(false), m_mode(TabButtonDisplayMode::NORMAL) {}

void TabButtonItem::setSelected(bool selected) {
    m_selected = selected;
    update(); // 重新绘制
}

void TabButtonItem::setDisplayMode(TabButtonDisplayMode mode) {
    m_mode = mode; // 设置显示模式
    update();
}

QString TabButtonItem::text() const { return m_text; }

void TabButtonItem::paintEvent(QPaintEvent* event) {
    QPainter painter(this);

    // 绘制背景
    if (m_selected && m_mode == TabButtonDisplayMode::BACKGROUND) {
        painter.setBrush(QBrush(Qt::gray));
        painter.setPen(Qt::NoPen);
        painter.drawRoundedRect(rect(), 10, 10); // 灰色圆角矩形
    }

    // 绘制文字
    painter.setPen(m_selected ? Qt::white : Qt::gray); // 选中为白色，未选中为灰色
    painter.setFont(QFont("Arial", 12));
    painter.drawText(rect(), Qt::AlignCenter, m_text);

    // 绘制分割线和橙色线条
    if (m_mode == TabButtonDisplayMode::UNDERLINE) {
        if (m_selected) {
            painter.setPen(QPen(Qt::darkRed, 2));
            painter.drawLine(0, height() - 4, width(), height() - 4); // 橙色线条
        }
    }
}

void TabButtonItem::mousePressEvent(QMouseEvent* event) {
    if (event->button() == Qt::LeftButton) {
        emit clicked(); // 发出点击信号
    }
}
