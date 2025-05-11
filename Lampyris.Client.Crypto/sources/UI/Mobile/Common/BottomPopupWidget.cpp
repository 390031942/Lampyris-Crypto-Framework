#include "BottomPopupWidget.h"
#include <QVBoxLayout>
#include <QMouseEvent>
#include <QDebug>

BottomPopupWidget::BottomPopupWidget(QWidget* parent)
    : QWidget(parent), m_parent(parent), m_show(false) {
    // ���ô����ޱ߿��͸������
    setWindowFlags(Qt::FramelessWindowHint | Qt::Dialog);
    setAttribute(Qt::WA_TranslucentBackground);

    // ��ɫ����
    m_mask = new QWidget(this);
    m_mask->setStyleSheet("background-color: rgba(0, 0, 0, 0.5);");
    m_mask->installEventFilter(this); // ��������¼�

    // ��������
    m_popup = new QWidget(this);
    m_popup->setStyleSheet("background-color: white; border-radius: 10px;");

    // ����
    m_animation = new QPropertyAnimation(m_popup, "geometry");
    m_animation->setDuration(300);
    m_animation->setEasingCurve(QEasingCurve::OutCubic);

    // �������ֺ͵�������
    connect(m_animation, &QPropertyAnimation::finished, [this]() {
        if (!m_show) {
            m_mask->hide();
            m_popup->hide();
            m_popup = nullptr;
            this->hide();
        }
    });
}

void BottomPopupWidget::setContentWidget(QWidget* contentWidget) {
    // ���õ������ڵ�����
    QVBoxLayout* layout = new QVBoxLayout(m_popup);
    layout->setContentsMargins(0, 0, 0, 0);
    layout->addWidget(contentWidget);
}

void BottomPopupWidget::showPopup(QWidget* popup) {
    if (popup == nullptr) {
        return;
    }

    int x = m_parent->pos().x();
    int y = m_parent->pos().y();
    int w = m_parent->width();
    int h = m_parent->height();

    this->setGeometry(x, y, w, h);
    this->m_popup = popup;

    // ��ʾ���ֺ͵�������
    m_mask->setGeometry(0, 0, w, h);
    m_mask->show();

    int popupHeight = m_popup->height();
    m_popup->setGeometry(0, h, w, popupHeight); // ��ʼλ������Ļ�ײ�
    
    // �������ӵײ�����
    m_animation->setStartValue(QRect(0, h, w, popupHeight));
    m_animation->setEndValue(QRect(0, h - popupHeight, w, popupHeight));
    m_animation->start();
    m_show = true;
    m_popup->show();

    this->show();
}

void BottomPopupWidget::hidePopup() {
    if (m_popup == nullptr) {
        return;
    }

    // �������ӵ�ǰλ�û��ص��ײ�
    int popupHeight = m_popup->height();
    m_animation->setStartValue(m_popup->geometry());
    m_animation->setEndValue(QRect(0, height(), width(), popupHeight));
    m_animation->start();
    m_show = false;
}

void BottomPopupWidget::resizeEvent(QResizeEvent* event) {
    QWidget::resizeEvent(event);
}

void BottomPopupWidget::onMaskClicked() {
    hidePopup(); // ������ֹرմ���
}

bool BottomPopupWidget::eventFilter(QObject* watched, QEvent* event) {
    if (watched == m_mask && event->type() == QEvent::MouseButtonPress) {
        onMaskClicked();
        return true;
    }
    return QWidget::eventFilter(watched, event);
}
