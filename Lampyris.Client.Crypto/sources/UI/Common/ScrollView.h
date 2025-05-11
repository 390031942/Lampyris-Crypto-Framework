#pragma once
#include <QScrollArea>
#include "ScrollViewContentWidget.h"

#include <QWidget>
#include <QScrollBar>
#include <QVBoxLayout>
#include <QWheelEvent>
#include <vector>
#include <memory>
#include <QTimer>

class ScrollView : public QWidget {
    Q_OBJECT

public:
    explicit ScrollView(QWidget* parent = nullptr) : QWidget(parent) {
        // ������������
        m_content = new ScrollViewContentWidget(this);

        // ������ֱ������
        m_scrollBar = new QScrollBar(Qt::Vertical, this);

        // ������ʱ�����ڽ���ˢ��
        m_updateTimer = new QTimer(this);
        m_updateTimer->setSingleShot(true); // ��ʱ��ֻ����һ��

        // ���ò���
        QHBoxLayout* layout = new QHBoxLayout(this);
        layout->setSpacing(0);
        layout->setContentsMargins(0, 0, 0, 0);
        layout->addWidget(m_content);
        layout->addWidget(m_scrollBar, 0, Qt::AlignRight);
        setLayout(layout);

        // ���ӹ�������ֵ�仯�ź�
        connect(m_scrollBar, &QScrollBar::valueChanged, this, [this](int value) {
            // ������ʱ�����ӳ�ˢ��
            m_pendingScrollValue = value;
            if (!m_updateTimer->isActive()) {
                m_updateTimer->start(50); // �ӳ� 50 ����ˢ��
            }
        });

        // ��ʱ������ʱ��������
        connect(m_updateTimer, &QTimer::timeout, this, [this]() {
            m_content->setScrollValue(m_pendingScrollValue);
            });
    }

    void setItemHeight(int height) {
        m_content->setItemHeight(height);
    }


    template <ScrollItemType T>
    void setItemWidgetType() {
        m_content->setItemWidgetFactory([](QWidget* parent) -> AbstractScrollItem* {
            return new T(parent);
        });
    }


    void updateView(const std::vector<std::shared_ptr<AbstractDataObject>>& dataList) {
        m_content->updateView(dataList);

        // ������������ĸ߶����ù������ķ�Χ
        int contentHeight = m_content->getContentHeight();
        int viewportHeight = height();
        m_scrollBar->setRange(0, std::max(0, contentHeight - viewportHeight - m_content->getItemHeight()));
        m_scrollBar->setPageStep(viewportHeight);
    }

protected:
    void wheelEvent(QWheelEvent* event) override {
        // ʹ��������ʵʱ������������ֵ
        int delta = event->angleDelta().y();
        m_scrollBar->blockSignals(true);

        if (delta > 0) {
            delta = std::max(delta, m_content->getItemHeight());
        }
        else if(delta < 0) {
            delta = std::min(delta, -m_content->getItemHeight());
        }
        m_scrollBar->setValue(m_scrollBar->value() - delta); // ÿ�ι���������������ֵ
        m_scrollBar->blockSignals(false);

        // ʵʱ������������
        m_content->setScrollValue(m_scrollBar->value());
    }

    void resizeEvent(QResizeEvent* event) override {
        // ������������ĸ߶����ù������ķ�Χ
        int contentHeight = m_content->getContentHeight();
        int viewportHeight = height();
        m_scrollBar->setRange(0, std::max(0, contentHeight - viewportHeight - m_content->getItemHeight()));
        m_scrollBar->setPageStep(viewportHeight);
        QWidget::resizeEvent(event);
    }
private:
    ScrollViewContentWidget* m_content;
    QScrollBar* m_scrollBar;
    QTimer* m_updateTimer; // ��ʱ�����ڽ���ˢ��
    int m_pendingScrollValue; // ���������������ֵ
};
