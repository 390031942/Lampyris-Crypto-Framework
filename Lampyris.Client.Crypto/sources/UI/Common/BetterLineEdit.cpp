// Project Include(s)
#include "BetterLineEdit.h"
#include <QFocusEvent>
#include "SymbolSearchResultWidget.h"

BetterLineEdit::BetterLineEdit(QWidget* parent)
    : QLineEdit(parent), m_focus(false), m_historyWidget(nullptr), m_dropDownWidget(new DropDownWidget(this)) {

    // 初始化右侧按钮
    m_optionsButton = new QToolButton(this);
    m_optionsButton->setStyleSheet("QToolButton { background-color:transparent; border: none; }");
    m_optionsButton->setCursor(Qt::PointingHandCursor);
    m_optionsButton->setIcon(QIcon(":/res/icons/down_triangle.png")); // 默认显示下三角图标
    m_optionsButton->setIconSize(QSize(16, 16));
    m_optionsButton->setFixedSize(20, 20);
    m_optionsButton->hide(); // 默认隐藏

    connect(m_optionsButton, &QToolButton::clicked, [=]() {
        clearFocus();

        m_optionsButton->setIcon(QIcon(":/res/icons/up_triangle.png")); // 默认显示下三角图标

        // 获取按钮的全局位置
        QPoint globalPos = m_optionsButton->mapToGlobal(QPoint(0, m_optionsButton->height()));

        // 设置下拉窗口的位置
        m_dropDownWidget->move(globalPos + QPoint(-m_dropDownWidget->width() + 20, 10));

        // 显示下拉窗口
        m_dropDownWidget->show();
    });

    // 安装事件过滤器
    m_dropDownWidget->installEventFilter(this);

    // 设置按钮在 QLineEdit 的右侧
    QHBoxLayout* layout = new QHBoxLayout(this);
    layout->setContentsMargins(0, 0, 0, 0);
    layout->addStretch();
    layout->setSpacing(0);
    setLayout(layout);
    setStyleSheet(
        "QLineEdit {"
        "    background-color: black;"
        "    color: white;"
        "    border: 1px solid white;"  // 边框颜色和宽度
        "    border-radius: 7px;"      // 圆角半径
        "    padding: 5px;"            // 内边距，避免文本贴边
        "}"
        "QLineEdit:focus {"
        "    border: 1px solid rgb(240,185,11);"
        "}"
        "QLineEdit:hover {"
        "    border: 1px solid rgb(240,185,11);"
        "}"
    );

    m_optionalText = new QLabel(this);
    m_optionalText->setAlignment(Qt::AlignRight|Qt::AlignVCenter);
    m_optionalText->setStyleSheet("color: white;");

    QFrame* line = new QFrame(this);
    line->setSizePolicy(QSizePolicy::Fixed, QSizePolicy::Expanding);
    line->setFrameShape(QFrame::VLine);
    line->setFrameShadow(QFrame::Plain);
    line->setStyleSheet("color: white;");
    line->setFixedWidth(2);
    line->hide(); // 默认隐藏
    m_line = line;

    layout->addWidget(m_optionalText);
    layout->addWidget(line);
    layout->addWidget(m_optionsButton);
}

void BetterLineEdit::focusOutEvent(QFocusEvent* event) {
    if (m_historyWidget && m_historyWidget->isVisible() && m_historyWidget->underMouse()) {
        // 如果鼠标在 HistoryWidget 上，则不隐藏
        return;
    }

    if (m_focus && event->lostFocus()) {
        m_focus = false;    // 焦点失去
        emit signalOutFocus();
        clearFocus(); // 失去焦点
    }
    QLineEdit::focusOutEvent(event);
}

void BetterLineEdit::focusInEvent(QFocusEvent* event) {
    emit signalInFocus();
    m_focus = true; // 焦点获得
    QLineEdit::focusInEvent(event);
}

void BetterLineEdit::keyPressEvent(QKeyEvent* event) {
    if (event->key() == Qt::Key_Return || event->key() == Qt::Key_Enter) {
        // 按下 Enter 键时，触发编辑完成逻辑
        // emit editingFinished(); // 发射自定义信号
        clearFocus(); // 失去焦点
    }
    else {
        // 其他按键正常处理
        QLineEdit::keyPressEvent(event);
    }
}

bool BetterLineEdit::eventFilter(QObject* obj, QEvent* event) {
    if (obj == m_dropDownWidget) {
        if (event->type() == QEvent::Close) {
            m_optionsButton->setIcon(QIcon(":/res/icons/down_triangle.png")); // 默认显示下三角图标
            return false;
        }
        else if (event->type() == QEvent::Hide) {
            m_optionsButton->setIcon(QIcon(":/res/icons/down_triangle.png")); // 默认显示下三角图标
            return false;
        }
    }
// 调
    return QWidget::eventFilter(obj, event);
}

void BetterLineEdit::shake() {
    // 创建一个属性动画，动画目标是 QLineEdit 的位置
    QPropertyAnimation* animation = new QPropertyAnimation(this, "pos");
    animation->setDuration(400); // 动画持续时间
    animation->setKeyValueAt(0, pos()); // 起始位置
    animation->setKeyValueAt(0.25, pos() + QPoint(-10, 0)); // 左移
    animation->setKeyValueAt(0.5, pos() + QPoint(10, 0)); // 右移
    animation->setKeyValueAt(0.75, pos() + QPoint(-10, 0)); // 左移
    animation->setKeyValueAt(1, pos()); // 回到原始位置
    animation->start(QAbstractAnimation::DeleteWhenStopped); // 动画结束后自动删除
}

void BetterLineEdit::flashRed() {
    // 使用 QPropertyAnimation 实现颜色渐变
    QPropertyAnimation* animation = new QPropertyAnimation(this, "backgroundColor");
    animation->setDuration(800); // 渐变持续时间
    animation->setKeyValueAt(0, backgroundColor()); // 起始颜色
    animation->setKeyValueAt(0.25, QColor(255, 200, 200)); // 渐变到淡红色
    animation->setKeyValueAt(0.5, backgroundColor()); // 恢复原始颜色
    animation->setKeyValueAt(0.75, QColor(255, 200, 200)); // 渐变到淡红色
    animation->setKeyValueAt(1, backgroundColor()); // 恢复原始颜色
    animation->start(QAbstractAnimation::DeleteWhenStopped); // 动画结束后自动删除
}
