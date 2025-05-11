// Project Include(s)
#include "BetterLineEdit.h"
#include <QFocusEvent>
#include "SymbolSearchResultWidget.h"

BetterLineEdit::BetterLineEdit(QWidget* parent)
    :QLineEdit(parent), m_focus(false) {}

void BetterLineEdit::focusOutEvent(QFocusEvent* event) {
    if (m_historyWidget && m_historyWidget->isVisible() && m_historyWidget->underMouse()) {
        // �������� HistoryWidget �ϣ�������
        return;
    }

    if (m_focus && event->lostFocus()) {
        m_focus = false;    // ����ʧȥ
        emit signalOutFocus();
        clearFocus(); // ʧȥ����
    }
    QLineEdit::focusOutEvent(event);
}

void BetterLineEdit::focusInEvent(QFocusEvent* event) {
    emit signalInFocus();
    m_focus = true; // ������
    QLineEdit::focusInEvent(event);
}

void BetterLineEdit::keyPressEvent(QKeyEvent* event) {
    if (event->key() == Qt::Key_Return || event->key() == Qt::Key_Enter) {
        // ���� Enter ��ʱ�������༭����߼�
        // emit editingFinished(); // �����Զ����ź�
        clearFocus(); // ʧȥ����
    }
    else {
        // ����������������
        QLineEdit::keyPressEvent(event);
    }
}
