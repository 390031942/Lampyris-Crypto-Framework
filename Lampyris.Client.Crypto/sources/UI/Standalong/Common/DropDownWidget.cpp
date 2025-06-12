// Project Include(s)
#include "DropDownWidget.h"

DropDownWidget::DropDownWidget(QWidget* parent)
    : QWidget(parent) {
    setWindowFlags(Qt::Popup | Qt::FramelessWindowHint); // ����Ϊ��������
    setAttribute(Qt::WA_TranslucentBackground); // ͸������
    setFixedWidth(200); // ���ô��ڿ��

    m_layout = new QVBoxLayout(this);
    m_layout->setContentsMargins(0, 0, 0, 0);
    m_layout->setSpacing(0);

    setStyleSheet(
        "QWidget{"
        "    background-color: black;"   // ������ɫ
        "    border-radius: 10px;"       // Բ�ǰ뾶
        "    border: 1px solid gray;"    // �߿���ɫ
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
                child->setSelected(false); // ȡ������ѡ���ѡ��״̬
            }
            item->setSelected(true); // ���õ�ǰѡ��Ϊѡ��״̬
            emit itemSelected(item->isSelected());
            emit itemSelected(index);
        });

        index++;
    }
}

void DropDownWidget::paintEvent(QPaintEvent* event) {
    QWidget::paintEvent(event);
}
