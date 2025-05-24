// Project Include(s)
#include "TabButtonView.h"

// QT Include(s)
#include <QScrollBar>

TabButtonView::TabButtonView(QWidget* parent)
    : QScrollArea(parent), m_dragEnabled(false), m_lastMousePos(0, 0) {
    setHorizontalScrollBarPolicy(Qt::ScrollBarAlwaysOff);
    setVerticalScrollBarPolicy(Qt::ScrollBarAlwaysOff);

    m_container = new QWidget(this);
    m_layout = new QHBoxLayout(m_container);
    m_layout->setSpacing(10);
    m_layout->setContentsMargins(0, 0, 0, 0);

    m_container->setLayout(m_layout);
    setWidget(m_container);
    setWidgetResizable(true);
}

void TabButtonView::addTab(const QString& text) {
    auto* tabItem = new TabButtonItem(text, this);
    m_layout->addWidget(tabItem);
    m_tabs.append(tabItem);

    // 添加空白填充
    QSpacerItem* spacer = new QSpacerItem(20, 20, QSizePolicy::Expanding, QSizePolicy::Minimum);
    m_layout->addSpacerItem(spacer);

    connect(tabItem, &TabButtonItem::clicked, this, [this, tabItem]() {
        setSelectedTab(tabItem);
    });
}

void TabButtonView::setSelectedTab(TabButtonItem* tabItem) {
    for (auto* tab : m_tabs) {
        tab->setSelected(tab == tabItem);
    }
}

void TabButtonView::setItemDisplayMode(TabButtonDisplayMode mode) {
    for (auto* tab : m_tabs) {
        tab->setDisplayMode(mode);
    }
}

void TabButtonView::setDragEnabled(bool enabled) {
    m_dragEnabled = enabled;
}

void TabButtonView::mousePressEvent(QMouseEvent* event) {
    if (m_dragEnabled) {
        m_lastMousePos = event->pos();
    }
}

void TabButtonView::mouseMoveEvent(QMouseEvent* event) {
    if (m_dragEnabled) {
        int dx = event->pos().x() - m_lastMousePos.x();
        horizontalScrollBar()->setValue(horizontalScrollBar()->value() - dx);
        m_lastMousePos = event->pos();
    }
}
