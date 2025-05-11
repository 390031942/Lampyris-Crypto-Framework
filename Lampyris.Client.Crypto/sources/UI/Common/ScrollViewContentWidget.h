#pragma once
// QT Include(s)
#include <QWidget>
#include <QVBoxLayout>
#include <QScrollBar>

// STL Include(s)
#include <vector>
#include <memory>

// Project Include(s)
#include "AbstractScrollItem.h"

class ScrollViewContentWidget : public QWidget {
    Q_OBJECT

public:
    explicit ScrollViewContentWidget(QWidget* parent = nullptr)
        : QWidget(parent), m_itemHeight(50), m_visibleItemCount(0) {
        m_layout = new QVBoxLayout(this);
        m_layout->setSpacing(0);
        m_layout->setContentsMargins(0, 0, 0, 0);
        setLayout(m_layout);
    }

    void setItemHeight(int height) {
        m_itemHeight = height;
    }

    int getItemHeight() {
        return m_itemHeight;
    }

    void setItemWidgetFactory(std::function<AbstractScrollItem* (QWidget*)> factory) {
        m_itemWidgetFactory = factory;
    }

    void updateView(const std::vector<std::shared_ptr<AbstractDataObject>>& dataList) {
        m_dataList = dataList;

        // 根据数据数量计算内容区域的高度
        m_contentHeight = m_dataList.size() * m_itemHeight;
        // setMinimumHeight(m_contentHeight);

        updateVisibleItems();
    }

    void onScroll(int scrollValue) {
        updateVisibleItems();
    }

    void resizeEvent(QResizeEvent* event) override {
        QWidget::resizeEvent(event);
        updateVisibleItems();
    }

    void setScrollValue(int value) {
        m_scrollValue = value;
        updateVisibleItems();
    }

    int getContentHeight() const {
        return m_contentHeight;
    }
private:
    QVBoxLayout* m_layout;
    int m_itemHeight;
    int m_contentHeight; // 内容区域的总高度
    int m_visibleItemCount;
    int m_scrollValue; // 当前滚动条的值
    std::vector<std::shared_ptr<AbstractDataObject>> m_dataList;
    std::vector<AbstractScrollItem*> m_itemPool;
    std::function<AbstractScrollItem* (QWidget*)> m_itemWidgetFactory;


    void updateVisibleItems() {
        if (!m_itemWidgetFactory) return;

        int viewportHeight = parentWidget()->height();
        m_visibleItemCount = viewportHeight / m_itemHeight + 1;

        // 确保池中有足够的 ItemWidget
        while (m_itemPool.size() < m_visibleItemCount) {
            auto* item = m_itemWidgetFactory(this);
            m_itemPool.push_back(item);
            m_layout->addWidget(item);
        }

        // 隐藏多余的 ItemWidget
        for (size_t i = m_visibleItemCount; i < m_itemPool.size(); ++i) {
            m_itemPool[i]->hide();
        }

        qDebug() << "scrollValue = " << m_scrollValue;

        // 更新可见区域的 ItemWidget 数据
        int startIndex = (int)std::ceil((float)m_scrollValue / (float)m_itemHeight); // 使用滚动条的值计算起始索引
        qDebug() << "startIndex = " << startIndex;

        for (int i = 0; i < m_visibleItemCount && startIndex + i < m_dataList.size(); ++i) {
            m_itemPool[i]->setData(*m_dataList[startIndex + i]);
            m_itemPool[i]->setIndex(startIndex + i);
            m_itemPool[i]->show();
        }
    }
};
