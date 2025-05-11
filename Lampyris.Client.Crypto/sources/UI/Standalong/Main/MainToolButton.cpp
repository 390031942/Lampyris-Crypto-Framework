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
    // 设置按钮的图标和文字
    setIcon(m_defaultIcon);
    setText(text);
    setToolButtonStyle(Qt::ToolButtonTextUnderIcon); // 图标在文字上方
    setCheckable(true); // 按钮可选中

    // 设置按钮的大小策略，使其随布局拉伸
    setSizePolicy(QSizePolicy::Expanding, QSizePolicy::Expanding);

    // 信号槽连接，用于动态更新图标
    connect(this, &QToolButton::toggled, this, &MainToolButton::updateIcon);

    this->setStyleSheet(R"(
            QToolButton {
                background-color: #181A20; /* 默认背景颜色 */
                color: white; /* 默认文字颜色 */
                padding: 5px;
            }
            QToolButton:hover {
                background-color: #181A20; /* 选中时背景颜色 */
                color: #F0B90B; /* 选中时文字颜色 */
            }
            QToolButton:checked {
                background-color: #181A20; /* 选中时背景颜色 */
                color: #F0B90B; /* 选中时文字颜色 */
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
