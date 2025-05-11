// Project Include(s)
#include "MainToolBar.h"

MainToolBar::MainToolBar(QWidget* parent) : QWidget(parent) {
    setAttribute(Qt::WA_StyledBackground); // 启用样式表背景支持
    setStyleSheet("QWidget { background-color: #181A20; color: white; }"); // 工具栏背景颜色
    layout = new QVBoxLayout;
    layout->setContentsMargins(0, 0, 0, 0);
    layout->setSpacing(0);
    buttonGroup = new QButtonGroup(this);
    buttonGroup->setExclusive(true);
    this->setLayout(layout);
    this->addSpacer();
    this->setFixedWidth(64);
}

MainToolButton* MainToolBar::addButton(const QString& defaultIconPath, const QString& checkedIconPath, const QString& text) {
    MainToolButton* button = new MainToolButton(defaultIconPath, checkedIconPath, checkedIconPath, text);
    layout->insertWidget(layout->count() - 1,button);
    button->setMaximumHeight(75);
    buttonGroup->addButton(button);
    return button;
}

void MainToolBar::addSpacer() {
    // 添加一个空白区域（Spacer）
    QSpacerItem* spacer = new QSpacerItem(0, 50, QSizePolicy::Minimum, QSizePolicy::Expanding);
    layout->addSpacerItem(spacer);
}
