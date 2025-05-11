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
        // 创建内容区域
        m_content = new ScrollViewContentWidget(this);

        // 创建垂直滚动条
        m_scrollBar = new QScrollBar(Qt::Vertical, this);

        // 创建定时器用于节流刷新
        m_updateTimer = new QTimer(this);
        m_updateTimer->setSingleShot(true); // 定时器只触发一次

        // 设置布局
        QHBoxLayout* layout = new QHBoxLayout(this);
        layout->setSpacing(0);
        layout->setContentsMargins(0, 0, 0, 0);
        layout->addWidget(m_content);
        layout->addWidget(m_scrollBar, 0, Qt::AlignRight);
        setLayout(layout);

        // 连接滚动条的值变化信号
        connect(m_scrollBar, &QScrollBar::valueChanged, this, [this](int value) {
            // 启动定时器，延迟刷新
            m_pendingScrollValue = value;
            if (!m_updateTimer->isActive()) {
                m_updateTimer->start(50); // 延迟 50 毫秒刷新
            }
        });

        // 定时器触发时更新内容
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

        // 根据内容区域的高度设置滚动条的范围
        int contentHeight = m_content->getContentHeight();
        int viewportHeight = height();
        m_scrollBar->setRange(0, std::max(0, contentHeight - viewportHeight - m_content->getItemHeight()));
        m_scrollBar->setPageStep(viewportHeight);
    }

protected:
    void wheelEvent(QWheelEvent* event) override {
        // 使用鼠标滚轮实时调整滚动条的值
        int delta = event->angleDelta().y();
        m_scrollBar->blockSignals(true);

        if (delta > 0) {
            delta = std::max(delta, m_content->getItemHeight());
        }
        else if(delta < 0) {
            delta = std::min(delta, -m_content->getItemHeight());
        }
        m_scrollBar->setValue(m_scrollBar->value() - delta); // 每次滚动调整滚动条的值
        m_scrollBar->blockSignals(false);

        // 实时更新内容区域
        m_content->setScrollValue(m_scrollBar->value());
    }

    void resizeEvent(QResizeEvent* event) override {
        // 根据内容区域的高度设置滚动条的范围
        int contentHeight = m_content->getContentHeight();
        int viewportHeight = height();
        m_scrollBar->setRange(0, std::max(0, contentHeight - viewportHeight - m_content->getItemHeight()));
        m_scrollBar->setPageStep(viewportHeight);
        QWidget::resizeEvent(event);
    }
private:
    ScrollViewContentWidget* m_content;
    QScrollBar* m_scrollBar;
    QTimer* m_updateTimer; // 定时器用于节流刷新
    int m_pendingScrollValue; // 保存滚动条的最新值
};
