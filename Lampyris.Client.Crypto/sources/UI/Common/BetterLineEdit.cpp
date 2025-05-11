// Project Include(s)
#include "BetterLineEdit.h"
#include <QFocusEvent>
#include "SymbolSearchResultWidget.h"

BetterLineEdit::BetterLineEdit(QWidget* parent)
    :QLineEdit(parent), m_focus(false) {}

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
