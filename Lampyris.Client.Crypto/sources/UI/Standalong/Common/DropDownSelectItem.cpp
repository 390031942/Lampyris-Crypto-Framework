// Project Include(s)
#include "DropDownSelectItem.h"

void DropDownSelectItem::paintEvent(QPaintEvent* event) {
    QPainter painter(this);

    // 绘制鼠标悬停背景
    QStyleOption opt;
    opt.initFrom(this);
    style()->drawPrimitive(QStyle::PE_Widget, &opt, &painter, this);

    if (underMouse()) {
        painter.fillRect(rect(), QColor(74, 74, 74, 255));
    }
    else {
        painter.fillRect(rect(), QColor(0, 0, 0, 255));
    }
    // 绘制文本
    painter.setPen(Qt::white);
    painter.drawText(10, height() / 2 + 5, m_text);

    // 绘制选中状态的钩图标
    if (m_selected) {
        QPixmap checkIcon(":/res/icons/dropdown_checked.png"); 
        painter.drawPixmap(width() - 30, (height() - 20) / 2, 20, 20, checkIcon);
    }
}

void DropDownSelectItem::mousePressEvent(QMouseEvent* event) {
    emit clicked(); // 发射点击信号
}

void DropDownSelectItem::enterEvent(QEnterEvent* event) {
    update();
    QWidget::enterEvent(event);
}

void DropDownSelectItem::leaveEvent(QEvent* event) {
    update();
    QWidget::leaveEvent(event);
}