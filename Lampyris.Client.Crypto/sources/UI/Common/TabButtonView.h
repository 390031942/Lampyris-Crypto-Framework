#pragma once
// QT Include(s)
#include <QWidget>
#include <QScrollArea>
#include <QHBoxLayout>
#include <QSpacerItem>
#include <QMouseEvent>

// Project Include(s)
#include "TabButtonItem.h"

class TabButtonView : public QScrollArea {
    Q_OBJECT
public:
    explicit              TabButtonView(QWidget* parent = nullptr);
    void                  addTab(const QString& text);
    void                  setSelectedTab(TabButtonItem* tabItem);
    void                  setItemDisplayMode(TabButtonDisplayMode mode);
    void                  setDragEnabled(bool enabled);
protected:                
    void                  mousePressEvent(QMouseEvent* event) override;
    void                  mouseMoveEvent(QMouseEvent* event) override;
private:
    QWidget*              m_container;
    QHBoxLayout*          m_layout;
    QList<TabButtonItem*> m_tabs;
    bool                  m_dragEnabled;
    QPoint                m_lastMousePos;
};
