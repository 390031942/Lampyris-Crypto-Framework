// Project Include(s)
#include "DropDownWidget.h"

DropDownWidget::DropDownWidget(QWidget* parent)
    : QWidget(parent) {
    setWindowFlags(Qt::Popup | Qt::FramelessWindowHint); // 设置为弹出窗口
    setAttribute(Qt::WA_TranslucentBackground); // 透明背景
    setFixedWidth(200); // 设置窗口宽度

    m_layout = new QVBoxLayout(this);
    m_layout->setContentsMargins(0, 0, 0, 0);
    m_layout->setSpacing(0);

    setStyleSheet(
        "QWidget{"
        "    background-color: black;"   // 背景颜色
        "    border-radius: 10px;"       // 圆角半径
        "    border: 1px solid gray;"    // 边框颜色
        "}"
    );

}

void DropDownWidget::setOptions(QStringList items) {
    int index = 0;
    for (const QString& itemText : items) {
        DropDownSelectItem* item = new DropDownSelectItem(itemText, index, this);
        m_layout->addWidget(item);

        connect(item, &DropDownSelectItem::clicked, this, [this, item, index]() {
            for (auto child : findChildren<DropDownSelectItem*>()) {
                child->setSelected(false); // 取消其他选项的选中状态
            }
            item->setSelected(true); // 设置当前选项为选中状态
            emit itemSelected(item->isSelected());
            emit itemSelected(index);
        });

        index++;
    }
}

void DropDownWidget::paintEvent(QPaintEvent* event) {
    QWidget::paintEvent(event);
}
