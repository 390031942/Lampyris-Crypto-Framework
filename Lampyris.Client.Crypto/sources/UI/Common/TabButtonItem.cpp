// Project Include(s)
#include "TabButtonItem.h"

TabButtonItem::TabButtonItem(const QString& text, QWidget* parent)
    : QWidget(parent), m_text(text), m_selected(false), m_mode(TabButtonDisplayMode::BACKGROUND) {}

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
    if (m_selected) {
        if (m_mode == TabButtonDisplayMode::BACKGROUND) {
            painter.setBrush(QBrush(QColor(252,213,53)));
            painter.setPen(Qt::NoPen);
            painter.drawRoundedRect(rect(), 6, 6); // 灰色圆角矩形
        }
        else if (m_mode == TabButtonDisplayMode::UNDERLINE) {
            painter.setPen(QPen(QColor(252, 213, 53), 2));
            painter.drawLine(0, height(), width(), height());
        }
    }

    // 绘制文字
    painter.setPen((m_selected ? QColor(0,0,0) : QColor(252,255,255))); // 选中为白色，未选中为灰色
    painter.setFont(QFont("Arial", 12));
    painter.drawText(rect(), Qt::AlignCenter, m_text);
}

void TabButtonItem::mousePressEvent(QMouseEvent* event) {
    if (event->button() == Qt::LeftButton) {
        emit clicked(); // 发出点击信号
    }
}
