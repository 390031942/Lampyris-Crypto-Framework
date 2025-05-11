#include "BottomPopupWidget.h"
#include <QVBoxLayout>
#include <QMouseEvent>
#include <QDebug>

BottomPopupWidget::BottomPopupWidget(QWidget* parent)
    : QWidget(parent), m_parent(parent), m_show(false) {
    // 设置窗口无边框和透明背景
    setWindowFlags(Qt::FramelessWindowHint | Qt::Dialog);
    setAttribute(Qt::WA_TranslucentBackground);

    // 灰色遮罩
    m_mask = new QWidget(this);
    m_mask->setStyleSheet("background-color: rgba(0, 0, 0, 0.5);");
    m_mask->installEventFilter(this); // 监听点击事件

    // 弹出窗口
    m_popup = new QWidget(this);
    m_popup->setStyleSheet("background-color: white; border-radius: 10px;");

    // 动画
    m_animation = new QPropertyAnimation(m_popup, "geometry");
    m_animation->setDuration(300);
    m_animation->setEasingCurve(QEasingCurve::OutCubic);

    // 隐藏遮罩和弹出窗口
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
    // 设置弹出窗口的内容
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

    // 显示遮罩和弹出窗口
    m_mask->setGeometry(0, 0, w, h);
    m_mask->show();

    int popupHeight = m_popup->height();
    m_popup->setGeometry(0, h, w, popupHeight); // 初始位置在屏幕底部
    
    // 动画：从底部滑出
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

    // 动画：从当前位置滑回到底部
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
    hidePopup(); // 点击遮罩关闭窗口
}

bool BottomPopupWidget::eventFilter(QObject* watched, QEvent* event) {
    if (watched == m_mask && event->type() == QEvent::MouseButtonPress) {
        onMaskClicked();
        return true;
    }
    return QWidget::eventFilter(watched, event);
}
