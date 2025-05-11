// Project Include(s)
#include "MainToolButton.h"

MainToolButton::MainToolButton(const QString& defaultIconPath, 
                               const QString& hoverIconPath, 
                               const QString& checkedIconPath, 
                               const QString& text,
                               QWidget* parent) : QToolButton(parent),
    m_defaultIcon(defaultIconPath),
    m_hoverIcon(hoverIconPath),
    m_checkedIcon(checkedIconPath) {

    setIconSize(QSize(32, 32));
    // ���ð�ť��ͼ�������
    setIcon(m_defaultIcon);
    setText(text);
    setToolButtonStyle(Qt::ToolButtonTextUnderIcon); // ͼ���������Ϸ�
    setCheckable(true); // ��ť��ѡ��

    // ���ð�ť�Ĵ�С���ԣ�ʹ���沼������
    setSizePolicy(QSizePolicy::Expanding, QSizePolicy::Expanding);

    // �źŲ����ӣ����ڶ�̬����ͼ��
    connect(this, &QToolButton::toggled, this, &MainToolButton::updateIcon);

    this->setStyleSheet(R"(
            QToolButton {
                background-color: #181A20; /* Ĭ�ϱ�����ɫ */
                color: white; /* Ĭ��������ɫ */
                padding: 5px;
            }
            QToolButton:hover {
                background-color: #181A20; /* ѡ��ʱ������ɫ */
                color: #F0B90B; /* ѡ��ʱ������ɫ */
            }
            QToolButton:checked {
                background-color: #181A20; /* ѡ��ʱ������ɫ */
                color: #F0B90B; /* ѡ��ʱ������ɫ */
            }
        )");
}

void MainToolButton::enterEvent(QEnterEvent* event) {
    if (!isChecked()) {
        setIcon(m_hoverIcon);
    }
    QToolButton::enterEvent(event);
}

void MainToolButton::leaveEvent(QEvent* event) {
    if (!isChecked()) {
        setIcon(m_defaultIcon);
    }
    QToolButton::leaveEvent(event);
}

void MainToolButton::updateIcon(bool checked) {
    if (checked) {
        setIcon(m_checkedIcon);
    }
    else {
        setIcon(m_defaultIcon);
    }
}
