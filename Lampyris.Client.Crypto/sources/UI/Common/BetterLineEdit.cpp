// Project Include(s)
#include "BetterLineEdit.h"
#include <QFocusEvent>
#include "SymbolSearchResultWidget.h"

BetterLineEdit::BetterLineEdit(QWidget* parent)
    : QLineEdit(parent), m_focus(false), m_historyWidget(nullptr), m_dropDownWidget(new DropDownWidget(this)) {

    // ��ʼ���Ҳఴť
    m_optionsButton = new QToolButton(this);
    m_optionsButton->setStyleSheet("QToolButton { background-color:transparent; border: none; }");
    m_optionsButton->setCursor(Qt::PointingHandCursor);
    m_optionsButton->setIcon(QIcon(":/res/icons/down_triangle.png")); // Ĭ����ʾ������ͼ��
    m_optionsButton->setIconSize(QSize(16, 16));
    m_optionsButton->setFixedSize(20, 20);
    m_optionsButton->hide(); // Ĭ������

    connect(m_optionsButton, &QToolButton::clicked, [=]() {
        clearFocus();

        m_optionsButton->setIcon(QIcon(":/res/icons/up_triangle.png")); // Ĭ����ʾ������ͼ��

        // ��ȡ��ť��ȫ��λ��
        QPoint globalPos = m_optionsButton->mapToGlobal(QPoint(0, m_optionsButton->height()));

        // �����������ڵ�λ��
        m_dropDownWidget->move(globalPos + QPoint(-m_dropDownWidget->width() + 20, 10));

        // ��ʾ��������
        m_dropDownWidget->show();
    });

    // ��װ�¼�������
    m_dropDownWidget->installEventFilter(this);

    // ���ð�ť�� QLineEdit ���Ҳ�
    QHBoxLayout* layout = new QHBoxLayout(this);
    layout->setContentsMargins(0, 0, 0, 0);
    layout->addStretch();
    layout->setSpacing(0);
    setLayout(layout);
    setStyleSheet(
        "QLineEdit {"
        "    background-color: black;"
        "    color: white;"
        "    border: 1px solid white;"  // �߿���ɫ�Ϳ��
        "    border-radius: 7px;"      // Բ�ǰ뾶
        "    padding: 5px;"            // �ڱ߾࣬�����ı�����
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
    line->hide(); // Ĭ������
    m_line = line;

    layout->addWidget(m_optionalText);
    layout->addWidget(line);
    layout->addWidget(m_optionsButton);
}

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

bool BetterLineEdit::eventFilter(QObject* obj, QEvent* event) {
    if (obj == m_dropDownWidget) {
        if (event->type() == QEvent::Close) {
            m_optionsButton->setIcon(QIcon(":/res/icons/down_triangle.png")); // Ĭ����ʾ������ͼ��
            return false;
        }
        else if (event->type() == QEvent::Hide) {
            m_optionsButton->setIcon(QIcon(":/res/icons/down_triangle.png")); // Ĭ����ʾ������ͼ��
            return false;
        }
    }
// ��
    return QWidget::eventFilter(obj, event);
}

void BetterLineEdit::shake() {
    // ����һ�����Զ���������Ŀ���� QLineEdit ��λ��
    QPropertyAnimation* animation = new QPropertyAnimation(this, "pos");
    animation->setDuration(400); // ��������ʱ��
    animation->setKeyValueAt(0, pos()); // ��ʼλ��
    animation->setKeyValueAt(0.25, pos() + QPoint(-10, 0)); // ����
    animation->setKeyValueAt(0.5, pos() + QPoint(10, 0)); // ����
    animation->setKeyValueAt(0.75, pos() + QPoint(-10, 0)); // ����
    animation->setKeyValueAt(1, pos()); // �ص�ԭʼλ��
    animation->start(QAbstractAnimation::DeleteWhenStopped); // �����������Զ�ɾ��
}

void BetterLineEdit::flashRed() {
    // ʹ�� QPropertyAnimation ʵ����ɫ����
    QPropertyAnimation* animation = new QPropertyAnimation(this, "backgroundColor");
    animation->setDuration(800); // �������ʱ��
    animation->setKeyValueAt(0, backgroundColor()); // ��ʼ��ɫ
    animation->setKeyValueAt(0.25, QColor(255, 200, 200)); // ���䵽����ɫ
    animation->setKeyValueAt(0.5, backgroundColor()); // �ָ�ԭʼ��ɫ
    animation->setKeyValueAt(0.75, QColor(255, 200, 200)); // ���䵽����ɫ
    animation->setKeyValueAt(1, backgroundColor()); // �ָ�ԭʼ��ɫ
    animation->start(QAbstractAnimation::DeleteWhenStopped); // �����������Զ�ɾ��
}
